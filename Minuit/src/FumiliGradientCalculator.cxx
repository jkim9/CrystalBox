// @(#)root/minuit2:$Id: FumiliGradientCalculator.cxx 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#include "FumiliGradientCalculator.h"
#include "FumiliFCNBase.h"
#include "MnUserTransformation.h"
#include "FunctionGradient.h"
#include "MinimumParameters.h"
#include "FumiliChi2FCN.h"
#include "FumiliMaximumLikelihoodFCN.h"

//to compare with N2P calculator
//#define DEBUG 1
#ifdef DEBUG
#include "MnPrint.h"
#include "Numerical2PGradientCalculator.h"
#include "MnStrategy.h"
#include "MnUserFcn.h"
#endif

namespace ROOT {

   namespace Minuit2 {


FunctionGradient FumiliGradientCalculator::operator()(const MinimumParameters& par) const {
   
   // Calculate gradient for Fumili using the gradient and Hessian provided by the FCN Fumili function
   // applying the external to int trasformation. 
   
   int nvar = par.Vec().size();
   std::vector<double> extParam = fTransformation(  par.Vec() );
   //   std::vector<double> deriv; 
   //   deriv.reserve( nvar ); 
   //   for (int i = 0; i < nvar; ++i) {
   //     unsigned int ext = fTransformation.ExtOfInt(i);
   //     if ( fTransformation.Parameter(ext).HasLimits()) 
   //       deriv.push_back( fTransformation.DInt2Ext( i, par.Vec()(i) ) );
   //     else 
   //       deriv.push_back(1.0); 
   //   }
   
   // eval Gradient 
   FumiliFCNBase & fcn = const_cast<FumiliFCNBase &>(fFcn);  
   
   fcn.EvaluateAll(extParam);
   
   
   MnAlgebraicVector v(nvar);
   MnAlgebraicSymMatrix h(nvar);
   
   
   const std::vector<double> & fcn_gradient = fFcn.Gradient(); 
   assert( fcn_gradient.size() == extParam.size() ); 
   
   
   //   for (int i = 0; i < nvar; ++i) { 
   //     unsigned int iext = fTransformation.ExtOfInt(i);    
   //     double ideriv = 1.0; 
   //     if ( fTransformation.Parameter(iext).HasLimits()) 
   //       ideriv =  fTransformation.DInt2Ext( i, par.Vec()(i) ) ;
   
   
   //     //     v(i) = fcn_gradient[iext]*deriv;
   //     v(i) = ideriv*fcn_gradient[iext];
   
   //     for (int j = i; j < nvar; ++j) { 
   //       unsigned int jext = fTransformation.ExtOfInt(j);
   //       double jderiv = 1.0; 
   //       if ( fTransformation.Parameter(jext).HasLimits()) 
   // 	jderiv =  fTransformation.DInt2Ext( j, par.Vec()(j) ) ;
   
   // //       h(i,j) = deriv[i]*deriv[j]*fFcn.Hessian(iext,jext); 
   //       h(i,j) = ideriv*jderiv*fFcn.Hessian(iext,jext); 
   //     }
   //   }
   
   
   // cache deriv and Index values . 
   // in large Parameter limit then need to re-optimize and see if better not caching
   
   std::vector<double> deriv(nvar); 
   std::vector<unsigned int> extIndex(nvar); 
   for (int i = 0; i < nvar; ++i) { 
      extIndex[i] = fTransformation.ExtOfInt(i);    
      deriv[i] = 1;
      if ( fTransformation.Parameter(extIndex[i]).HasLimits()) 
         deriv[i] =  fTransformation.DInt2Ext( i, par.Vec()(i) ) ;
      
      v(i) = fcn_gradient[extIndex[i]]*deriv[i];
      
      for (int j = 0; j <= i; ++j) {       
         h(i,j) = deriv[i]*deriv[j]*fFcn.Hessian(extIndex[i],extIndex[j]); 
      }
   }
   
#ifdef DEBUG
   // compare with other Gradient 
   //   // calculate Gradient using Minuit method 
   
   Numerical2PGradientCalculator gc(MnUserFcn(fFcn,fTransformation), fTransformation, MnStrategy(1));
   FunctionGradient g2 = gc(par);
   
   std::cout << "Fumili Gradient " << v << std::endl;
   std::cout << "Minuit Gradient " << g2.Vec() << std::endl;
#endif  
   
   // store calculated Hessian
   fHessian = h; 
   return FunctionGradient(v); 
}

FunctionGradient FumiliGradientCalculator::operator()(const MinimumParameters& par,
                                                      const FunctionGradient&) const

{ 
   // Needed for interface of base class. 
   return this->operator()(par); 
   
}

   }  // namespace Minuit2

}  // namespace ROOT
