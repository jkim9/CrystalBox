// @(#)root/minuit2:$Id$
// Author: L. Moneta Wed Oct 18 11:48:00 2006

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2006  LCG ROOT Math Team, CERN/PH-SFT                *
 *                                                                    *
 *                                                                    *
 **********************************************************************/

// Header file for class Minuit2Minimizer

#ifndef ROOT_Minuit2_Minuit2Minimizer
#define ROOT_Minuit2_Minuit2Minimizer

#ifndef ROOT_Math_Minimizer
#include "Math/Minimizer.h"
#endif

#ifndef ROOT_Minuit2_MnUserParameterState
#include "MnUserParameterState.h"
#endif

#ifndef ROOT_Math_IFunctionfwd
#include "Math/IFunctionfwd.h"
#endif

#ifndef ROOT_Math_IParamFunctionfwd
#include "Math/IParamFunctionfwd.h"
#endif





namespace ROOT { 

   namespace Minuit2 { 

      class ModularFunctionMinimizer; 
      class FCNBase; 
      class FunctionMinimum;

      // enumeration specifying the type of Minuit2 minimizers
      enum EMinimizerType { 
         kMigrad, 
         kSimplex, 
         kCombined, 
         kScan,
         kFumili
      };

   }

   namespace Minuit2 { 
//_____________________________________________________________________________________________________
/** 
   Minuit2Minimizer class implementing the ROOT::Math::Minimizer interface for 
   Minuit2 minimization algorithm. 
   In ROOT it can be instantiated using the plug-in manager (plug-in "Minuit2")
   Using a string  (used by the plugin manager) or via an enumeration 
   an one can set all the possible minimization algorithms (Migrad, Simplex, Combined, Scan and Fumili).  
*/ 
class Minuit2Minimizer : public ROOT::Math::Minimizer {

public: 

   /** 
      Default constructor
   */ 
   Minuit2Minimizer (ROOT::Minuit2::EMinimizerType type = ROOT::Minuit2::kMigrad); 

   /** 
      Constructor with a char (used by PM) 
   */ 
   Minuit2Minimizer (const char *  type); 

   /** 
      Destructor (no operations)
   */ 
   virtual ~Minuit2Minimizer (); 

private:
   // usually copying is non trivial, so we make this unaccessible

   /** 
      Copy constructor
   */ 
   Minuit2Minimizer(const Minuit2Minimizer &); 

   /** 
      Assignment operator
   */ 
   Minuit2Minimizer & operator = (const Minuit2Minimizer & rhs); 

public: 

   // clear resources (parameters) for consecutives minimizations
   virtual void Clear();

   /// set the function to minimize
   virtual void SetFunction(const ROOT::Math::IMultiGenFunction & func); 

   /// set gradient the function to minimize
   virtual void SetFunction(const ROOT::Math::IMultiGradFunction & func); 

   /// set free variable 
   virtual bool SetVariable(unsigned int ivar, const std::string & name, double val, double step); 

   /// set lower limit variable  (override if minimizer supports them )
   virtual bool SetLowerLimitedVariable(unsigned int  ivar , const std::string & name , double val , double step , double lower );
   /// set upper limit variable (override if minimizer supports them )
   virtual bool SetUpperLimitedVariable(unsigned int ivar , const std::string & name , double val , double step , double upper ); 
   /// set upper/lower limited variable (override if minimizer supports them )
   virtual bool SetLimitedVariable(unsigned int ivar , const std::string & name , double val , double step , double /* lower */, double /* upper */); 
   /// set fixed variable (override if minimizer supports them )
   virtual bool SetFixedVariable(unsigned int /* ivar */, const std::string & /* name */, double /* val */);  

   /// method to perform the minimization
   virtual  bool Minimize(); 

   /// return minimum function value
   virtual double MinValue() const { return fState.Fval(); } 

   /// return expected distance reached from the minimum
   virtual double Edm() const { return fState.Edm(); }

   /// return  pointer to X values at the minimum 
   virtual const double *  X() const { 
      // need to copy them since MnUserParameterState returns them by value
      fValues=fState.Params();
      return &fValues.front(); 
   }

   /// return pointer to gradient values at the minimum 
   virtual const double *  MinGradient() const { return 0; } // not available in Minuit2 

   /// number of function calls to reach the minimum 
   virtual unsigned int NCalls() const { return fState.NFcn(); } 

   /// this is <= Function().NDim() which is the total 
   /// number of variables (free+ constrained ones) 
   virtual unsigned int NDim() const { return fDim; }   

   /// number of free variables (real dimension of the problem) 
   /// this is <= Function().NDim() which is the total 
   virtual unsigned int NFree() const { return fState.VariableParameters(); }  

   /// minimizer provides error and error matrix
   virtual bool ProvidesError() const { return true; } 

   /// return errors at the minimum 
   virtual const double * Errors() const { 
      fErrors = fState.Errors(); 
      return  &fErrors.front(); 
   }

   /** 
       return covariance matrices elements 
       if the variable is fixed or const the value is zero
       The ordering of the variables is the same as in errors and parameter value. 
       This is different from the direct interface of Minuit2 or TMinuit where the 
       values were obtained only to variable parameters
   */ 
   virtual double CovMatrix(unsigned int i, unsigned int j) const;  

   /**
      return correlation coefficient between variable i and j.
      If the variable is fixed or const the return value is zero
    */
   virtual double Correlation(unsigned int i, unsigned int j ) const; 

   /**
      get global correlation coefficient for the variable i. This is a number between zero and one which gives 
      the correlation between the i-th variable  and that linear combination of all other variables which 
      is most strongly correlated with i.
      If the variable is fixed or const the return value is zero
    */
   virtual double GlobalCC(unsigned int i) const;

   /// minos error for parameter i, return false if Minos failed
   virtual bool GetMinosError(unsigned int i, double & errLow, double & errUp); 

   
   /// return reference to the objective function
   ///virtual const ROOT::Math::IGenFunction & Function() const; 


protected: 
   
   // protected function for accessing the internal Minuit2 object. Needed for derived classes

   virtual const ROOT::Minuit2::ModularFunctionMinimizer * GetMinimizer() const { return fMinimizer; } 

   virtual void SetMinimizer( ROOT::Minuit2::ModularFunctionMinimizer * m)  { fMinimizer = m; } 

   void SetMinimizerType( ROOT::Minuit2::EMinimizerType type);

   virtual const  ROOT::Minuit2::FCNBase * GetFCN() const { return fMinuitFCN; } 

   /// examine the minimum result 
   bool ExamineMinimum(const ROOT::Minuit2::FunctionMinimum & min); 

private: 
   
   // dimension of the function to be minimized 
   unsigned int fDim; 
   // error code
   int fErrorCode; 
   bool fUseFumili; 

   ROOT::Minuit2::MnUserParameterState fState;
   // std::vector<ROOT::Minuit2::MinosError> fMinosErrors;
   ROOT::Minuit2::ModularFunctionMinimizer * fMinimizer;
   ROOT::Minuit2::FCNBase * fMinuitFCN;
   ROOT::Minuit2::FunctionMinimum * fMinimum;
   mutable std::vector<double> fValues;
   mutable std::vector<double> fErrors;

}; 

   } // end namespace Fit

} // end namespace ROOT



#endif /* ROOT_Minuit2_Minuit2Minimizer */
