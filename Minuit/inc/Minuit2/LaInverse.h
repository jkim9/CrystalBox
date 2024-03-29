// @(#)root/minuit2:$Id: LaInverse.h 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#ifndef ROOT_Minuit2_LaInverse
#define ROOT_Minuit2_LaInverse

/** LAPACK Algebra
    specialize the Invert function for LASymMatrix 
 */

#include "MatrixInverse.h"
#include "LASymMatrix.h"

namespace ROOT {

   namespace Minuit2 {


inline ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double> Inverse(const ABObj<sym, LASymMatrix, double>& obj) {
  return ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>(MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>(obj));
}

template<class T>
inline ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double> operator*(T f, const  ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>& inv) {
  return ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>(inv.Obj(), f*inv.f());
}

template<class T>
inline ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double> operator/(const  ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>& inv, T f) {
  return ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>(inv.Obj(), inv.f()/f);
}

template<class T>
inline ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double> operator-(const  ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>& inv) {
  return ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>(inv.Obj(), T(-1.)*inv.f());
}

int Invert(LASymMatrix&);

int Invert_undef_sym(LASymMatrix&);

/*
template<class M>
inline ABObj<sym, MatrixInverse<sym, ABObj<sym, M, double>, double>, double> Inverse(const ABObj<sym, M, double>& obj) {
  return ABObj<sym, MatrixInverse<sym, ABObj<sym, M, double>, double>, double>(MatrixInverse<sym, ABObj<sym, M, double>, double>(obj));
}

inline ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double> Inverse(const ABObj<sym, LASymMatrix, double>& obj) {
  return ABObj<sym, MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>, double>(MatrixInverse<sym, ABObj<sym, LASymMatrix, double>, double>(obj));
}
*/

  }  // namespace Minuit2

}  // namespace ROOT

#endif  // ROOT_Minuit2_LaInverse
