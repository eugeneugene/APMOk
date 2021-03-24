#pragma once

#include "framework.h"

#ifdef HW_EXPORTS
#define HWLIBRARY_API __declspec(dllexport)
#else
#define HWLIBRARY_API __declspec(dllimport)
#endif

#define WIN_IDENTIFYDEVICE 0xEC
#define WIN_SETFEATURES 0xEF
#define SETFEATURES_EN_APM 0x05
#define SETFEATURES_DIS_APM 0x85 

struct EnumDiskInfo		// 2124
{
	uint16_t DiskIndex;					// 2
	uint32_t Index;						// 4
	uint16_t Availability;				// 2
	wchar_t Caption[256];				// 512
	uint32_t ConfigManagerErrorCode;	// 4
	wchar_t Description[256];			// 512
	wchar_t DeviceID[256];				// 512
	wchar_t InterfaceType[16];			// 32
	wchar_t Manufacturer[64];			// 128
	wchar_t Model[64];					// 128
	wchar_t Name[64];					// 128
	wchar_t SerialNumber[64];			// 128
	wchar_t Status[16];					// 32
};

const int DataBufSize = 512;
typedef struct _ATA_PASS_THROUGH_EX_WITH_BUFFERS
{
	ATA_PASS_THROUGH_EX AtaPassThroughEx;
	UCHAR ucDataBuf[DataBufSize];
} ATA_PASS_THROUGH_EX_WITH_BUFFERS, * PATA_PASS_THROUGH_EX_WITH_BUFFERS;

extern "C" HWLIBRARY_API int EnumerateDisks(void* ptr);
extern "C" HWLIBRARY_API int SetAPM(wchar_t* dskName, byte val, bool disable);
