using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using CAENDigitizerWrapper;

namespace CrystalTestBox
{
    [TestClass]
    public class CAENDigitizerWrapperTest
    {
        DT5780 mca;
        [TestInitialize]
        public void Initialize()
        {
            Console.WriteLine(DateTime.UtcNow);
            mca = new DT5780();
            bool foundone = mca.ScanConnect();
            if (!foundone) Assert.Inconclusive("DT5780 Not Found");
            mca.Reset();
            mca.UseDefaultBoardSetting();
        }

        [TestCleanup]
        public void Cleanup()
        {
            mca.Dispose();
        }

        [TestMethod]
        public void TestReadWrite()
        {
            mca.WriteRegister(CAENDPP.DUMMY, 0xDEADBEEF);
            UInt32 val = mca.ReadRegister(CAENDPP.DUMMY);
            Assert.AreEqual(val, 0xDEADBEEF, "read");    
        }

        [TestMethod]
        public void TestReset()
        {
            mca.WriteRegister(CAENDPP.DUMMY, 0x12345678);
            mca.Reset();
            UInt32 val = mca.ReadRegister(CAENDPP.DUMMY);
            Assert.AreNotEqual(val, 0x12345678, "read");
        }

        [TestMethod]
        public void TestGetBoardStatus()
        {
            BoardStatus bs = mca.GetBoardStatus();
            Console.WriteLine(bs);
            //Assert.AreEqual<UInt32>(bs.CONFIG, 0x10);
        }

        [TestMethod]
        public void TestGetChannelStatus()
        {
            ChannelStatus cs = mca.GetChannelStatus(0);
            Console.WriteLine(cs);
            //Assert.AreEqual<UInt32>(cs.MISC, 0x0);
        }

        [TestMethod]
        public void TestReadOut()
        {
            //mca.UseDefaultChannelSetting(0);
            mca.UseDefaultBoardSetting();

            Console.WriteLine(mca);
            mca.StartAcquisition();
            Thread.Sleep(500);
            ReadoutData rd = mca.ReadData();
            if (rd.Channel[0].Event.Length == 0) { Assert.Inconclusive("Never Get Triggered. Is it connected to pulse generator?"); }
            Assert.AreNotEqual(rd.Channel[0].Event[0].Energy, 0);
            Console.WriteLine(rd);
            mca.StopAcquisition();
            mca.Reset();
        }

        [TestMethod, Timeout(2000)]
        public void TestReadOutWithWrite()
        {
            //mca.UseDefaultChannelSetting(0);
            mca.UseDefaultBoardSetting();

            Console.WriteLine(mca);
            mca.StartAcquisition();
            Thread.Sleep(500);
            try
            {
                mca.set_TriggerThreshold(0, 100);
                ReadoutData rd = mca.ReadData();
                Assert.AreNotEqual(rd.Channel[0].Event[0].Energy, 0);
                Console.WriteLine(rd);
            }
            catch(MCAException e){
                Assert.AreEqual(e.ErrorCode , MCAException.ErrorCodeType.WriteDeviceRegisterFail);
            }
            mca.StopAcquisition();
            mca.Reset();
        }
    }
}
