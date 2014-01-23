using CAENDigitizerWrapper;
using CITHEPLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MCACalibrator.Threads
{
    public class DAQThreadUnhandledExceptionArgs
    {
        public readonly Exception Exception;
        public bool Handled = false;
        public DAQThreadUnhandledExceptionArgs(Exception e)
        {
            this.Exception = e;
        }
    }

    public enum ExperimentServerStatus
    {
        Idle, Running, Stopping
    };

    public class ExperimentServer : INotifyPropertyChanged
    {
        private ConcurrentQueue<DAQThread> QJobs = new ConcurrentQueue<DAQThread>();
        //private List<ExperimentalRun> Jobs = null;

        //private DAQThread runningthread = null;
        private Thread serverthread = null;
        private bool shouldstopserver = false;
        private bool pause = false;
        private bool kill = false;

        private ExperimentServerStatus serverStatus;
        public ExperimentServerStatus ServerStatus
        {
            get
            {
                return serverStatus;
            }
            set
            {
                serverStatus = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ServerStatus"));
            }
        }

        public ExperimentServer()
        {
            serverthread = new Thread(new ThreadStart(this.mainloop));
            serverthread.IsBackground = true;
            serverthread.Start();
            ServerStatus = ExperimentServerStatus.Idle;
        }

        private void DoPause()
        {
            while (pause) { Thread.Sleep(200); }
        }

        private void mainloop()
        {

            while (!shouldstopserver)
            {
                DAQThread thisthread = null;
                ServerStatus = ExperimentServerStatus.Idle;
                DoPause();
                //Debug.Write(".");
                Thread.Sleep(1000);
                if (QJobs.TryDequeue(out thisthread))
                {
                    //Debug.WriteLine("Running");
                    PropertyChanged(this, new PropertyChangedEventArgs("JobLeft"));
                    ServerStatus = ExperimentServerStatus.Running;
                    thisthread.Start();
                    while (!thisthread.Wait(1000))
                    {
                        //Debug.WriteLine("Running2");
                        if (kill)
                        {
                            //Debug.WriteLine("Killing");
                            ServerStatus = ExperimentServerStatus.Stopping;
                            thisthread.Stop();
                            DAQThread tmp;
                            while (QJobs.TryDequeue(out tmp)) { }
                            PropertyChanged(this, new PropertyChangedEventArgs("JobLeft"));
                            kill = false;
                        }
                    }
                    //Debug.WriteLine("Closing");
                }
                if (kill)
                {
                    ServerStatus = ExperimentServerStatus.Stopping;
                    DAQThread tmp;
                    while (QJobs.TryDequeue(out tmp)) { }
                    PropertyChanged(this, new PropertyChangedEventArgs("JobLeft"));
                    kill = false;
                }
                ServerStatus = ExperimentServerStatus.Idle;
            }
        }

        public void AddJobs(List<ExperimentalRun> runs, RunMode? forcemode=null)
        {
            foreach (ExperimentalRun run in runs)
            {
                QJobs.Enqueue(run.ToDAQThread(forcemode));
                PropertyChanged(this, new PropertyChangedEventArgs("JobLeft"));
            }
        }

        public void AddJobs(ExperimentalRun run, RunMode? forcemode=null)
        {
            QJobs.Enqueue(run.ToDAQThread(forcemode));
            PropertyChanged(this, new PropertyChangedEventArgs("JobLeft"));
        }

        public void StopAndClearJob()
        {
            kill = true;
        }

        public void Pause()
        {
            pause = true;
        }

        public void StopServer()
        {
            shouldstopserver = true;
        }

        public int JobLeft { get { return QJobs.Count; } }


        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class DAQThread : Bindable
    {
        private volatile bool shouldstop = false;

        public DT5780 MCA;
        public AutoResetEvent ready = new AutoResetEvent(true);

        public delegate bool ShouldStopConditionDelegate();
        public delegate bool UpdateProgressDelegate(ReadoutData rd);
        public delegate void DAQThreadUnhandledExceptionEventHandler(DAQThreadUnhandledExceptionArgs args);
        public delegate bool OnSuccessfulStartDelegate();
        public delegate bool OnSuccessfulStopDelegate();

        public DAQThreadUnhandledExceptionEventHandler UnhandledException = delegate { };
        public UpdateProgressDelegate UpdateProgress = (rd) => true;
        public ShouldStopConditionDelegate ShouldStopCondition = () => false;
        public OnSuccessfulStartDelegate OnSuccessfulStart = () => true;
        public OnSuccessfulStopDelegate OnSuccessfulStop = () => true;
        public int PollInterval = 100;
        private Thread thread = null;
        public DAQThread(DT5780 MCA) { this.MCA = MCA; }
        public int nwaveform = 0;
        public bool DryMode = false; // never touch peripherals just fake numbers
 
        public void Start()
        {
            if (thread == null || !thread.IsAlive)
            {
                shouldstop = false;
                thread = new Thread(() =>
                    Acquire()
                ) { Name = "DAQThread" };
                thread.IsBackground = true;
                ready.Reset();//this needs to be reset before return
                thread.Start();
            }
        }
        
        public void Stop()
        {
            shouldstop = true;
        }
        
        public bool Wait(int sec = Timeout.Infinite)
        {
            bool ret = ready.WaitOne(sec);
            return ret;
        }
        
        private void SimulatedReadoutData()
        {
            ReadoutData rd = new ReadoutData(2);
        }
        
        public void Acquire()
        {
            try
            {
                //Application.Current.Dispatcher.Invoke(OnSuccessfulStart);//this needs to be sync call
                OnSuccessfulStart();
                //Debug.WriteLine(MCA.GetBoardStatus());
                if (!DryMode) MCA.StartAcquisition();
                while (!shouldstop && !ShouldStopCondition())
                {
                    Thread.Sleep(PollInterval);
                    ReadoutData rd = null;
                    if (!DryMode)
                    {
                        rd = MCA.ReadData(nwaveform);
                    }
                    else
                    {
                        rd = ReadoutData.DummyData(2, 100, nwaveform, 2000, 100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() => UpdateProgress(rd)));
                }
            }
            catch (MCAException e)
            {
                DAQThreadUnhandledExceptionArgs args = new DAQThreadUnhandledExceptionArgs(e);
                this.UnhandledException(args);
                if (!args.Handled) { throw; }
            }
            finally
            {
                try
                {
                    if (!DryMode)
                    {
                        MCA.StopAcquisition();
                        //App.MCA.ClearData();
                        while (MCA.HasData)
                        {
                            ReadoutData rd = MCA.ReadData(nwaveform);
                            Application.Current.Dispatcher.BeginInvoke((Action)(() => UpdateProgress(rd)));
                        }
                    }
                }
                catch (MCAException e) //if clean up fail just log
                {
                    DAQThreadUnhandledExceptionArgs args = new DAQThreadUnhandledExceptionArgs(e);
                    this.UnhandledException(args);
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(OnSuccessfulStop);
                    ready.Set();
                }
            }
        }

    }
}
