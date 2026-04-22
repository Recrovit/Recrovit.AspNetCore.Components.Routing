using System.Reflection;
using Microsoft.AspNetCore.Components;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Resolution;

public sealed class RecrovitRouteModeResolver
{
    private readonly IReadOnlyList<RouteEntry> _entries;
    private readonly IRecrovitPageRouteDefinitionResolver _pageDefinitionResolver;

    public RecrovitRouteModeResolver(
        IRecrovitPageRouteDefinitionResolver pageDefinitionResolver,
        IEnumerable<Assembly> assemblies)
    {
        _pageDefinitionResolver = pageDefinitionResolver;
        _entries = assemblies
            .Distinct()
            .SelectMany(GetRouteEntries)
            .OrderByDescending(static entry => entry.SegmentCount)
            .ThenByDescending(static entry => entry.LiteralSegmentCount)
            .ToArray();
    }

    public RecrovitPageRouteDefinition Resolve(string? requestPath)
    {
        var normalizedPath = NormalizePath(requestPath);

        foreach (var entry in _entries)
        {
            if (entry.IsMatch(normalizedPath))
            {
                return entry.Definition;
            }
        }

        return _pageDefinitionResolver.GetFallbackDefinition();
    }

    private IEnumerable<RouteEntry> GetRouteEntries(Assembly assembly)
    {
        foreach (var pageType in assembly.ExportedTypes)
        {
            var routeAttributes = pageType.GetCustomAttributes<RouteAttribute>(inherit: false).ToArray();
            if (routeAttributes.Length == 0)
            {
                continue;
            }

            var definition = _pageDefinitionResolver.GetDefinition(pageType);

            foreach (var routeAttribute in routeAttributes)
            {
                var template = NormalizeTemplate(routeAttribute.Template);

                yield return new RouteEntry(
                    template,
                    definition,
                    CountSegments(template),
                    CountLiteralSegments(template));
            }
        }
    }

    private static string NormalizePath(string? requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath) || requestPath == "/")
        {
            return "/";
        }

        return "/" + requestPath.Trim().Trim('/').ToLowerInvariant();
    }

    private static string NormalizeTemplate(string? template)
        => string.IsNullOrWhiteSpace(template) || template == "/"
            ? string.Empty
            : template.Trim().Trim('/').ToLowerInvariant();

    private static int CountSegments(string template)
        => string.IsNullOrEmpty(template)
            ? 0
            : template.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

    private static int CountLiteralSegments(string template)
        => string.IsNullOrEmpty(template)
            ? 0
            : template.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Count(static segment => !RouteEntry.IsParameterSegment(segment));

    private sealed record RouteEntry(
        string Template,
        RecrovitPageRouteDefinition Definition,
        int SegmentCount,
        int LiteralSegmentCount)
    {
        private string[] TemplateSegments { get; } = GetSegments(Template);

        public bool IsMatch(string requestPath)
        {
            var requestSegments = GetSegments(requestPath);

            if (TemplateSegments.Length != requestSegments.Length)
            {
                return false;
            }

            for (var index = 0; index < TemplateSegments.Length; index++)
            {
                if (!SegmentMatches(TemplateSegments[index], requestSegments[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static string[] GetSegments(string value)
            => string.IsNullOrWhiteSpace(value) || value == "/"
                ? []
                : value.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        private static bool SegmentMatches(string templateSegment, string requestSegment)
        {
            if (IsParameterSegment(templateSegment))
            {
                return !string.IsNullOrWhiteSpace(requestSegment);
            }

            return string.Equals(templateSegment, requestSegment, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsParameterSegment(string segment)
            => segment.StartsWith('{') && segment.EndsWith('}');
    }
}
