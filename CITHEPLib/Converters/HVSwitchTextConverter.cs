using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CITHEPLib.Converters
{
    public class HVSwitchTextConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool hvon = System.Convert.ToBoolean(value);
            int channel = System.Convert.ToInt32(parameter);
            return String.Format("HV{0} {1}", channel, hvon ? "Off" : "On");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
