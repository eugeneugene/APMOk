#pragma once

#include "framework.h"
#include "atahdd.h"

#ifdef HW_EXPORTS
#define HWLIBRARY_API __declspec(dllexport)
#else
#define HWLIBRARY_API __declspec(dllimport)
#endif

constexpr auto WIN_IDENTIFYDEVICE = 0xEC;
constexpr auto WIN_SETFEATURES = 0xEF;
constexpr auto SETFEATURES_EN_APM = 0x05;
constexpr auto SETFEATURES_DIS_APM = 0x85;

struct EnumDiskInfo
{
	uint32_t InfoValid;					// 4
	uint32_t Index;						// 4
	uint16_t Availability;				// 2
	wchar_t Caption[256];				// 512
	wchar_t Description[256];			// 512
	wchar_t DeviceID[256];				// 512
	wchar_t InterfaceType[16];			// 32
	wchar_t Manufacturer[64];			// 128
	wchar_t Model[64];					// 128
	wchar_t SerialNumber[64];			// 128
};

const int DataBufSize = 512;
typedef struct _ATA_PASS_THROUGH_EX_WITH_BUFFERS
{
	ATA_PASS_THROUGH_EX AtaPassThroughEx;
	UCHAR ucDataBuf[DataBufSize];
} ATA_PASS_THROUGH_EX_WITH_BUFFERS, * PATA_PASS_THROUGH_EX_WITH_BUFFERS;

extern "C" HWLIBRARY_API int EnumerateDisks(EnumDiskInfo* diskInfo);
extern "C" HWLIBRARY_API uint16_t GetAPM(wchar_t* dskName);
extern "C" HWLIBRARY_API int SetAPM(wchar_t* dskName, byte val, bool disable);

BOOL GetHDDId(HANDLE hDrive, IDENTIFY_DEVICE_DATA* hddid);
BOOL SetAPM(HANDLE hDrive, BYTE APMVal, BOOL Disable);

void WriteLog(const wchar_t* format, ...);
