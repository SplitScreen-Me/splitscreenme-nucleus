using Nucleus.Coop.Controls;
using Nucleus.Coop.Forms;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class Generic_Functions
    {
        

        public static List<Control> ListAllFormControls(Form form)
        {
            List<Control> AllFormControls = new List<Control>();

            foreach (Control control in form.Controls)
            {
                AllFormControls.Add(control);

                foreach (Control container1 in control.Controls)
                {
                    AllFormControls.Add(container1);
                    foreach (Control container2 in container1.Controls)
                    {
                        AllFormControls.Add(container2);
                        foreach (Control container3 in container2.Controls)
                        {
                            AllFormControls.Add(container3);
                        }
                    }
                }
            }

            return AllFormControls;
        }

        public static Control FindControlAtPoint(Control container, Point pos)
        {
            Control child;
            foreach (Control c in container.Controls)
            {
                if (c.Visible && c.Bounds.Contains(pos))
                {
                    child = FindControlAtPoint(c, new Point(pos.X - c.Left, pos.Y - c.Top));
                    if (child == null)
                    {
                        return c;
                    }
                    else
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        public static Control FindControlAtCursor(MainForm form)
        {
            Point pos = Cursor.Position;
            if (form.Bounds.Contains(pos))
            {
                return FindControlAtPoint(form, form.PointToClient(pos));
            }

            return null;
        }

        public static void Btn_ZoomIn(object sender, EventArgs e)
        {
            Control con = sender as Control;
            con.Size = new Size(con.Width += 3, con.Height += 3);
            con.Location = new Point(con.Location.X - 1, con.Location.Y - 1);
        }

        public static void Btn_ZoomOut(object sender, EventArgs e)
        {
            Control con = sender as Control;
            con.Size = new Size(con.Width -= 3, con.Height -= 3);
            con.Location = new Point(con.Location.X + 1, con.Location.Y + 1);
        }

        public static void ClickAnyControl(object sender, EventArgs e)
        {
            if (UI_Interface.Settings.Visible)
            {
                UI_Interface.Settings.BringToFront();
            }

            if (UI_Interface.ProfileSettings.Visible)
            {
                UI_Interface.ProfileSettings.BringToFront();
            }

            if (GamePathForm.Instance != null)
            {
                GamePathForm.Instance.BringToFront();
            }

            //auto search games
            if (SearchDisksForm.Instance != null)
            {
                SearchDisksForm.Instance.BringToFront();
            }

            if (!(sender is SortCheckBox) && !(sender is SortOptionsPanel))
            {
                UI_Interface.SortOptionsPanel.Visible = false;
            }

            if (UI_Interface.ProfilesList.Visible)
            {
                UI_Functions.OnClickHideProfileList(null, null);
            }

            if (UI_Interface.Tutorial != null)
            {
                DisposeTutorial();
            }
        }

        public static void DisposeTutorial()
        {
            UI_Interface.MainForm.Controls.Remove(UI_Interface.Tutorial);
            UI_Interface.Tutorial.Dispose();
            UI_Interface.Tutorial = null;
        }

        public static void RefreshAll()
        {
            UIThread.Post(() =>
            {
                UI_Interface.WindowPanel.Invalidate();
                UI_Interface.GameListContainer.Invalidate();
                UI_Interface.SetupPanel.Invalidate();
                UI_Interface.InfoPanel.Invalidate();

                UI_Interface.MainForm.Refresh();
            });
        }

        public static void SizeAndScaleTuto()
        {
            RectangleF client = UI_Interface.HomeScreen.ClientRectangle;

            float ratio = 1.78f;

            RectangleF bounds = new RectangleF(0, 0, client.Width - 200, (client.Width - 200) / ratio);

            if (client.Width / client.Height < ratio)
            {
                UI_Interface.Tutorial.Height = (int)(bounds.Height);
                UI_Interface.Tutorial.Width = (int)((bounds.Height * ratio));
            }
            else
            {
                UI_Interface.Tutorial.Height = (int)(bounds.Width / ratio);
                UI_Interface.Tutorial.Width = (int)((bounds.Width));

                if (UI_Interface.Tutorial.ClientRectangle.Height > client.Height)
                {
                    UI_Interface.Tutorial.Height -= (UI_Interface.Tutorial.ClientRectangle.Height - (int)client.Height);
                    UI_Interface.Tutorial.Height -= (int)(200 / ratio);
                    UI_Interface.Tutorial.Width = (int)((UI_Interface.Tutorial.Height * ratio));
                }
            }

            UI_Interface.Tutorial.Location = new Point(UI_Interface.HomeScreen.Width / 2 - UI_Interface.Tutorial.Width / 2,
                                                       UI_Interface.HomeScreen.Height / 2 - UI_Interface.Tutorial.Height / 2);
        }

        public static void InsertWebview(object sender, EventArgs e)
        {
            if (e is MouseEventArgs click)
            {
                if (click.Button == MouseButtons.Right)
                {
                    return;
                }
            }

            if (GenericGameHandler.Instance != null)
            {
                if (!GenericGameHandler.Instance.HasEnded)
                {
                    return;
                }
            }

            if (UI_Interface.WebView != null)
            {
                return;
            }

            Core_Interface.Current_UserGameInfo = null;
            UI_Interface.WebView = new HubWebView();

            UI_Interface.WebView.Disposed += WebviewDisposed;
            UI_Interface.HomeScreen.Controls.Add(UI_Interface.WebView);

            UI_Interface.WebView.Size = new Size(UI_Interface.HomeScreen.Size.Width, UI_Interface.GameListContainer.Height);
            UI_Interface.WebView.Location = UI_Interface.GameListContainer.Location;

            UI_Interface.HubButton.Selected = true;

            UI_Functions.RefreshUI(true);
            UI_Interface.MainForm.Invalidate(true);
            UI_Interface.WebView.BringToFront();
            Core_Interface.Current_UserGameInfo = null;
        }

        public static void WebviewDisposed(object sender, EventArgs e)
        {
            if (UI_Interface.WebView == null)
            {
                return;
            }

            if (UI_Interface.WebView.Downloading)
            {
                return;
            }

            if (e is MouseEventArgs click)
            {
                if (click.Button == MouseButtons.Right)
                {
                    return;
                }
            }

            UI_Interface.HomeScreen.Controls.Remove(UI_Interface.WebView);

            UI_Interface.WebView.Dispose();
            UI_Interface.WebView = null;

            UI_Interface.HubButton.Selected = false;

            UI_Interface.MainForm.Invalidate(false);

            if (sender is HubWebView)
            {
                UI_Interface.BigLogo.Visible = true;
                UI_Interface.BigLogo.Refresh();
            }

            UI_Interface.BigLogo.Focus();
        }

        public static void SaveNucleusWindowPosAndLoc()
        {
            if (UI_Interface.MainForm.Location.X == -32000 || UI_Interface.MainForm.Width == 0)
            {
                return;
            }

            App_Misc.WindowSize = UI_Interface.MainForm.Width + "X" + UI_Interface.MainForm.Height;
            App_Misc.WindowLocation = UI_Interface.MainForm.Location.X + "X" + UI_Interface.MainForm.Location.Y;
        }
    }
}
