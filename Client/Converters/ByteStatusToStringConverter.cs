using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(byte), typeof(string))]
    public class ByteStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte val)
                throw new InvalidOperationException("The target must be a byte");

            switch (val)
            {
                case 0:
                    return "❌";
                case 1:
                    return "✔️";
                case 2:
                    return "❔";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
