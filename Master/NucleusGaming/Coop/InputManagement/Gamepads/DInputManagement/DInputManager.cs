using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using SDL;
using SharpDX.DirectInput;

public static class DInputManager
{
    private static DirectInput dinput;

    private static List<D_Joystick> d_joysticksList = new List<D_Joystick>();
    private static IList<DeviceInstance> devicesList = new List<DeviceInstance>();

    public static List<D_Joystick> D_JoysticksList => d_joysticksList.AsReadOnly().ToList();

    private static bool INITIALIZED;

    public static void Init()
    {
        if(INITIALIZED)
        {
            return;
        }

        dinput = new DirectInput();

        Thread di_Loop = new Thread(() => WatchDevicesList());
        di_Loop.IsBackground = true;
        INITIALIZED = true;
        di_Loop.Start();    
    }

    public static void Refresh()
    {
        for (int i = 0; i < d_joysticksList.Count; i++)
        {
            d_joysticksList[i].Dispose();        
        }

        d_joysticksList.Clear();

        INITIALIZED = false;

        Init();
    }

    private static void WatchDevicesList()
    {
        InitJoystickList();

        while (INITIALIZED)
        {
           var watchList = dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

            // sCheck for newly connected devices
            for(int i = 0; i < watchList.Count; i++)
            {
                var device = watchList[i];

                if (dinput.IsDeviceAttached(device.InstanceGuid))
                {
                    if (d_joysticksList.All(d => d.InstanceGuid != device.InstanceGuid))
                    {
                        foreach (D_Joystick joy in d_joysticksList)
                        {
                            joy.Status = JoystickStatus.Disconnected;
                        }

                        UIThread.Post(() =>
                        {
                            Refresh();
                        });

                        return;
                    }
                }
            }

            //Check for disconnected devices
            for (int i = 0; i < devicesList.Count; i++)
            {
                var device = devicesList[i];
                if (!dinput.IsDeviceAttached(device.InstanceGuid))
                {
                    var d_Joystick = d_joysticksList.FirstOrDefault(d_j => d_j.Device_Guid == device.InstanceGuid);

                    if (d_Joystick != null)
                    {
                        DeleteJoystick(d_Joystick);
                    }

                    devicesList.RemoveAt(i);

                    if(i - 1 >= 0)
                    {
                        i--;
                    }        
                }
            }

            Thread.Sleep(1000);
        }
    }

    private static void InitJoystickList()
    {
        var devices = dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

        for (int i = 0; i < devices.Count; i++)
        {
            devicesList.Add(devices[i]);
            d_joysticksList.Add(MakeNewJoystick(devices[i]));
        }
    }

    private static void DeleteJoystick(D_Joystick d_Joystick)
    {
        d_Joystick.Status = JoystickStatus.Disconnected;
        d_joysticksList.Remove(d_Joystick);
        d_Joystick.Joystick.Dispose();
        d_Joystick.Dispose();
    }

    private static D_Joystick MakeNewJoystick(DeviceInstance device)
    {
        var joystick = new Joystick(dinput, device.InstanceGuid);

        D_Joystick d_Joystick = new D_Joystick();

        d_Joystick.Joystick = joystick;
        d_Joystick.InstanceName = joystick.Information.InstanceName;
        d_Joystick.InstanceGuid = joystick.Information.InstanceGuid;
        d_Joystick.ProductGuid = joystick.Information.ProductGuid;
        d_Joystick.InterfacePath = joystick.Properties.InterfacePath;
        d_Joystick.VendorId = joystick.Properties.VendorId.ToString();
        d_Joystick.NativePointer = joystick.NativePointer;
        d_Joystick.Device_Guid = device.InstanceGuid;

        d_Joystick.Joystick.Acquire();
        d_Joystick.Status = JoystickStatus.Connected;

        return d_Joystick;
    }

    public static bool Poll(PlayerInfo player)
    {
        if (player.DInputJoystick == null || !player.IsDInput)
        {
            return false;
        }

        if (player.DInputJoystick.Status == JoystickStatus.Disconnected)
        {
            return false;
        }

        try
        {
            if (App_Misc.VGMOnly)
            {
                if (!player.DInputJoystick.VendorId.StartsWith("202"))
                {
                   return false;
                }
            }

            if ((bool)player.DInputJoystick?.State.Buttons.Any(b => b == true))
            {
                player.IsInputUsed = true; 
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }
}
