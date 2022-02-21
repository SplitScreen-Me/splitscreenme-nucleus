# OpenXinput
This library can help you manage more than 4 xinput gamepads on Windows.

## Why ?
Because the legit Xinput library can only handle 4 XUSB devices while the underlying driver can handle more, with this library, you can use more than 4 controllers if you want to.

## Is it breaking Xinput compatibility ?
It is not breaking the compatibility, its a complete rewrite and should match the original library behavior.\
If you want even more compatibility with legit Xinput, you can check the library you are currently using (Xinput/OpenXinput) (See [How to use](#finding-max-controller-count)).


## How to use ?
### Finding max controller count.
```
DWORD XinputMaxControllerCount = XUSER_MAX_COUNT;
HMODULE hXinput = GetModuleHandleW(L"Xinput1_4.dll");
if (hXinput != NULL)
{
    XInputGetMaxControllerCount_t* pfnXInputGetMaxControllerCount_t = (XInputGetMaxControllerCount_t*)GetProcAddress(hXinput, "XInputGetMaxControllerCount");
    if (pfnXInputGetMaxControllerCount_t != nullptr)
    {// We are using OpenXinput, check for the max controller count.
        XinputMaxControllerCount = pfnXInputGetMaxControllerCount_t();
    }
}
```

After that, instead of doing the classic:
```
XINPUT_STATE gamepad_state;
for(DWORD player = 0; player < XUSER_MAX_COUNT; ++player)
{
  if (XInputGetState(i, &gamepad_state) == ERROR_SUCCESS)
  {
    // Do something
  }
}
```
do this:
```
XINPUT_STATE gamepad_state;
for(DWORD player = 0; player < XinputMaxControllerCount; ++player)
{
  if (XInputGetState(i, &gamepad_state) == ERROR_SUCCESS)
  {
    // Do something
  }
}
```

### Building the library.
It obviously will not work on Linux/MacOS (for some cross-platform Xinput like library, see my [gamepad library](https://gitlab.com/Nemirtingas/gamepad)).

Here are some options you can customize when building the library:
  - OPENXINPUT_NO_XINPUT_HEADER: Disables legit Xinput.h include. (Windows.h will still be included as it is required.)

  - OPENXINPUT_XUSER_MAX_COUNT: defaults = 4, Cannot be less than 4(for legit Xinput compatibility).\
Customize this to allow for more controllers (the dwUserIndex). Check XInputGetMaxControllerCount.

Theses options are used at compile time:
  - OPENXINPUT_STATIC_GUIDS: Use static versions of all UUIDs used by OpenXinput (for more compatility).\
Note: If Microsoft removed theses UUIDs from the system, using the static version will still not work, but you'll be able to use/compile the OpenXinput library.

  - OPENXINPUT_DISABLE_COM: Disable calls to COM objects.
XInput 1.4 seems to use some COM object to open the XInput device, setting this will disable this behavior.


## Minimal implementation
To implement a minimal gamepad library using XUSB devices (XBox protocol) without using Xinput.\
An example can be found in my gamepad library here: https://gitlab.com/Nemirtingas/gamepad/-/blob/master/gamepad.cpp