using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace CryptoCompanion.Converters;

public class SentimentToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string sentiment && Application.Current != null)
        {
            switch (sentiment.ToLowerInvariant())
            {
                case "bullish":
                    if (Application.Current.Resources.TryGetValue("GreenNeon", out var green) && green is Color greenColor)
                        return greenColor;
                    return Colors.Green;
                case "bearish":
                    if (Application.Current.Resources.TryGetValue("RedNeon", out var red) && red is Color redColor)
                        return redColor;
                    return Colors.Red;
                case "neutral":
                default:
                    if (Application.Current.Resources.TryGetValue("TextSecondary", out var gray) && gray is Color grayColor)
                        return grayColor;
                    return Colors.Gray;
            }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
