using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Mg2.Converters
{
    public class StringCollectionsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                var stringValue = value as string;
                if (stringValue != null)
                {
                    return stringValue;
                }
                if (value == null)
                {
                    return String.Empty;
                }
                var parts = (IEnumerable<string>)value;
                return String.Join(", ", parts);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
