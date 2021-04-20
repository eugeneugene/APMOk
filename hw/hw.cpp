#include "pch.h"
#include "hw.h"

HRESULT hrInit = HRESULT_CODE(-1);
HRESULT hrInitSec = HRESULT_CODE(-1);

/// <summary>
/// Вернуть массив из 16 элементов типа EnumDiskInfo в аргументе diskInfo
/// Вызывающая сторона резервирует память под diskInfo
/// Возврат:     0 - Успех
///	         Иначе - Ошибка
/// </summary>
/// <param name="diskInfo">Массив из 16 элементов типа EnumDiskInfo</param>
/// <returns>
/// 0 - Успех
/// 1 - Failed to get Disk Drive information
/// 2 - Failed to set proxy blanket
/// 3 - Failed to connect to root namespace
/// 4 - Failed to create IWbemLocator object
/// 5 - Failed to set COM Security
/// 6 - Failed to initialize COM
/// </returns>
extern "C" HWLIBRARY_API int EnumerateDisks(EnumDiskInfo * diskInfo)
{
	int retVal = 0;

	if (FAILED(hrInit))
		hrInit = CoInitializeEx(0, COINIT_MULTITHREADED);

	if (FAILED(hrInit))
	{
		WriteLog(L"Failed to initialize COM. Error code = %x\r\n", hrInit);
		return 6;
	}

	if (FAILED(hrInitSec))
	{
		hrInitSec = CoInitializeSecurity(
			NULL,
			-1,                          // COM authentication
			NULL,                        // Authentication services
			NULL,                        // Reserved
			RPC_C_AUTHN_LEVEL_DEFAULT,   // Default authentication 
			RPC_C_IMP_LEVEL_IMPERSONATE, // Default Impersonation  
			NULL,                        // Authentication info
			EOAC_NONE,                   // Additional capabilities 
			NULL                         // Reserved
		);
	}

	if (SUCCEEDED(hrInitSec))
	{
		HRESULT hr;
		CComPtr<IWbemLocator> pLoc;
		hr = CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER, IID_IWbemLocator, (LPVOID*)&pLoc);
		if (SUCCEEDED(hr))
		{
			CComPtr<IWbemServices> pSvc;
			hr = pLoc->ConnectServer(CComBSTR(L"ROOT\\CIMV2"), nullptr, nullptr, nullptr, 0, nullptr, nullptr, &pSvc);
			if (SUCCEEDED(hr))
			{
				CComPtr<IEnumWbemClassObject> pEnumerator;
				hr = CoSetProxyBlanket(
					pSvc,                        // Indicates the proxy to set
					RPC_C_AUTHN_WINNT,           // RPC_C_AUTHN_xxx
					RPC_C_AUTHZ_NONE,            // RPC_C_AUTHZ_xxx
					NULL,                        // Server principal name 
					RPC_C_AUTHN_LEVEL_CALL,      // RPC_C_AUTHN_LEVEL_xxx 
					RPC_C_IMP_LEVEL_IMPERSONATE, // RPC_C_IMP_LEVEL_xxx
					NULL,                        // client identity
					EOAC_NONE                    // proxy capabilities 
				);
				if (SUCCEEDED(hr))
				{
					hr = pSvc->ExecQuery(CComBSTR(L"WQL"), CComBSTR(L"SELECT * FROM Win32_DiskDrive"),
						WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, NULL, &pEnumerator);
					if (SUCCEEDED(hr))
					{
						for (UINT16 index = 0U; index < 16U; index++)
						{
							EnumDiskInfo* info = &diskInfo[index];
							info->InfoValid = FALSE;
							if (pEnumerator)
							{
								CComPtr<IWbemClassObject> pclsObj;
								CComVariant var;
								ULONG uReturn;
								CIMTYPE cType;

								hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);

								if (uReturn)
								{
									info->InfoValid = TRUE;
									WriteLog(L"InfoValid: %i\r\n", TRUE);

									hr = pclsObj->Get(L"Index", 0, &var, &cType, 0);
									info->Index = var.uintVal;
									WriteLog(L"Index: %u\r\n", info->Index);

									hr = pclsObj->Get(L"Availability", 0, &var, &cType, 0);
									info->Availability = var.uiVal;
									WriteLog(L"Availability: %hu\r\n", info->Availability);

									hr = pclsObj->Get(L"Caption", 0, &var, &cType, 0);
									wcscpy_s(info->Caption, var.bstrVal);
									WriteLog(L"Caption: %s\r\n", info->Caption);

									hr = pclsObj->Get(L"ConfigManagerErrorCode", 0, &var, &cType, 0);
									info->ConfigManagerErrorCode = var.uintVal;
									WriteLog(L"ConfigManagerErrorCode: %u\r\n", info->ConfigManagerErrorCode);

									hr = pclsObj->Get(L"Description", 0, &var, &cType, 0);
									wcscpy_s(info->Description, var.bstrVal);
									WriteLog(L"Description: %s\r\n", info->Description);

									hr = pclsObj->Get(L"DeviceID", 0, &var, &cType, 0);
									wcscpy_s(info->DeviceID, var.bstrVal);
									WriteLog(L"DeviceID: %s\r\n", info->DeviceID);

									info->APMValue = (uint16_t)GetAPM(info->DeviceID);
									WriteLog(L"APMValue: %hu\r\n", info->APMValue);

									hr = pclsObj->Get(L"InterfaceType", 0, &var, &cType, 0);
									wcscpy_s(info->InterfaceType, var.bstrVal);
									WriteLog(L"InterfaceType: %s\r\n", info->InterfaceType);

									hr = pclsObj->Get(L"Manufacturer", 0, &var, &cType, 0);
									wcscpy_s(info->Manufacturer, var.bstrVal);
									WriteLog(L"Manufacturer: %s\r\n", info->Manufacturer);

									hr = pclsObj->Get(L"Model", 0, &var, &cType, 0);
									wcscpy_s(info->Model, var.bstrVal);
									WriteLog(L"Model: %s\r\n", info->Model);

									hr = pclsObj->Get(L"Name", 0, &var, &cType, 0);
									wcscpy_s(info->Name, var.bstrVal);
									WriteLog(L"Name: %s\r\n", info->Name);

									hr = pclsObj->Get(L"SerialNumber", 0, &var, &cType, 0);
									wcscpy_s(info->SerialNumber, var.bstrVal);
									WriteLog(L"SerialNumber: %s\r\n", info->SerialNumber);

									hr = pclsObj->Get(L"Status", 0, &var, &cType, 0);
									wcscpy_s(info->Status, var.bstrVal);
									WriteLog(L"Status: %s\r\n", info->Status);
								}
							}
						}
						retVal = 0;
					}
					else
					{
						WriteLog(L"Failed to get Disk Drive information. Error code = %x\r\n", hr);
						CoUninitialize();
						hrInit = HRESULT_CODE(-1);
						retVal = 1;
					}
				}
				else
				{
					WriteLog(L"Failed to set proxy blanket. Error code = %x\r\n", hr);
					CoUninitialize();
					hrInit = HRESULT_CODE(-1);
					retVal = 2;
				}
			}
			else
			{
				WriteLog(L"Failed to connect to root namespace. Error code = %x\r\n", hr);
				CoUninitialize();
				hrInit = HRESULT_CODE(-1);
				retVal = 3;
			}
		}
		else
		{
			WriteLog(L"Failed to create IWbemLocator object. Error code = %x\r\n", hr);
			CoUninitialize();
			hrInit = HRESULT_CODE(-1);
			retVal = 4;
		}
	}
	else
	{
		WriteLog(L"Failed to set COM Security. Error code = %x\r\n", hrInitSec);
		CoUninitialize();
		hrInit = HRESULT_CODE(-1);
		retVal = 5;
	}

	CoUninitialize();
	hrInit = HRESULT_CODE(-1);

	return retVal;

}

extern "C" HWLIBRARY_API uint16_t GetAPM(wchar_t* dskName)
{
	WriteLog(L"GetAPM = %s\r\n", dskName);
	HANDLE hDrive = CreateFile(dskName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0);
	if (hDrive == INVALID_HANDLE_VALUE)
	{
		WriteLog(L"GetAPM: Invalid handle\r\n");
		return UINT16_MAX;
	}

	IDENTIFY_DEVICE_DATA hddid;
	BOOL rez = GetHDDId(hDrive, &hddid);
	if (!rez)
	{
		WriteLog(L"GetAPM: Invalid HDD Id. Error code = 0x%04X\r\n", GetLastError());
		return UINT16_MAX - 1;
	}

	if ((hddid.command_set_2 & 0x08) == 0)
	{
		WriteLog(L"GetAPM: Invalid CommandSet\r\n");
		return UINT16_MAX - 2;
	}

	uint16_t APMVal = hddid.CurAPMvalues & 0x00FF;

	return APMVal;
}

extern "C" HWLIBRARY_API int SetAPM(wchar_t* dskName, byte val, bool disable)
{
	HANDLE hDrive = CreateFile(dskName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0);
	if (hDrive == INVALID_HANDLE_VALUE)
		return 0;

	BOOL rez = SetAPM(hDrive, val, disable);
	return rez;
}

BOOL GetHDDId(HANDLE hDrive, IDENTIFY_DEVICE_DATA* hddid)
{
	BOOL rez = false;
	DWORD bytesRet;
	ATA_PASS_THROUGH_EX_WITH_BUFFERS aptexb;
	void* ptr;

	aptexb.AtaPassThroughEx.Length = sizeof ATA_PASS_THROUGH_EX;
	aptexb.AtaPassThroughEx.AtaFlags = ATA_FLAGS_DATA_IN;
	aptexb.AtaPassThroughEx.DataTransferLength = DataBufSize;
	aptexb.AtaPassThroughEx.DataBufferOffset = sizeof ATA_PASS_THROUGH_EX;

	IDEREGS CurrentTaskFile;
	CurrentTaskFile.bCommandReg = WIN_IDENTIFYDEVICE;
	CurrentTaskFile.bSectorCountReg = 1;
	memcpy_s(aptexb.AtaPassThroughEx.CurrentTaskFile, sizeof(aptexb.AtaPassThroughEx.CurrentTaskFile), &CurrentTaskFile, sizeof IDEREGS);
	aptexb.AtaPassThroughEx.TimeOutValue = 3;

	bytesRet = 0;
	rez = DeviceIoControl(hDrive, IOCTL_ATA_PASS_THROUGH, &aptexb, sizeof ATA_PASS_THROUGH_EX, &aptexb, sizeof ATA_PASS_THROUGH_EX_WITH_BUFFERS, &bytesRet, NULL);

	if (!rez)
		return 0;

	ptr = (void*)(&aptexb.ucDataBuf);
	memcpy(hddid, ptr, sizeof IDENTIFY_DEVICE_DATA);

	return rez;
}

BOOL SetAPM(HANDLE hDrive, BYTE APMVal, BOOL Disable)
{
	BOOL rez = false;
	DWORD bytesRet;
	ATA_PASS_THROUGH_EX_WITH_BUFFERS aptex;

	aptex.AtaPassThroughEx.Length = sizeof ATA_PASS_THROUGH_EX;
	aptex.AtaPassThroughEx.AtaFlags = ATA_FLAGS_DATA_IN;
	aptex.AtaPassThroughEx.DataTransferLength = DataBufSize;
	aptex.AtaPassThroughEx.DataBufferOffset = sizeof ATA_PASS_THROUGH_EX;
	aptex.AtaPassThroughEx.TimeOutValue = 1;

	IDEREGS CurrentTaskFile;
	CurrentTaskFile.bCommandReg = WIN_SETFEATURES;
	CurrentTaskFile.bSectorCountReg = 0;

	if (Disable)
	{
		CurrentTaskFile.bFeaturesReg = SETFEATURES_DIS_APM;
	}
	else
	{
		CurrentTaskFile.bFeaturesReg = SETFEATURES_EN_APM;
		CurrentTaskFile.bSectorCountReg = APMVal;
	}

	memcpy_s(aptex.AtaPassThroughEx.CurrentTaskFile, sizeof(aptex.AtaPassThroughEx.CurrentTaskFile), &CurrentTaskFile, sizeof IDEREGS);

	bytesRet = 0;
	rez = DeviceIoControl(hDrive, IOCTL_ATA_PASS_THROUGH, &aptex, sizeof ATA_PASS_THROUGH_EX, &aptex, sizeof ATA_PASS_THROUGH_EX_WITH_BUFFERS, &bytesRet, NULL);

	return rez;
}
