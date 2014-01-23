using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minuit2;
using OxyPlot.Series;
using System.Diagnostics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics;
using System.Linq;
using PropertyChanged;
using OxyPlot;

namespace CITHEPLib
{
    public delegate double F1(double x);
    public delegate double F2(double x, double y);
    public delegate double F3(double x1, double x2, double x3);
    public delegate double F4(double x1, double x2, double x3, double x4);
    public delegate double F5(double x1, double x2, double x3, double x4,
                       double x5);
    public delegate double F6(double x1, double x2, double x3, double x4,
                       double x5, double x6);
    public delegate double F7(double x1, double x2, double x3, double x4,
                       double x5, double x6, double x7);
    public delegate double F11(double x1, double x2, double x3, double x4,
                        double x5, double x6, double x7, double x8);
    namespace PeakLocator
    {
        public delegate double PDF(double[] v);
        //using first first argument independent variable convention
        public class Integral
        {
            public static double Integrate1D(F1 f, double lower, double upper, int pieces)
            {
                PDF pdf = (v) => f(v[0]);
                double[] tmp = new double[] { };
                return Integrate1D(pdf, lower, upper, pieces, tmp);
            }

            public static double Integrate1D(PDF pdf, double lower, double upper, int pieces, double[] partial_arg)
            {
                double[] tmparg = new double[partial_arg.Length + 1];
                double integral = 0;
                for (int i = 0; i < partial_arg.Length; i++) { tmparg[i + 1] = partial_arg[i]; }
                //this can be optimized a bit but meh.
                for (int i = 0; i < pieces; i++)
                {
                    double width = (upper - lower) / (pieces);
                    double h = width / 3.0;
                    double x0 = lower + width * i;
                    double x1 = x0 + h;
                    double x2 = x0 + 2.0 * h;
                    double x3 = x0 + 3.0 * h;

                    tmparg[0] = x0;
                    double y0 = pdf(tmparg);
                    tmparg[0] = x1;
                    double y1 = pdf(tmparg);
                    tmparg[0] = x2;
                    double y2 = pdf(tmparg);
                    tmparg[0] = x3;
                    double y3 = pdf(tmparg);

                    integral += 3.0 * h / 8.0 * (y0 + 3 * y1 + 3 * y2 + y3);
                }
                return integral;
            }
        }
        public class CrudeZeroCrossing
        {
            public int Crossing;
            public int FirstTrigger;
            public int LastTrigger;
            public CrudeZeroCrossing(int crossing, int firsttrigger, int lasttrigger)
            {
                this.Crossing = crossing;
                this.FirstTrigger = firsttrigger;
                this.LastTrigger = lasttrigger;
            }

            public override bool Equals(object o)
            {
                CrudeZeroCrossing c = (CrudeZeroCrossing)o;
                return c.Crossing == this.Crossing && c.FirstTrigger == this.FirstTrigger && c.LastTrigger == this.LastTrigger;
            }

            public override string ToString()
            {
                return String.Format("x:{0:d}, ft:{1:d}, lt:{2:d}", this.Crossing, this.FirstTrigger, this.LastTrigger);
            }
        }

        public class ZeroCrossingSlopeResult
        {
            public double Slope;
            public double XIntercept;
            public double YIntercept;
            public double bw;
            public ZeroCrossingSlopeResult(double m, double c, double xintercept, double bw)
            {
                this.Slope = m;
                this.YIntercept = c;
                this.XIntercept = xintercept;
                this.bw = bw;
            }
        }

        [ImplementPropertyChanged]
        public class PeakFinder
        {
            public CrudeZeroCrossing[] Crossings { get; set; }
            public InitialPeakGuess[] Guesses { get; set; }
            public double[] x { get; set; }
            public double[] y { get; set; }
            public double[] Edges { get; set; }
            public double[] Gradient { get; set; }
            public double[] SmoothGradient { get; set; }
            public double[] ScaledGradient { get; set; }
            public double TriggerValue { get; set; }
            public PeakFinder()
            {
                Crossings = new CrudeZeroCrossing[0];
                Guesses = new InitialPeakGuess[0];
                x = new double[0];
                y = new double[0];
                Edges = new double[0];
                Gradient = new double[0];
                SmoothGradient = new double[0];
                ScaledGradient = new double[0];
                TriggerValue = 0.5;

            }
            
            public PeakFinder(double[] y, double triggerlevel = 0.4, double scalingoffset = 0, int smoothwindow = 100)
            {
                double scalingpower = 3.0;
                x = NP.Arange(0, y.Length);
                Edges = NP.Add(NP.Arange(0, y.Length), -0.5);
                Debug.Assert(x.Length == y.Length, "Dimension not match");
                Gradient = NP.Gradient(x, y);
                SmoothGradient = NP.GlobalParabolicSmooth(Gradient, smoothwindow);
                double[] scale = NP.Linspace(scalingoffset, scalingoffset + y.Length, y.Length);
                scale = NP.Broadcast((xx) => Math.Pow(xx, scalingpower), scale);
                ScaledGradient = NP.Multiply(SmoothGradient, scale);
                TriggerValue = ScaledGradient.Max() * triggerlevel;
                Crossings = PeakLocator.ZeroCrossings(ScaledGradient, TriggerValue);
                InitialPeakGuess[] tmp = new InitialPeakGuess[Crossings.Length];
                for (int i = 0; i < Crossings.Length; i++)
                {
                    tmp[i] = InitialPeakGuess.OfCrudeZeroCrossingAndGradient(Crossings[i], x, y, SmoothGradient);
                }
                Guesses = tmp;

            }
        }

        /// <summary>
        /// Initial peak guess
        /// </summary>
        [ImplementPropertyChanged]
        [Serializable]
        public class InitialPeakGuess
        {
            public double mu { get; set; }
            public double sigma { get; set; }
            public double m { get; set; }
            public double c { get; set; }
            public double nsig { get; set; }
            public double nbkg { get; set; }
            public double lowerbound { get; set; }
            public double upperbound { get; set; }
            public int lowerboundIndex { get; set; }
            public int upperboundIndex { get; set; }

            /// <summary>
            /// Default initial peakguess
            /// </summary>
            public InitialPeakGuess()
            {
                mu = 0;
                sigma = 0;
                m = 0;
                c = 0;
                nsig = 0;
                nbkg = 0;
                lowerbound = 0;
                upperbound = 0;
                lowerboundIndex = 0;
                upperboundIndex = 0;
            }

            public InitialPeakGuess(InitialPeakGuess guess)
            {
                mu = guess.mu;
                sigma = guess.sigma;
                m = guess.m;
                c = guess.c;
                nsig = guess.nsig;
                nbkg = guess.nbkg;
                lowerbound = guess.lowerbound;
                upperbound = guess.upperbound;
                lowerboundIndex = guess.lowerboundIndex;
                upperboundIndex = guess.upperboundIndex;
            }

            public override string ToString()
            {
                return String.Format(
                @"
                ----------
                mu = {0}
                sigma = {1}
                m = {2}
                c = {3}
                nsig = {4}
                nbkg = {5}
                lowerbound = {6}
                upperbound = {7}
                lowerboundIndex = {8}
                upperboundIndex = {9}
                -----------",
                mu, sigma, m, c, nsig, nbkg, lowerbound, upperbound, lowerboundIndex, upperboundIndex);
            }

            /// <summary>
            /// Construct initial peak guess from crude zerocrossing.
            /// </summary>
            /// <param name="cross"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="sg">Smooth gradient</param>
            /// <param name="width"></param>
            /// <param name="nsigma"></param>
            /// <returns></returns>
            public static InitialPeakGuess OfCrudeZeroCrossingAndGradient(CrudeZeroCrossing cross, double[] x, double[] y, double[] sg, int width=10, double nsigma = 2.0)
            {
                InitialPeakGuess ret = new InitialPeakGuess();
                double[] g = sg;
                //NP.PrintArray(g);
                ZeroCrossingSlopeResult zerocrossing = PeakLocator.CrossingSlope(x, g, cross, width);
                ret.mu = zerocrossing.XIntercept;
                double inflection = PeakLocator.FastZeroCrossingLeftInflection(x, g, cross);
                Console.WriteLine("Inflection {0}", inflection);
                double sigma = ret.mu - inflection;
                ret.sigma = sigma;

                double lowerbound = Math.Max(0.0, zerocrossing.XIntercept - nsigma * sigma);
                double upperbound = Math.Min(zerocrossing.XIntercept + nsigma * sigma, x.Last());

                ret.lowerboundIndex = Math.Max(0, NP.LowerBound(x, lowerbound));
                ret.upperboundIndex = Math.Min(x.Length - 1, NP.UpperBound(x, upperbound));
                ret.lowerbound = x[ret.lowerboundIndex];
                ret.upperbound = x[ret.upperboundIndex];
                double[] xSlice = NP.Slice(x, ret.lowerboundIndex, ret.upperboundIndex + 1);//remember slice is right exclusive
                double[] ySlice = NP.Slice(y, ret.lowerboundIndex, ret.upperboundIndex + 1);
                lowerbound = ret.lowerbound;
                upperbound = ret.upperbound;

                Console.WriteLine("slope {0}, bw {1}", zerocrossing.Slope, zerocrossing.bw);
                double ntotalpeak = -zerocrossing.Slope / zerocrossing.bw * Math.Pow(sigma, 3) * Math.Sqrt(2 * Math.PI);
                double leftnsigma = (zerocrossing.XIntercept - lowerbound) / sigma;
                double rightnsigma = (upperbound - zerocrossing.XIntercept) / sigma;
                double npeakfrac = 0.5 * (SpecialFunctions.Erf(leftnsigma / Math.Sqrt(2)) + SpecialFunctions.Erf(rightnsigma / Math.Sqrt(2)));
                Console.WriteLine("npeakfrac {0}", npeakfrac);
                Console.WriteLine("ntotalpeak {0}", ntotalpeak);
                ret.nsig = ntotalpeak * npeakfrac;

                F3 gaussian = (xx, xmu, xsigma) => 1 / (xsigma * Math.Sqrt(2 * Math.PI)) * Math.Exp(-(xx - xmu) * (xx - xmu) / (2 * xsigma * xsigma));
                NP.Func1 rgauss = (xx) => ntotalpeak * zerocrossing.bw * gaussian(xx, ret.mu, ret.sigma);
                double[] peakamount = NP.Broadcast(rgauss, xSlice);
                double[] bkgamount = NP.Broadcast((xx, yy) => xx - yy, ySlice, peakamount);

                //NP.PrintArray(bkgamount);

                NP.LinearFitResult bkgshape = NP.LinearFit(xSlice, bkgamount);
                double m = bkgshape.m;
                double c = bkgshape.c;
                double bw = zerocrossing.bw;
                Console.WriteLine("bkg.m {0}, bkg.c {1}", m, c);
                double nbkg = 1 / bw * (m / 2.0 * (upperbound * upperbound - lowerbound * lowerbound) + c * (upperbound - lowerbound));

                ret.m = m;
                ret.c = c;
                ret.nbkg = nbkg;

                return ret;
            }

            /// <summary>
            /// Construct Initial PeakGuess from CrudeZeroCrossing. Find mu from zero crossing position, sigma from inflection point
            /// nbkg, nsig is from zero crossing slope. The bound is mu+-n*sigma.
            /// </summary>
            /// <param name="cross"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="window">smoothing parameter. indicate the size of piecewise</param>
            /// <param name="nsigma">window size</param>
            /// <param name="width">zero crossing fit width</param>
            /// <returns></returns>
            public static InitialPeakGuess OfCrudeZeroCrossing(CrudeZeroCrossing cross, double[] x, double[] y, int window = 30, double nsigma = 2.0, int width=30 )
            {
                InitialPeakGuess ret = new InitialPeakGuess();
                double[] rawg = NP.Gradient(x, y); //this is a raw gradient
                double[] g = NP.GlobalParabolicSmooth(y, window);
                return InitialPeakGuess.OfCrudeZeroCrossingAndGradient(cross, x, y, g, width, nsigma);
            }
        }

        public class PeakLocator
        {
            /// <summary>
            /// Find all zero crossing of s with given trigger. The zero crossing is determined
            /// by detecting the plot rise above trigger and falls below zero.
            /// </summary>
            /// <param name="s"></param>
            /// <param name="trigger"></param>
            /// <returns></returns>
            public static CrudeZeroCrossing[] ZeroCrossings(double[] s, double trigger)
            {
                bool triggered = false;
                int tmpFirstTrigger = 0;
                int tmpLastTrigger = 0;
                int tmpZeroCrossing = 0;

                List<CrudeZeroCrossing> l = new List<CrudeZeroCrossing>();
                for (int i = 0; i < s.Length; i++)
                {
                    double v = s[i];

                    if (!triggered)
                    {
                        if (v > trigger)
                        {
                            triggered = true;
                            tmpFirstTrigger = i;
                            tmpLastTrigger = i;
                        }
                    }
                    else //in triggered mode
                    {
                        if (v < 0)
                        {
                            tmpZeroCrossing = i;
                            triggered = false;
                            CrudeZeroCrossing c = new CrudeZeroCrossing(i, tmpFirstTrigger, tmpLastTrigger);
                            l.Add(c);
                        }
                        else
                        {
                            if (v > trigger)
                            {
                                tmpLastTrigger = i;
                            }
                        }
                    }
                }
                return l.ToArray();
            }

            //public static ZeroCrossingSlopeResult CrossingSlope(double[] x, double[] y, CrudeZeroCrossing c, int width = 30)
            //{
            //    double[] xs = NP.Slice(x, c.Crossing - width, c.Crossing + width);
            //    double[] ys = NP.Slice(y, c.Crossing - width, c.Crossing + width);
            //    NP.LinearFitResult result = NP.LinearFit(xs, ys);
            //    double bw = (x[c.Crossing + width] - x[c.Crossing - width]) / (2 * width);
            //    ZeroCrossingSlopeResult ret = new ZeroCrossingSlopeResult(result.m, result.c, bw);
            //    return ret;
            //}

            /// <summary>
            /// Perform linear fit to x and y around c.Crossing+-width
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="c"></param>
            /// <param name="width"></param>
            /// <returns></returns>
            public static ZeroCrossingSlopeResult CrossingSlope(double[] x, double[] y, CrudeZeroCrossing c, int width = 10)
            {
                //we do want the right hand side to be inclusive
                double[] xs = NP.Slice(x, c.Crossing - width, c.Crossing + width + 1);
                double[] ys = NP.Slice(y, c.Crossing - width, c.Crossing + width + 1);

                NP.LinearFitResult linr = NP.LinearFit(xs, ys);
                //parabolic fit doesn't help since it's symmetric
                //NP.ParabolicFitResult parar = NP.ParabolicFit(xs, ys); //improve slope with intercept
                double cc = linr.c;
                double xintercept = -linr.c / linr.m;
                double mm = linr.m;
                double bw = (xs[xs.Length - 1] - xs[0]) / (xs.Length - 1);
                //double mm = 2 * parar.a * xintercept + parar.b; //improved slope
                ZeroCrossingSlopeResult ret = new ZeroCrossingSlopeResult(mm, cc, xintercept, bw);
                return ret;
            }

            /// <summary>
            /// Find inflection point by finding the nearest maximum and fit parabola to it.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="c"></param>
            /// <returns></returns>
            public static double ZeroCrossingLeftInflection(double[] x, double[] y, CrudeZeroCrossing c)
            {
                double[] xs = NP.Slice(x, c.FirstTrigger, c.LastTrigger);
                double[] ys = NP.Slice(y, c.FirstTrigger, c.LastTrigger);
                int argmax = NP.Argmax(ys);
                NP.PrintArray(xs);
                NP.PrintArray(ys);
                NP.ParabolicFitResult result = NP.ParabolicFit(xs, ys);

                return (-result.b / (2.0 * result.a));
            }

            /// <summary>
            /// Find left inflection with out fitting parabola
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="c"></param>
            /// <returns></returns>
            public static double FastZeroCrossingLeftInflection(double[] x, double[] y, CrudeZeroCrossing c)
            {
                //this assumes that ys is smooth
                double[] xs = NP.Slice(x, c.FirstTrigger, c.LastTrigger);
                double[] ys = NP.Slice(y, c.FirstTrigger, c.LastTrigger);
                int argmax = NP.Argmax(ys);
                Console.WriteLine("Argmax {0}", argmax);
                return xs[argmax];
            }

            /// <summary>
            /// Find zerocrossing of smoothed gradient. The Gradient is scaled by x+offset^scalingpower to take into accout of
            /// mean dependent width. The trigger for zero crossing is fraction of Maximum value of smoothed gradient.
            /// </summary>
            /// <param name="x">center value for each bin</param>
            /// <param name="y">hist value of each bin</param>
            /// <param name="offset">scaling offset</param>
            /// <param name="trigger">Trigger value. Fraction of maximum gradient</param>
            /// <param name="scalingpower">Scaling power default 3 to account of mu scaling of width.</param>
            /// <param name="smoothwindow">Smooth window</param>
            /// <returns></returns>
            public static CrudeZeroCrossing[] SpectrumCrossing(double[] x, double[] y, double offset = 0, double trigger = 0.4, double scalingpower = 3.0, int smoothwindow = 20)
            {
                Debug.Assert(x.Length == y.Length, "Dimension not match");
                double[] grad = NP.Gradient(x, y);
                grad = NP.GlobalParabolicSmooth(grad, smoothwindow);
                double[] scale = NP.Linspace(offset, offset + y.Length, y.Length);
                scale = NP.Broadcast((xx) => Math.Pow(xx, scalingpower), scale);
                double[] scaledGradient = NP.Multiply(grad, scale);
                double triggerLevel = scaledGradient.Max() * trigger;
                CrudeZeroCrossing[] crossings = ZeroCrossings(scaledGradient, triggerLevel);
                return crossings;
            }

        }

        class PeakFitterException : Exception
        {
            public PeakFitterException(String msg)
                : base(msg)
            {
            }
        }

        /// <summary>
        /// Normalized function with cache class
        /// </summary>
        public class CachedNormalized
        {
            double cache;//the normalization cache value
            double[] partial_argcache;// all argument to except the dependent variable(assuming the left most one).
            double lower;//lower bound of the integral
            double upper;//upper bound of the integral
            int pieces;//number of pieces to us
            PDF pdf;//pdf
            /// <summary>
            /// construct
            /// </summary>
            /// <param name="pdf">PDF. The first argument(v[0]) must be dependent variable.</param>
            /// <param name="lowerbound"></param>
            /// <param name="upperbound"></param>
            /// <param name="pieces"></param>
            public CachedNormalized(PDF pdf, double lowerbound, double upperbound, int pieces = 100)
            {
                this.pdf = pdf;
                this.lower = lowerbound;
                this.upper = upperbound;
                this.partial_argcache = new double[0];
                this.cache = -1;
                this.pieces = pieces;
            }

            /// <summary>
            /// Compute normalization given arg
            /// </summary>
            /// <param name="arg"></param>
            /// <returns></returns>
            public double Compute(double[] arg)
            {
                double[] partial_arg = GetPartialArg(arg);
                if (CacheHit(partial_arg))
                {
                    //do nothing
                    //Console.WriteLine("Hit");
                }
                else
                {
                    //Console.WriteLine("Miss");
                    cache = normalization(partial_arg);
                    //NP.PrintArray(partial_arg);
                    partial_argcache = (double[])partial_arg.Clone();
                }
                double value = pdf(arg);
                //Console.WriteLine(value);
                //Console.WriteLine(cache);
                return value / cache;
            }

            bool CacheHit(double[] partial_arg)
            {
                if (partial_arg.Length != partial_argcache.Length) return false;
                for (int i = 0; i < partial_arg.Length; i++)
                {
                    if (partial_arg[i] != partial_argcache[i]) return false;
                }
                return true;
            }

            double[] GetPartialArg(double[] arg)
            {
                double[] ret = new double[arg.Length - 1];
                for (int i = 1; i < arg.Length; i++)
                {
                    ret[i - 1] = arg[i];
                }
                return ret;
            }

            //simpsons 3/8
            double normalization(double[] partial_arg)
            {
                return Integral.Integrate1D(this.pdf, lower, upper, pieces, partial_arg);
            }
        }

        public class Chi2
        {
            PDF pdf;
            double[] x;
            double[] y;
            double[] sigma;
            public double minsigma = 1e-7;

            public Chi2(PDF pdf, double[] x, double[] y, double[] sigma = null)
            {
                Debug.Assert(x.Length == y.Length);
                if (sigma == null)
                {
                    sigma = NP.Ones(x.Length);
                }
                Debug.Assert(x.Length == sigma.Length);

                this.x = x;
                this.y = y;
                this.sigma = sigma;
                this.pdf = pdf;
            }

            public double Compute(double[] arg)
            {
                double[] arg_cascade = new double[arg.Length + 1];
                double chi2 = 0.0;
                for (int i = 0; i < arg.Length; i++)
                {
                    arg_cascade[i + 1] = arg[i];
                }
                for (int i = 0; i < this.x.Length; ++i)
                {
                    arg_cascade[0] = x[i];
                    double expected = pdf(arg_cascade);
                    NP.PrintArray(arg_cascade);
                    Console.WriteLine("{0}, {1}", x[i], y[i]);
                    if (sigma[i] > minsigma)
                    {
                        double tmp = (y[i] - expected) / sigma[i];
                        chi2 += tmp * tmp;
                    }
                }
                return chi2;
            }
        }

        public class BinChi2
        {
            PDF pdf;
            double[] edges;
            double[] y;
            double[] sigma;
            public double minsigma = 1e-7;

            public BinChi2(PDF pdf, double[] edges, double[] y, double[] sigma = null)
            {
                Debug.Assert(edges.Length == y.Length + 1);
                if (sigma == null)
                {
                    sigma = NP.Ones(y.Length);
                }
                Debug.Assert(y.Length == sigma.Length);

                this.edges = edges;
                this.y = y;
                this.sigma = sigma;
                this.pdf = pdf;
            }

            public double Compute(double[] arg)
            {
                //NP.PrintArray(arg);
                double[] arg_cascade = new double[arg.Length + 1];
                double chi2 = 0.0;

                for (int i = 0; i < arg.Length; i++)
                {
                    arg_cascade[i + 1] = arg[i];
                }
                F1 rpdf = (xx) =>
                {
                    double[] tmparg = arg_cascade;
                    tmparg[0] = xx;
                    return pdf(tmparg);
                };
                for (int i = 0; i < this.edges.Length - 1; ++i)
                {
                    //Console.WriteLine("i: {0}", i);
                    double midpoint = (edges[i] + edges[i + 1]) / 2;
                    arg_cascade[0] = edges[i];
                    double expected = Integral.Integrate1D(this.pdf, edges[i], edges[i + 1], 2, arg);
                    //Console.WriteLine("{0}, {1}", expected, y[i]);
                    if (sigma[i] > minsigma)
                    {
                        double tmp = (y[i] - expected) / sigma[i];
                        chi2 += tmp * tmp;
                    }
                }
                return chi2;
            }
        }

        public class PeakingPDF
        {
            CachedNormalized bkg;
            CachedNormalized peak;
            public PeakingPDF(double lowerbound, double upperbound, int pieces = 2000)
            {
                PDF gauss = GenericPDF.gaussian;
                PDF linear = GenericPDF.linear;
                this.bkg = new CachedNormalized(linear, lowerbound, upperbound, pieces);
                this.peak = new CachedNormalized(gauss, lowerbound, upperbound, pieces);
            }

            public static PeakingPDF OfInitialPeakGuess(InitialPeakGuess guess)
            {
                return new PeakingPDF(guess.lowerbound, guess.upperbound);
            }

            public string[] ParamNames()
            {
                string[] ret = { "x", "mu", "sigma", "nsig", "m", "c", "nbkg" };
                return ret;
            }

            public double Compute(double[] v)
            {
                double x = v[0];
                double mu = v[1];
                double sigma = v[2];
                double nsig = v[3];
                double m = v[4];
                double c = v[5];
                double nbkg = v[6];
                double[] peakarg = { x, mu, sigma };
                double[] bkgarg = { x, m, c };
                double ret = nsig * peak.Compute(peakarg) + nbkg * bkg.Compute(bkgarg);
                return ret;
            }
        }

        [ImplementPropertyChanged]
        [Serializable]
        public class FitResult
        {
            public double mu { get; set; }
            public double err_mu { get; set; }
            public double sigma { get; set; }
            public double err_sigma { get; set; }
            public double nsig { get; set; }
            public double err_nsig { get; set; }
            public double m { get; set; }
            public double err_m { get; set; }
            public double c { get; set; }
            public double err_c { get; set; }
            public double nbkg { get; set; }
            public double err_nbkg { get; set; }
            public double fmin { get; set; }
            public double nbin { get; set; }
            public double Resolution { get; set; }
            public bool FitConverged { get; set; }
            public FitResult()
            {
                mu = 0;
                err_mu = 0;
                sigma = 0;
                err_sigma = 0;
                nsig = 0;
                err_nsig = 0;
                m = 0;
                err_m = 0;
                c = 0;
                err_c = 0;
                nbkg = 0;
                err_nbkg = 0;
                fmin = 0;
                nbin = 1;
                Resolution = 1;
                FitConverged = false;
            }

            public FitResult(Minuit m, int nbin=1)
            {
                mu = m.GetValue("mu");
                err_mu = m.GetError("mu");
                sigma = m.GetValue("sigma");
                err_sigma = m.GetError("sigma");
                nsig = m.GetValue("nsig");
                err_nsig = m.GetError("nsig");
                this.m = m.GetValue("m");
                err_m = m.GetError("m");
                this.c = m.GetValue("c");
                err_c = m.GetValue("c");
                this.nbkg = m.GetValue("nbkg");
                err_nbkg = m.GetValue("nbkg");
                fmin = m.GetFmin();
                this.nbin = nbin;
                Resolution = mu != 0 ? sigma / mu : 1;
                FitConverged = m.FitConverged();
            }

            public double[] PartialArg()
            {
                double[] ret = { mu, sigma, nsig, m, c, nbkg };
                return ret;
            }
        }

        public class PeakFitter
        {
            public Minuit minuit;
            public PeakingPDF pdf;
            public double[] edges;

            /// <summary>
            /// Construct PeakFitter from initialpeakguess, histogam edges+values, 
            /// </summary>
            /// <param name="guess">Initial peak guess.</param>
            /// <param name="edges">edges of the truncated histogram</param>
            /// <param name="y">histogram value</param>
            /// <param name="sigma">Uncertainty on Y</param>
            /// <returns>PeakFitter</returns>
            public static PeakFitter FromGuess(InitialPeakGuess guess, double[] edges, double[] y, double[] sigma = null)
            {
                
                PeakingPDF pdf = PeakingPDF.OfInitialPeakGuess(guess);
                BinChi2 x2 = new BinChi2(pdf.Compute, edges, y, sigma);
                string[] paramnames = pdf.ParamNames(); //full paramname with x
                string[] pname = new string[paramnames.Length - 1]; //Dock off x
                for (int i = 0; i < pname.Length; i++)
                {
                    pname[i] = paramnames[i + 1];
                }
                Minuit minuit = new Minuit((FCN)(x2.Compute), pname);
                minuit.SetInitialValue("mu", guess.mu);
                minuit.SetInitialValue("sigma", guess.sigma);
                minuit.SetInitialValue("nsig", guess.nsig);
                minuit.SetInitialValue("m", guess.m);
                minuit.SetInitialValue("c", guess.c);
                minuit.SetInitialValue("nbkg", guess.nbkg);

                PeakFitter ret = new PeakFitter() { pdf = pdf, minuit = minuit, edges=edges};
                return ret;
            }

            /// <summary>
            /// Perform migrad
            /// </summary>
            public void Fit()
            {
                minuit.migrad();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public double[] FittedValue()
            {
                FitResult result = new FitResult(minuit);
                double[] ret = new double[edges.Length - 1];
                for (int i = 0; i < this.edges.Length - 1; i++)
                {
                    ret[i] = Integral.Integrate1D(pdf.Compute, edges[i], edges[i + 1], 5, result.PartialArg());
                }
                return ret;
            }

            public DataPoint[] FittedDataPoint()
            {
                if (edges.Length == 0) { return new DataPoint[0];}
                FitResult result = new FitResult(minuit);
                
                DataPoint[] ret = new DataPoint[edges.Length - 1];

                
                for (int i = 0; i < this.edges.Length - 1; i++)
                {
                    double y = Integral.Integrate1D(pdf.Compute, edges[i], edges[i + 1], 5, result.PartialArg());
                    double x = (this.edges[i]+this.edges[i+1])/2.0;
                    ret[i] = new DataPoint(x, y);
                }
                return ret;
            }

        }

        /// <summary>
        /// Utility functions
        /// </summary>
        public class GenericPDF
        {
            /// <summary>
            /// gaussian
            /// </summary>
            /// <param name="v">[x, mu, sigma]</param>
            /// <returns>double</returns>
            public static double gaussian(double[] v)
            {
                Debug.Assert(v.Length == 3);
                double x = v[0];
                double mu = v[1];
                double sigma = v[2];
                return gaussian_flat(x, mu, sigma);
            }
            public static double gaussian_flat(double x, double mu, double sigma)
            {
                double a = 1 / (Math.Sqrt(2 * Math.PI)) / sigma;
                return a * Math.Exp(-((x - mu) * (x - mu)) / (2 * sigma * sigma));
            }

            public static double linear(double[] v)
            {
                Debug.Assert(v.Length == 3);
                double x = v[0];
                double m = v[1];
                double c = v[2];
                return m * x + c;
            }

            public static double linear_flat(double x, double m, double c)
            {
                return m * x + c;
            }

        }

        //class MultipeakSetting
        //{
        //    public Range FitRange;
        //    public List<InitialPeakGuess> Guesses;
        //    public MultipeakSetting(Range r, InitialPeakGuess g)
        //    {
        //        this.FitRange = r;
        //        this.Guesses = new List<InitialPeakGuess>() { g };
        //    }
        //    public void Merge(MultipeakSetting ms)
        //    {
        //        this.FitRange = this.FitRange.Union(ms.FitRange);
        //        foreach (InitialPeakGuess ipg in ms.Guesses)
        //        {
        //            Guesses.Add(ipg);
        //        }
        //    }

        //    public bool IsMergeableWith(MultipeakSetting ms)
        //    {
        //        return this.FitRange.HasIntersection(ms.FitRange);
        //    }
        //}

        //class Range{
        //    public double lower;
        //    public double upper;
        //    class RangeException : Exception
        //    {
        //        public RangeException(string msg) : base(msg) { }
        //    }
        //    public Range(double lower, double upper)
        //    {
        //        this.lower = lower;
        //        this.upper = upper;
        //    }
        //    public Range Union(Range r)
        //    {
        //        if (!this.HasIntersection(r)) throw new RangeException("Can't union non intersecting range");
        //        double newlow = Math.Min(r.lower, this.lower);
        //        double newup = Math.Max(r.upper, this.upper);
        //        return new Range(newlow, newup);
        //    }

        //    public bool HasIntersection(Range r)
        //    {
        //        return (this.lower <= r.lower && r.lower <= this.upper) || (this.lower <= r.upper && r.upper <= this.upper);
        //    }

        //    public bool IsSupersetOf(Range r)
        //    {
        //        return (this.lower < r.lower && this.upper > r.upper);
        //    }
        //}

        //class MultiPeakPDF{
        //    List<InitialPeakGuess> InitialGuess;
        //    List<string> BackgroundPDFParam;
        //    List<string> param;
        //    Range limit;
        //    List<CachedNormalized> normpeak; //normalized pdf
        //    CachedNormalized normbkg; //normalized background
        //    const int nparamperpeak=3;//nsig, mu, sigma
        //    public MultiPeakPDF(MultipeakSetting mps, PDF BackgroundPDF, List<string> BackgroundPDFParam)
        //    {
        //        this.BackgroundPDFParam = BackgroundPDFParam;
        //        this.limit = mps.FitRange;
        //        InitialGuess = mps.Guesses;

        //        normpeak = new List<CachedNormalized>();
        //        foreach (InitialPeakGuess guess in InitialGuess)
        //        {
        //            normpeak.Add(new CachedNormalized(GenericPDF.gaussian, limit.lower, limit.upper));
        //        }

        //        normbkg = new CachedNormalized(BackgroundPDF, limit.lower, limit.upper);

        //        //building parameter list
        //        param = new List<string>();
        //        param.Add("x");
        //        param.Add("nbkg");
        //        bool first = false;
        //        foreach (string bkgparam in BackgroundPDFParam)
        //        {
        //            if (first)
        //            {
        //                //ignore "x"
        //                first = false;
        //            }
        //            else
        //            {
        //                param.Add(bkgparam);
        //            }
        //        }
        //        for (int i = 0; i < InitialGuess.Count; i++)
        //        {
        //            param.Add(String.Format("npeak_{0:d}", i));
        //            param.Add(String.Format("mu_{0:d}", i));
        //            param.Add(String.Format("sigma_{0:d}", i));
        //        }


        //    } 
        //    /***
        //     * The order for parameter is 
        //     * x, nbkg, bkgparam1, bkgparam2...., npeak_1, mu1, sigma1, npeak2, mu2, sigma2,.....
        //     * compute nbkg*(norm)BackgroundPDF(x, bkgparam1, ...) + npeak_1* (norm)gaussian(x,mu1, sigma1), ...
        //     ***/
        //    double Compute(double[] arg)
        //    {
        //        Debug.Assert(arg.Length == param.Count);
        //        //TODO: FIXME


        //        double ret = 0.0;
        //        ret += this.ComputeBackgroundPart(arg);
        //        for (int i = 0; i < normpeak.Count; i++)
        //        {
        //            ret += this.ComputePeakingPart(arg, i);
        //        }
        //        return ret;
        //    }

        //    double ComputeBackgroundPart(double[] arg)
        //    {
        //        double ret = 0.0;
        //        double x = arg[0];
        //        double[] bkgarg = new double[BackgroundPDFParam.Count];

        //        bkgarg[0] = x;
        //        int targetoffset = 1;
        //        int sourceoffset = 2;//one for x and one for nbkg
        //        for (int i = 0; i < BackgroundPDFParam.Count - 1; i++)//-1 for x
        //        {
        //            bkgarg[targetoffset + i] = arg[sourceoffset + i];
        //        }
        //        double nbkg = arg[1];
        //        ret += nbkg * normbkg.Compute(bkgarg);

        //        return ret;
        //    }

        //    Minuit Fitter()
        //    {
        //        Minuit ret = new Minuit(this.Compute, this.param.ToArray());

        //        return ret;
        //    }

        //    double ComputePeakingPart(double[] arg, int ipeak)
        //    {
        //        double ret = 0.0;
        //        double x = arg[0];
        //        int argoffset = 1; //for x
        //        argoffset += 1; //for nbkg
        //        argoffset += BackgroundPDFParam.Count - 1; //bkg param without x
        //        argoffset += nparamperpeak * (ipeak-1);
        //        double[] peakarg = new double[nparamperpeak - 1 + 1]; //-1 for nsig +1 for x
        //        double npeak = arg[argoffset];
        //        peakarg[0] = x;
        //        int targetoffset = 1;
        //        int sourceoffset = argoffset +1; //+1 for npeak
        //        for (int i = 0; i < nparamperpeak - 1; i++)
        //        {
        //            peakarg[targetoffset + i] = arg[sourceoffset + i];
        //        }
        //        ret = npeak * normpeak[ipeak].Compute(peakarg);
        //        return ret;
        //    }

        //}
    }
}
