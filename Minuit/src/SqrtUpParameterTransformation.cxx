// @(#)root/minuit2:$Id: SqrtUpParameterTransformation.cxx 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

// Project   : LCG
// Package   : Minuit
// Author    : Lorenzo.MONETA@cern.ch 
// Created by: moneta  at Thu Apr  8 10:26:22 2004


#include "SqrtUpParameterTransformation.h"
#include "MnMachinePrecision.h"

namespace ROOT {

   namespace Minuit2 {



double SqrtUpParameterTransformation::Int2ext(double value, double upper) const {
   // internal to external transformation
   double val = upper + 1. - sqrt( value*value + 1.);
   return val; 
}


double SqrtUpParameterTransformation::Ext2int(double value, double upper, const MnMachinePrecision& prec) const {
   // external to internal transformation  
   double yy = upper - value + 1.; 
   double yy2 = yy*yy; 
   if (yy2 < (1. + prec.Eps2()) ) 
      return 8*sqrt(prec.Eps2()); 
   else 
      return sqrt( yy2 -1); 
}


double SqrtUpParameterTransformation::DInt2Ext(double value, double) const {
   // derivative of internal to external transofrmation :  d Ext / d Int   
   double val = - value/( sqrt( value*value + 1.) );
   return val; 
}

   }  // namespace Minuit2

}  // namespace ROOT
