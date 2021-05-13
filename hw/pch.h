#ifndef PCH_H
#define PCH_H

// add headers that you want to pre-compile here
#include "framework.h"

#include <stdio.h>
#include <stdarg.h>
#include <tchar.h>
#include <atlbase.h>
#include <sys/timeb.h>
#include <time.h>

#include <Wbemidl.h>
#include <winioctl.h>
#include <ntddscsi.h>

#if defined(_DEBUG)
#define BOOST_LIB_DIAGNOSTIC
#include <crtdbg.h>
#endif

#endif //PCH_H
