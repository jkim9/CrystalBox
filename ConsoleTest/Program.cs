using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrystalTestBox;
using MCACalibrator;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ShapingSetting s = ShapingSetting.CAENA1422Default();
            XmlSerializer ser = new XmlSerializer(s.GetType());
            StringWriter o = new StringWriter();
            ser.Serialize(o, s);
            Debug.WriteLine(o);
            Console.ReadKey();

        }
    }
}
