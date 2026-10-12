using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using SDL;
using SharpDX.DirectInput;

public static class DInputManager
{
    private static DirectInput dinput;

    //Guards the two lists and the generation: the watcher thread changes them while the setup reads them on the UI thread.
    private static readonly object sync = new object();

    private static List<D_Joystick> d_joysticksList = new List<D_Joystick>();
    private static List<DeviceInstance> devicesList = new List<DeviceInstance>();
    private static bool INITIALIZED;

    //Each Init starts one watcher thread for the current generation; a refresh bumps it, so the previous watcher exits
    //(it used to keep looping: one more thread per "Refresh devices", all changing the lists).
    private static int generation;

    private static int activeWatchers;

    /// <summary>Running watcher threads: 1 after Init, whatever the number of refreshes (0 before Init).</summary>
    public static int ActiveWatchers => Volatile.Read(ref activeWatchers);

    /// <summary>A copy of the DirectInput devices (safe to iterate while the watcher thread updates the list).</summary>
    public static List<D_Joystick> D_JoysticksList
    {
        get
        {
            lock (sync)
            {
                return d_joysticksList.ToList();
            }
        }
    }

    public static void Init(SynchronizationContext syncContext)
    {
        DirectInput di;
        int gen;

        lock (sync)
        {
            if (INITIALIZED)
            {
                return;
            }

            di = dinput = new DirectInput();
            gen = ++generation;
            INITIALIZED = true;
        }

        Thread di_Loop = new Thread(() => WatchDevicesList(syncContext, di, gen))
        {
            IsBackground = true,
            Name = "Nucleus DirectInput watcher",
        };

        di_Loop.Start();
    }

    public static void Refresh(SynchronizationContext syncContext)
    {
        lock (sync)
        {
            for (int i = 0; i < d_joysticksList.Count; i++)
            {
                d_joysticksList[i].Release();
            }

            d_joysticksList.Clear();
            devicesList.Clear();

            generation++;//stops the current watcher
            INITIALIZED = false;
        }

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

    private static bool IsCurrent(int gen)
    {
        lock (sync)
        {
            return gen == generation && INITIALIZED;
        }
    }

    private static void WatchDevicesList(SynchronizationContext syncContext, DirectInput di, int gen)
    {
        //Devices that can't be opened: not reported as "new" on every pass (that would refresh everything twice a second).
        HashSet<Guid> failed = new HashSet<Guid>();

        Interlocked.Increment(ref activeWatchers);

        try
        {
            InitJoystickList(di, gen, failed);

            while (IsCurrent(gen))
            {
                IList<DeviceInstance> watchList = di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
                bool refresh = false;

                lock (sync)
                {
                    if (gen != generation)
                    {
                        return;
                    }

                    // Check for newly connected devices
                    for (int i = 0; i < watchList.Count; i++)
                    {
                        DeviceInstance device = watchList[i];

                        if (failed.Contains(device.InstanceGuid) || !di.IsDeviceAttached(device.InstanceGuid))
                        {
                            continue;
                        }

                        if (d_joysticksList.All(d => d.InstanceGuid != device.InstanceGuid))
                        {
                            foreach (D_Joystick joy in d_joysticksList)
                            {
                                joy.Status = JoystickStatus.Disconnected;
                            }

                            refresh = true;
                            break;
                        }
                    }

                    if (!refresh)
                    {
                        // Check for disconnected devices
                        List<DeviceInstance> deviceToRemove = devicesList.Where(device => !di.IsDeviceAttached(device.InstanceGuid)).ToList();

                        foreach (DeviceInstance device in deviceToRemove)
                        {
                            devicesList.Remove(device);

                            D_Joystick joystick = d_joysticksList.FirstOrDefault(j => j.Device_Guid == device.InstanceGuid);

                            if (joystick != null)
                            {
                                d_joysticksList.Remove(joystick);
                                joystick.Release();
                            }
                        }

                        foreach (D_Joystick joy in d_joysticksList.Where(dj => dj.Status == JoystickStatus.Disconnected).ToList())
                        {
                            d_joysticksList.Remove(joy);
                            joy.Release();
                        }
                    }
                }

                if (refresh)
                {
                    Refresh_DInput_CallBack(syncContext);
                    return;
                }

                Thread.Sleep(500);
            }
        }
        catch (Exception e)
        {
            //A background thread exception would end the process: log it, the next refresh starts a new watcher.
            try
            {
                LogManager.Log("DirectInput watcher stopped: " + e.Message);
            }
            catch (Exception)
            {
            }
        }
        finally
        {
            Interlocked.Decrement(ref activeWatchers);
        }
    }

    private static void InitJoystickList(DirectInput di, int gen, HashSet<Guid> failed)
    {
        IList<DeviceInstance> devices = di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

        for (int i = 0; i < devices.Count; i++)
        {
            D_Joystick joystick;

            try
            {
                joystick = MakeNewJoystick(di, devices[i]);
            }
            catch (Exception)
            {
                failed.Add(devices[i].InstanceGuid);
                continue;
            }

            lock (sync)
            {
                if (gen != generation)
                {
                    joystick.Release();
                    return;
                }

                devicesList.Add(devices[i]);
                d_joysticksList.Add(joystick);
            }
        }
    }

    private static D_Joystick MakeNewJoystick(DirectInput di, DeviceInstance device)
    {
        var joystick = new Joystick(di, device.InstanceGuid);

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
