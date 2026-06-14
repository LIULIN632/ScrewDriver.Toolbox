using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ScrewDriver.Toolbox.Helpers;

/// <summary>
/// 将非空字符串转换为 Visible，空字符串转换为 Collapsed
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
