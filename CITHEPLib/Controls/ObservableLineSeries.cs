using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITHEPLib.Controls
{
    public class ObservableLineSeries : LineSeries
    {
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            //Debug.WriteLine("hey");
            OnDataChanged();
        }
    }

}
