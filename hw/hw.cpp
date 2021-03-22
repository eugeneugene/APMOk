#include "pch.h"
#include "hw.h"

extern "C" HWLIBRARY_API int EnumerateDisks(EnumDiskInfo diskInfo[])
{
	HRESULT hr;
	hr = CoInitializeEx(0, COINIT_MULTITHREADED);

	if (FAILED(hr))
	{
		_RPTF0(_CRT_ERROR, _T("Failed to initialize COM\n"));
		return 1;
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

							EnumDiskInfo* info = &diskInfo[index++];

							if (index == 16)
								break;

							hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);

							if (0 == uReturn)
								break;

							info->DiskIndex = index;

							hr = pclsObj->Get(_T("Availability"), 0, &var, &cType, 0);
							info->Availability = var.uiVal;

							hr = pclsObj->Get(_T("Caption"), 0, &var, &cType, 0);
							_tcscpy_s(info->Caption, var.bstrVal);

							hr = pclsObj->Get(_T("ConfigManagerErrorCode"), 0, &var, &cType, 0);
							info->ConfigManagerErrorCode = var.uintVal;

							hr = pclsObj->Get(_T("Description"), 0, &var, &cType, 0);
							_tcscpy_s(info->Description, var.bstrVal);

							hr = pclsObj->Get(_T("DeviceID"), 0, &var, &cType, 0);
							_tcscpy_s(info->DeviceID, var.bstrVal);

							hr = pclsObj->Get(_T("Index"), 0, &var, &cType, 0);
							info->Index = var.uintVal;

							hr = pclsObj->Get(_T("InterfaceType"), 0, &var, &cType, 0);
							_tcscpy_s(info->InterfaceType, var.bstrVal);

							hr = pclsObj->Get(_T("Manufacturer"), 0, &var, &cType, 0);
							_tcscpy_s(info->Manufacturer, var.bstrVal);

							hr = pclsObj->Get(_T("Model"), 0, &var, &cType, 0);
							_tcscpy_s(info->Model, var.bstrVal);

							hr = pclsObj->Get(_T("Name"), 0, &var, &cType, 0);
							_tcscpy_s(info->Name, var.bstrVal);

							hr = pclsObj->Get(_T("SerialNumber"), 0, &var, &cType, 0);
							_tcscpy_s(info->SerialNumber, var.bstrVal);

							hr = pclsObj->Get(_T("Status"), 0, &var, &cType, 0);
							_tcscpy_s(info->Status, var.bstrVal);
						}
						CoUninitialize();
						return 0;
					}
					else
						_RPTFWN(_CRT_ERROR, _T("Failed to get Disk Drive information. Error code = %x\n"), hr);
				}
				else
					_RPTFWN(_CRT_ERROR, _T("Failed to set proxy blanket. Error code = %x\n"), hr);
			}
			else
				_RPTFWN(_CRT_ERROR, _T("Failed to connect to root namespace. Error code = %x\n"), hr);
		}
		else
			_RPTFWN(_CRT_ERROR, _T("Failed to create IWbemLocator object. Error code = %x\n"), hr);
	}
	else
		_RPTFWN(_CRT_ERROR, _T("Failed to set COM Security. Error code = %x\n"), hr);

	CoUninitialize();

	return 0;
}