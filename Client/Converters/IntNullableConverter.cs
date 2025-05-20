using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class IntNullableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value?.ToString();

            if (string.IsNullOrWhiteSpace(input))
                return 0;

            if (int.TryParse(input, out int result))
                return result;

            return 0;
        }
    }
}
