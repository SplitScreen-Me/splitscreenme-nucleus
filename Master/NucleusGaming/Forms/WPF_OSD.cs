using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WindowScrape.Static;

internal class WPF_OSD_Child : WPF_OSD
{
    public WPF_OSD_Child(System.Drawing.Rectangle destBoundsRect) : base(destBoundsRect)
    {
    }
}

public class WPF_OSD : Window
{
    /// Can use different rgb colors per word with the following text input pattern ex:"*@255,0,127|Settings@* Saved"  
    /// here the word "Settings" will be in rgb(255,0,127) and "Saved" in the default osd color '|','*' and '@' must be reserverd for formating.

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_EX_STYLE = -20;
    private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

    private string[] osdColor = App_Misc.OSDColor.Split(',');

    private System.Windows.Controls.Label Value;

    private List<WPF_OSD_Child> childs = new List<WPF_OSD_Child>();

    private System.Drawing.Rectangle destBounds;

    private Timer timer;

    private bool initialized;
    private bool setStyle;
    private bool hideMain;

    public WPF_OSD(System.Drawing.Rectangle destBoundsRect)
    {
        destBounds = destBoundsRect;
        //window properties
        Title = "OSD";
        AllowsTransparency = true;
        ShowInTaskbar = false;
        Focusable = false;
        IsTabStop = false;

        Opacity = 0.0;
        Background = new SolidColorBrush(Color.FromArgb(0, 154, 65, 55));
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Normal;
        BorderThickness = new Thickness(0, 0, 0, 0);
        BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        WindowStartupLocation = WindowStartupLocation.Manual;

        //Important this is necessary in order to move
        //the window to the desired screen.
        Left = destBounds.X;
        Top = destBounds.Y;
        Width = destBounds.Width;
        Height = destBounds.Height;

        Border roundedLabel = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(235, 0, 0, 0)),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(10, 5, 10, 5),
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.DarkGray
        };

        //label properties
        Value = new System.Windows.Controls.Label();
        Value.FontSize = 25f;
        Value.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        Value.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
        Value.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
        Value.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        Value.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;     
        Value.BorderThickness = new Thickness(0);

        roundedLabel.Child = Value;
        Visibility = Visibility.Hidden;
        
        AddChild(roundedLabel);
    }

    private void Resize(TextBlock textBlock)
    {
        Opacity = 0.0;
        Value.FontSize = 25f;
        Value.Content = textBlock;
        UpdateLayout();
    }

    private TextBlock ParseText(string rawText)
    {
        List<string> rawWordColor = new List<string>(rawText.Split('*'));

        TextBlock textBlock = new TextBlock();

        bool firstWord = true;
        //"Test *@255,0,127|Settings and test and more words@* test *@0,255,0|Settings and test and more words@* *@0,0,255|Settings and test and more words@*"
        foreach (string wordColor in rawWordColor)
        {
            string word = null;
            string colorString = null;

            if (wordColor.StartsWith("@") && wordColor.EndsWith("@"))
            {
                string trimmedWordColor = wordColor.TrimStart('@').TrimEnd('@');

                string[] split = trimmedWordColor.Split('|');
                word = split[1];
                colorString = split[0];
            }
            else
            {
                word = wordColor;
            }

            string[] wordOrSentence = null;

            if(word == null || word == " " || word == string.Empty)
            {
                continue;
            }

            wordOrSentence = word?.Split(' ');

            if(wordOrSentence == null)
            {
                continue;
            }

            bool isSingleWord = wordOrSentence.Length == 1;

            if (colorString == null)
            {
                //string noSpaces = wordColor.Contains(" ") ? wordColor.Replace(" ","") : wordColor;
                textBlock.Inlines.Add(firstWord ? " " + wordColor + " " : wordColor + " ");
            }
            else
            {
                if (isSingleWord)
                {
                    string noSpaces = word.Contains(" ") ? word.Replace(" ", "") : word;
                    textBlock.Inlines.Add(new Run(firstWord ? " " + noSpaces + " " : noSpaces + " ") { Foreground = GetWordBrush(colorString) });
                }
                else
                {
                    bool firstSubWord = true;

                    foreach (string subWord in wordOrSentence)
                    {
                        string noSpaces = subWord.Contains(" ") ? subWord.Replace(" ", "") : subWord;
                        if (firstWord && firstSubWord)
                        {
                            textBlock.Inlines.Add(new Run(firstWord ? " " + noSpaces + " " : noSpaces) { Foreground = GetWordBrush(colorString) });
                        }
                        else
                        {
                            textBlock.Inlines.Add(new Run(noSpaces + " ") { Foreground = GetWordBrush(colorString) });
                        }
                        
                        firstSubWord = false;
                    }
                }
            }

            if(firstWord)
            {
                firstWord = false;
            }    
        }

        return textBlock;
    }

    private SolidColorBrush GetWordBrush(string sColor)
    {
        SolidColorBrush brush = new SolidColorBrush();
        string[] rgb = sColor.Split(',');
        brush.Color = Color.FromArgb(255,byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
        return brush;
    }

    public void Show(int timing, string text)
    {
        try
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (!initialized)
                {
                    //Set Value.Foreground here else the winforms designer breaks
                    Value.Foreground = new SolidColorBrush(Color.FromArgb(255, byte.Parse(osdColor[0]), byte.Parse(osdColor[1]), byte.Parse(osdColor[2])));
                    Value.BorderBrush = Value.Foreground;
                    Visibility = Visibility.Visible;
                    initialized = true;
                };

                if (GenericGameHandler.Instance != null)
                {
                    if (childs.Count == 0 && this.GetType() != typeof(WPF_OSD_Child) && !GenericGameHandler.Instance.HasEnded)//so it's the main osd
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
                    //pretty much all window parameters will mess with the game windows
                    //(mostly bringing the taskbar on top and focus stealing) so using tricky ways here.
                    Resize(ParseText(text));

                    Show();
                   
                    IntPtr hwnd = User32Interop.FindWindow(null, Title);

                    while (hwnd == IntPtr.Zero)
                    {
                        hwnd = User32Interop.FindWindow(null, Title);
                    }

                    if (!setStyle)
                    {
                        //hide the window from Alt + Tab
                        SetWindowLong(hwnd, GWL_EX_STYLE, (GetWindowLong(hwnd, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
                        setStyle = true;
                    }

                    WindowState = WindowState.Maximized;//only way found to properly center the osd between dpi scaling factors
                    Topmost = true;
                    HwndInterface.MakeTop(hwnd);
                    Opacity = 1.0D;
                }

                //keep it for the main osd even if
                //disabled so we can handle the childs and re-enable it automatically afterward
                timer?.Dispose();
                timer = new Timer(TimerTick, null, timing, 0);
                
            }));
        }
        catch { }
    }

    private void TimerTick(object obj)
    {
        this.Dispatcher.Invoke(new Action(() =>
        {
            //pretty much all window parameters will mess with the game windows
            //(mostly bringing the taskbar on top and focus stealing) so using tricky ways here.
            Value.Content = "";    
            Opacity = 0.0D;
            IsEnabled = false;
            Topmost = false;
            IntPtr hwnd = User32Interop.FindWindow(null, Title);

            if(hwnd != IntPtr.Zero)
            {
                hwnd = User32Interop.FindWindow(null, Title);
                //hide the window from Alt + Tab
                SetWindowLong(hwnd, GWL_EX_STYLE, (GetWindowLong(hwnd, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
                setStyle = true;
            }

            if (GenericGameHandler.Instance != null)
            {
                //close all childs osd and re-enable the main one => hideMain = false;
                if (GenericGameHandler.Instance.HasEnded && this.GetType() != typeof(WPF_OSD_Child))//so it's the main osd
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



