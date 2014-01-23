// CAENDigitizerWrapper.h
#pragma once

#include "CAENDPP.h"
#include <vector>
#include "MCAException.h"
#include <msclr\lock.h>
#include "MCAHelper.h"

#define BOARD_REG_FORMAT(x) String::Format("{0:s}(0x{1:X4}) = 0x{2:X8}\n", #x, CAENDPP::x, x);
#define CH_REG_FORMAT(x) String::Format("{0:s}[{1:d}](0x{2:X4}) = 0x{3:X8}\n", #x, Channel, CAENDPP::x(Channel), x);
namespace CAENDigitizerWrapper {
	using namespace System::Collections::Generic;
	using namespace System;
	using namespace System::Diagnostics;
	using namespace System::ComponentModel;
	public ref class HVStatusType{
	public:
		property double Voltage;
		property double Current;
		property double Vmax;
		property bool Powered;
		property bool RampUp;
		property bool RampDown;
		property bool OverCurrent;
		property bool OverVoltage;
		property bool UnderVoltage;
		property bool MaxVoltage;
		property bool MaxCurrent;
		property bool TemperatureWarning;
		property bool Disabled;
		property bool GoingOff;
		HVStatusType(){}
	};

	inline UInt32 us2sp(double us, UInt32 decimation=0){
		//convert micro second to sampling period
		//TODO: put overflow warning here
		return ((UInt32)(us * 100)) >> decimation; //full sampling period = 10ns 
	}

	inline UInt32 us2sp8(double us, UInt32 decimation=0){
		//convert micro second to 8 sampling period
		//TODO: put overflow warning here
		return ((UInt32)(us * 100)) >> (decimation+3); //full sampling period = 10ns 
	}

	inline UInt32 v2ch(double v, double inputrange=1.6){
		//convert voltage to channels
		//TODO: put overflow warning here
		//is this right??? There is an offset...
		return (UInt32)(v/inputrange*0x3FFF);
	}

	public ref class DT5780: INotifyPropertyChanged
	{
	public:

		static const int NCHANNEL=4;
		static const int NREALCHANNEL=2;
		static const int NHVCHANNEL=2;
		static const double SamplingPeriod = 0.01;//us
		static const int NBIT = 14;
		DT5780();
		DT5780(int linknumber);
		~DT5780();
		!DT5780();

		bool ScanConnect();
		void Connect(int linknum);
		UInt32 ReadRegister(UInt32 address, bool shouldthrow);
		UInt32 ReadRegister(UInt32 address); // no throw
		void WriteRegister(UInt32 address, UInt32 value);
		void UseDefaultBoardSetting();
		//void UseDefaultChannelSetting(int channel);
		void Reset();
		void SetChannelEnableMask(UInt32 mask);
		void SetGlobalTriggerMask(UInt32 mask);
		void StartAcquisition();
		void StopAcquisition();
		void ToggleHV(int channel){
			HVPower[channel] = !HVPower[channel];
		}
		property bool IsRunning{ bool get(); }
		property bool CanChangeShaping{ bool get(){ return IsConnected() && !IsRunning; }}
		property bool IsRunnable{ bool get(){ return IsConnected() && !IsRunning; }}
		///<summary>
		///Clear internal buffer
		///</summary>
		void ClearData();
		property bool HasData{ bool get(); }
		void Close();

		enum class DPPAcqMode{
			Oscilloscope = CAEN_DGTZ_DPP_ACQ_MODE_Oscilloscope,
			List = CAEN_DGTZ_DPP_ACQ_MODE_List,
			Mixed = CAEN_DGTZ_DPP_ACQ_MODE_Mixed
		};

		enum class DPPSaveParam{
			EnergyOnly = CAEN_DGTZ_DPP_SAVE_PARAM_EnergyOnly,
			TimeOnly = CAEN_DGTZ_DPP_SAVE_PARAM_TimeOnly,
			EnergyAndTime = CAEN_DGTZ_DPP_SAVE_PARAM_EnergyAndTime,
			ChargeAndTime = CAEN_DGTZ_DPP_SAVE_PARAM_ChargeAndTime,
			None = CAEN_DGTZ_DPP_SAVE_PARAM_None
		};
		
		/// <summary>
		/// Set acquisition mode
		/// </summary>
		void SetDPPAcquisitionMode(DPPAcqMode mode, DPPSaveParam param);

		enum class VirtualProbeMode{
			Single = CAEN_DGTZ_DPP_VIRTUALPROBE_SINGLE,
			Dual = CAEN_DGTZ_DPP_VIRTUALPROBE_DUAL
		};

		enum class VirtualProbe1Mode{
			Input = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE1_Input,
			Delta = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE1_Delta,
			Delta2 = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE1_Delta2,
			Trapezoid = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE1_trapezoid
		};
		
		enum class VirtualProbe2Mode{
			Input = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE2_Input,
			S3 = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE2_S3,
			DigitalCombo = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE2_DigitalCombo,
			TrapBaseLine = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE2_trapBaseline,
			None = CAEN_DGTZ_DPP_PHA_VIRTUALPROBE2_None
		};

		enum class DigitalProbeMode{
			TrgKln = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_trgKln,
			Armed = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_Armed,
			PkRun = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_PkRun,
			PkAbort = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_PkAbort,
			Peaking = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_Peaking,
			PkHoldOff = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_PkHoldOff,
			Flat = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_Flat,
			TrgHoldOff = CAEN_DGTZ_DPP_PHA_DIGITAL_PROBE_trgHoldOff,
		};

		/// <summary>
		/// Set Probe mode
		/// </summary>
		void SetVirtualProbe(VirtualProbeMode mode,
							 VirtualProbe1Mode mode1,
							 VirtualProbe2Mode mode2,
							 DigitalProbeMode dmode);
		
		enum class InputRange:UInt32{
			VPP95=0x0A,
			VPP37=0x09,
			VPP14=0x06,
			VPP06=0x05
		};
		void SetInputRange(int channel, InputRange ir);

		///<summary>
		/// number of sample in the waveform
		///</summary>
		property UInt32 RecordLength{
			UInt32 get();
			void set(UInt32 nsample);
		}

		void SendSWTrigger();

		property int TriggerThreshold[int]{
			int get(int index);
			void set(int index, int value);
		}

		enum class PulsePolarityType{
			Positive = CAEN_DGTZ_PulsePolarityPositive,
			Negative = CAEN_DGTZ_PulsePolarityNegative
		};

		property PulsePolarityType PulsePolarity[int]{
			PulsePolarityType get(int channel);
			void set(int channel, PulsePolarityType p);
		}

		enum class AcquisitionModeType{
			SoftwareControl = CAEN_DGTZ_SW_CONTROLLED,
			SignalInControl = CAEN_DGTZ_S_IN_CONTROLLED
		};

		property AcquisitionModeType AcquisitionMode{
			AcquisitionModeType get();
			void set(AcquisitionModeType acqmode);
		}

		enum class TriggerMode{
			Disabled = CAEN_DGTZ_TRGMODE_DISABLED,
			ExtOutOnly = CAEN_DGTZ_TRGMODE_EXTOUT_ONLY,
			AcqOnly = CAEN_DGTZ_TRGMODE_ACQ_ONLY,
			AcqAndExtOut = CAEN_DGTZ_TRGMODE_ACQ_AND_EXTOUT
		};

		TriggerMode GetSWTriggerMode();
		void SetSWTriggerMode(TriggerMode trgmode);

		TriggerMode GetChannelSelfTriggerMode(int channel);
		void SetChannelSelfTriggerMode(int channel, TriggerMode trgmode);

		TriggerMode GetExtTriggerInputMode();
		void SetExtTriggerInputMode(TriggerMode trgmode);


		//shaping stuff
		//decaytime in us
		property double DecayTime[int]{
			double get(int channel);
			void set(int channel, double us);
		}

		property double TrapezoidRiseTime[int]{
			double get(int channel);
			void set(int channel, double us);
		}

		property double InputRiseTime[int]{
			double get(int channel);
			void set(int channel, double us);
		}

		property double TrapezoidFlatTop[int]{
			double get(int channel);
			void set(int channel, double time);
		}

		property double PeakingTime[int]{
			double get(int channel);
			void set(int channel, double us);
		}

		property double BaselineHoldOff[int]{
			double get(int channel);
			void set(int channel, double us);
		}

		property double PeakHoldOffExtension[int]{
			double get(int channel);
			void set(int channel, double us);
		}

		property UInt32 TriggerFilterSmoothing[int]{
			UInt32 get(int channel);
			void set(int channel, UInt32 value);
		}

		property double TriggerHoldoff[int]{
			double get(int channel);
			void set(int channel, double holdoff);
		}
		///<summary>
		/// number of waveform sample
		///</summary>
		property UInt32 PreTrg[int]{
			UInt32 get(int channel);
			void set(int channel, UInt32 nsample);
		}

		property UInt32 TrapezoidRescaling[int]{
			UInt32 get(int channel){
				return (ReadRegister(CAENDPP::MISC(channel)) & 0x3F);
			};
			void set(int channel, UInt32 scale){
				UInt32 otherold = ReadRegister(CAENDPP::MISC(channel)) & ~0x3F;
				UInt32 towrite = otherold | (scale & 0x3F);
				WriteRegister(CAENDPP::MISC(channel), towrite);
			}
		}

		//set the trapezoid gain and return true gain
		//see Page54 of UM2088 rev2
		//use this after you set the decay time and traprisetime
		double SetTrapezoidGain(int channel, double gain){
			UInt32 k = ReadRegister(CAENDPP::TRAPEZOID_RISE_TIME(channel));
			UInt32 M = ReadRegister(CAENDPP::DECAY_TIME(channel));
			double kM = (double)(k)*M;
			int scaling = (int)(floor(log(kM/gain) / log(2)));
			this->TrapezoidRescaling[channel] = scaling;
			double truegain = kM / (1 << scaling);
			return truegain;
		}

		enum class BaselineAveragingWindowType: UInt32{
			None = 0x0,
			Sample16 = 0x1,
			Sample64 = 0x2,
			Sample128 = 0x3,
			Sample1024 = 0x4,
			Sample4096 = 0x5,
			Sample16384 = 0x6
		};

		property BaselineAveragingWindowType BaselineAveragingWindow[int]{
			BaselineAveragingWindowType get(int channeindowType);
			void set(int channel, BaselineAveragingWindowType value);
		}

		enum class PeakAveragingWindowType: UInt32{
			Sample1 = 0x0,
			Sample4 = 0x1,
			Sample16 = 0x2,
			Sample64 = 0x3
		};

		property PeakAveragingWindowType PeakAveragingWindow[int]{
			PeakAveragingWindowType get(int channel);
			void set(int channel, PeakAveragingWindowType paw);
		}

		BoardStatus^ GetBoardStatus();
		ChannelStatus^ GetChannelStatus(int channel);

		ReadoutData^ ReadData();
		//read data and part nwaveform waveforms
		//<0 = all
		//0 = none
		//>0 read out max(nwaveform, availablewavform)
		ReadoutData^ ReadData(int nwaveform);

		bool IsConnected(){
			bool ret = false;
			try{
				ReadRegister(CAENDPP::DUMMY, true);
				ret = true;
			}
			catch(MCAException^){
				ret = false;
			}
			return ret;
		}

		virtual String^ ToString() override;

		virtual event PropertyChangedEventHandler^ PropertyChanged;
		void OnPropertyChanged(String^ info);

		property double HVVSet[int]{ //unit volt
			double get(int index){
				return ReadRegister(CAENDPP::HV_VSET_ADDR(index))/CAENDPP::HV_VSET_RES;
			}
			void set(int index, double v){
				UInt32 reg = (UInt32)(Math::Round(v*CAENDPP::HV_VSET_RES));
				WriteRegister(CAENDPP::HV_VSET_ADDR(index), reg);
			}
		}

		property double HVISet[int]{ //unit micro amp
			double get(int index){
				return ReadRegister(CAENDPP::HV_ISET_ADDR(index))/CAENDPP::HV_ISET_RES;
			} 
			void set(int index, double I){
				UInt32 reg = (UInt32)(Math::Round(I*CAENDPP::HV_ISET_RES));
				WriteRegister(CAENDPP::HV_ISET_ADDR(index), reg);
			}
		}

		property double HVVMax[int]{
			double get(int index){
				return ReadRegister(CAENDPP::HV_VMAX_ADDR(index))/CAENDPP::HV_VMAX_RES;
			}
			void set(int index, double V){
				UInt32 reg = (UInt32)(Math::Round(V*CAENDPP::HV_VMAX_RES));
				WriteRegister(CAENDPP::HV_VMAX_ADDR(index), reg);
			}
		}

		property bool HVPower[int]{
			bool get(int index){
				UInt32 reg = ReadRegister(CAENDPP::HV_POWER_ADDR(index));
				return (bool)(reg & 0x1 );
			}
			void set(int index, bool on){
				UInt32 reg = on?0x1:0x0;
				WriteRegister(CAENDPP::HV_POWER_ADDR(index), reg);
			}
		}



		property HVStatusType^ HVStatus[int]{
			HVStatusType^ get(int index){
				double vmon = ReadRegister(CAENDPP::HV_VMON_ADDR(index)) / CAENDPP::HV_VMON_RES;
				double imon = ReadRegister(CAENDPP::HV_IMON_ADDR(index)) / CAENDPP::HV_IMON_RES;
				double vmax = ReadRegister(CAENDPP::HV_VMAX_ADDR(index)) / CAENDPP::HV_VMAX_RES;
				UInt32 status = ReadRegister(CAENDPP::HV_STATUS_ADDR(index));
				HVStatusType^ ret = gcnew HVStatusType();
				ret->Voltage = vmon;
				ret->Current = imon;
				ret->Vmax = vmax;
				ret->Powered = (status >> CAENDPP::HV_STATUSBIT_PW) & 0x1;
				ret->RampUp = (status >> CAENDPP::HV_STATUSBIT_RAMPUP) & 0x1;
				ret->RampDown = (status >> CAENDPP::HV_STATUSBIT_RAMPDOWN) & 0x1;
				ret->OverCurrent = (status >> CAENDPP::HV_STATUSBIT_OVCURR) & 0x1;
				ret->OverVoltage = (status >> CAENDPP::HV_STATUSBIT_OVVOLT) & 0x1;
				ret->UnderVoltage = (status >> CAENDPP::HV_STATUSBIT_UNDVOLT) & 0x1;
				ret->MaxVoltage = (status >> CAENDPP::HV_STATUSBIT_MAXV) & 0x1;
				ret->MaxCurrent = (status >> CAENDPP::HV_STATUSBIT_MAXI) & 0x1;
				ret->TemperatureWarning = (status >> CAENDPP::HV_STATUSBIT_TEMPWARN) & 0x1;
				ret->Disabled = (status >> CAENDPP::HV_STATUSBIT_DISABLE) & 0x1;
				ret->GoingOff = (status >> CAENDPP::HV_STATUSBIT_GOINGOFF) & 0x1;
				return ret;
			}
		}


	private:
	
		int Handle;
		bool ValidHandle;
		void BroadcastPropertyChanged();
		Object^ HandleLock;

	};

}
