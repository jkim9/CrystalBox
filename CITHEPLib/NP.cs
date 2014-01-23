using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using System.Diagnostics;
namespace CITHEPLib
{
    //numpy-like interface
    public class NP
    {
        public static double[] Linspace(double low, double high, int length)
        {
            double[] ret = new double[length];
            double step = (high-low) / (length - 1);
            for (int i = 0; i < length; i++)
            {
                ret[i] = low + step * i;
            }
            ret[length - 1] = high;
            return ret;
        }

        public static double[] MidPoints(double[] x)
        {
            double[] ret = new double[x.Length-1];
            for(int i=0; i<ret.Length; i++){
                ret[i] = (x[i]+x[i+1])/2.0;
            }
            return ret;
        }

        /// <summary>
        /// Create an array of double from [low to high).
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static double[] Arange(int low, int high)
        {
            if (!(low < high)) { throw new ArgumentException("low must be less than high"); }
            double[] ret = new double[high- low];
            for (int i = low; i < high; i++)
            {
                ret[i - low] = i;
            }
            return ret;
        }


        public static double[] Randn(int n, double mu = 0.0, double sigma = 1.0)
        {
            Normal d = new Normal(mu, sigma);
            double[] ret = d.Samples().Take(n).ToArray();
            return ret;
        }


        public static HistogramResult Histogram(double[] data, int nbin, double lower, double upper)
        {
            HistogramResult ret = new HistogramResult(nbin, lower, upper);
            double bw = (upper-lower) / nbin;
            foreach (double x in data)
            {
                if (x < lower) { ret.underflow += 1; continue; }
                if (x > upper) { ret.overflow += 1; continue; }
                int bin = (int)Math.Floor((x - lower) / bw);
                ret.hist[bin] += 1;
            }
            return ret;
        }


        public static double[] Solve(double[,] A, double[] C)
        {
            Matrix<double> AA = DenseMatrix.OfArray(A);
            Vector<double> CC = new DenseVector(C);
            return Solve(AA, CC);
        }

        public static double[] Solve(Matrix<double> A, Vector<double> C)
        {
            Vector<double> B = A.QR().Solve(C);
            return B.ToArray();
        }

        public class LinearFitResult{
           public double m;
           public double c;
        }

        public static LinearFitResult LinearFit(double[]x, double[] y, double[] w=null){
            Debug.Assert(x.Length == y.Length);
            w = w==null?NP.Ones(y.Length):w;
            
            double[] xw = Multiply(x, w);
            double[] yxw = Multiply(y, xw);
            double[] x2w = Multiply(x, xw);
            double[] yw = Multiply(y, w);

            double A = Sum(yxw);
            double B = Sum(x2w);
            double D = Sum(w);
            double E = Sum(yw);
            double F = Sum(xw);

            double m = (A * D - E * F) / (B * D - F * F);
            double c = (E - m * F) / D;

            return new LinearFitResult() { m = m, c = c };
        }

        public static double[] ConstArray(int n, double value)
        {
            double[] ret = new double[n];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = value;
            }
            return ret;
        }

        public static double[] Ones(int n)
        {
            return ConstArray(n, 1.0);
        }

        public static double[] Zeroes(int n)
        {
            return ConstArray(n, 0.0);
        }

        public static double[,] Zeroes(int nrow, int ncol)
        {
            double[,] ret = new double[nrow, ncol];
            for (int irow = 0; irow < nrow; irow++)
            {
                for (int icol = 0; icol < ncol; icol++)
                {
                    ret[irow, icol] = 0.0;
                }
            }
            return ret;
        }

        public static double Sum(double[] x)
        {
            return x.Sum();
        }

        public static double[] Multiply(double[] x, double[] y){
            Debug.Assert(x.Length == y.Length);
            double[] ret = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                ret[i] = x[i] * y[i];
            }
            return ret;
        }

        public static double[] Multiply(double x, double[] y){
            double[] ret = new double[y.Length];
            for(int i=0;i<y.Length;i++){
                ret[i] = x*y[i];
            }
            return ret;
        }

        public static double[] Multiply(double[] x, double y)
        {
            return Multiply(y, x);
        }

        public static double[,] Multiply(double[,] x, double[,] y)
        {
            Debug.Assert(x.GetLength(0) == y.GetLength(0));
            Debug.Assert(x.GetLength(1) == y.GetLength(1));
            double[,] ret = new double[x.GetLength(0), x.GetLength(1)];
            for (int irow = 0; irow < x.GetLength(0); irow++)
            {
                for (int icol = 0; icol < x.GetLength(1); icol++)
                {
                    ret[irow, icol] = x[irow, icol] * y[irow, icol];
                }
            }
            return ret;
        }


        public static double[] Add(double[] x, double[] y)
        {
            Debug.Assert(x.Length == y.Length);
            double[] ret = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                ret[i] = x[i] + y[i];
            }
            return ret;
        }

        public static double[] Add(double[] x, double y)
        {
            double[] ret = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                ret[i] = x[i] + y;
            }
            return ret;
        }

        public static void Accumulate(double[,] x, double[,] y){
            int nrow = x.GetLength(0);
            int ncol = x.GetLength(1);
            Debug.Assert(nrow == y.GetLength(0));
            Debug.Assert(ncol == y.GetLength(1));
            for(int irow=0; irow<nrow; irow++){
                for(int icol=0; icol<ncol; icol++){
                    x[irow, icol] += y[irow, icol];
                }
            }
        }

        public static double[] Add(double x, double[] y)
        {
            return Add(y, x);
        }

        public static double[] Pow(double[] x, double a)
        {
            double[] ret = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                ret[i] = Math.Pow(x[i], a);
            }
            return ret;
        }

        public delegate double Func2(double x, double y);
        public delegate double Func1(double x);

        public static double[] Broadcast(Func1 f, double[] x)
        {
            double[] ret = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                ret[i] = f(x[i]);
            }
            return ret;
        }

        public static double[] Broadcast(Func2 f, double[] x, double[] y)
        {
            double[] ret = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                ret[i] = f(x[i], y[i]);
            }
            return ret;
        }

        public static double[] Broadcast(Func2 f, double[] x, double y)
        {
            double[] ret = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                ret[i] = f(x[i], y);
            }
            return ret;
        }

        public static double[] Broadcast(Func2 f, double x, double[] y)
        {
            double[] ret = new double[y.Length];
            for (int i = 0; i < y.Length; i++)
            {
                ret[i] = f(x, y[i]);
            }
            return ret;
        }

        public class ParabolicFitResult{
            public double a;
            public double b;
            public double c;
        }

        public static ParabolicFitResult ParabolicFit(double[] x, double[] y, double[] w = null)
        {
            Debug.Assert(x.Length == y.Length);
            w = w == null ? NP.Ones(x.Length) : w;
            
            double[] wx = Multiply(w, x);
            double[] wx2 = Multiply(wx, x);
            double[] wx3 = Multiply(wx2, x);
            double[] wx4 = Multiply(wx3, x);

            double alpha = NP.Sum(Multiply(y, wx2));
            double beta = NP.Sum(Multiply(y, wx));
            double theta = NP.Sum(Multiply(y, w));

            double k4 = Sum(wx4);
            double k3 = Sum(wx3);
            double k2 = Sum(wx2);
            double k1 = Sum(wx);
            double k0 = Sum(w);

            double den = -k0 * k2 * k4 + k0 * k3 * k3 + k1 * k1 * k4 - 2 * k1 * k2 * k3 + k2 * k2 * k2;
            double a = alpha * (k0 * k2 - k1 * k1) + beta * (k1 * k2 - k0 * k3) + theta*(k1 * k3 - k2 * k2);
            a = -a / den;

            double b = alpha * (k1 * k2 - k0 * k3) + beta * (k0 * k4 - k2 * k2) + theta * (k2 * k3 - k1 * k4);
            b = -b / den;

            double c = alpha * (k1 * k3 - k2 * k2) + beta * (k2 * k3 - k1 * k4) + theta * (k2 * k4 - k3 * k3);
            c = -c / den;

            return new ParabolicFitResult() { a = a, b = b, c = c };
        }

        public static double[,] RowMultiply(double[,] x, double[] y)
        {
            Debug.Assert(x.GetLength(1) == y.Length);
            double[,] ret = new double[x.GetLength(0), x.GetLength(1)];
            for (int irow = 0; irow < x.GetLength(0); irow++)
            {
                for (int icol = 0; icol < y.Length; icol++)
                {
                    ret[irow, icol] = x[irow, icol] * y[icol];
                }
            }
            return ret;
        }

        public static double[] GlobalParabolicSmooth(double[] y, int window = 20, double[] w = null)
        {

            int orglen = y.Length;
            int nchunk = (orglen / window) + (orglen%window==0?0:1);
            int ncoeff = nchunk + 2;
            y = Resize(y, nchunk*window);
            double[,] yArr = Reshape(y, nchunk, window);

            w = w == null ? Ones(nchunk*window) : Resize(w, nchunk*window);
            double[,] wArr = Reshape(w, nchunk, window);

            double[] x = Linspace(1.0 / window, 1, window);

            double[,] tmp = Multiply(yArr, wArr);
            double[] yx0w = RowSum(tmp);

            tmp = RowMultiply(tmp, x);
            double[] yx1w = RowSum(tmp);

            tmp = RowMultiply(tmp, x);
            double[] yx2w = RowSum(tmp);

            tmp = wArr;
            double[] wx0 = RowSum(tmp);

            tmp = RowMultiply(tmp, x);
            double[] wx1 = RowSum(tmp);

            tmp = RowMultiply(tmp, x);
            double[] wx2 = RowSum(tmp);

            tmp = RowMultiply(tmp, x);
            double[] wx3 = RowSum(tmp);

            tmp = RowMultiply(tmp, x);
            double[] wx4 = RowSum(tmp);

            int npower = 2 * 2 + 1;

            double[,] aqp = Identity(nchunk);
            double[,] bqp = MatrixFromFunc((r, c) => c>r?2.0:0.0, nchunk, nchunk);
            double[,] cqp = MatrixFromFunc((r, c) => c > r ? 2 * (c - r - 1) + 1 : 0.0, nchunk, nchunk);

            double[,] apb = MatrixFromFunc((r, c) => 0.0, 1, nchunk);
            double[,] bpb = MatrixFromFunc((r, c) => 1.0, 1, nchunk);
            double[,] cpb = MatrixFromFunc((r, c) => c, 1, nchunk);

            double[,] apc = MatrixFromFunc((r, c) => 0.0, 1, nchunk);
            double[,] bpc = MatrixFromFunc((r, c) => 0.0, 1, nchunk);
            double[,] cpc = MatrixFromFunc((r, c) => 1.0, 1, nchunk);

            double[,] dads = Concatenate(apc, apb, aqp);
            double[,] dbds = Concatenate(bpc, bpb, bqp);
            double[,] dcds = Concatenate(cpc, cpb, cqp);

            Matrix<double> mdads = DenseMatrix.OfArray(dads);
            Matrix<double> mdbds = DenseMatrix.OfArray(dbds);
            Matrix<double> mdcds = DenseMatrix.OfArray(dcds);

            Vector<double> vyx2w = DenseVector.OfEnumerable(yx2w);
            Vector<double> vyx1w = DenseVector.OfEnumerable(yx1w);
            Vector<double> vyx0w = DenseVector.OfEnumerable(yx0w);

            Vector<double> vwx0 = DenseVector.OfEnumerable(wx0);
            Vector<double> vwx1 = DenseVector.OfEnumerable(wx1);
            Vector<double> vwx2 = DenseVector.OfEnumerable(wx2);
            Vector<double> vwx3 = DenseVector.OfEnumerable(wx3);
            Vector<double> vwx4 = DenseVector.OfEnumerable(wx4);
            Vector<double> B = mdads * vyx2w + mdbds * vyx1w + mdcds * vyx0w;

            Matrix<double> A = RowMultiply(mdads, vwx4).TransposeAndMultiply(mdads) +
                // x^3
                            RowMultiply(mdbds, vwx3).TransposeAndMultiply(mdads) +
                            RowMultiply(mdads, vwx3).TransposeAndMultiply(mdbds) +
                //x^2
                            RowMultiply(mdcds, vwx2).TransposeAndMultiply(mdads) +
                            RowMultiply(mdbds, vwx2).TransposeAndMultiply(mdbds) +
                            RowMultiply(mdads, vwx2).TransposeAndMultiply(mdcds) +
                //x^1
                            RowMultiply(mdcds, vwx1).TransposeAndMultiply(mdbds) +
                            RowMultiply(mdbds, vwx1).TransposeAndMultiply(mdcds) +
                //x^0
                            RowMultiply(mdcds, vwx0).TransposeAndMultiply(mdcds);
            Vector<double> S = A.LU().Solve(B);
            double[] a = new double[nchunk];
            double[] b = new double[nchunk];
            double[] cc = new double[nchunk];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = S.At(2 + i);
            }
            b[0] = S[1];
            cc[0] = S[0];
            for (int i = 1; i < nchunk; i++)
            {
                b[i] = 2 * a[i - 1] + b[i - 1];
                cc[i] = a[i - 1] + b[i - 1] + cc[i - 1];
            }
            double[,] xx = RowTile(x, nchunk);

            double[,] yy = ColumnMultiply(a, Multiply(xx, xx));
            Accumulate(yy, ColumnMultiply(b, xx));
            Accumulate(yy, ColumnTile(cc, window));

            double[] ret = Flatten(yy);

            return Resize(ret, orglen);
        }

        public static double[,] ColumnTile(double[] x, int ncol)
        {
            int nrow = x.Length;
            double[,] ret = new double[nrow, ncol];
            for (int irow = 0; irow < nrow; irow++)
            {
                for (int icol = 0; icol < ncol; icol++)
                {
                    ret[irow, icol] = x[irow];
                }
            }
            return ret;
        }

        public static double[,] RowTile(double[] x, int nrow)
        {
            double[,] ret = new double[nrow, x.Length];
            for (int irow = 0; irow < nrow; irow++)
            {
                for (int icol = 0; icol < x.Length; icol++)
                {
                    ret[irow, icol] = x[icol];
                }
            }
            return ret;
        }

        public static double[] Flatten(double[,] x)
        {
            int nrow = x.GetLength(0);
            int ncol = x.GetLength(1);
            double[] ret = new double[nrow * ncol];
            for (int irow = 0; irow < nrow; irow++)
            {
                for (int icol = 0; icol < ncol; icol++)
                {
                    ret[irow*ncol + icol] = x[irow, icol];
                }
            }
            return ret;
        }

        public static double[,] ColumnMultiply(double[] v, double[,] m)
        {
            Debug.Assert(m.GetLength(0) == v.Length);
            int nrow = m.GetLength(0);
            int ncol = m.GetLength(1);
            double[,] ret = new double[nrow, ncol];
            for (int irow = 0; irow < nrow; irow++)
            {
                for (int icol = 0; icol < ncol; icol++)
                {
                    ret[irow, icol] = m[irow, icol]* v[irow];
                }
            }
            return ret;
        }

        private static DenseMatrix RowMultiply(Matrix<double> m, Vector<double> v)
        {
            Debug.Assert(m.ColumnCount == v.Count);
            DenseMatrix ret = new DenseMatrix(m.RowCount, m.ColumnCount);
            for (int irow = 0; irow < m.RowCount; irow++)
            {
                for (int icol = 0; icol < m.ColumnCount; icol++)
                {
                    ret.At(irow, icol, m.At(irow, icol) * v.At(icol));
                }
            }
            return ret;
        }


        public delegate double MatrixBuilder(int irow, int icol);
        public static double[,] MatrixFromFunc(MatrixBuilder f, int nrow, int ncol)
        {
            double[,] ret = new double[nrow, ncol];
            for (int irow = 0; irow < nrow; irow++)
            {
                for (int icol = 0; icol < ncol; icol++)
                {
                    ret[irow, icol] = f(irow, icol);
                }
            }
            return ret;
        }

        public static double[,] Identity(int nrow)
        {
            return MatrixFromFunc((irow, icol) => irow == icol ? 1.0 : 0, nrow, nrow);
        }


        public static double[] RowSum(double[,] x)
        {
            int nrow = x.GetLength(0);
            double[] ret = new double[nrow];
            for (int irow = 0; irow < nrow; irow++)
            {
                double sum = 0;
                for (int icol = 0; icol < x.GetLength(1); icol++)
                {
                    sum += x[irow, icol];
                }
                ret[irow] = sum;
            }
            return ret;
        }

        public static double[,] Concatenate(params double[][,] v)
        {
            int nCol = v[0].GetLength(1);
            int nRow = 0;
            foreach(double[,] x in v){
                nRow += x.GetLength(0);
                Debug.Assert(x.GetLength(1) == nCol, "Dimension not match");
            }
            double[,] ret = new double[nRow, nCol];
            int targetRow = 0;
            foreach (double[,] x in v)
            {
                for(int irow=0; irow<x.GetLength(0); irow++){
                    for(int icol=0; icol<x.GetLength(1); icol++){
                        ret[targetRow, icol] = x[irow, icol];
                    }
                    targetRow++;
                }
            }
            return ret;
        }

        /***
         * Zero padding resize 
         ***/
        public static double[] Resize(double[] y, int newlen, double fill=0.0)
        {
            double[] ret = new double[newlen];
            for (int i = 0; i < Math.Min(y.Length, ret.Length); i++)
            {
                ret[i] = y[i];
            }

            for(int i=y.Length; i< ret.Length; i++){
                ret[i] = fill;
            }

            return ret;
        }

        //This assumes width of one
        public static double[] Gradient(double[] x)
        {
            double[] ret = new double[x.Length];
            for (int i = 1; i < x.Length - 1; i++)
            {
                ret[i] = (x[i + 1] - x[i - 1]) / 2.0;
            }
            ret[0] = x[1] - x[0];
            ret[x.Length - 1] = x[x.Length - 1] - x[x.Length - 2];
            return ret;
        }

        public static double[] Gradient(double[] x, double[] y){
            Debug.Assert(x.Length == y.Length);
            double[] ret = new double[y.Length];
            for (int i = 1; i < y.Length - 1; i++)
            {
                ret[i] = (y[i + 1] - y[i - 1]) / (x[i+1]-x[i-1]);
            }
            ret[0] = (y[1] - y[0])/(x[1]-x[0]);
            ret[x.Length - 1] = (y[y.Length - 1] - y[y.Length - 2]) / (x[x.Length - 1] - x[x.Length - 2]);
            return ret;
        }



        public static double[,] Reshape(double[] x, int nrow, int ncol)
        {
            Debug.Assert(x.Length == nrow * ncol);
            double[,] ret = new double[nrow, ncol];
            for (int irow = 0; irow < nrow; irow++)
            {
                for (int icol = 0; icol < ncol; icol++)
                {
                    ret[irow, icol] = x[irow * ncol + icol];
                }
            }
            return ret;
        }

        public static void PrintArray(double[] x, String fmt="{0:0.0000}, ", int ncol=5)
        {
            int len = x.Length;
            Console.Write("[");
            for (int i = 0; i < len; i++)
            {
                if(i%ncol ==0){
                    Console.WriteLine();
                }
                Console.Write(String.Format(fmt, x[i]));
            }
            Console.Write("]");
        }

        public static void PrintArray(double[,] x, String fmt="{0:0.0000}, ")
        {
            int nrow = x.GetLength(0);
            int ncol = x.GetLength(1);
            Console.Write("[");
            for (int irow = 0; irow<nrow ; irow++)
            {
                for(int icol = 0; icol<ncol ; icol++){
                    Console.Write(String.Format(fmt, x[irow, icol]));
                }
                Console.WriteLine();
                Console.WriteLine("------");
            }
            Console.Write("]");
        }

        public static double[] Slice(double[] x, int start=0, int? stop=null)
        {
            //inclusive start and exclusive stop
            int stopp = stop == null ? x.Length : (int)stop;
            stopp = stopp < 0 ? Math.Max(0, x.Length + stopp) : stopp;
            stopp = Math.Min(stopp, x.Length);
            start = Math.Min(start, x.Length);
            start = Math.Max(start, 0);
            double[] ret = new double[Math.Max(stopp - start,0)];
            for (int i = start; i < stopp; i++)
            {
                ret[i - start] = x[i];
            }

            return ret;
        }

        public static int LowerBound(double[] x, double v)
        {
            //assume x is sorted ascendingly
            //find the index of the last element that x[i] < v
            //return -1 if 
            //TODO: make this O(log(n))
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] >= v)
                {
                    return i-1;
                }
            }
            return x.Length;
        }

        public static int Argmax(double[] x)
        {
            int ret = -1;
            bool first = true;
            double vmax = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                double v = x[i];
                if (first)
                {
                    ret = i;
                    vmax = v;
                    first = false;
                }
                if (v > vmax)
                {
                    ret = i;
                    vmax = v;
                }
            }
            return ret;
        }

        public static int UpperBound(double[] x, double v)
        {
            //assume x is sorted ascendingly
            //find the index of the first element in x that's > v
            //TODO: make this O(log(n))
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] >= v)
                {
                    return i;
                }
            }
            return x.Length;
        }
    }


    public class HistogramResult
    {
        public double[] edges;
        public double[] hist;
        public double overflow;
        public double underflow;
        public HistogramResult(int nbin, double lower, double upper)
        {
            edges = NP.Linspace(lower, upper, nbin + 1);
            hist = NP.Zeroes(nbin);
            overflow = 0;
            underflow = 0;
        }
    }
}

