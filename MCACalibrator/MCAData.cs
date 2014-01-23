using CAENDigitizerWrapper;
using CITHEPLib;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCACalibrator
{
    [ImplementPropertyChanged]
    public class MCAData
    {
        public IntHistogram Channel0Hist { get; set; }
        public IntHistogram Channel1Hist { get; set; }
        public ObservableCollection<double> Channel0VirtualProbe1 { get; set; }
        public ObservableCollection<double> Channel0VirtualProbe2 { get; set; }
        public ObservableCollection<double> Channel0DigitalProbe1 { get; set; }
        public ObservableCollection<double> Channel0DigitalProbe2 { get; set; }
        public ObservableCollection<double> Channel1VirtualProbe1 { get; set; }
        public ObservableCollection<double> Channel1VirtualProbe2 { get; set; }
        public ObservableCollection<double> Channel1DigitalProbe1 { get; set; }
        public ObservableCollection<double> Channel1DigitalProbe2 { get; set; }

        public MCAData()
        {
            Channel0Hist = new IntHistogram(1 << DT5780.NBIT);
            Channel1Hist = new IntHistogram(1 << DT5780.NBIT);
            Channel0VirtualProbe1 = new ObservableCollection<double>();
            Channel0VirtualProbe2 = new ObservableCollection<double>();
            Channel0DigitalProbe1 = new ObservableCollection<double>();
            Channel0DigitalProbe2 = new ObservableCollection<double>();
            Channel1VirtualProbe1 = new ObservableCollection<double>();
            Channel1VirtualProbe2 = new ObservableCollection<double>();
            Channel1DigitalProbe1 = new ObservableCollection<double>();
            Channel1DigitalProbe2 = new ObservableCollection<double>();
        }


        public void AddHist(ReadoutData rd)
        {
            Channel0Hist.Add(rd.Channel[0].Energies());
            Channel1Hist.Add(rd.Channel[1].Energies());
        }

        private ObservableCollection<double> CopyUShortArray(ushort[] org)
        {
            ObservableCollection<double> ret = new ObservableCollection<double>();
            for (int i = 0; i < org.Length; i++)
            {
                ret.Add((double)org[i]);
            }
            return ret;

        }

        private ObservableCollection<double> CopyByteArray(byte[] org)
        {
            ObservableCollection<double> ret = new ObservableCollection<double>();
            for (int i = 0; i < org.Length; i++)
            {
                ret.Add((double)org[i]);
            }
            return ret;

        }
        public void SetWaveform(ReadoutData rd)
        {

            if (rd.Channel[0].Event.Length > 0 && rd.Channel[0].Event[0].Waveform != null)
            {
                
                Channel0DigitalProbe1 = CopyByteArray(rd.Channel[0].Event[0].Waveform.DTrace1);
                Channel0DigitalProbe2 = CopyByteArray(rd.Channel[0].Event[0].Waveform.DTrace2);
                Channel0VirtualProbe1 = CopyUShortArray(rd.Channel[0].Event[0].Waveform.Trace1);
                Channel0VirtualProbe2 = CopyUShortArray(rd.Channel[0].Event[0].Waveform.Trace2);
            }
            if (rd.Channel[1].Event.Length > 0 && rd.Channel[1].Event[0].Waveform != null)
            {
                Channel1DigitalProbe1 = CopyByteArray(rd.Channel[1].Event[0].Waveform.DTrace1);
                Channel1DigitalProbe2 = CopyByteArray(rd.Channel[1].Event[0].Waveform.DTrace2);
                Channel1VirtualProbe1 = CopyUShortArray(rd.Channel[1].Event[0].Waveform.Trace1);
                Channel1VirtualProbe2 = CopyUShortArray(rd.Channel[1].Event[0].Waveform.Trace2);
            }
        }
        
        public void Clear()
        {
            Channel0Hist.Reset();
            Channel1Hist.Reset();
            Channel0VirtualProbe1 = new ObservableCollection<double>();
            Channel0VirtualProbe2 = new ObservableCollection<double>();
            Channel0DigitalProbe1 = new ObservableCollection<double>();
            Channel0DigitalProbe2 = new ObservableCollection<double>();
            Channel1VirtualProbe1 = new ObservableCollection<double>();
            Channel1VirtualProbe2 = new ObservableCollection<double>();
            Channel1DigitalProbe1 = new ObservableCollection<double>();
            Channel1DigitalProbe2 = new ObservableCollection<double>();
        }
    }
}