// @(#)root/minuit2:$Id: LaVtMVSimilarity.cxx 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#include "LASymMatrix.h"
#include "LAVector.h"
#include "LaProd.h"

namespace ROOT {

   namespace Minuit2 {


double mnddot(unsigned int, const double*, int, const double*, int);

double similarity(const LAVector& avec, const LASymMatrix& mat) {
   // calculate the similarity vector-matrix product: V^T M V
   // use matrix product and then dot function (using mnddot)
   
   LAVector tmp = mat*avec;
   
   double value = mnddot(avec.size(), avec.Data(), 1, tmp.Data(), 1);
   return value;
}

   }  // namespace Minuit2

}  // namespace ROOT
