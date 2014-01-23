using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CITHEPLib;
using CAENDigitizerWrapper;
using LinearMotorWrapper;
using TemperatureSensor;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace MCACalibrator.ViewModel
{


    [ImplementPropertyChanged]
    public class SettingMaker
    {
        public DT5780 mca { get; private set; }
        public LinearMotor motor { get; private set; }
        public DigitalThermometer thermometer { get; private set; }
        public MCASetting CurrentSetting { get; set; }
        public MCAData MCAData { get; set; }
        public ObservableCollection<MCASetting> MCASettings { get; set; }
        public double TargetPosition { get; set; }
        public int MaxSample { get; set; }
        public double Timeout { get; set; }
        public SettingMaker(DT5780 mca, LinearMotor motor, DigitalThermometer thermometer, ObservableCollection<MCASetting> mcasettings)
        {
            this.mca = mca;
            this.motor = motor;
            this.thermometer = thermometer;
            this.MCASettings = mcasettings;
            this.MCAData = new MCAData();
            TargetPosition = -15;
            MaxSample = 0;
            Timeout = -1;
            InitCommand();
        }

        public SimpleCommand<object> StartTestRunCommand { get; private set; }
        public SimpleCommand<object> StopTestRunCommand { get; private set; }
        public SimpleCommand<object> SendSWTriggerCommand { get; private set; }
        public SimpleCommand<object> ClearPlotCommand { get; private set; }

        public SimpleCommand<int> HVToggleCommand { get; private set; }
        public SimpleCommand<int?> MoveToCommand { get; private set; }
        public SimpleCommand<object> AddSettingCommand { get; private set; }
        public SimpleCommand<MCASetting> RemoveSettingCommand { get; private set; }
        public SimpleCommand<object> SaveSettingCommand { get; private set; }
        public SimpleCommand<object> DebugCommand { get; private set; }
        public SimpleCommand<object> StartSimulationRunCommand { get; private set; }
        public void InitCommand()
        {
            StartTestRunCommand = new SimpleCommand<object>(o => this.StartTestRun(), o => mca.IsConnected());
            StopTestRunCommand = new SimpleCommand<object>(o => this.StopTestRun());
            SendSWTriggerCommand = new SimpleCommand<object>(o => this.SendSWTrigger());
            ClearPlotCommand = new SimpleCommand<object>(o => this.ClearPlot());

            HVToggleCommand = new SimpleCommand<int>(i => this.ToggleHV(i));
            MoveToCommand = new SimpleCommand<int?>(d => this.MoveTo(d.Value), d => (motor.IsConnected()));
            AddSettingCommand = new SimpleCommand<object>(o => this.AddSetting());
            RemoveSettingCommand = new SimpleCommand<MCASetting>(o => this.RemoveSetting(o));
            SaveSettingCommand = new SimpleCommand<object>(o => { App.Settings.Save(); Debug.WriteLine("Save"); });
            DebugCommand = new SimpleCommand<object>(o => this.Test());
            StartSimulationRunCommand = new SimpleCommand<object>(o => this.StartSimulationRun());
        }


        public void Test()
        {
            Debug.WriteLine(MCASettings[0].XMLString());
            Debug.WriteLine(".......");
            Debug.WriteLine(App.Settings.MCASettings[0].XMLString());
        }

        public void StartTestRun()
        {
            if (CurrentSetting != null)
            {
                ExperimentalRun run = new ExperimentalRun(mca, motor, thermometer, RunMode.Calibrate);
                //use our thing as data collector
                run.Log.Setting = CurrentSetting;
                run.Log.MCAData = this.MCAData;
                run.Log.MotorPosition = TargetPosition;
                run.Timeout = Timeout;
                run.MaxSample = MaxSample;
                App.ExperimentServer.AddJobs(run);
            }
            else
            {
                MessageBox.Show("Setting Not Selected.", "Setting Not Selected", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartSimulationRun()
        {
            ExperimentalRun run = new ExperimentalRun(mca, motor, thermometer, RunMode.Simulated);
            run.Log.MCAData = this.MCAData;
            App.ExperimentServer.AddJobs(run);
        }

        public void StopTestRun()
        {
            App.ExperimentServer.StopAndClearJob();
        }

        public void SendSWTrigger()
        {
            mca.SendSWTrigger();
        }

        public void ClearPlot()
        {
            MCAData.Clear();
        }

        public void ToggleHV(int channel)
        {
            mca.ToggleHV(channel);
        }

        public void AddSetting()
        {
            MCASettings.Add(new MCASetting("Default Name"));
            //Debug.WriteLine(MCASettings.Count);
        }

        public void RemoveSetting(MCASetting setting)
        {
            for (int i = 0; i < MCASettings.Count; i++)
            {
                if (Object.ReferenceEquals(setting, MCASettings[i]))
                {
                    MCASettings.RemoveAt(i);
                    break;
                }
            }
        }
        public void MoveTo(double cm)
        {
            motor.MoveTocm(cm);
        }

        public bool SaveCurrentMCA(string filename)
        {
            return false;
        }

        public void SaveFileDialog()
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.DefaultExt = ".xml";
            dialog.Filter = "MCA Setting (.xml)|*.xml";

            bool? save = dialog.ShowDialog();
            if (save.HasValue && save.Value)
            {
                string filename = dialog.SafeFileName;
                bool success = SaveCurrentMCA(filename);
                if (!success)
                {
                    Debug.WriteLine("Failed");
                }
            }

        }
    }
}
