using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using MCACalibrator.ViewModel;
using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Axes;
namespace MCACalibrator.View.Tab
{
    /// <summary>
    /// Interaction logic for RunList.xaml
    /// </summary>
    public partial class RunList : UserControl
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(Object), typeof(RunList));
        public Object SelectedItem { 
            get { return GetValue(SelectedItemProperty); } 
            set { SetValue(SelectedItemProperty, value); } 
        }

        public RunList()
        {
            InitializeComponent();
        }
    }
}
