XInput Plus (ver4.15.2)

(c) 0dd14Lab 2011-2019
https://sites.google.com/site/0dd14lab/
* Redistribution of this program without permission is prohibited.

*********************************************************************************
** What's this 

XInputPlus is a tool which performs a detailed setup of a XInput (XBOX360 controller etc.) input. 
In the game using XInput, change of key assignment, sensitivity adjustment of a stick, automatic fire, etc. can be performed. 
And [DirectInput Output] Funtion.The function at the time of using it by DirectInput is extended. 
 

*********************************************************************************
** Usage

1. Run XInputPlus.exe. (need Admin permission) 

2. Set up exe file of game wants to apply XInputPlus to "Target Program".

3. Configure input settings with each tab, 

4. Click the "Total Test" button and Check settings. 

5. Click the Apply button. A setup is applied by TargetProgram. 

6. Execute the target program ordinarily.


*********************************************************************************
** Optional:using XInput Plus Loader

- Cautions ------------------------------------------------------- 
XInputPlusLoader injects XInputPlus into a program FORCIBLY.
Since a possibility of being detected by the Anti-Cheat is higher,
Using for OnlineGame is NOT RECOMMENDED. 
Please use it in self-responsibility. 
 -----------------------------------------------------------------
XInputPlusLoader is a launcher program which injects XInputPlus to a target program. 

1. Set up at XInputPlus.exe.
 
2. Perform "Apply Only ini File" from the menu button. 
"XInputPlus.ini" is copied to a target program folder. 

3. Execute "XInputPlusLoader.exe" in the "Loader" folder. 

4. Execute a Target program by Drag&Drop to the window of XInput Plus Loader. 


*********************************************************************************
** Changes
01-12-2019  Ver4.15.2
			Fix:
			 Change the recovery process after the controller connection is lost
 			 Changed to use XInput1_4.dll if XInput1_3.dll does not exist in the system
			etc
01-10-2017  Ver4.15
			Add:
			 [Switch Settings] Function (Buttons -> Switch Settings )
			 DirectInput [Disable Other Gaming Device] Option (DirectInput -> Advanced)
			 DirectInput [Disable XInput] Option (DirectInput -> Advanced)
	 	 	 DirectInput [Override Device Name] Option (DirectInput -> Advanced)
			Improve:
			 Improve DirectInput compatibility.
		   	 - GetDeviceData/GetDeviceState:Change behavior when device is lost during operation.
			 - GetCapabilities/EnumObjects: Change response of FFB suport flag and button number.
 			 - Release: Change vibration stop processing at device release.
			 - Change cooperation way between XInputDLL / DirectInputDLL
			 - Other
			 Change DirectInput handling in setting tool.
			 Discontinue requesting administrator authority at startup of setting tool.
			 Change icon and image of the setting tool.
			Fix:
			 Setting tool: fixed assignment of BACK/START buttons in DirectInput custom setting
 			 Setting tool: fixed "Disable Unmapped Object" option in DirectInput setting not saved correctly 
			 Other.

05-06-2016  Ver4.14.3	Fixed: issue of DirectInput custom mapping on setting tool.
01-12-2015  Ver4.14.2	Fixed: X-axis inversion option of the right stick was not working correctly.  
22-11-2015  Ver4.14.1	Fixed a dialog text.
21-11-2015  Ver4.14	Improve: Stability of XInput1_3.dll.
			Add: "CenterOffset","OutputScale" to sticks adjustment advenced parameters.
			And: "XInput1_4.Dll" option to Installed File (Provisional implementation).
			Add: x64-Dll option to the proxy Dll (x360ce cooperation)
			Add: Custom specified in key assignment of DirectInput. 
			Improve: Analog axis to target of SHIFT function in DirectInput.
			Improve: Compatibility of vibration (FFB emulation) in DirectInput.
			Add: Modification and expansion of XInputPlus.ini file format.
			Add: Support for high DPI environment.(Setting UI)
			etc

26-07-2014  Ver4.12	Add: "Rectangle Correction" to LS/RS.
			Improve DirectInput Output Function.(CreateDevice/GetDeviceData)
			Add: "Recent Target Programs" at SettingUI.
			Fixed: Linearity of the sticks / triggers did not correctly reflect.
			etc

27-05-2014  Ver4.10	Add XInputPlusLoader
			etc

30-04-2014  Ver4.9.1	Fixed: Issue caused by Windows locale(fr/es/...).
			Fixed: Issue caused by an incorrect value in ini file.

02-09-2014  Ver4.9	Add setting of SoundMessage Volume
			The start-up time of the target program which equipped the DirectInput output function is improved. 
			Fixed:x360ce cooperation (ProxyDLL) function was not operating correctly
			etc
08-12-2013  Ver4.8	Add Option which limits DPAD only in the four directions.
			Improve FFB Emulation 
17-11-2013  Ver4.7	Improve DirectInput-OutPut function.
			Fix Bug
			etc
17-08-2013  Ver4.5	Improve RapidFire/Hold Function
			Add Reset&Reload Function
			Add ControllerNumber Assgin Function
			Improve DirectInput Output Function
			Fix Bug
			etc
17-06-2013  Ver4.4.1 	Fix bug
16-06-2013  Ver4.4 	Improve Compatibility of a DirectInput Output function.(DirectInput5-8)
21-04-2013  Ver4.3.1 	Improve Compatibility of a DirectInput Output function.
30-03-2013  Ver4.3	Add support to 64bitProcess
20-03-2013  Ver4.2	Extend KeyAssignment of DirectInput Output
18-03-2013  Ver4.1.3 	Improve Compatibility of a DirectInput Output function.(POV Issue)
20-02-2013  Ver4.1.2 	Improve Compatibility of a DirectInput Output function.
16-02-2013  Ver4.1 	Improve Compatibility of a DirectInput Output function. 
10-02-2013  Ver4.0 	Add DirectInput OutPut Function
			Spec Change:Enable/Disable Key, before key assigning change is used. 
03-01-2013  Ver3.6	Add assign Multiple-Key function to LS/RS/DPAD.
			change internal logic.(Improvement in compatibility)
09-12-2012  Ver3.5	Add support to Guide-Button.
			Add Assign Multiple-Key Function 
			Add check function of a connection state of a controllers.
29-09-2012  Ver3.1	Add Option "Install File","Init beep"
28-04-2012  Ver3.0	Add TotalTest.In Rapid File/Hold, BACK/START/LT/RT can be specified now.  

*********************************************************************************

XInputPlusLoader uses Mhook Libraly.
Mhook is distributed under the MIT license.
------------------------------------------------------------------------------
MHOOK, AN API HOOKING LIBRARY
http://codefromthe70s.org/

Copyright (c) 2007-2008, Marton Anka
Portions Copyright (c) 2007, Matt Conover

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR 
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
OTHER DEALINGS IN THE SOFTWARE.
------------------------------------------------------------------------------

