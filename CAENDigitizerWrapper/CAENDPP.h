#pragma once
namespace CAENDigitizerWrapper{
	using namespace System;
	public ref class CAENDPP{
	private:
		static inline const UInt32 H2C(UInt32 i){return i+2;}//convert HV channel number to register channel
	public:
		static const UInt32 CONFIG = 0x8000;
		static const UInt32 CONFIG_SET = 0x8004;
		static const UInt32 ACQUISITION_CONTROL = 0x8100;
		static const UInt32 ACQUISITION_STATUS = 0x8104;
		static const UInt32 SOFTWARE_TRIGGER = 0x8108; //write only
		static const UInt32 GLOBAL_TRIGGER_MASK = 0x810c;
		static const UInt32 TRIGGER_OUT_MASK = 0x8110;
		static const UInt32 LVDS_DATA = 0x8118;
		static const UInt32 FRONT_PANEL_CONTROL = 0x811c;
		static const UInt32 CHANNEL_ENABLE_MASK = 0x8120;
		static const UInt32 FIRMWARE_REVISION = 0x8124;
		static const UInt32 SW_CLOCK_SYNC = 0x813c; // write only
		static const UInt32 BOARD_INFO = 0x8140;
		static const UInt32 MONITOR_DAC_MODE = 0x8144;
		static const UInt32 EVENT_SIZE = 0x814c;
		static const UInt32 VETO = 0x817c;
		static const UInt32 READOUT_CONTROL = 0xEF00;
		static const UInt32 READOUT_STATUS = 0xEF04;
		static const UInt32 SOFTWARE_RESET = 0xEF24; //write only
		static const UInt32 SOFTWARE_CLEAR = 0xEF28; //write only
		static const UInt32 DUMMY = 0xEF20;
		
		//Channel registers
		static const UInt32 CUST_SIZE(UInt32 i){return 0x1020 | i <<8;}
		static const UInt32 NEV_AGGREGATE(UInt32 i){return 0x1034 | i << 8;}
		static const UInt32 PRE_TRG(UInt32 i){return 0x1038 | i << 8;}
		static const UInt32 TRG_FILTER_SMOOTHING(UInt32 i){return 0x1054 | i << 8;}
		static const UInt32 INPUT_RISE_TIME(UInt32 i){return 0x1058 | i << 8;}
		static const UInt32 TRAPEZOID_RISE_TIME(UInt32 i){return 0x105c | i << 8;}
		static const UInt32 TRAPEZOID_FLAT_TOP(UInt32 i){return 0x1060 | i << 8;}
		static const UInt32 PEAKING_TIME(UInt32 i){return 0x1064 | i << 8;}
		static const UInt32 DECAY_TIME(UInt32 i){return 0x1068 | i << 8;}
		static const UInt32 TRG_THRESHOLD(UInt32 i){return 0x106c | i << 8;}
		static const UInt32 RTD_WINDOW_WIDTH(UInt32 i){return 0x1070 | i << 8;}
		static const UInt32 TRG_HOLD_OFF(UInt32 i){return 0x1074 | i << 8;}
		static const UInt32 PEAK_HOLD_OFF_EXTENSION(UInt32 i){return 0x1078 | i << 8;}
		static const UInt32 BASELINE_HOLD_OFF_EXTENSION(UInt32 i){return 0x107c | i << 8;}
		static const UInt32 MISC(UInt32 i){return 0x1080 | i << 8;}
		static const UInt32 INPUT_RANGE(UInt32 i){return 0x10B4 | i << 8;}


		static const UInt32 HV_VSET_ADDR(UInt32 i){return 0x1020 | H2C(i) << 8;}// VSET Address
		static const double HV_VSET_RES = 10.0;   // VSET Register Resolution (V)
		static const double HV_VSET_MAX = 5100.0; // VSET Maximum allowed value

		static const UInt32 HV_ISET_ADDR(UInt32 i){return 0x1024 | H2C(i) << 8;} // ISET Address
		static const double HV_ISET_RES = 100.0;  // ISET Register Resolution (uA)
		static const double HV_ISET_MAX = 315.00; // ISET Maximum allowed value

		static const UInt32 HV_RAMPUP_ADDR(UInt32 i){return 0x1028 | H2C(i) << 8;} // RAMPUP Address
		static const double HV_RAMPUP_RES = 1.0;    // RAMPUP Register Resolution (V/s)
		static const double HV_RAMPUP_MAX = 500;    // RAMPUP Maximum allowed value

		static const UInt32 HV_RAMPDOWN_ADDR(UInt32 i){return 0x102C | H2C(i) << 8;} // RAMPDOWN Address
		static double HV_RAMPDOWN_RES = 1.0;    // RAMPDOWN Register Resolution (V/s)
		static double HV_RAMPDOWN_MAX = 500;    // RAMPDOWN Maximum allowed value

		static const UInt32 HV_VMAX_ADDR(UInt32 i){return 0x1030 | H2C(i) << 8;} // VMAX Address
		static const double HV_VMAX_RES = 0.05;   // VMAX Register Resolution (V)
		static const double HV_VMAX_MAX = 5100;   // VMAX Maximum allowed value

		static const UInt32 HV_VMON_ADDR(UInt32 i){return 0x1040 | H2C(i) << 8;} // VMON Address (Also used for VEXT monitoring, see HV_EXT_MON_SWITCHMASK description)
		static const double HV_VMON_RES = 10.0;   // VMON Register Resolution (V)

		static const UInt32 HV_IMON_ADDR(UInt32 i){return 0x1044 | H2C(i) << 8;} // IMON Address (Also used for RTMP monitoring, see HV_EXT_MON_SWITCHMASK description)
		static const double HV_IMON_RES = 100.0;  // IMON Register Resolution (uA)

		static const UInt32 HV_POWER_ADDR(UInt32 i){return 0x1034 | H2C(i) << 8;} // POWER Address (Also used for switch mode KILL/RAMP (bit 1) and for EXT monitoring)
		static const UInt32 HV_STATUS_ADDR(UInt32 i){return 0x1038 | H2C(i) <<8;} // STATUS Address

		static const double HV_VEXT_RES = 1000.0; // VEXT Register Resolution (V)
		static const double HV_RTMP_RES = 10.0;   // RTMP Register Resolution (V)

		// The following mask can be applied to the value of register HV_POWER_ADDR
		// to determine if the values read from HV_VMON_ADDR and HV_IMON_ADDR must
		// be VMON and IMON (!HV_EXT_MON_SWITCHMASK) or VEXT/RTMP (HV_EXT_MON_SWITCHMASK)

        static const UInt32 HV_EXT_MON_SWITCHMASK = 0x80;

		// Bits to decode HV Channel status
		static const UInt32 HV_STATUSBIT_PW         = 0	;
		static const UInt32 HV_STATUSBIT_RAMPUP     = 1	;
		static const UInt32 HV_STATUSBIT_RAMPDOWN   = 2	;
		static const UInt32 HV_STATUSBIT_OVCURR     = 3	;
		static const UInt32 HV_STATUSBIT_OVVOLT     = 4	;
		static const UInt32 HV_STATUSBIT_UNDVOLT    = 5	;
		static const UInt32 HV_STATUSBIT_MAXV       = 6	;
		static const UInt32 HV_STATUSBIT_MAXI       = 7	;
		static const UInt32 HV_STATUSBIT_TEMPWARN   = 8	;
		static const UInt32 HV_STATUSBIT_OVTEMP     = 9	;
		static const UInt32 HV_STATUSBIT_DISABLE    = 10;
		static const UInt32 HV_STATUSBIT_CALIBERR   = 11;
		static const UInt32 HV_STATUSBIT_RESET      = 12;
		static const UInt32 HV_STATUSBIT_GOINGOFF   = 13;
							

	};
}