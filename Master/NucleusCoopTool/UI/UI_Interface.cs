
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming;
using System;
using Nucleus.Coop.Controls;
using Nucleus.Coop.Forms;
using System.Windows.Forms;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Cache;
using Nucleus.Coop.Tools;
using System.Drawing;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Windows;
using System.Threading;

namespace Nucleus.Coop.UI
{
    public static class UI_Interface
    {
        public static Settings Settings = null;
        public static ProfileSettings ProfileSettings = null;
        public static XInputShortcutsSetup Xinput_S_Setup;

        public static bool WindowRoundedCorners => bool.Parse(Globals.ThemeConfigFile.IniReadValue("Misc", "UseroundedCorners"));

        public static Rectangle OsdBounds;
        
        public static Point DefCoverLoc;

        public static bool ShowFavoriteOnly = App_Misc.ShowFavoriteOnly;
        public static bool DisableForcedNote = App_Misc.DisableForcedNote;
        public static bool NoGamesPresent;
        public static bool IsFormClosing;
        public static bool RestartRequired = false;

        private static MainForm mainForm;
        public static MainForm MainForm
        {
            get => mainForm;
            set 
            {              
                mainForm = value;

                Core_Interface.SyncContext = SynchronizationContext.Current ?? new SynchronizationContext();

                mainForm.Font = new Font(Theme_Settings.CustomFont, Theme_Settings.FontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                mainForm.ForeColor = Theme_Settings.ControlsForeColor;

                mainForm.Cursor = Theme_Settings.Default_Cursor;

                mainForm.StartPosition = FormStartPosition.Manual;
                MainWindowFunc.SetWindowSizeAndLoc(mainForm);

                if (WindowRoundedCorners)
                {
                    FormGraphicsUtil.CreateRoundedControlRegion(mainForm, 0, 0, mainForm.Width, mainForm.Height, 20, 20);
                }

                mainForm.ClientSizeChanged += MainWindowFunc.MainForm_ClientSizeChanged;
                mainForm.ResizeBegin += MainWindowFunc.MainForm_ResizeBegin;
                mainForm.ResizeEnd += MainWindowFunc.MainForm_ResizeEnd;
                mainForm.FormClosing += MainWindowFunc.MainForm_FormClosing;
                mainForm.FormClosed += MainWindowFunc.MainForm_Closed;              
            }
        }

        private static DoubleBufferPanel homeScreen;
        public static DoubleBufferPanel HomeScreen
        { 
            get => homeScreen;
            set 
            {
                homeScreen = value;

                if (WindowRoundedCorners)
                {
                    FormGraphicsUtil.CreateRoundedControlRegion(HomeScreen, 0, 0, HomeScreen.Width, HomeScreen.Height, 20, 20);
                }

                var notesZoom = Runtime_Controls.BuildHandlerNotesZoom();
                HandlerNotesZoom = notesZoom;
                homeScreen.Controls.Add(notesZoom);

                homeScreen.AllowDrop = true;
                homeScreen.DragEnter += UI_Functions.ExtrtactHandlerDragEnter;
                homeScreen.DragDrop += UI_Functions.ExtrtactHandlerDragDrop;
                homeScreen.Paint += UI_Graphics.HomeScreen_Paint;
            }
        }

        private static SortOptionsPanel sortOptionsPanel;
        public static SortOptionsPanel SortOptionsPanel
        {
            get => sortOptionsPanel;
            set
            {
                sortOptionsPanel = value;          
                homeScreen.Controls.Add(sortOptionsPanel);       
            }
        }

        private static ControlListBox gameList;
        public static ControlListBox GameList
        {
            get => gameList;
            set 
            {
                gameList = value;
                gameList.SelectedChanged += UI_Functions.GameList_SelectedChanged;
            } 
        }

        private static Button settingsButton;
        public static Button SettingsButton
        {
            get => settingsButton;
            set
            {
                settingsButton = value;
                settingsButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_settings.png");
                CustomToolTips.SetToolTip(settingsButton, "Global Nucleus Co-op settings.", "btn_settings", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                settingsButton.Click += UI_Functions.SettingsButton_Click;
                settingsButton.MouseEnter += UI_Functions.SettingsButton_MouseEnter;
                settingsButton.MouseLeave += UI_Functions.SettingsButton_MouseLeave;
            }
        }

        private static DoubleBufferPanel gameListContainer;
        public static DoubleBufferPanel GameListContainer
        { 
            get => gameListContainer; 
            set
            {
                gameListContainer = value;
                gameListContainer.BackColor = Theme_Settings.GameListBackColor;
                gameListContainer.Paint += UI_Graphics.GameListContainer_Paint;       
            }
        }

        private static DoubleBufferPanel infoPanel;
        public static DoubleBufferPanel InfoPanel
        { 
            get=> infoPanel;
            set
            {
                infoPanel = value;
                infoPanel.Paint += UI_Graphics.InfoPanel_Paint;
                infoPanel.BackColor = Theme_Settings.InfoPanelBackColor;
            }
        }

        private static DoubleBufferPanel windowPanel;
        public static DoubleBufferPanel WindowPanel 
        {
            get => windowPanel;
            set 
            {
                windowPanel = value;

                PictureBox mainButtonsPanelButton = Runtime_Controls.BuildMainButtonsPanelButton();
                MainButtonsPanelButton = mainButtonsPanelButton;

                windowPanel.Controls.Add(mainButtonsPanelButton);
                windowPanel.MouseDown += UI_Functions.WindowPanelMouseDown;
                windowPanel.Paint += UI_Graphics.WindowPanel_Paint;
                windowPanel.BackColor = Theme_Settings.WindowPanelBackColor;
            }
        }

        public static PictureBox MainButtonsPanelButton { get; set; }

        private static Button maximizeButton;
        public static Button MaximizeButton
        {
            get => maximizeButton;
            set
            {
                maximizeButton = value;
                maximizeButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_maximize.png");
                maximizeButton.Click += UI_Functions.MaximizeButton_Click;
                maximizeButton.MouseEnter += UI_Functions.MaximizeButton_MouseEnter;
                maximizeButton.MouseLeave += UI_Functions.MaximizeButton_MouseLeave;
            }
        }

        private static Button minimizeButton;
        public static Button MinimizeButton
        {
            get => minimizeButton;
            set
            {
                minimizeButton = value;
                minimizeButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_minimize.png");
                minimizeButton.Click += UI_Functions.MinimizeButton_Click;
                minimizeButton.MouseEnter += UI_Functions.MinimizeButton_MouseEnter;
                minimizeButton.MouseLeave += UI_Functions.MinimizeButton_MouseLeave;
            }
        }

        private static Button closeButton;
        public static Button CloseButton
        {
            get => closeButton;
            set
            {
                closeButton = value;
                closeButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
                closeButton.Click += UI_Functions.CloseButton_Click;
                closeButton.MouseEnter += UI_Functions.CloseButton_MouseEnter;
                closeButton.MouseLeave += UI_Functions.CloseButton_MouseLeave;
            }
        }

        private static PictureBox toggleVirtualMouse;
        public static PictureBox ToggleVirtualMouse
        {
            get => toggleVirtualMouse;
            set
            {
                toggleVirtualMouse = value;
                toggleVirtualMouse.Paint += UI_Graphics.VirtualMouseToggle_Paint;
                toggleVirtualMouse.MouseClick += UI_Functions.VirtualMouseToggle_MouseClick;
                CustomToolTips.SetToolTip(toggleVirtualMouse, "Left click to turn On/Off gamepad emulated mouse.\nRight click to open gamepad shortcuts settings. ", "toggleVirtualMouse", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                GamepadNavigation.OnUpdateState += ToggleVirtualMouse.Invalidate;
            }
        }

        private static Button donationButton;
        public static Button DonationButton
        {
            get => donationButton;
            set
            {
                donationButton = value;
                donationButton.BackColor = Color.Transparent;
                donationButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
                donationButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "donation.png");
                CustomToolTips.SetToolTip(donationButton, "Nucleus Co-op Patreon.", "donationBtn", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                donationButton.Click += UI_Functions.DonationButton_Click;

            }
        }

        private static DoubleBufferPanel mainButtonsPanel;
        public static DoubleBufferPanel MainButtonsPanel
        {
            get => mainButtonsPanel;
            set 
            {
                mainButtonsPanel = value;
                mainButtonsPanel.Visible = true;
            }      
        }

        private static Button openLogButton;
        public static Button OpenLogButton
        {
            get => openLogButton;
            set
            {
                openLogButton = value;
                openLogButton.BackColor = Color.Transparent;
                openLogButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
                openLogButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "log.png");
                CustomToolTips.SetToolTip(openLogButton, "Open Nucleus debug-log.txt file if available, debug log can be disabled in Nucleus settings in the \"Settings\" tab.", "btn_debuglog", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                openLogButton.Click += UI_Functions.OpenLogButton_Click;
                openLogButton.MouseEnter += UI_Functions.OpenLogButton_MouseEnter;
                openLogButton.MouseLeave += UI_Functions.OpenLogButton_MouseLeave;
            }
        }
 
        private static Button searchGameButton;
        public static Button SearchGameButton
        {
            get => searchGameButton;
            set
            {
                searchGameButton = value;
                searchGameButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
                searchGameButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "search_game.png");
                CustomToolTips.SetToolTip(searchGameButton, "Search and add a game to the game list (its handler must be installed).", "btnSearch", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                searchGameButton.Click += UI_Functions.SearchGameButton_Click;
                searchGameButton.MouseEnter += UI_Functions.SearchGameButton_MouseEnter;
                searchGameButton.MouseLeave += UI_Functions.SearchGameButton_MouseLeave;
            }
        }

        private static Button extractHandlerButton;
        public static Button ExtractHandlerButton
        {
            get => extractHandlerButton;
            set
            {
                extractHandlerButton = value;
                extractHandlerButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
                extractHandlerButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "extract_nc.png");
                CustomToolTips.SetToolTip(extractHandlerButton, "Extract a handler from a \".nc\" archive.", "btn_Extract", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                extractHandlerButton.Click += UI_Functions.ExtractHandlerButton_Click;
                extractHandlerButton.MouseEnter += UI_Functions.ExtractHandlerButton_MouseEnter;
                extractHandlerButton.MouseLeave += UI_Functions.ExtractHandlerButton_MouseLeave;
            }
        }

        private static Button socialMenuButton;
        public static Button SocialMenuButton
        {
            get => socialMenuButton;
            set
            {
                socialMenuButton = value;
                socialMenuButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_dropdown_closed.png");
                socialMenuButton.Click += UI_Functions.SocialMenuButton_Click;
            }
        }

        private static ContextMenuStrip socialMenu;
        public static ContextMenuStrip SocialMenu
        {
            get => socialMenu;
            set
            {
                socialMenu = value;
                socialMenu.Renderer = new CustomToolStripRenderer.MyRenderer();
                socialMenu.BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
                socialMenu.Cursor = Theme_Settings.Hand_Cursor;
                socialMenu.BackColor = Theme_Settings.GameOptionMenuBackColor;
                socialMenu.ForeColor = Theme_Settings.GameOptionMenuForeColor;

                socialMenu.Opening += SocialMenuFunc.SocialMenu_Opening;
                socialMenu.Opened += SocialMenuFunc.SocialMenu_Opened;
                socialMenu.Closing += SocialMenuFunc.SocialMenu_Closing;
            }
        }

        private static Button dwldAssetsButton;
        public static Button DwldAssetsButton
        {
            get => dwldAssetsButton;
            set
            {
                dwldAssetsButton = value;
                dwldAssetsButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_download_assets.png");
                CustomToolTips.SetToolTip(dwldAssetsButton, "Download or update games covers and screenshots.", "btn_downloadAssets", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                dwldAssetsButton.Click += UI_Functions.DwldAssetsButton_Click;
                dwldAssetsButton.MouseEnter += UI_Functions.DwldAssetsButton_MouseEnter;
                dwldAssetsButton.MouseLeave += UI_Functions.DwldAssetsButton_MouseLeave;
            }
        }

        private static PlaytimePanel playTimePanel;
        public static PlaytimePanel PlayTimePanel
        { 
            get => playTimePanel;
            set
            {
                playTimePanel = value;
                playTimePanel.BackColor = Color.Transparent; 
            } 
        }

        private static DoubleBufferPanel cover;
        public static DoubleBufferPanel Cover 
        { 
            get => cover;
            set
            {
                cover = value;
                bool coverBorderOff = bool.Parse(Globals.ThemeConfigFile.IniReadValue("Misc", "DisableCoverBorder"));

                if (coverBorderOff)
                {
                    cover.BorderStyle = BorderStyle.None;
                }
            }             
        }

        private static DoubleBufferPanel coverFrame;
        public static DoubleBufferPanel CoverFrame
        { 
            get => coverFrame; 
            set
            {
                coverFrame = value;
                coverFrame.Paint += UI_Graphics.CoverFrame_Paint;
                coverFrame.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "cover_layer.png");
            }
        }

        private static MultiColorLabel inputsTextLabel;
        public static MultiColorLabel InputsTextLabel
        { 
            get => inputsTextLabel;
            set
            {
                inputsTextLabel = value;
            }
        }


        private static PictureBox logo;
        public static PictureBox Logo
        {
            get => logo;
            set
            {
                logo = value;
                logo.Image = ImageCache.GetImage(Globals.ThemeFolder + "title_logo.png");
                logo.Cursor = Theme_Settings.Hand_Cursor;
                logo.Click += UI_Functions.Logo_Click;
            }
        }

        private static PictureBox bigLogo;
        public static PictureBox BigLogo
        {
            get => bigLogo;
            set
            {
                bigLogo = value;
                bigLogo.Paint += UI_Graphics.BigLogo_Paint;
                bigLogo.Image = ImageCache.GetImage(Globals.ThemeFolder + "logo.png");
            }
        }

        private static PictureBox expandHandlerNotesButton;
        public static PictureBox ExpandHandlerNotesButton
        {
            get => expandHandlerNotesButton;
            set
            {
                expandHandlerNotesButton = value;
                expandHandlerNotesButton.Cursor = Theme_Settings.Hand_Cursor;
                expandHandlerNotesButton.Image = ImageCache.GetImage(Globals.ThemeFolder + "expand_Notes.png");
                CustomToolTips.SetToolTip(expandHandlerNotesButton, "Expand handler notes.", "btn_expandNotes", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                expandHandlerNotesButton.Click += UI_Functions.ExpandHandlerNotesButton_Click;
            }
        }

        private static BufferedFlowLayoutPanel iconsContainer;
        public static BufferedFlowLayoutPanel IconsContainer
        {
            get => iconsContainer;
            set
            {
                iconsContainer = value;
                iconsContainer.BackColor = Color.FromArgb(InfoPanel.BackColor.A - InfoPanel.BackColor.A, InfoPanel.BackColor.R, InfoPanel.BackColor.G, InfoPanel.BackColor.B);
            }
        }

        private static DoubleBufferPanel setupPanel;
        public static DoubleBufferPanel SetupPanel
        { 
            get => setupPanel;
            set
            {
                setupPanel = value;
                setupPanel.BackColor = Theme_Settings.SetupScreenBackColor;
                setupPanel.Paint += UI_Graphics.SetupPanel_Paint;
            }
        }

        public static DoubleBufferPanel StepButtonsPanel { get; set; }

        private static ContextMenuStrip gameOptionMenu;
        public static ContextMenuStrip GameOptionMenu 
        { 
            get=> gameOptionMenu;
            set
            {
                gameOptionMenu = value;
                gameOptionMenu.Renderer = new CustomToolStripRenderer.MyRenderer();
                gameOptionMenu.BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
                gameOptionMenu.Cursor = Theme_Settings.Hand_Cursor;
                gameOptionMenu.BackColor = Theme_Settings.GameOptionMenuBackColor;
                gameOptionMenu.ForeColor = Theme_Settings.GameOptionMenuForeColor;

                gameOptionMenu.Opening += GameOptionMenuFunc.GameOptionMenu_Opening;
                gameOptionMenu.Opened += GameOptionMenuFunc.GameOptionMenu_Opened;
                gameOptionMenu.Closing += GameOptionMenuFunc.GameOptionMenu_Closing;
            }
        }

        public static HubWebView WebView { get; set; }

        private static SetupScreenControl setupScreen;
        public static SetupScreenControl SetupScreen
        { 
            get=> setupScreen;
            set
            {
                setupScreen = value;
                setupScreen.Paint += UI_Graphics.SetupScreen_Paint;
                setupScreen.Click += Generic_Functions.ClickAnyControl;
                setupScreen.OnCanPlayUpdated += Core_Interface.StepCanPlay;
                setupScreen.MouseDown += UI_Functions.OnClickHideProfileList;
                ProfilesList  profilelist  = new ProfilesList(setupScreen);
                setupScreen.Controls.Add(profilelist);
                ProfilesList = profilelist;
                profilelist.BringToFront();
            }
        }

        public static ProfilesList ProfilesList;

        public static Label HandlerNoteTitle { get; set; }

        private static DoubleBufferPanel handlerNotesContainer;
        public static DoubleBufferPanel HandlerNotesContainer
        { 
            get => handlerNotesContainer; 
            set 
            {
                handlerNotesContainer = value;

                bool noteBorderOff = bool.Parse(Globals.ThemeConfigFile.IniReadValue("Misc", "DisableNoteBorder"));

                if (noteBorderOff)
                {
                   handlerNotesContainer.BorderStyle = BorderStyle.None;
                }

                handlerNotesContainer.BackColor = Theme_Settings.HandlerNoteBackColor;
            }
        }

        private static TransparentRichTextBox handlerNotes;
        public static TransparentRichTextBox HandlerNotes
        {
            get => handlerNotes;
            set
            {
                handlerNotes = value;
                handlerNotes.ForeColor = Theme_Settings.HandlerNoteForeColor;
                handlerNotes.LinkClicked += UI_Functions.HandlerNotes_LinkClicked;
                handlerNotes.TextChanged += UI_Functions.HandlerNotes_TextChanged;
            }
        }

        private static CustomSwitch saveProfileSwitch;
        public static CustomSwitch SaveProfileSwitch
        {
            get => saveProfileSwitch;
            set
            {
                saveProfileSwitch = value;
                saveProfileSwitch.Visible = false;
                saveProfileSwitch.TickCursor = Theme_Settings.Hand_Cursor;
                saveProfileSwitch.Click += UI_Functions.SaveProfileSwitch_Click;
            }       
        }

        public static GameProfile Current_GameProfile { get; set; }

        private static Button playButton;
        public static Button PlayButton
        {
            get => playButton;
            set 
            {
                playButton = value;
                playButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "play.png");
                playButton.BackColor = Color.Transparent;
                playButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
                playButton.Cursor = Theme_Settings.Hand_Cursor;
                playButton.MouseEnter += Generic_Functions.Btn_ZoomIn;
                playButton.MouseLeave += Generic_Functions.Btn_ZoomOut;
                playButton.Click += UI_Functions.Play_Click;
                GameProfile.OnReadyAutoPlayTrigger += Core_Interface.PlayClicked;
            }
        }

        public static Tutorial Tutorial { get; set; }

        private static Button tutorialButton;
        public static Button TutorialButton
        { 
            get => tutorialButton;
            set
            {
                tutorialButton = value;
                tutorialButton.MouseEnter += UI_Functions.TutorialButton_MouseEnter;
                tutorialButton.MouseLeave += UI_Functions.TutorialButton_MouseLeave;
                tutorialButton.Click += UI_Functions.TutorialButton_Click;
                tutorialButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "instruction_closed.png");
                CustomToolTips.SetToolTip(tutorialButton, "How to setup players.", "instruction_btn", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            }
        }

        private static Button gotoPrev;
        public static Button GotoPrev
        {
            get => gotoPrev;
            set
            {
                gotoPrev = value;             
                gotoPrev.FlatAppearance.MouseOverBackColor = Color.Transparent;
                gotoPrev.FlatAppearance.MouseDownBackColor = Color.Transparent;
                gotoPrev.Tag= "BACK";
                gotoPrev.BackColor = Color.Transparent;
                gotoPrev.Click += UI_Functions.GotoPrev_Click;
                gotoPrev.EnabledChanged += UI_Functions.GotoPrevEnabledChanged;
                gotoPrev.Paint += UI_Graphics.GotoPrevPaint;
            }
        }

        private static Button gotoNext;
        public static Button GotoNext
        { 
            get => gotoNext;
            set
            {
                gotoNext = value;
                gotoNext.FlatAppearance.MouseOverBackColor = Color.Transparent;
                gotoNext.FlatAppearance.MouseDownBackColor = Color.Transparent;
                gotoNext.Tag = "NEXT";
                gotoNext.BackColor = Color.Transparent;

                gotoNext.Click += UI_Functions.GotoNext_Click;
                gotoNext.EnabledChanged += UI_Functions.GotoNextEnabledChanged;
                gotoNext.Paint += UI_Graphics.GotoNextPaint;
            }
        }

        private static HubButton hubButton;
        public static HubButton HubButton
        {
            get => hubButton;
            set
            {            
                hubButton = value;
            }
        }

        private static SearchTextBox searchTextBox;
        public static SearchTextBox SearchTextBox
        {
            get => searchTextBox;
            set
            {
                searchTextBox = value;
                searchTextBox.SearchText.Font = new Font(Theme_Settings.CustomFont, 13f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
                searchTextBox.SearchText.TextChanged += Core_Interface.RefreshGames;
                searchTextBox.Visible = false;
            }
        }
       
        private static SortGamesButton sortGamesButton;
        public static SortGamesButton SortGamesButton
        {
            get => sortGamesButton;
            set
            {
                sortGamesButton = value;
                sortGamesButton.Visible = false;
            }
        }

        private static ToggleFavoriteButton toggleFavoriteButton;
        public static ToggleFavoriteButton ToggleFavoriteButton
        {
            get => toggleFavoriteButton;
            set
            {
                toggleFavoriteButton = value;
                toggleFavoriteButton.Visible = false;
            }
        }
      
        private static Button profileSettingsButton;
        public static Button ProfileSettingsButton
        {
            get => profileSettingsButton;
            set
            {
                profileSettingsButton = value;
                profileSettingsButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "profile_settings.png");
                profileSettingsButton.Click += UI_Functions.ProfileSettingsButton_Click;
                profileSettingsButton.Tag = profileSettingsButton.Location;
            }
        }
       
        private static Button profileListButton;
        public static Button ProfileListButton 
        { 
            get => profileListButton;
            set 
            {
                profileListButton = value;
                profileListButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "profiles_list.png");
                profileListButton.Click += UI_Functions.ProfileListButton_Click;
                profileListButton.VisibleChanged += UI_Functions.ProfileListButton_VisibleChanged;                
            } 
        }

        public static GameControl CurrentGameListControl { get; set; }

        private static DoubleBufferPanel profileButtonsPanel;
        public static DoubleBufferPanel ProfileButtonsPanel
        {
            get => profileButtonsPanel; 
            set
            {
                profileButtonsPanel = value;
                profileButtonsPanel.BackColor = Color.Transparent;
                profileButtonsPanel.EnabledChanged += UI_Functions.ProfileButtonsPanel_StateChanged;
                profileButtonsPanel.VisibleChanged += UI_Functions.ProfileButtonsPanel_StateChanged;
                foreach (Control b in profileButtonsPanel.Controls)
                {
                    if (!(b is CustomSwitch ))
                    {
                        b.MouseEnter += Generic_Functions.Btn_ZoomIn;
                        b.MouseLeave += Generic_Functions.Btn_ZoomOut;
                    }     
                }
            }
        }

        private static Label profileButtonPanelLockPb;
        public static Label ProfileButtonPanelLockPb
        {
            get => profileButtonPanelLockPb;
            set
            {
                profileButtonPanelLockPb = value;
                profileButtonPanelLockPb.Font = new Font(Theme_Settings.CustomFont, 16F,FontStyle.Bold);
                profileButtonPanelLockPb.Text = "";            
            }
        }
      
        private static HandlerNotesZoom handlerNotesZoom;
        public static HandlerNotesZoom HandlerNotesZoom 
        {
            get => handlerNotesZoom;
            set
            {
                handlerNotesZoom = value;
            }
        }

        private static Label versionTxt;
        public static Label VersionTxt
        {
            get => versionTxt;
            set
            {
                versionTxt = value;
#if DEBUG
                versionTxt.ForeColor = Color.LightSteelBlue;
                versionTxt.Text = "DEBUG " + "v" + Globals.Version;
#else
             
                if (bool.Parse(Globals.ThemeConfigFile.IniReadValue("Misc", "HideVersion")) == false)
                {
                    versionTxt.Text = "v" + Globals.Version;
                    versionTxt.ForeColor = MainForm.ForeColor;
                    //versionTxt.ForeColor = Color.Gold;
                }
                else
                {
                    versionTxt.Text = "";
                }
#endif
            }
        }

        public static float UIScalingRatio = 1;
        public static float ScalingRatio = 1;
        public static float TextScalingRatio = 1;

        public static bool Closing { get; set; }
    }
}
