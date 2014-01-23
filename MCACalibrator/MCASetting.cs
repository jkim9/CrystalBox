using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using CITHEPLib;
using CAENDigitizerWrapper;
using System.Xml.Serialization;
using System.IO;

namespace MCACalibrator
{
    [ImplementPropertyChanged]
    [Serializable]
    public class ShapingSetting
    {
        //The property used here matches the DPPPHA interface
        public DT5780.PulsePolarityType Polarity { get; set; }
        public double DecayTime { get; set; }
        public double InputRiseTime { get; set; }
        public double TrapezoidRiseTime { get; set; }

        public double FlatTop { get; set; }
        public DT5780.BaselineAveragingWindowType BaselineMean { get; set; }
        public UInt32 TrapezoidGain { get; set; }
        public double PeakingDelay { get; set; }
        public DT5780.PeakAveragingWindowType PeakMean { get; set; }
        public double BaselineHoldoff { get; set; }
        public double PeakHoldoff { get; set; }
        public ShapingSetting()
        {
            Polarity = DT5780.PulsePolarityType.Positive;
            DecayTime = 100;
            InputRiseTime = 0.01;
            TrapezoidRiseTime = 0.1;
            FlatTop = 0.5;
            BaselineMean = DT5780.BaselineAveragingWindowType.Sample16;
            TrapezoidGain = 10;
            PeakingDelay = 0.3;
            PeakMean = DT5780.PeakAveragingWindowType.Sample4;
            BaselineHoldoff = 0.0;
            PeakHoldoff = 0.0;
        }
        public void UseSettingOn(DT5780 mca, int channel)
        {
            mca.set_PulsePolarity(channel, Polarity);
            mca.set_DecayTime(channel, DecayTime);
            mca.set_InputRiseTime(channel, DecayTime);
            mca.set_TrapezoidRiseTime(channel, TrapezoidRiseTime);
            mca.set_TrapezoidFlatTop(channel, FlatTop);
            mca.set_BaselineAveragingWindow(channel, BaselineMean);
            mca.SetTrapezoidGain(channel, TrapezoidGain);
            mca.set_PeakingTime(channel, PeakingDelay);
            mca.set_PeakAveragingWindow(channel, PeakMean);
            mca.set_BaselineHoldOff(channel, BaselineHoldoff);
            mca.set_PeakHoldOffExtension(channel, PeakHoldoff);
        }

        public static ShapingSetting CAENA1422Default()
        {
            return new ShapingSetting()
            {
                Polarity = DT5780.PulsePolarityType.Positive,
                DecayTime = 100,
                InputRiseTime = 0.01,
                TrapezoidRiseTime = 0.1,
                FlatTop = 0.5,
                BaselineMean = DT5780.BaselineAveragingWindowType.Sample16,
                TrapezoidGain = 10,
                PeakingDelay = 0.3,
                PeakMean = DT5780.PeakAveragingWindowType.Sample4,
                BaselineHoldoff = 0.1,
                PeakHoldoff = 0.0
            };
        }

        public override string ToString()
        {
            return GenericToString.ToString(this);
        }
    }

    [ImplementPropertyChanged]
    [Serializable]
    public class TriggerSetting
    {
        public DT5780.InputRange InputRange { get; set; }
        public int TriggerLevel { get; set; }
        public uint SmoothingFactor { get; set; }
        //public double TriggerDelay { get; set; }
        public double TriggerHoldoff { get; set; }
        //public double RTDiscriminationWindow { get; set; }
        public DT5780.TriggerMode SelfTrigger { get; set; }
        public TriggerSetting()
        {
            TriggerLevel = 20;
            SmoothingFactor = 4;
            TriggerHoldoff = 0;
            SelfTrigger = DT5780.TriggerMode.AcqOnly;
        }
        public void UseSettingOn(DT5780 mca, int channel)
        {
            mca.set_TriggerThreshold(channel, TriggerLevel);
            mca.set_TriggerFilterSmoothing(channel, SmoothingFactor);
            mca.set_PreTrg(channel, 0);
            mca.set_TriggerHoldoff(channel, TriggerHoldoff);
            mca.SetChannelSelfTriggerMode(channel, SelfTrigger);
            mca.SetInputRange(channel, InputRange);
        }
        public static TriggerSetting DefaultSetting()
        {
            return new TriggerSetting()
            {
                InputRange = DT5780.InputRange.VPP95,
                TriggerLevel = 30,
                SmoothingFactor = 4,
                TriggerHoldoff = 0
            };
        }

        public override string ToString()
        {
            return GenericToString.ToString(this);
        }
    }

    [ImplementPropertyChanged]
    [Serializable]
    public class HVSetting
    {
        public double VSet { get; set; }
        public double ISet { get; set; }
        public double VMax { get; set; }

        public void UseSettingOnMCA(DT5780 mca, int channel)
        {
            mca.set_HVVMax(channel, VMax);
            mca.set_HVVSet(channel, VSet);
            mca.set_HVISet(channel, ISet);
        }

        public static HVSetting HamamatsuS8664()
        {
            return new HVSetting()
            {
                VSet = 450,
                ISet = 50,
                VMax = 500
            };
        }

        public override string ToString()
        {
            return GenericToString.ToString(this);
        }
    }

    [ImplementPropertyChanged]
    [Serializable]
    public class ProbeSetting
    {
        public enum ProbeMode { Oscilloscope, Histogram }
        public ProbeMode Mode { get; set; }
        public DT5780.AcquisitionModeType AcquisitionMode { get; set; }
        public DT5780.VirtualProbeMode VirtualProbeMode { get; set; }
        public DT5780.VirtualProbe1Mode VirtualProbe1Mode { get; set; }
        public DT5780.VirtualProbe2Mode VirtualProbe2Mode { get; set; }
        public DT5780.DigitalProbeMode DigitalProbeMode { get; set; }
        public DT5780.TriggerMode SWTrigger { get; set; }
        public DT5780.TriggerMode ExtTriggerInput { get; set; }
        public uint RecordLength { get; set; }
        public uint PreTrigger { get; set; }

        public ProbeSetting()
        {
            Mode = ProbeMode.Histogram;
            AcquisitionMode = DT5780.AcquisitionModeType.SoftwareControl;
            VirtualProbeMode = DT5780.VirtualProbeMode.Dual;
            VirtualProbe1Mode = DT5780.VirtualProbe1Mode.Trapezoid;
            VirtualProbe2Mode = DT5780.VirtualProbe2Mode.Input;
            DigitalProbeMode = DT5780.DigitalProbeMode.Peaking;
            SWTrigger = DT5780.TriggerMode.AcqOnly;
            ExtTriggerInput = DT5780.TriggerMode.AcqOnly;
            RecordLength = 1000;
            PreTrigger = 200;
        }

        public void UseSettingOn(DT5780 mca)
        {
            mca.SetChannelEnableMask(0x3);
            
            if (Mode == ProbeMode.Histogram)
            {
                mca.SetDPPAcquisitionMode(DT5780.DPPAcqMode.List, DT5780.DPPSaveParam.EnergyAndTime);
            }
            else if (Mode == ProbeMode.Oscilloscope)
            {
                mca.SetDPPAcquisitionMode(DT5780.DPPAcqMode.Mixed, DT5780.DPPSaveParam.EnergyAndTime);
            }
            mca.SetVirtualProbe(VirtualProbeMode,
                                VirtualProbe1Mode,
                                VirtualProbe2Mode,
                                DigitalProbeMode);
            mca.RecordLength = RecordLength;
            mca.set_PreTrg(0, PreTrigger);
            mca.set_PreTrg(1, PreTrigger);
            mca.SetSWTriggerMode(SWTrigger);
            mca.SetExtTriggerInputMode(ExtTriggerInput);
            
            mca.AcquisitionMode = DT5780.AcquisitionModeType.SoftwareControl;
        }

        public static ProbeSetting HistogramProbe()
        {
            return new ProbeSetting()
            {
                Mode = ProbeMode.Histogram,
                AcquisitionMode = DT5780.AcquisitionModeType.SoftwareControl
            };
        }

        public override string ToString()
        {
            return GenericToString.ToString(this);
        }
    }

    [ImplementPropertyChanged]
    [Serializable]
    public class MCASetting//this might need to be in c++
    {
        public string Name { get; set; }
        public HVSetting HV0 { get; set; }
        public HVSetting HV1 { get; set; }
        public ProbeSetting ProbeSetting { get; set; }
        public TriggerSetting TriggerSetting0 { get; set; }
        public ShapingSetting ShapingSetting0 { get; set; }
        public TriggerSetting TriggerSetting1 { get; set; }
        public ShapingSetting ShapingSetting1 { get; set; }
        //private double DefaultISet = 10;
        //private double DefaultVMax = 500;

        public MCASetting()
        {
            Init("Default Name");
        }


        public void Init(string name="")
        {
            Name = name;
            HV0 = HVSetting.HamamatsuS8664();
            HV1 = HVSetting.HamamatsuS8664();
            ProbeSetting = ProbeSetting.HistogramProbe();
            TriggerSetting0 = TriggerSetting.DefaultSetting();
            TriggerSetting1 = TriggerSetting.DefaultSetting();
            ShapingSetting0 = ShapingSetting.CAENA1422Default();
            ShapingSetting1 = ShapingSetting.CAENA1422Default();
        }

        public MCASetting(string name)
        {
            Init(name);
        }

        public static MCASetting LargeLYSOSetting()
        {
            return new MCASetting("LongLYSO")
            {
                HV0 = HVSetting.HamamatsuS8664(),
                HV1 = HVSetting.HamamatsuS8664(),
                ProbeSetting = ProbeSetting.HistogramProbe(),
                TriggerSetting0 = TriggerSetting.DefaultSetting(),
                TriggerSetting1 = TriggerSetting.DefaultSetting(),
                ShapingSetting0 = ShapingSetting.CAENA1422Default(),
                ShapingSetting1 = ShapingSetting.CAENA1422Default()
            };
        }

        public void UseSettingOnMCA(DT5780 mca)
        {
            mca.Reset();
            mca.SetChannelEnableMask(0x3);
            HV0.UseSettingOnMCA(mca, 0);
            HV1.UseSettingOnMCA(mca, 1);
            TriggerSetting0.UseSettingOn(mca, 0);
            TriggerSetting1.UseSettingOn(mca, 1);
            ShapingSetting0.UseSettingOn(mca, 0);
            ShapingSetting1.UseSettingOn(mca, 1);
            ProbeSetting.UseSettingOn(mca);
        }

        public override string ToString()
        {
            return GenericToString.ToString(this);
        }

        public string XMLString()
        {
            Type thistype = this.GetType();
            XmlSerializer ser = new XmlSerializer(thistype);
            StringWriter writer = new StringWriter();
            ser.Serialize(writer, this);
            writer.Close();
            return writer.ToString();
        }

    }

}
