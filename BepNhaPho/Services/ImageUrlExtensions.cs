using Microsoft.AspNetCore.Mvc;

namespace BepNhaPho.Services;

public static class ImageUrlExtensions
{
    public static string RecipeImage(this IUrlHelper url, string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath)) return string.Empty;

        var path = storedPath.Trim().Replace('\\', '/');
        if (path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return path;

        // Older data may contain a wwwroot prefix or a physical project path.
        // Browsers need the public URL, resolved against the application's PathBase.
        var webRootIndex = path.IndexOf("/wwwroot/", StringComparison.OrdinalIgnoreCase);
        if (webRootIndex >= 0) path = path[(webRootIndex + "/wwwroot/".Length)..];
        else if (path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            path = path["wwwroot/".Length..];

        if (path.StartsWith("~/", StringComparison.Ordinal)) path = path[2..];
        return url.Content("~/" + path.TrimStart('/'));
    }
}
