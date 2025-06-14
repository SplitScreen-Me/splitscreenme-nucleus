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
    private static bool INITIALIZED;

    public static List<D_Joystick> D_JoysticksList => d_joysticksList.AsReadOnly().ToList();

    public static void Init(SynchronizationContext syncContext)
    {
        if(INITIALIZED)
        {
            return;
        }

        dinput = new DirectInput();

        Thread di_Loop = new Thread(() => WatchDevicesList(syncContext));
        di_Loop.IsBackground = true;
        INITIALIZED = true;
        di_Loop.Start();    
    }

    public static void Refresh(SynchronizationContext syncContext)
    {
        for (int i = 0; i < d_joysticksList.Count; i++)
        {
            d_joysticksList[i].Dispose();        
        }

        d_joysticksList.Clear();

        INITIALIZED = false;

        Init(syncContext);
    }

    private static void Refresh_DInput_CallBack(SynchronizationContext syncContext)
    {
        // Post the method callback to DevicesFunctions thread(main thread)
        syncContext.Post(_ =>
        {
            DevicesFunctions.RefreshDInput(syncContext);

        }, null);
    }

    private static void WatchDevicesList(SynchronizationContext syncContext)
    {
        InitJoystickList();

        while (INITIALIZED)
        {
           var watchList = dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

            // Check for newly connected devices
            foreach (var device in watchList)
            {
                if (dinput.IsDeviceAttached(device.InstanceGuid))
                {
                    if (d_joysticksList.All(d => d.InstanceGuid != device.InstanceGuid))
                    {
                        foreach (D_Joystick joy in d_joysticksList)
                        {
                            joy.Status = JoystickStatus.Disconnected;
                        }

                        Refresh_DInput_CallBack(syncContext);

                        return;
                    }
                }
            }

            // Check for disconnected devices
            var deviceToRemove = new List<DeviceInstance>();

            foreach (var device in devicesList)
            {
                if (!dinput.IsDeviceAttached(device.InstanceGuid))
                {
                    deviceToRemove.Add(device);

                    var joystick = d_joysticksList.FirstOrDefault(j => j.Device_Guid == device.InstanceGuid);
                    if (joystick != null)
                    {
                        DeleteJoystick(joystick);
                    }
                }
            }

            foreach (var device in deviceToRemove)
            {
                devicesList.Remove(device);
            }

            var disconnectedJoystick = d_joysticksList.Where(dj => dj.Status == JoystickStatus.Disconnected);

            if (disconnectedJoystick.Count() > 0)
            {
                foreach (D_Joystick joy in disconnectedJoystick)
                {
                    joy.Dispose();
                    d_joysticksList.Remove(joy);
                }
            }

            Thread.Sleep(500);
        }
    }

    private static void InitJoystickList()
    {
        var devices = dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

        foreach (var device in devices)
        {
            devicesList.Add(device);
            d_joysticksList.Add(MakeNewJoystick(device));
        }
    }

    private static void AddJoystick(DeviceInstance device)
    {
        d_joysticksList.Add(MakeNewJoystick(device));
    }

    private static void DeleteJoystick(D_Joystick joystick)
    {
        joystick.Status = JoystickStatus.Disconnected;
        d_joysticksList.Remove(joystick);
        joystick.Joystick.Dispose();
        joystick.Dispose();
    }

    public static void DeletePlayerDInputJoystick(D_Joystick joystick)
    {
        joystick.Status = JoystickStatus.Disconnected;
        d_joysticksList.Remove(joystick);
        joystick.Joystick.Dispose();
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
