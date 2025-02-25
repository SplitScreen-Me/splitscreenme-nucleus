using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SharpDX.DirectInput;

public static class DInput
{
    private static DirectInput dinput;
    private static Thread watchDevicesListThread;
    private static readonly object _joystickLock = new object();
    private static List<Joystick> d_joysticksList = new List<Joystick>();
    private static IList<DeviceInstance> devicesList = new List<DeviceInstance>();

    public static List<Joystick> D_JoysticksList => GetJoystickList();

    public static bool Init()
    {
        if (dinput == null)
        {
            if (dinput == null) 
            {
                dinput = new DirectInput();
                watchDevicesListThread = new Thread(WatchDevicesList)
                {
                    IsBackground = true 
                };
                watchDevicesListThread.Start();
            }
        }

        return true;
    }

    private static List<Joystick> GetJoystickList()
    {
        lock (_joystickLock)
        {
            return new List<Joystick>(d_joysticksList);
        }
    }

    private static void WatchDevicesList()
    {
        while (true)
        {
            IList<DeviceInstance> watchList;

            lock (_joystickLock)
            {
                watchList = dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

                // Check for newly connected devices
                foreach (var device in watchList)
                {

                    if (devicesList.All(d => d.InstanceGuid != device.InstanceGuid))
                    {
                        devicesList.Add(device);
                        AddJoystick(device);
                    }

                }

                // Check for disconnected devices
                var deviceToRemove = new List<DeviceInstance>();

                foreach (var device in devicesList)
                {
                    if (!dinput.IsDeviceAttached(device.InstanceGuid))
                    {
                        deviceToRemove.Add(device);

                        var joystick = d_joysticksList.FirstOrDefault(j => j.Information.InstanceGuid == device.InstanceGuid);
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
            }

            Thread.Sleep(200);
        }
    }

    private static void InitJoystickList(IList<DeviceInstance> _devicesIList)
    {
        lock (_joystickLock)
        {
            foreach (var device in _devicesIList)
            {
                d_joysticksList.Add(MakeNewJoystick(device));
            }
        }
    }

    public static bool GetJoystickState(Joystick joystick)
    {
        lock (_joystickLock)
        {
            int index = d_joysticksList.IndexOf(joystick);

            if (index == -1)
            {
                return false;
            }

            Joystick joystickList = d_joysticksList[index];

            if (joystick.IsDisposed || joystick == null)
            {
                return false;
            }
            else if (dinput.IsDeviceAttached(joystickList.Information.InstanceGuid))
            {
                try
                {
                    if (joystickList.GetCurrentState().Buttons.Any(b => b))
                    {
                        return true;
                    }
                }
                catch (SharpDX.SharpDXException)
                {
                    d_joysticksList.Remove(joystickList);
                    return false;
                }
            }
            else
            {
                d_joysticksList.Remove(joystickList);
            }

            return false;
        }
    }

    private static void AddJoystick(DeviceInstance device)
    {
        lock (_joystickLock)
        {
            d_joysticksList.Add(MakeNewJoystick(device));
        }
    }

    private static void DeleteJoystick(Joystick joystick)
    {
        lock (_joystickLock)
        {
            d_joysticksList.Remove(joystick);
            joystick.Unacquire();
            joystick.Dispose();
        }
    }

    public static void DeletePlayerDInputJoystick(Joystick joystick)
    {
        lock (_joystickLock)
        {
            d_joysticksList.Remove(joystick);
        }
    }

    private static Joystick MakeNewJoystick(DeviceInstance device)
    {
        var joystick = new Joystick(dinput, device.InstanceGuid);
        joystick.Acquire();
        return joystick;
    }
}
