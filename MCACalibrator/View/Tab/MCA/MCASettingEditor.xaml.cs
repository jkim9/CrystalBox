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

namespace MCACalibrator.View.Tab.MCA
{
    /// <summary>
    /// Interaction logic for MCASettingEditor.xaml
    /// </summary>
    public partial class MCASettingEditor : UserControl
    {
        public static readonly DependencyProperty SettingProperty = DependencyProperty.Register("Setting", typeof(MCASetting), typeof(MCASettingEditor));

        public MCASetting Setting
        {
            get
            {
                return GetValue(SettingProperty) as MCASetting;
            }
            set
            {
                SetValue(SettingProperty, value);
            }
        }
        public MCASettingEditor()
        {
            InitializeComponent();
        }
    }
}
