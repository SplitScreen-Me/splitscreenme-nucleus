using System;
using System.Drawing;

namespace Nucleus.Coop
{
    internal class ForeColor
    {
        private string v;

        public ForeColor(string v)
        {
            this.v = v;
        }

        public static implicit operator Color(ForeColor v)
        {
            throw new NotImplementedException();
        }
    }
}