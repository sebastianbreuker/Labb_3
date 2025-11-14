using System;
using System.Globalization;
using System.Windows.Data;

namespace Labb_3.Utilities
{
    public class NullableTimeSpanSecondsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return ((int)timeSpan.TotalSeconds).ToString(culture);
            }

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text && int.TryParse(text, NumberStyles.Integer, culture, out var seconds) && seconds >= 0)
            {
                return TimeSpan.FromSeconds(seconds);
            }

            return null;
        }
    }
}
