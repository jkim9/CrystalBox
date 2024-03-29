// @(#)root/minuit2:$Id: BasicMinimumSeed.h 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#ifndef ROOT_Minuit2_BasicMinimumSeed
#define ROOT_Minuit2_BasicMinimumSeed

#include "MinimumState.h"
#include "MinimumParameters.h"
#include "MinimumError.h"
#include "FunctionGradient.h"
#include "MnUserTransformation.h"

#include "StackAllocator.h"

namespace ROOT {

   namespace Minuit2 {


//extern StackAllocator gStackAllocator;

class BasicMinimumSeed {

public:
  
  BasicMinimumSeed(const MinimumState& state, const MnUserTransformation& trafo) : fState(state), fTrafo(trafo), fValid(true) {}
  
  ~BasicMinimumSeed() {}

  BasicMinimumSeed(const BasicMinimumSeed& seed) : fState(seed.fState), fTrafo(seed.fTrafo), fValid(seed.fValid) {}
  
  BasicMinimumSeed& operator=(const BasicMinimumSeed& seed) {
    fState = seed.fState;
    fTrafo = seed.fTrafo;
    fValid = seed.fValid;
    return *this;
  }

  void* operator new(size_t nbytes) {
    return StackAllocatorHolder::Get().Allocate(nbytes);
  }
  
  void operator delete(void* p, size_t /*nbytes*/) {
    StackAllocatorHolder::Get().Deallocate(p);
  }

  const MinimumState& State() const {return fState;}
  const MinimumParameters& Parameters() const {return State().Parameters();}
  const MinimumError& Error() const {return State().Error();};
  const FunctionGradient& Gradient() const {return State().Gradient();}
  const MnUserTransformation& Trafo() const {return fTrafo;}
  const MnMachinePrecision& Precision() const {return fTrafo.Precision();}
  double Fval() const {return State().Fval();}
  double Edm() const {return State().Edm();}
  unsigned int NFcn() const {return State().NFcn();}
  bool IsValid() const {return fValid;}

private:

  MinimumState fState;
  MnUserTransformation fTrafo;
  bool fValid;
};

  }  // namespace Minuit2

}  // namespace ROOT

#endif  // ROOT_Minuit2_BasicMinimumSeed
