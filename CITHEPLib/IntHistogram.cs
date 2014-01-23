using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using System.Diagnostics;
using System.Collections.Specialized;

namespace CITHEPLib
{
    [Serializable]
    public class IntHistogram : Bindable, INotifyCollectionChanged
    {
        public int[] Channel { get; set; }
        public int Overflow { get; set; }
        public int Underflow { get; set; }
        public int NumChannel { get; set; }
        private Object histlock = new Object();
        public int min { get; set; }
        public int max { get; set; }
        public int NSample { get; set; }
        public bool RejectZero { get; set; }
        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };

        public IntHistogram() { Channel = new int[0]; }
        public IntHistogram(int max = 10, int min = 0)
        {
            //histogram for left inclusive [min,max)
            this.min = min;
            this.max = max;
            NumChannel = max - min;
            RejectZero = true;
            Channel = new int[NumChannel];
            ClearChannel();
            Reset();
        }

        private void Add_nolock(int channel)
        {
            if (RejectZero && channel == 0) return;
            if (channel >= max)
            {
                Overflow++;
            }
            else if (channel >= min)
            {
                this.Channel[channel - min]++;
            }
            else
            {
                Underflow++;
            }
            //don't switch these two lines
            NSample++;
            Average += (channel - Average) / NSample;
        }

        private void NotifyChanges()
        {
            OnPropertyChanged("Channel");
            OnPropertyChanged("Series");
            OnPropertyChanged("NSample");
            OnPropertyChanged("Average");
            OnPropertyChanged("ScannedFwhmSD");
            OnPropertyChanged("MaxPosition");
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void Add(int channel)
        {
            lock (histlock)
            {
                Add_nolock(channel);
            }

        }

        public void Add(IEnumerable<int> channels)
        {
            lock (histlock)
            {
                foreach (int c in channels)
                {
                    Add_nolock(c);
                }
            }
            NotifyChanges();
        }


        [System.Xml.Serialization.XmlIgnore]
        public int NBins
        {
            get { return Channel.Length; }
        }

        [System.Xml.Serialization.XmlIgnore]
        public double Average
        {
            get;
            private set;
        }

        private void ClearChannel()
        {
            for (int i = 0; i < Channel.Length; i++)
            {
                Channel[i] = 0;
            }
        }

        public void Reset()
        {
            lock (histlock)
            {
                ClearChannel();
                Overflow = 0;
                Underflow = 0;
                NSample = 0;
                Average = 0;
                NotifyChanges();
            }
        }


        public double[] ToDoubleArray()
        {
            double[] ret = new double[Channel.Length];
            for (int i = 0; i < Channel.Length; i++)
            {
                ret[i] = Channel[i];
            }
            return ret;
        }
        /// <summary>
        ///  Poor man scan fwhm always under estimate
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public double ScannedFwhmSD
        {
            get
            {
                int maxIndex = MaxPosition;
                double halfmax = Channel[maxIndex] / 2.0;
                int rightIndex = Array.FindIndex(Channel, maxIndex, Channel.Length - maxIndex, (x) => (x < halfmax));
                int leftIndex = Array.FindLastIndex(Channel, 0, maxIndex + 1, (x) => (x < halfmax));
                if (rightIndex < 0 || leftIndex < 0) return 0;
                return (rightIndex - leftIndex) / 2.35;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public int MaxPosition
        {
            get
            {
                return Array.IndexOf(Channel, Channel.Max());
            }
        }
    }

}
