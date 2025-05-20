using Client.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Client.Converters
{
    public class ThresholdsToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] is null || values[1] is null || values[2] is null
                || values[0] is not int count || values[1] is not DisciplineStatusThresholds thresholds
                || values[2] is not byte eduLevel)
                return Brushes.Transparent;

            var thresholdsValue = thresholds.GetValue(eduLevel);

            if (thresholdsValue is null) return Brushes.Transparent;

            if (count < thresholdsValue.NotEnough)
                return Brushes.LightPink;

            if (count < thresholdsValue.PartiallyFilled)
                return Brushes.LightYellow;

            return Brushes.LightGreen;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
