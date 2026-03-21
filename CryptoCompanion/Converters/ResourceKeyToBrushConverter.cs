using System.Globalization;

namespace CryptoCompanion.Converters;

public class ResourceKeyToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string resourceKey && Application.Current != null)
        {
            if (Application.Current.Resources.TryGetValue(resourceKey, out var resource) && resource is Brush brush)
            {
                return brush;
            }
        }
        return Colors.Transparent; // Default fallback
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
