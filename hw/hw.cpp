#include "pch.h"
#include "hw.h"
#include "atahdd.h"

BOOL GeHDDId(HANDLE hDrive, IDENTIFY_DEVICE_DATA* hddid);
BOOL SetAPM(HANDLE hDrive, BYTE APMVal, BOOL Disable);

extern "C" HWLIBRARY_API int EnumerateDisks(void* ptr)
{
	EnumDiskInfo* diskInfo = (EnumDiskInfo*)ptr;
	HRESULT hr;
	hr = CoInitializeEx(0, COINIT_MULTITHREADED);

	if (FAILED(hr))
	{
		_RPTF0(_CRT_ERROR, L"Failed to initialize COM\n");
		return -1;
	}

	hr = CoInitializeSecurity(
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

	if (SUCCEEDED(hr))
	{
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
						CIMTYPE cType;

						UINT16 index = 0U;

						while (pEnumerator)
						{
							CComPtr<IWbemClassObject> pclsObj;
							CComVariant var;
							ULONG uReturn;

							EnumDiskInfo* info = &diskInfo[index];

							if (index == 16)
								break;

							hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);

							if (0 == uReturn)
								break;

							info->DiskIndex = index++;

							hr = pclsObj->Get(L"Availability", 0, &var, &cType, 0);
							info->Availability = var.uiVal;

							hr = pclsObj->Get(L"Caption", 0, &var, &cType, 0);
							wcscpy_s(info->Caption, var.bstrVal);

							hr = pclsObj->Get(L"ConfigManagerErrorCode", 0, &var, &cType, 0);
							info->ConfigManagerErrorCode = var.uintVal;

							hr = pclsObj->Get(L"Description", 0, &var, &cType, 0);
							wcscpy_s(info->Description, var.bstrVal);

							hr = pclsObj->Get(L"DeviceID", 0, &var, &cType, 0);
							wcscpy_s(info->DeviceID, var.bstrVal);

							hr = pclsObj->Get(L"Index", 0, &var, &cType, 0);
							info->Index = var.uintVal;

							hr = pclsObj->Get(L"InterfaceType", 0, &var, &cType, 0);
							wcscpy_s(info->InterfaceType, var.bstrVal);

							hr = pclsObj->Get(L"Manufacturer", 0, &var, &cType, 0);
							wcscpy_s(info->Manufacturer, var.bstrVal);

							hr = pclsObj->Get(L"Model", 0, &var, &cType, 0);
							wcscpy_s(info->Model, var.bstrVal);

							hr = pclsObj->Get(L"Name", 0, &var, &cType, 0);
							wcscpy_s(info->Name, var.bstrVal);

							hr = pclsObj->Get(L"SerialNumber", 0, &var, &cType, 0);
							wcscpy_s(info->SerialNumber, var.bstrVal);

							hr = pclsObj->Get(L"Status", 0, &var, &cType, 0);
							wcscpy_s(info->Status, var.bstrVal);
						}
						CoUninitialize();
						return index;
					}
					else
						_RPTFWN(_CRT_ERROR, L"Failed to get Disk Drive information. Error code = %x\n", hr);
				}
				else
					_RPTFWN(_CRT_ERROR, L"Failed to set proxy blanket. Error code = %x\n", hr);
			}
			else
				_RPTFWN(_CRT_ERROR, L"Failed to connect to root namespace. Error code = %x\n", hr);
		}
		else
			_RPTFWN(_CRT_ERROR, L"Failed to create IWbemLocator object. Error code = %x\n", hr);
	}
	else
		_RPTFWN(_CRT_ERROR, L"Failed to set COM Security. Error code = %x\n", hr);

	CoUninitialize();

	return -1;
}

extern "C" HWLIBRARY_API int GetAPM(wchar_t* dskName)
{
	HANDLE hDrive = CreateFile(dskName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0);
	if (hDrive == INVALID_HANDLE_VALUE)
		return -1;

	IDENTIFY_DEVICE_DATA hddid;
	BOOL rez = GeHDDId(hDrive, &hddid);
	if (!rez)
		return -2;

	if ((hddid.command_set_2 & 0x08) == 0)
		return -3;

	uint16_t APMVal = hddid.CurAPMvalues & 0x00FF;

	return APMVal;
}

extern "C" HWLIBRARY_API int SetAPM(wchar_t* dskName, byte val, bool disable)
{
	HANDLE hDrive = CreateFile(dskName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0);
	if (hDrive == INVALID_HANDLE_VALUE)
		return -1;

	BOOL rez = SetAPM(hDrive, val, disable);
	return rez;
}

BOOL GeHDDId(HANDLE hDrive, IDENTIFY_DEVICE_DATA* hddid)
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
	rez = DeviceIoControl(hDrive, IOCTL_ATA_PASS_THROUGH, &aptexb, sizeof ATA_PASS_THROUGH_EX, &aptexb,
		sizeof ATA_PASS_THROUGH_EX_WITH_BUFFERS, &bytesRet, NULL);

	if (rez == 0)
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
	rez = DeviceIoControl(hDrive, IOCTL_ATA_PASS_THROUGH, &aptex, sizeof ATA_PASS_THROUGH_EX, &aptex,
		sizeof ATA_PASS_THROUGH_EX_WITH_BUFFERS, &bytesRet, NULL);

	return rez;
}
