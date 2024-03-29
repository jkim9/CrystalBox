// @(#)root/minuit2:$Id: ParametricFunction.cxx 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#include "ParametricFunction.h"
#include "MnFcn.h"
#include "MnStrategy.h"
#include "MnUserParameterState.h"
#include "Numerical2PGradientCalculator.h"
#include "FunctionGradient.h"
#include "MnVectorTransform.h"

namespace ROOT {

   namespace Minuit2 {

//#include "MnPrint.h"



std::vector<double>  ParametricFunction::GetGradient(const std::vector<double>& x) const { 
   // calculate the numerical gradient (using Numerical2PGradientCalculator) 

   //LM:  this I believe is not very efficient
   MnFcn mfcn(*this);
   
   MnStrategy strategy(1);
   
   // ????????? how does it know the transformation????????
   std::vector<double> err(x.size());
   err.assign(x.size(), 0.1);
   // need to use parameters 
   MnUserParameterState st(x, err);
   
   Numerical2PGradientCalculator gc(mfcn, st.Trafo(), strategy);
   FunctionGradient g = gc(x); 
   const MnAlgebraicVector & grad = g.Vec();
   assert( grad.size() == x.size() );
   MnVectorTransform vt; 
   //  std::cout << "Param Function Gradient " << grad << std::endl; 
   return vt( grad ); 
}

   }  // namespace Minuit2

}  // namespace ROOT
