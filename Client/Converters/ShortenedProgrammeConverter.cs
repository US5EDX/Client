using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(byte), typeof(string))]
    public class ShortenedProgrammeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte val)
                throw new InvalidOperationException("The target must be a byte");

            if (val == 0)
                return "Ні";
            else
                return $"Так ({val})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
