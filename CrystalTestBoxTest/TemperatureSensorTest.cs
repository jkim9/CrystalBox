using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemperatureSensor;
namespace CrystalTestBox
{
    [TestClass]
    public class TemperatureSensorTest
    {
        [TestMethod]
        public void ConnectTest()
        {
            DigitalThermometer sensor = new DigitalThermometer();
            bool foundone = sensor.ScanConnect();
            if(!foundone){
               Assert.Inconclusive("Cannot Connect to Digital Thermometer");
            }
            TemperatureData data = sensor.Read();
            Console.WriteLine(data);
        }
    }
}
