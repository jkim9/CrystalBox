// MINUIT.h

#pragma once
#include "FCNBase.h"
#include <vector>
#include <string>
#include <memory>
#include "MnMigrad.h"
#include "MnHesse.h"
#include "MnMinos.h"
#include "MinosError.h"
#include "MnUserParameterState.h"
#include "FunctionMinimum.h"
#include <msclr\marshal_cppstd.h>

typedef double (__stdcall *FCNPtr)(const double*, int);

namespace Minuit2 {
	using namespace System;
	using namespace ROOT::Minuit2;
	using namespace System::Runtime::InteropServices;
	using namespace msclr::interop;
	public delegate double FCN(array<double>^);
	private delegate double FCNWrapper(const double*, int);

	private class InternalFCN: public FCNBase{
	public:
		InternalFCN(FCNPtr f){
			fcn = f;
			this->errordef = 1.0;
		}
		InternalFCN(FCNPtr f, double errordef){
			fcn = f;
			this->errordef = errordef;
		}
		virtual double operator()(const std::vector<double>& x) const{
			//Console::WriteLine("Size: {0}, val{1}",x.size(), x[0]);
			double ret = fcn(&x[0], x.size());
			return ret;
		}
		
		virtual double Up() const{return errordef;}
	private:
		double errordef;
		FCNPtr fcn;
	};
	public ref class MinuitException:Exception{
	public:
		MinuitException():Exception(){}
		MinuitException(String^ msg):Exception(msg){}
	};
	public ref class MinosError{
	public:
		double upper;
		double lower;
		bool uppervalid;
		bool lowervalid;
	};
	public ref class Minuit
	{
	public:
		ref class Limit{
		public:
			Nullable<double> upper;
			Nullable<double> lower;
			Limit(){
				upper = Nullable<double>();
				lower = Nullable<double>();
			}
		};


		// TODO: Put introspection version here
		Minuit(FCN^ f, array<String^>^ paramnames, double errordef){
			Init(f, paramnames, errordef);
		}

		Minuit(FCN^ f, array<String^>^ paramnames){
			Init(f, paramnames, 1.0);
		}

		Minuit(FCN^ f, int nparam){
			array<String^>^ paramnames = gcnew array<String^>(nparam);
			for(int i=0; i<nparam; i++){
				paramnames[i] = String::Format("x_{0:d}", i);
			}
			Init(f, paramnames, 1.0);
		}

		~Minuit(){
			gc.Free();
			delete IntFCN;
			delete CurrentMinimum;
		}

		void migrad(){
			MnStrategy strat(strategy);
			std::auto_ptr<MnUserParameterState> initialstate(GetInitialParameterState());
			std::auto_ptr<MnMigrad> mnmigrad(new MnMigrad(*IntFCN, *(initialstate.get()), strat));
			FunctionMinimum fmin = (*mnmigrad)();
			delete CurrentMinimum;
			CurrentMinimum = new FunctionMinimum(fmin);
			delete CurrentState;
			CurrentState = new MnUserParameterState(CurrentMinimum->UserState());
		}

		void hesse(){
			MnUserParameterState* state = CurrentState;
			if(state == NULL){
				state = GetInitialParameterState();
			}
			MnHesse hesse(strategy);
			MnUserParameterState newstate = hesse(*IntFCN, *state);
			delete state;
			CurrentState = new MnUserParameterState(newstate);
		}

		array<String^>^ Parameters(){
			return Paramnames;
		}

		MinosError^ minos(String^ pname){
			if(CurrentMinimum==NULL || CurrentState==NULL){
				throw gcnew MinuitException("Run migrad first");
			}
			int index = name2index(pname);
			MnMinos minos(*IntFCN, *CurrentMinimum, strategy);
			ROOT::Minuit2::MinosError mne = minos.Minos(index);
			MinosError^ ret  = gcnew MinosError();
			ret->lower = mne.Lower();
			ret->upper = mne.Upper();
			ret->lowervalid = mne.LowerValid();
			ret->uppervalid = mne.UpperValid();
			return ret;
		}

		void SetInitialValue(String^ pname, double initialvalue){
			int index = name2index(pname);
			InitialValues[index] = initialvalue;

		}

		void SetInitialValue(int index, double initialvalue){
			InitialValues[index] = initialvalue;
		}

		void SetInitialStep(String^ pname, double initialerror){
			int index = name2index(pname);
			InitialSteps[index] = initialerror;
		}

		void SetInitialStep(int index, double initialerror){
			InitialSteps[index] = initialerror;
		}

		void SetLimit(String^ pname, double upperlimit, double lowerlimit){
			int index = name2index(pname);
			Limits[index]->upper = upperlimit;
			Limits[index]->lower = lowerlimit;
		}

		void SetLimit(int index, double upperlimit, double lowerlimit){
			Limits[index]->upper = upperlimit;
			Limits[index]->lower = lowerlimit;
		}

		void SetUpperLimit(String^ pname, double upperlimit){
			int index = name2index(pname);
			Limits[index]->upper = upperlimit;
		}

		void SetLowerLimit(String^ pname, double lowerlimit){
			int index = name2index(pname);
			Limits[index]->lower =  lowerlimit;
		}

		double GetValue(String^ pname){
			int index = name2index(pname);
			return GetValue(index);
		}

		double GetValue(int index){
			if(CurrentState==NULL){
				return InitialValues[index];
			}
			else{
				return CurrentState->Value(index);
			}
		}

		double GetFmin(){
			if(CurrentMinimum == NULL){
				throw gcnew MinuitException("Run migrad first");
			}
			return CurrentMinimum->Fval();
		}

		double GetError(String^ pname){
			int index = name2index(pname);
			if(CurrentState==NULL){
				return InitialValues[index];
			}
			else{
				return CurrentState->Error(index);
			}
		}

		bool FitConverged(){
			if(CurrentMinimum==NULL){
				return false;
			}else{
				return CurrentMinimum->IsValid();
			}
		}

		double call(const double* x, int n){
			array<double>^ tmp = gcnew array<double>(n);
			for(int i=0;i<n;i++ ){
				tmp[i] = x[i];
			}
			return fcn(tmp);
		}

	private:
		InternalFCN* IntFCN;
		FunctionMinimum* CurrentMinimum;
		MnUserParameterState* CurrentState;
		FCN^ fcn;
		array<String^>^ Paramnames;
		array<Limit^>^ Limits;
		array<double>^ InitialSteps;
		array<double>^ InitialValues;
		array<bool>^ IsFixed;
		GCHandle gc;
		int nparam;
		int strategy;

		void Init(FCN^ f, array<String^>^ paramnames, double errordef){
			fcn = f;

			nparam = paramnames->Length;
			this->Paramnames = paramnames;
			Limits = gcnew array<Limit^>(nparam);
			InitialSteps = gcnew array<double>(nparam);
			InitialValues = gcnew array<double>(nparam);
			IsFixed = gcnew array<bool>(nparam);
			for(int i=0;i<nparam;i++){
				Limits[i] = gcnew Limit();
				InitialSteps[i] = 1;
				InitialValues[i] = 0;
				IsFixed[i] = false;
			}
			FCNWrapper^ fcnw = gcnew FCNWrapper(this, &Minuit::call);
			gc = GCHandle::Alloc(fcnw);
			IntPtr fptr = Marshal::GetFunctionPointerForDelegate(fcnw);
			IntFCN = new InternalFCN(static_cast<FCNPtr>(fptr.ToPointer()), errordef);
			

			//FCNWrapper^ fcnw = gcnew FCNWrapper(this, &Minuit::call);
			//IntPtr fptr = Marshal::GetFunctionPointerForDelegate(fcnw);
			//IntFCN = new InternalFCN(static_cast<FCNPtr>(fptr.ToPointer()), errordef);
			strategy=1;
			CurrentMinimum = NULL;
			CurrentState = NULL;
		}

		MnUserParameterState* GetInitialParameterState(){
			MnUserParameterState* ups = new MnUserParameterState();
			for(int i=0;i<Paramnames->Length;i++){
				System::String^ p = Paramnames[i];
				std::string paramname = marshal_as<std::string>(p);
				ups->Add(paramname, InitialValues[i], InitialSteps[i]);
				if(IsFixed[i]){
					ups->Fix(paramname);
				}
				Limit^ limit = Limits[i];
				if(limit->upper.HasValue){
					ups->SetUpperLimit(paramname, limit->upper.Value);
				}
				if(limit->lower.HasValue){
					ups->SetLowerLimit(paramname, limit->lower.Value);
				}
			}
			return ups;
		}

		int name2index(String^ str){
			return name2index(str, true);
		}

		int name2index(String^ str, bool shouldthrow){
			for(int i=0;i<Paramnames->Length;i++){
				if(str == Paramnames[i]){
					return i;
				}
			}
			if(shouldthrow) throw gcnew MinuitException("Parameter name lookup failed");
			return -1;
		}
	};

}
