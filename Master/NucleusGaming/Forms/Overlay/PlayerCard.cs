using Nucleus.Gaming.Coop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static Nucleus.Gaming.Localization.I18n;

namespace Nucleus.Gaming.Forms.Overlay
{
    /// <summary>What a launch card shows, read from the player on the launch thread.</summary>
    internal sealed class PlayerCardModel
    {
        public int Number = 1;
        public string Nickname = "";
        public string Device = "";
        public char Glyph;
        public string Game = "";

        /// <summary>Where the player's game window goes, in the launch thread's screen coordinates.</summary>
        public System.Drawing.Rectangle Region;

        public static PlayerCardModel From(PlayerInfo player, string gameName)
        {
            int number = player.PlayerID >= 0 ? player.PlayerID + 1 : 1;
            string device;
            char glyph;

            if (player.IsController || player.IsXInput || player.IsDInput || player.IsSDL2)
            {
                device = T("engine.overlay.device.controller");
                glyph = OverlayStyle.GlyphController;
            }
            else
            {
                switch (player.InputType)
                {
                    case InputType.KB:
                        device = T("engine.overlay.device.keyboard");
                        glyph = OverlayStyle.GlyphKeyboard;
                        break;
                    case InputType.Mouse:
                        device = T("engine.overlay.device.mouse");
                        glyph = OverlayStyle.GlyphMouse;
                        break;
                    case InputType.KBM:
                    case InputType.SingleKB:
                        device = T("engine.overlay.device.keyboardMouse");
                        glyph = OverlayStyle.GlyphKeyboard;
                        break;
                    default:
                        device = T("engine.overlay.device.controller");
                        glyph = OverlayStyle.GlyphController;
                        break;
                }
            }

            int guests = player.InstanceGuests?.Count ?? 0;

            if (guests > 0)
            {
                device = T("engine.overlay.device.withGuests", new { device, count = guests });
            }

            return new PlayerCardModel
            {
                Number = number,
                Nickname = string.IsNullOrWhiteSpace(player.Nickname) ? T("engine.overlay.playerFallback", new { number }) : player.Nickname.Trim(),
                Device = device,
                Glyph = glyph,
                Game = gameName ?? "",
                Region = player.MonitorBounds,
            };
        }
    }

    /// <summary>Card variant and size for a screen region.</summary>
    internal struct CardLayout
    {
        /// <summary>Badge and name only, for small regions (6 players and more on one screen).</summary>
        public bool Compact;

        /// <summary>Card design units (DIP at 1920x1080, 100 %) to region DIPs.</summary>
        public double Scale;

        public static CardLayout For(double regionWidthDip, double regionHeightDip)
        {
            double w = Math.Max(1, regionWidthDip);
            double h = Math.Max(1, regionHeightDip);

            //1 = a whole 1080p screen. The card shrinks slower than the region: a quarter of a screen still reads well.
            double area = Math.Sqrt((w / 1920.0) * (h / 1080.0));
            double k = Clamp(Math.Sqrt(area) * 1.12, 0.5, 2.6);
            k = Math.Min(k, Math.Min(0.86 * w / PlayerCard.FullWidth, 0.40 * h / PlayerCard.FullHeight));

            if (k >= 0.74)
            {
                return new CardLayout { Compact = false, Scale = k };
            }

            return new CardLayout { Compact = true, Scale = Clamp(Math.Sqrt(area) * 1.65, 0.55, 1.8) };
        }

        /// <summary>Final scale once the card is measured: never wider than 90 % or taller than 42 % of the region.</summary>
        public double Fit(Size card, double regionWidthDip, double regionHeightDip)
        {
            return Math.Min(Scale, Math.Min(0.9 * regionWidthDip / Math.Max(1, card.Width), 0.42 * regionHeightDip / Math.Max(1, card.Height)));
        }

        private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// A player's launch card in the "Bezel" look: P{n} badge in the player's colour, nickname, device and game,
    /// "Starting…" then "Ready". Drawn in design units, the window scales it to the region.
    /// </summary>
    internal sealed class PlayerCard : Grid
    {
        public const double FullWidth = 520;
        public const double FullHeight = 116;

        private readonly bool compact;
        private readonly Color color;
        private readonly double margin;

        private readonly Border card;
        private readonly SolidColorBrush border;
        private readonly Grid inner;
        private readonly Border ring;
        private readonly Grid spinner;
        private readonly RotateTransform spin;
        private readonly Grid check;
        private readonly ScaleTransform checkScale;
        private readonly TextBlock status;
        private readonly Rectangle shimmer;
        private readonly TranslateTransform shimmerMove;
        private readonly Rectangle fill;
        private readonly ScaleTransform fillScale;
        private readonly TranslateTransform move = new TranslateTransform();
        private readonly ScaleTransform zoom = new ScaleTransform(1, 1);
        private double edgeWidth;
        private bool working;

        public bool IsReady { get; private set; }

        public PlayerCard(PlayerCardModel model, bool compact)
        {
            this.compact = compact;
            color = OverlayStyle.PlayerColor(model.Number);
            margin = compact ? 28 : 44;

            double radius = compact ? 15 : 18;
            double edge = compact ? 2 : 3;

            SnapsToDevicePixels = false;
            UseLayoutRounding = false;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.Grayscale);
            RenderTransformOrigin = new Point(0.5, 0.5);
            RenderTransform = new TransformGroup { Children = { zoom, move } };

            //Shadow on its own cached layer: the spinner doesn't redraw it.
            Border shadow = new Border
            {
                Margin = new Thickness(margin),
                CornerRadius = new CornerRadius(radius),
                Background = OverlayStyle.Brush(OverlayStyle.Pane),
                Effect = OverlayStyle.Shadow(compact ? 26 : 40, compact ? 6 : 12, 0.6),
                CacheMode = new BitmapCache(),
            };

            border = new SolidColorBrush(OverlayStyle.Line);
            card = new Border
            {
                Margin = new Thickness(margin),
                CornerRadius = new CornerRadius(radius),
                Background = OverlayStyle.PaneBrush(0.97),
                BorderBrush = border,
                BorderThickness = new Thickness(1),
            };

            if (!compact)
            {
                //A minimum, not a fixed width: a long device + game line widens the card instead of being cut
                //(CardLayout.Fit then scales the measured card down to 90 % of the region at most).
                card.MinWidth = FullWidth;
            }

            inner = new Grid();
            inner.SizeChanged += (s, e) => inner.Clip = new RectangleGeometry(new Rect(e.NewSize), radius - 1, radius - 1);
            card.Child = inner;

            //A breath of the player's colour behind the badge.
            RadialGradientBrush glow = new RadialGradientBrush
            {
                Center = new Point(0.06, 0.5),
                GradientOrigin = new Point(0.06, 0.5),
                RadiusX = compact ? 0.55 : 0.42,
                RadiusY = 1.4,
            };
            glow.GradientStops.Add(new GradientStop(OverlayStyle.WithAlpha(color, 0.28), 0));
            glow.GradientStops.Add(new GradientStop(OverlayStyle.WithAlpha(color, 0.06), 0.55));
            glow.GradientStops.Add(new GradientStop(OverlayStyle.WithAlpha(color, 0), 1));
            glow.Freeze();
            inner.Children.Add(new Rectangle { Fill = glow });

            //Lit top edge.
            LinearGradientBrush highlight = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            highlight.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 0));
            highlight.GradientStops.Add(new GradientStop(Color.FromArgb(28, 255, 255, 255), 0.3));
            highlight.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1));
            highlight.Freeze();
            inner.Children.Add(new Rectangle { Height = 1, VerticalAlignment = VerticalAlignment.Top, Fill = highlight });

            Grid content = new Grid
            {
                Margin = compact ? new Thickness(9, 9, 18, 9) : new Thickness(22, 20, 26, 20),
            };
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = compact ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inner.Children.Add(content);

            //Badge
            double badge = compact ? 42 : 66;
            Grid badgeHost = new Grid { Width = badge * (model.Number >= 10 ? 1.24 : 1), Height = badge, VerticalAlignment = VerticalAlignment.Center };

            ring = new Border
            {
                CornerRadius = new CornerRadius(compact ? 11 : 16),
                BorderBrush = OverlayStyle.Brush(color),
                BorderThickness = new Thickness(compact ? 2 : 2.5),
                Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1),
            };

            LinearGradientBrush badgeFill = new LinearGradientBrush(OverlayStyle.Mix(color, Colors.White, 0.14), color, 90);
            badgeFill.Freeze();

            Border badgeFace = new Border
            {
                CornerRadius = new CornerRadius(compact ? 11 : 16),
                Background = badgeFill,
                Child = new TextBlock
                {
                    Text = T("engine.overlay.badge", new { number = model.Number }),
                    FontFamily = OverlayStyle.DisplayFont,
                    FontWeight = FontWeights.Bold,
                    FontSize = (compact ? 19 : 27) * (model.Number >= 10 ? 0.92 : 1),
                    Foreground = OverlayStyle.Brush(OverlayStyle.OnLight),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, compact ? 1 : 2),
                },
            };

            badgeHost.Children.Add(ring);
            badgeHost.Children.Add(badgeFace);
            content.Children.Add(badgeHost);

            //Name, device, game
            TextBlock name = new TextBlock
            {
                Text = model.Nickname,
                FontFamily = OverlayStyle.DisplayFont,
                FontWeight = FontWeights.SemiBold,
                FontSize = compact ? 22 : 31,
                Foreground = OverlayStyle.Brush(OverlayStyle.Ink),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
            };

            if (compact)
            {
                name.MaxWidth = 250;
                name.Margin = new Thickness(13, 0, 0, 2);
                Grid.SetColumn(name, 1);
                content.Children.Add(name);
            }
            else
            {
                StackPanel texts = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(20, 0, 18, 2) };
                texts.Children.Add(name);
                texts.Children.Add(MetaLine(model));
                Grid.SetColumn(texts, 1);
                content.Children.Add(texts);
            }

            //State: spinner then check, with words on the full card.
            double glyph = compact ? 18 : 22;
            Grid state = new Grid { Width = glyph, Height = glyph, VerticalAlignment = VerticalAlignment.Center };

            spin = new RotateTransform(0, glyph / 2, glyph / 2);
            spinner = Spinner(glyph, compact ? 2.2 : 2.6, color);
            spinner.RenderTransform = spin;
            state.Children.Add(spinner);

            checkScale = new ScaleTransform(1, 1);
            check = Check(glyph);
            check.Opacity = 0;
            check.RenderTransformOrigin = new Point(0.5, 0.5);
            check.RenderTransform = checkScale;
            state.Children.Add(check);

            status = new TextBlock
            {
                Text = T("engine.overlay.starting"),
                FontFamily = OverlayStyle.TextFont,
                FontWeight = FontWeights.SemiBold,
                FontSize = 16,
                Foreground = OverlayStyle.Brush(OverlayStyle.Ink2),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 1),
            };

            StackPanel stateRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(compact ? 14 : 0, 0, 0, 0),
            };
            stateRow.Children.Add(state);

            if (!compact)
            {
                stateRow.Children.Add(status);
            }

            Grid.SetColumn(stateRow, 2);
            content.Children.Add(stateRow);

            //Bottom edge: a light running along it while starting, filled when ready.
            Grid edgeHost = new Grid { Height = edge, VerticalAlignment = VerticalAlignment.Bottom, ClipToBounds = true };
            edgeHost.Children.Add(new Rectangle { Fill = OverlayStyle.Brush(color, 0.16) });

            LinearGradientBrush shimmerBrush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            shimmerBrush.GradientStops.Add(new GradientStop(OverlayStyle.WithAlpha(color, 0), 0));
            shimmerBrush.GradientStops.Add(new GradientStop(OverlayStyle.WithAlpha(color, 1), 0.5));
            shimmerBrush.GradientStops.Add(new GradientStop(OverlayStyle.WithAlpha(color, 0), 1));
            shimmerBrush.Freeze();

            shimmerMove = new TranslateTransform(-1000, 0);
            shimmer = new Rectangle { Fill = shimmerBrush, HorizontalAlignment = HorizontalAlignment.Left, RenderTransform = shimmerMove };
            edgeHost.Children.Add(shimmer);

            fillScale = new ScaleTransform(0, 1);
            fill = new Rectangle { Fill = OverlayStyle.Brush(color), RenderTransform = fillScale };
            edgeHost.Children.Add(fill);

            edgeHost.SizeChanged += (s, e) =>
            {
                edgeWidth = e.NewSize.Width;
                shimmer.Width = Math.Max(40, edgeWidth * 0.38);

                if (working && !IsReady)
                {
                    RunShimmer();
                }
            };
            inner.Children.Add(edgeHost);

            Children.Add(shadow);
            Children.Add(card);
        }

        /// <summary>Size of the card with the room for its shadow, in design units.</summary>
        public Size Measured
        {
            get
            {
                Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                return DesiredSize;
            }
        }

        /// <summary>Size of the card alone (no shadow room), in design units.</summary>
        public Size CardSize
        {
            get
            {
                Size all = Measured;
                return new Size(Math.Max(1, all.Width - 2 * margin), Math.Max(1, all.Height - 2 * margin));
            }
        }

        public void Enter()
        {
            BeginAnimation(OpacityProperty, Anim(0, 1, 260, OverlayStyle.Out));
            move.BeginAnimation(TranslateTransform.YProperty, Anim(14, 0, 560, OverlayStyle.Snap));
            zoom.BeginAnimation(ScaleTransform.ScaleXProperty, Anim(0.965, 1, 560, OverlayStyle.Snap));
            zoom.BeginAnimation(ScaleTransform.ScaleYProperty, Anim(0.965, 1, 560, OverlayStyle.Snap));
            StartWorking();
        }

        /// <summary>Back to "Starting…" (an instance restarted).</summary>
        public void SetStarting()
        {
            if (!IsReady)
            {
                return;
            }

            IsReady = false;
            status.Text = T("engine.overlay.starting");
            status.Foreground = OverlayStyle.Brush(OverlayStyle.Ink2);
            border.BeginAnimation(SolidColorBrush.ColorProperty, null);
            border.Color = OverlayStyle.Line;
            check.BeginAnimation(OpacityProperty, null);
            check.Opacity = 0;
            fillScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            fillScale.ScaleX = 0;
            spinner.BeginAnimation(OpacityProperty, null);
            spinner.Opacity = 1;
            shimmer.BeginAnimation(OpacityProperty, null);
            shimmer.Opacity = 1;
            StartWorking();
        }

        public void SetReady(bool animate)
        {
            if (IsReady)
            {
                return;
            }

            IsReady = true;
            status.Text = T("engine.overlay.ready");
            status.Foreground = OverlayStyle.Brush(OverlayStyle.Ok);

            Color lit = OverlayStyle.WithAlpha(color, 0.75);

            if (!animate)
            {
                border.Color = lit;
                spinner.Opacity = 0;
                shimmer.Opacity = 0;
                check.Opacity = 1;
                fillScale.ScaleX = 1;
                return;
            }

            DoubleAnimation hideSpinner = Anim(1, 0, 160, OverlayStyle.Out);
            hideSpinner.Completed += (s, e) => spin.BeginAnimation(RotateTransform.AngleProperty, null);
            spinner.BeginAnimation(OpacityProperty, hideSpinner);

            DoubleAnimation hideShimmer = Anim(1, 0, 220, OverlayStyle.Out);
            hideShimmer.Completed += (s, e) => shimmerMove.BeginAnimation(TranslateTransform.XProperty, null);
            shimmer.BeginAnimation(OpacityProperty, hideShimmer);

            border.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(lit, TimeSpan.FromMilliseconds(420)) { EasingFunction = OverlayStyle.Out });
            check.BeginAnimation(OpacityProperty, Anim(0, 1, 200, OverlayStyle.Out));
            checkScale.BeginAnimation(ScaleTransform.ScaleXProperty, Anim(0.4, 1, 460, OverlayStyle.Pop));
            checkScale.BeginAnimation(ScaleTransform.ScaleYProperty, Anim(0.4, 1, 460, OverlayStyle.Pop));
            status.BeginAnimation(OpacityProperty, Anim(0, 1, 260, OverlayStyle.Out));
            fillScale.BeginAnimation(ScaleTransform.ScaleXProperty, Anim(0, 1, 620, OverlayStyle.Snap));

            ScaleTransform ringScale = (ScaleTransform)ring.RenderTransform;
            ring.BeginAnimation(OpacityProperty, Anim(0.9, 0, 760, OverlayStyle.Out));
            ringScale.BeginAnimation(ScaleTransform.ScaleXProperty, Anim(1, 1.45, 760, OverlayStyle.Out));
            ringScale.BeginAnimation(ScaleTransform.ScaleYProperty, Anim(1, 1.45, 760, OverlayStyle.Out));
        }

        public void Exit(int durationMs, Action done)
        {
            DoubleAnimation fade = Anim(Opacity, 0, durationMs, OverlayStyle.Smooth);
            fade.Completed += (s, e) => done?.Invoke();
            BeginAnimation(OpacityProperty, fade);
            move.BeginAnimation(TranslateTransform.YProperty, Anim(0, -10, durationMs, OverlayStyle.Smooth));
        }

        /// <summary>Stops the endless animations (window closing).</summary>
        public void Stop()
        {
            working = false;
            spin.BeginAnimation(RotateTransform.AngleProperty, null);
            shimmerMove.BeginAnimation(TranslateTransform.XProperty, null);
        }

        private void StartWorking()
        {
            working = true;

            //30 fps is plenty for a spinner and keeps the games' launch free of overlay work.
            DoubleAnimation rotate = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(1000)) { RepeatBehavior = RepeatBehavior.Forever };
            Timeline.SetDesiredFrameRate(rotate, 30);
            spin.BeginAnimation(RotateTransform.AngleProperty, rotate);
            RunShimmer();
        }

        private void RunShimmer()
        {
            if (edgeWidth <= 0)
            {
                return;//starts on the first SizeChanged
            }

            double length = Math.Max(40, edgeWidth * 0.38);
            DoubleAnimation run = new DoubleAnimation(-length, edgeWidth, TimeSpan.FromMilliseconds(1700))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = OverlayStyle.Smooth,
            };
            Timeline.SetDesiredFrameRate(run, 30);
            shimmerMove.BeginAnimation(TranslateTransform.XProperty, run);
        }

        private FrameworkElement MetaLine(PlayerCardModel model)
        {
            StackPanel meta = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(1, 3, 0, 0) };
            Brush ink2 = OverlayStyle.Brush(OverlayStyle.Ink2);
            FontFamily icons = OverlayStyle.IconFont;

            if (icons != null && model.Glyph != '\0')
            {
                meta.Children.Add(new TextBlock
                {
                    Text = model.Glyph.ToString(),
                    FontFamily = icons,
                    FontSize = 16,
                    Foreground = ink2,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 1, 8, 0),
                });
            }

            meta.Children.Add(new TextBlock
            {
                Text = model.Device,
                FontFamily = OverlayStyle.TextFont,
                FontSize = 16,
                Foreground = ink2,
                VerticalAlignment = VerticalAlignment.Center,
            });

            if (!string.IsNullOrWhiteSpace(model.Game))
            {
                meta.Children.Add(new TextBlock
                {
                    Text = "·",
                    FontFamily = OverlayStyle.TextFont,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = OverlayStyle.Brush(OverlayStyle.Ink3),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(9, 0, 9, 0),
                });

                meta.Children.Add(new TextBlock
                {
                    Text = model.Game,
                    FontFamily = OverlayStyle.TextFont,
                    FontSize = 16,
                    Foreground = OverlayStyle.Brush(OverlayStyle.Ink3),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 230,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            return meta;
        }

        private static Grid Spinner(double size, double thickness, Color color)
        {
            Grid host = new Grid { Width = size, Height = size };
            double r = (size - thickness) / 2;
            double c = size / 2;

            host.Children.Add(new Ellipse
            {
                Margin = new Thickness(thickness / 2),
                Stroke = OverlayStyle.Brush(OverlayStyle.LineStrong, 0.9),
                StrokeThickness = thickness,
            });

            double end = (-90 + 110) * Math.PI / 180;
            PathFigure arc = new PathFigure { StartPoint = new Point(c, c - r), IsClosed = false };
            arc.Segments.Add(new ArcSegment(new Point(c + r * Math.Cos(end), c + r * Math.Sin(end)), new Size(r, r), 0, false, SweepDirection.Clockwise, true));
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(arc);
            geometry.Freeze();

            host.Children.Add(new Path
            {
                Data = geometry,
                Stroke = OverlayStyle.Brush(color),
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            });

            return host;
        }

        private static Grid Check(double size)
        {
            Grid host = new Grid { Width = size, Height = size };
            host.Children.Add(new Ellipse { Fill = OverlayStyle.Brush(OverlayStyle.Ok) });

            PathFigure mark = new PathFigure { StartPoint = new Point(size * 0.29, size * 0.52), IsClosed = false };
            mark.Segments.Add(new LineSegment(new Point(size * 0.44, size * 0.66), true));
            mark.Segments.Add(new LineSegment(new Point(size * 0.72, size * 0.37), true));
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(mark);
            geometry.Freeze();

            host.Children.Add(new Path
            {
                Data = geometry,
                Stroke = OverlayStyle.Brush(OverlayStyle.OnLight),
                StrokeThickness = size * 0.105,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
            });

            return host;
        }

        private static DoubleAnimation Anim(double from, double to, int ms, IEasingFunction easing)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(ms)) { EasingFunction = easing };
        }
    }

    /// <summary>
    /// The window of one card: centred on the player's region, topmost, click-through, never activated,
    /// sized in physical pixels (the card scales inside it).
    /// </summary>
    internal sealed class PlayerCardWindow : System.Windows.Window
    {
        private readonly System.Drawing.Rectangle pixels;
        private bool closed;

        public PlayerCard Card { get; }

        public int Number { get; }

        public PlayerCardWindow(PlayerCardModel model)
        {
            Number = model.Number;
            Title = T("engine.overlay.windowTitle", new { number = model.Number });
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            ShowActivated = false;
            Topmost = true;
            Focusable = false;
            IsTabStop = false;
            IsHitTestVisible = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            SizeToContent = SizeToContent.Manual;
            Left = -32000;
            Top = -32000;
            Width = 1;
            Height = 1;

            System.Drawing.Rectangle region = model.Region;
            double dpi = OverlayNative.DpiScale(region);
            double regionWidth = region.Width / dpi;
            double regionHeight = region.Height / dpi;

            CardLayout layout = CardLayout.For(regionWidth, regionHeight);
            Card = new PlayerCard(model, layout.Compact);

            Size canvas = Card.Measured;
            double scale = layout.Fit(Card.CardSize, regionWidth, regionHeight) * dpi;
            int width = (int)Math.Ceiling(canvas.Width * scale);
            int height = (int)Math.Ceiling(canvas.Height * scale);

            pixels = new System.Drawing.Rectangle(
                region.X + (region.Width - width) / 2,
                region.Y + (region.Height - height) / 2,
                width,
                height);

            Content = new Viewbox { Stretch = Stretch.Uniform, Child = Card };
        }

        /// <summary>Shows the card without activating it.</summary>
        public void ShowOverlay()
        {
            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            OverlayNative.MakeOverlay(hwnd);

            //WPF's own bounds in DIPs, so it never moves the window back somewhere else.
            Matrix fromDevice = HwndSource.FromHwnd(hwnd)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
            Left = pixels.X * fromDevice.M11;
            Top = pixels.Y * fromDevice.M22;
            Width = pixels.Width * fromDevice.M11;
            Height = pixels.Height * fromDevice.M22;

            OverlayNative.Place(hwnd, pixels);
            Card.Opacity = 0;
            Show();
            OverlayNative.Place(hwnd, pixels);
            Card.Enter();
        }

        /// <summary>Above game windows made topmost after the card was shown.</summary>
        public void Raise()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            if (hwnd != IntPtr.Zero)
            {
                OverlayNative.RaiseTopmost(hwnd);
            }
        }

        /// <summary>Closes the window once, whatever closed it first.</summary>
        public void Dismiss()
        {
            if (!closed)
            {
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            closed = true;
            Card.Stop();
            base.OnClosed(e);
        }
    }
}
