#include "Stdafx.h"
#include "MCAException.h"

namespace CAENDigitizerWrapper
{
	MCAException::MCAException(int err){
		this->ErrorCode = (ErrorCodeType)err;
		this->msg = gcnew String("");
	}

	MCAException::MCAException(int err, String^ message){
		this->ErrorCode = (ErrorCodeType)err;
		this->msg = message;
	}

	String^ MCAException::ErrCode2String(ErrorCodeType errcode){
		if(errmap==nullptr){//initialize on the fly
			errmap = gcnew Dictionary<int, String^>;
			errmap->Add(0, "Operation completed successfully.");
			errmap->Add(-1, "Communication Error.");
			errmap->Add(-2, "Unspecified Error.");
			errmap->Add(-3, "Invalid Parameter.");
			errmap->Add(-4, "Invalid Link Type.");
			errmap->Add(-5, "Invalid device handler.");
			errmap->Add(-6, "Maximum number of devices exceed.");
			errmap->Add(-7, "Operation not allowed on this type of board.");
			errmap->Add(-8, "The interrupt level is not allowed.");
			errmap->Add(-9, "The event number is bad.");
			errmap->Add(-10, "Unable to read the registry.");
			errmap->Add(-11, "Unable to write into the registry.");
			errmap->Add(-13, "The Channel is busy.");
			errmap->Add(-14, "The channel number is invalid.");
			errmap->Add(-15, "Invalid FPIO Mode.");
			errmap->Add(-16, "Wrong acquisition mode.");
			errmap->Add(-17, "This function is not allowed for this module.");
			errmap->Add(-18, "Communication Timeout.");
			errmap->Add(-19, "The buffer is invalid.");
			errmap->Add(-20, "the event is not found.");
			errmap->Add(-21, "The event is invalid.");
			errmap->Add(-22, "Out of memory.");
			errmap->Add(-23, "Unable to calibrate the board.");
			errmap->Add(-24, "Unable to open the digitizer.");
			errmap->Add(-25, "The digitizer is already open.");
			errmap->Add(-26, "The Digitizer is not ready to operate.");
			errmap->Add(-27, "The digitizer does not have the IRQ configured.");
			errmap->Add(-28, "The digitizer flash memory is corrupted.");
			errmap->Add(-29, "The digitizer DPP firmware is not supported in this lib version.");
			errmap->Add(-99, "The function is not yet implemented.");
		}
		return errmap[(int)errcode];
	}

	String^ MCAException::ToString(){
		String^ ret = ErrCode2String(ErrorCode);
		if(!String::IsNullOrWhiteSpace(msg)){
			ret += "\nMsg: "+ msg;
		}
		return ret;
	} 
}