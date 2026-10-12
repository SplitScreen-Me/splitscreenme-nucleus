using System.Diagnostics;
using System.Drawing;
using WindowScrape.Types;

namespace Nucleus.Gaming
{
    public class ProcessData
    {
        private Process process;
        public bool Finished;

        public Point Position;
        private HwndObject hWnd;
        public bool HWNDRetry;
        
        public Size Size;
        public bool Setted;

        public bool Register0;

        public bool KilledMutexes;
        public long RegLong;
        public int Status;
        public float? AudioVolume;

        // Fast launch: re-checks of the window after its (faster) setup, in case the game moved or recreated it.
        public System.IntPtr FastFinalHwnd;
        public Rectangle FastFinalRect;
        public System.DateTime FastRecheckAt;
        public int FastRechecksLeft;
        public int FastReruns;
        /// <summary>
        /// A reference to the game's process, if it's running
        /// </summary>
        public Process Process => process;

        public HwndObject HWnd
        {
            get => hWnd;
            set => hWnd = value;
        }

        public ProcessData(Process proc)
        {
            process = proc;
        }

        public void AssignProcess(Process proc)
        {
            process = proc;
        }
    }
}
