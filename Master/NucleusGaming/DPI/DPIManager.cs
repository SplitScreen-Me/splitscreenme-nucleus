using Nucleus.Gaming.Windows;

namespace Nucleus.Gaming
{
    /// <summary>Desktop scaling factor, read once at startup (logged with each launch).</summary>
    public static class DPIManager
    {
        public static float Scale = 1.0f;

        /// <summary>Reads the scaling factor. Call it before the process DPI awareness is set.</summary>
        public static void PreInitialize()
        {
            Scale = User32Util.GetDPIScalingFactor();
        }
    }
}
