using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CITHEPLib
{
    public class GenericToString
    {
        public static string ToString<T>(T o)
        {
            PropertyInfo[] pinfo = o.GetType().GetProperties();
            StringWriter ret = new StringWriter();
            ret.WriteLine("{0} {{ ", o.GetType().Name);
            foreach (PropertyInfo p in pinfo)
            {
                ret.WriteLine("- {0}: {1},", p.Name, p.GetValue(o));
            }
            ret.Write("}");
            return ret.ToString();
        }
    }
}
