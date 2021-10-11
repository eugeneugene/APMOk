#pragma once

#if _DEBUG
void WriteLog(const wchar_t* format, ...);
#else
inline void WriteLog(const wchar_t* format, ...) {}
#endif
