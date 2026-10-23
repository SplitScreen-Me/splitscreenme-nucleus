using Nucleus.Coop.Forms;
using System.Drawing;
using System.Threading;

namespace Nucleus.Gaming.Forms
{
    public static class ReminderFormThread
    {
        public static void StartReminderForms(Rectangle destBounds)
        {
            Thread backgroundFormThread = new Thread(delegate ()
            {
                ShortcutsReminder reminder = new ShortcutsReminder(destBounds);
                GenericGameHandler.Instance.shortcutsReminders.Add(reminder);
                System.Windows.Threading.Dispatcher.Run();
            });

            backgroundFormThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
            backgroundFormThread.IsBackground = true;
            backgroundFormThread.Start();
        }
    }
}
