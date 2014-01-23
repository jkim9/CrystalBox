using System;
using System.Windows;
using System.Windows.Controls;
using CAENDigitizerWrapper;
using System.Windows.Threading;
using MCACalibrator.ViewModel;
using MCACalibrator.Threads;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
namespace MCACalibrator.View
{
    public partial class MainWindow : Window
    {
        public bool Running = false;
        //These two should be in xaml instead... designer doesn't work in 64 bit.
        
        //public List<DT5780.BaselineAveragingWindowType> BaselineAveragingWindowEnum { get; set; }
        //public List<DT5780.PeakAveragingWindowType> PeakAveragingWindowEnum { get; set; }
        
        static MainWindow(){
            //BaselineAveragingWindowEnum = (cdw.DT5780.BaselineAveragingWindowType[])Enum.GetValues(typeof(cdw.DT5780.BaselineAveragingWindowType));
            //PeakAveragingWindowEnum = (cdw.DT5780.PeakAveragingWindowType[])Enum.GetValues(typeof(cdw.DT5780.PeakAveragingWindowType));
        }
        public MainWindow()
        {
            //BaselineAveragingWindowEnum = ((DT5780.BaselineAveragingWindowType[])Enum.GetValues(typeof(DT5780.BaselineAveragingWindowType))).ToList();
            //PeakAveragingWindowEnum = ((DT5780.PeakAveragingWindowType[])Enum.GetValues(typeof(DT5780.PeakAveragingWindowType))).ToList();
        
            InitializeComponent();
        
            MainViewModel mvm = MainViewModel.GetInstance();
            DataContext = mvm;
            App.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            //mvm.DAQThread.UpdateProgress += (DataContext as MainViewModel).UpdateHistogram;
            //mvm.DAQThread.UnhandledException += HandleDAQException;
            //mvm.DAQThread.OnSuccessfulStart += () => { Running = true; startBtn.Content = "Stop"; };
            //mvm.DAQThread.OnSuccessfulStop += () => { Running = false; startBtn.Content = "Start"; };
            //mvm.PropertyChanged += (o, e) =>
            //{
            //    if (e.PropertyName == "RawWaveform") WaveformPlot.InvalidatePlot(true);
            //};
            //mvm.ChannelHisto.PropertyChanged += (o, e) =>
            //{
            //    if (e.PropertyName == "Series") EnergyPlot.InvalidatePlot(true);
            //};

        }

        public void HandleDAQException(DAQThreadUnhandledExceptionArgs args)
        {
            //this will be running on non ui thread
            if (args.Exception is MCAException)
            {
                //MessageBox.Show(args.Exception.ToString());
                Application.Current.Dispatcher.BeginInvoke((Action)(()=>LogException(args.Exception)));
                args.Handled = true;
            }
        }

        public void LogException(Exception e)
        {
            MainViewModel mvm = (MainViewModel)DataContext;
            mvm.LogEntries.Add(new LogEntry(e.ToString(), e.StackTrace));
            //txtLog.Text += DateTime.Now.ToString("dd MMM HH:mm:ss") + ": ";
            //txtLog.Text += e.ToString() + "\n";
            //txtLog.Text += e.StackTrace + "\n";
            //tabLog.Header = "Log*";
        }

        public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is MCAException)
            {
                LogException(e.Exception);
                e.Handled = true;
            }
            else
            { 
                //do nothing
                e.Handled = false;
            }
        }

        //private void tabLog_Reset(object sender, RoutedEventArgs e)
        //{
        //    (sender as TabItem).Header = "Log";
        //}


        //private void startBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Running)
        //    {
        //        //Running=false;
        //        //(sender as Button).Content = "Start";
        //        //(DataContext as MainViewModel).DAQThread.Stop();

        //    }
        //    else
        //    {
        //        //Running = true;
        //        //(sender as Button).Content = "Stop";
        //        //(DataContext as MainViewModel).DAQThread.Start();
        //    }
        //}

        //private void clearPlotButton_Click(object sender, RoutedEventArgs e)
        //{
        //    (DataContext as MainViewModel).Reset();
        //    //EHistoPlot.RefreshPlot(true);
        //}


        //private void statusRefreshButton_Click(object sender, RoutedEventArgs e)
        //{
        //    MainViewModel mvm = (MainViewModel)DataContext;
        //    settingTextBox.Text = mvm.MCA.GetBoardStatus().ToString() +
        //        mvm.MCA.GetChannelStatus(0).ToString();
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ExperimentalRun exr = RunList.SelectedItem as ExperimentalRun;
            //Debug.WriteLine(exr.MotorPosition);
            //MainViewModel mvm = (MainViewModel)DataContext;
            //LinearMotorWrapper.LinearMotor LinearMotor = mvm.LinearMotor;
            //Random r = new Random();
            //LinearMotor.Abort();
            //LinearMotor.Wait();
            //LinearMotor.MoveTocm(r.NextDouble() * -20);
            //LinearMotor.Wait();
        }

    }
}
