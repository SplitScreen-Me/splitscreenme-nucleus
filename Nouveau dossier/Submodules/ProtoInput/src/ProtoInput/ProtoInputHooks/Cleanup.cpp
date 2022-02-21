#include "Cleanup.h"
#include <cstdio>
#include "FocusMessageLoop.h"

void Proto::ProtoPerformCleanup()
{
	printf("Proto Input performing shutdown cleanup\n");

	FocusMessageLoop::Cleanup();
}
