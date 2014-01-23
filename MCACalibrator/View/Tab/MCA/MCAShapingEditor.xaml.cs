﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for MCAShapingEditor.xaml
    /// </summary>
    public partial class MCAShapingEditor : UserControl
    {
        public static readonly DependencyProperty SettingProperty = DependencyProperty.Register("Setting", typeof(ShapingSetting), typeof(MCAShapingEditor));

        public ShapingSetting Setting
        {
            get
            {
                return GetValue(SettingProperty) as ShapingSetting;
            }
            set
            {
                SetValue(SettingProperty, value);
            }
        }

        public MCAShapingEditor()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Setting.ToString());
        }
    }
}
