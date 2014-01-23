#include "CAENDPP.h"
#include "CAENDigitizer.h"
#pragma once


namespace CAENDigitizerWrapper{
using namespace System::Collections::Generic;
using namespace System;
public ref class MCAException: public SystemException{
		
	public:

		enum class ErrorCodeType{
			Success = CAEN_DGTZ_Success,
			CommError = CAEN_DGTZ_CommError,
			GenericError = CAEN_DGTZ_GenericError,
			InvalidParam = CAEN_DGTZ_InvalidParam,
			InvalidLinktype = CAEN_DGTZ_InvalidLinkType,
			InvalidHandle = CAEN_DGTZ_InvalidHandle,
			MaxDevicesError = CAEN_DGTZ_MaxDevicesError,
			BadBoardType = CAEN_DGTZ_BadBoardType,
			BadInterruptLev = CAEN_DGTZ_BadInterruptLev,
			BadEventNumber = CAEN_DGTZ_BadEventNumber,
			ReadDeviceRegisterFail = CAEN_DGTZ_ReadDeviceRegisterFail,
			WriteDeviceRegisterFail = CAEN_DGTZ_WriteDeviceRegisterFail,
			InvalidChannelNumber = CAEN_DGTZ_InvalidChannelNumber,
			ChannelBusy = CAEN_DGTZ_ChannelBusy,
			FPIOModeInvalid = CAEN_DGTZ_FPIOModeInvalid,
			WrongAcqMode = CAEN_DGTZ_WrongAcqMode,
			FunctionNotAllowed = CAEN_DGTZ_FunctionNotAllowed,
			Timeout = CAEN_DGTZ_Timeout,
			InvalidBuffer = CAEN_DGTZ_InvalidBuffer,
			EventNotFound = CAEN_DGTZ_EventNotFound,
			InvalidEvent = CAEN_DGTZ_InvalidEvent,
			OutOfMemory = CAEN_DGTZ_OutOfMemory,
			CalibrationError = CAEN_DGTZ_CalibrationError,
			DigitizerNotFound = CAEN_DGTZ_DigitizerNotFound,
			DigitizerAlreadyOpen = CAEN_DGTZ_DigitizerAlreadyOpen,	
			DigitizerNotReady = CAEN_DGTZ_DigitizerNotReady,
			InterruptNotConfigured = CAEN_DGTZ_InterruptNotConfigured,
			DigitizerMemoryCorrupted = CAEN_DGTZ_DigitizerMemoryCorrupted,
			DPPFirmwareNotSupported = CAEN_DGTZ_DPPFirmwareNotSupported,
			NotYetImplemented = CAEN_DGTZ_NotYetImplemented
		};

		property ErrorCodeType ErrorCode;
		property String^ msg;

		MCAException(int err);
		MCAException(int err, String^ message);

		static String^ ErrCode2String(ErrorCodeType errcode);
		virtual String^ ToString() override;
	private: 		
		static Dictionary<int, String^>^ errmap = nullptr;

	};
}