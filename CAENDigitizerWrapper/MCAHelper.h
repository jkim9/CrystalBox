#pragma once
#include "CAENDigitizer.h"
#include "CAENDigitizerType.h"
#include "MCAException.h"
#include <vector>
#include <random>

namespace CAENDigitizerWrapper{
	using namespace System;
	using namespace System::Collections::Generic;
	ref class DT5780;

	public ref class BoardStatus{
	public:
		UInt32 CONFIG;
		UInt32 ACQUISITION_CONTROL;
		UInt32 ACQUISITION_STATUS;
		UInt32 GLOBAL_TRIGGER_MASK;
		UInt32 TRIGGER_OUT_MASK;
		UInt32 LVDS_DATA;
		UInt32 FRONT_PANEL_CONTROL;
		UInt32 CHANNEL_ENABLE_MASK;
		UInt32 FIRMWARE_REVISION;

		UInt32 BOARD_INFO;
		UInt32 MONITOR_DAC_MODE;
		UInt32 EVENT_SIZE;
		UInt32 VETO;
		UInt32 READOUT_CONTROL;
		UInt32 READOUT_STATUS;
		UInt32 DUMMY;

		virtual String^ ToString() override;
	};

	public ref class ChannelStatus{
	public:
		int Channel;
		UInt32 CUST_SIZE;
		UInt32 NEV_AGGREGATE;
		UInt32 PRE_TRG;
		UInt32 TRG_FILTER_SMOOTHING;
		UInt32 INPUT_RISE_TIME;
		UInt32 TRAPEZOID_RISE_TIME;
		UInt32 TRAPEZOID_FLAT_TOP;
		UInt32 PEAKING_TIME;
		UInt32 DECAY_TIME;
		UInt32 TRG_THRESHOLD;
		UInt32 RTD_WINDOW_WIDTH;
		UInt32 TRG_HOLD_OFF;
		UInt32 PEAK_HOLD_OFF_EXTENSION;
		UInt32 BASELINE_HOLD_OFF_EXTENSION;
		UInt32 MISC;
		UInt32 INPUT_RANGE;

		virtual String^ ToString() override;
	};

	public ref class WaveformData{
	public:
		typedef unsigned char UInt8;
		UInt32 Ns;
		UInt8 DualTrace;
		UInt8 VProbe1;
		UInt8 VProbe2;
		UInt8 VDProbe;
		array<UInt16>^ Trace1;
		array<UInt16>^ Trace2;
		array<UInt8>^ DTrace1;
		array<UInt8>^ DTrace2;
		WaveformData(int n){
			Ns = n;
			Trace1 = gcnew array<UINT16>(n);
			Trace2 = gcnew array<UInt16>(n);
			DTrace1 = gcnew array<UInt8>(n);
			DTrace2 = gcnew array<UInt8>(n);
		}
		virtual String^ ToString() override{
			return gcnew String("Not Implemented yet...");
		}

		static WaveformData^ DummyData(){
			int ndata = 1000;
			WaveformData^ ret = gcnew WaveformData(ndata);
			for (int i = 0; i < ndata; i++){
				ret->Trace1[i] = 0;
				ret->Trace2[i] = 0;
				ret->DTrace1[i] = 0;
				ret->DTrace2[i] = 0;
				if (i < ndata / 2) ret->Trace1[i] = 50;
				if (i > ndata / 2) ret->Trace2[i] = 30;
				if (i < ndata / 4) ret->DTrace1[i] = 20;
				if (i >(3 * ndata) / 4) ret->DTrace2[i] = 40;
			}
			return ret;
		}
	};

	public ref class EventData{
	public:
		UInt32 TimeTag;
		UInt32 Energy;
		WaveformData^ Waveform;
		EventData(){ Waveform = nullptr; }
		virtual String^ ToString() override;
		static EventData^ DummyData(bool withwaveform, double E){
			EventData^ ret = gcnew EventData();
			ret->TimeTag = 999;
			ret->Energy = (int)std::round(E);
			if (withwaveform){
				ret->Waveform = WaveformData::DummyData();
			}
			return ret;
		}
	};

	public ref class ChannelData{
	public:
		array<EventData^>^ Event;
		ChannelData(int nev);

		List<int>^ Energies();
		virtual String^ ToString() override;

		/// <summary>Create a dummy Channel Data. Energy is normal distribution with given mu and sigma.</summary>
		static ChannelData^ DummyData(int nev, int nwaveform, double mu, double sigma){
			ChannelData^ ret = gcnew ChannelData(nev);
			std::default_random_engine generator(std::random_device{}());
			std::normal_distribution<double> dist(mu, sigma);
			for (int i = 0; i < nev; i++){
				double E = dist(generator);
				E = E>0 ? E : 0;
				ret->Event[i] = EventData::DummyData(i < nwaveform, E);
			}
			return ret;
		}
	};

	public ref class ReadoutData{
	public:
		array<ChannelData^>^ Channel;
		ReadoutData(int nchannel){
			Channel = gcnew array<ChannelData^>(nchannel);
			for (int i = 0; i < nchannel; i++){
				Channel[i] = gcnew ChannelData(0);
			}
		}

		virtual String^ ToString() override{
			String^ ret = "";
			for (int ch = 0; ch < Channel->Length; ch++){
				ret += String::Format("**********CHANEL {0:d}***************\n", ch);
				ret += Channel[ch]->ToString();
				ret += "\n";
			}
			return ret;
		}

		static ReadoutData^ DummyData(int nchannel, int nev, int nwaveform, double mu, double sigma){
			ReadoutData^ ret = gcnew ReadoutData(nchannel);
			for (int i = 0; i < nchannel; i++){
				ret->Channel[i] = ChannelData::DummyData(nev, nwaveform, mu, sigma);
			}
			return ret;
		}

		static ReadoutData^ FromHandle(int handle);
		static ReadoutData^ FromHandle(int handle, int nwaveform);
	};

	public class MCAHelper{
	public:
		class DPPEvents{
		public:
			std::vector<CAEN_DGTZ_DPP_PHA_Event_t*> events;
			int handle;
			DPPEvents(int handle, int nchannel) :events(nchannel, 0), handle(handle){
				UInt32 allocsize;
				int ret = CAEN_DGTZ_MallocDPPEvents(handle, event_ptr(), &allocsize);
				if (ret) throw gcnew MCAException(ret);
			}
			~DPPEvents(){
				CAEN_DGTZ_FreeDPPEvents(handle, event_ptr());
			}
			void** event_ptr(){
				return (void**)&events[0];
			}
			void print_info(){

			}
		};

		class ReadoutBuffer{
		public:
			char* buffer;
			UInt32 buffersize;
			int handle;
			ReadoutBuffer(int handle) :handle(handle){
				int ret = CAEN_DGTZ_MallocReadoutBuffer(handle, &buffer, &buffersize);
				if (ret) throw gcnew MCAException(ret);
			}

			~ReadoutBuffer(){
				CAEN_DGTZ_FreeReadoutBuffer(&buffer);
			}
			bool IsEmpty(){
				return buffersize == 0;
			}
			void ReadData(){
				int ret = CAEN_DGTZ_ReadData(handle, CAEN_DGTZ_SLAVE_TERMINATED_READOUT_MBLT, buffer, &buffersize);
				if (ret) gcnew MCAException(ret);
			}

		};

		class DPPWaveform{
		public:
			CAEN_DGTZ_DPP_PHA_Waveforms_t *wf;
			int handle;
			DPPWaveform(int handle) : handle(handle){
				UInt32 allocsize;
				int ret = CAEN_DGTZ_MallocDPPWaveforms(handle, (void**)&wf, &allocsize);
				if (ret) throw gcnew MCAException(ret);
			}
			~DPPWaveform(){
				CAEN_DGTZ_FreeDPPWaveforms(handle, wf);
			}
			void** WaveformPtr(){ return (void**)&wf; }
		};

		static ReadoutData^ CreateReadoutData(ReadoutBuffer& rb, int nwaveform);
		static ChannelData^ CreateChannelData(int handle, CAEN_DGTZ_DPP_PHA_Event_t* events, UInt32 numevents, int nwaveform);
		static EventData^ CreateEventData(int handle, CAEN_DGTZ_DPP_PHA_Event_t& ev, bool withwaveform);
		static WaveformData^ CreateWaveformData(const CAEN_DGTZ_DPP_PHA_Waveforms_t& wf);
	};

}