// LinearMotor.h

#pragma once

#include "PerformaxCom.h"
#include <cmath>
#include <msclr\lock.h>

namespace LinearMotorWrapper {
	using namespace System;
	using namespace System::Threading;
	using namespace System::Diagnostics;
	using namespace System::Runtime::InteropServices;
	public ref class LinearMotorException :Exception{
	public:
		LinearMotorException() : Exception() {}
		LinearMotorException(String^ message) : Exception(message) {}
	};

	///<summary>
	/// A singleton class for communicating with nsc-a1 stepper motor controller (performax pmx in disguise)
	///</summary>
	public ref class LinearMotor
	{
	public:

		static LinearMotor(){
			instance = nullptr;
		}

		static LinearMotor^ GetInstance(){
			if (instance == nullptr){
				instance = gcnew LinearMotor();
			}
			return instance;
		}

		~LinearMotor(){ this->!LinearMotor(); }
		!LinearMotor(){
			try{
				Stop();
				fnPerformaxComFlush(handle);
				Close();
			}
			catch (LinearMotorException^ e){
			}
		}

		///<summary>
		///Close the connection to motor. This function will not throw Exception.
		///</summary>
		void Close(){
			try{
				fnPerformaxComClose(handle);
			}
			catch (SEHException^){
			}
		}

		static const int COMMAND_LENGTH = 64;
		static const double CM_PER_STEP = 1.2 / (200 * 250); //screwpitch / (step per rev*microstep per rev)
		static const double STEP_PER_CM = 200 * 250 / 1.2;
		bool testmode;

		///<summary>
		///Try connect to motor controller on usb number 0 to 20.
		///If found, return true. False otherwise.
		///</summary>
		bool ScanConnect(){
			bool foundone = false;
			for (int i = 0; i < 20; i++){
				try{
					Connect(i);
					Console::WriteLine(i);
					foundone = true;
					break;
				}
				catch (LinearMotorException^ e){
				}
			}
			//if(!foundone) throw gcnew LinearMotorException();
			return foundone;
		}

		///<summary>
		///Connect to motor controll with given linknum
		///If there is only one motor controller attached, use ScanConnect would be easier.
		///</summary>
		void Connect(int linknum){
			pin_ptr<HANDLE> hptr = &handle;
			bool ret = fnPerformaxComOpen(linknum, (HANDLE*)hptr);
			if (!ret) throw gcnew LinearMotorException();
			ret = fnPerformaxComSetTimeouts(1000, 1000);
			if (!ret) throw gcnew LinearMotorException();
			ret = fnPerformaxComFlush(handle);
			if (!ret) throw gcnew LinearMotorException();
			//SetSpeed(5000);
		}

		///<summary>
		///Test if the communation link is connected
		///</summary>
		bool IsConnected(){
			bool ret = true;
			try{
				SendRecv("VER", true);
			}
			catch (LinearMotorException^ e){
				ret = false;
			}
			return ret;
		}

		///<summary>
		///Set the speed of the motor in the unit of microstep/sec
		///If you need to speed in cm/sec use STEP_PER_CM
		///</summary>
		void SetSpeed(UInt32 speed){
			String^ tosend = String::Format("HSPD={0:d}", speed);
			SendRecv(tosend);
			tosend = String::Format("LSPD={0:d}", speed / 2);
			SendRecv(tosend);
		}

		/***
		* <summary>
		* Convert String to byte array with the length of COMMAND_LENGTH.
		* <summary>
		***/
		array<byte>^ str2byte(String^ str){
			return str2byte(str, COMMAND_LENGTH);
		}

		///<summary>
		/// convert byte array to String for given length.
		///</summary>
		array<byte>^ str2byte(String^ str, int len){
			array<byte>^ ret = gcnew array<byte>(len);
			for (int i = 0; i < min(str->Length, len - 1); i++){
				ret[i] = str[i];
			}
			for (int i = str->Length; i < len; i++){
				ret[i] = 0;
			}
			return ret;
		}

		///<summary>
		/// convert byte array to string.
		///</summary>
		String^ byte2str(array<byte>^ b){
			pin_ptr<byte> x = &b[0];
			String^ ret = gcnew String((char*)x, 0, strnlen((char*)x, b->Length));
			return ret;
		}

		///<summary>
		///Send command to the controll and get the reply message.
		///</summary>
		String^ SendRecv(String^ send, bool shouldthrow){
			msclr::lock l(lock); //performax library not thread safe?
			array<byte>^ bsend = str2byte(send);
			array<byte>^ brecv = gcnew array<byte>(COMMAND_LENGTH);
			for (int i = 0; i < brecv->Length; i++){ brecv[i] = 0; }
			pin_ptr<byte> sendbuffer = &bsend[0];
			pin_ptr<byte> recvbuffer = &brecv[0];
			bool ret = true;
			ret = ret ? fnPerformaxComSetTimeouts(1000, 1000) : ret;
			if (!ret && shouldthrow) throw gcnew LinearMotorException();
			ret = ret ? fnPerformaxComFlush(handle): ret;
			if (!ret && shouldthrow) throw gcnew LinearMotorException();
			ret = ret?fnPerformaxComSendRecv(handle, (LPVOID)sendbuffer, COMMAND_LENGTH, COMMAND_LENGTH, (LPVOID)recvbuffer):ret;
			if (!ret && shouldthrow) throw gcnew LinearMotorException();
			String^ toret = byte2str(brecv);
			if (testmode){
				Console::Write(send); Console::Write(":"); Console::WriteLine(toret);
			}
			return toret;
		}

		String^ SendRecv(String^ send){
			return SendRecv(send, false);
		}

		///<summary>
		///Send command to the controller and check if the return status is OK.
		///</summary>
		bool SendRecvOK(String^ send, bool shouldthrow){
			String^ recv = SendRecv(send);
			bool ok = recv == "OK";
			if (!ok && shouldthrow){
				throw gcnew LinearMotorException(recv);
			}
			return ok;
		}

		bool SendRecvOK(String^ send){
			return SendRecvOK(send, false);
		}

		///<summary>
		///Do the standard calibration. Move to limit plus slowly.
		///</summary>
		void Calibrate(){
			//Move fast to limit plus then move away and move back slowly to limit plus and set home
			SetMotorPower(1);
			SetSpeed(150000);
			LimitPlus();
			Wait();
			SetSpeed(10000);
			MoveTo(-5000);
			Wait();
			SetSpeed(5000);
			LimitPlus();
			Wait();
			SetSpeed(50000);
		}

		///<summary>
		/// Move the motor until it hits the limit switch on the plus side
		/// and set that position as home
		///</summary>
		void LimitPlus(){ SendRecvOK("L+"); Clear(); }

		void LimitMinus(){ SendRecvOK("L-"); }

		//void HomePlus(){SendRecvOK("H+");}

		//void HomeMinus(){SendRecvOK("H-");}

		///<summary>
		///Stop the motor with ramping down
		///</summary>
		void Stop(){ SendRecvOK("STOP"); }

		///<summary>
		///Stop the motor immediately. No ramping down.
		///</summary>
		void Abort(){ SendRecvOK("ABORT"); }

		///<summary>
		///clear limit switch bits
		///</summary>
		void Clear(){ SendRecvOK("CLR"); }

		///<summary>
		/// Check if the motor is still running
		///</summary>
		bool IsRunning(){
			String^ msg = SendRecv("MST");
			UInt32 status = 0;
			try{
				status = Convert::ToUInt32(msg);
			}
			catch (FormatException^){
				//throw gcnew LinearMotorException("Unable to parse MST reply");
				return false;
			}
			return (status & 0xF) != 0;
		}
		///<summary>
		///Wait til motor stop
		///</summary>
		bool Wait(){
			return Wait(0);
		}

		///<summary>
		///wait til timeout or until the motor stop
		///</summary>
		bool Wait(int timeout){
			int time = 0;
			bool motorstopped = false;
			bool timeup = false;
			int timesofar = 0;
			int sleepinterval = 500;
			while (!motorstopped && !timeup){
				Thread::Sleep(sleepinterval);
				motorstopped = !IsRunning();
				timeup = (timeout > 0) && (timesofar > timeout);
				timesofar += sleepinterval;
			}
			return motorstopped;
		}

		///<summary>
		///get the current position
		//</summary>
		int Position(bool shouldthrow){
			int ret = 0xEFFFFFFF;
			try{
				String^ tmp = SendRecv("PX", shouldthrow);
				ret = Convert::ToInt32(tmp);
			}
			catch (FormatException^){
				if (shouldthrow) throw gcnew LinearMotorException("Unable to Parse PX response");
			}
			return ret;
		}
		int Position(){
			return Position(false);
		}

		///<summary>
		///get the current position in cm
		//</summary>
		double PositionCM(bool shouldthrow){
			return Position(shouldthrow)*CM_PER_STEP;
		}
		double PositionCM(){
			return PositionCM(false);
		}
		///<summary>
		///Move absolutely to x microstep position
		///</summary>
		void MoveTo(int x){
			Clear();
			SetMotorPower(1);
			SendRecvOK(String::Format("X{0:d}", x));
		}

		///<summary>
		///Move absolutely to x cm position
		///</summary>
		void MoveTocm(double x){
			int pos = floor((x*STEP_PER_CM) + 0.5);
			MoveTo(pos);
		}

		bool MotorPower(){
			String^ reply = SendRecv("EO");
			int power = 0;
			try{
				power = Int32::Parse(reply);
			}
			catch (FormatException^){
				power = 0;
			}
			return power == 1;
		}

		void SetMotorPower(int pwr){
			pwr = pwr ? 1 : 0;
			SendRecvOK(String::Format("EO={0:d}", pwr));
		}

	private:
		static LinearMotor^ instance;
		void writehex(array<byte>^ b){
			for (int i = 0; i < b->Length; i++){
				Console::Write(String::Format("{0:X}", b[i]));
			}
			Console::WriteLine();
		}
		LinearMotor(){ testmode = false; lock = gcnew Object(); }
		HANDLE handle;
		Object^ lock;
	};

}
