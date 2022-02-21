#pragma once
#include "RawInputFilter.h"
#include "MouseMoveFilter.h"
#include "MouseActivateFilter.h"
#include "WindowActivateFilter.h"
#include "WindowActivateAppFilter.h"
#include "MouseWheelFilter.h"
#include "MouseButtonFilter.h"
#include "KeyboardButtonFilter.h"

namespace Proto
{

// Add all the filters to this macro so they show up in the menu and can be enabled through the pipe
#define PROTO_MESSAGE_FILTERS \
RawInputFilter, \
MouseMoveFilter, \
MouseActivateFilter, \
WindowActivateFilter, \
WindowActivateAppFilter, \
MouseWheelFilter, \
MouseButtonFilter, \
KeyboardButtonFilter

}