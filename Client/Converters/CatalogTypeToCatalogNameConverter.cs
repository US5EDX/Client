﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(byte), typeof(string))]
    class CatalogTypeToCatalogNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte)
                throw new InvalidOperationException("The target must be a byte");

            switch ((byte)value)
            {
                case 1:
                    return "УВК";
                case 2:
                    return "ФВК";
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
