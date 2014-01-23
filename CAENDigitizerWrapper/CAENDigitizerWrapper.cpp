// This is the main DLL file.

#include "Stdafx.h"
#include "CAENDigitizerType.h"
#include "CAENDigitizer.h"
#include "CAENDPP.h"

////////////////////////////////////
/////DO NOT MOVE THIS INCLUDE UP////
////VC++ is really stupid at parsing the header files///
#include "CAENDigitizerWrapper.h"

namespace CAENDigitizerWrapper{
	using namespace System::Diagnostics;
	
	DT5780::DT5780():Handle(0), ValidHandle(false){
	}
	

	DT5780::DT5780(int linknum):Handle(0), ValidHandle(false){
		Connect(linknum);
	}
	
	DT5780::~DT5780(){this->!DT5780();}
	DT5780::!DT5780(){Close();}

	bool DT5780::ScanConnect(){
		bool foundone = false;
		for(int i=0;i<20;i++){
			try{
				Connect(i);
				foundone = true;
				break;
			}
			catch (MCAException^ e){
				//do nothing
				if(e->ErrorCode == MCAException::ErrorCodeType::DigitizerAlreadyOpen){
					throw;
				}
			}
		}
		return foundone;
	}

	void DT5780::Connect(int linknum){
		int tmp_handle = 0;
		int ret = CAEN_DGTZ_OpenDigitizer(CAEN_DGTZ_USB, linknum, 0 , 0, &tmp_handle);
		if(ret) throw gcnew MCAException(ret);
		Handle = tmp_handle;
		ValidHandle = !ret;
		HandleLock = gcnew Object();
		BroadcastPropertyChanged();
	}

	void DT5780::SetDPPAcquisitionMode(DPPAcqMode mode, DPPSaveParam param){
		int ret = CAEN_DGTZ_SetDPPAcquisitionMode(Handle, 
						(CAEN_DGTZ_DPP_AcqMode_t)mode,
						(CAEN_DGTZ_DPP_SaveParam_t)param);
		if(ret) throw gcnew MCAException(ret);
	}


	void DT5780::UseDefaultBoardSetting(){
		Reset();
		this->SetChannelEnableMask(0x3); //Enable both Channel
		this->SetVirtualProbe(VirtualProbeMode::Dual,
							  VirtualProbe1Mode::Trapezoid,
							  VirtualProbe2Mode::Input,
							  DigitalProbeMode::Peaking);
		this->SetDPPAcquisitionMode(DPPAcqMode::Mixed, DPPSaveParam::EnergyAndTime);
		this->RecordLength = 1000;
		this->AcquisitionMode = AcquisitionModeType::SoftwareControl;
		for(int channel=0; channel<DT5780::NREALCHANNEL; channel++){
			this->PulsePolarity[channel] = PulsePolarityType::Negative;
			this->SetInputRange(channel, InputRange::VPP14);
			this->TriggerThreshold[channel] = 1000;
			this->PreTrg[channel] = 50;
			
			//Setting
			this->DecayTime[channel] = 110; //us
			this->TrapezoidRiseTime[channel] = 2.7; //us
			this->PeakingTime[channel] = 1; //us
			this->InputRiseTime[channel] = .1; //us
			this->TrapezoidFlatTop[channel] = 2; //us
			this->BaselineHoldOff[channel] = 0.1; //us
			this->PeakHoldOffExtension[channel] = 0.2; //us
			this->TriggerFilterSmoothing[channel] = 8; //sample
			this->TriggerHoldoff[channel] = 0; //us
			this->BaselineAveragingWindow[channel] = BaselineAveragingWindowType::Sample64;
			this->PeakAveragingWindow[channel] = PeakAveragingWindowType::Sample64;
		}

		for(int hvchannel=0; hvchannel<DT5780::NHVCHANNEL; hvchannel++){
			this->HVVMax[hvchannel] = 500;
			this->HVPower[hvchannel] = false;
			this->HVISet[hvchannel] = 10;
			this->HVVSet[hvchannel] = 10;
		}

		BroadcastPropertyChanged();
	}


	void DT5780::ClearData(){
		int ret = CAEN_DGTZ_ClearData(Handle);
		if(ret) throw gcnew MCAException(ret);
	}


	BoardStatus^ DT5780::GetBoardStatus(){
		BoardStatus^ bs = gcnew BoardStatus();
		bs->CONFIG = ReadRegister(CAENDPP::CONFIG);
		bs->ACQUISITION_CONTROL = ReadRegister(CAENDPP::ACQUISITION_CONTROL);
		bs->ACQUISITION_STATUS = ReadRegister(CAENDPP::ACQUISITION_STATUS);
		bs->GLOBAL_TRIGGER_MASK = ReadRegister(CAENDPP::GLOBAL_TRIGGER_MASK);
		bs->TRIGGER_OUT_MASK = ReadRegister(CAENDPP::TRIGGER_OUT_MASK);
		bs->LVDS_DATA = ReadRegister(CAENDPP::LVDS_DATA);
		bs->FRONT_PANEL_CONTROL = ReadRegister(CAENDPP::FRONT_PANEL_CONTROL);
		bs->CHANNEL_ENABLE_MASK = ReadRegister(CAENDPP::CHANNEL_ENABLE_MASK);
		bs->FIRMWARE_REVISION = ReadRegister(CAENDPP::FIRMWARE_REVISION);
		bs->BOARD_INFO = ReadRegister(CAENDPP::BOARD_INFO);
		bs->MONITOR_DAC_MODE = ReadRegister(CAENDPP::MONITOR_DAC_MODE);
		bs->EVENT_SIZE = ReadRegister(CAENDPP::EVENT_SIZE);
		bs->VETO = ReadRegister(CAENDPP::VETO);
		bs->READOUT_CONTROL = ReadRegister(CAENDPP::READOUT_CONTROL);
		bs->READOUT_STATUS = ReadRegister(CAENDPP::READOUT_STATUS);
		bs->DUMMY = ReadRegister(CAENDPP::DUMMY);
		return bs;
	}


	ChannelStatus^ DT5780::GetChannelStatus(int channel){
		ChannelStatus^ cs = gcnew ChannelStatus();
		cs->Channel=channel;
		cs->CUST_SIZE = ReadRegister(CAENDPP::CUST_SIZE(channel));
		cs->NEV_AGGREGATE = ReadRegister(CAENDPP::NEV_AGGREGATE(channel));
		cs->PRE_TRG = ReadRegister(CAENDPP::PRE_TRG(channel));
		cs->TRG_FILTER_SMOOTHING = ReadRegister(CAENDPP::TRG_FILTER_SMOOTHING(channel));
		cs->INPUT_RISE_TIME = ReadRegister(CAENDPP::INPUT_RISE_TIME(channel));
		cs->TRAPEZOID_RISE_TIME = ReadRegister(CAENDPP::TRAPEZOID_RISE_TIME(channel));
		cs->TRAPEZOID_FLAT_TOP = ReadRegister(CAENDPP::TRAPEZOID_FLAT_TOP(channel));
		cs->PEAKING_TIME = ReadRegister(CAENDPP::PEAKING_TIME(channel));
		cs->DECAY_TIME = ReadRegister(CAENDPP::DECAY_TIME(channel));
		cs->TRG_THRESHOLD = ReadRegister(CAENDPP::TRG_THRESHOLD(channel));
		cs->RTD_WINDOW_WIDTH = ReadRegister(CAENDPP::RTD_WINDOW_WIDTH(channel));
		cs->TRG_HOLD_OFF = ReadRegister(CAENDPP::TRG_HOLD_OFF(channel));
		cs->PEAK_HOLD_OFF_EXTENSION = ReadRegister(CAENDPP::PEAK_HOLD_OFF_EXTENSION(channel));
		cs->BASELINE_HOLD_OFF_EXTENSION = ReadRegister(CAENDPP::BASELINE_HOLD_OFF_EXTENSION(channel));
		cs->MISC = ReadRegister(CAENDPP::MISC(channel));
		return cs;
	}


	void DT5780::Reset(){
		if (IsRunning){ StopAcquisition(); }
		WriteRegister(CAENDPP::SOFTWARE_RESET, 0);
		//event format is necessary for readout parsing
		//write event format bit
		//somehow reset didn't get this right.
		WriteRegister(CAENDPP::CONFIG_SET, 1<<24);
		//Setting trapezoid rescaling
		WriteRegister(CAENDPP::MISC(0), 0x04410013);
		WriteRegister(CAENDPP::MISC(1), 0x04410013);
		BroadcastPropertyChanged();
	}

	//CLI is stupid in default interface so we need two
	UInt32 DT5780::ReadRegister(UInt32 address, bool shouldthrow){
		UInt32 tmp=0;
		int ret = CAEN_DGTZ_ReadRegister(Handle, address, &tmp);
		if (ret && shouldthrow) throw gcnew MCAException(ret);
		return tmp;
	}

	UInt32 DT5780::ReadRegister(UInt32 address){
		return ReadRegister(address, false);
	}


	ReadoutData^ DT5780::ReadData(){
		return ReadData(0);
	}


	ReadoutData^ DT5780::ReadData(int withwaveform){
		return ReadoutData::FromHandle(Handle, withwaveform);
	}
	

	void DT5780::WriteRegister(UInt32 address, UInt32 value){
		if(!IsRunning){
			int ret = CAEN_DGTZ_WriteRegister(Handle, address, value);
			if(ret) throw gcnew MCAException(ret);
		}
		else{
			throw gcnew MCAException((int)MCAException::ErrorCodeType::WriteDeviceRegisterFail,
							gcnew String("Cannnot write while acquiring"));
		}
	}


	void DT5780::Close(){
		if(ValidHandle){
			this->HVPower[0] = false;
			this->HVPower[1] = false;
			int err = CAEN_DGTZ_CloseDigitizer(Handle);
			if(err) throw gcnew MCAException(err);
			Handle = 0;
			ValidHandle = false;
		}
	}


	void DT5780::SetVirtualProbe(VirtualProbeMode mode,
						VirtualProbe1Mode mode1,
						VirtualProbe2Mode mode2,
						DigitalProbeMode dmode){
		int ret = CAEN_DGTZ_SetDPP_PHA_VirtualProbe(Handle,
				(CAEN_DGTZ_DPP_VirtualProbe_t)mode,
				(CAEN_DGTZ_DPP_PHA_VirtualProbe1_t)mode1,
				(CAEN_DGTZ_DPP_PHA_VirtualProbe2_t)mode2,
				(CAEN_DGTZ_DPP_PHA_DigitalProbe_t)dmode);
		if(ret) throw gcnew MCAException(ret);
	}


	UInt32 DT5780::RecordLength::get(){
		UInt32 tmp = 0;
		int ret = CAEN_DGTZ_GetRecordLength(Handle, &tmp);
		if (ret) throw gcnew MCAException(ret);
		return tmp;
	}


	void  DT5780::RecordLength::set(UInt32 nsample){
		int ret = CAEN_DGTZ_SetRecordLength(Handle, nsample);
		if(ret) throw gcnew MCAException(ret);
		ret = CAEN_DGTZ_SetDPPEventAggregation(Handle, 0, 0);
		if(ret) throw gcnew MCAException(ret);
	}


	void DT5780::SendSWTrigger(){
		int ret = CAEN_DGTZ_SendSWtrigger(Handle);
		if(ret) throw gcnew MCAException(ret);
	}

	DT5780::TriggerMode DT5780::GetSWTriggerMode(){
		CAEN_DGTZ_TriggerMode_t trgmode;
		int ret = CAEN_DGTZ_GetSWTriggerMode(Handle, &trgmode);
		if (ret) throw gcnew MCAException(ret);
		return (TriggerMode)trgmode;
	}

	void DT5780::SetSWTriggerMode(TriggerMode trgmode){
		int ret = CAEN_DGTZ_SetSWTriggerMode(Handle, (CAEN_DGTZ_TriggerMode_t)trgmode);
		if (ret) throw gcnew MCAException(ret);
	}

	DT5780::TriggerMode DT5780::GetChannelSelfTriggerMode(int channel){
		CAEN_DGTZ_TriggerMode_t trgmode;
		int ret = CAEN_DGTZ_GetChannelSelfTrigger(Handle, channel, &trgmode);
		if (ret) throw gcnew MCAException(ret);
		return (TriggerMode)trgmode;
	}
	void DT5780::SetChannelSelfTriggerMode(int channel, TriggerMode trgmode){
		int ret = CAEN_DGTZ_SetChannelSelfTrigger(Handle, (CAEN_DGTZ_TriggerMode_t)trgmode, 1 << channel);
		if (ret) throw gcnew MCAException(ret);
	}

	DT5780::TriggerMode DT5780::GetExtTriggerInputMode(){
		CAEN_DGTZ_TriggerMode_t trgmode;
		int ret = CAEN_DGTZ_GetExtTriggerInputMode(Handle, &trgmode);
		if (ret) throw gcnew MCAException(ret);
		return (TriggerMode)trgmode;
	}

	void DT5780::SetExtTriggerInputMode(TriggerMode trgmode){
		int ret = CAEN_DGTZ_SetExtTriggerInputMode(Handle, (CAEN_DGTZ_TriggerMode_t)trgmode);
		if (ret) throw gcnew MCAException(ret);
	}

	int DT5780::TriggerThreshold::get(int index){
		//CAEN DGTZ settrigger seems to bug out?
		return ReadRegister(CAENDPP::TRG_THRESHOLD(index));
	}


	void DT5780::TriggerThreshold::set(int index, int value){
		WriteRegister(CAENDPP::TRG_THRESHOLD(index), (UInt32)value);
	}


	DT5780::PulsePolarityType DT5780::PulsePolarity::get(int channel){
		CAEN_DGTZ_PulsePolarity_t p;
		int ret = CAEN_DGTZ_GetChannelPulsePolarity(Handle, channel, &p);
		if(ret) throw gcnew MCAException(ret);
		return (PulsePolarityType)p;
	}
	
	
	void DT5780::PulsePolarity::set(int channel, PulsePolarityType p){
		int ret = CAEN_DGTZ_SetChannelPulsePolarity(Handle, channel, (CAEN_DGTZ_PulsePolarity_t)p);
		if(ret) throw gcnew MCAException(ret);
	}


	void DT5780::SetChannelEnableMask(UInt32 mask){
		int ret = CAEN_DGTZ_SetChannelEnableMask(Handle, mask);
		if(ret) throw gcnew MCAException(ret);
	}


	void DT5780::SetGlobalTriggerMask(UInt32 mask){
		WriteRegister(CAENDPP::GLOBAL_TRIGGER_MASK, mask);
	}


	DT5780::AcquisitionModeType DT5780::AcquisitionMode::get(){
		CAEN_DGTZ_AcqMode_t acqmode;
		int ret = CAEN_DGTZ_GetAcquisitionMode(Handle, &acqmode);
		if(ret) throw gcnew MCAException(ret);
		return (AcquisitionModeType)acqmode;
	}


	void DT5780::AcquisitionMode::set(AcquisitionModeType acqmode){
		int ret = CAEN_DGTZ_SetAcquisitionMode(Handle, (CAEN_DGTZ_AcqMode_t) acqmode);
		if(ret) throw gcnew MCAException(ret);
	}


	void DT5780::StartAcquisition(){
		int ret = CAEN_DGTZ_SWStartAcquisition(Handle);
		if(ret) throw gcnew MCAException(ret);
		OnPropertyChanged("IsRunning");
		OnPropertyChanged("IsRunnable");
		OnPropertyChanged("CanChangeShaping");
	}
	
	
	void DT5780::StopAcquisition(){
		int ret = CAEN_DGTZ_SWStopAcquisition(Handle);
		if(ret) throw gcnew MCAException(ret);
		OnPropertyChanged("IsRunning");
		OnPropertyChanged("IsRunnable");
		OnPropertyChanged("CanChangeShaping");
	}


	bool DT5780::IsRunning::get(){
		return IsConnected() && ((ReadRegister(CAENDPP::ACQUISITION_STATUS) & 0x4) != 0);
	}


	void DT5780::SetInputRange(int channel, InputRange ir){
		WriteRegister(CAENDPP::INPUT_RANGE(channel), (UInt32)ir);
	}


	bool DT5780::HasData::get(){
		UInt32 status = ReadRegister(CAENDPP::ACQUISITION_STATUS);
		return (bool)((status & 0x8) >> 3);
	}


	String^ DT5780::ToString(){
		String^ ret = GetBoardStatus()->ToString();
		ret += "\n";
		ret += GetChannelStatus(0)->ToString();
		ret += "\n";
		ret += GetChannelStatus(1)->ToString();
		return ret;
	}


	double DT5780::DecayTime::get(int channel){
		return ReadRegister(CAENDPP::DECAY_TIME(channel))*SamplingPeriod;
	}
	
	
	void DT5780::DecayTime::set(int channel, double us){
		WriteRegister(CAENDPP::DECAY_TIME(channel), us2sp(us));
	}


	double DT5780::TrapezoidRiseTime::get(int channel){
		return ReadRegister(CAENDPP::TRAPEZOID_RISE_TIME(channel))*SamplingPeriod;
	}
	
	
	void DT5780::TrapezoidRiseTime::set(int channel, double us){
		WriteRegister(CAENDPP::TRAPEZOID_RISE_TIME(channel), us2sp(us));
	}


	double DT5780::InputRiseTime::get(int channel){
		return ReadRegister(CAENDPP::INPUT_RISE_TIME(channel))*SamplingPeriod;
	}
	
	
	void DT5780::InputRiseTime::set(int channel, double us){
		WriteRegister(CAENDPP::INPUT_RISE_TIME(channel), us2sp(us));
	}
	

	double DT5780::TrapezoidFlatTop::get(int channel){
		return ReadRegister(CAENDPP::TRAPEZOID_FLAT_TOP(channel))*SamplingPeriod;
	}
	
	
	void DT5780::TrapezoidFlatTop::set(int channel, double time){
		WriteRegister(CAENDPP::TRAPEZOID_FLAT_TOP(channel), us2sp(time));
	}

		
	double DT5780::PeakingTime::get(int channel){
		return ReadRegister(CAENDPP::PEAKING_TIME(channel))*SamplingPeriod;
	}
	
	
	void DT5780::PeakingTime::set(int channel, double us){
		WriteRegister(CAENDPP::PEAKING_TIME(channel), us2sp(us));
	}

		
	double DT5780::BaselineHoldOff::get(int channel){
		return 8*ReadRegister(CAENDPP::BASELINE_HOLD_OFF_EXTENSION(channel))*SamplingPeriod;
	}
	
	
	void DT5780::BaselineHoldOff::set(int channel, double us){
		WriteRegister(CAENDPP::BASELINE_HOLD_OFF_EXTENSION(channel), us2sp8(us));
	}
	

	double DT5780::PeakHoldOffExtension::get(int channel){
		return 8*ReadRegister(CAENDPP::PEAK_HOLD_OFF_EXTENSION(channel))*SamplingPeriod;
	}
	
	
	void DT5780::PeakHoldOffExtension::set(int channel, double us){
		WriteRegister(CAENDPP::PEAK_HOLD_OFF_EXTENSION(channel), us2sp8(us));
	}

	
	UInt32 DT5780::TriggerFilterSmoothing::get(int channel){
		return ReadRegister(CAENDPP::TRG_FILTER_SMOOTHING(channel));
	}
	
	
	void DT5780::TriggerFilterSmoothing::set(int channel, UInt32 value){
		WriteRegister(CAENDPP::TRG_FILTER_SMOOTHING(channel), value);
	}


	double DT5780::TriggerHoldoff::get(int channel){
		return 8*ReadRegister(CAENDPP::TRG_HOLD_OFF(channel))*SamplingPeriod;
	}
	
	
	void DT5780::TriggerHoldoff::set(int channel, double holdoff){
		WriteRegister(CAENDPP::TRG_HOLD_OFF(channel), us2sp8(holdoff));
	}


	UInt32 DT5780::PreTrg::get(int channel){
		return ReadRegister(CAENDPP::PRE_TRG(channel));
	}
	

	void DT5780::PreTrg::set(int channel, UInt32 nsample){
		WriteRegister(CAENDPP::PRE_TRG(channel), nsample);
	}


	DT5780::BaselineAveragingWindowType DT5780::BaselineAveragingWindow::get(int channel){
		UInt32 misc = ReadRegister(CAENDPP::MISC(channel));
		UInt32 mask = 0x00700000;
		UInt32 shift = 20;
		UInt32 baw = (misc & mask) >> shift;
		return (BaselineAveragingWindowType) baw;
	}


	void DT5780::BaselineAveragingWindow::set(int channel, BaselineAveragingWindowType value){
		UInt32 misc = ReadRegister(CAENDPP::MISC(channel));
		UInt32 mask = 0x00700000;
		UInt32 shift = 20;
		UInt32 baw = (UInt32)value;
		misc = (misc & ~mask) | (baw << shift);
		WriteRegister(CAENDPP::MISC(channel), misc);
	}


	DT5780::PeakAveragingWindowType DT5780::PeakAveragingWindow::get(int channel){
		UInt32 misc = ReadRegister(CAENDPP::MISC(channel));
		UInt32 mask = 0x00003000;
		UInt32 shift = 12;	
		PeakAveragingWindowType ret = (PeakAveragingWindowType)((misc & mask) >> shift);
		return ret;
	}


	void DT5780::PeakAveragingWindow::set(int channel, PeakAveragingWindowType paw){
		UInt32 misc = ReadRegister(CAENDPP::MISC(channel));
		UInt32 mask = 0x00003000;
		UInt32 shift = 12;
		misc = (misc & ~mask) | ((UInt32)paw << shift);
		WriteRegister(CAENDPP::MISC(channel), misc);
	}


	void DT5780::BroadcastPropertyChanged(){
		array<String^>^ plist = gcnew array<String^>{
			"TriggerThreshold",
			"PulsePolarity",
			"AcquisitionMode",
			"DecayTime",
			"RecordLength",
			"TrapezoidRiseTime",
			"InputRiseTime",
			"TrapezoidFlatTop",
			"PeakingTime",
			"BaselineHoldOff",
			"PeakHoldOffExtension",
			"TriggerFilterSmoothing",
			"TriggerHoldoff",
			"TrapezoidRescaling",
			"BaselineAveragingWindow",
			"PeakAveragingWindow",
			"IsRunning",
			"IsRunnable",
			"CanChangeShaping",
			"HVVSet",
			"HVISet",
			"HVVMax",
			"HVPower",
			"HVStatus"
		};
		for(int i=0; i<plist->Length; i++){
			OnPropertyChanged(plist[i]);
		}
	}


	void DT5780::OnPropertyChanged(String^ info)
	{
		PropertyChanged(this, gcnew PropertyChangedEventArgs(info));
	}




}