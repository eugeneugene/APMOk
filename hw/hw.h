#pragma once

#include "framework.h"

#ifdef HW_EXPORTS
#define HWLIBRARY_API __declspec(dllexport)
#else
#define HWLIBRARY_API __declspec(dllimport)
#endif

struct EnumDiskInfo
{
	UINT16 DiskIndex;
	UINT16 Availability;
	TCHAR Caption[256];
	UINT32 ConfigManagerErrorCode;
	TCHAR Description[256];
	TCHAR DeviceID[256];
	UINT32 Index;
	TCHAR InterfaceType[16];
	TCHAR Manufacturer[64];
	TCHAR Model[64];
	TCHAR Name[64];
	TCHAR SerialNumber[64];
	TCHAR Status[16];
};

extern "C" HWLIBRARY_API int EnumerateDisks(EnumDiskInfo diskInfo[]);
