#pragma once

// Need these praga comments so the functions are re-exported in the dll project
#ifdef _WIN64
#pragma comment(linker, "/export:LockInput")

#pragma comment(linker, "/export:SuspendExplorer")
#pragma comment(linker, "/export:RestartExplorer")

#pragma comment(linker, "/export:SetTaskbarVisibility")
#pragma comment(linker, "/export:GetTaskbarVisibility")
#else
#pragma comment(linker, "/export:_LockInput")

#pragma comment(linker, "/export:_SuspendExplorer")
#pragma comment(linker, "/export:_RestartExplorer")

#pragma comment(linker, "/export:_SetTaskbarVisibility")
#pragma comment(linker, "/export:_GetTaskbarVisibility")
#endif



#include "protoinpututil.h"