using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Nucleus.Gaming;

namespace Nucleus.Inject
{
	class Program
	{
		class Injector32
		{
			[DllImport("EasyHook32.dll", CharSet = CharSet.Ansi)]
			public static extern int RhInjectLibrary(
				uint InTargetPID,
				uint InWakeUpTID,
				uint InInjectionOptions,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x86,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x64,
				IntPtr InPassThruBuffer,
				uint InPassThruSize
			);

			[DllImport("EasyHook32.dll", CharSet = CharSet.Ansi)]
			public static extern int RhCreateAndInject(
				[MarshalAs(UnmanagedType.LPWStr)] string InEXEPath,
				[MarshalAs(UnmanagedType.LPWStr)] string InCommandLine,
				uint InProcessCreationFlags,
				IntPtr InEnvironment,
				uint InInjectionOptions,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x86,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x64,
				IntPtr InPassThruBuffer,
				uint InPassThruSize,
				IntPtr OutProcessId //Pointer to a UINT (the PID of the new process)
			);
		}

		class Injector64
		{
			[DllImport("EasyHook64.dll", CharSet = CharSet.Ansi)]
			public static extern int RhInjectLibrary(
				uint InTargetPID,
				uint InWakeUpTID,
				uint InInjectionOptions,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x86,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x64,
				IntPtr InPassThruBuffer,
				uint InPassThruSize
			);

			[DllImport("EasyHook64.dll", CharSet = CharSet.Ansi)]
			public static extern int RhCreateAndInject(
				[MarshalAs(UnmanagedType.LPWStr)] string InEXEPath,
				[MarshalAs(UnmanagedType.LPWStr)] string InCommandLine,
				uint InProcessCreationFlags,
				IntPtr InEnvironment,
				uint InInjectionOptions,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x86,
				[MarshalAs(UnmanagedType.LPWStr)] string InLibraryPath_x64,
				IntPtr InPassThruBuffer,
				uint InPassThruSize,
				IntPtr OutProcessId //Pointer to a UINT (the PID of the new process)
			);
		}

		private static readonly IniFile
			ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

		private static void Log(string logMessage)
		{
			if (ini.IniReadValue("Misc", "DebugLog") == "True")
			{
				using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
				{
					writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]INJECT: {logMessage}");
					writer.Close();
				}
			}
		}

		static void Main(string[] args)
		{
			bool is64 = Environment.Is64BitProcess;

			var argsDecoded = new string[args.Length];
			for (int j = 0; j < args.Length; j++)
			{
				argsDecoded[j] = Encoding.UTF8.GetString(Convert.FromBase64String(args[j]));
			}

			int i = 0;
			int.TryParse(argsDecoded[i++], out int tier);

			if (tier == 0)
			{
				StartupHook(argsDecoded, i, is64);
			}
			else if (tier == 1)
			{
				try
				{


					RuntimeHook(argsDecoded, i, is64);
				}
				catch (Exception e)
				{
					Log("error = " + e);
					throw;
				}
			}
		}

		private static void StartupHook(string[] args, int i, bool is64)
		{
			string InEXEPath = args[i++];
			string InCommandLine = args[i++];
			uint.TryParse(args[i++], out uint InProcessCreationFlags);
			uint.TryParse(args[i++], out uint InInjectionOptions);
			string InLibraryPath_x86 = args[i++];
			string InLibraryPath_x64 = args[i++];
			bool.TryParse(args[i++], out bool hookWindow); // E.g. FindWindow, etc
			bool.TryParse(args[i++], out bool renameMutex);
			string mutexToRename = args[i++];
			bool.TryParse(args[i++], out bool setWindow);
			bool.TryParse(args[i++], out bool isDebug);
			string nucleusFolderPath = args[i++];
			bool.TryParse(args[i++], out bool blockRaw);
			bool.TryParse(args[i++], out bool useCustomEnvironment);
			string playerNick = args[i++];
			bool.TryParse(args[i++], out bool createSingle);
			string rawHid = args[i++];
			int.TryParse(args[i++], out int width);
			int.TryParse(args[i++], out int height);
			int.TryParse(args[i++], out int posx);
			int.TryParse(args[i++], out int posy);
			string docpath = args[i++];
			bool.TryParse(args[i++], out bool useDocs);

			//IntPtr InPassThruBuffer = Marshal.StringToHGlobalUni(args[i++]);
			//uint.TryParse(args[i++], out uint InPassThruSize);

			IntPtr envPtr = IntPtr.Zero;

			if (useCustomEnvironment)
			{
				Log("Setting up Nucleus environment");

				string NucleusEnvironmentRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				//string DocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

				IDictionary envVars = Environment.GetEnvironmentVariables();
				var sb = new StringBuilder();
				//var username = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"C:\Users\", "");
				string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
				envVars["USERPROFILE"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{playerNick}";
				envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{playerNick}";
				envVars["APPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{playerNick}\AppData\Roaming";
				envVars["LOCALAPPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{playerNick}\AppData\Local";

				//Some games will crash if the directories don't exist
				Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
				Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
				Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
				Directory.CreateDirectory(envVars["APPDATA"].ToString());
				Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());

				Directory.CreateDirectory(Path.GetDirectoryName(docpath) + $@"\NucleusCoop\{playerNick}\Documents");

				if (useDocs)
				{
					if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
					{
						//string mydocPath = key.GetValue("Personal").ToString();
						ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
					}

					RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
					dkey.SetValue("Personal", Path.GetDirectoryName(docpath) + $@"\NucleusCoop\{playerNick}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
				}


				foreach (object envVarKey in envVars.Keys)
				{
					if (envVarKey != null)
					{
						string key = envVarKey.ToString();
						string value = envVars[envVarKey].ToString();

						sb.Append(key);
						sb.Append("=");
						sb.Append(value);
						sb.Append("\0");
					}
				}

				sb.Append("\0");

				byte[] envBytes = Encoding.Unicode.GetBytes(sb.ToString());
				envPtr = Marshal.AllocHGlobal(envBytes.Length);
				Marshal.Copy(envBytes, 0, envPtr, envBytes.Length);

				Thread.Sleep(1000);
			}

			var logPath = Encoding.Unicode.GetBytes(nucleusFolderPath);
			int logPathLength = logPath.Length;

			var targetsBytes = Encoding.Unicode.GetBytes(mutexToRename);
			int targetsBytesLength = targetsBytes.Length;

			var rawHidBytes = Encoding.Unicode.GetBytes(rawHid);
			int rawHidBytesLength = rawHidBytes.Length;

			var playerNickBytes = Encoding.Unicode.GetBytes(playerNick);
			int playerNickLength = playerNickBytes.Length;

			int size = 48 + logPathLength + targetsBytesLength + rawHidBytesLength + playerNickLength;
			var data = new byte[size];
			data[0] = hookWindow == true ? (byte)1 : (byte)0;
			data[1] = renameMutex == true ? (byte)1 : (byte)0;
			data[2] = setWindow == true ? (byte)1 : (byte)0;
			data[3] = isDebug == true ? (byte)1 : (byte)0;
			data[4] = blockRaw == true ? (byte)1 : (byte)0;
			data[5] = createSingle == true ? (byte)1 : (byte)0;

			data[6] = (byte)(rawHidBytesLength >> 24);
			data[7] = (byte)(rawHidBytesLength >> 16);
			data[8] = (byte)(rawHidBytesLength >> 8);
			data[9] = (byte)rawHidBytesLength;

			data[10] = (byte)(logPathLength >> 24);
			data[11] = (byte)(logPathLength >> 16);
			data[12] = (byte)(logPathLength >> 8);
			data[13] = (byte)logPathLength;

			data[14] = (byte)(targetsBytesLength >> 24);
			data[15] = (byte)(targetsBytesLength >> 16);
			data[16] = (byte)(targetsBytesLength >> 8);
			data[17] = (byte)targetsBytesLength;

			data[18] = (byte)(width >> 24);
			data[19] = (byte)(width >> 16);
			data[20] = (byte)(width >> 8);
			data[21] = (byte)width;

			data[22] = (byte)(height >> 24);
			data[23] = (byte)(height >> 16);
			data[24] = (byte)(height >> 8);
			data[25] = (byte)height;

			data[26] = (byte)(posx >> 24);
			data[27] = (byte)(posx >> 16);
			data[28] = (byte)(posx >> 8);
			data[29] = (byte)posx;

			data[30] = (byte)(posy >> 24);
			data[31] = (byte)(posy >> 16);
			data[32] = (byte)(posy >> 8);
			data[33] = (byte)posy;

			data[34] = (byte)(playerNickLength >> 24);
			data[35] = (byte)(playerNickLength >> 16);
			data[36] = (byte)(playerNickLength >> 8);
			data[37] = (byte)playerNickLength;

			Array.Copy(logPath, 0, data, 38, logPathLength);

			Array.Copy(targetsBytes, 0, data, 39 + logPathLength, targetsBytesLength);

			Array.Copy(rawHidBytes, 0, data, 40 + logPathLength + targetsBytesLength, rawHidBytesLength);

			Array.Copy(playerNickBytes, 0, data, 41 + logPathLength + targetsBytesLength + rawHidBytesLength, playerNickLength);

			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(data, 0, ptr, size);

			IntPtr pid = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));

			bool isFailed = false;
			try
			{
				int result = -1;
				int attempts = 0; // 5 attempts to inject
				const int maxAttempts = 5;

				while (result != 0)
				{
					//if (procid > 0)
					//{
					//	string currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					//	if (is64)
					//		result = Injector64.RhInjectLibrary(procid, 0, 0, null, Path.Combine(currDir, "Nucleus.SHook64.dll"), ptr, (uint)size);
					//	else
					//		result = Injector32.RhInjectLibrary(procid, 0, 0, Path.Combine(currDir, "Nucleus.SHook32.dll"), null, ptr, (uint)size);
					//	if (result != 0)
					//	{
					//		Log("Attempt " + (attempts + 1) + "/5 Failed to inject start up hook dll. Result code: " + result);
					//	}
					//}
					//else
					//{
					if (is64)
					{
						//Log("is64 " + InEXEPath + " " + InCommandLine + " " + InProcessCreationFlags + " " + envPtr + " " + InInjectionOptions + " " + InLibraryPath_x64 + " " + ptr + " " + (uint)size + " " + pid);
						result = Injector64.RhCreateAndInject(InEXEPath, InCommandLine, InProcessCreationFlags, envPtr, InInjectionOptions, "", InLibraryPath_x64, ptr, (uint)size, pid);
					}

					else
					{
						//Log("isNOT64 " + InEXEPath + " " + InCommandLine + " " + InProcessCreationFlags + " " + envPtr + " " + InInjectionOptions + " " + InLibraryPath_x86 + " " + ptr + " " + (uint)size + " " + pid);
						result = Injector32.RhCreateAndInject(InEXEPath, InCommandLine, InProcessCreationFlags, envPtr, InInjectionOptions, InLibraryPath_x86, "", ptr, (uint)size, pid);
					}

					Thread.Sleep(1000);

					Log("CreateAndInject result code: " + result);
					if (result != 0)
					{
						Log("Attempt " + (attempts + 1) +
							"/5 Failed to create process and inject start up hook dll. Result code: " + result);
					}
					//}

					Thread.Sleep(1000);

					if (++attempts == maxAttempts)
					{
						isFailed = true;
						break;
					}
				}

				Marshal.FreeHGlobal(pid);

				if (isFailed)
				{
					Log("CreateAndInject failed");
					Console.WriteLine("injectfailed");

				}
				else
				{
					//if (procid == 0)
					Log("CreateAndInject successful, returning " + Marshal.ReadInt32(pid));
					Console.WriteLine(Marshal.ReadInt32(pid));

					//else
					//Console.WriteLine(procid);
				}
			}
			catch (Exception ex)
			{
				Log(string.Format("ERROR - {0}", ex.Message));
				Console.WriteLine("injectfailed");
			}

			//Marshal.FreeHGlobal(pid);

			//Console.WriteLine(Marshal.ReadInt32(pid).ToString());
		}


		private static void RuntimeHook(string[] args, int i, bool is64)
		{
			int.TryParse(args[i++], out int InTargetPID);
			int.TryParse(args[i++], out int InWakeUpTID);
			int.TryParse(args[i++], out int InInjectionOptions);
			string InLibraryPath_x86 = args[i++];
			string InLibraryPath_x64 = args[i++];
			//IntPtr InPassThruBuffer = Marshal.StringToHGlobalUni(args[i++]);
			int.TryParse(args[i++], out int hWnd);
			bool.TryParse(args[i++], out bool hookFocus);
			bool.TryParse(args[i++], out bool hideCursor);
			bool.TryParse(args[i++], out bool isDebug);
			string nucleusFolderPath = args[i++];
			bool.TryParse(args[i++], out bool setWindow);
			bool.TryParse(args[i++], out bool preventWindowDeactivation);

			int.TryParse(args[i++], out int width);
			int.TryParse(args[i++], out int height);
			int.TryParse(args[i++], out int posx);
			int.TryParse(args[i++], out int posy);

			int.TryParse(args[i++], out int controllerIndex);

			bool.TryParse(args[i++], out bool setCursorPos);
			bool.TryParse(args[i++], out bool getCursorPos);
			bool.TryParse(args[i++], out bool getKeyState);
			bool.TryParse(args[i++], out bool getAsyncKeyState);
			bool.TryParse(args[i++], out bool getKeyboardState);
			bool.TryParse(args[i++], out bool filterRawInput);
			bool.TryParse(args[i++], out bool filterMouseMessages);
			bool.TryParse(args[i++], out bool legacyInput);
			bool.TryParse(args[i++], out bool updateAbsoluteFlagInMouseMessage);
			bool.TryParse(args[i++], out bool mouseVisibilitySendBack);
			bool.TryParse(args[i++], out bool reRegisterRawInput);
			bool.TryParse(args[i++], out bool reRegisterRawInputMouse);
			bool.TryParse(args[i++], out bool reRegisterRawInputKeyboard);
			bool.TryParse(args[i++], out bool hookXinput);
			bool.TryParse(args[i++], out bool dinputToXinputTranslation);

			string writePipeName = args[i++];
			string readPipeName = args[i++];

			int.TryParse(args[i++], out int allowedRawMouseHandle);
			int.TryParse(args[i++], out int allowedRawKeyboardHandle);

			var logPath = Encoding.Unicode.GetBytes(nucleusFolderPath);
			int logPathLength = logPath.Length;
			//int.TryParse(args[i++], out int InPassThruSize);

			var writePipeNameBytes = Encoding.Unicode.GetBytes(writePipeName);
			int writePipeNameLength = writePipeNameBytes.Length;

			var readPipeNameBytes = Encoding.Unicode.GetBytes(readPipeName);
			int readPipeNameLength = readPipeNameBytes.Length;

			int size = 256 + logPathLength + writePipeNameLength + readPipeNameLength;
			IntPtr intPtr = Marshal.AllocHGlobal(size);
			byte[] dataToSend = new byte[size];

			int index = 0;
			dataToSend[index++] = (byte)(hWnd >> 24);
			dataToSend[index++] = (byte)(hWnd >> 16);
			dataToSend[index++] = (byte)(hWnd >> 8);
			dataToSend[index++] = (byte)(hWnd);

			dataToSend[index++] = (byte)(allowedRawMouseHandle >> 24);
			dataToSend[index++] = (byte)(allowedRawMouseHandle >> 16);
			dataToSend[index++] = (byte)(allowedRawMouseHandle >> 8);
			dataToSend[index++] = (byte)(allowedRawMouseHandle);

			dataToSend[index++] = (byte)(allowedRawKeyboardHandle >> 24);
			dataToSend[index++] = (byte)(allowedRawKeyboardHandle >> 16);
			dataToSend[index++] = (byte)(allowedRawKeyboardHandle >> 8);
			dataToSend[index++] = (byte)(allowedRawKeyboardHandle);

			byte Bool_1_0(bool x) => x ? (byte)1 : (byte)0;
			dataToSend[index++] = Bool_1_0(preventWindowDeactivation);
			dataToSend[index++] = Bool_1_0(setWindow);
			dataToSend[index++] = Bool_1_0(isDebug);
			dataToSend[index++] = Bool_1_0(hideCursor);
			dataToSend[index++] = Bool_1_0(hookFocus);

			dataToSend[index++] = Bool_1_0(setCursorPos);
			dataToSend[index++] = Bool_1_0(getCursorPos);
			dataToSend[index++] = Bool_1_0(getKeyState);
			dataToSend[index++] = Bool_1_0(getAsyncKeyState);
			dataToSend[index++] = Bool_1_0(getKeyboardState);
			dataToSend[index++] = Bool_1_0(filterRawInput);
			dataToSend[index++] = Bool_1_0(filterMouseMessages);
			dataToSend[index++] = Bool_1_0(legacyInput);
			dataToSend[index++] = Bool_1_0(updateAbsoluteFlagInMouseMessage);
			dataToSend[index++] = Bool_1_0(mouseVisibilitySendBack);
			dataToSend[index++] = Bool_1_0(reRegisterRawInput);
			dataToSend[index++] = Bool_1_0(reRegisterRawInputMouse);
			dataToSend[index++] = Bool_1_0(reRegisterRawInputKeyboard);
			dataToSend[index++] = Bool_1_0(hookXinput);
			dataToSend[index++] = Bool_1_0(dinputToXinputTranslation);

			dataToSend[index++] = (byte)(logPathLength >> 24);
			dataToSend[index++] = (byte)(logPathLength >> 16);
			dataToSend[index++] = (byte)(logPathLength >> 8);
			dataToSend[index++] = (byte)logPathLength;

			dataToSend[index++] = (byte)(writePipeNameLength >> 24);
			dataToSend[index++] = (byte)(writePipeNameLength >> 16);
			dataToSend[index++] = (byte)(writePipeNameLength >> 8);
			dataToSend[index++] = (byte)writePipeNameLength;

			dataToSend[index++] = (byte)(readPipeNameLength >> 24);
			dataToSend[index++] = (byte)(readPipeNameLength >> 16);
			dataToSend[index++] = (byte)(readPipeNameLength >> 8);
			dataToSend[index++] = (byte)readPipeNameLength;

			dataToSend[index++] = (byte)(width >> 24);
			dataToSend[index++] = (byte)(width >> 16);
			dataToSend[index++] = (byte)(width >> 8);
			dataToSend[index++] = (byte)width;

			dataToSend[index++] = (byte)(height >> 24);
			dataToSend[index++] = (byte)(height >> 16);
			dataToSend[index++] = (byte)(height >> 8);
			dataToSend[index++] = (byte)height;

			dataToSend[index++] = (byte)(posx >> 24);
			dataToSend[index++] = (byte)(posx >> 16);
			dataToSend[index++] = (byte)(posx >> 8);
			dataToSend[index++] = (byte)posx;

			dataToSend[index++] = (byte)(posy >> 24);
			dataToSend[index++] = (byte)(posy >> 16);
			dataToSend[index++] = (byte)(posy >> 8);
			dataToSend[index++] = (byte)posy;

			dataToSend[index++] = (byte)(controllerIndex >> 24);
			dataToSend[index++] = (byte)(controllerIndex >> 16);
			dataToSend[index++] = (byte)(controllerIndex >> 8);
			dataToSend[index++] = (byte)controllerIndex;

			Array.Copy(logPath, 0, dataToSend, index, logPathLength);
			Array.Copy(writePipeNameBytes, 0, dataToSend, index + logPathLength, writePipeNameLength);
			Array.Copy(readPipeNameBytes, 0, dataToSend, index + logPathLength + writePipeNameLength,
				readPipeNameLength);

			Marshal.Copy(dataToSend, 0, intPtr, size);

			try
			{
				if (is64)
				{
					Injector64.RhInjectLibrary((uint)InTargetPID, (uint)InWakeUpTID, (uint)InInjectionOptions, "",
						InLibraryPath_x64, intPtr, (uint)size);
				}
				else
				{
					Injector32.RhInjectLibrary((uint)InTargetPID, (uint)InWakeUpTID, (uint)InInjectionOptions,
						InLibraryPath_x86, "", intPtr, (uint)size);
				}
			}
			catch (Exception ex)
			{
				Log("ERROR - " + ex.Message);
			}
		}

		private static void ExportRegistry(string strKey, string filepath)
		{
			try
			{
				using (Process proc = new Process())
				{
					proc.StartInfo.FileName = "reg.exe";
					proc.StartInfo.UseShellExecute = false;
					proc.StartInfo.RedirectStandardOutput = true;
					proc.StartInfo.RedirectStandardError = true;
					proc.StartInfo.CreateNoWindow = true;
					proc.StartInfo.Arguments = "export \"" + strKey + "\" \"" + filepath + "\" /y";
					proc.Start();
					string stdout = proc.StandardOutput.ReadToEnd();
					string stderr = proc.StandardError.ReadToEnd();
					proc.WaitForExit();
				}
			}
			catch (Exception ex)
			{
				// handle exception
			}
		}
	}
}
