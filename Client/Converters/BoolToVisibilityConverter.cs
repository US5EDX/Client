using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool val)
                throw new InvalidOperationException("The target must be a bool");

            if (Inverse) val = !val;

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
