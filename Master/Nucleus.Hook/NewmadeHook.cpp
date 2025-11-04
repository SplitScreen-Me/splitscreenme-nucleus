#include "pch.h"
#include "InstallHooks.h"
//#include "Logging.h"
//#include <mmsystem.h>
//#include "controller.h"
//#pragma comment(lib, "winmm.lib")


//template for a new hook. search for "NewmadeHook", uncomment and rename in all these locations:

//locations Nucleus Hook: Hook.cpp, InstallHooks.h, Options.h.
//locations Nucleus Inject: Program.cs
//locations Nucleus Gaming: GenericContext.cs, GenericGameInfo.cs, IGenericGameInfo.cs, DllsInjector.cs


//this template now hooks winmm.dll's joystick ID, could be it is useful, not tested yet



//MMRESULT WINAPI NewmadeHook(UINT uJoyID, LPJOYINFOEX pji)
//{
   // MMRESULT resultat = 

  //  if (Controller::controllerIndex == 0) //MKB instance
  //      return joyGetPosEx(15, pji);

  //  else if (Controller::controllerIndex <= 14)
 //   return joyGetPosEx(Controller::controllerIndex - 1, pji);
//}
//MMRESULT WINAPI NewmadeHook2(UINT uJoyID, LPJOYINFO pji)
//{
 //   if (Controller::controllerIndex == 0)
//		return joyGetPos(14, pji); //MAX 14 but thats alright for MKB instance

  //  else if (Controller::controllerIndex <= 14)
  //      return joyGetPos(Controller::controllerIndex - 1, pji);
//} //also return anything above ID 14? highly unexpected error

//void installNewmadeHook()
//{
   // installHook("winmm", "joyGetPosEx", NewmadeHook);
   // installHook("winmm", "joyGetPos", NewmadeHook2);
//} 