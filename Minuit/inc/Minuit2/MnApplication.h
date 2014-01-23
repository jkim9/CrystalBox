// @(#)root/minuit2:$Id: MnApplication.h 23654 2008-05-06 07:30:34Z moneta $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#ifndef ROOT_Minuit2_MnApplication
#define ROOT_Minuit2_MnApplication

#include "MnUserParameterState.h"
#include "MnStrategy.h"

namespace ROOT {

   namespace Minuit2 {



class FunctionMinimum;
class MinuitParameter;
class MnMachinePrecision;
class ModularFunctionMinimizer;
class FCNBase;
class FCNGradientBase;

//___________________________________________________________________________
/** 
    application interface class for minimizers (migrad, simplex, Minimize, 
    Scan)
    User normally instantiates the derived class like ROOT::Minuit2::MnMigrad 
    for using Migrad for minimization
 */

class MnApplication {

public:


   /// constructor from non-gradient functions
   MnApplication(const FCNBase& fcn, const MnUserParameterState& state, const MnStrategy& stra, unsigned int nfcn = 0);

   /// constructor from gradient function
   MnApplication(const FCNGradientBase& fcn, const MnUserParameterState& state, const MnStrategy& stra, unsigned int nfcn = 0);

   virtual ~MnApplication() { }

   /// Minimize
   virtual FunctionMinimum operator()(unsigned int = 0, double = 0.1);
 
   virtual const ModularFunctionMinimizer& Minimizer() const = 0;

   const MnMachinePrecision& Precision() const {return fState.Precision();}
   const MnUserParameterState& State() const {return fState;}
   const MnUserParameters& Parameters() const {return fState.Parameters();}
   const MnUserCovariance& Covariance() const {return fState.Covariance();}
   virtual const FCNBase& Fcnbase() const {return fFCN;}
   const MnStrategy& Strategy() const {return fStrategy;}
   unsigned int NumOfCalls() const {return fNumCall;}

protected:

   const FCNBase& fFCN;
   MnUserParameterState fState;
   MnStrategy fStrategy;
   unsigned int fNumCall;
   bool fUseGrad;

public:  

// facade: forward interface of MnUserParameters and MnUserTransformation
// via MnUserParameterState

   //access to parameters (row-wise)
   const std::vector<ROOT::Minuit2::MinuitParameter>& MinuitParameters() const;
   //access to parameters and errors in column-wise representation 
   std::vector<double> Params() const;
   std::vector<double> Errors() const;

   //access to single Parameter
   const MinuitParameter& Parameter(unsigned int i) const;

   //add free Parameter
   void Add(const char* Name, double val, double err);
   //add limited Parameter
   void Add(const char* Name, double val, double err, double , double);
   //add const Parameter
   void Add(const char*, double);

   //interaction via external number of Parameter
   void Fix(unsigned int);
   void Release(unsigned int);
   void SetValue(unsigned int, double);
   void SetError(unsigned int, double);
   void SetLimits(unsigned int, double, double);
   void RemoveLimits(unsigned int);

   double Value(unsigned int) const;
   double Error(unsigned int) const;
  
   //interaction via Name of Parameter
   void Fix(const char*);
   void Release(const char*);
   void SetValue(const char*, double);
   void SetError(const char*, double);
   void SetLimits(const char*, double, double);
   void RemoveLimits(const char*);
   void SetPrecision(double);

   double Value(const char*) const;
   double Error(const char*) const;
  
   //convert Name into external number of Parameter
   unsigned int Index(const char*) const;
   //convert external number into Name of Parameter
   const char* Name(unsigned int) const;

   // transformation internal <-> external
   double Int2ext(unsigned int, double) const;
   double Ext2int(unsigned int, double) const;
   unsigned int IntOfExt(unsigned int) const;
   unsigned int ExtOfInt(unsigned int) const;
   unsigned int VariableParameters() const;

};

  }  // namespace Minuit2

}  // namespace ROOT

#endif  // ROOT_Minuit2_MnApplication
