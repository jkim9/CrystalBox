// @(#)root/minuit2:$Id: MnFunctionCross.h 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#ifndef ROOT_Minuit2_MnFunctionCross
#define ROOT_Minuit2_MnFunctionCross

#include "MnConfig.h"
#include <vector>

namespace ROOT {

   namespace Minuit2 {



class FCNBase;
class MnUserParameterState;
class MnStrategy;
class MnCross;

/**
   MnFunctionCross 
*/

class MnFunctionCross {

public:
  
  MnFunctionCross(const FCNBase& fcn, const MnUserParameterState& state, double fval, const MnStrategy& stra) : fFCN(fcn), fState(state), fFval(fval), fStrategy(stra) {} 
  
  ~MnFunctionCross() {}
  
  MnCross operator()(const std::vector<unsigned int>&, const std::vector<double>&, const std::vector<double>&, double, unsigned int) const;

private:

  const FCNBase& fFCN;
  const MnUserParameterState& fState;
  double fFval;
  const MnStrategy& fStrategy;
};

  }  // namespace Minuit2

}  // namespace ROOT

#endif  // ROOT_Minuit2_MnFunctionCross
