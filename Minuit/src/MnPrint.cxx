// @(#)root/minuit2:$Id: MnPrint.cxx 20880 2007-11-19 11:23:41Z rdm $
// Authors: M. Winkler, F. James, L. Moneta, A. Zsenei   2003-2005  

/**********************************************************************
 *                                                                    *
 * Copyright (c) 2005 LCG ROOT Math team,  CERN/PH-SFT                *
 *                                                                    *
 **********************************************************************/

#include "MnPrint.h"
#include "LAVector.h"
#include "LASymMatrix.h"
#include "FunctionMinimum.h"
#include "MnUserParameters.h"
#include "MnUserCovariance.h"
#include "MnGlobalCorrelationCoeff.h"
#include "MnUserParameterState.h"
#include "MinuitParameter.h"
#include "MnMachinePrecision.h"
#include "MinosError.h"
#include "ContoursError.h"
#include "MnPlot.h"

#include <iomanip>

namespace ROOT {

   namespace Minuit2 {


std::ostream& operator<<(std::ostream& os, const LAVector& vec) {
   // print a vector
   os << "LAVector parameters:" << std::endl;
   { 
      //os << std::endl;
      int nrow = vec.size();
      for (int i = 0; i < nrow; i++) {
         os.precision(8); os.width(15); 
         os << vec(i) << std::endl;
      }
   }
   return os;
}

std::ostream& operator<<(std::ostream& os, const LASymMatrix& matrix) {
   // print a matrix
   os << "LASymMatrix parameters:" << std::endl;
   { 
      //os << std::endl;
      int n = matrix.Nrow();
      for (int i = 0; i < n; i++) {
         for (int j = 0; j < n; j++) {
            os.precision(8); os.width(15); os << matrix(i,j);
         }
         os << std::endl;
      }
   }
   return os;
}

std::ostream& operator<<(std::ostream& os, const MnUserParameters& par) {
   // print the MnUserParameter object
   os << std::endl;
   
   os << "# ext. |" << "|   Name    |" << "|   type  |" << "|     Value     |" << "|  Error +/- " << std::endl;
   
   os << std::endl;
   
   bool atLoLim = false;
   bool atHiLim = false;
   for(std::vector<MinuitParameter>::const_iterator ipar = par.Parameters().begin(); ipar != par.Parameters().end(); ipar++) {
      os << std::setw(4) << (*ipar).Number() << std::setw(5) << "||"; 
      os << std::setw(10) << (*ipar).Name()   << std::setw(3) << "||";
      if((*ipar).IsConst()) {
         os << "  const  ||" << std::setprecision(8) << std::setw(14) << (*ipar).Value() << " ||" << std::endl;
      } else if((*ipar).IsFixed()) {
         os << "  fixed  ||" << std::setprecision(8) << std::setw(14) << (*ipar).Value() << " ||" << std::endl;
      } else if((*ipar).HasLimits()) {
         if((*ipar).Error() > 0.) {
            os << " limited ||" << std::setprecision(8) << std::setw(14) << (*ipar).Value();
            if(fabs((*ipar).Value() - (*ipar).LowerLimit()) < par.Precision().Eps2()) {
               os <<"*";
               atLoLim = true;
            }
            if(fabs((*ipar).Value() - (*ipar).UpperLimit()) < par.Precision().Eps2()) {
               os <<"**";
               atHiLim = true;
            }
            os << " ||" << std::setw(12) << (*ipar).Error() << std::endl;
         } else
            os << "  free   ||" << std::setprecision(8) << std::setw(14) << (*ipar).Value() << " ||" << std::setw(12) << "no" << std::endl;
      } else {
         if((*ipar).Error() > 0.)
            os << "  free   ||" << std::setprecision(8) << std::setw(14) << (*ipar).Value() << " ||" << std::setw(12) << (*ipar).Error() << std::endl;
         else
            os << "  free   ||" << std::setprecision(8) << std::setw(14) << (*ipar).Value() << " ||" << std::setw(12) << "no" << std::endl;
         
      }
   }
   os << std::endl;
   if(atLoLim) os << "* Parameter is at Lower limit" << std::endl;
   if(atHiLim) os << "** Parameter is at Upper limit" << std::endl;
   os << std::endl;
   
   return os;
}

std::ostream& operator<<(std::ostream& os, const MnUserCovariance& matrix) {
   // print the MnUserCovariance
   os << std::endl;
   
   os << "MnUserCovariance: " << std::endl;
   
   { 
      os << std::endl;
      unsigned int n = matrix.Nrow();
      for (unsigned int i = 0; i < n; i++) {
         for (unsigned int j = 0; j < n; j++) {
            os.precision(6); os.width(13); os << matrix(i,j);
         }
         os << std::endl;
      }
   }
   
   os << std::endl;
   os << "MnUserCovariance Parameter correlations: " << std::endl;
   
   { 
      os << std::endl;
      unsigned int n = matrix.Nrow();
      for (unsigned int i = 0; i < n; i++) {
         double di = matrix(i,i);
         for (unsigned int j = 0; j < n; j++) {
            double dj = matrix(j,j);	
            os.precision(6); os.width(13); os << matrix(i,j)/sqrt(fabs(di*dj));
         }
         os << std::endl;
      }
   }
   
   return os;   
}

std::ostream& operator<<(std::ostream& os, const MnGlobalCorrelationCoeff& coeff) {
   // print the global correlation coefficient
   os << std::endl;
   
   os << "MnGlobalCorrelationCoeff: " << std::endl;
   
   { 
      os << std::endl;
      for (unsigned int i = 0; i < coeff.GlobalCC().size(); i++) {
         os.precision(6); os.width(13); os << coeff.GlobalCC()[i];
         os << std::endl;
      }
   }
   
   return os;   
}

std::ostream& operator<<(std::ostream& os, const MnUserParameterState& state) {
   // print the MnUserParameterState
   os << std::endl;
   
   if(!state.IsValid()) {
      os << std::endl;
      os <<"WARNING: MnUserParameterState is not valid."<<std::endl;
      os << std::endl;
   }
   
   os <<"# of function calls: "<<state.NFcn()<<std::endl;
   os <<"function Value: "<< std::setprecision(12) << state.Fval()<<std::endl;
   os <<"expected distance to the Minimum (edm): "<< std::setprecision(8) << state.Edm()<<std::endl;
   os <<"external parameters: "<<state.Parameters()<<std::endl;
   if(state.HasCovariance())
      os <<"covariance matrix: "<<state.Covariance()<<std::endl;
   if(state.HasGlobalCC()) 
      os <<"global correlation coefficients : "<<state.GlobalCC()<<std::endl;
   
   if(!state.IsValid())
      os <<"WARNING: MnUserParameterState is not valid."<<std::endl;
   
   os << std::endl;
   
   return os;
} 

std::ostream& operator<<(std::ostream& os, const FunctionMinimum& min) {
   // print the FunctionMinimum
   os << std::endl;
   if(!min.IsValid()) {
      os <<"WARNING: Minuit did not converge."<<std::endl;
      os << std::endl;
   } else {
      os <<"Minuit did successfully converge."<<std::endl;
   }
   
   os <<"# of function calls: "<<min.NFcn()<<std::endl;
   os <<"minimum function Value: "<< std::setprecision(12) << min.Fval()<<std::endl;
   os <<"minimum edm: "<< std::setprecision(8) << min.Edm()<<std::endl;
   os <<"minimum internal state vector: "<<min.Parameters().Vec()<<std::endl;
   if(min.HasValidCovariance()) 
      os <<"minimum internal covariance matrix: "<<min.Error().Matrix()<<std::endl;
   
   os << min.UserParameters() << std::endl;
   //os << min.UserCovariance() << std::endl;
   //os << min.UserState().GlobalCC() << std::endl;
   
   if(!min.IsValid())
      os <<"WARNING: FunctionMinimum is invalid."<<std::endl;
   
   os << std::endl;
   
   return os;
}

std::ostream& operator<<(std::ostream& os, const MinimumState& min) {
   
   os << std::endl;
   
   os <<"minimum function Value: "<< std::setprecision(12) << min.Fval()<<std::endl;
   os <<"minimum edm: "<< std::setprecision(8) << min.Edm()<<std::endl;
   os <<"minimum internal state vector: "<<min.Vec()<<std::endl;
   os <<"minimum internal Gradient vector: "<<min.Gradient().Vec()<<std::endl;
   if(min.HasCovariance()) 
      os <<"minimum internal covariance matrix: "<<min.Error().Matrix()<<std::endl;
   
   os << std::endl;
   
   return os;
}

std::ostream& operator<<(std::ostream& os, const MnMachinePrecision& prec) {
   // print the Precision
   os << std::endl;
   
   os <<"current machine precision is set to "<<prec.Eps()<<std::endl;
   
   os << std::endl;
   
   return os;
}

std::ostream& operator<<(std::ostream& os, const MinosError& me) {
   // print the Minos Error
   os << std::endl;
   
   os <<"Minos # of function calls: "<<me.NFcn()<<std::endl;
   
   if(!me.IsValid())
      os << "Minos Error is not valid." <<std::endl;
   if(!me.LowerValid())
      os << "lower Minos Error is not valid." <<std::endl;
   if(!me.UpperValid())
      os << "upper Minos Error is not valid." <<std::endl;
   if(me.AtLowerLimit())
      os << "Minos Error is Lower limit of Parameter "<<me.Parameter()<<"." <<std::endl;
   if(me.AtUpperLimit())
      os << "Minos Error is Upper limit of Parameter "<<me.Parameter()<<"." <<std::endl;
   if(me.AtLowerMaxFcn())
      os << "Minos number of function calls for Lower Error exhausted."<<std::endl;
   if(me.AtUpperMaxFcn())
      os << "Minos number of function calls for Upper Error exhausted."<<std::endl;
   if(me.LowerNewMin()) {
      os << "Minos found a new Minimum in negative direction."<<std::endl;
      os << me.LowerState() <<std::endl;
   }
   if(me.UpperNewMin()) {
      os << "Minos found a new Minimum in positive direction."<<std::endl;
      os << me.UpperState() <<std::endl;
   }
   
   os << "# ext. |" << "|   Name    |" << "|   Value@min   |" << "|    negative   |" << "|   positive  " << std::endl;
   os << std::setw(4) << me.Parameter() << std::setw(5) << "||"; 
   os << std::setw(10) << me.LowerState().Name(me.Parameter()) << std::setw(3) << "||";
   os << std::setprecision(8) << std::setw(14) << me.Min() << " ||" << std::setprecision(8) << std::setw(14) << me.Lower() << " ||" << std::setw(14) << me.Upper() << std::endl;
   
   os << std::endl;
   
   return os;
}

std::ostream& operator<<(std::ostream& os, const ContoursError& ce) {
   // print the ContoursError
   os << std::endl;
   os <<"Contours # of function calls: "<<ce.NFcn()<<std::endl;
   os << "MinosError in x: "<<std::endl;
   os << ce.XMinosError() << std::endl;
   os << "MinosError in y: "<<std::endl;
   os << ce.YMinosError() << std::endl;
   MnPlot plot;
   plot(ce.XMin(), ce.YMin(), ce());
   for(std::vector<std::pair<double,double> >::const_iterator ipar = ce().begin(); ipar != ce().end(); ipar++) {
      os << ipar - ce().begin() <<"  "<< (*ipar).first <<"  "<< (*ipar).second <<std::endl;
   }
   os << std::endl;
   
   return os;
}

   }  // namespace Minuit2

}  // namespace ROOT
