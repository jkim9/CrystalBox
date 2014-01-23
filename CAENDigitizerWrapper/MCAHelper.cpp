#include "stdafx.h"
#include "MCAHelper.h"
#include "CAENDigitizerWrapper.h"

namespace CAENDigitizerWrapper{
	
	ChannelData::ChannelData(int nev){
		this->Event = gcnew array<EventData^>(nev);
		for(int i=0; i<nev; ++i){this->Event[i]=gcnew EventData;}
	}

	List<int>^ ChannelData::Energies(){
		List<int>^ ret = gcnew List<int>;
		for(int i=0; i<Event->Length; i++){
			ret->Add(Event[i]->Energy);
		}
		return ret;
	}

	String^ ChannelData::ToString(){
		String^ ret = "";
		for(int i=0; i<Event->Length; i++){
			ret += Event[i]->ToString();
			ret += "\n";
		}
		return ret;
	}

	String^ EventData::ToString(){
		return String::Format("Time: {0:d8} Energy {1:d8}", TimeTag, Energy);
	}

	ReadoutData^ ReadoutData::FromHandle(int handle){
		return FromHandle(handle, 0);
	}

	ReadoutData^ ReadoutData::FromHandle(int handle, int nwaveform){
		MCAHelper::ReadoutBuffer rb(handle);
		rb.ReadData();
		if(rb.IsEmpty()){
			return gcnew ReadoutData(DT5780::NCHANNEL);
		}
		ReadoutData^ rd = MCAHelper::CreateReadoutData(rb, nwaveform);
		return rd;
	}

	ReadoutData^ MCAHelper::CreateReadoutData(ReadoutBuffer& rb, int nwaveform){
		using namespace std;
		int handle = rb.handle;
		int nchannel = DT5780::NCHANNEL;
		ReadoutData^ rd = gcnew ReadoutData(nchannel);
		DPPEvents dppevents(handle, nchannel);
		std::vector<UInt32> numevents(nchannel);
		int ret = CAEN_DGTZ_GetDPPEvents(handle, rb.buffer, rb.buffersize, dppevents.event_ptr(),
										&numevents[0]);
		if(ret) gcnew MCAException(ret);

		for(int ch=0; ch<nchannel; ch++){
			rd->Channel[ch] = CreateChannelData(handle, dppevents.events[ch], numevents[ch], nwaveform);		
		}
		return rd;
	}
	
	ChannelData^ MCAHelper::CreateChannelData(int handle, CAEN_DGTZ_DPP_PHA_Event_t* events, UInt32 numevents, int nwaveform){
		ChannelData^ cd = gcnew ChannelData(numevents);
		for(int i=0; i<numevents; ++i){
			bool withwaveform = (nwaveform<0) || i<nwaveform;
			cd->Event[i]  = CreateEventData(handle, events[i], withwaveform);
		}
		return cd;
	}
	
	EventData^ MCAHelper::CreateEventData(int handle, CAEN_DGTZ_DPP_PHA_Event_t& ev, bool withwaveform){
		EventData^ ed = gcnew EventData();
		ed->Energy = ev.Energy;
		ed->TimeTag = ev.TimeTag;
		if(withwaveform){
			DPPWaveform wf(handle);
			int ret = CAEN_DGTZ_DecodeDPPWaveforms(handle, &ev, (void*)wf.wf);
			if(ret) throw gcnew MCAException(ret);
			ed->Waveform = CreateWaveformData(*(wf.wf));
		}
		return ed;
	}

	WaveformData^ MCAHelper::CreateWaveformData(const CAEN_DGTZ_DPP_PHA_Waveforms_t& wf){
		WaveformData^ wfd = gcnew WaveformData(wf.Ns);
		wfd->Ns = wf.Ns;
		wfd->DualTrace = wf.DualTrace;
		wfd->VProbe1 = wf.VProbe1;
		wfd->VProbe2 = wf.VProbe2;
		wfd->VDProbe = wf.VDProbe;
		for(int i=0; i<wfd->Ns; ++i){
			wfd->Trace1[i] = wf.Trace1[i];
			wfd->Trace2[i] = wf.Trace2[i];
			wfd->DTrace1[i] = wf.DTrace1[i];
			wfd->DTrace2[i] = wf.DTrace2[i];
		}
		return wfd;
	}

	String^ ChannelStatus::ToString(){
		String ^ret = "";
		ret += CH_REG_FORMAT(CUST_SIZE);
		ret += CH_REG_FORMAT(NEV_AGGREGATE);
		ret += CH_REG_FORMAT(PRE_TRG);
		ret += CH_REG_FORMAT(TRG_FILTER_SMOOTHING);
		ret += CH_REG_FORMAT(INPUT_RISE_TIME);
		ret += CH_REG_FORMAT(TRAPEZOID_RISE_TIME);
		ret += CH_REG_FORMAT(TRAPEZOID_FLAT_TOP);
		ret += CH_REG_FORMAT(PEAKING_TIME);
		ret += CH_REG_FORMAT(DECAY_TIME);
		ret += CH_REG_FORMAT(TRG_THRESHOLD);
		ret += CH_REG_FORMAT(RTD_WINDOW_WIDTH);
		ret += CH_REG_FORMAT(TRG_HOLD_OFF);
		ret += CH_REG_FORMAT(PEAK_HOLD_OFF_EXTENSION);
		ret += CH_REG_FORMAT(BASELINE_HOLD_OFF_EXTENSION);
		ret += CH_REG_FORMAT(MISC);
		return ret;
	}

	String^ BoardStatus::ToString(){
		String^ ret = "";
		ret += BOARD_REG_FORMAT(CONFIG);
		ret += BOARD_REG_FORMAT(ACQUISITION_CONTROL);
		ret += BOARD_REG_FORMAT(ACQUISITION_STATUS);
		ret += BOARD_REG_FORMAT(GLOBAL_TRIGGER_MASK);
		ret += BOARD_REG_FORMAT(TRIGGER_OUT_MASK);
		ret += BOARD_REG_FORMAT(LVDS_DATA);
		ret += BOARD_REG_FORMAT(FRONT_PANEL_CONTROL);
		ret += BOARD_REG_FORMAT(CHANNEL_ENABLE_MASK);
		ret += BOARD_REG_FORMAT(FIRMWARE_REVISION);
		ret += BOARD_REG_FORMAT(BOARD_INFO);
		ret += BOARD_REG_FORMAT(MONITOR_DAC_MODE);
		ret += BOARD_REG_FORMAT(EVENT_SIZE);
		ret += BOARD_REG_FORMAT(VETO);
		ret += BOARD_REG_FORMAT(READOUT_CONTROL);
		ret += BOARD_REG_FORMAT(READOUT_STATUS);
		ret += BOARD_REG_FORMAT(DUMMY);
		return ret;
	}
}