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

        private JoystickState GetState()
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

            return state;
        }

        public void Dispose()
        {
            if (!Disposing)
            {
                Status = JoystickStatus.Disconnected;
                Disposing = true;
            }
        }
    }
}
