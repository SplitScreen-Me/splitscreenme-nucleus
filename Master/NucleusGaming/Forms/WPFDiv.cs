
using Nucleus;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class WPFDivFormThread
{
    public static void StartBackgroundForm(GenericGameInfo gen, Display dp)
    {
        Thread backgroundFormThread = new Thread(delegate ()
        {
            WPFDiv backgroundForm = new WPFDiv(gen, dp);
            backgroundForm.Show();
            System.Windows.Threading.Dispatcher.Run();
        });

        backgroundFormThread.IsBackground = true;
        backgroundFormThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
        backgroundFormThread.Start();
    }
}

public class WPFDiv : System.Windows.Window
{
    private System.Windows.Forms.Timer fading;
    private SolidColorBrush userColor;
    private string gameGUID;
    private float alpha = 1.0F;
    private bool fullApha = true;

    private int imgIndex = 0;
    private ImageBrush backBrush;

    public WPFDiv(GenericGameInfo game, Display screen)
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        //ShowInTaskbar = false;
        Background = Brushes.Black;
        Topmost = false;

        Name = $"SplitForm{screen.DisplayIndex}";
        Title = Name;
        WindowStartupLocation = WindowStartupLocation.Manual;

        Left = screen.Bounds.Left;
        Top = screen.Bounds.Top;

        Width = screen.Bounds.Width;
        Height = screen.Bounds.Height;

        backBrush = new ImageBrush();

        gameGUID = game.GUID;
        game.OnFinishedSetup += SetupFinished;

        GenericGameHandler.Instance?.splitForms.Add(this);
        Setup();
    }

    private void Setup()
    {     
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
        if (fading == null)
        {
            if (Directory.Exists(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, $@"gui\screenshots\{gameGUID}")))
            {         
                var imgsPath = Directory.GetFiles(Path.Combine(System.Windows.Forms.Application.StartupPath, $"gui\\screenshots\\{gameGUID}")).Where(s =>
                    s.EndsWith(".png") ||
                    s.EndsWith(".jpeg") ||
                    s.EndsWith(".jpg") ||
                    s.EndsWith(".bmp") ||
                    s.EndsWith(".gif")
                    ).ToList();

                if (imgsPath.Count > 0)
                {
                    backBrush.ImageSource = new BitmapImage(new Uri(imgsPath[imgIndex], UriKind.Absolute));
                    Background = backBrush;

                    if (imgsPath.Count >= 2)
                    {
                        imgIndex++;

                        fading = new System.Windows.Forms.Timer();
                        fading.Tick += new EventHandler(FadingTick);
                        fading.Interval = 50;
                        fading.Start();
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
            if (Directory.Exists(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, $@"gui\screenshots\{gameGUID}")))
            {
                var imgsPath = Directory.GetFiles(Path.Combine(System.Windows.Forms.Application.StartupPath, $"gui\\screenshots\\{gameGUID}")).Where(s =>
                    s.EndsWith(".png") ||
                    s.EndsWith(".jpeg") ||
                    s.EndsWith(".jpg") ||
                    s.EndsWith(".bmp") ||
                    s.EndsWith(".gif")
                    ).ToList();

                backBrush.ImageSource = new BitmapImage(new Uri(imgsPath[imgIndex], UriKind.Absolute));
                Background = backBrush;

                imgIndex++;

                if (imgIndex >= imgsPath.Count)
                {
                    imgIndex = 0;
                }
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
        if (fading != null)
        {
            fading.Dispose();
        }

        this.Dispatcher.Invoke(new Action(() => { Background = userColor; }));
    }
}



