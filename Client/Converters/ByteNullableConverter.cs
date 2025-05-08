using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(byte), typeof(string))]
    public class ByteNullableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value?.ToString();

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (byte.TryParse(input, out byte result))
                return result;

            return string.Empty;
        }
    }
}
