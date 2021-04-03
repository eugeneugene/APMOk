#include "pch.h"

void WriteLog(const wchar_t* format, ...);


BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_THREAD_ATTACH:
	case DLL_PROCESS_ATTACH:
		break;

	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

void WriteLog(const wchar_t* format, ...)
{
#if _DEBUG
	HANDLE hLog;
	hLog = ::CreateFileW(L"hw.log", FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

	if (hLog != INVALID_HANDLE_VALUE)
	{
		wchar_t buffer[256];
		va_list args;
		va_start(args, format);
		int i = _vsnwprintf_s(buffer, 256, format, args);
		::WriteFile(hLog, buffer, i << 1, NULL, NULL);
		va_end(args);
		::CloseHandle(hLog);
	}
#endif
}
