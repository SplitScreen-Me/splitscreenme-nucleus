using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    public class D_Joystick
    {
        public Action UnPlugged;
        public Joystick Joystick;
        public JoystickState State => GetState();
        public Guid InstanceGuid;
        public Guid ProductGuid;
        public string HIDPath;
        public string InstanceName;
        public int Index;

        private JoystickState GetState()
        {
            JoystickState state = Joystick?.GetCurrentState();

            if (state.Buttons[10])//Guide button
            {
                return new JoystickState();
            }

            return state;
        }
    }
}
