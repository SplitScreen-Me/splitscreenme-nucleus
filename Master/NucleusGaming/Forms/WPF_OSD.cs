using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Forms.Overlay;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

internal class WPF_OSD_Child : WPF_OSD
{
    public WPF_OSD_Child(System.Drawing.Rectangle destBoundsRect) : base(destBoundsRect)
    {
    }
}

/// <summary>
/// On-screen display: short toasts (inputs locked, session ended, hotkeys…) centred on a screen, in the app's
/// "Bezel" look. During a session the main one shows its messages on every screen in use (one child per screen).
/// Never activated and click-through: the games keep the focus and the inputs.
/// </summary>
public class WPF_OSD : Window
{
    private const double ShadowRoom = 34;

    private readonly Grid root;
    private readonly Border pill;
    private readonly TextBlock label;
    private readonly TranslateTransform rise = new TranslateTransform();

    private readonly List<WPF_OSD_Child> childs = new List<WPF_OSD_Child>();

    private readonly System.Drawing.Rectangle destBounds;

    private Timer timer;

    private IntPtr hwnd;
    private bool initialized;
    private bool hideMain;
    private int generation;

    public WPF_OSD(System.Drawing.Rectangle destBoundsRect)
    {
        destBounds = destBoundsRect;
        //window properties
        Title = "OSD";
        AllowsTransparency = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        Focusable = false;
        IsTabStop = false;
        IsHitTestVisible = false;

        Opacity = 0.0;
        Background = Brushes.Transparent;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Normal;
        BorderThickness = new Thickness(0, 0, 0, 0);
        WindowStartupLocation = WindowStartupLocation.Manual;
        SizeToContent = SizeToContent.Manual;
        Left = -32000;
        Top = -32000;
        Width = 1;
        Height = 1;

        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Grayscale);

        label = new TextBlock
        {
            FontFamily = OverlayStyle.DisplayFont,
            FontWeight = FontWeights.SemiBold,
            FontSize = 22,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 920,
            VerticalAlignment = VerticalAlignment.Center,
        };

        pill = new Border
        {
            Margin = new Thickness(ShadowRoom),
            Padding = new Thickness(28, 11, 28, 13),
            Background = OverlayStyle.PaneBrush(0.96),
            BorderBrush = OverlayStyle.Brush(OverlayStyle.Line),
            BorderThickness = new Thickness(1),
            Effect = OverlayStyle.Shadow(30, 8, 0.55),
            Child = label,
            Opacity = 0,
            RenderTransform = rise,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        root = new Grid();
        root.Children.Add(pill);
        Content = root;
    }

    /// <summary>
    /// Raised for every message of the main on-screen display (not its per-screen copies): (durationMs, text).
    /// Raised on the calling thread, before the message is shown. No subscriber by default.
    /// </summary>
    public static event Action<int, string> MessageShown;

    /// <summary>Raises <see cref="MessageShown"/> for a message shown another way (the launch player cards).</summary>
    internal static void Mirror(int timing, string text)
    {
        try
        {
            MessageShown?.Invoke(timing, text);
        }
        catch
        {
        }
    }

    public void Show(int timing, string text)
    {
        if (!(this is WPF_OSD_Child))
        {
            Mirror(timing, text);
        }

        try
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (!initialized)
                {
                    label.Foreground = OsdTextBrush();
                    initialized = true;
                };

                if (GenericGameHandler.Instance != null)
                {
                    if (childs.Count == 0 && this.GetType() != typeof(WPF_OSD_Child) && !GenericGameHandler.Instance.hasEnded)//so it's the main osd
                    {
                        CreateChilds(timing, text);
                    }
                    else
                    {
                        foreach (WPF_OSD_Child osd in childs)
                        {
                            osd.Dispatcher.Invoke(new Action(() =>
                            {
                                osd.Show(timing, text);
                            }));
                        }
                    }
                }

                if (!hideMain)//main is hidden or it's a child
                {
                    Present(text);
                }

                //keep it for the main osd even if it's kind of
                //disabled so we can handle the childs and re-enable it automatically afterward
                timer?.Dispose();
                timer = new Timer(TimerTick, null, timing, 0);

            }));
        }
        catch { }
    }

    /// <summary>Sizes the toast to the text, centres it on its screen and fades it in. Never activates the window.</summary>
    private void Present(string text)
    {
        generation++;
        label.Text = text ?? "";

        if (hwnd == IntPtr.Zero)
        {
            hwnd = new WindowInteropHelper(this).EnsureHandle();
            OverlayNative.MakeOverlay(hwnd);//no activation, click-through, no Alt+Tab entry
        }

        CompositionTarget target = HwndSource.FromHwnd(hwnd)?.CompositionTarget;
        Matrix toDevice = target?.TransformToDevice ?? Matrix.Identity;
        Matrix fromDevice = target?.TransformFromDevice ?? Matrix.Identity;

        //The size the screen asks for, whatever DPI WPF itself renders at.
        double ratio = OverlayNative.DpiScale(destBounds) / Math.Max(0.5, toDevice.M11);
        root.LayoutTransform = Math.Abs(ratio - 1) < 0.01 ? Transform.Identity : new ScaleTransform(ratio, ratio);

        pill.CornerRadius = new CornerRadius(16);
        root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        double pillHeight = pill.DesiredSize.Height - 2 * ShadowRoom;
        pill.CornerRadius = new CornerRadius(pillHeight < 64 ? pillHeight / 2 : 16);//a pill for one line, a card for more

        Size dip = root.DesiredSize;
        int width = (int)Math.Ceiling(dip.Width * toDevice.M11);
        int height = (int)Math.Ceiling(dip.Height * toDevice.M22);
        System.Drawing.Rectangle work = OverlayNative.WorkArea(destBounds);
        System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(
            work.X + (work.Width - width) / 2,
            work.Y + (work.Height - height) / 2,
            width,
            height);

        Left = bounds.X * fromDevice.M11;
        Top = bounds.Y * fromDevice.M22;
        Width = bounds.Width * fromDevice.M11;
        Height = bounds.Height * fromDevice.M22;

        OverlayNative.Place(hwnd, bounds);
        Opacity = 1.0D;

        if (!IsVisible)
        {
            Show();//SW_SHOWNA: ShowActivated is false
            OverlayNative.Place(hwnd, bounds);
        }

        pill.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)) { EasingFunction = OverlayStyle.Out });
        rise.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(8, 0, TimeSpan.FromMilliseconds(320)) { EasingFunction = OverlayStyle.Snap });
    }

    /// <summary>The OSD colour setting (Settings.ini [Dev] OSDColor = "r,g,b") for the text, the app's ink otherwise.</summary>
    private static Brush OsdTextBrush()
    {
        try
        {
            string[] rgb = App_Misc.OSDColor?.Split(',');

            if (rgb != null && rgb.Length >= 3 &&
                byte.TryParse(rgb[0].Trim(), out byte r) &&
                byte.TryParse(rgb[1].Trim(), out byte g) &&
                byte.TryParse(rgb[2].Trim(), out byte b) &&
                !(r == 255 && g == 255 && b == 255))
            {
                return OverlayStyle.Brush(Color.FromRgb(r, g, b));
            }
        }
        catch
        {
        }

        return OverlayStyle.Brush(OverlayStyle.Ink);//the default white
    }

    private void TimerTick(object obj)
    {
        try
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                int shown = generation;
                DoubleAnimation fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(180)) { EasingFunction = OverlayStyle.Out };

                fade.Completed += (s, e) =>
                {
                    if (shown != generation)
                    {
                        return;//a newer message is showing
                    }

                    //pretty much all window parameters will mess with the game windows
                    //(mostly bringing the taskbar on top and focus stealing): the window stays, invisible and no longer topmost.
                    label.Text = "";
                    Opacity = 0.0D;

                    if (hwnd != IntPtr.Zero)
                    {
                        OverlayNative.DropTopmost(hwnd);
                    }
                };

                pill.BeginAnimation(OpacityProperty, fade);

                if (GenericGameHandler.Instance != null)
                {
                    //close all childs osd and re-enable the main one => hideMain = false;
                    if (GenericGameHandler.Instance.hasEnded && this.GetType() != typeof(WPF_OSD_Child))//so it's the main osd
                    {
                        foreach (WPF_OSD_Child child in childs)
                        {
                            child.Dispatcher.Invoke(new Action(() => { child.Close(); }));
                        }

                        childs.Clear();
                        hideMain = false;
                    }
                }

                timer.Dispose();
            }));
        }
        catch { }
    }

    private void CreateChilds(int timing, string text)
    {
        //create an osd for each screen used by the handler and hide the main one until the handler get stopped.
        int osdIndex = 0;

        foreach (Nucleus.Display dp in GenericGameHandler.Instance.screensInUse)
        {
            WPF_OSD_Child osd = new WPF_OSD_Child(dp.Bounds);
            osd.Title = $"OSD{++osdIndex}";
            osd.Show(timing, text);
            childs.Add(osd);
            hideMain = true;
        }
    }
}
