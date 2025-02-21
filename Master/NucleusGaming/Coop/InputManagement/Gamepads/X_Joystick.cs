using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    public class X_Joystick
    {
        public OpenXinputController XinputController;
        public State State => GetState();
        public Guid InstanceGuid;
        public Guid ProductGuid;
        public string HIDPath;
        public string InstanceName;
        public int UserIndex;

        private State GetState()
        {
            if(!XinputController.IsConnected)
            {
                return new State();
            }

            State state = XinputController.GetState();

            return state;
        }
    }
}
