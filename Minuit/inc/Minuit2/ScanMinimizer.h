// @(#)root/minuit2:$Id: ScanMinimizer.h 21530 2007-12-20 11:14:35Z moneta $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#ifndef ROOT_Minuit2_ScanMinimizer
#define ROOT_Minuit2_ScanMinimizer

#include "MnConfig.h"
#include "ModularFunctionMinimizer.h"
#include "ScanBuilder.h"
#include "SimplexSeedGenerator.h"

#include <vector>

namespace ROOT {

   namespace Minuit2 {

//_____________________________________________________________
/**
   Class implementing the required methods for a minimization using SCAN
   API is provided in the upper ROOT::Minuit2::ModularFunctionMinimizer class
 */

class ScanMinimizer : public ModularFunctionMinimizer {

public:

   ScanMinimizer() : fSeedGenerator(SimplexSeedGenerator()), 
                     fBuilder(ScanBuilder()) {}
  
   ~ScanMinimizer() {}
  
   const MinimumSeedGenerator& SeedGenerator() const {return fSeedGenerator;}
   const MinimumBuilder& Builder() const {return fBuilder;}
  
private:
  
   SimplexSeedGenerator fSeedGenerator;
   ScanBuilder fBuilder;
};

  }  // namespace Minuit2

}  // namespace ROOT

#endif  // ROOT_Minuit2_ScanMinimizer
