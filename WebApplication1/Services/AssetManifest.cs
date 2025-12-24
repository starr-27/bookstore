using Microsoft.AspNetCore.Hosting;

namespace WebApplication1.Services;

public interface IAssetManifest
{
    IReadOnlyList<string> GetFlowerCovers();
    IReadOnlyList<string> GetLandBackgrounds();
}

public sealed class AssetManifest : IAssetManifest
{
    private readonly IReadOnlyList<string> _flowerCovers;
    private readonly IReadOnlyList<string> _landBackgrounds;

    public AssetManifest(IWebHostEnvironment env)
    {
        _flowerCovers = LoadRelativeUrls(env, Path.Combine("images", "flowers"));
        _landBackgrounds = LoadRelativeUrls(env, Path.Combine("images", "land"));
    }

    public IReadOnlyList<string> GetFlowerCovers() => _flowerCovers;

    public IReadOnlyList<string> GetLandBackgrounds() => _landBackgrounds;

    private static IReadOnlyList<string> LoadRelativeUrls(IWebHostEnvironment env, string relativeFolder)
    {
        var wwwroot = env.WebRootPath;
        var folder = Path.Combine(wwwroot, relativeFolder);
        if (!Directory.Exists(folder))
        {
            return Array.Empty<string>();
        }

        var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"
        };

        var urls = Directory.EnumerateFiles(folder)
            .Where(f => allowedExt.Contains(Path.GetExtension(f)))
            .Select(f => "/" + relativeFolder.Replace('\\', '/').Trim('/') + "/" + Path.GetFileName(f))
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return urls;
    }
}
