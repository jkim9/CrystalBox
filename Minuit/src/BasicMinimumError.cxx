// @(#)root/minuit2:$Id: BasicMinimumError.cxx 23522 2008-04-24 15:09:19Z moneta $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#include "BasicMinimumError.h"

#if defined(DEBUG) || defined(WARNINGMSG)
#include "MnPrint.h" 
#endif


namespace ROOT {
   
   namespace Minuit2 {
      
      
      
MnAlgebraicSymMatrix BasicMinimumError::Hessian() const {
   // calculate Heassian: inverse of error matrix 
   MnAlgebraicSymMatrix tmp(fMatrix);
   int ifail = Invert(tmp);
   if(ifail != 0) {
#ifdef WARNINGMSG
      MN_INFO_MSG("BasicMinimumError:  inversion fails; return diagonal matrix.");
#endif
      MnAlgebraicSymMatrix tmp2(fMatrix.Nrow());
      for(unsigned int i = 0; i < fMatrix.Nrow(); i++) {
         tmp2(i,i) = 1./fMatrix(i,i);
      }
      return tmp2;
   }
   return tmp;
}

   }  // namespace Minuit2
   
}  // namespace ROOT
