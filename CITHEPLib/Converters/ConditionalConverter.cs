using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CITHEPLib.Converters
{
    public class ConditionalConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Any(e => e == DependencyProperty.UnsetValue)) return null;
            bool x = System.Convert.ToBoolean(values[0]);
            object truevalue = values[1];
            object falsevalue = values[2];
            object ret = x ? truevalue : falsevalue;
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
