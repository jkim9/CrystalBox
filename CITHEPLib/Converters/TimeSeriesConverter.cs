using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using OxyPlot;

namespace CITHEPLib.Converters
{
    public class TimeSeriesConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convert array of double to List of DataPoints
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IList<double> v = values[0] as IList<double>;
            double xscale = System.Convert.ToDouble(values[1]);
            double yscale = System.Convert.ToDouble(values[2]);
            double yshift = System.Convert.ToDouble(values[3]);
            //parameter is string of the form xscale|yscale

            List<DataPoint> ret = new List<DataPoint>();
            for (int i = 0; i < v.Count(); i++)
            {
                ret.Add(new DataPoint(i * xscale, v[i] * yscale + yshift));
            }
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
