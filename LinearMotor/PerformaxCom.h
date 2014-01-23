/*************************************************************************
 *                                                                       *
 * $Archive:: /Arcus Technology/PerformaxCom/PerformaxCom.h              *
 *                                                                       *
 * $Workfile:: PerformaxCom.h											 *
 *                                                                       *
 * $Revision:: 1                                                         *
 *                                                                       *
 * $Date:: 10/17/04														 *
 *                                                                       *
 * $Author:: CCC		                                                 *
 *                                                                       *
 * Module Description:                                                   *
 *     DLL for the Performax Device										 *
 *     Main Module                                                       *
 *                                                                       *
 * Environment:                                                          *
 *     User mode only                                                    *
 *                                                                       *
 *************************************************************************/
#include <Windows.h>
#ifdef PERFORMAXCOM_EXPORTS
#define PERFORMAXCOM_API __declspec(dllexport)
#else
#define PERFORMAXCOM_API __declspec(dllimport)
#endif

extern PERFORMAXCOM_API int nPerformaxCom;

#ifdef __cplusplus
extern "C" {
#endif

PERFORMAXCOM_API int fnPerformaxCom(void);

//USB reset

BOOL PERFORMAXCOM_API _stdcall fnPerformaxComUSBReset(IN HANDLE pHandle);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComFlush(IN HANDLE pHandle);

BOOL PERFORMAXCOM_API _stdcall fnPerformaxComGetNumDevices(OUT LPDWORD lpNumDevices);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComGetProductString(IN DWORD dwNumDevices, OUT LPVOID lpDeviceString, IN DWORD dwOptions);	
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComOpen(IN DWORD dwDeviceNum, OUT HANDLE* pHandle);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComClose(IN HANDLE pHandle);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSetTimeouts(IN DWORD dwReadTimeout, DWORD dwWriteTimeout);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSendRecv(IN HANDLE pHandle, IN LPVOID wBuffer, IN DWORD dwNumBytesToWrite,IN DWORD dwNumBytesToRead,OUT LPVOID rBuffer);

signed int PERFORMAXCOM_API _stdcall fnPerformaxMove(IN HANDLE pHandle, 
													IN int commandCode, 
													IN int axes,
													IN int moveDir,
													IN long *value,
													IN int modelCode);

signed int PERFORMAXCOM_API _stdcall fnPerformaxCommandReply(IN HANDLE pHandle, 
															 IN char *command, 
															 IN char *reply);

signed int PERFORMAXCOM_API _stdcall fnPerformaxIO(IN HANDLE pHandle, 
													IN int commandCode, 
													IN int bitNumber,
													IN long *value,
													IN int modelCode);

signed int PERFORMAXCOM_API _stdcall fnPerformaxMotorStat(IN HANDLE pHandle, 
													IN int commandCode, 
													IN int axes,
													IN long *value,
													IN int modelCode);

signed int PERFORMAXCOM_API _stdcall fnPerformaxSpeedAccel(IN HANDLE pHandle, 
														   IN int commandCode, 
														   IN int axes,
														   IN long *value,
														   IN int modelCode);

signed int PERFORMAXCOM_API _stdcall fnPerformaxGeneral(IN HANDLE pHandle, 
														IN int commandCode, 
														IN int axes,
														IN long *value,  
														IN int modelCode);


long PERFORMAXCOM_API _stdcall fnPerformaxComGetAxisStatus(IN HANDLE pHandle,DWORD dwAxisNumber);
long PERFORMAXCOM_API _stdcall fnPerformaxComGetPulseSpeed(IN HANDLE pHandle,DWORD dwAxisNumber);
long PERFORMAXCOM_API _stdcall fnPerformaxComGetPulsePos(IN HANDLE pHandle,DWORD dwAxisNumber);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSetPulsePos(IN HANDLE pHandle,DWORD dwAxisNumber, DWORD pos);
long PERFORMAXCOM_API _stdcall fnPerformaxComGetEncoderPos(IN HANDLE pHandle,DWORD dwAxisNumber);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSetEncoderPos(IN HANDLE pHandle,DWORD dwAxisNumber, DWORD pos);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSetPolarity(IN HANDLE pHandle,DWORD dwAxisNumber,DWORD dwPolarity);
long PERFORMAXCOM_API _stdcall fnPerformaxComGetPolarity(IN HANDLE pHandle,DWORD dwAxisNumber);
long PERFORMAXCOM_API _stdcall fnPerformaxComGetDIOSetup(IN HANDLE pHandle);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSetDIOSetup(IN HANDLE pHandle, DWORD dio_setup);
long PERFORMAXCOM_API _stdcall fnPerformaxComGetDIO(IN HANDLE pHandle);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComGetDIOBit(IN HANDLE pHandle,DWORD bitn);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSetDIO(IN HANDLE pHandle,DWORD dio_value );
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComSetDIOBit(IN HANDLE pHandle,DWORD bitn,DWORD bitv );
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComDoHome(IN HANDLE pHandle,
														   DWORD axis,
														   DWORD dir,
														   DWORD low_speed,
														   DWORD high_speed,
														   DWORD accel_time,
														   DWORD decel_time,
														   DWORD search_mode,
														   DWORD ramp_mode);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComDoMove(IN HANDLE pHandle,
														   DWORD axisn,
														   DWORD target,
														   DWORD low_speed,
														   DWORD high_speed,
														   DWORD accel_time,
														   DWORD decel_time);
BOOL PERFORMAXCOM_API _stdcall fnPerformaxComStop(IN HANDLE pHandle,
														   DWORD axisn,
														   DWORD ramp_stop);






#ifdef __cplusplus
}
#endif
