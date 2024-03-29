// @(#)root/minuit2:$Id: VariableMetricMinimizer.h 21530 2007-12-20 11:14:35Z moneta $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#ifndef ROOT_Minuit2_VariableMetricMinimizer
#define ROOT_Minuit2_VariableMetricMinimizer

#include "MnConfig.h"
#include "ModularFunctionMinimizer.h"
#include "MnSeedGenerator.h"
#include "VariableMetricBuilder.h"

namespace ROOT {

   namespace Minuit2 {

//______________________________________________________________________________
/** 
    Instantiates the SeedGenerator and MinimumBuilder for
    Variable Metric Minimization method.
    API is provided in the upper ROOT::Minuit2::ModularFunctionMinimizer class
 
 */

class VariableMetricMinimizer : public ModularFunctionMinimizer {

public:

   VariableMetricMinimizer() : fMinSeedGen(MnSeedGenerator()),
                               fMinBuilder(VariableMetricBuilder()) {}
  
   ~VariableMetricMinimizer() {}

   const MinimumSeedGenerator& SeedGenerator() const {return fMinSeedGen;}
   const MinimumBuilder& Builder() const {return fMinBuilder;}

private:

   MnSeedGenerator fMinSeedGen;
   VariableMetricBuilder fMinBuilder;
};

  }  // namespace Minuit2

}  // namespace ROOT

#endif  // ROOT_Minuit2_VariableMetricMinimizer
