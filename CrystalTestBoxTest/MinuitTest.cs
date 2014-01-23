using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minuit2;
namespace CrystalTestBox
{
    [TestClass]
    public class MinuitTest
    {
        [TestMethod]
        public void SimpleTest()
        {
            FCN f = (v)=>{
                double x = v[0];
                double y = v[1];
                double z = v[2];
                return (x-1)*(x-1) + (y-2)*(y-2) + (z-3)*(z-3);
            };
            String[] pname = {"x", "y", "z"};
            Minuit m = new Minuit(f, pname);
            m.migrad();
            Assert.AreEqual(1.0, m.GetValue(0), 1e-7);
            Assert.AreEqual(2.0, m.GetValue(1), 1e-7);
            Assert.AreEqual(3.0, m.GetValue(2), 1e-7);
        }
    }
}
