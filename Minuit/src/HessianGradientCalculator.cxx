// @(#)root/minuit2:$Id: HessianGradientCalculator.cxx 23654 2008-05-06 07:30:34Z moneta $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#include "HessianGradientCalculator.h"
#include "InitialGradientCalculator.h"
#include "MnFcn.h"
#include "MnUserTransformation.h"
#include "MnMachinePrecision.h"
#include "MinimumParameters.h"
#include "FunctionGradient.h"
#include "MnStrategy.h"

#include <math.h>

//#define DEBUG

#if defined(DEBUG) || defined(WARNINGMSG)
#include "MnPrint.h"
#endif


namespace ROOT {

   namespace Minuit2 {


FunctionGradient HessianGradientCalculator::operator()(const MinimumParameters& par) const {
   // use initial gradient as starting point
   InitialGradientCalculator gc(fFcn, fTransformation, fStrategy);
   FunctionGradient gra = gc(par);
   
   return (*this)(par, gra);  
}

FunctionGradient HessianGradientCalculator::operator()(const MinimumParameters& par, const FunctionGradient& Gradient) const {
   // interface of the base class. Use DeltaGradient for op.
   std::pair<FunctionGradient, MnAlgebraicVector> mypair = DeltaGradient(par, Gradient);
   
   return mypair.first;
}

const MnMachinePrecision& HessianGradientCalculator::Precision() const {
   // return the precision
   return fTransformation.Precision();
}

unsigned int HessianGradientCalculator::Ncycle() const {
   // return number of calculation cycles (defined in strategy)
   return Strategy().HessianGradientNCycles();
}

double HessianGradientCalculator::StepTolerance() const {
   // return tolerance on step size (defined in strategy)
   return Strategy().GradientStepTolerance();
}

double HessianGradientCalculator::GradTolerance() const {
   // return gradient tolerance (defines in strategy)
   return Strategy().GradientTolerance();
}

std::pair<FunctionGradient, MnAlgebraicVector> HessianGradientCalculator::DeltaGradient(const MinimumParameters& par, const FunctionGradient& Gradient) const {
   // calculate gradient for Hessian
   assert(par.IsValid());
   
   MnAlgebraicVector x = par.Vec();
   MnAlgebraicVector grd = Gradient.Grad();
   const MnAlgebraicVector& g2 = Gradient.G2();
   //const MnAlgebraicVector& gstep = Gradient.Gstep();
   // update also gradient step sizes
   MnAlgebraicVector gstep = Gradient.Gstep();
   
   double fcnmin = par.Fval();
   //   std::cout<<"fval: "<<fcnmin<<std::endl;
   
   double dfmin = 4.*Precision().Eps2()*(fabs(fcnmin)+Fcn().Up());
   
   unsigned int n = x.size();
   MnAlgebraicVector dgrd(n);
   
   // initial starting values
   for(unsigned int i = 0; i < n; i++) {
      double xtf = x(i);
      double dmin = 4.*Precision().Eps2()*(xtf + Precision().Eps2());
      double epspri = Precision().Eps2() + fabs(grd(i)*Precision().Eps2());
      double optstp = sqrt(dfmin/(fabs(g2(i))+epspri));
      double d = 0.2*fabs(gstep(i));
      if(d > optstp) d = optstp;
      if(d < dmin) d = dmin;
      double chgold = 10000.;
      double dgmin = 0.;
      double grdold = 0.;
      double grdnew = 0.;
      for(unsigned int j = 0; j < Ncycle(); j++)  {
         x(i) = xtf + d;
         double fs1 = Fcn()(x);
         x(i) = xtf - d;
         double fs2 = Fcn()(x);
         x(i) = xtf;
         //       double sag = 0.5*(fs1+fs2-2.*fcnmin);
         //LM: should I calculate also here second derivatives ???

         grdold = grd(i);
         grdnew = (fs1-fs2)/(2.*d);
         dgmin = Precision().Eps()*(fabs(fs1) + fabs(fs2))/d;
         //if(fabs(grdnew) < Precision().Eps()) break;
         if (grdnew == 0) break; 
         double change = fabs((grdold-grdnew)/grdnew);
         if(change > chgold && j > 1) break;
         chgold = change;
         grd(i) = grdnew;
         //LM : update also the step sizes
         gstep(i) = d; 

         if(change < 0.05) break;
         if(fabs(grdold-grdnew) < dgmin) break;
         if(d < dmin) break;
         d *= 0.2;
      }  

      dgrd(i) = std::max(dgmin, fabs(grdold-grdnew));

#ifdef DEBUG
      std::cout << "HGC Param : " << i << "\t new g1 = " << grd(i) << " gstep = " << d << " dgrd = " << dgrd(i) << std::endl;
#endif

   }
   
   return std::pair<FunctionGradient, MnAlgebraicVector>(FunctionGradient(grd, g2, gstep), dgrd);
}

   }  // namespace Minuit2

}  // namespace ROOT
