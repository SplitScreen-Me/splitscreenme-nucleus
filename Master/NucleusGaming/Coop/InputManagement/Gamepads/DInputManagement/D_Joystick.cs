using SharpDX.DirectInput;
using System;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    public class D_Joystick : IDisposable
    {
        public Joystick Joystick;
        public JoystickState State => GetState();
        public JoystickStatus Status;
        public Guid InstanceGuid;
        public Guid ProductGuid;
        public string VendorId;
        public string InterfacePath;
        public string InstanceName;
        public IntPtr NativePointer;
        public Guid Device_Guid;
        public bool Disposing;

        private JoystickState dummyState = new JoystickState();

        //The setup polls on the UI thread while the DirectInput watcher thread releases devices: never both at once.
        private readonly object stateLock = new object();

        private JoystickState GetState()
        {
            lock (stateLock)
            {
                JoystickState state;

                if (Disposing)
                {
                    return dummyState;
                }

                try
                {
                    state = Joystick?.GetCurrentState();
                }
                catch
                {
                    Dispose();
                    return dummyState;
                }

                return state ?? dummyState;
            }
        }

        /// <summary>Marks the device disconnected (its state reads as nothing pressed from now on). Does not release it.</summary>
        public void Dispose()
        {
            if (!Disposing)
            {
                Status = JoystickStatus.Disconnected;
                Disposing = true;
            }
        }

        /// <summary>Marks the device disconnected and releases the DirectInput device, without racing a state read.</summary>
        public void Release()
        {
            lock (stateLock)
            {
                Dispose();

                try
                {
                    Joystick?.Unacquire();
                    Joystick?.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
