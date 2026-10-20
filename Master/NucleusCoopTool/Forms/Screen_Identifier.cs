using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

public static class ScreenIdInstance
{
    public static ScreenId Instance;
}

public class ScreenId : System.Windows.Window
{
    private System.Windows.Forms.Timer DisposeT;

    public ScreenId(System.Drawing.Point loc)
    {
        WindowStyle = System.Windows.WindowStyle.None;
        ResizeMode = System.Windows.ResizeMode.NoResize;
        ShowInTaskbar = false;
        AllowsTransparency = true;
        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
        Opacity = 1.0;
        Topmost = false;

        Title = Name;
        WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

        Left = loc.X;
        Top = loc.Y;

        Width = 150;
        Height = 150;

        Closed += (s, e) => OnClose();

        Border roundedLabel = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(10, 5, 10, 5),
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.DarkGray
        };

        System.Windows.Controls.Label Value = new System.Windows.Controls.Label();
        Value.FontSize = 25f;
        Value.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
        Value.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 255, 0));
        Value.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
        Value.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
        Value.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        Value.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
        Value.BorderThickness = new System.Windows.Thickness(1);
        Value.Height = Height;
        Value.Width = Width;
        Value.Content = $"✌";

        roundedLabel.Child = Value;

        AddChild(roundedLabel);

        DisposeT = new System.Windows.Forms.Timer();
        DisposeT.Tick += new EventHandler(CloseTick);
        DisposeT.Interval = 2000;
        DisposeT.Start();        
    }

    private void OnClose()
    {
        DisposeT?.Dispose();
        ScreenIdInstance.Instance = null;
    }

    private void CloseTick(object Object, EventArgs EventArgs)
    {
        this.Dispatcher.Invoke(new Action(() =>
        {      
            Close();
        }));     
    }
}