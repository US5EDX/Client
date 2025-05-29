using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class AxisPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double axis && double.TryParse(parameter?.ToString(), out double percent))
                return axis * percent;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
