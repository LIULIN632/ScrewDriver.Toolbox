using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScrewDriver.Toolbox.UI.Converters;

public static class IconHelper
{
    private static readonly Dictionary<string, ImageSource> _cache = new();

    public static ImageSource? GetIcon(string exePath)
    {
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            return null;

        if (_cache.TryGetValue(exePath, out var cached))
            return cached;

        try
        {
            using var icon = Icon.ExtractAssociatedIcon(exePath);
            if (icon == null) return null;

            var source = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            _cache[exePath] = source;
            return source;
        }
        catch
        {
            return null;
        }
    }
}
