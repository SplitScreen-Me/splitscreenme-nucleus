using Nucleus.Coop.Controls;
using Nucleus.Coop.Forms;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class Generic_Functions
    {
        public static void EnablePlay() => UI_Interface.PlayButton.Visible = true;

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

            if(!(sender is SortCheckBox) && !(sender is SortOptionsPanel))
            {
                UI_Interface.SortOptionsPanel.Visible = false;
            }

            if (sender != UI_Interface.TutorialButton && UI_Interface.Tutorial != null)
            {
                DisposeTutorial();
            }
        }

        public static void SetCoverLocation(bool profileEnabled)
        {
            UI_Interface.Cover.Visible = false;

            if (profileEnabled)
            {
                UI_Interface.Cover.Location = UI_Interface.DefCoverLoc;
                UI_Interface.Cover.Visible = true;
                return;
            }

            UI_Interface.Cover.BringToFront();
            UI_Interface.Cover.Location = new Point(UI_Interface.Cover.Location.X, UI_Interface.ProfileButtonsPanel.Bottom - 5);
            UI_Interface.Cover.Visible = true;
        }

        public static void DisposeTutorial()
        {
            UI_Interface.MainForm.Controls.Remove(UI_Interface.Tutorial);
            UI_Interface.Tutorial.Dispose();
            UI_Interface.Tutorial = null;
        }

        public static void RefreshAll()
        {
            UI_Interface.GameListContainer.Refresh();
            UI_Interface.SetupPanel.Refresh();
            UI_Interface.WindowPanel.Refresh();
            UI_Interface.InfoPanel.Refresh();
            
            UI_Interface.MainForm.Invalidate(false);//Update top/bottom border colors
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

            UI_Interface.WebView = new HubWebView();

            UI_Interface.WebView.Disposed += WebviewDisposed;
            UI_Interface.HomeScreen.Controls.Add(UI_Interface.WebView);

            UI_Interface.WebView.Size = UI_Interface.HomeScreen.Size;
            UI_Interface.WebView.Location = UI_Interface.GameListContainer.Location;

            UI_Interface.HubButton.Selected = true;

            UI_Functions.RefreshUI(true);
            UI_Interface.MainForm.Invalidate(true);
            UI_Interface.WebView.BringToFront();
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
