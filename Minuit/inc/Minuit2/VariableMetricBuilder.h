// @(#)root/minuit2:$Id: VariableMetricBuilder.h 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#ifndef ROOT_Minuit2_VariableMetricBuilder
#define ROOT_Minuit2_VariableMetricBuilder

#include "MnConfig.h"
#include "MinimumBuilder.h"
#include "VariableMetricEDMEstimator.h"
#include "DavidonErrorUpdator.h"

#include <vector>

namespace ROOT {

   namespace Minuit2 {

/**
   Build (find) function minimum using the Variable Metric method (MIGRAD) 
 */
class VariableMetricBuilder : public MinimumBuilder {

public:

  VariableMetricBuilder() : fEstimator(VariableMetricEDMEstimator()), 
			    fErrorUpdator(DavidonErrorUpdator()) {}

  ~VariableMetricBuilder() {}

  virtual FunctionMinimum Minimum(const MnFcn&, const GradientCalculator&, const MinimumSeed&, const MnStrategy&, unsigned int, double) const;

  FunctionMinimum Minimum(const MnFcn&, const GradientCalculator&, const MinimumSeed&, std::vector<MinimumState> &, unsigned int, double) const;

  const VariableMetricEDMEstimator& Estimator() const {return fEstimator;}
  const DavidonErrorUpdator& ErrorUpdator() const {return fErrorUpdator;}

private:

  VariableMetricEDMEstimator fEstimator;
  DavidonErrorUpdator fErrorUpdator;
};

  }  // namespace Minuit2

}  // namespace ROOT

#endif  // ROOT_Minuit2_VariableMetricBuilder
