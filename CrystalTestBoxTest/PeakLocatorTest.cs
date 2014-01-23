using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CITHEPLib.PeakLocator;
using CITHEPLib;
using Minuit2;
namespace CrystalTestBox
{
    [TestClass]
    public class PeakLocatorTest
    {
        [TestMethod]
        public void TestZeroCrossing()
        {
            double[] x = NP.Linspace(1, -1, 100);
            CrudeZeroCrossing[] crossings = PeakLocator.ZeroCrossings(x, 0.5);
            Assert.AreEqual(crossings.Length, 1);
            Assert.AreEqual(new CrudeZeroCrossing(50, 0, 24), crossings[0]);
        }

        [TestMethod]
        public void TestZeroCrossing2()
        {
            double[] x = NP.Linspace(0, 4*Math.PI, 100);
            double[] y = NP.Broadcast(Math.Sin,x);
            CrudeZeroCrossing[] crossings = PeakLocator.ZeroCrossings(y, 0.5);
            Assert.AreEqual(crossings.Length, 2);
            Assert.AreEqual(new CrudeZeroCrossing(25, 5, 20), crossings[0]);
            Assert.AreEqual(new CrudeZeroCrossing(75, 54, 70), crossings[1]);
        }

        [TestMethod]
        public void TestCachedNormalized()
        {
            PDF linear = (v) =>
            {
                double x = v[0];
                double m = v[1];
                double c = v[2];
                return m * x + c;
            };
            CachedNormalized cnorm = new CachedNormalized(linear, 0, 1);

            F3 normlin = (x, m, c) => (m * x + c) / (m / 2 + c);

            double[] xmc = { 1, 2, 3 };
            Assert.AreEqual(normlin(1, 2, 3), cnorm.Compute(xmc));
        }

        [TestMethod]
        public void TestIntegral()
        {
            PDF linear = (v) =>
            {
                double x = v[0];
                double m = v[1];
                double c = v[2];
                return m * x + c;
            };
            double[] mc = {1, 1};
            double integral = Integral.Integrate1D(linear, 0, 1, 1, mc);
            Assert.AreEqual(1.5, integral);

            integral = Integral.Integrate1D(linear, 0, 1, 100, mc);
            Assert.AreEqual(1.5, integral, 1e-7);

            integral = Integral.Integrate1D(linear, 1, 2, 100, mc);
            Assert.AreEqual(2.5, integral, 1e-7);

            PDF polynomial = (v) =>
            {
                double x = v[0];
                double a = v[1];
                double b = v[2];
                double c = v[3];
                return a * x * x + b * x + c;
            };

            double[] abc = { 1, 2, 3 };
            integral = Integral.Integrate1D(polynomial, 0, 1, 100, abc);
            Assert.AreEqual(1/3.0+2/2.0+3.0, integral, 1e-7);
        }

        [TestMethod]
        public void TestSpectrumCrossing()
        {
            int npieces = 1000;
            double lower = 0;
            double upper = 1;
            double bw = (upper - lower) / npieces;
            double[] x = NP.Linspace(lower, upper, npieces);
            F3 gaussian = (xx, xmu, xsigma) => 1/(xsigma*Math.Sqrt(2*Math.PI)) * Math.Exp(-(xx-xmu)*(xx-xmu)/(2*xsigma*xsigma));
            double nsig = 100000;
            double nbkg = 25000;
            NP.Func1 pdf = (xx) => nsig*bw*gaussian(xx, 0.5, 0.1)+nbkg*bw;
            double[] y = NP.Broadcast(pdf, x);
            //this is not really a spectrum with mu scaling sigma
            //so turn off scaling
            CrudeZeroCrossing[] crossings = PeakLocator.SpectrumCrossing(x, y, scalingpower:0.0);
            //NP.PrintArray(y);
            Assert.AreEqual(1, crossings.Length);
            Assert.AreEqual(npieces/2, crossings[0].Crossing);
            InitialPeakGuess guess = InitialPeakGuess.OfCrudeZeroCrossing(crossings[0], x, y);
            //Console.WriteLine(guess);
            //hmm the slope is off...
            //but it's usable
            
        }

        [TestMethod]
        public void TestChi2()
        {
            double[] x = NP.Arange(0, 10);
            double[] y = NP.Broadcast((xx) => 2 * xx + 3, x);

            PDF linear = (v) => v[0] * v[1] + v[2];
            Chi2 x2 = new Chi2(linear, x, y);

            double[] tmp = { 2, 3 };
            Assert.AreEqual(0.0, x2.Compute(tmp));

            double[] tmp2 = { 2, 4 };
            Assert.AreEqual(10.0, x2.Compute(tmp2));
        }

        [TestMethod]
        public void TestBinChi2()
        {
            int npiece = 1000;
            double bw = 1.0 / npiece;
            double nsig = 10000;
            double[] edges = NP.Linspace(0,1, npiece);
            double[] x = NP.MidPoints(edges);
            double[] y = NP.Broadcast(
                (xx) =>  nsig*bw*GenericPDF.gaussian_flat(xx, 0.5, 0.1), 
                x);
            
            PDF pdf = (v) => {
                    double xx = v[0];
                    double mu = v[1];
                    double sigma = v[2];
                    double N = v[3];
                    return N*GenericPDF.gaussian_flat(xx, mu, sigma);
                };

            double[] arg = { 0.5, 0.1, nsig };
            BinChi2 x2 = new BinChi2(pdf, edges, y);
            double chi2 = x2.Compute(arg);
            Assert.IsTrue(chi2 < 1.0);
        }

        [TestMethod]
        public void TestPeakingPDF()
        {
            double lower = 0.0;
            double upper = 1.0;
            double x = 0.5;
            double mu = 0.5;
            double sigma = 0.1;
            double nsig=10000;
            double m = 0;
            double c = 1;
            double nbkg = 2000;
            PeakingPDF pdf = new PeakingPDF(0, 1);
            double[] arg = { x, mu, sigma, nsig, m, c, nbkg };
            Console.WriteLine(pdf.Compute(arg));
            
            CachedNormalized peak = new CachedNormalized(GenericPDF.gaussian, lower, upper, 10000);
            CachedNormalized bkg = new CachedNormalized(GenericPDF.linear, lower, upper, 10000);

            double expected = nsig * peak.Compute(new double[]{x, mu, sigma}) + nbkg * bkg.Compute(new double[]{x, m, c});
            Assert.AreEqual(expected, pdf.Compute(arg), 1e-7);
            
        }

        [TestMethod]
        public void TestPeakFitting()
        {
            int npieces = 15000;
            double lower = 0; 
            double upper = 1;
            double bw = (upper - lower) / npieces;
            double[] x = NP.Linspace(lower, upper, npieces);
            double[] mp = NP.MidPoints(x);
            F3 gaussian = (xx, xmu, xsigma) => 1 / (xsigma * Math.Sqrt(2 * Math.PI)) * Math.Exp(-(xx - xmu) * (xx - xmu) / (2 * xsigma * xsigma));
            double nsig = 100000;
            double nbkg = 25000;
            NP.Func1 pdf = (xx) => nsig * bw * gaussian(xx, 0.5, 0.1) + nbkg * bw;
            double[] y = NP.Broadcast(pdf, mp);
            //this is not really a spectrum with mu scaling sigma
            //so turn off scaling
            CrudeZeroCrossing[] crossings = PeakLocator.SpectrumCrossing(mp, y, scalingpower: 0.0);
            //NP.PrintArray(y);
            Assert.AreEqual(1, crossings.Length);
            Assert.AreEqual(npieces / 2, crossings[0].Crossing, 2);
            InitialPeakGuess guess = InitialPeakGuess.OfCrudeZeroCrossing(crossings[0], mp, y);
            //Console.WriteLine(guess);
            PeakFitter fitter = PeakFitter.FromGuess(guess, x, y);
            fitter.minuit.migrad();
            Assert.AreEqual(0.5, fitter.minuit.GetValue("mu"), 1e-4);
            Assert.AreEqual(0.1, fitter.minuit.GetValue("sigma"), 1e-4);
            //Console.WriteLine(m.GetValue("nsig"));
            //Console.WriteLine(m.GetValue("mu"));

            //hmm the width is off by a bit shaping
            //but it's usable
        }
    }
}
