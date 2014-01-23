using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CITHEPLib.Converters
{
    public class IntHistogramToDatapointsConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IntHistogram hist = value as IntHistogram;
            Debug.WriteLine("Beeeeee {0}", hist == null);
            if (hist == null) return null;
            DataPoint[] ret = new DataPoint[hist.NBins];
            for (int i = 0; i < hist.NBins; i++)
            {
                ret[i] = new DataPoint(i, hist.Channel[i]);
            }
            Debug.WriteLine(ret.Length);
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
