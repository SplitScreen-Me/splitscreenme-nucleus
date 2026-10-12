using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Nucleus.Coop.Services;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Host
{
    /// <summary>
    /// Main window of the web UI: a frameless top-level window that keeps the native frame behaviour
    /// (Aero Snap, Win+arrows, minimize/maximize animations, resize) and hosts a WebView2.
    /// </summary>
    /// <remarks>
    /// The window keeps WS_THICKFRAME and the caption styles; WM_NCCALCSIZE removes the non-client area
    /// so the client area covers the whole window. The WebView2 fills the client area minus a black bezel
    /// that the form hit-tests for resizing. Dragging is done by the page with CSS <c>app-region: drag</c>.
    /// </remarks>
    internal sealed class HostForm : Form
    {
        /// <summary>Width of the black resize bezel around the WebView, in device-independent pixels.</summary>
        public const int BezelDip = 6;

        private const string DefaultTitle = "Nucleus Co-op";
        private const int MaxCrashesPerMinute = 3;

        private static readonly Size MinimumSizeDip = new Size(1100, 680);
        private static readonly Size DefaultSizeDip = new Size(1360, 840);

        private readonly HostEnvironment environment;

        private WebView2 webView;
        private int dpi = 96;
        private Rectangle normalBounds;
        private bool restoreToMaximized;
        private bool isActive;
        private bool isClosing;
        private bool unresponsivePromptOpen;
        private bool inWindowPosChanged;
        private int crashCount;
        private DateTime firstCrashUtc;
        private HostWindowSnapshot lastSnapshot;
        private int pageState = (int)UiPageState.Loading;

        public HostForm(HostEnvironment environment)
        {
            this.environment = environment;

            Text = DefaultTitle;
            Icon = LoadAppIcon();
            BackColor = Color.Black;
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.Sizable;
            ControlBox = true;
            MinimizeBox = true;
            MaximizeBox = true;
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.Manual;

            ApplyInitialPlacement();
            CreateWebViewControl();
        }

        /// <summary>Whether the web UI can show messages (see <see cref="IHostShell.PageState"/>). Any thread.</summary>
        public UiPageState PageState => (UiPageState)Volatile.Read(ref pageState);

        private void SetPageState(UiPageState state) => Volatile.Write(ref pageState, (int)state);

        /// <summary>Raised on the UI thread each time a CoreWebView2 is ready, before the first navigation (again after a browser process crash).</summary>
        public event EventHandler<CoreWebView2> CoreWebView2Ready;

        /// <summary>Raised on the UI thread before the current CoreWebView2 is disposed (browser process crash, window closing).</summary>
        public event EventHandler<CoreWebView2> CoreWebView2Closing;

        /// <summary>Raised on the UI thread when <see cref="GetSnapshot"/> changes (maximize, restore, minimize, focus, DPI).</summary>
        public event EventHandler WindowStateChanged;

        /// <summary>The current CoreWebView2, or null while it is being created.</summary>
        public CoreWebView2 CoreWebView2 => webView?.CoreWebView2;

        /// <summary>Version of the WebView2 runtime in use, or of the installed one before initialization.</summary>
        public string WebViewVersion
        {
            get
            {
                try
                {
                    CoreWebView2 core = CoreWebView2;
                    return core != null ? core.Environment.BrowserVersionString : CoreWebView2Environment.GetAvailableBrowserVersionString();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        /// <summary>Current window state.</summary>
        public HostWindowSnapshot GetSnapshot()
        {
            bool created = IsHandleCreated;
            bool minimized = created && HostNative.IsIconic(Handle);

            // Active (WM_ACTIVATE) is not enough: a window activated from a background process is active in its thread
            // without being the foreground window, and a minimized window can stay active.
            bool focused = created && isActive && !minimized && HostNative.GetForegroundWindow() == Handle;

            return new HostWindowSnapshot(
                created && HostNative.IsZoomed(Handle),
                minimized,
                focused,
                dpi / 96.0);
        }

        public void MinimizeWindow()
        {
            HostNative.ShowWindow(Handle, HostNative.SW_MINIMIZE);
        }

        public void ToggleMaximizeWindow()
        {
            HostNative.ShowWindow(Handle, HostNative.IsZoomed(Handle) ? HostNative.SW_RESTORE : HostNative.SW_MAXIMIZE);
        }

        /// <summary>
        /// Brings the window to the front with the keyboard focus in the page (end of a game session). Windows refuses
        /// SetForegroundWindow to a background process, which Nucleus is when the last input went to a game or a gamepad
        /// shortcut ended the session: the window then only flashed in the taskbar, the page had no focus, and gamepad
        /// navigation (it needs document.hasFocus()) ignored every pad until a mouse click. Attached to the input of the
        /// foreground window's thread, the call is allowed.
        /// </summary>
        public void BringToForeground()
        {
            IntPtr hwnd = Handle;

            if (HostNative.GetForegroundWindow() != hwnd && !HostNative.SetForegroundWindow(hwnd))
            {
                IntPtr foreground = HostNative.GetForegroundWindow();
                uint foregroundThread = foreground == IntPtr.Zero ? 0 : HostNative.GetWindowThreadProcessId(foreground, IntPtr.Zero);
                uint thread = HostNative.GetCurrentThreadId();
                bool attached = foregroundThread != 0 && foregroundThread != thread && HostNative.AttachThreadInput(thread, foregroundThread, true);

                try
                {
                    HostNative.BringWindowToTop(hwnd);

                    if (!HostNative.SetForegroundWindow(hwnd))
                    {
                        HostLog.Info("Could not bring the window to the front after the session.");
                    }
                }
                finally
                {
                    if (attached)
                    {
                        HostNative.AttachThreadInput(thread, foregroundThread, false);
                    }
                }
            }

            FocusPage();
            EnsurePageFocusAsync();
        }

        /// <summary>
        /// Checks what gamepad navigation checks, <c>document.hasFocus()</c>, a few times after a restore, and moves the
        /// focus into the page again while it is false: a WebView restored from minimized can ignore the first focus move.
        /// </summary>
        private async void EnsurePageFocusAsync()
        {
            try
            {
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    await Task.Delay(200);

                    CoreWebView2 core = webView?.CoreWebView2;

                    if (isClosing || IsDisposed || core == null || !isActive || PageState != UiPageState.Ready)
                    {
                        return;
                    }

                    if (await core.ExecuteScriptAsync("document.hasFocus()") == "true")
                    {
                        return;
                    }

                    WebView2 view = webView;

                    if (view != null && !view.IsDisposed)
                    {
                        HostNative.SetFocus(Handle); // the form hands the focus back to the WebView, which moves it into the page
                        view.Focus();
                    }
                }

                HostLog.Info("The page still has no keyboard focus after the session: gamepad navigation needs a click.");
            }
            catch (Exception ex)
            {
                HostLog.Warning($"Could not check the page focus: {ex.Message}");
            }
        }

        /// <summary>
        /// Gives the keyboard focus to the page when the active window doesn't have it inside the browser's window
        /// (activation by Alt+Tab, the taskbar or <see cref="BringToForeground"/>; WinForms doesn't always hand it back).
        /// </summary>
        private void FocusPage()
        {
            WebView2 view = webView;

            if (isClosing || IsDisposed || view == null || view.IsDisposed || !view.IsHandleCreated || view.CoreWebView2 == null || !isActive)
            {
                return;
            }

            try
            {
                IntPtr focus = HostNative.GetFocus();

                // The browser's window is a child of the WebView2 control: the page has the focus.
                if (focus != IntPtr.Zero && focus != view.Handle && HostNative.IsChild(view.Handle, focus))
                {
                    return;
                }

                // WebView2 moves the focus into the page when its control gets the focus (OnGotFocus → MoveFocus): when the
                // control already has it, go through the form, which hands it back to its active control, the WebView.
                if (focus == view.Handle)
                {
                    HostNative.SetFocus(Handle);
                }

                view.Focus();
            }
            catch (Exception ex)
            {
                HostLog.Warning($"Could not focus the page: {ex.Message}");
            }
        }

        /// <summary>Closes the window after the current message is handled (safe to call from a WebView2 event).</summary>
        public void CloseWindow()
        {
            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(new Action(Close));
            }
        }

        /// <summary>The native window title. Always "Nucleus Co-op": other code finds the window with FindWindow(null, "Nucleus Co-op").</summary>
        public string WindowTitle => DefaultTitle;

        #region Window frame

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            Globals.MainWindowHandle = Handle;
            dpi = HostNative.GetWindowDpi(Handle);
            UpdateBezel();
            ApplyDwmAttributes();

            // Ask for a new WM_NCCALCSIZE so the frame is removed with the current styles.
            HostNative.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                HostNative.SWP_FRAMECHANGED | HostNative.SWP_NOMOVE | HostNative.SWP_NOSIZE | HostNative.SWP_NOZORDER | HostNative.SWP_NOACTIVATE);
        }

        // Some of these messages arrive while CreateWindowEx runs: use m.HWnd, never the Handle property, in this path.
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case HostNative.WM_NCCALCSIZE:
                    if (m.WParam != IntPtr.Zero)
                    {
                        OnNcCalcSize(ref m);
                        return;
                    }
                    break;

                case HostNative.WM_NCHITTEST:
                    m.Result = new IntPtr(HitTest(m.HWnd, m.LParam));
                    return;

                case HostNative.WM_GETMINMAXINFO:
                    base.WndProc(ref m);
                    ApplyMinimumTrackSize(m.HWnd, m.LParam);
                    return;

                case HostNative.WM_DPICHANGED:
                    OnDpiChangedMessage(ref m);
                    return;

                case HostNative.WM_ACTIVATE:
                case HostNative.WM_ACTIVATEAPP:
                    base.WndProc(ref m);
                    if (m.Msg == HostNative.WM_ACTIVATE)
                    {
                        int activation = HostNative.LoWordSigned(m.WParam) & 0xFFFF;
                        isActive = activation != 0; // WA_INACTIVE = 0

                        // Not on a click: the click itself focuses what it lands on in the page.
                        if (activation == HostNative.WA_ACTIVE && IsHandleCreated)
                        {
                            BeginInvoke(new Action(FocusPage));
                        }
                    }

                    // Re-checked once the activation change is complete (foreground window updated).
                    if (IsHandleCreated)
                    {
                        BeginInvoke(new Action(RaiseWindowStateChangedIfNeeded));
                    }
                    return;

                case HostNative.WM_WINDOWPOSCHANGED:
                    inWindowPosChanged = true;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        inWindowPosChanged = false;
                    }
                    return;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// After a restore from maximized, WinForms re-applies the bounds it stashed while handling WM_WINDOWPOSCHANGED,
        /// converting a client size to a window size with AdjustWindowRectEx: the frame we removed would be added to
        /// the window at each restore. Windows has already restored the right rectangle, so that fix-up is skipped.
        /// </summary>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (inWindowPosChanged)
            {
                return;
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        /// <summary>The client area covers the whole window. When maximized, it is clamped to the monitor work area.</summary>
        private void OnNcCalcSize(ref Message m)
        {
            if (HostNative.IsZoomed(m.HWnd))
            {
                HostNative.NCCALCSIZE_PARAMS p = (HostNative.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(HostNative.NCCALCSIZE_PARAMS));
                IntPtr monitor = HostNative.MonitorFromRect(ref p.rgrc0, HostNative.MONITOR_DEFAULTTONEAREST);
                HostNative.MONITORINFO info = new HostNative.MONITORINFO { cbSize = Marshal.SizeOf(typeof(HostNative.MONITORINFO)) };

                if (monitor != IntPtr.Zero && HostNative.GetMonitorInfo(monitor, ref info))
                {
                    HostNative.RECT work = info.rcWork;

                    // A maximized window that covers the whole monitor hides an auto-hide taskbar for good: leave it one pixel.
                    if (HostNative.IsTaskbarAutoHide())
                    {
                        if (HostNative.HasAutoHideTaskbar(HostNative.ABE_BOTTOM, info.rcMonitor)) work.Bottom -= 1;
                        else if (HostNative.HasAutoHideTaskbar(HostNative.ABE_TOP, info.rcMonitor)) work.Top += 1;
                        else if (HostNative.HasAutoHideTaskbar(HostNative.ABE_LEFT, info.rcMonitor)) work.Left += 1;
                        else if (HostNative.HasAutoHideTaskbar(HostNative.ABE_RIGHT, info.rcMonitor)) work.Right -= 1;
                    }

                    p.rgrc0 = work;
                    Marshal.StructureToPtr(p, m.LParam, false);
                }
            }

            m.Result = IntPtr.Zero;
        }

        /// <summary>Resize borders in the bezel. The WebView covers the rest of the client area.</summary>
        private int HitTest(IntPtr hwnd, IntPtr lParam)
        {
            Point p = PointToClient(new Point(HostNative.LoWordSigned(lParam), HostNative.HiWordSigned(lParam)));
            Size size = ClientSize;
            int bezel = Math.Max(1, ScaleDip(BezelDip));

            if (HostNative.IsZoomed(hwnd))
            {
                // Top edge of a maximized window: drag down to restore, double-click to restore.
                return p.Y < bezel ? HostNative.HTCAPTION : HostNative.HTCLIENT;
            }

            bool top = p.Y < bezel;
            bool bottom = p.Y >= size.Height - bezel;
            bool left = p.X < bezel;
            bool right = p.X >= size.Width - bezel;

            if (!(top || bottom || left || right))
            {
                return HostNative.HTCLIENT;
            }

            // Corners grab a longer stretch of the bezel so they are easy to hit.
            int corner = bezel * 3;
            bool nearLeft = p.X < corner;
            bool nearRight = p.X >= size.Width - corner;
            bool nearTop = p.Y < corner;
            bool nearBottom = p.Y >= size.Height - corner;

            if ((top && nearLeft) || (left && nearTop)) return HostNative.HTTOPLEFT;
            if ((top && nearRight) || (right && nearTop)) return HostNative.HTTOPRIGHT;
            if ((bottom && nearLeft) || (left && nearBottom)) return HostNative.HTBOTTOMLEFT;
            if ((bottom && nearRight) || (right && nearBottom)) return HostNative.HTBOTTOMRIGHT;
            if (top) return HostNative.HTTOP;
            if (bottom) return HostNative.HTBOTTOM;
            return left ? HostNative.HTLEFT : HostNative.HTRIGHT;
        }

        private void ApplyMinimumTrackSize(IntPtr hwnd, IntPtr lParam)
        {
            HostNative.MINMAXINFO info = (HostNative.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(HostNative.MINMAXINFO));
            Size min = GetMinimumSizePx(hwnd);
            info.ptMinTrackSize.X = min.Width;
            info.ptMinTrackSize.Y = min.Height;
            Marshal.StructureToPtr(info, lParam, false);
        }

        private void OnDpiChangedMessage(ref Message m)
        {
            dpi = Math.Max(96, HostNative.LoWordSigned(m.WParam));
            HostNative.RECT suggested = (HostNative.RECT)Marshal.PtrToStructure(m.LParam, typeof(HostNative.RECT));

            UpdateBezel();
            HostNative.SetWindowPos(m.HWnd, IntPtr.Zero, suggested.Left, suggested.Top,
                suggested.Right - suggested.Left, suggested.Bottom - suggested.Top,
                HostNative.SWP_NOZORDER | HostNative.SWP_NOACTIVATE);

            m.Result = IntPtr.Zero;
            RaiseWindowStateChangedIfNeeded();
        }

        private void ApplyDwmAttributes()
        {
            try
            {
                // A one-pixel frame extension keeps the DWM shadow on a window without non-client area.
                HostNative.MARGINS margins = new HostNative.MARGINS { cyBottomHeight = 1 };
                HostNative.DwmExtendFrameIntoClientArea(Handle, ref margins);

                int enabled = 1;
                HostNative.DwmSetWindowAttribute(Handle, HostNative.DWMWA_USE_IMMERSIVE_DARK_MODE, ref enabled, sizeof(int));

                int round = HostNative.DWMWCP_ROUND;
                HostNative.DwmSetWindowAttribute(Handle, HostNative.DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int));
            }
            catch (DllNotFoundException)
            {
            }
        }

        private void UpdateBezel()
        {
            int bezel = ScaleDip(BezelDip);
            if (Padding.All != bezel)
            {
                Padding = new Padding(bezel);
            }
        }

        private int ScaleDip(int value) => (int)Math.Round(value * dpi / 96.0);

        private Size GetMinimumSizePx(IntPtr hwnd)
        {
            Rectangle work = Screen.FromHandle(hwnd).WorkingArea;
            return new Size(
                Math.Min(ScaleDip(MinimumSizeDip.Width), work.Width),
                Math.Min(ScaleDip(MinimumSizeDip.Height), work.Height));
        }

        #endregion

        #region Placement and state

        private void ApplyInitialPlacement()
        {
            Rectangle saved;
            bool maximized;

            if (TryParsePlacement(App_Misc.HostWindowPlacement, out saved, out maximized) && IsReachable(saved))
            {
                Bounds = saved;
                normalBounds = saved;
                if (maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }

                return;
            }

            Rectangle work = Screen.PrimaryScreen.WorkingArea;
            double scale = HostNative.GetDpiForPoint(work.X + work.Width / 2, work.Y + work.Height / 2) / 96.0;
            int width = Math.Min((int)(DefaultSizeDip.Width * scale), work.Width);
            int height = Math.Min((int)(DefaultSizeDip.Height * scale), work.Height);

            Bounds = new Rectangle(work.X + (work.Width - width) / 2, work.Y + (work.Height - height) / 2, width, height);
            normalBounds = Bounds;
        }

        private static bool TryParsePlacement(string value, out Rectangle bounds, out bool maximized)
        {
            bounds = Rectangle.Empty;
            maximized = false;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] parts = value.Split(',');
            int[] numbers = new int[5];

            if (parts.Length != 5)
            {
                return false;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out numbers[i]))
                {
                    return false;
                }
            }

            if (numbers[2] < 200 || numbers[3] < 150)
            {
                return false;
            }

            bounds = new Rectangle(numbers[0], numbers[1], numbers[2], numbers[3]);
            maximized = numbers[4] != 0;
            return true;
        }

        /// <summary>Whether enough of the rectangle is on a connected screen to grab it.</summary>
        private static bool IsReachable(Rectangle bounds)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                Rectangle visible = Rectangle.Intersect(screen.WorkingArea, bounds);
                if (visible.Width >= 160 && visible.Height >= 80)
                {
                    return true;
                }
            }

            return false;
        }

        private void SavePlacement()
        {
            try
            {
                Rectangle bounds = normalBounds.Width > 0 ? normalBounds : Bounds;
                bool maximized = HostNative.IsZoomed(Handle) || (HostNative.IsIconic(Handle) && restoreToMaximized);

                App_Misc.HostWindowPlacement = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4}",
                    bounds.X, bounds.Y, bounds.Width, bounds.Height, maximized ? 1 : 0);
            }
            catch (Exception ex)
            {
                HostLog.Warning($"Could not save the window placement: {ex.Message}");
            }
        }

        private void TrackNormalBounds()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            bool zoomed = HostNative.IsZoomed(Handle);
            bool iconic = HostNative.IsIconic(Handle);

            if (!iconic)
            {
                restoreToMaximized = zoomed;
            }

            if (!zoomed && !iconic)
            {
                normalBounds = Bounds;
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            TrackNormalBounds();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            TrackNormalBounds();
            RaiseWindowStateChangedIfNeeded();
        }

        private void RaiseWindowStateChangedIfNeeded()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            HostWindowSnapshot snapshot = GetSnapshot();
            if (snapshot.Equals(lastSnapshot))
            {
                return;
            }

            lastSnapshot = snapshot;
            WindowStateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Saved first: a FormClosing handler may cancel the close to end a session and quit later with the window hidden.
            if (Visible)
            {
                SavePlacement();
            }

            base.OnFormClosing(e);
            if (!e.Cancel)
            {
                isClosing = true;
                SetPageState(UiPageState.Unavailable);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            DisposeWebView();
            base.OnFormClosed(e);
        }

        #endregion

        #region WebView2

        private void CreateWebViewControl()
        {
            webView = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                DefaultBackgroundColor = Color.Black, // no white flash while the page loads
            };

            Controls.Add(webView);
        }

        private void DisposeWebView()
        {
            if (webView == null)
            {
                return;
            }

            WebView2 old = webView;
            webView = null;

            CoreWebView2 core = old.CoreWebView2;
            if (core != null)
            {
                try
                {
                    CoreWebView2Closing?.Invoke(this, core);
                }
                catch (Exception ex)
                {
                    HostLog.Error("CoreWebView2Closing handler failed.", ex);
                }
            }

            Controls.Remove(old);
            old.Dispose();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await StartWebViewAsync();
        }

        private async Task StartWebViewAsync()
        {
            WebView2 target = webView;

            try
            {
                CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions();
                if (environment.RemoteDebuggingPort.HasValue)
                {
                    options.AdditionalBrowserArguments = "--remote-debugging-port=" + environment.RemoteDebuggingPort.Value.ToString(CultureInfo.InvariantCulture);
                }

                CoreWebView2Environment webViewEnvironment = await CoreWebView2Environment.CreateAsync(null, environment.UserDataFolder, options);
                await target.EnsureCoreWebView2Async(webViewEnvironment);
            }
            catch (WebView2RuntimeNotFoundException ex)
            {
                HostLog.Error("The WebView2 runtime is not installed.", ex);
                SetPageState(UiPageState.Unavailable);
                ShowRuntimeMissing();
                Close();
                return;
            }
            catch (Exception ex)
            {
                if (isClosing || IsDisposed || target != webView)
                {
                    return;
                }

                HostLog.Error("WebView2 initialization failed.", ex);
                SetPageState(UiPageState.Unavailable);
                DialogResult answer = MessageBox.Show(this,
                    "The Nucleus Co-op interface could not be started.\n\n" + ex.Message +
                    "\n\nClose any other Nucleus Co-op window, then click Retry.",
                    DefaultTitle, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                if (answer == DialogResult.Retry)
                {
                    RecreateWebView();
                }
                else
                {
                    Close();
                }

                return;
            }

            if (isClosing || IsDisposed || target != webView)
            {
                return;
            }

            CoreWebView2 core = target.CoreWebView2;
            ConfigureCoreWebView2(core);

            HostLog.Info($"WebView2 {core.Environment.BrowserVersionString} ready, loading {environment.StartUri}.");
            CoreWebView2Ready?.Invoke(this, core);

            core.Navigate(environment.StartUri.AbsoluteUri);
            target.Focus();
        }

        private void ConfigureCoreWebView2(CoreWebView2 core)
        {
            bool debug = environment.IsDebug;
            CoreWebView2Settings settings = core.Settings;

            settings.AreDevToolsEnabled = debug;
            settings.AreDefaultContextMenusEnabled = debug;
            settings.AreBrowserAcceleratorKeysEnabled = debug;
            settings.AreHostObjectsAllowed = false;
            settings.IsWebMessageEnabled = true;
            settings.IsScriptEnabled = true;
            settings.IsStatusBarEnabled = false;
            settings.IsZoomControlEnabled = false;
            settings.IsPinchZoomEnabled = false;
            settings.IsSwipeNavigationEnabled = false;
            settings.IsGeneralAutofillEnabled = false;
            settings.IsPasswordAutosaveEnabled = false;

            try
            {
                // CSS app-region: drag moves the window, double-click maximizes, right-click shows the system menu.
                settings.IsNonClientRegionSupportEnabled = true;
            }
            catch (Exception ex)
            {
                HostLog.Warning($"The WebView2 runtime does not support app-region (update it): {ex.Message}");
            }

            // UI: only its own pages may use it. Assets: images must be readable from canvas (cover colours), so they
            // answer CORS requests too. Handlers: images in custom steps only, no CORS reads of the handler scripts.
            MapFolder(core, HostEnvironment.UiHost, environment.WebUiFolder, CoreWebView2HostResourceAccessKind.Deny);
            MapFolder(core, HostEnvironment.AssetsHost, environment.AssetsFolder, CoreWebView2HostResourceAccessKind.Allow);
            MapFolder(core, HostEnvironment.HandlersHost, environment.HandlersFolder, CoreWebView2HostResourceAccessKind.DenyCors);

            core.NavigationStarting += OnNavigationStarting;
            core.NewWindowRequested += OnNewWindowRequested;
            core.LaunchingExternalUriScheme += OnLaunchingExternalUriScheme;
            core.PermissionRequested += OnPermissionRequested;
            core.ProcessFailed += OnProcessFailed;
            core.NavigationCompleted += OnNavigationCompleted;
        }

        private static void MapFolder(CoreWebView2 core, string host, string folder, CoreWebView2HostResourceAccessKind access)
        {
            try
            {
                core.SetVirtualHostNameToFolderMapping(host, folder, access);
            }
            catch (Exception ex)
            {
                HostLog.Error($"Could not map https://{host}/ to {folder}.", ex);
            }
        }

        /// <summary>The WebView only shows the UI. Other pages open in the default browser.</summary>
        private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (environment.IsAppUri(e.Uri))
            {
                SetPageState(UiPageState.Loading);
                return;
            }

            e.Cancel = true;
            OpenOutside(e.Uri, e.IsUserInitiated, "navigation");
        }

        private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true; // never open a WebView2 popup window
            OpenOutside(e.Uri, e.IsUserInitiated, "new window");
        }

        private static void OpenOutside(string uri, bool userInitiated, string kind)
        {
            if (!userInitiated)
            {
                HostLog.Warning($"Blocked a {kind} to {uri} that the user did not start.");
                return;
            }

            if (ExternalLauncher.ParseWebUrl(uri) == null)
            {
                HostLog.Warning($"Blocked a {kind} to {uri}.");
                return;
            }

            HostLog.Info($"Opening {uri} in the default browser ({kind}).");
            ExternalLauncher.TryOpenUrl(uri);
        }

        private void OnLaunchingExternalUriScheme(object sender, CoreWebView2LaunchingExternalUriSchemeEventArgs e)
        {
            e.Cancel = true;
            HostLog.Warning($"Blocked external protocol {e.Uri}.");
        }

        private void OnPermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
        {
            bool allowed = e.PermissionKind == CoreWebView2PermissionKind.ClipboardRead && environment.IsAppUri(e.Uri);
            e.State = allowed ? CoreWebView2PermissionState.Allow : CoreWebView2PermissionState.Deny;

            if (!allowed)
            {
                HostLog.Info($"Denied permission {e.PermissionKind} to {e.Uri}.");
            }
        }

        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                if (environment.IsAppUri((sender as CoreWebView2)?.Source))
                {
                    SetPageState(UiPageState.Ready);
                }
            }
            else if (e.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
            {
                //A cancelled navigation (a link opened outside) leaves the UI page in place.
                SetPageState(UiPageState.Unavailable);
                HostLog.Warning($"UI navigation failed: {e.WebErrorStatus} (HTTP {e.HttpStatusCode}).");
            }
        }

        private void OnProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            HostLog.Error($"WebView2 process failure: kind={e.ProcessFailedKind}, reason={e.Reason}, exit code={e.ExitCode}, process={e.ProcessDescription}.");

            switch (e.ProcessFailedKind)
            {
                case CoreWebView2ProcessFailedKind.BrowserProcessExited:
                    SetPageState(UiPageState.Unavailable);
                    BeginInvoke(new Action(() => RecoverFromCrash(true)));
                    break;

                case CoreWebView2ProcessFailedKind.RenderProcessExited:
                    SetPageState(UiPageState.Unavailable);
                    BeginInvoke(new Action(() => RecoverFromCrash(false)));
                    break;

                case CoreWebView2ProcessFailedKind.RenderProcessUnresponsive:
                    BeginInvoke(new Action(OnRendererUnresponsive));
                    break;

                default:
                    // GPU, utility and iframe renderer failures: WebView2 recovers by itself.
                    break;
            }
        }

        private void RecoverFromCrash(bool browserProcessExited)
        {
            if (isClosing || IsDisposed)
            {
                return;
            }

            DateTime now = DateTime.UtcNow;
            if (crashCount == 0 || now - firstCrashUtc > TimeSpan.FromMinutes(1))
            {
                crashCount = 0;
                firstCrashUtc = now;
            }

            crashCount++;

            if (crashCount > MaxCrashesPerMinute)
            {
                MessageBox.Show(this,
                    "The Nucleus Co-op interface keeps stopping unexpectedly and will close.\n\n" +
                    "Details are in content\\app.log. If this keeps happening, repair or reinstall the Microsoft Edge WebView2 Runtime.",
                    DefaultTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            MessageBox.Show(this, "The Nucleus Co-op interface stopped unexpectedly. It will be reloaded.",
                DefaultTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (browserProcessExited || webView?.CoreWebView2 == null)
            {
                RecreateWebView();
            }
            else
            {
                ReloadUi();
            }
        }

        private void OnRendererUnresponsive()
        {
            if (isClosing || IsDisposed || unresponsivePromptOpen)
            {
                return;
            }

            unresponsivePromptOpen = true;
            try
            {
                DialogResult answer = MessageBox.Show(this,
                    "The Nucleus Co-op interface is not responding.\n\nReload it now? Click No to keep waiting.",
                    DefaultTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (answer == DialogResult.Yes && !isClosing && !IsDisposed)
                {
                    ReloadUi();
                }
            }
            finally
            {
                unresponsivePromptOpen = false;
            }
        }

        private void ReloadUi()
        {
            CoreWebView2 core = webView?.CoreWebView2;
            if (core == null)
            {
                return;
            }

            if (environment.IsAppUri(core.Source))
            {
                core.Reload();
            }
            else
            {
                core.Navigate(environment.StartUri.AbsoluteUri);
            }
        }

        /// <summary>Replaces the WebView2 control and its browser process (after a browser crash or a failed start).</summary>
        private void RecreateWebView()
        {
            DisposeWebView();
            CreateWebViewControl();
            BeginInvoke(new Action(async () =>
            {
                try
                {
                    await StartWebViewAsync();
                }
                catch (Exception ex)
                {
                    HostLog.Error("Restarting the WebView2 failed.", ex);
                }
            }));
        }

        private void ShowRuntimeMissing()
        {
            const string downloadPage = "https://developer.microsoft.com/microsoft-edge/webview2/";

            DialogResult answer = MessageBox.Show(this,
                "Nucleus Co-op needs the Microsoft Edge WebView2 Runtime to show its interface, and it is not installed.\n\n" +
                "Open the download page now?",
                DefaultTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (answer == DialogResult.Yes)
            {
                ExternalLauncher.TryOpenUrl(downloadPage);
            }
        }

        #endregion

        private static Icon LoadAppIcon()
        {
            try
            {
                return Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>Immutable state of the host window.</summary>
    internal sealed class HostWindowSnapshot : IEquatable<HostWindowSnapshot>
    {
        public HostWindowSnapshot(bool maximized, bool minimized, bool focused, double dpiScale)
        {
            Maximized = maximized;
            Minimized = minimized;
            Focused = focused;
            DpiScale = dpiScale;
        }

        public bool Maximized { get; }

        public bool Minimized { get; }

        public bool Focused { get; }

        public double DpiScale { get; }

        public bool Equals(HostWindowSnapshot other)
        {
            return other != null && other.Maximized == Maximized && other.Minimized == Minimized &&
                   other.Focused == Focused && Math.Abs(other.DpiScale - DpiScale) < 0.001;
        }

        public override bool Equals(object obj) => Equals(obj as HostWindowSnapshot);

        public override int GetHashCode() => (Maximized ? 1 : 0) | (Minimized ? 2 : 0) | (Focused ? 4 : 0) | ((int)(DpiScale * 100) << 3);
    }
}
