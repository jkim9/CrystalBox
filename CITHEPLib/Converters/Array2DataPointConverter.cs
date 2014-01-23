using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CITHEPLib.Converters
{
    public class Array2DataPointConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int[] v = value as int[];
            DataPoint[] ret = new DataPoint[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                ret[i] = new DataPoint(i, v[i]);
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
