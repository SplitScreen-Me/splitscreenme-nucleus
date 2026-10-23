using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Size = System.Windows.Size;
using System.Drawing;
using WindowScrape.Types;
using System.Collections.Generic;
using System.Windows.Interop;
using WindowScrape.Static;
using Nucleus.Gaming.Windows.Interop;
using WindowScrape.Constants;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using System.Linq;
using System.Windows.Forms;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.UI;

public static class WindowsMergerThread
{
    public static Thread MergerThread;

    public static void StartWindowsMerger(Size size)
    {   
        Thread windowsMergerThread = new Thread(() =>
        {
            var backgroundForm = new WindowsMerger(size);
            backgroundForm.Show();
            Dispatcher.Run();
        });

        windowsMergerThread.IsBackground = true;
        windowsMergerThread.SetApartmentState(ApartmentState.STA);
        windowsMergerThread.Start();

        MergerThread = windowsMergerThread;
    }
}

public class WindowsMerger : System.Windows.Window
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string className, string windowText);

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr childHWnd, IntPtr parentHWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hWnd, out Nucleus.Gaming.Coop.BasicTypes.RECT lpRect);

    private System.Windows.Forms.Timer fadingTimer;
    private SolidColorBrush userColor;
    private string gameGUID;
    private float alpha = 1.0F;
    private bool fullApha = true;

    private int imgIndex = 0;
    private ImageBrush backBrush;

    public IntPtr Handle { get;private set;}

    public Rectangle WindowBounds { get; private set; }

    public static WindowsMerger Instance { get;private set;}
    private List<string> images;

    public WindowsMerger(Size size)
    {       
        Name = "WindowsMerger";
        Title = Name;
        Background = System.Windows.Media.Brushes.Black;

        WindowStartupLocation = WindowStartupLocation.Manual;        
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        
        Top = 0; Left = 0; Width = size.Width; Height = size.Height;

        WindowBounds = new Rectangle((int)this.Left,(int)Top,(int)Width,(int)Height);
        Instance = this;

        backBrush = new ImageBrush();
        Setup();
        
        Loaded += async (sender, e) => await GetHandleAsync();
    } 

    public void Dispose()
    {
        if (Instance == null)
        {
            return;
        }
            
        Instance?.Dispatcher.Invoke(() =>
        {
            Instance?.Close();
            Dispatcher.CurrentDispatcher?.BeginInvokeShutdown(DispatcherPriority.Background);
        });

        Instance = null;
        WindowsMergerThread.MergerThread = null;
    }

    private void MainWindowLoaded(object sender, RoutedEventArgs e)
    {
        HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source.AddHook(new HwndSourceHook(WndProc));
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if(msg != 127)
        {
            //Console.WriteLine(msg);
        }
       
        return IntPtr.Zero;
    }

    private async Task GetHandleAsync()
    {
        while (Handle == IntPtr.Zero)
        {
            Handle = FindWindow(null, Title);
            await Task.Delay(50);
        }

        await SetNewPlayersBoundsAsync();
    }

    private async Task SetNewPlayersBoundsAsync()
    {
        if (GenericGameHandler.Instance != null)
        {
            var players = GenericGameHandler.Instance.profile.DevicesList;

            foreach (var player in players)
            {
                await SetChildBoundsAsync(player);
            }
        }

        if (App_Layouts.LosslessHook && !GameProfile.Ready)
        {
            await InjectLosslessHookAsync();
        }
        else if(GameProfile.EnableLosslessHook)
        {
            await InjectLosslessHookAsync();
        }     
    }

    private async Task InjectLosslessHookAsync()
    {
        Process[] searchLossless = Process.GetProcessesByName("LosslessScaling");

        while (searchLossless.Length == 0)
        {
            searchLossless = Process.GetProcessesByName("LosslessScaling");
            await Task.Delay(1500);
        }

        string pid = searchLossless[0].Id.ToString();

        string injectorPath = Path.Combine(System.Windows.Forms.Application.StartupPath, $@"utils\Hooks\Hk_IJ.exe");

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = injectorPath;
        startInfo.Arguments = $"\"{pid}\" \"Lossless_Hook.dll\" \"Lossless_Hook.dll\"";
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = false;
        startInfo.WorkingDirectory = Path.Combine(System.Windows.Forms.Application.StartupPath, $@"utils\Hooks");
        startInfo.RedirectStandardOutput = false;

        Process injectProc = Process.Start(startInfo);

        await Task.Run(() => injectProc.WaitForExit());
    }

    private Task SetChildBoundsAsync(PlayerInfo player)
    {
        return Task.Run(() =>
        {
            GetClientRect(Handle, out Nucleus.Gaming.Coop.BasicTypes.RECT clientRect);
            player.MonitorBounds = TranslateBounds(
                player,
                new Rectangle(clientRect.Left, clientRect.Top, clientRect.Right, clientRect.Bottom)
            );
        });
    }

    private Dictionary<string, IntPtr> childsInfo = new Dictionary<string, IntPtr>();

    private int currentFocused = 0;

    public void SwitchChildFocus()
    {
        if (childsInfo.Count == 0)
        {
            return;
        }
        
        if (currentFocused == childsInfo.Count) { currentFocused = 0; }

        var players = childsInfo.Keys.ToList();
        HwndInterface.ActivateWindow(childsInfo[players[currentFocused]]);
        Globals.MainOSD.Show(1600, $"*@{OSDHighlightColors.Regular}|{players[currentFocused]}@* Window Focused");

        currentFocused++;
    }

    public async Task InsertGameWindows()
    {
        var players = GenericGameHandler.Instance.profile.DevicesList;

        for (int i = 0; i < players.Count; i++)
        {
            await Task.Delay(1200);//2000

            PlayerInfo player = players[i];

            while (player.ProcessData == null)
            {
                await Task.Delay(150);
            }

            while (player.ProcessData.HWnd == null)
            {
                await Task.Delay(150);
            }

            while (player.ProcessData.HWnd.NativePtr == null)
            {
                await Task.Delay(150);
            }

            if (GenericGameHandler.Instance.CurrentGameInfo.RefreshWindowAfterStart)
            {
                GlobalWindowMethods.ShowWindow(player.ProcessData.HWnd.NativePtr, 6);
                GlobalWindowMethods.ShowWindow(player.ProcessData.HWnd.NativePtr, 9);
            }

            SetParent(player.ProcessData.HWnd.NativePtr, Handle);
            await Task.Delay(500);//1000
            player.ProcessData.HWnd = new HwndObject(player.ProcessData.HWnd.NativePtr);

            HwndInterface.MoveWindow(player.ProcessData.HWnd.NativePtr, player.MonitorBounds.X, player.MonitorBounds.Y, player.MonitorBounds.Width, player.MonitorBounds.Height, true);
           
            await Task.Delay(500);//1000

            player.ProcessData.Position = player.MonitorBounds.Location;
            player.ProcessData.Size = player.MonitorBounds.Size;

            while (player.RawInputWindow == null)
            {
                await Task.Delay(150);
            }

            player.RawInputWindow.UpdateBounds();
            childsInfo.Add(player.Nickname, player.ProcessData.HWnd.NativePtr);
        }

        SetupFinished();
    }

    public IntPtr RefreshChildWindows(PlayerInfo player)
    {
        IntPtr foundChild = IntPtr.Zero;
        var childs = HwndInterface.EnumChildren(Handle);

        foreach (var child in childs)
        {
            if (child == player.ProcessData.HWnd.NativePtr)
            {
                player.ProcessData.HWnd = new HwndObject(child);
                User32Interop.MoveWindow(child, player.MonitorBounds.X, player.MonitorBounds.Y, player.MonitorBounds.Width, player.MonitorBounds.Height, true);
                player.ProcessData.Position = player.MonitorBounds.Location;
                player.ProcessData.Size = player.MonitorBounds.Size;
                foundChild = child;
            }
        }

        return foundChild;
    }

    private Rectangle TranslateBounds(PlayerInfo player, Rectangle mergerWindowClientRectangle)
    {
        var ogScr = player.Owner.MonitorBounds;
        var ogMb = player.MonitorBounds;
        var ogscr = new Vector2(ogScr.X, ogScr.Y);
        var ogPMb = new Vector2(ogMb.X, ogMb.Y);
        var VogOnScrLoc = Vector2.Subtract(ogPMb, ogscr);

        float ratioMW = (float)ogScr.Width / mergerWindowClientRectangle.Width;
        float ratioMH = (float)ogScr.Height / mergerWindowClientRectangle.Height;

        return new Rectangle(
            mergerWindowClientRectangle.Left + Convert.ToInt32(VogOnScrLoc.X / ratioMW),
            mergerWindowClientRectangle.Top + Convert.ToInt32(VogOnScrLoc.Y / ratioMH),
            Convert.ToInt32(ogMb.Width / ratioMW),
            Convert.ToInt32(ogMb.Height / ratioMH)
        );
    }

    //Extra aesthetic setup
    private void Setup()
    {
        gameGUID = GenericGameHandler.Instance.CurrentGameInfo.GUID;

        foreach (KeyValuePair<string, SolidColorBrush> color in BackgroundColors.ColorsDictionnary)
        {
            if (color.Key != GameProfile.SplitDivColor)
            {
                continue;
            }

            userColor = color.Value;
            break;
        }

        SlideshowStart();
    }

    private void SlideshowStart()
    {
        if (fadingTimer == null)
        {
            if (Directory.Exists(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, $@"gui\screenshots\{gameGUID}")))
            {
                images = Directory.GetFiles(Path.Combine(System.Windows.Forms.Application.StartupPath, $"gui\\screenshots\\{gameGUID}")).Where(s =>
                  s.EndsWith(".png") ||
                  s.EndsWith(".jpeg") ||
                  s.EndsWith(".jpg") ||
                  s.EndsWith(".bmp") ||
                  s.EndsWith(".gif")
                  ).ToList();

                if (images.Count > 0)
                {
                    backBrush.ImageSource = new BitmapImage(new Uri(images[imgIndex], UriKind.Absolute));
                    Background = backBrush;

                    if (images.Count >= 2)
                    {
                        imgIndex++;

                        fadingTimer = new System.Windows.Forms.Timer();
                        fadingTimer.Tick += new EventHandler(FadingTick);
                        fadingTimer.Interval = 50;
                        fadingTimer.Start();
                    }
                }
            }
        }
    }

    private void FadingTick(object Object, EventArgs EventArgs)
    {
        if (fullApha)
        {
            alpha += 0.01F;
        }

        if (!fullApha)
        {
            alpha -= 0.01F;
        }

        if (alpha <= 0.01F)
        {

            backBrush.ImageSource = new BitmapImage(new Uri(images[imgIndex], UriKind.Absolute));
            Background = backBrush;

            imgIndex++;

            if (imgIndex >= images.Count)
            {
                imgIndex = 0;
            }

            fullApha = true;

        }
        else if (alpha >= 1.0)
        {
            fullApha = false;
        }

        backBrush.Opacity = alpha;
    }

    private void SetupFinished()
    {
       fadingTimer?.Dispose();
       Dispatcher.Invoke(new Action(() => { Background = userColor; }));
    } 
}
