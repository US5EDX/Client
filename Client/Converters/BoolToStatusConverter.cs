using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                throw new InvalidOperationException("The target must be a bool");

            if ((bool)value)
                return "✔️";
            else
                return "❌";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
