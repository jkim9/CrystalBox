using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAENDigitizerWrapper;
namespace MCACalibrator
{
    //this should be static but will screwed up designerview
    public class EnumProvider
    {
        public static List<DT5780.BaselineAveragingWindowType> BaselineAveragingWindowType { get; private set; }
        public static List<DT5780.PeakAveragingWindowType> PeakAveragingWindowType { get; private set; }
        public static List<DT5780.PulsePolarityType> PulsePolarityType { get; private set; }

        static EnumProvider()
        {
            BaselineAveragingWindowType = ((DT5780.BaselineAveragingWindowType[])Enum.GetValues(typeof(DT5780.BaselineAveragingWindowType))).ToList();
            PeakAveragingWindowType = ((DT5780.PeakAveragingWindowType[])Enum.GetValues(typeof(DT5780.PeakAveragingWindowType))).ToList();
            PulsePolarityType = ((DT5780.PulsePolarityType[])Enum.GetValues(typeof(DT5780.PulsePolarityType))).ToList();
        }
        public EnumProvider() { }
    }
    //public class EnumProvider //workaround visual studio not copy dll to shadow cache
    //{
    //    public List<DT5780.BaselineAveragingWindowType> BaselineAveragingWindowType { get; private set; }
    //    public List<DT5780.PeakAveragingWindowType> PeakAveragingWindowType { get; private set; }
    //    public List<DT5780.PulsePolarityType> PulsePolarityType { get; private set; }

    //    public EnumProvider()
    //    {
    //        BaselineAveragingWindowType = ((DT5780.BaselineAveragingWindowType[])Enum.GetValues(typeof(DT5780.BaselineAveragingWindowType))).ToList();
    //        PeakAveragingWindowType = ((DT5780.PeakAveragingWindowType[])Enum.GetValues(typeof(DT5780.PeakAveragingWindowType))).ToList();
    //        PulsePolarityType = ((DT5780.PulsePolarityType[])Enum.GetValues(typeof(DT5780.PulsePolarityType))).ToList();
    //    }
    //}
}