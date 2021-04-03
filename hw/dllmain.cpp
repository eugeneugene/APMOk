#include "pch.h"

HANDLE hLog;
void WriteLog(const wchar_t* format, ...);

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	hLog = INVALID_HANDLE_VALUE;

	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
#if _DEBUG
		hLog = ::CreateFileW(L"hw.log", GENERIC_WRITE, FILE_SHARE_READ, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
#endif
		break;
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		if (hLog != INVALID_HANDLE_VALUE)
			::CloseHandle(hLog);
		break;
	}
	return TRUE;
}

void WriteLog(const wchar_t* format, ...)
{
#if _DEBUG
	if (hLog != INVALID_HANDLE_VALUE)
	{
		wchar_t buffer[256];
		va_list args;
		va_start(args, format);
		int i = _vsnwprintf_s(buffer, 256, format, args);
		::WriteFile(hLog, buffer, i << 1, NULL, NULL);
		va_end(args);
	}
#endif
}
