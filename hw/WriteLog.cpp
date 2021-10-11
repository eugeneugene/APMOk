#include "pch.h"

#if _DEBUG
void WriteLog(const wchar_t* format, ...)
{
	HANDLE hLog;
	hLog = ::CreateFileW(L"hw.log", FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

	if (hLog != INVALID_HANDLE_VALUE)
	{
		struct _timeb timebuffer;
		struct tm today;
		wchar_t tmpbuf[25];
		wchar_t millibuf[10];

		_ftime64_s(&timebuffer);
		_localtime64_s(&today, &timebuffer.time);
		DWORD i1 = (DWORD)wcsftime(tmpbuf, _countof(tmpbuf), L"%F %X", &today);
		::WriteFile(hLog, tmpbuf, i1 * 2, NULL, NULL);
		DWORD i2 = swprintf_s(millibuf, _countof(millibuf), L".%03hu ", timebuffer.millitm);
		::WriteFile(hLog, millibuf, i2 * 2, NULL, NULL);

		wchar_t buffer[256];
		va_list args;
		va_start(args, format);
		DWORD i3 = _vsnwprintf_s(buffer, _countof(buffer), format, args);
		::WriteFile(hLog, buffer, i3 * 2, NULL, NULL);
		va_end(args);
		::CloseHandle(hLog);
	}
}
#endif
