using System;
using System.Globalization;
using System.Windows.Data;

namespace ScrewDriver.Toolbox.UI.Converters;

public class ExeIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string exePath)
            return IconHelper.GetIcon(exePath);
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
