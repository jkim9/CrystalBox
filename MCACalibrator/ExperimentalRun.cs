using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CITHEPLib;

using System.Collections.ObjectModel;
using OxyPlot;
using CAENDigitizerWrapper;
using PropertyChanged;
using LinearMotorWrapper;
using TemperatureSensor;
using MCACalibrator.Threads;
using System.Diagnostics;
using System.Threading;
using CITHEPLib.PeakLocator;
using System.ComponentModel;
using System.Xml.Serialization;
namespace MCACalibrator
{
    //public interface IExperiment
    //{
    //     bool OnExperimentStart();
    //     bool OnExperimentEnd ();
    //     bool UpdateExperiment(ReadoutData rd);
    //}

    public enum RunMode
    {
        Experiment, //use setting exactly as written
        Calibrate, //Enable Software Trigger and use waveform mode
        //ignore turning on and off power at the beginning and the end
        Simulated //never touch mca just for testing
    }

    [ImplementPropertyChanged]
    [Serializable]
    public class QCData : INotifyPropertyChanged
    {
        public double[] resolution0 { get; set; }
        public double[] resolution1 { get; set; }
        public double[] positions { get; set; }
        public bool TestOK { get; set; }
        public QCData()
        {
            resolution0 = new double[0];
            resolution1 = new double[0];
            positions = new double[0];
            TestOK = false;
        }
        /// <summary>
        /// Construct QC data from collection of experimental runs
        /// </summary>
        /// <param name="runs"></param>
        public QCData(ObservableCollection<ExperimentalRun> runs)
        {
            double[] resolution0 = new double[runs.Count];
            double[] resolution1 = new double[runs.Count];
            double[] positions = new double[runs.Count];

            for (int i = 0; i < runs.Count; i++ )
            {
                positions[i] = runs[i].Log.MotorPosition;
                resolution0[i] = runs[i].Log.Channel0FitResult.Resolution;
                resolution1[i] = runs[i].Log.Channel1FitResult.Resolution;
            }
            this.resolution1 = resolution1;
            this.resolution0 = resolution0;
            this.positions = positions;
            TestOK = ComputeTestOK();
        }

        /// <summary>
        /// Put QC Test logic here
        /// </summary>
        /// <returns></returns>
        private bool ComputeTestOK()
        {
            return this.positions.Length > 4;//as an example if there are more than 4 point the test is passed :P
        }

        [XmlIgnore]
        public DataPoint[] Channel0ResolutionDataPoints
        {
            get
            {
                return DoubleArrayToDataPoints(positions, resolution0);
            }
        }

        [XmlIgnore]
        public DataPoint[] Channel1ResolutionDataPoints {
            get
            {
                return DoubleArrayToDataPoints(positions, resolution1);
            }
        }

        [XmlIgnore]
        public DataPoint[] Channel0FittedDataPoints {
            get
            {
                NP.LinearFitResult lfr = NP.LinearFit(positions, resolution0);
                double[] fittedy = NP.Broadcast((x) => lfr.m * x + lfr.c, positions);
                return DoubleArrayToDataPoints(positions, fittedy);
            }
        }
        [XmlIgnore]
        public DataPoint[] Channel1FittedDataPoints {
            get
            {
                NP.LinearFitResult lfr = NP.LinearFit(positions, resolution1);
                double[] fittedy = NP.Broadcast((x) => lfr.m * x + lfr.c, positions);
                return DoubleArrayToDataPoints(positions, fittedy);
            }
        }

        private DataPoint[] DoubleArrayToDataPoints(double[] x, double[] y)
        {
            int l = Math.Min(x.Length, y.Length);
            DataPoint[] ret = new DataPoint[l];
            for (int i = 0; i < l; i++)
            {
                ret[i] = new DataPoint(x[i], y[i]);
            }
            return ret;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [ImplementPropertyChanged]
    [Serializable]
    public class ExperimentLog
    {
        public MCASetting Setting { get; set; }
        public double MotorPosition { get; set; }
        public MCAData MCAData { get; set; }
        public double Temp1Start { get; set; }
        public double Temp2Start { get; set; }
        public double Temp1Stop { get; set; }
        public double Temp2Stop { get; set; }
        public InitialPeakGuess Channel0InitialPeakGuess { get; set; }
        public InitialPeakGuess Channel1InitialPeakGuess { get; set; }
        public double RunTime { get; set; }
        public FitResult Channel0FitResult { get; set; }
        public FitResult Channel1FitResult { get; set; }
        public ExperimentLog()
        {
            Setting = new MCASetting();
            MotorPosition = 0;
            MCAData = new MCAData();
            Temp1Start = 0;
            Temp2Start = 0;
            Temp1Stop = 0;
            Temp2Stop = 0;
            RunTime = 0;
            Channel0InitialPeakGuess = new InitialPeakGuess();
            Channel0FitResult = new FitResult();
            Channel1InitialPeakGuess = new InitialPeakGuess();
            Channel1FitResult = new FitResult();
        }
        public void ClearData()
        {
            MCAData.Clear();
            RunTime = 0;
        }

    }

    public enum ExperimentStatusEnum
    {
        Prepared, Running, Finished
    }

    [ImplementPropertyChanged]
    public class PeakFinderSetting
    {
        public int Channel { get; set; }
        public double Offset { get; set; }
        public double ScalingPower { get; set; }
        public double TriggerLevel { get; set; }
        public int SmoothWindow { get; set; }
        public PeakFinderSetting()
        {
            Channel = 0;
            Offset = 1;
            ScalingPower = 3.0;
            SmoothWindow = 100;
            TriggerLevel = 0.4;
        }
    }

    [ImplementPropertyChanged]
    public class ExperimentalRun
    {
        public RunMode RunMode;
        public LinearMotor motor;
        public DT5780 mca;

        public DigitalThermometer thermometer;
        public ExperimentLog Log { get; set; }
        public ExperimentStatusEnum ExperimentStatus { get; set; }
        private Dictionary<RunMode, DAQThread.OnSuccessfulStartDelegate> StartActionMap;
        private Dictionary<RunMode, DAQThread.OnSuccessfulStopDelegate> StopActionMap;
        private Dictionary<RunMode, DAQThread.UpdateProgressDelegate> UpdateActionMap;

        public DateTime? starttime = null;
        public double Timeout { get; set; }//in second use negative value for no timeout
        public int MaxSample { get; set; }//stop when either one of data length reach max sample
        public int hvpause = 5000;//5 second pause for HV to fully turn on
        public PeakFinder Channel0PeakFinder { get; set; }
        public PeakFinder Channel1PeakFinder { get; set; }
        public DataPoint[] Channel0FittedDataPoints { get; set; }
        public DataPoint[] Channel1FittedDataPoints { get; set; }
        /// <summary>
        /// Create Experimental Run.
        /// Refactor Idea: This class should not hold mca/motor/thermometer at all. It should be pass along from DAQThread as arguments.
        /// </summary>
        /// <param name="mca"></param>
        /// <param name="motor"></param>
        /// <param name="thermometer"></param>
        /// <param name="runmode"></param>
        public ExperimentalRun(DT5780 mca, LinearMotor motor, DigitalThermometer thermometer, RunMode runmode = RunMode.Experiment)
        {
            this.mca = mca;
            this.thermometer = thermometer;
            this.motor = motor;
            Timeout = 60;
            MaxSample = 0;
            Log = new ExperimentLog();
            ClearData();
            RunMode = runmode;
            Channel0PeakFinder = new PeakFinder();
            Channel1PeakFinder = new PeakFinder();
            ExperimentStatus = ExperimentStatusEnum.Prepared;
            StartActionMap = new Dictionary<RunMode, DAQThread.OnSuccessfulStartDelegate>{
                {RunMode.Experiment, OnExperimentStart},
                {RunMode.Calibrate, OnCalibrationStart},
                {RunMode.Simulated, OnSimulationStart}
            };

            StopActionMap = new Dictionary<RunMode, DAQThread.OnSuccessfulStopDelegate>{
                {RunMode.Experiment, OnExperimentEnd},
                {RunMode.Calibrate, OnCalibrationEnd},
                {RunMode.Simulated, OnSimulationEnd}
            };

            UpdateActionMap = new Dictionary<RunMode, DAQThread.UpdateProgressDelegate>{
                {RunMode.Experiment, OnExperimentUpdate},
                {RunMode.Calibrate, OnCalibrationUpdate},
                {RunMode.Simulated, OnSimulationUpdate}
            };
            InitCommand();
            Channel0FittedDataPoints = new DataPoint[0];
            Channel1FittedDataPoints = new DataPoint[0];
        }

        /// <summary>
        /// Initialize commands
        /// </summary>
        private void InitCommand()
        {
            FillInitialPeakGuessCommand = new SimpleCommand<PeakFinderSetting>(o => FillInitialPeakGuess(o));
            FitChannel0PeakCommand = new SimpleCommand<object>(o => FitPeak(0));
            FitChannel1PeakCommand = new SimpleCommand<object>(o => FitPeak(1));
        }

        public SimpleCommand<PeakFinderSetting> FillInitialPeakGuessCommand { get; private set; }
        /// <summary>
        /// Read Histogram from Log and find peak
        /// </summary>
        /// <param name="setting"></param>
        public void FillInitialPeakGuess(PeakFinderSetting setting)
        {
            Debug.Assert(setting.Channel == 0 || setting.Channel == 1);
            double[] y = setting.Channel == 0 ? Log.MCAData.Channel0Hist.ToDoubleArray() : Log.MCAData.Channel1Hist.ToDoubleArray();
            PeakFinder pf = new PeakFinder(y, setting.TriggerLevel, setting.ScalingPower, setting.SmoothWindow);
            if (setting.Channel == 0)
            {
                Channel0PeakFinder = pf;
            }
            else if (setting.Channel == 1)
            {
                Channel1PeakFinder = pf;
            }
            //Debug.WriteLine("aaa: {0}", pf.Guesses.Length);
        }

        /// <summary>
        /// Quick auto peakfinding/fit no step require just pick the first peak
        /// and fit it
        /// </summary>
        /// <param name="Channel"></param>
        public void AutoFit(PeakFinderSetting setting)
        {
            FillInitialPeakGuess(setting);
            PeakFinder pf = null;

            if (setting.Channel == 0) { pf = Channel0PeakFinder; }
            else if (setting.Channel == 1) { pf = Channel1PeakFinder; }

            if (pf.Guesses.Length == 0) { return; } //no peak found just return
            InitialPeakGuess guess = pf.Guesses[0];

            if (setting.Channel == 0) { Log.Channel0InitialPeakGuess = guess; }
            else if (setting.Channel == 1) { Log.Channel1InitialPeakGuess = guess; }

            FitPeak(setting.Channel);
        }

        public SimpleCommand<object> FitChannel0PeakCommand { get; private set; }
        public SimpleCommand<object> FitChannel1PeakCommand { get; private set; }

        public void FitPeak(int Channel)
        {
            InitialPeakGuess guess = null;
            double[] y = null;
            double[] sigma = null;
            if (Channel == 0)
            {
                guess = Log.Channel0InitialPeakGuess;
                y = Log.MCAData.Channel0Hist.ToDoubleArray();
            }
            else if (Channel == 1)
            {
                guess = Log.Channel1InitialPeakGuess;
                y = Log.MCAData.Channel1Hist.ToDoubleArray();

            }
            if (guess == null) { return; }
            double[] edges = NP.Linspace(-0.5, y.Length + 0.5, y.Length + 1);

            //make slices for fitting
            edges = NP.Slice(edges, guess.lowerboundIndex, guess.upperboundIndex + 1);
            y = NP.Slice(y, guess.lowerboundIndex, guess.upperboundIndex);

            //and error
            sigma = NP.Broadcast(Math.Sqrt, y);
            sigma = NP.Broadcast((o) => o >= 1 ? o : 1, sigma); //screen out zero bin giving it sigma=1

            PeakFitter pf = PeakFitter.FromGuess(guess, edges, y);
            pf.Fit();
            int nbin = guess.upperboundIndex - guess.lowerboundIndex + 1;
            if (Channel == 0)
            {
                Log.Channel0FitResult = new FitResult(pf.minuit, nbin);
                Channel0FittedDataPoints = pf.FittedDataPoint();
            }
            else if (Channel == 1)
            {
                Log.Channel1FitResult = new FitResult(pf.minuit, nbin);
                Channel1FittedDataPoints = pf.FittedDataPoint();
            }
        }


        /// <summary>
        /// Construct a daq thread
        /// </summary>
        /// <param name="forcemode">Temporary force runmode(useful for setting maker)</param>
        /// <returns></returns>
        public DAQThread ToDAQThread(RunMode? forcemode = null)
        {
            RunMode oldrunmode = this.RunMode;
            if (forcemode != null)
            {
                this.RunMode = forcemode.Value;
            }
            DAQThread ret = new DAQThread(this.mca)
            {
                OnSuccessfulStart = StartActionMap[RunMode],
                OnSuccessfulStop = StopActionMap[RunMode],
                UpdateProgress = UpdateActionMap[RunMode],
                ShouldStopCondition = this.ShouldStopRunning
            };
            if (RunMode == RunMode.Calibrate || RunMode == RunMode.Simulated) ret.nwaveform = 1;
            if (RunMode == RunMode.Simulated) ret.DryMode = true;
            this.RunMode = oldrunmode;
            return ret;
        }

        public void ClearData()
        {
            Log.ClearData();
        }

        /// <summary>
        /// Experiment startup call
        /// </summary>
        /// <returns></returns>
        public bool OnExperimentStart()
        {
            ExperimentStatus = ExperimentStatusEnum.Running;
            //turn on motor
            motor.SetMotorPower(1);
            //move motor
            motor.MoveTocm(Log.MotorPosition);
            motor.Wait();
            //turn off motor
            motor.SetMotorPower(0);
            //setting mca
            Log.Setting.UseSettingOnMCA(mca);
            //readTemperature
            TemperatureData T = thermometer.Read();
            //Start timer
            starttime = DateTime.Now;
            //HV on
            mca.set_HVPower(0, true);
            mca.set_HVPower(1, true);
            Thread.Sleep(hvpause);

            //read temperature
            Log.Temp1Start = T.Temperature1;
            Log.Temp2Start = T.Temperature2;
            //TODO: return the right value
            return true;
        }
        /// <summary>
        /// Update histogram
        /// </summary>
        /// <param name="rd"></param>
        /// <returns></returns>
        public bool OnExperimentUpdate(ReadoutData rd)
        {
            Log.MCAData.AddHist(rd);
            Log.MCAData.SetWaveform(rd);

            return true;
        }
        /// <summary>
        /// On experiment end...Timing is off though. The right way is to read timestamp from MCA but good for now.
        /// </summary>
        /// <returns></returns>
        public bool OnExperimentEnd()
        {
            Log.RunTime += (DateTime.Now - starttime.Value).TotalSeconds;
            ExperimentStatus = ExperimentStatusEnum.Finished;
            TemperatureData T = thermometer.Read();
            Log.Temp1Stop = T.Temperature1;
            Log.Temp2Stop = T.Temperature2;
            mca.set_HVPower(0, false);
            mca.set_HVPower(1, false);
            starttime = null;
            return true;
        }

        /// <summary>
        /// Start calibration mode in this mode. In this mode probe move is forced to output waveform
        /// </summary>
        /// <returns></returns>
        public bool OnCalibrationStart()
        {
            ExperimentStatus = ExperimentStatusEnum.Running;
            //turn on motor
            motor.SetMotorPower(1);
            //move motor
            motor.MoveTocm(Log.MotorPosition);
            motor.Wait();
            //turn off motor
            motor.SetMotorPower(0);
            //setting mca
            ProbeSetting.ProbeMode pmode = Log.Setting.ProbeSetting.Mode;
            Log.Setting.ProbeSetting.Mode = ProbeSetting.ProbeMode.Oscilloscope;
            Log.Setting.UseSettingOnMCA(mca);
            Log.Setting.ProbeSetting.Mode = pmode;
            //override the probe

            //readTemperature
            TemperatureData T = thermometer.Read();
            //Start timer
            starttime = DateTime.Now;
            //HV on
            mca.set_HVPower(0, true);
            mca.set_HVPower(1, true);
            Thread.Sleep(hvpause);
            //read temperature
            Log.Temp1Start = T.Temperature1;
            Log.Temp2Start = T.Temperature2;
            //TODO: return the right value
            return true;
        }
        /// <summary>
        /// Calibration Mode update histogram
        /// </summary>
        /// <param name="rd"></param>
        /// <returns></returns>
        public bool OnCalibrationUpdate(ReadoutData rd)
        {
            //Debug.WriteLine("boooo {0}", rd.Channel[0].Energies().Count);
            Log.MCAData.AddHist(rd);
            Log.MCAData.SetWaveform(rd);
            return true;
        }
        /// <summary>
        /// Calibration Mode Clean up.
        /// </summary>
        /// <returns></returns>
        public bool OnCalibrationEnd()
        {
            ExperimentStatus = ExperimentStatusEnum.Finished;
            starttime = null;
            mca.set_HVPower(0, false);
            mca.set_HVPower(1, false);
            return true;
        }

        /// <summary>
        /// Simulation start delegate
        /// </summary>
        /// <returns></returns>
        public bool OnSimulationStart()
        {
            ExperimentStatus = ExperimentStatusEnum.Running;
            starttime = DateTime.Now;
            return true;
        }
        /// <summary>
        /// Simulation update
        /// </summary>
        /// <returns></returns>
        public bool OnSimulationUpdate(ReadoutData rd)
        {
            //Debug.WriteLine("boooo {0}", rd.Channel[0].Energies().Count);
            Log.MCAData.AddHist(rd);
            Log.MCAData.SetWaveform(rd);
            return true;
        }

        /// <summary>
        /// Simulation update
        /// </summary>
        /// <returns></returns>
        public bool OnSimulationEnd()
        {
            ExperimentStatus = ExperimentStatusEnum.Finished;
            starttime = null;
            return true;
        }

        /// <summary>
        /// Check timeout and number of sample in histogram for stop condition
        /// </summary>
        /// <returns>shouldstop</returns>
        public bool ShouldStopRunning()
        {
            if (starttime == null) return false;
            DateTime now = DateTime.Now;
            double secpass = (now - starttime.Value).TotalSeconds;
            Debug.WriteLine("{0}/{1}", secpass, Timeout);
            bool reachmaxsample = (MaxSample > 0 && (Log.MCAData.Channel0Hist.NSample > MaxSample || Log.MCAData.Channel1Hist.NSample > MaxSample));
            bool reachtimeout = Timeout > 0 && secpass > Timeout;
            return reachtimeout || reachmaxsample;
        }


        public static ExperimentalRun DummyEmptyData(DT5780 mca, LinearMotor motor, DigitalThermometer thermometer, double position = 0)
        {
            ExperimentalRun ret = new ExperimentalRun(mca, motor, thermometer);
            ret.Log.Setting = MCASetting.LargeLYSOSetting();
            ret.Log.MotorPosition = position;
            return ret;
        }

        public static ExperimentalRun DummyFilledData(DT5780 mca, LinearMotor motor, DigitalThermometer thermometer, double position = 0)
        {
            ExperimentalRun ret = ExperimentalRun.DummyEmptyData(mca, motor, thermometer, position);
            ret.FillWithDummyData();
            return ret;
        }

        public void FillWithDummyData()
        {
            //Setting = new MCASetting("Setting 1", 100, 200);
            //MotorPosition = -20;
            double[] s = NP.Randn(10000, 1000, 50);
            int[] newsample = new int[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                newsample[i] = (int)Math.Round(s[i]);
            }
            Log.MCAData.Channel0Hist.Add(newsample);
            Log.MCAData.Channel1Hist.Add(newsample);
        }
    }
}
