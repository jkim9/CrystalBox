using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace TemperatureSensor
{
    public class TemperatureSensorException : Exception
    {
        public TemperatureSensorException(string msg) : base(msg) { }
    }

    public class TemperatureData
    {
        public TemperatureData(string reply)
        {
            //Parsing "T1,H1,T2,H2
            string[] tokens = reply.Split(',');
            Temperature1 = 0;
            Temperature2 = 0;
            Humidity1 = 0;
            Humidity2 = 0;
            ValidData = false;
            DateTime TimeStamp = DateTime.Now;
            if (tokens.Length != 4) return; // throw new TemperatureSensorException("Unable to parse the reply: " + reply);
            try
            {
                Temperature1 = Double.Parse(tokens[0]);
                Humidity1 = Double.Parse(tokens[1]);
                Temperature2 = Double.Parse(tokens[2]);
                Humidity2 = Double.Parse(tokens[3]);
                ValidData = true;
            }
            catch (FormatException)
            {
                //do nothing
            }
        }
        public bool ValidData { get; private set; }
        public long TimeStamp { get; private set; }
        public double Temperature1 { get; private set; }
        public double Temperature2 { get; private set; }
        public double Humidity1 { get; private set; }
        public double Humidity2 { get; private set; }
        public override string ToString()
        {
            return String.Format("T1={0:##.##}, H1={1:##.##}, T2={2:##.##}, H2={3:##.##}", Temperature1, Humidity1, Temperature2, Humidity2);
        }
    }

    public class DigitalThermometer : INotifyPropertyChanged
    {
        private System.IO.Ports.SerialPort serial;
        private BackgroundWorker poller;

        public DigitalThermometer()
        {
            serial = null;
            poller = new BackgroundWorker();
            poller.DoWork += (a, b) =>
            {
                this._data = this.Read();
                PropertyChangedEventArgs o = new PropertyChangedEventArgs("Data");
                this.PropertyChanged(this, o);
                Thread.Sleep(2000);
            };
        }

        ~DigitalThermometer()
        {
            if(serial!=null) serial.Close();
        }

        public bool IsConnected()
        {
            if (serial == null) return false;
            return serial.IsOpen;
        }

        public bool ScanConnect()
        {
            //try connect to serial port then ping 'C" and check if
            //the reply is "Temp Sensor"
            List<string> ports = new List<string>() { "COM1", "COM2", "COM3", "COM4" };
            bool success = false;
            foreach (string port in ports)
            {
                //Debug.WriteLine("Trying: " + port);
                SerialPort tmp = new SerialPort(port, 9600);
                tmp.ReadTimeout = 3000;
                try
                {
                    tmp.Open();
                }
                catch (IOException)
                {
                    success = false;
                    continue;
                }
                catch (InvalidOperationException)
                {
                    success = false;
                    continue;
                }
                //now try to ping and make sure it gives back the right status
                tmp.Write("C");
                try
                {
                    string reply = tmp.ReadLine();
                    if (reply == "Temp Sensor")
                    {
                        success = true;
                        serial = tmp;
                        break;
                    }
                    else
                    {
                        success = false;
                    }
                }
                catch (IOException)
                {
                    success = false;
                }

            }
            return success;
        }

        public TemperatureData Read(bool shouldthrow = false)
        {
            string reply = "";
            try
            {
                serial.Write("D");
                reply = serial.ReadLine();
            }
            catch (IOException)
            {
                if (shouldthrow)
                {
                    throw new TemperatureSensorException("Read fail");
                }
            }
            return new TemperatureData(reply);
        }

        private TemperatureData _data;
        public TemperatureData Data
        {
            get { return _data; }
        }

        public void StartDataPoller()
        {
            if (!poller.IsBusy)
            {
                poller.RunWorkerAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = (o,e) => { };
    }
}
