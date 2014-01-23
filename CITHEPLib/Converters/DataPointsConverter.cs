using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using OxyPlot;
namespace CITHEPLib.Converters
{
    public class DataPointsConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IList<double> v = value as IList<double>;
            DataPoint[] ret = new DataPoint[v.Count()];
            Console.WriteLine(ret.Length);
            for(int x=0; x<v.Count(); x++)
            {
                ret[x] = new DataPoint(x, v[x]);
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
