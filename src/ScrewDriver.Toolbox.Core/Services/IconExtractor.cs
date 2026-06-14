using System.Drawing;

namespace ScrewDriver.Toolbox.Core.Services;

public static class IconExtractor
{
    public static string? ExtractIconToFile(string exePath)
    {
        var cacheDir = Path.Combine(Path.GetTempPath(), "ScrewDriver.Toolbox", "icons");
        return ExtractAndSaveIcon(exePath, cacheDir);
    }

    public static string? ExtractAndSaveIcon(string exePath, string cacheDir)
    {
        try
        {
            if (!File.Exists(exePath)) return null;

            var iconName = Path.GetFileNameWithoutExtension(exePath) + ".png";
            var iconPath = Path.Combine(cacheDir, iconName);

            if (File.Exists(iconPath)) return iconPath;

            using var icon = Icon.ExtractAssociatedIcon(exePath);
            if (icon == null) return null;

            using var bitmap = icon.ToBitmap();
            Directory.CreateDirectory(cacheDir);
            bitmap.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
            return iconPath;
        }
        catch
        {
            return null;
        }
    }
}
