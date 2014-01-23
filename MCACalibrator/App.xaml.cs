using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CAENDigitizerWrapper;
using System.Threading;
using System.Windows.Threading;
using MCACalibrator.Threads;
using MCACalibrator.ViewModel;
using MCACalibrator.Setting;
namespace MCACalibrator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        public static ExperimentServer ExperimentServer { get; set; }
        public static MCACalibratorSettings Settings { get; set; }
        static App(){
            App.ExperimentServer = new ExperimentServer();
            Settings = new MCACalibratorSettings();
            
        }

        public void CleanUp(Object o, ExitEventArgs e)
        {
            MainViewModel.GetInstance().CleanUp();
        }
    }
}
