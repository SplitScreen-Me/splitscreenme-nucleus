using NAudio.Wave;
using Nucleus.Coop.Tools;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Http_Client;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{
    public static class Patrons_Init
    {
        private static List<string> names = new List<string>();
        private static string patronsGitUrl = "https://raw.githubusercontent.com/SplitScreen-Me/splitscreenme-patreon/refs/heads/main/tiers.txt";
        private static string cacheDir = Path.Combine(Application.StartupPath, $"webview\\cache");
        private static string cacheFile = Path.Combine(Application.StartupPath, $"webview\\cache\\patreon");

        public static async Task Start()
        {
            bool canInit = await GetNameList();

            if (canInit)
            {
                UI_Interface.WindowPanel?.Focus();

                if (Patrons.Instance == null)
                {
                    Patrons patrons = new Patrons(names);
                    patrons.Show();
                }
                else
                {
                    Patrons.Instance.BringToFront();
                }
            }
            else
            {
                Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Info}|No Internet@*");
            }
        }

        private static async Task<bool> GetNameList()
        {
            try
            {               
                bool proceed = await Http_Client.CheckInternetAccessAsync(3000);

                if (proceed)
                {
                    string response = await Http_Client.GetAsync(patronsGitUrl);
                    names = response?.Split('\r')?.ToList();

                    if(names.Count == 0)
                    {
                        if (File.Exists(cacheFile))
                        {
                            names = File.ReadAllLines(cacheFile).ToList();
                        }
                    }
                    else
                    {                       
                        if (!Directory.Exists(cacheDir))
                        {
                            Directory.CreateDirectory(cacheDir);
                        }

                        File.WriteAllText(cacheFile, string.Join("", names));
                    }
                }
                else 
                {
                    if (File.Exists(cacheFile))
                    {
                        names = File.ReadAllLines(cacheFile).ToList();  
                    }
                }

                return names.Count != 0;           
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get name list: {ex.Message}");
                return false;
            }
        }
    }

    public partial class Patrons : Form
    {
        public static Patrons Instance;

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private int refreshRate = 70;
        private int spacePress = 0;
        private int scrollSpeed = 4;
        private int volMinLoc;
        private int volMaxLoc;

        private float defVolume = 0.4f;
        private float currentVolume = 0.4f;
        private float volBeforeMute;

        private Font nameFont = new Font("Impact", 40.0f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
        private Font exitBtnFont = new Font("Impact", 20f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
        private Font infoFont = new Font("Impact", 18f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
        private Font creditsFont = new Font("Impact",20f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
        private Font volFont = new Font("Franklin Gothic", 25f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
        
        private Timer invalTimer;
       
        private Rectangle topBar;
        private Rectangle bottomBar;
        private Rectangle scrollingRect;
        private Rectangle swapColorRec;
        private Rectangle volIcon;
        private Rectangle volBarHor;
        private Rectangle volBarVer;
        private RectangleF speedUpRect;
        private RectangleF exitBtn;

        private static List<string> names = new List<string>();
        private List<Rectangle> namesRects = new List<Rectangle>();

        private bool startScolling = false;
        private bool setVol = false;
        private bool muted = false;
        private bool drawMuteLine;

        private IWavePlayer waveOut;
        private MediaFoundationReader audioFile;

        private string exitBtnText = "EXIT";
        private string volIconString = "♫";

        private SolidBrush barsBrush;
        private SolidBrush randomBrush;

        private Pen[] pens = new Pen[]
        {
            new Pen(Color.White, 3),
            new Pen(Color.Gray, 3),
            new Pen(Color.Gray, 2),
            new Pen(Color.Red,3)
        };

        private DrawParticles particles;

        public Patrons(List<string> namesList)
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            MaximizeBox = false;
            MinimizeBox = false;
            
            Size = new Size(854, 480);
            MinimumSize = Size;
            MaximumSize = Size;

            MouseMove += Mouse_Move;
            MouseDown += Input;
            MouseUp += Mouse_Up;

            KeyDown += On_KeyDown;
            
            FormClosing += StopTimer;
                
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;

            FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, Width, Height, 10, 10);

            randomBrush = new SolidBrush(Color.GreenYellow);

            names = namesList;

            barsBrush = new SolidBrush(Color.FromArgb(255, 20, 20, 20));
            MakeScrollingNameList();
        }

        private void MakeScrollingNameList()
        {
            try
            {
                Graphics g = CreateGraphics();

                names.Insert(0, "\nPatreon Supporters");

                for (int i = 0; i < names.Count(); i++)
                {
                    names[i] = names[i].Replace("<div>", "");
                    names[i] = names[i].Replace("</div>", "");

                    if (i > 0)//skip thanks string
                    {
                        names[i] = names[i].Replace("\n", "");
                    }

                    var size = g.MeasureString(names[i], nameFont);
                    scrollingRect = new Rectangle(0, 0, Width, (int)(scrollingRect.Height + size.Height));

                    var nameRect = Rectangle.Empty;

                    if (i == 0)
                    {
                        nameRect = new Rectangle((int)((scrollingRect.Width / 2) - (size.Width / 2)), (int)(scrollingRect.Bottom - size.Height), (int)size.Width, (int)size.Height);
                    }
                    else
                    {
                        nameRect = new Rectangle((int)((scrollingRect.Width / 2) - (size.Width / 2)), (int)(namesRects[i - 1].Top - size.Height), (int)size.Width, (int)size.Height);
                    }

                    namesRects.Add(nameRect);
                }

                string space = "\n";
                var sizeExt = g.MeasureString(space, creditsFont);
                var spaceRec = new Rectangle((int)((scrollingRect.Width / 2) - (sizeExt.Width / 2)), (int)(namesRects[namesRects.Count() - 1].Top - sizeExt.Height), (int)sizeExt.Width, (int)sizeExt.Height);
                names.Add(space);
                namesRects.Add(spaceRec);
        
                string creditsAuthor = "https://pixabay.com/users/scaperlee-23524192/";
                sizeExt = g.MeasureString(creditsAuthor, creditsFont);
                var creditsAuthorRec = new Rectangle((int)((scrollingRect.Width / 2) - (sizeExt.Width / 2)), (int)(namesRects[namesRects.Count() - 1].Top - sizeExt.Height), (int)sizeExt.Width, (int)sizeExt.Height);
                names.Add(creditsAuthor);
                namesRects.Add(creditsAuthorRec);
                
                string credits = "Music Credits";
                sizeExt = g.MeasureString(credits, creditsFont);
                var creditsRec = new Rectangle((int)((scrollingRect.Width / 2) - (sizeExt.Width / 2)), (int)(namesRects[namesRects.Count() - 1].Top - sizeExt.Height), (int)sizeExt.Width, (int)sizeExt.Height);
                names.Add(credits);
                namesRects.Add(creditsRec);

                string t = "♥♥ THANK YOU ♥♥";
                sizeExt = g.MeasureString(t, nameFont);
                names.Add(t);
                var tRec = new Rectangle((int)((scrollingRect.Width / 2) - (sizeExt.Width / 2)), (int)(namesRects[namesRects.Count() - 1].Top - sizeExt.Height), (int)sizeExt.Width, (int)sizeExt.Height);
                namesRects.Add(tRec);

                invalTimer?.Dispose();
                invalTimer = new System.Windows.Forms.Timer();
                invalTimer.Interval = refreshRate;                 
                invalTimer.Tick += InvalTimer_Tick;
                invalTimer.Start();

                startScolling = true;
                PlayMusic();
                g.Dispose();

                Instance = this;        
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
      
        private void On_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Space)
            {
                if(spacePress == 3)
                {
                    scrollSpeed = 3;
                    spacePress = 0;
                    return;
                }

                scrollSpeed *=2;
                spacePress++;
            }
            else if( e.KeyCode == Keys.Escape)
            {
                StopTimer(null, null);
                Close();
            }
        }

        private void Mouse_Move(object sender, MouseEventArgs m)
        {
            bool drawHandCurs = false;

            if (setVol && volBarVer.Contains(m.Location))
            {
                if(m.X > (volBarHor.Left) && m.X < volBarHor.Right)
                {
                    volBarVer = new Rectangle(m.X - (volBarVer.Width / 2), (int)(volBarHor.Top + (volBarHor.Height / 2) - 8), 16, 16);
                    float test = ((float)volMaxLoc - (float)volMinLoc);
                    var currentLoc = (float)volMinLoc - ((float)volBarVer.X + ((float)volBarVer.Width / 2));

                    test = (test + (currentLoc)) / 100f;

                    if(waveOut != null)
                    {
                        waveOut.Volume = 1.0f - test;
                        currentVolume = waveOut.Volume;
                        muted = false;
                    }                          
                }

                drawHandCurs = true;
            }
            else if(volBarVer.Contains(m.Location) || exitBtn.Contains(m.Location) || speedUpRect.Contains(m.Location))
            {
                drawHandCurs = true;
            }              

            if (volIcon.Contains(m.Location))
            {
                drawMuteLine = true;
                drawHandCurs = true;
            }
            else
            {
                drawMuteLine = false;
            }

            if(drawHandCurs)
            {
                if(Cursor != Theme_Settings.Hand_Cursor)
                {
                    Cursor = Theme_Settings.Hand_Cursor;
                }
            }
            else if (Cursor != Theme_Settings.Default_Cursor)
            {
                Cursor = Theme_Settings.Default_Cursor;
            }
        }

        private void Mouse_Up(object sender, MouseEventArgs m)
        {
            setVol = false;
        }

        private void Input(object sender, MouseEventArgs m)
        {
            if (m.Button == MouseButtons.Left)
            {
                if (exitBtn.Contains(m.Location))
                {
                    StopTimer(null, null);
                    Close();
                }
                else if(volBarVer.Contains(m.Location))
                {
                    setVol = true;
                }
                else if (volIcon.Contains(m.Location))
                {
                    if (!muted)
                    {
                        waveOut.Volume = 0;
                        volBeforeMute = currentVolume;
                        currentVolume = 0;
                        muted = true;
                    }
                    else
                    {
                        waveOut.Volume = volBeforeMute;
                        currentVolume = volBeforeMute;
                        muted = false;
                    }
                }
                else if (speedUpRect.Contains(m.Location))
                {
                    if (spacePress == 3)
                    {
                        scrollSpeed = 3;
                        spacePress = 0;
                        return;
                    }

                    scrollSpeed *= 2;
                    spacePress++;
                }
                else if(topBar.Contains(m.Location))
                {
                    User32Interop.ReleaseCapture();
                    IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                    User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
                }               
            }
        }

        private void InvalTimer_Tick(object Object, EventArgs eventArgs)
        {
            try
            {
                UIThread.Post(() =>
                {
                    scrollingRect = new Rectangle(scrollingRect.X, scrollingRect.Y + 2, scrollingRect.Width, scrollingRect.Height);

                    for (int i = 0; i < namesRects.Count(); i++)
                    {
                        namesRects[i] = new Rectangle(namesRects[i].X, namesRects[i].Y + scrollSpeed, namesRects[i].Width, namesRects[i].Height);
                    }

                    Invalidate();

                    if (namesRects.Count() - 8 > 0)
                    {
                        if (namesRects[namesRects.Count() - 8].Top > ClientRectangle.Bottom)
                        {
                            scrollSpeed = 1;
                            spacePress = 0;
                        }
                    }

                    //Close the form 
                    if (namesRects.Last().Top > ClientRectangle.Bottom + 60)
                    {
                        StopTimer(null, null);
                        Close();
                    }
                });
            }
            catch
            { }
        }

        private void PlayMusic()
        {
            if (File.Exists(Globals.NucleusInstallRoot + "\\gui\\audio\\patreon_credits.mp3"))
            {
                waveOut = new WaveOutEvent();      
                audioFile = new MediaFoundationReader(Globals.NucleusInstallRoot + "\\gui\\audio\\patreon_credits.mp3");
                waveOut.Init(audioFile);
                waveOut.Volume = currentVolume;
                waveOut.Play();
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            if (particles == null)
            {
                particles = new DrawParticles();
            }

            particles.Draw(this, false, null, e, 50, 0, new int[] {255, 255, 255, 255 });

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (swapColorRec == Rectangle.Empty)
            {
                swapColorRec = new Rectangle(0, (ClientRectangle.Height / 2) - 50, Width, 100);
            }

            if (startScolling)
            {
                //loop the music
                if (waveOut?.PlaybackState == PlaybackState.Stopped)
                {
                    PlayMusic();
                }

                for (int i = 0; i < namesRects.Count(); i++)
                {
                    var rec = namesRects[i];
                    if (namesRects[i].Top >= ClientRectangle.Bottom || namesRects[i].Bottom <= ClientRectangle.Top)
                    {
                        continue;
                    }

                    if (i == 0)
                    {
                        //thanks
                        g.DrawString(names[i], nameFont, Brushes.Sienna, rec.X, rec.Y);
                    }
                    else if (i == names.Count() - 3)
                    {
                        //credits
                        g.DrawString(names[i], creditsFont, Brushes.Yellow, rec.X, rec.Y);
                    }
                    else if (i == names.Count() - 2)
                    {
                        //credits
                        g.DrawString(names[i], creditsFont, Brushes.Yellow, rec.X, rec.Y);
                    }
                    else if (i == names.Count() - 1)
                    {
                        //Final thanks
                        g.DrawString(names[i], nameFont, Brushes.Red, rec.X, rec.Y - 50);
                    }
                    else if (swapColorRec.Contains(rec))
                    {
                        //Highlight
                        g.DrawString(names[i], nameFont, randomBrush, rec.X, rec.Y);
                    }
                    else
                    {
                        //Regular
                        g.DrawString(names[i], nameFont, Brushes.DimGray, rec.X, rec.Y);
                    }
                }

                if (exitBtn == Rectangle.Empty)
                {
                    SizeF btnSize = g.MeasureString(exitBtnText, exitBtnFont);
                    topBar = new Rectangle(-2, -2, ClientRectangle.Width + 3, (int)btnSize.Height + 10);
                    bottomBar = new Rectangle(-2, ClientRectangle.Bottom - ((int)btnSize.Height + 8), ClientRectangle.Width + 3, (int)btnSize.Height + 8);
                    exitBtn = new RectangleF((ClientRectangle.Width - btnSize.Width) - 5, 4, btnSize.Width - 1, btnSize.Height - 3);
                    volBarHor = new Rectangle((int)(exitBtn.Left - 115), (int)(exitBtn.Top + exitBtn.Height / 2) - 3, 100, 6);
                    volMinLoc = volBarHor.Left;
                    volMaxLoc = volBarHor.Right;

                    SizeF iconSize = g.MeasureString(volIconString, volFont);
                    var defLoc = volBarHor.X + (defVolume * 100) + 8;
                    volBarVer = new Rectangle((int)(defLoc), (int)(volBarHor.Top + (volBarHor.Height / 2) - (16 / 2)), 16, 16);
                    volIcon = new Rectangle((int)(volBarHor.Left - iconSize.Width) - 4, (int)(exitBtn.Top + volIcon.Height / 2) - 3, (int)iconSize.Width, (int)iconSize.Height);
                }

                g.FillRectangle(barsBrush, topBar);
                g.FillRectangle(barsBrush, bottomBar);
                
                g.DrawRectangles(pens[0], new RectangleF[] { exitBtn });

                if (muted || drawMuteLine)
                {
                    if(muted)
                    {
                        g.DrawLine(pens[3], volIcon.X + 5, volIcon.Top + 5, volIcon.X + volIcon.Width - 5, volIcon.Bottom - 8);
                    }
                    else
                    {
                        g.DrawLine(pens[0], volIcon.X + 5, volIcon.Top + 5, volIcon.X + volIcon.Width - 5, volIcon.Bottom - 8);
                    }         
                }
                
                g.DrawRectangle(pens[1], volBarHor);
                
                g.DrawString(exitBtnText, exitBtnFont, Brushes.White, exitBtn.X, exitBtn.Y);
                g.FillRectangle(Brushes.White, volBarHor);
                g.FillEllipse(randomBrush, volBarVer);
                g.DrawString(volIconString, volFont, Brushes.White, volIcon.X, volIcon.Y);

                int speedResult = spacePress + 1;
                string speedText = $"SPACE BAR/CLICK ⏩ X{speedResult}";

                SizeF infoSize = g.MeasureString(speedText, infoFont);

                speedUpRect = new RectangleF((bottomBar.X + ((bottomBar.Width / 2) - (infoSize.Width / 2))), (bottomBar.Y + ((bottomBar.Height / 2) - (infoSize.Height / 2))), infoSize.Width, infoSize.Height);
                g.DrawString(speedText, infoFont, Brushes.White, speedUpRect);

                g.DrawEllipse(pens[2], volBarVer);             
            }
        }

        private void StopTimer(object sender, EventArgs e)
        {
            invalTimer?.Dispose();     
            audioFile?.Dispose();
            waveOut?.Stop();
            waveOut?.Dispose();
            Instance = null;
        }
    }
}
