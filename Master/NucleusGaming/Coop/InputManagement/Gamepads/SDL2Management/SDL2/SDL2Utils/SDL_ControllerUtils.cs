
using Gamepads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;

namespace SDL
{
    public unsafe static class SDL_ControllerUtils
    {
        private static string dbsDirectory = Path.Combine(Application.StartupPath, "GameControllerDB");
        /// <summary>
        /// Dictionary<string []{controller guid,mapping file name},string mapping string>
        /// </summary>
        private static Dictionary<string[],string> mappingsRef = new Dictionary<string[], string>();

        public class SDL_DeviceInfo
        {
            public SDL_Joystick JOYSTICK;
            public string NAME;
            public string HIDPATH;
            public Guid GUID;
            public string GUIDSTRING;
            public string MAPPINGSTRING;
            public JoystickID JOYSTICKID;
            public Guid DEVICEGUID;
            public string SERIAL;
            public string VENDORID;
        }

        public static void LoadUserControllerMappings()
        { 
            //<reminder: app that help creating mappings
            //https://gitlab.com/ryochan7/sdl2-gamepad-mapper/-/releases
            if (Directory.Exists(dbsDirectory))
            {
                var allDBFiles = Directory.GetFileSystemEntries(dbsDirectory);

                foreach (var dbFile in allDBFiles)
                {
                    if (File.Exists(dbFile))
                    {
                        try
                        {
                            string[] lines = File.ReadAllLines(dbFile);

                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (i == 1)
                                {
                                    string fileName = dbFile.Split('\\').Last();
                                    mappingsRef.Add(new string[] { GetGuidFromMappingString(lines[i]), fileName},lines[i]);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }       

        public static void LoadControllerMapping(SDL_GameController controller)
        {
            if (mappingsRef.Count != 0)
            {
                string report = string.Empty;

                SDL_DeviceInfo info = GetSDL_DeviceInfo(controller);

                foreach (KeyValuePair<string[], string> key in mappingsRef)
                {
                    if (key.Key[0] == info.GUIDSTRING)
                    {
                        int result = SDL2.GameControllerAddMapping(key.Value);

                        switch (result)
                        {
                            case -1:
                                report = $"Failed to load \"{key.Key[1]}\" for \n\n\"{info.NAME}\"";
                                break;
                            case 0:

                                byte* updMapping = SDL2.GameControllerMappingForGUID(info.GUID);
                                string s_updMapping = SDL2.GetString(updMapping);
                                report = $"Succefully updated mapping from \"{key.Key[1]}\" for \n\n\"{info.NAME}\" to \n\n" +
                                    $"{s_updMapping}";
                                break;
                            case 1:

                                byte* newMapping = SDL2.GameControllerMappingForGUID(info.GUID);
                                string s_newMapping = SDL2.GetString(newMapping);
                                report = $"Succefully loaded new mapping from \"{key.Key[1]}\" for \n\n\"{info.NAME}\"\n\n" +
                                    $"{s_newMapping}";
                                break;
                        }
                    }
                }

                return;
            }
        }

        public static void RefreshControllerMappings()
        {
            LoadUserControllerMappings();

            foreach (SDL_GameController controller in SDLManager.SDL2DevicesList)
            {
                LoadControllerMapping(controller);
            }
        }

        public static SDL_DeviceInfo GetSDL_DeviceInfo(SDL_GameController controller)
        {
            SDL_DeviceInfo info = new SDL_DeviceInfo();

            SDL_Joystick joy = GetGamepadJoystick(controller);
            info.JOYSTICK = joy;
            info.JOYSTICKID = GetJoystickID(controller);
            info.NAME = SDL2.GetString(SDL2.GameControllerName(controller));
            info.HIDPATH = SDL2.GetString(SDL2.SDL_GameControllerPath(controller));
            info.GUID = SDL2.JoystickGetGUID(joy);
            info.GUIDSTRING = GetGuidString(info.GUID);
            info.MAPPINGSTRING = SDL2.GetString(SDL2.GameControllerMappingForGUID(info.GUID));

            int index = SDLManager.SDL2DevicesList.IndexOf(controller);
            if (SDLManager.SDL2DevicesList.Contains(controller))
            {
                if(index != -1)
                {
                    info.DEVICEGUID = SDL2.JoystickGetDeviceGUID(index);
                }
            }

            info.SERIAL = GetGameControllerSerial((IntPtr)controller);
            info.VENDORID = SDL2.JoystickGetDeviceVendor(index).ToString();
            return info;
        }


        public static string GetGameControllerSerial(IntPtr gamecontroller)
        {
            IntPtr serialPtr = SDL2.GameControllerGetSerial(gamecontroller);
            return Marshal.PtrToStringAnsi(serialPtr);
        }
        public static void ExportControllerMapping(SDL_GameController controller)
        {
            SDL_DeviceInfo info = GetSDL_DeviceInfo(controller);
            string path = Path.Combine(dbsDirectory,info.NAME + ".txt");

            if(File.Exists(path))
            {
                int i = 0;

                while(File.Exists(path))
                {
                    ++i;
                    path = Path.Combine(dbsDirectory, info.NAME + "_" +  i.ToString() + ".txt");
                }
            }

            File.WriteAllText(path, info.MAPPINGSTRING);
        }

        public static void LogDeviceInfo(SDL_GameController controller)
        {
            SDL_DeviceInfo devInfo = GetSDL_DeviceInfo(controller);

            StringBuilder sb = new StringBuilder();           
            sb.Append("\nFound Device Info\n");
            sb.Append("_ _ _ _ _ _ _ _ _ _ _ _ _\n\n");
            sb.Append($"Name => {devInfo.NAME}\n");
            sb.Append($"Hid path => {devInfo.HIDPATH}\n");
            sb.Append($"Id => {(int)devInfo.JOYSTICKID}\n");
            sb.Append($"Guid => {devInfo.GUIDSTRING}\n");
            sb.Append($"Device Guid => {devInfo.DEVICEGUID}\n");
            sb.Append($"Device Serial number => {devInfo.SERIAL}\n");
            sb.Append($"Vendor Id => {devInfo.VENDORID}\n");
            sb.Append($"_ _ _ _ _ _ _ _ _ _ _ _ _\n");
            Console.WriteLine(sb.ToString());
        }

        private static JoystickID GetJoystickID(SDL_GameController controller)
        {
            SDL_Joystick joy = SDL2.GameControllerGetJoystick(controller);
            JoystickID instance_ID = SDL2.JoystickInstanceID(joy);
            return instance_ID;
        }

        private static SDL_Joystick GetGamepadJoystick(SDL_GameController controller)
        {
            SDL_Joystick joy = SDL2.GameControllerGetJoystick(controller);
            return joy;
        }

        private static Guid GetGamepadJoystickGuid(int i)
        {
            Guid joy = SDL2.JoystickGetDeviceGUID(i);
            return joy;
        }

        public static string GetGuidString(Guid guid)
        {
            //Allocate a buffer for the GUID string
            byte[] guidStringBuffer = new byte[33]; // 32 chars for GUID + 1 null terminator

            //Pin the array to get a pointer
            unsafe
            {
                fixed (byte* pGuidStr = guidStringBuffer)
                {
                    // Call the SDL function
                    SDL2.JoystickGetGUIDString(guid, pGuidStr, guidStringBuffer.Length);
                }
            }

            // Convert the byte array to string, trimming the null terminator
            return System.Text.Encoding.ASCII.GetString(guidStringBuffer).TrimEnd('\0');
        }

        private static string GetGuidFromMappingString(string mappingString)
        {
            return mappingString.Split(',')[0];
        }

        /// <summary>
        /// Send Rumbles to the controller passed as argument, 
        /// rumbles the triggers if the controller supports it.
        /// </summary>
        /// <param name="controller"></param>
        public static void SendRumble(SDL_GameController controller)
        {
            //Rumble  min = 0 max = 65535
            if (SDL2.SDL_GameControllerRumbleTriggers(controller, 10000, 10000, 70) == -1)
            {
                SDL2.SDL_GameControllerRumble(controller, 10000, 10000, 70);
            }
        }
    }
}
