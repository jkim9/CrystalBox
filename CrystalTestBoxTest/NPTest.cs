using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CITHEPLib;
namespace CrystalTestBox
{
    [TestClass]
    public class NPTest
    {
        [TestMethod]
        public void TestLinspace()
        {
            double[] x = NP.Linspace(0, 100, 100);
            for (int i = 0; i < x.Length;i++ )
            {
                System.Console.WriteLine(x[i]);
            }
            Assert.AreEqual(x.Length, 100);
            for (int i = 1; i < 100; i++)
            {
                Assert.AreEqual(x[i] - x[i-1], 100/99.0, 1e-7);
            }
            Assert.AreEqual(x[0], 0.0, 1e-7);
            Assert.AreEqual(x[99], 100.0, 1e-7);
        }
        
        [TestMethod]
        public void TestSolve()
        {
            double[,] A = {
                              {1, 2},
                              {3, 4}
                          };
            double[] C = { 5, 11 };
            double[] B = NP.Solve(A, C);
            Assert.AreEqual(B[0], 1.0, 1e-7);
            Assert.AreEqual(B[1], 2.0, 1e-7);
            Assert.AreEqual(B.Length, 2);
        }

        [TestMethod]
        public void TestBroadcast()
        {
            double[] x = { 1, 2, 3 };
            double[] y = { 4, 5, 6 };
            //double z = 7;

            double[] expadd = {5, 7, 9};
            TestArray(NP.Add(x,y), expadd);

            double[] expmul = { 4, 10, 18 };
            TestArray(NP.Multiply(x, y), expmul);

            NP.Func2 f = (double xx, double yy) => xx*xx + yy*yy;
            double[] expbcst = {17, 29, 45};
            TestArray(NP.Broadcast(f, x, y), expbcst);

            double[] exppow = {1, 4, 9};
            TestArray(NP.Pow(x, 2), exppow);

            double[] exp2x = { 2, 4, 6 };
            TestArray(NP.Broadcast((xx) => 2 * xx, x), exp2x);
        }

        [TestMethod]
        public void TestLinearFit()
        {
            double[] x = NP.Linspace(0, 1, 100);
            double[] y = NP.Broadcast((xx) => 2 * xx + 3, x);
            NP.LinearFitResult fit = NP.LinearFit(x, y);
            Assert.AreEqual(fit.m, 2.0, 1e-7);
            Assert.AreEqual(fit.c, 3.0, 1e-7);
        }

        [TestMethod]
        public void TestParabolicFit()
        {
            double[] x = NP.Linspace(1, 2, 100);
            double[] y = NP.Broadcast((xx) => 3 * xx*xx - 2 * xx + 1, x);
            NP.ParabolicFitResult fit = NP.ParabolicFit(x, y);
            Assert.AreEqual(fit.a, 3.0, 1e-7);
            Assert.AreEqual(fit.b, -2.0, 1e-7);
            Assert.AreEqual(fit.c, 1.0, 1e-7);
        }

        [TestMethod]
        public void TestGradient()
        {
            double[] x = { 1, 2, 3, 3, 2, 1 };
            double[] grad = { 1, 1, 0.5, -0.5, -1, -1 };
            TestArray(grad, NP.Gradient(x));

            double[] xx = NP.Linspace(0, 1, 100);
            double[] yy = NP.Broadcast((s) => 2 * s + 33, xx);

            double[] gg = NP.Gradient(xx, yy);
            double[] exp_gg = NP.ConstArray(100, 2);
            TestArray(exp_gg, gg);
        }

        [TestMethod]
        public void TestConcatenate()
        {
            double[,] x = {
                            {1,2,3},
                            {4,5,6},
                            {7,8,9}
                          };
            double[,] y = {
                              {10, 11, 12},
                              {13,14,15}
                          };

            double[,] expcat ={
                                {1,2,3},
                                {4,5,6},
                                {7,8,9},
                                {10, 11, 12},
                                {13,14,15}
                              };
            TestArray2(NP.Concatenate(x, y), expcat);

        }

        [TestMethod]
        public void TestGlobalParabolicSmooth()
        {
            double[] x = NP.Linspace(0, 5, 80);
            double[] y = NP.Broadcast((xx) => 2 * xx * xx + xx - 1, x);
            double[] z = NP.GlobalParabolicSmooth(y);
            Console.WriteLine(y);
            Console.WriteLine("----");
            Console.WriteLine(z);
            TestArray(y, z);

        }

        [TestMethod]
        public void TestSlice()
        {
            double[] x = { 1, 2, 3, 4, 5, 6 };

            double[] exp1 = {2,3,4,5,6};
            TestArray(NP.Slice(x, 1), exp1);

            double[] exp2 = { 3, 4, 5 };
            TestArray(NP.Slice(x, 2, -1), exp2);
            TestArray(NP.Slice(x, 2, 5), exp2);
        }

        public void TestArray(double[] x, double[] y, double eps=1e-7)
        {
            Assert.AreEqual(x.Length, y.Length);
            for (int i = 0; i < x.Length; i++)
            {
                Console.WriteLine(String.Format("{0:d} {1:0.0000000} {2:0.0000000}", i, x[i], y[i]));
                Assert.AreEqual(x[i], y[i], eps);
            }
        }

        public void TestArray2(double[,] x, double[,] y, double eps = 1e-7)
        {
            Assert.AreEqual(x.GetLength(0), y.GetLength(0));
            Assert.AreEqual(x.GetLength(1), y.GetLength(1));
            for (int irow = 0; irow < x.GetLength(0); irow++)
            {
                for(int icol=0; icol<x.GetLength(1); icol++){
                    Assert.AreEqual(x[irow, icol], y[irow, icol], eps);
                }
            }
        }

        [TestMethod]
        public void TestBound()
        {
            double[] x = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.AreEqual(2, NP.LowerBound(x, 2.5));
            Assert.AreEqual(8, NP.UpperBound(x, 7.5));
            Assert.AreEqual(-1, NP.LowerBound(x, -1));
            Assert.AreEqual(10, NP.UpperBound(x, 10));

        }

        [TestMethod]
        public void TestArgMax()
        {
            double[] x = { 0, 1, 2, 3, 4, 50, 6, 7, 8, 9 };
            int argmax = NP.Argmax(x);
            Assert.AreEqual(5, argmax);
        }
    }
}
