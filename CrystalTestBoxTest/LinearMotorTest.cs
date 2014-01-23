using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinearMotorWrapper;
using System.Threading;
namespace CrystalTestBox
{
    [TestClass]
    public class LinearMotorTest
    {
        LinearMotor lin;
        [TestInitialize]
        public void Initialize()
        {
            lin = LinearMotor.GetInstance();
            bool foundone = lin.ScanConnect();
            if (!foundone) Assert.Inconclusive("Can't connect to the linear stage.");
            lin.testmode = true;
            lin.SetMotorPower(1);
        }

        [TestCleanup]
        public void Cleanup()
        {
            lin.Dispose();
        }

        [TestMethod]
        public void TestMethod1()
        {
            lin.SendRecv("HSPD=15000");
            lin.SendRecv("L+");
        }

        [TestMethod]
        public void TestByte2Str()
        {
            byte[] b = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', 0, 0, 0 };
            LinearMotor lin = LinearMotor.GetInstance();
            Assert.AreEqual(lin.byte2str(b), "abcd");
        }

        [TestMethod]
        public void TestStr2Byte()
        {
            LinearMotor lin = LinearMotor.GetInstance();
            byte[] b = lin.str2byte("abcd");
            Assert.AreEqual(b.Length, LinearMotor.COMMAND_LENGTH);
            Assert.AreEqual(b[0], (byte)'a');
            Assert.AreEqual(b[1], (byte)'b');
            Assert.AreEqual(b[2], (byte)'c');
            Assert.AreEqual(b[3], (byte)'d');
            Assert.AreEqual(b[4], 0);
        }

        [TestMethod]
        public void TestIsRunning()
        {
            lin.Abort();
            lin.SetSpeed(50000);
            Assert.IsFalse(lin.IsRunning());
            lin.LimitMinus();
            Thread.Sleep(3000);
            lin.Stop();
            Thread.Sleep(1000);
            lin.LimitPlus();
            
            Thread.Sleep(2000);
            Assert.IsTrue(lin.IsRunning());
        }

        [TestMethod]
        public void TestHome()
        {
            lin.Abort();
            lin.LimitPlus();
            lin.Wait();
            Assert.AreEqual(lin.Position(),0);
        }

        [TestMethod]
        public void TestCalibrate()
        {
            lin.Calibrate();
            Assert.AreEqual(0,lin.Position());
        }

        [TestMethod]
        public void TestMoveTocm()
        {
            lin.Calibrate();
            lin.MoveTocm(-5);
            lin.Wait();
            Assert.AreEqual(lin.Position(), Math.Round(-5*LinearMotor.STEP_PER_CM));
        }

        [TestMethod]
        public void TestPower()
        {
            lin.SetMotorPower(1);
            Thread.Sleep(50);
            Assert.AreEqual(true, lin.MotorPower());
            
            lin.SetMotorPower(0);
            Thread.Sleep(50);
            Assert.AreEqual(false, lin.MotorPower());

            lin.SetMotorPower(1);
            Thread.Sleep(50);
            Assert.AreEqual(true, lin.MotorPower());
        }
    }
}
