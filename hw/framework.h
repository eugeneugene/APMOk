#pragma once

#define _WIN32_WINNT_WIN10                  0x0A00 // Windows 10

#define WINVER			_WIN32_WINNT_WIN10                  
#define _WIN32_WINNT	_WIN32_WINNT_WIN10                  
#define _WIN32_IE		_WIN32_WINNT_WIN10
#define _RICHEDIT_VER	_WIN32_WINNT_WIN10
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#include <windows.h>
