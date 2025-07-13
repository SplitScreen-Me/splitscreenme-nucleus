using Nucleus.Coop.Controls;
using Nucleus.Gaming.Controls;
using System.Drawing;
using System.Windows.Forms;


namespace Nucleus.Coop
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		
		
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
		       
		
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.GameOptionMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gameNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.notesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detailsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.openHandlerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOrigExePathMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openContentFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteContentFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openUserProfConfigMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteUserProfConfigMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openUserProfSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteUserProfSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDocumentConfMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDocumentConfMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDocumentSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDocumentSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBackupFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBackupFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.useAPIIndexMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useGamepadButtonPressMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useGamepadAPIIndexForGuestsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.keepInstancesFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableProfilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableHandlerUpdateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.changeIconMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameAssetsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coverMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.screenshotsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.steamLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.SteamLangCb = new System.Windows.Forms.ToolStripComboBox();
            this.menuSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.removeGameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.socialLinksMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fAQMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.discordMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redditMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.splitCalculatorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.thirdPartyToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dS4WindowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hidHideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scpToolkitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HomeScreen = new DoubleBufferPanel();
            this.SetupPanel = new DoubleBufferPanel();
            this.GameListContainer = new DoubleBufferPanel();
            this.GameList = new Nucleus.Gaming.ControlListBox();
            this.InfoPanel = new DoubleBufferPanel();
            this.cover = new DoubleBufferPanel();
            this.coverFrame = new DoubleBufferPanel();
            this.btn_Play = new System.Windows.Forms.Button();
            this.btn_Prev = new System.Windows.Forms.Button();
            this.btn_Next = new System.Windows.Forms.Button();
            this.PlayTimePanel = new Nucleus.Coop.Controls.PlaytimePanel();
            this.ProfileButtonsPanel = new DoubleBufferPanel();
            this.ProfileButtonPanelLockPb = new System.Windows.Forms.Label();
            this.ProfileSettings_btn = new System.Windows.Forms.Button();
            this.ProfilesList_btn = new System.Windows.Forms.Button();
            this.SaveProfileSwitch = new Nucleus.Coop.Controls.CustomSwitch();
            this.Icons_Container = new BufferedFlowLayoutPanel();
            this.HandlerNotesContainer = new DoubleBufferPanel();
            this.ExpandHandlerNotes_btn = new System.Windows.Forms.PictureBox();
            this.HandlerNotes = new Nucleus.Gaming.Controls.TransparentRichTextBox();
            this.HandlerNoteTitle = new System.Windows.Forms.Label();
            this.WindowPanel = new DoubleBufferPanel();
            this.VirtualMouseToggle = new System.Windows.Forms.PictureBox();
            this.MainButtonsPanel = new DoubleBufferPanel();
            this.btn_downloadAssets = new System.Windows.Forms.Button();
            this.btn_debuglog = new System.Windows.Forms.Button();
            this.btn_Extract = new System.Windows.Forms.Button();
            this.Tutorial_btn = new System.Windows.Forms.Button();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.InputsTextLabel = new System.Windows.Forms.Label();
            this.donationBtn = new System.Windows.Forms.Button();
            this.closeBtn = new System.Windows.Forms.Button();
            this.btn_Links = new System.Windows.Forms.Button();
            this.maximizeBtn = new System.Windows.Forms.Button();
            this.minimizeBtn = new System.Windows.Forms.Button();
            this.txt_version = new System.Windows.Forms.Label();
            this.logo = new System.Windows.Forms.PictureBox();
            this.BigLogo = new System.Windows.Forms.PictureBox();
            this.GameOptionMenu.SuspendLayout();
            this.socialLinksMenu.SuspendLayout();
            this.HomeScreen.SuspendLayout();
            this.GameListContainer.SuspendLayout();
            this.InfoPanel.SuspendLayout();
            this.cover.SuspendLayout();
            this.coverFrame.SuspendLayout();
            this.ProfileButtonsPanel.SuspendLayout();
            this.HandlerNotesContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExpandHandlerNotes_btn)).BeginInit();
            this.WindowPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VirtualMouseToggle)).BeginInit();
            this.MainButtonsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BigLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // GameOptionMenu
            // 
            this.GameOptionMenu.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.GameOptionMenu.DropShadowEnabled = false;
            this.GameOptionMenu.ImageScalingSize = new System.Drawing.Size(15, 15);
            this.GameOptionMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameNameMenuItem,
            this.menuSeparator1,
            this.notesMenuItem,
            this.detailsMenuItem,
            this.toolStripSeparator7,
            this.openHandlerMenuItem,
            this.openOrigExePathMenuItem,
            this.openContentFolderMenuItem,
            this.deleteContentFolderMenuItem,
            this.menuSeparator2,
            this.openUserProfConfigMenuItem,
            this.deleteUserProfConfigMenuItem,
            this.openUserProfSaveMenuItem,
            this.deleteUserProfSaveMenuItem,
            this.openDocumentConfMenuItem,
            this.deleteDocumentConfMenuItem,
            this.openDocumentSaveMenuItem,
            this.deleteDocumentSaveMenuItem,
            this.openBackupFolderMenuItem,
            this.deleteBackupFolderMenuItem,
            this.toolStripSeparator3,
            this.useAPIIndexMenuItem,
            this.useGamepadButtonPressMenuItem,
            this.useGamepadAPIIndexForGuestsMenuItem,
            this.toolStripSeparator5,
            this.keepInstancesFolderMenuItem,
            this.disableProfilesMenuItem,
            this.disableHandlerUpdateMenuItem,
            this.toolStripSeparator4,
            this.changeIconMenuItem,
            this.gameAssetsMenuItem,
            this.steamLanguage,
            this.menuSeparator3,
            this.removeGameMenuItem});
            this.GameOptionMenu.Name = "gameContextMenuStrip";
            this.GameOptionMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.GameOptionMenu.Size = new System.Drawing.Size(306, 662);
            // 
            // gameNameMenuItem
            // 
            this.gameNameMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.gameNameMenuItem.Name = "gameNameMenuItem";
            this.gameNameMenuItem.Size = new System.Drawing.Size(305, 22);
            this.gameNameMenuItem.Text = "null";
            // 
            // menuSeparator1
            // 
            this.menuSeparator1.Name = "menuSeparator1";
            this.menuSeparator1.Size = new System.Drawing.Size(302, 6);
            // 
            // notesMenuItem
            // 
            this.notesMenuItem.Name = "notesMenuItem";
            this.notesMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.notesMenuItem.Size = new System.Drawing.Size(305, 22);
            this.notesMenuItem.Text = "Handler Author\'s Notes";
            this.notesMenuItem.Visible = false;
            // 
            // detailsMenuItem
            // 
            this.detailsMenuItem.Name = "detailsMenuItem";
            this.detailsMenuItem.Size = new System.Drawing.Size(305, 22);
            this.detailsMenuItem.Text = "Nucleus Game Details";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(302, 6);
            // 
            // openHandlerMenuItem
            // 
            this.openHandlerMenuItem.Name = "openHandlerMenuItem";
            this.openHandlerMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openHandlerMenuItem.Text = "Open Game Handler";
            // 
            // openOrigExePathMenuItem
            // 
            this.openOrigExePathMenuItem.Name = "openOrigExePathMenuItem";
            this.openOrigExePathMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openOrigExePathMenuItem.Text = "Open Original Exe Path";
            // 
            // openContentFolderMenuItem
            // 
            this.openContentFolderMenuItem.Name = "openContentFolderMenuItem";
            this.openContentFolderMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openContentFolderMenuItem.Text = "Open Nucleus Content Folder";
            // 
            // deleteContentFolderMenuItem
            // 
            this.deleteContentFolderMenuItem.Name = "deleteContentFolderMenuItem";
            this.deleteContentFolderMenuItem.Size = new System.Drawing.Size(305, 22);
            this.deleteContentFolderMenuItem.Text = "Delete Game Content Folder";
            // 
            // menuSeparator2
            // 
            this.menuSeparator2.Name = "menuSeparator2";
            this.menuSeparator2.Size = new System.Drawing.Size(302, 6);
            // 
            // openUserProfConfigMenuItem
            // 
            this.openUserProfConfigMenuItem.Name = "openUserProfConfigMenuItem";
            this.openUserProfConfigMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openUserProfConfigMenuItem.Text = "Open UserProfile Config Path";
            this.openUserProfConfigMenuItem.Visible = false;
            // 
            // deleteUserProfConfigMenuItem
            // 
            this.deleteUserProfConfigMenuItem.Name = "deleteUserProfConfigMenuItem";
            this.deleteUserProfConfigMenuItem.Size = new System.Drawing.Size(305, 22);
            this.deleteUserProfConfigMenuItem.Text = "Delete UserProfile Config Path";
            this.deleteUserProfConfigMenuItem.Visible = false;
            // 
            // openUserProfSaveMenuItem
            // 
            this.openUserProfSaveMenuItem.Name = "openUserProfSaveMenuItem";
            this.openUserProfSaveMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openUserProfSaveMenuItem.Text = "Open UserProfile Save Path";
            this.openUserProfSaveMenuItem.Visible = false;
            // 
            // deleteUserProfSaveMenuItem
            // 
            this.deleteUserProfSaveMenuItem.Name = "deleteUserProfSaveMenuItem";
            this.deleteUserProfSaveMenuItem.Size = new System.Drawing.Size(305, 22);
            this.deleteUserProfSaveMenuItem.Text = "Delete UserProfile Save Path";
            this.deleteUserProfSaveMenuItem.Visible = false;
            // 
            // openDocumentConfMenuItem
            // 
            this.openDocumentConfMenuItem.Name = "openDocumentConfMenuItem";
            this.openDocumentConfMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openDocumentConfMenuItem.Text = "Open Document Config Path";
            this.openDocumentConfMenuItem.Visible = false;
            // 
            // deleteDocumentConfMenuItem
            // 
            this.deleteDocumentConfMenuItem.Name = "deleteDocumentConfMenuItem";
            this.deleteDocumentConfMenuItem.Size = new System.Drawing.Size(305, 22);
            this.deleteDocumentConfMenuItem.Text = "Delete Document Config Path";
            this.deleteDocumentConfMenuItem.Visible = false;
            // 
            // openDocumentSaveMenuItem
            // 
            this.openDocumentSaveMenuItem.Name = "openDocumentSaveMenuItem";
            this.openDocumentSaveMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openDocumentSaveMenuItem.Text = "Open Document Save Path";
            this.openDocumentSaveMenuItem.Visible = false;
            // 
            // deleteDocumentSaveMenuItem
            // 
            this.deleteDocumentSaveMenuItem.Name = "deleteDocumentSaveMenuItem";
            this.deleteDocumentSaveMenuItem.Size = new System.Drawing.Size(305, 22);
            this.deleteDocumentSaveMenuItem.Text = "Delete Document Save Path";
            this.deleteDocumentSaveMenuItem.Visible = false;
            // 
            // openBackupFolderMenuItem
            // 
            this.openBackupFolderMenuItem.Name = "openBackupFolderMenuItem";
            this.openBackupFolderMenuItem.Size = new System.Drawing.Size(305, 22);
            this.openBackupFolderMenuItem.Text = "Open Backup Folder";
            // 
            // deleteBackupFolderMenuItem
            // 
            this.deleteBackupFolderMenuItem.Name = "deleteBackupFolderMenuItem";
            this.deleteBackupFolderMenuItem.Size = new System.Drawing.Size(305, 22);
            this.deleteBackupFolderMenuItem.Text = "Delete Backup Folder";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(302, 6);
            // 
            // useAPIIndexMenuItem
            // 
            this.useAPIIndexMenuItem.Name = "useAPIIndexMenuItem";
            this.useAPIIndexMenuItem.Size = new System.Drawing.Size(305, 22);
            this.useAPIIndexMenuItem.Text = "Assign Gamepads By Index";
            // 
            // useGamepadButtonPressMenuItem
            // 
            this.useGamepadButtonPressMenuItem.Name = "useGamepadButtonPressMenuItem";
            this.useGamepadButtonPressMenuItem.Size = new System.Drawing.Size(305, 22);
            this.useGamepadButtonPressMenuItem.Text = "Assign Gamepads On Button Press (profile)";
            // 
            // useGamepadAPIIndexForGuestsMenuItem
            // 
            this.useGamepadAPIIndexForGuestsMenuItem.Name = "useGamepadAPIIndexForGuestsMenuItem";
            this.useGamepadAPIIndexForGuestsMenuItem.Size = new System.Drawing.Size(305, 22);
            this.useGamepadAPIIndexForGuestsMenuItem.Text = "Assign Gamepads By Index For Guests Only ";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(302, 6);
            // 
            // keepInstancesFolderMenuItem
            // 
            this.keepInstancesFolderMenuItem.Name = "keepInstancesFolderMenuItem";
            this.keepInstancesFolderMenuItem.Size = new System.Drawing.Size(305, 22);
            this.keepInstancesFolderMenuItem.Text = "Keep Instances Content Folder";
            // 
            // disableProfilesMenuItem
            // 
            this.disableProfilesMenuItem.Name = "disableProfilesMenuItem";
            this.disableProfilesMenuItem.Size = new System.Drawing.Size(305, 22);
            this.disableProfilesMenuItem.Text = "Disable Profile";
            // 
            // disableHandlerUpdateMenuItem
            // 
            this.disableHandlerUpdateMenuItem.Name = "disableHandlerUpdateMenuItem";
            this.disableHandlerUpdateMenuItem.Size = new System.Drawing.Size(305, 22);
            this.disableHandlerUpdateMenuItem.Text = "Disable Handler Update";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(302, 6);
            // 
            // changeIconMenuItem
            // 
            this.changeIconMenuItem.Name = "changeIconMenuItem";
            this.changeIconMenuItem.Size = new System.Drawing.Size(305, 22);
            this.changeIconMenuItem.Text = "Change Game Icon";
            // 
            // gameAssetsMenuItem
            // 
            this.gameAssetsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.coverMenuItem,
            this.screenshotsMenuItem});
            this.gameAssetsMenuItem.Name = "gameAssetsMenuItem";
            this.gameAssetsMenuItem.Size = new System.Drawing.Size(305, 22);
            this.gameAssetsMenuItem.Text = "Game Assets";
            // 
            // coverMenuItem
            // 
            this.coverMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.coverMenuItem.Name = "coverMenuItem";
            this.coverMenuItem.Size = new System.Drawing.Size(205, 22);
            this.coverMenuItem.Text = "Open Cover Folder";
            // 
            // screenshotsMenuItem
            // 
            this.screenshotsMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.screenshotsMenuItem.Name = "screenshotsMenuItem";
            this.screenshotsMenuItem.Size = new System.Drawing.Size(205, 22);
            this.screenshotsMenuItem.Text = "Open Screenshots Folder";
            // 
            // steamLanguage
            // 
            this.steamLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SteamLangCb});
            this.steamLanguage.Name = "steamLanguage";
            this.steamLanguage.Size = new System.Drawing.Size(305, 22);
            this.steamLanguage.Text = "Steam Language";
            // 
            // SteamLangCb
            // 
            this.SteamLangCb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SteamLangCb.Items.AddRange(new object[] {
            "App Setting",
            "Arabic",
            "Brazilian",
            "Bulgarian",
            "Czech",
            "Danish",
            "Dutch",
            "English",
            "Finnish",
            "French",
            "German",
            "Greek",
            "Hungarian",
            "Italian",
            "Japanese",
            "Koreana",
            "Latam",
            "Norwegian",
            "Polish",
            "Portuguese",
            "Romanian",
            "Russian",
            "Schinese",
            "Spanish",
            "Swedish",
            "Tchinese",
            "Thai",
            "Turkish",
            "Ukrainian"});
            this.SteamLangCb.MaxDropDownItems = 10;
            this.SteamLangCb.Name = "SteamLangCb";
            this.SteamLangCb.Size = new System.Drawing.Size(121, 23);
            this.SteamLangCb.Sorted = true;
            // 
            // menuSeparator3
            // 
            this.menuSeparator3.Name = "menuSeparator3";
            this.menuSeparator3.Size = new System.Drawing.Size(302, 6);
            // 
            // removeGameMenuItem
            // 
            this.removeGameMenuItem.Name = "removeGameMenuItem";
            this.removeGameMenuItem.Size = new System.Drawing.Size(305, 22);
            this.removeGameMenuItem.Text = "Remove Game From List";
            // 
            // socialLinksMenu
            // 
            this.socialLinksMenu.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.socialLinksMenu.DropShadowEnabled = false;
            this.socialLinksMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fAQMenuItem,
            this.discordMenuItem,
            this.redditMenuItem,
            this.toolStripSeparator1,
            this.splitCalculatorMenuItem,
            this.toolStripSeparator2,
            this.thirdPartyToolsToolStripMenuItem});
            this.socialLinksMenu.Name = "socialLinksMenu";
            this.socialLinksMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.socialLinksMenu.ShowImageMargin = false;
            this.socialLinksMenu.Size = new System.Drawing.Size(137, 126);
            // 
            // fAQMenuItem
            // 
            this.fAQMenuItem.Name = "fAQMenuItem";
            this.fAQMenuItem.Size = new System.Drawing.Size(136, 22);
            this.fAQMenuItem.Text = "FAQ";
            // 
            // discordMenuItem
            // 
            this.discordMenuItem.Name = "discordMenuItem";
            this.discordMenuItem.Size = new System.Drawing.Size(136, 22);
            this.discordMenuItem.Text = "Discord";
            // 
            // redditMenuItem
            // 
            this.redditMenuItem.Name = "redditMenuItem";
            this.redditMenuItem.Size = new System.Drawing.Size(136, 22);
            this.redditMenuItem.Text = "Reddit";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(133, 6);
            // 
            // splitCalculatorMenuItem
            // 
            this.splitCalculatorMenuItem.Name = "splitCalculatorMenuItem";
            this.splitCalculatorMenuItem.Size = new System.Drawing.Size(136, 22);
            this.splitCalculatorMenuItem.Text = "SplitCalculator";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(133, 6);
            // 
            // thirdPartyToolsToolStripMenuItem
            // 
            this.thirdPartyToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xOutputToolStripMenuItem,
            this.dS4WindowsToolStripMenuItem,
            this.hidHideToolStripMenuItem,
            this.scpToolkitToolStripMenuItem});
            this.thirdPartyToolsToolStripMenuItem.Name = "thirdPartyToolsToolStripMenuItem";
            this.thirdPartyToolsToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.thirdPartyToolsToolStripMenuItem.Text = "Third Party Tools";
            // 
            // xOutputToolStripMenuItem
            // 
            this.xOutputToolStripMenuItem.Name = "xOutputToolStripMenuItem";
            this.xOutputToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.xOutputToolStripMenuItem.Text = "XOutput";
            // 
            // dS4WindowsToolStripMenuItem
            // 
            this.dS4WindowsToolStripMenuItem.Name = "dS4WindowsToolStripMenuItem";
            this.dS4WindowsToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.dS4WindowsToolStripMenuItem.Text = "DS4Windows";
            // 
            // hidHideToolStripMenuItem
            // 
            this.hidHideToolStripMenuItem.Name = "hidHideToolStripMenuItem";
            this.hidHideToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.hidHideToolStripMenuItem.Text = "HidHide";
            // 
            // scpToolkitToolStripMenuItem
            // 
            this.scpToolkitToolStripMenuItem.Name = "scpToolkitToolStripMenuItem";
            this.scpToolkitToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.scpToolkitToolStripMenuItem.Text = "ScpToolkit";
            // 
            // HomeScreen
            // 
            this.HomeScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HomeScreen.BackColor = System.Drawing.Color.Black;
            this.HomeScreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.HomeScreen.Controls.Add(this.SetupPanel);
            this.HomeScreen.Controls.Add(this.GameListContainer);
            this.HomeScreen.Controls.Add(this.InfoPanel);
            this.HomeScreen.Controls.Add(this.WindowPanel);
            this.HomeScreen.Controls.Add(this.BigLogo);
            this.HomeScreen.Location = new System.Drawing.Point(5, 4);
            this.HomeScreen.Margin = new System.Windows.Forms.Padding(0);
            this.HomeScreen.Name = "HomeScreen";
            this.HomeScreen.Size = new System.Drawing.Size(1166, 656);
            this.HomeScreen.TabIndex = 34;
            // 
            // SetupPanel
            // 
            this.SetupPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SetupPanel.AutoScroll = true;
            this.SetupPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SetupPanel.BackColor = System.Drawing.Color.Transparent;
            this.SetupPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SetupPanel.Cursor = System.Windows.Forms.Cursors.Default;
            this.SetupPanel.Location = new System.Drawing.Point(209, 58);
            this.SetupPanel.Margin = new System.Windows.Forms.Padding(0);
            this.SetupPanel.Name = "SetupPanel";
            this.SetupPanel.Size = new System.Drawing.Size(771, 598);
            this.SetupPanel.TabIndex = 0;
            this.SetupPanel.Visible = false;
            // 
            // GameListContainer
            // 
            this.GameListContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.GameListContainer.BackColor = System.Drawing.Color.Transparent;
            this.GameListContainer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.GameListContainer.Controls.Add(this.GameList);
            this.GameListContainer.Location = new System.Drawing.Point(0, 58);
            this.GameListContainer.Margin = new System.Windows.Forms.Padding(0);
            this.GameListContainer.Name = "GameListContainer";
            this.GameListContainer.Size = new System.Drawing.Size(209, 598);
            this.GameListContainer.TabIndex = 35;
            // 
            // GameList
            // 
            this.GameList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GameList.AutoScroll = true;
            this.GameList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GameList.BackColor = System.Drawing.Color.Transparent;
            this.GameList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.GameList.Border = 0;
            this.GameList.ContextMenuStrip = this.GameOptionMenu;
            this.GameList.Location = new System.Drawing.Point(0, 0);
            this.GameList.Margin = new System.Windows.Forms.Padding(0);
            this.GameList.Name = "GameList";
            this.GameList.Offset = new System.Drawing.Size(0, 0);
            this.GameList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GameList.Size = new System.Drawing.Size(230, 572);
            this.GameList.TabIndex = 2;
            // 
            // InfoPanel
            // 
            this.InfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoPanel.BackColor = System.Drawing.Color.Transparent;
            this.InfoPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.InfoPanel.Controls.Add(this.cover);
            this.InfoPanel.Controls.Add(this.btn_Prev);
            this.InfoPanel.Controls.Add(this.btn_Next);
            this.InfoPanel.Controls.Add(this.PlayTimePanel);
            this.InfoPanel.Controls.Add(this.ProfileButtonsPanel);
            this.InfoPanel.Controls.Add(this.Icons_Container);
            this.InfoPanel.Controls.Add(this.HandlerNotesContainer);
            this.InfoPanel.Location = new System.Drawing.Point(980, 58);
            this.InfoPanel.Margin = new System.Windows.Forms.Padding(0);
            this.InfoPanel.Name = "InfoPanel";
            this.InfoPanel.Size = new System.Drawing.Size(186, 598);
            this.InfoPanel.TabIndex = 34;
            this.InfoPanel.Visible = false;
            // 
            // cover
            // 
            this.cover.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cover.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cover.BackColor = System.Drawing.Color.Black;
            this.cover.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cover.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cover.Controls.Add(this.coverFrame);
            this.cover.Location = new System.Drawing.Point(23, 127);
            this.cover.Name = "cover";
            this.cover.Size = new System.Drawing.Size(140, 180);
            this.cover.TabIndex = 27;
            // 
            // coverFrame
            // 
            this.coverFrame.BackColor = System.Drawing.Color.Transparent;
            this.coverFrame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.coverFrame.Controls.Add(this.btn_Play);
            this.coverFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.coverFrame.Location = new System.Drawing.Point(0, 0);
            this.coverFrame.Margin = new System.Windows.Forms.Padding(0);
            this.coverFrame.Name = "coverFrame";
            this.coverFrame.Size = new System.Drawing.Size(138, 178);
            this.coverFrame.TabIndex = 26;
            // 
            // btn_Play
            // 
            this.btn_Play.BackColor = System.Drawing.Color.Transparent;
            this.btn_Play.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Play.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_Play.FlatAppearance.BorderSize = 0;
            this.btn_Play.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Play.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Play.ForeColor = System.Drawing.Color.Lime;
            this.btn_Play.Location = new System.Drawing.Point(17, 38);
            this.btn_Play.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Play.Name = "btn_Play";
            this.btn_Play.Size = new System.Drawing.Size(106, 106);
            this.btn_Play.TabIndex = 4;
            this.btn_Play.Tag = "START";
            this.btn_Play.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_Play.UseVisualStyleBackColor = false;
            // 
            // btn_Prev
            // 
            this.btn_Prev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Prev.BackColor = System.Drawing.Color.Transparent;
            this.btn_Prev.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Prev.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_Prev.FlatAppearance.BorderSize = 0;
            this.btn_Prev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Prev.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Prev.Location = new System.Drawing.Point(3, 128);
            this.btn_Prev.Margin = new System.Windows.Forms.Padding(0);
            this.btn_Prev.Name = "btn_Prev";
            this.btn_Prev.Size = new System.Drawing.Size(19, 180);
            this.btn_Prev.TabIndex = 9;
            this.btn_Prev.UseVisualStyleBackColor = false;
            // 
            // btn_Next
            // 
            this.btn_Next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Next.BackColor = System.Drawing.Color.Transparent;
            this.btn_Next.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Next.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_Next.FlatAppearance.BorderSize = 0;
            this.btn_Next.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Next.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Next.Location = new System.Drawing.Point(164, 128);
            this.btn_Next.Margin = new System.Windows.Forms.Padding(0);
            this.btn_Next.Name = "btn_Next";
            this.btn_Next.Size = new System.Drawing.Size(19, 180);
            this.btn_Next.TabIndex = 11;
            this.btn_Next.UseVisualStyleBackColor = false;
            // 
            // PlayTimePanel
            // 
            this.PlayTimePanel.AutoSize = true;
            this.PlayTimePanel.LastPlayed = null;
            this.PlayTimePanel.Location = new System.Drawing.Point(4, 41);
            this.PlayTimePanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PlayTimePanel.Name = "PlayTimePanel";
            this.PlayTimePanel.Playtime = null;
            this.PlayTimePanel.Size = new System.Drawing.Size(174, 28);
            this.PlayTimePanel.TabIndex = 104;
            // 
            // ProfileButtonsPanel
            // 
            this.ProfileButtonsPanel.Controls.Add(this.ProfileButtonPanelLockPb);
            this.ProfileButtonsPanel.Controls.Add(this.ProfileSettings_btn);
            this.ProfileButtonsPanel.Controls.Add(this.ProfilesList_btn);
            this.ProfileButtonsPanel.Controls.Add(this.SaveProfileSwitch);
            this.ProfileButtonsPanel.Location = new System.Drawing.Point(8, 86);
            this.ProfileButtonsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ProfileButtonsPanel.Name = "ProfileButtonsPanel";
            this.ProfileButtonsPanel.Size = new System.Drawing.Size(175, 24);
            this.ProfileButtonsPanel.TabIndex = 103;
            this.ProfileButtonsPanel.Visible = false;
            // 
            // ProfileButtonPanelLockPb
            // 
            this.ProfileButtonPanelLockPb.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ProfileButtonPanelLockPb.AutoSize = true;
            this.ProfileButtonPanelLockPb.Location = new System.Drawing.Point(162, 5);
            this.ProfileButtonPanelLockPb.Name = "ProfileButtonPanelLockPb";
            this.ProfileButtonPanelLockPb.Size = new System.Drawing.Size(0, 15);
            this.ProfileButtonPanelLockPb.TabIndex = 106;
            this.ProfileButtonPanelLockPb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProfileSettings_btn
            // 
            this.ProfileSettings_btn.BackColor = System.Drawing.Color.Transparent;
            this.ProfileSettings_btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ProfileSettings_btn.FlatAppearance.BorderSize = 0;
            this.ProfileSettings_btn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.ProfileSettings_btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ProfileSettings_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ProfileSettings_btn.ForeColor = System.Drawing.Color.White;
            this.ProfileSettings_btn.Location = new System.Drawing.Point(25, 1);
            this.ProfileSettings_btn.Margin = new System.Windows.Forms.Padding(2);
            this.ProfileSettings_btn.Name = "ProfileSettings_btn";
            this.ProfileSettings_btn.Size = new System.Drawing.Size(21, 21);
            this.ProfileSettings_btn.TabIndex = 1;
            this.ProfileSettings_btn.UseVisualStyleBackColor = false;
            // 
            // ProfilesList_btn
            // 
            this.ProfilesList_btn.BackColor = System.Drawing.Color.Transparent;
            this.ProfilesList_btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ProfilesList_btn.FlatAppearance.BorderSize = 0;
            this.ProfilesList_btn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.ProfilesList_btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ProfilesList_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ProfilesList_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProfilesList_btn.Location = new System.Drawing.Point(1, 1);
            this.ProfilesList_btn.Margin = new System.Windows.Forms.Padding(2);
            this.ProfilesList_btn.Name = "ProfilesList_btn";
            this.ProfilesList_btn.Size = new System.Drawing.Size(21, 21);
            this.ProfilesList_btn.TabIndex = 3;
            this.ProfilesList_btn.UseVisualStyleBackColor = false;
            // 
            // SaveProfileSwitch
            // 
            this.SaveProfileSwitch.AutoSize = true;
            this.SaveProfileSwitch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SaveProfileSwitch.BackColor = System.Drawing.Color.Transparent;
            this.SaveProfileSwitch.Location = new System.Drawing.Point(51, 4);
            this.SaveProfileSwitch.Margin = new System.Windows.Forms.Padding(1);
            this.SaveProfileSwitch.Name = "SaveProfileSwitch";
            this.SaveProfileSwitch.RadioBackColor = System.Drawing.Color.Transparent;
            this.SaveProfileSwitch.RadioChecked = true;
            this.SaveProfileSwitch.RadioText = "Save Profile";
            this.SaveProfileSwitch.RadioTooltipText = " If turned off the current setup will not be saved to a new profile.";
            this.SaveProfileSwitch.Size = new System.Drawing.Size(109, 16);
            this.SaveProfileSwitch.TabIndex = 105;
            this.SaveProfileSwitch.TextColor = System.Drawing.Color.White;
            // 
            // Icons_Container
            // 
            this.Icons_Container.AutoSize = true;
            this.Icons_Container.Location = new System.Drawing.Point(4, 6);
            this.Icons_Container.Name = "Icons_Container";
            this.Icons_Container.Size = new System.Drawing.Size(44, 19);
            this.Icons_Container.TabIndex = 32;
            // 
            // HandlerNotesContainer
            // 
            this.HandlerNotesContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HandlerNotesContainer.BackColor = System.Drawing.Color.Transparent;
            this.HandlerNotesContainer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.HandlerNotesContainer.Controls.Add(this.ExpandHandlerNotes_btn);
            this.HandlerNotesContainer.Controls.Add(this.HandlerNotes);
            this.HandlerNotesContainer.Controls.Add(this.HandlerNoteTitle);
            this.HandlerNotesContainer.Location = new System.Drawing.Point(8, 331);
            this.HandlerNotesContainer.Margin = new System.Windows.Forms.Padding(5);
            this.HandlerNotesContainer.Name = "HandlerNotesContainer";
            this.HandlerNotesContainer.Size = new System.Drawing.Size(171, 249);
            this.HandlerNotesContainer.TabIndex = 31;
            // 
            // ExpandHandlerNotes_btn
            // 
            this.ExpandHandlerNotes_btn.BackColor = System.Drawing.Color.Transparent;
            this.ExpandHandlerNotes_btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ExpandHandlerNotes_btn.Location = new System.Drawing.Point(151, 0);
            this.ExpandHandlerNotes_btn.Name = "ExpandHandlerNotes_btn";
            this.ExpandHandlerNotes_btn.Size = new System.Drawing.Size(20, 20);
            this.ExpandHandlerNotes_btn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ExpandHandlerNotes_btn.TabIndex = 43;
            this.ExpandHandlerNotes_btn.TabStop = false;
            // 
            // HandlerNotes
            // 
            this.HandlerNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HandlerNotes.BackColor = System.Drawing.Color.Black;
            this.HandlerNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.HandlerNotes.BulletIndent = 1;
            this.HandlerNotes.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.HandlerNotes.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.HandlerNotes.ForeColor = System.Drawing.Color.White;
            this.HandlerNotes.Location = new System.Drawing.Point(0, 22);
            this.HandlerNotes.Margin = new System.Windows.Forms.Padding(0);
            this.HandlerNotes.MinimumSize = new System.Drawing.Size(188, 192);
            this.HandlerNotes.Name = "HandlerNotes";
            this.HandlerNotes.ReadOnly = true;
            this.HandlerNotes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.HandlerNotes.Size = new System.Drawing.Size(188, 227);
            this.HandlerNotes.TabIndex = 13;
            this.HandlerNotes.Text = "";
            // 
            // HandlerNoteTitle
            // 
            this.HandlerNoteTitle.BackColor = System.Drawing.Color.Transparent;
            this.HandlerNoteTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.HandlerNoteTitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HandlerNoteTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HandlerNoteTitle.Location = new System.Drawing.Point(0, 0);
            this.HandlerNoteTitle.Name = "HandlerNoteTitle";
            this.HandlerNoteTitle.Size = new System.Drawing.Size(171, 20);
            this.HandlerNoteTitle.TabIndex = 33;
            this.HandlerNoteTitle.Text = "Handler Notes";
            this.HandlerNoteTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // WindowPanel
            // 
            this.WindowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WindowPanel.BackColor = System.Drawing.Color.Transparent;
            this.WindowPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.WindowPanel.Controls.Add(this.VirtualMouseToggle);
            this.WindowPanel.Controls.Add(this.MainButtonsPanel);
            this.WindowPanel.Controls.Add(this.InputsTextLabel);
            this.WindowPanel.Controls.Add(this.donationBtn);
            this.WindowPanel.Controls.Add(this.closeBtn);
            this.WindowPanel.Controls.Add(this.btn_Links);
            this.WindowPanel.Controls.Add(this.maximizeBtn);
            this.WindowPanel.Controls.Add(this.minimizeBtn);
            this.WindowPanel.Controls.Add(this.txt_version);
            this.WindowPanel.Controls.Add(this.logo);
            this.WindowPanel.Location = new System.Drawing.Point(0, 0);
            this.WindowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.WindowPanel.Name = "WindowPanel";
            this.WindowPanel.Size = new System.Drawing.Size(1166, 58);
            this.WindowPanel.TabIndex = 0;
            // 
            // VirtualMouseToggle
            // 
            this.VirtualMouseToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.VirtualMouseToggle.Location = new System.Drawing.Point(1008, 8);
            this.VirtualMouseToggle.Name = "VirtualMouseToggle";
            this.VirtualMouseToggle.Size = new System.Drawing.Size(27, 20);
            this.VirtualMouseToggle.TabIndex = 106;
            this.VirtualMouseToggle.TabStop = false;
            // 
            // MainButtonsPanel
            // 
            this.MainButtonsPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.MainButtonsPanel.Controls.Add(this.btn_downloadAssets);
            this.MainButtonsPanel.Controls.Add(this.btn_debuglog);
            this.MainButtonsPanel.Controls.Add(this.btn_Extract);
            this.MainButtonsPanel.Controls.Add(this.Tutorial_btn);
            this.MainButtonsPanel.Controls.Add(this.SettingsButton);
            this.MainButtonsPanel.Controls.Add(this.btnSearch);
            this.MainButtonsPanel.Location = new System.Drawing.Point(469, 4);
            this.MainButtonsPanel.Name = "MainButtonsPanel";
            this.MainButtonsPanel.Size = new System.Drawing.Size(209, 30);
            this.MainButtonsPanel.TabIndex = 105;
            this.MainButtonsPanel.Visible = false;
            // 
            // btn_downloadAssets
            // 
            this.btn_downloadAssets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_downloadAssets.BackColor = System.Drawing.Color.Transparent;
            this.btn_downloadAssets.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_downloadAssets.FlatAppearance.BorderSize = 0;
            this.btn_downloadAssets.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_downloadAssets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_downloadAssets.Location = new System.Drawing.Point(78, 4);
            this.btn_downloadAssets.Margin = new System.Windows.Forms.Padding(1);
            this.btn_downloadAssets.Name = "btn_downloadAssets";
            this.btn_downloadAssets.Size = new System.Drawing.Size(25, 25);
            this.btn_downloadAssets.TabIndex = 23;
            this.btn_downloadAssets.Text = " ";
            this.btn_downloadAssets.UseVisualStyleBackColor = false;
            // 
            // btn_debuglog
            // 
            this.btn_debuglog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_debuglog.BackgroundImage = global::Nucleus.Coop.Properties.Resources.log1;
            this.btn_debuglog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_debuglog.FlatAppearance.BorderSize = 0;
            this.btn_debuglog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_debuglog.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_debuglog.Location = new System.Drawing.Point(105, 4);
            this.btn_debuglog.Margin = new System.Windows.Forms.Padding(1);
            this.btn_debuglog.Name = "btn_debuglog";
            this.btn_debuglog.Size = new System.Drawing.Size(25, 25);
            this.btn_debuglog.TabIndex = 101;
            this.btn_debuglog.UseVisualStyleBackColor = false;
            // 
            // btn_Extract
            // 
            this.btn_Extract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Extract.BackColor = System.Drawing.Color.Transparent;
            this.btn_Extract.BackgroundImage = global::Nucleus.Coop.Properties.Resources.extract_nc;
            this.btn_Extract.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Extract.FlatAppearance.BorderSize = 0;
            this.btn_Extract.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Extract.ForeColor = System.Drawing.Color.White;
            this.btn_Extract.Location = new System.Drawing.Point(24, 4);
            this.btn_Extract.Margin = new System.Windows.Forms.Padding(1);
            this.btn_Extract.Name = "btn_Extract";
            this.btn_Extract.Size = new System.Drawing.Size(25, 25);
            this.btn_Extract.TabIndex = 100;
            this.btn_Extract.UseVisualStyleBackColor = false;
            // 
            // Tutorial_btn
            // 
            this.Tutorial_btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Tutorial_btn.BackColor = System.Drawing.Color.Transparent;
            this.Tutorial_btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Tutorial_btn.FlatAppearance.BorderSize = 0;
            this.Tutorial_btn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Tutorial_btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Tutorial_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Tutorial_btn.ForeColor = System.Drawing.Color.White;
            this.Tutorial_btn.Location = new System.Drawing.Point(159, 4);
            this.Tutorial_btn.Margin = new System.Windows.Forms.Padding(1);
            this.Tutorial_btn.Name = "Tutorial_btn";
            this.Tutorial_btn.Size = new System.Drawing.Size(25, 25);
            this.Tutorial_btn.TabIndex = 2;
            this.Tutorial_btn.UseVisualStyleBackColor = false;
            // 
            // SettingsButton
            // 
            this.SettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SettingsButton.BackColor = System.Drawing.Color.Transparent;
            this.SettingsButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SettingsButton.FlatAppearance.BorderSize = 0;
            this.SettingsButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.SettingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SettingsButton.Location = new System.Drawing.Point(132, 4);
            this.SettingsButton.Margin = new System.Windows.Forms.Padding(1);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(25, 25);
            this.SettingsButton.TabIndex = 16;
            this.SettingsButton.UseVisualStyleBackColor = false;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSearch.BackColor = System.Drawing.Color.Transparent;
            this.btnSearch.BackgroundImage = global::Nucleus.Coop.Properties.Resources.search_game;
            this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(51, 4);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(1);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(25, 25);
            this.btnSearch.TabIndex = 7;
            this.btnSearch.UseVisualStyleBackColor = false;
            // 
            // InputsTextLabel
            // 
            this.InputsTextLabel.AutoSize = true;
            this.InputsTextLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputsTextLabel.Location = new System.Drawing.Point(209, 35);
            this.InputsTextLabel.Name = "InputsTextLabel";
            this.InputsTextLabel.Size = new System.Drawing.Size(0, 15);
            this.InputsTextLabel.TabIndex = 104;
            this.InputsTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // donationBtn
            // 
            this.donationBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.donationBtn.BackColor = System.Drawing.Color.Transparent;
            this.donationBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.donationBtn.FlatAppearance.BorderSize = 0;
            this.donationBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.donationBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.donationBtn.Location = new System.Drawing.Point(1068, 6);
            this.donationBtn.Margin = new System.Windows.Forms.Padding(2);
            this.donationBtn.Name = "donationBtn";
            this.donationBtn.Size = new System.Drawing.Size(20, 20);
            this.donationBtn.TabIndex = 102;
            this.donationBtn.UseVisualStyleBackColor = false;
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.BackColor = System.Drawing.Color.Transparent;
            this.closeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(1140, 6);
            this.closeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(20, 20);
            this.closeBtn.TabIndex = 16;
            this.closeBtn.UseVisualStyleBackColor = false;
            // 
            // btn_Links
            // 
            this.btn_Links.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Links.BackColor = System.Drawing.Color.Transparent;
            this.btn_Links.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Links.FlatAppearance.BorderSize = 0;
            this.btn_Links.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_Links.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_Links.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Links.Location = new System.Drawing.Point(1040, 8);
            this.btn_Links.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Links.Name = "btn_Links";
            this.btn_Links.Size = new System.Drawing.Size(20, 20);
            this.btn_Links.TabIndex = 42;
            this.btn_Links.UseVisualStyleBackColor = false;
            // 
            // maximizeBtn
            // 
            this.maximizeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maximizeBtn.BackColor = System.Drawing.Color.Transparent;
            this.maximizeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.maximizeBtn.FlatAppearance.BorderSize = 0;
            this.maximizeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.maximizeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.maximizeBtn.Location = new System.Drawing.Point(1116, 6);
            this.maximizeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.maximizeBtn.Name = "maximizeBtn";
            this.maximizeBtn.Size = new System.Drawing.Size(20, 20);
            this.maximizeBtn.TabIndex = 16;
            this.maximizeBtn.UseVisualStyleBackColor = false;
            // 
            // minimizeBtn
            // 
            this.minimizeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minimizeBtn.BackColor = System.Drawing.Color.Transparent;
            this.minimizeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.minimizeBtn.FlatAppearance.BorderSize = 0;
            this.minimizeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.minimizeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minimizeBtn.Location = new System.Drawing.Point(1092, 6);
            this.minimizeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.minimizeBtn.Name = "minimizeBtn";
            this.minimizeBtn.Size = new System.Drawing.Size(20, 20);
            this.minimizeBtn.TabIndex = 16;
            this.minimizeBtn.UseVisualStyleBackColor = false;
            // 
            // txt_version
            // 
            this.txt_version.AutoSize = true;
            this.txt_version.BackColor = System.Drawing.Color.Transparent;
            this.txt_version.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.txt_version.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_version.ForeColor = System.Drawing.Color.White;
            this.txt_version.Location = new System.Drawing.Point(157, 23);
            this.txt_version.Margin = new System.Windows.Forms.Padding(0);
            this.txt_version.Name = "txt_version";
            this.txt_version.Size = new System.Drawing.Size(28, 13);
            this.txt_version.TabIndex = 35;
            this.txt_version.Text = "vxxx";
            this.txt_version.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // logo
            // 
            this.logo.BackColor = System.Drawing.Color.Transparent;
            this.logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.logo.Location = new System.Drawing.Point(10, 17);
            this.logo.Margin = new System.Windows.Forms.Padding(0);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(145, 25);
            this.logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.logo.TabIndex = 24;
            this.logo.TabStop = false;
            // 
            // BigLogo
            // 
            this.BigLogo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BigLogo.BackColor = System.Drawing.Color.Transparent;
            this.BigLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.BigLogo.Location = new System.Drawing.Point(420, 188);
            this.BigLogo.Margin = new System.Windows.Forms.Padding(0);
            this.BigLogo.Name = "BigLogo";
            this.BigLogo.Size = new System.Drawing.Size(532, 306);
            this.BigLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.BigLogo.TabIndex = 15;
            this.BigLogo.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1176, 664);
            this.ControlBox = false;
            this.Controls.Add(this.HomeScreen);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1176, 664);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Nucleus Co-op";
            this.GameOptionMenu.ResumeLayout(false);
            this.socialLinksMenu.ResumeLayout(false);
            this.HomeScreen.ResumeLayout(false);
            this.GameListContainer.ResumeLayout(false);
            this.InfoPanel.ResumeLayout(false);
            this.InfoPanel.PerformLayout();
            this.cover.ResumeLayout(false);
            this.coverFrame.ResumeLayout(false);
            this.ProfileButtonsPanel.ResumeLayout(false);
            this.ProfileButtonsPanel.PerformLayout();
            this.HandlerNotesContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ExpandHandlerNotes_btn)).EndInit();
            this.WindowPanel.ResumeLayout(false);
            this.WindowPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VirtualMouseToggle)).EndInit();
            this.MainButtonsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BigLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Gaming.ControlListBox GameList;
        private System.Windows.Forms.Button btn_Play;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btn_Prev;
        private System.Windows.Forms.Button btn_Next;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.ContextMenuStrip GameOptionMenu;
        private System.Windows.Forms.ToolStripSeparator menuSeparator1;
        private System.Windows.Forms.ToolStripSeparator menuSeparator3;
		private System.Windows.Forms.PictureBox BigLogo;
        private Button minimizeBtn;
        private Button maximizeBtn;
        private PictureBox logo;
        private DoubleBufferPanel HandlerNotesContainer;
        private Label txt_version;
        private Button btn_Extract;
        public DoubleBufferPanel HomeScreen;
        private Button btn_Links;
        private Label HandlerNoteTitle;
        private PictureBox ExpandHandlerNotes_btn;
        public BufferedFlowLayoutPanel Icons_Container;
        private Button closeBtn;
        public Button SettingsButton;
        public DoubleBufferPanel coverFrame;
        public DoubleBufferPanel cover;
        public DoubleBufferPanel SetupPanel;
        public DoubleBufferPanel GameListContainer;
        public DoubleBufferPanel WindowPanel;
        public Button btn_downloadAssets;
        public TransparentRichTextBox HandlerNotes;
        public Button btn_debuglog;
        private Button donationBtn;
        private ContextMenuStrip socialLinksMenu;
        private ToolStripMenuItem fAQMenuItem;
        private ToolStripMenuItem redditMenuItem;
        private ToolStripMenuItem discordMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem splitCalculatorMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem thirdPartyToolsToolStripMenuItem;
        private ToolStripMenuItem xOutputToolStripMenuItem;
        private ToolStripMenuItem dS4WindowsToolStripMenuItem;
        private ToolStripMenuItem hidHideToolStripMenuItem;
        private ToolStripMenuItem scpToolkitToolStripMenuItem;
        private DoubleBufferPanel ProfileButtonsPanel;
        private Button Tutorial_btn;
        private Button ProfileSettings_btn;
        private Button ProfilesList_btn;
        private CustomSwitch SaveProfileSwitch;
        private Label InputsTextLabel;
        private ToolStripMenuItem coverMenuItem;
        private ToolStripMenuItem screenshotsMenuItem;
        private DoubleBufferPanel MainButtonsPanel;
        public DoubleBufferPanel InfoPanel;
        private ToolStripComboBox SteamLangCb;
        private Controls.PlaytimePanel PlayTimePanel;
        public ToolStripMenuItem gameNameMenuItem;
        public ToolStripMenuItem detailsMenuItem;
        public ToolStripMenuItem openHandlerMenuItem;
        public ToolStripMenuItem openContentFolderMenuItem;
        public ToolStripMenuItem notesMenuItem;
        public ToolStripMenuItem openOrigExePathMenuItem;
        public ToolStripMenuItem deleteContentFolderMenuItem;
        public ToolStripMenuItem openUserProfConfigMenuItem;
        public ToolStripMenuItem deleteUserProfConfigMenuItem;
        public ToolStripMenuItem openUserProfSaveMenuItem;
        public ToolStripMenuItem deleteUserProfSaveMenuItem;
        public ToolStripMenuItem openDocumentConfMenuItem;
        public ToolStripMenuItem removeGameMenuItem;
        public ToolStripMenuItem changeIconMenuItem;
        public ToolStripMenuItem deleteDocumentConfMenuItem;
        public ToolStripMenuItem openDocumentSaveMenuItem;
        public ToolStripMenuItem deleteDocumentSaveMenuItem;
        public ToolStripMenuItem keepInstancesFolderMenuItem;
        public ToolStripMenuItem disableProfilesMenuItem;
        public ToolStripMenuItem openBackupFolderMenuItem;
        public ToolStripMenuItem deleteBackupFolderMenuItem;
        public ToolStripMenuItem gameAssetsMenuItem;
        public ToolStripMenuItem disableHandlerUpdateMenuItem;
        public ToolStripMenuItem steamLanguage;
        public PictureBox VirtualMouseToggle;
        private Label ProfileButtonPanelLockPb;
        public ToolStripMenuItem useAPIIndexMenuItem;
        private ToolStripMenuItem useGamepadAPIIndexForGuestsMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripSeparator menuSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator4;
        public ToolStripMenuItem useGamepadButtonPressMenuItem;
    }
}