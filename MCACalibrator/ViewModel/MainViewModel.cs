using CAENDigitizerWrapper;
using CITHEPLib;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using LinearMotorWrapper;
using TemperatureSensor;
using MCACalibrator.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml.Serialization;
using System.IO;
using MCACalibrator.Threads;
namespace MCACalibrator.ViewModel
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public String Message { get; set; }
        public String Detail { get; set; }
        public enum LogType { Error, Debug, Info };
        public double LastAverage { get; set; }
        public double LastLength { get; set; }

        public LogEntry(string message, string detail = "", DateTime? timestamp = null)
        {
            Message = message;
            Detail = detail;
            Timestamp = timestamp ?? DateTime.Now;
        }

    }

    public class PeripheralConnection
    {
        public PeripheralConnection(bool motor = false, bool mca = false, bool thermometer = false)
        {
            MotorConnected = motor;
            MCAConnected = mca;
            ThermometerConnected = thermometer;
        }
        public bool MotorConnected { get; set; }
        public bool MCAConnected { get; set; }
        public bool ThermometerConnected { get; set; }

        public bool AllConnected
        {
            get
            {
                return MotorConnected && MCAConnected && ThermometerConnected;
            }
        }
        public override string ToString()
        {
            return String.Format("Motor:{0} MCA:{1} Thermometer:{2}", MotorConnected, MCAConnected, ThermometerConnected);
        }
    }

    public class MotorStatus
    {
        public MotorStatus()
        {
            this.LinearMotor = null;
        }
        public MotorStatus(LinearMotor LinearMotor)
        {
            this.LinearMotor = LinearMotor;
        }
        private LinearMotorWrapper.LinearMotor LinearMotor;
        public void Refresh()
        {
            IsConnected = LinearMotor.IsConnected();
            IsRunning = IsConnected ? LinearMotor.IsRunning() : false;
            Position = IsConnected ? LinearMotor.Position() : -999;
            PositionCM = IsConnected ? LinearMotor.PositionCM() : -999;
            Powered = IsConnected ? LinearMotor.MotorPower() : false;
        }
        public int? Position { get; private set; }
        public double? PositionCM { get; private set; }
        public bool Powered { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsIdle { get { return IsConnected && !IsRunning; } }
    }

    [PropertyChanged.ImplementPropertyChanged]
    public class FillExperimentArgument
    {
        public MCASetting Setting { get; set; }
        public double StartPosition { get; set; }
        public double EndPosition { get; set; }
        public int NumPosition { get; set; }
        public int MaxSample { get; set; }
        public double Timeout { get; set; }
        public FillExperimentArgument()
        {
            Setting = null;
            StartPosition = 0;
            EndPosition = 12;
            NumPosition = 6;
            MaxSample = 2000;
            Timeout = 60;
        }
    }

    public class MainViewModel : Bindable
    {
        public DT5780 MCA { get; set; }
        public LinearMotorWrapper.LinearMotor LinearMotor { get; set; }
        //public MCACalibrator.Threads.DAQThread DAQThread;
        public DigitalThermometer Thermometer { get; set; }
        public MotorStatus MotorStatus { get; private set; }
        public SettingMaker SettingMaker { get; set; }
        public QCData QCData { get; set; }
        static MainViewModel()
        {
            instance = null;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    App.Settings.Reset();
            //}
            MCA = new DT5780();
            LinearMotor = LinearMotorWrapper.LinearMotor.GetInstance();
            Thermometer = new DigitalThermometer();

            //DAQThread = new MCACalibrator.Threads.DAQThread(MCA);
            MotorStatus = new MotorStatus(LinearMotor);
            PeripheralConnection = new PeripheralConnection();
            LogEntries = new BindingList<LogEntry>();
            ExperimentalRuns = new ObservableCollection<ExperimentalRun>();
            QCData = new QCData();

            MCASettings = App.Settings.MCASettings;

            StartHVPoller();
            StartMotorPoller();
            StartPeripheralConnectionPoller();
            SetupCommand();

            MotorCommand = new MotorCommand(this);
            SettingMaker = new SettingMaker(MCA, LinearMotor, Thermometer, MCASettings);
            

        }

        static private MainViewModel instance;
        public static MainViewModel GetInstance()
        {
            if (instance == null)
            {
                instance = new MainViewModel();
            }
            return instance;
        }
        public MotorCommand MotorCommand { get; private set; }

        private ObservableCollection<ExperimentalRun> _ExperimentalRuns;
        public ObservableCollection<ExperimentalRun> ExperimentalRuns
        {
            get
            {
                return _ExperimentalRuns;
            }
            set
            {
                SetProperty(ref _ExperimentalRuns, value);
            }
        }

        private ObservableCollection<MCASetting> _MCASettings;
        public ObservableCollection<MCASetting> MCASettings
        {
            get
            {
                return _MCASettings;
            }
            set
            {
                //SetProperty(ref _MCASettings, value);
                _MCASettings = value;
            }
        }

        public void FillExperimentalRuns(FillExperimentArgument arg = null)
        {
            if (arg == null) arg = new FillExperimentArgument();

            double[] positions = NP.Linspace(arg.StartPosition, arg.EndPosition, arg.NumPosition);
            foreach (double position in positions)
            {
                ExperimentalRun run = new ExperimentalRun(MCA, LinearMotor, Thermometer, RunMode.Experiment);
                run.Log.Setting = arg.Setting;
                run.Log.MotorPosition = position;
                run.MaxSample = arg.MaxSample;
                run.Timeout = arg.Timeout;
                //Debug.WriteLine(arg.Setting.Name);
                ExperimentalRuns.Add(run);
            }
        }

        /// <summary>
        /// Cleanup mvm. Turn off HV. Stop DAQ. Stop and disconnect Motor.
        /// </summary>
        public void CleanUp()
        {
            try
            {
                MCA.set_HVPower(0, false);
                MCA.set_HVPower(1, false);
            }
            catch (MCAException)
            {
                //just ignore
            }
            //DAQThread.Stop();
            LinearMotor.Close();
        }

        public BindingList<LogEntry> LogEntries { get; set; }

        public void ConnectPeripheral()
        {

            if (!MCA.IsConnected()) MCA.ScanConnect();
            if (MCA.IsConnected())
            {
                MCA.Reset();
                MCA.UseDefaultBoardSetting();
                //App.MCA.UseDefaultChannelSetting(0);
            }
            Debug.WriteLine(LinearMotor.IsConnected());
            if (!LinearMotor.IsConnected()) LinearMotor.ScanConnect();
            if (LinearMotor.IsConnected())
            {
                LinearMotor.Calibrate();
                LinearMotor.Wait(2000);
            }
            else
            {
                Debug.WriteLine("Motor fail to connect");
            }

            if (!Thermometer.IsConnected()) Thermometer.ScanConnect();
            if (Thermometer.IsConnected())
            {
                Thermometer.StartDataPoller();
            }
        }
        public void ToggleHV(int channel)
        {
            Console.WriteLine("Powered: {0}", MCA.get_HVStatus(channel).Powered);
            if (MCA.get_HVStatus(channel).Powered)
            {
                MCA.set_HVPower(channel, false);
            }
            else
            {
                MCA.set_HVPower(channel, true);
            }
        }

        public SimpleCommand<FillExperimentArgument> FillExperimentalRunsCommand { get; private set; }
        public SimpleCommand<object> ClearExperimentalRunsCommand { get; private set; }
        public SimpleCommand<int> ToggleHVCommand { get; private set; }
        public SimpleCommand<object> ConnectPeripheralCommand { get; private set; }
        public SimpleCommand<ExperimentalRun> RunSingleCommand { get; private set; }
        public SimpleCommand<object> SaveExperimentCommand { get; private set; }
        public SimpleCommand<object> RunAllSimulationCommand { get; private set; }
        public SimpleCommand<object> StopServerCommand { get; private set; }
        public SimpleCommand<object> RunAllCommand { get; private set; }
        public SimpleCommand<object> FitChannel1Command { get; private set; } //use default peak finder setting and fit the peak
        public SimpleCommand<object> FitChannel0Command { get; private set; }
        public SimpleCommand<object> FitAllChannelCommand { get; private set; }
        public SimpleCommand<object> RunQCTestCommand { get; private set; }

        private void SetupCommand()
        {
            ToggleHVCommand = new SimpleCommand<int>(this.ToggleHV);
            ConnectPeripheralCommand = new SimpleCommand<object>(o => this.ConnectPeripheral());
            FillExperimentalRunsCommand = new SimpleCommand<FillExperimentArgument>(o => this.FillExperimentalRuns(o), o => o.Setting != null);
            ClearExperimentalRunsCommand = new SimpleCommand<object>(o => { ExperimentalRuns.Clear(); QCData = new QCData(); });
            RunSingleCommand = new SimpleCommand<ExperimentalRun>(o => App.ExperimentServer.AddJobs(o));
            SaveExperimentCommand = new SimpleCommand<object>(o => this.SaveWithDialog());
            RunAllSimulationCommand = new SimpleCommand<object>(o => this.RunAllSimulation());
            RunAllCommand = new SimpleCommand<object>(o => this.RunAll());
            StopServerCommand = new SimpleCommand<object>(o => App.ExperimentServer.StopAndClearJob());
            FitChannel1Command = new SimpleCommand<object>(o => FitAllPeak(0), Async:true);
            FitChannel0Command = new SimpleCommand<object>(o => FitAllPeak(1), Async:true);
            FitAllChannelCommand = new SimpleCommand<object>(o => { FitAllPeak(0); FitAllPeak(1); }, Async:true);
            RunQCTestCommand = new SimpleCommand<object>(o => RunQCTest());
        }
        /// <summary>
        /// Run Quality control test ie building QCData
        /// Look at class QCData on how to implement this
        /// </summary>
        private void RunQCTest()
        {
            QCData = new QCData(ExperimentalRuns);
        }


        private void FitAllPeak(int channel)
        {
            PeakFinderSetting pfsetting = new PeakFinderSetting() { Channel = channel };
            foreach (ExperimentalRun run in ExperimentalRuns)
            {
                run.AutoFit(pfsetting);
            }
        }

        public HVStatusType HV0Status
        {
            get
            {
                try
                {
                    return MCA.get_HVStatus(0);
                }
                catch (MCAException)
                {
                    return null;
                }
            }
        }
        public HVStatusType HV1Status
        {
            get
            {
                try
                {
                    return MCA.get_HVStatus(1);
                }
                catch (MCAException)
                {
                    return null;
                }
            }
        }
        public void StartHVPoller()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (DoWorkEventHandler)(
                            (s, e) =>
                            {
                                while (!(s as BackgroundWorker).CancellationPending)
                                {
                                    Thread.Sleep(500);
                                    OnPropertyChanged("HV0Status");
                                    OnPropertyChanged("HV1Status");
                                }
                            }
                         );
            bw.RunWorkerAsync();
        }

        public PeripheralConnection PeripheralConnection { get; set; }

        /***
         * Polling peripheral connection
         * This is needed since if someone pull the cable we won't get property changed
         ***/
        public void StartPeripheralConnectionPoller()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (DoWorkEventHandler)(
                (s, e) =>
                {
                    while (!(s as BackgroundWorker).CancellationPending)
                    {
                        PeripheralConnection.MotorConnected = this.LinearMotor.IsConnected();
                        PeripheralConnection.MCAConnected = MCA.IsConnected();
                        PeripheralConnection.ThermometerConnected = Thermometer.IsConnected();

                        OnPropertyChanged("PeripheralConnection");
                        Thread.Sleep(1000);
                    }
                });

            bw.RunWorkerAsync();
        }

        public void StartMotorPoller()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (DoWorkEventHandler)(
                    (s, e) =>
                    {
                        while (!(s as BackgroundWorker).CancellationPending)
                        {
                            Thread.Sleep(500);
                            MotorStatus.Refresh();
                            OnPropertyChanged("MotorStatus");
                        }
                    }
                );
            bw.RunWorkerAsync();
        }

        public void RunAll()
        {
            foreach (ExperimentalRun run in ExperimentalRuns)
            {
                App.ExperimentServer.AddJobs(run);
            }

        }

        public void RunAllSimulation()
        {
            foreach (ExperimentalRun run in ExperimentalRuns)
            {
                App.ExperimentServer.AddJobs(run, RunMode.Simulated);
            }
        }
        public void SaveWithDialog()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "ExperimentResult"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Text documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                Debug.WriteLine(filename);
                Save(filename);
            }
        }
        public void Save(string filename)
        {
            ExperimentSaveData esd = new ExperimentSaveData();
            foreach (ExperimentalRun run in ExperimentalRuns)
            {
                esd.Logs.Add(run.Log);
            }
            esd.QCData = QCData;
            XmlSerializer ser = new XmlSerializer(esd.GetType());
            StreamWriter writer = new StreamWriter(filename);
            ser.Serialize(writer, esd);
            writer.Close();
        }
        public void Reset()
        {
        }

    }

    [Serializable]
    public class ExperimentSaveData
    {
        public QCData QCData { get; set; }
        public List<ExperimentLog> Logs { get; set; }
        public ExperimentSaveData()
        {
            Logs = new List<ExperimentLog>();
            QCData = new QCData();
        }
    }
}
