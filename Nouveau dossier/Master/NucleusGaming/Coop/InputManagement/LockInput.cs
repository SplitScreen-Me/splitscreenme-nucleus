using Nucleus.Gaming.Coop.ProtoInput;
using System.Diagnostics;

namespace Nucleus.Gaming.Coop.InputManagement
{
    public static class LockInput
    {
        private static bool isLocking = false;

        public static bool IsLocked { get; private set; }

        public static void Lock(bool suspendExplorer, bool freezeExternalInputWhenInputNotLocked, ProtoInputOptions protoOptions)
        {
            if (isLocking)
            {
                return;
            }
            else
            {
                isLocking = true;
            }


            //InputInterceptor.InterceptEnabled = true;


            //System.Windows.Forms.Cursor.Hide();

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(0, 0);
            System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(0, 0, 1, 1);

            //WinApi.SetForegroundWindow(WinApi.GetDesktopWindow());

            ProtoInput.ProtoInput.protoInput.LockInput(true);

            if (suspendExplorer)
            {
                ProtoInput.ProtoInput.protoInput.SuspendExplorer();
            }

            Debug.WriteLine("Locked input, suspended explorer = " + suspendExplorer);

            isLocking = false;
            IsLocked = true;

            if (freezeExternalInputWhenInputNotLocked)
            {
                ProtoInputLauncher.NotifyInputLockChange();
            }

            protoOptions?.OnInputLocked?.Invoke();
        }

        public static void Unlock(bool freezeExternalInputWhenInputNotLocked, ProtoInputOptions protoOptions)
        {
            if (isLocking)
            {
                return;
            }
            else
            {
                isLocking = true;
            }

            //System.Windows.Forms.Cursor.Show();
            System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle();

            //InputInterceptor.InterceptEnabled = false;

            ProtoInput.ProtoInput.protoInput.LockInput(false);

            ProtoInput.ProtoInput.protoInput.RestartExplorer();

            Debug.WriteLine("Unlocked input");

            isLocking = false;
            IsLocked = false;

            if (freezeExternalInputWhenInputNotLocked)
            {
                ProtoInputLauncher.NotifyInputLockChange();
            }

            protoOptions?.OnInputUnlocked?.Invoke();
        }
    }
}
