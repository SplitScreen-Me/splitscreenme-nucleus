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
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.btn_noHub = new System.Windows.Forms.Button();
            this.btn_reddit = new System.Windows.Forms.Button();
            this.btn_Discord = new System.Windows.Forms.Button();
            this.btn_SplitCalculator = new System.Windows.Forms.Button();
            this.btn_thirdPartytools = new System.Windows.Forms.Button();
            this.btn_faq = new System.Windows.Forms.Button();
            this.btn_downloadAssets = new System.Windows.Forms.Button();
            this.btn_settings = new System.Windows.Forms.Button();
            this.logo = new System.Windows.Forms.PictureBox();
            this.btn_dlFromHub = new System.Windows.Forms.Button();
            this.gameContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.detailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDataFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOrigExePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteUserProfileConfigPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openUserProfileSavePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteUserProfileSavePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDocumentConfigPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDocumentConfigPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDocumentSavePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDocumentSavePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.changeIconToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteContentFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.third_party_tools_container = new System.Windows.Forms.Panel();
            this.txt_version = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.minimizeBtn = new System.Windows.Forms.Button();
            this.maximizeBtn = new System.Windows.Forms.Button();
            this.splash = new System.Windows.Forms.PictureBox();
            this.clientAreaPanel = new BufferedClientAreaPanel();
            this.stepPanelPictureBox = new System.Windows.Forms.PictureBox();
            this.StepPanel = new BufferedClientAreaPanel();
            this.glowingLine1 = new System.Windows.Forms.PictureBox();
            this.glowingLine0 = new System.Windows.Forms.PictureBox();
            this.game_listSizer = new BufferedClientAreaPanel();
            this.list_Games = new Nucleus.Gaming.ControlListBox();
            this.rightFrame = new BufferedClientAreaPanel();
            this.buttonContainer = new BufferedClientAreaPanel();
            this.btn_GameDesc = new System.Windows.Forms.Button();
            this.btn_gameOptions = new System.Windows.Forms.Button();
            this.btn_scriptAuthorTxt = new System.Windows.Forms.Button();
            this.button_UpdateAvailable = new System.Windows.Forms.Button();
            this.scriptAuthorTxtSizer = new BufferedClientAreaPanel();
            this.scriptAuthorTxt = new System.Windows.Forms.TextBox();
            this.cover = new BufferedClientAreaPanel();
            this.coverFrame = new BufferedClientAreaPanel();
            this.txt_GameDescSizer = new BufferedClientAreaPanel();
            this.txt_GameDesc = new System.Windows.Forms.TextBox();
            this.mainButtonFrame = new BufferedClientAreaPanel();
            this.btn_Download = new System.Windows.Forms.Button();
            this.btn_Extract = new System.Windows.Forms.Button();
            this.label_StepTitle = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btn_Play = new System.Windows.Forms.Button();
            this.btn_Next = new System.Windows.Forms.Button();
            this.btnAutoSearch = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            this.gameContextMenuStrip.SuspendLayout();
            this.third_party_tools_container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splash)).BeginInit();
            this.clientAreaPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stepPanelPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.glowingLine1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.glowingLine0)).BeginInit();
            this.game_listSizer.SuspendLayout();
            this.rightFrame.SuspendLayout();
            this.buttonContainer.SuspendLayout();
            this.scriptAuthorTxtSizer.SuspendLayout();
            this.cover.SuspendLayout();
            this.txt_GameDescSizer.SuspendLayout();
            this.mainButtonFrame.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkLabel4
            // 
            this.linkLabel4.ActiveLinkColor = System.Drawing.Color.Aqua;
            this.linkLabel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkLabel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel4.LinkColor = System.Drawing.Color.DeepSkyBlue;
            this.linkLabel4.Location = new System.Drawing.Point(0, 45);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(93, 15);
            this.linkLabel4.TabIndex = 3;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "ScpToolkit ";
            this.linkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.linkLabel4, "Xinput emulator for Ps3 controllers.");
            this.linkLabel4.VisitedLinkColor = System.Drawing.Color.Silver;
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.ActiveLinkColor = System.Drawing.Color.Aqua;
            this.linkLabel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel3.LinkColor = System.Drawing.Color.DeepSkyBlue;
            this.linkLabel3.Location = new System.Drawing.Point(0, 30);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(93, 15);
            this.linkLabel3.TabIndex = 2;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "HidHide";
            this.linkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.linkLabel3, "With HidHide it is possible to deny a specific application access to one or more " +
        "human interface devices.");
            this.linkLabel3.VisitedLinkColor = System.Drawing.Color.Silver;
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.ActiveLinkColor = System.Drawing.Color.Aqua;
            this.linkLabel2.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel2.LinkColor = System.Drawing.Color.DeepSkyBlue;
            this.linkLabel2.Location = new System.Drawing.Point(0, 15);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(93, 15);
            this.linkLabel2.TabIndex = 1;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "DS4Windows";
            this.linkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.linkLabel2, "Xinput emulator for Ps4 controllers.");
            this.linkLabel2.VisitedLinkColor = System.Drawing.Color.Silver;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.Aqua;
            this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.LinkColor = System.Drawing.Color.DeepSkyBlue;
            this.linkLabel1.Location = new System.Drawing.Point(0, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(93, 15);
            this.linkLabel1.TabIndex = 0;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "XOutput";
            this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.linkLabel1, "XOutput is a software that can convert DirectInput into XInput. ");
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.Silver;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // btn_noHub
            // 
            this.btn_noHub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_noHub.BackColor = System.Drawing.Color.Transparent;
            this.btn_noHub.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_noHub.BackgroundImage")));
            this.btn_noHub.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_noHub.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_noHub.FlatAppearance.BorderSize = 0;
            this.btn_noHub.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_noHub.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_noHub.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_noHub.ForeColor = System.Drawing.Color.Transparent;
            this.btn_noHub.Location = new System.Drawing.Point(752, 4);
            this.btn_noHub.Name = "btn_noHub";
            this.btn_noHub.Size = new System.Drawing.Size(26, 25);
            this.btn_noHub.TabIndex = 41;
            this.btn_noHub.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btn_noHub, resources.GetString("btn_noHub.ToolTip"));
            this.btn_noHub.UseVisualStyleBackColor = false;
            this.btn_noHub.Visible = false;
            this.btn_noHub.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btn_reddit
            // 
            this.btn_reddit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_reddit.BackColor = System.Drawing.Color.Transparent;
            this.btn_reddit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_reddit.BackgroundImage")));
            this.btn_reddit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_reddit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_reddit.FlatAppearance.BorderSize = 0;
            this.btn_reddit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_reddit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_reddit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_reddit.ForeColor = System.Drawing.Color.White;
            this.btn_reddit.Location = new System.Drawing.Point(814, 5);
            this.btn_reddit.Name = "btn_reddit";
            this.btn_reddit.Size = new System.Drawing.Size(25, 22);
            this.btn_reddit.TabIndex = 40;
            this.btn_reddit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btn_reddit, "Official Nucleus Co-op Subreddit.");
            this.btn_reddit.UseVisualStyleBackColor = false;
            this.btn_reddit.Click += new System.EventHandler(this.button2_Click);
            // 
            // btn_Discord
            // 
            this.btn_Discord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Discord.BackColor = System.Drawing.Color.Transparent;
            this.btn_Discord.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_Discord.BackgroundImage")));
            this.btn_Discord.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Discord.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Discord.FlatAppearance.BorderSize = 0;
            this.btn_Discord.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_Discord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Discord.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Discord.ForeColor = System.Drawing.Color.White;
            this.btn_Discord.Location = new System.Drawing.Point(845, 6);
            this.btn_Discord.Name = "btn_Discord";
            this.btn_Discord.Size = new System.Drawing.Size(25, 20);
            this.btn_Discord.TabIndex = 39;
            this.btn_Discord.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btn_Discord, "Join the official Nucleus Co-op discord server.");
            this.btn_Discord.UseVisualStyleBackColor = false;
            this.btn_Discord.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_SplitCalculator
            // 
            this.btn_SplitCalculator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_SplitCalculator.BackColor = System.Drawing.Color.Transparent;
            this.btn_SplitCalculator.BackgroundImage = global::Nucleus.Coop.Properties.Resources.SplitCalculator;
            this.btn_SplitCalculator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_SplitCalculator.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_SplitCalculator.FlatAppearance.BorderSize = 0;
            this.btn_SplitCalculator.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_SplitCalculator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SplitCalculator.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SplitCalculator.ForeColor = System.Drawing.Color.White;
            this.btn_SplitCalculator.Location = new System.Drawing.Point(876, 6);
            this.btn_SplitCalculator.Name = "btn_SplitCalculator";
            this.btn_SplitCalculator.Size = new System.Drawing.Size(20, 20);
            this.btn_SplitCalculator.TabIndex = 38;
            this.btn_SplitCalculator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btn_SplitCalculator, "This program can estimate the system requirements needed\r\nto run a game in split-" +
        "screen.");
            this.btn_SplitCalculator.UseVisualStyleBackColor = false;
            this.btn_SplitCalculator.Click += new System.EventHandler(this.btn_SplitCalculator_Click);
            // 
            // btn_thirdPartytools
            // 
            this.btn_thirdPartytools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_thirdPartytools.BackColor = System.Drawing.Color.Transparent;
            this.btn_thirdPartytools.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_thirdPartytools.BackgroundImage")));
            this.btn_thirdPartytools.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_thirdPartytools.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_thirdPartytools.FlatAppearance.BorderSize = 0;
            this.btn_thirdPartytools.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_thirdPartytools.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_thirdPartytools.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_thirdPartytools.ForeColor = System.Drawing.Color.White;
            this.btn_thirdPartytools.Location = new System.Drawing.Point(902, 7);
            this.btn_thirdPartytools.Name = "btn_thirdPartytools";
            this.btn_thirdPartytools.Size = new System.Drawing.Size(20, 20);
            this.btn_thirdPartytools.TabIndex = 37;
            this.btn_thirdPartytools.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btn_thirdPartytools, "Third party tools useful to make non xinput controllers work with Nucleus Co-op.");
            this.btn_thirdPartytools.UseVisualStyleBackColor = false;
            this.btn_thirdPartytools.Click += new System.EventHandler(this.btn_thirdPartytools_Click);
            // 
            // btn_faq
            // 
            this.btn_faq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_faq.BackColor = System.Drawing.Color.Transparent;
            this.btn_faq.BackgroundImage = global::Nucleus.Coop.Properties.Resources.faq;
            this.btn_faq.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_faq.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_faq.FlatAppearance.BorderSize = 0;
            this.btn_faq.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_faq.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_faq.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_faq.ForeColor = System.Drawing.Color.White;
            this.btn_faq.Location = new System.Drawing.Point(783, 4);
            this.btn_faq.Name = "btn_faq";
            this.btn_faq.Size = new System.Drawing.Size(25, 23);
            this.btn_faq.TabIndex = 36;
            this.btn_faq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btn_faq, "Nucleus Co-op FAQ.");
            this.btn_faq.UseVisualStyleBackColor = false;
            this.btn_faq.Click += new System.EventHandler(this.link_faq_Click);
            // 
            // btn_downloadAssets
            // 
            this.btn_downloadAssets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_downloadAssets.BackColor = System.Drawing.Color.Transparent;
            this.btn_downloadAssets.BackgroundImage = global::Nucleus.Coop.Properties.Resources.DownloadAssets_button;
            this.btn_downloadAssets.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_downloadAssets.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_downloadAssets.FlatAppearance.BorderSize = 0;
            this.btn_downloadAssets.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_downloadAssets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_downloadAssets.Location = new System.Drawing.Point(928, 6);
            this.btn_downloadAssets.Name = "btn_downloadAssets";
            this.btn_downloadAssets.Size = new System.Drawing.Size(20, 20);
            this.btn_downloadAssets.TabIndex = 23;
            this.btn_downloadAssets.Text = " ";
            this.toolTip1.SetToolTip(this.btn_downloadAssets, "Download or update games covers and screenshots.");
            this.btn_downloadAssets.UseVisualStyleBackColor = false;
            this.btn_downloadAssets.Click += new System.EventHandler(this.btn_downloadAssets_Click);
            // 
            // btn_settings
            // 
            this.btn_settings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_settings.BackColor = System.Drawing.Color.Transparent;
            this.btn_settings.BackgroundImage = global::Nucleus.Coop.Properties.Resources.settings;
            this.btn_settings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_settings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_settings.FlatAppearance.BorderSize = 0;
            this.btn_settings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_settings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_settings.Location = new System.Drawing.Point(953, 6);
            this.btn_settings.Margin = new System.Windows.Forms.Padding(2);
            this.btn_settings.Name = "btn_settings";
            this.btn_settings.Size = new System.Drawing.Size(20, 20);
            this.btn_settings.TabIndex = 16;
            this.toolTip1.SetToolTip(this.btn_settings, "Settings");
            this.btn_settings.UseVisualStyleBackColor = false;
            this.btn_settings.Click += new System.EventHandler(this.SettingsBtn_Click);
            // 
            // logo
            // 
            this.logo.BackColor = System.Drawing.Color.Transparent;
            this.logo.BackgroundImage = global::Nucleus.Coop.Properties.Resources.logo;
            this.logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.logo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.logo.Location = new System.Drawing.Point(9, 9);
            this.logo.Margin = new System.Windows.Forms.Padding(0);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(150, 26);
            this.logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.logo.TabIndex = 24;
            this.logo.TabStop = false;
            this.toolTip1.SetToolTip(this.logo, "Nucleus Co-op Github release page.");
            this.logo.Click += new System.EventHandler(this.logo_Click);
            // 
            // btn_dlFromHub
            // 
            this.btn_dlFromHub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_dlFromHub.BackColor = System.Drawing.Color.Transparent;
            this.btn_dlFromHub.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_dlFromHub.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_dlFromHub.FlatAppearance.BorderSize = 0;
            this.btn_dlFromHub.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_dlFromHub.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_dlFromHub.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_dlFromHub.ForeColor = System.Drawing.Color.Transparent;
            this.btn_dlFromHub.Location = new System.Drawing.Point(355, 7);
            this.btn_dlFromHub.Margin = new System.Windows.Forms.Padding(0);
            this.btn_dlFromHub.Name = "btn_dlFromHub";
            this.btn_dlFromHub.Size = new System.Drawing.Size(17, 15);
            this.btn_dlFromHub.TabIndex = 42;
            this.btn_dlFromHub.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btn_dlFromHub, "Download handlers (.nc) directly from the handlers hub, use the extract handler o" +
        "ption to install them.");
            this.btn_dlFromHub.UseVisualStyleBackColor = false;
            this.btn_dlFromHub.Click += new System.EventHandler(this.button1_Click_2);
            // 
            // gameContextMenuStrip
            // 
            this.gameContextMenuStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.gameContextMenuStrip.Font = new System.Drawing.Font("Franklin Gothic Medium", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.gameContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nullToolStripMenuItem,
            this.scriptNotesToolStripMenuItem,
            this.toolStripMenuItem1,
            this.detailsToolStripMenuItem,
            this.openScriptToolStripMenuItem,
            this.openDataFolderToolStripMenuItem,
            this.openOrigExePathToolStripMenuItem,
            this.toolStripMenuItem2,
            this.openToolStripMenuItem,
            this.deleteUserProfileConfigPathToolStripMenuItem,
            this.openUserProfileSavePathToolStripMenuItem,
            this.deleteUserProfileSavePathToolStripMenuItem,
            this.openDocumentConfigPathToolStripMenuItem,
            this.deleteDocumentConfigPathToolStripMenuItem,
            this.openDocumentSavePathToolStripMenuItem,
            this.deleteDocumentSavePathToolStripMenuItem,
            this.toolStripMenuItem3,
            this.changeIconToolStripMenuItem,
            this.deleteContentFolderToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.gameContextMenuStrip.Name = "gameContextMenuStrip";
            this.gameContextMenuStrip.ShowImageMargin = false;
            this.gameContextMenuStrip.Size = new System.Drawing.Size(204, 396);
            this.gameContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.GameContextMenuStrip_Opening);
            // 
            // nullToolStripMenuItem
            // 
            this.nullToolStripMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.nullToolStripMenuItem.Name = "nullToolStripMenuItem";
            this.nullToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.nullToolStripMenuItem.Text = "null";
            // 
            // scriptNotesToolStripMenuItem
            // 
            this.scriptNotesToolStripMenuItem.Name = "scriptNotesToolStripMenuItem";
            this.scriptNotesToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.scriptNotesToolStripMenuItem.Text = "Handler Author\'s Notes";
            this.scriptNotesToolStripMenuItem.Visible = false;
            this.scriptNotesToolStripMenuItem.Click += new System.EventHandler(this.ScriptNotesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(200, 6);
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // detailsToolStripMenuItem
            // 
            this.detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
            this.detailsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.detailsToolStripMenuItem.Text = "Nucleus Game Details";
            this.detailsToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
            // 
            // openScriptToolStripMenuItem
            // 
            this.openScriptToolStripMenuItem.Name = "openScriptToolStripMenuItem";
            this.openScriptToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openScriptToolStripMenuItem.Text = "Open Game Handler";
            this.openScriptToolStripMenuItem.Click += new System.EventHandler(this.OpenScriptToolStripMenuItem_Click);
            // 
            // openDataFolderToolStripMenuItem
            // 
            this.openDataFolderToolStripMenuItem.Name = "openDataFolderToolStripMenuItem";
            this.openDataFolderToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openDataFolderToolStripMenuItem.Text = "Open Nucleus Content Folder";
            this.openDataFolderToolStripMenuItem.Click += new System.EventHandler(this.OpenDataFolderToolStripMenuItem_Click);
            // 
            // openOrigExePathToolStripMenuItem
            // 
            this.openOrigExePathToolStripMenuItem.Name = "openOrigExePathToolStripMenuItem";
            this.openOrigExePathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openOrigExePathToolStripMenuItem.Text = "Open Original Exe Path";
            this.openOrigExePathToolStripMenuItem.Click += new System.EventHandler(this.OpenOrigExePathToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(200, 6);
            this.toolStripMenuItem2.Visible = false;
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openToolStripMenuItem.Text = "Open UserProfile Config Path";
            this.openToolStripMenuItem.Visible = false;
            // 
            // deleteUserProfileConfigPathToolStripMenuItem
            // 
            this.deleteUserProfileConfigPathToolStripMenuItem.Name = "deleteUserProfileConfigPathToolStripMenuItem";
            this.deleteUserProfileConfigPathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.deleteUserProfileConfigPathToolStripMenuItem.Text = "Delete UserProfile Config Path";
            this.deleteUserProfileConfigPathToolStripMenuItem.Visible = false;
            // 
            // openUserProfileSavePathToolStripMenuItem
            // 
            this.openUserProfileSavePathToolStripMenuItem.Name = "openUserProfileSavePathToolStripMenuItem";
            this.openUserProfileSavePathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openUserProfileSavePathToolStripMenuItem.Text = "Open UserProfile Save Path";
            this.openUserProfileSavePathToolStripMenuItem.Visible = false;
            // 
            // deleteUserProfileSavePathToolStripMenuItem
            // 
            this.deleteUserProfileSavePathToolStripMenuItem.Name = "deleteUserProfileSavePathToolStripMenuItem";
            this.deleteUserProfileSavePathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.deleteUserProfileSavePathToolStripMenuItem.Text = "Delete UserProfile Save Path";
            this.deleteUserProfileSavePathToolStripMenuItem.Visible = false;
            // 
            // openDocumentConfigPathToolStripMenuItem
            // 
            this.openDocumentConfigPathToolStripMenuItem.Name = "openDocumentConfigPathToolStripMenuItem";
            this.openDocumentConfigPathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openDocumentConfigPathToolStripMenuItem.Text = "Open Document Config Path";
            this.openDocumentConfigPathToolStripMenuItem.Visible = false;
            // 
            // deleteDocumentConfigPathToolStripMenuItem
            // 
            this.deleteDocumentConfigPathToolStripMenuItem.Name = "deleteDocumentConfigPathToolStripMenuItem";
            this.deleteDocumentConfigPathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.deleteDocumentConfigPathToolStripMenuItem.Text = "Delete Document Config Path";
            this.deleteDocumentConfigPathToolStripMenuItem.Visible = false;
            // 
            // openDocumentSavePathToolStripMenuItem
            // 
            this.openDocumentSavePathToolStripMenuItem.Name = "openDocumentSavePathToolStripMenuItem";
            this.openDocumentSavePathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openDocumentSavePathToolStripMenuItem.Text = "Open Document Save Path";
            this.openDocumentSavePathToolStripMenuItem.Visible = false;
            // 
            // deleteDocumentSavePathToolStripMenuItem
            // 
            this.deleteDocumentSavePathToolStripMenuItem.Name = "deleteDocumentSavePathToolStripMenuItem";
            this.deleteDocumentSavePathToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.deleteDocumentSavePathToolStripMenuItem.Text = "Delete Document Save Path";
            this.deleteDocumentSavePathToolStripMenuItem.Visible = false;
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(200, 6);
            // 
            // changeIconToolStripMenuItem
            // 
            this.changeIconToolStripMenuItem.Name = "changeIconToolStripMenuItem";
            this.changeIconToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.changeIconToolStripMenuItem.Text = "Change Game Icon";
            this.changeIconToolStripMenuItem.Click += new System.EventHandler(this.ChangeIconToolStripMenuItem_Click);
            // 
            // deleteContentFolderToolStripMenuItem
            // 
            this.deleteContentFolderToolStripMenuItem.Name = "deleteContentFolderToolStripMenuItem";
            this.deleteContentFolderToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.deleteContentFolderToolStripMenuItem.Text = "Delete Nucleus Content Folder";
            this.deleteContentFolderToolStripMenuItem.Click += new System.EventHandler(this.deleteContentFolderToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.deleteToolStripMenuItem.Text = "Remove Game from List";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // third_party_tools_container
            // 
            this.third_party_tools_container.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.third_party_tools_container.BackColor = System.Drawing.Color.Black;
            this.third_party_tools_container.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.third_party_tools_container.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.third_party_tools_container.Controls.Add(this.linkLabel4);
            this.third_party_tools_container.Controls.Add(this.linkLabel3);
            this.third_party_tools_container.Controls.Add(this.linkLabel2);
            this.third_party_tools_container.Controls.Add(this.linkLabel1);
            this.third_party_tools_container.Cursor = System.Windows.Forms.Cursors.Hand;
            this.third_party_tools_container.Location = new System.Drawing.Point(902, 33);
            this.third_party_tools_container.Name = "third_party_tools_container";
            this.third_party_tools_container.Size = new System.Drawing.Size(95, 69);
            this.third_party_tools_container.TabIndex = 0;
            this.third_party_tools_container.Visible = false;
            // 
            // txt_version
            // 
            this.txt_version.AutoSize = true;
            this.txt_version.BackColor = System.Drawing.Color.Transparent;
            this.txt_version.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.txt_version.Font = new System.Drawing.Font("Constantia", 9.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.txt_version.ForeColor = System.Drawing.Color.White;
            this.txt_version.Location = new System.Drawing.Point(159, 15);
            this.txt_version.Margin = new System.Windows.Forms.Padding(0);
            this.txt_version.Name = "txt_version";
            this.txt_version.Size = new System.Drawing.Size(32, 15);
            this.txt_version.TabIndex = 35;
            this.txt_version.Text = "v2.0";
            this.txt_version.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.BackColor = System.Drawing.Color.Transparent;
            this.closeBtn.BackgroundImage = global::Nucleus.Coop.Properties.Resources.close_button;
            this.closeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.closeBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(1025, 6);
            this.closeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(20, 20);
            this.closeBtn.TabIndex = 16;
            this.closeBtn.UseVisualStyleBackColor = false;
            // 
            // minimizeBtn
            // 
            this.minimizeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minimizeBtn.BackColor = System.Drawing.Color.Transparent;
            this.minimizeBtn.BackgroundImage = global::Nucleus.Coop.Properties.Resources.minimize_button;
            this.minimizeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.minimizeBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.minimizeBtn.FlatAppearance.BorderSize = 0;
            this.minimizeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.minimizeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minimizeBtn.Location = new System.Drawing.Point(977, 6);
            this.minimizeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.minimizeBtn.Name = "minimizeBtn";
            this.minimizeBtn.Size = new System.Drawing.Size(20, 20);
            this.minimizeBtn.TabIndex = 16;
            this.minimizeBtn.UseVisualStyleBackColor = false;
            // 
            // maximizeBtn
            // 
            this.maximizeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maximizeBtn.BackColor = System.Drawing.Color.Transparent;
            this.maximizeBtn.BackgroundImage = global::Nucleus.Coop.Properties.Resources.maximize_button;
            this.maximizeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.maximizeBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.maximizeBtn.FlatAppearance.BorderSize = 0;
            this.maximizeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.maximizeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.maximizeBtn.Location = new System.Drawing.Point(1001, 6);
            this.maximizeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.maximizeBtn.Name = "maximizeBtn";
            this.maximizeBtn.Size = new System.Drawing.Size(20, 20);
            this.maximizeBtn.TabIndex = 16;
            this.maximizeBtn.UseVisualStyleBackColor = false;
            // 
            // splash
            // 
            this.splash.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splash.BackColor = System.Drawing.Color.Black;
            this.splash.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splash.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.splash.Location = new System.Drawing.Point(0, 0);
            this.splash.Margin = new System.Windows.Forms.Padding(0);
            this.splash.Name = "splash";
            this.splash.Size = new System.Drawing.Size(1050, 602);
            this.splash.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.splash.TabIndex = 1;
            this.splash.TabStop = false;
            // 
            // clientAreaPanel
            // 
            this.clientAreaPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clientAreaPanel.BackColor = System.Drawing.Color.Black;
            this.clientAreaPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.clientAreaPanel.Controls.Add(this.stepPanelPictureBox);
            this.clientAreaPanel.Controls.Add(this.StepPanel);
            this.clientAreaPanel.Controls.Add(this.glowingLine1);
            this.clientAreaPanel.Controls.Add(this.glowingLine0);
            this.clientAreaPanel.Controls.Add(this.game_listSizer);
            this.clientAreaPanel.Controls.Add(this.rightFrame);
            this.clientAreaPanel.Controls.Add(this.mainButtonFrame);
            this.clientAreaPanel.Location = new System.Drawing.Point(0, 46);
            this.clientAreaPanel.Margin = new System.Windows.Forms.Padding(0);
            this.clientAreaPanel.Name = "clientAreaPanel";
            this.clientAreaPanel.Size = new System.Drawing.Size(1050, 557);
            this.clientAreaPanel.TabIndex = 34;
            // 
            // stepPanelPictureBox
            // 
            this.stepPanelPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stepPanelPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.stepPanelPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.stepPanelPictureBox.Location = new System.Drawing.Point(353, 140);
            this.stepPanelPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.stepPanelPictureBox.Name = "stepPanelPictureBox";
            this.stepPanelPictureBox.Size = new System.Drawing.Size(550, 303);
            this.stepPanelPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.stepPanelPictureBox.TabIndex = 15;
            this.stepPanelPictureBox.TabStop = false;
            // 
            // StepPanel
            // 
            this.StepPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StepPanel.AutoScroll = true;
            this.StepPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StepPanel.BackColor = System.Drawing.Color.Transparent;
            this.StepPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.StepPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.StepPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StepPanel.Location = new System.Drawing.Point(211, 38);
            this.StepPanel.Margin = new System.Windows.Forms.Padding(0);
            this.StepPanel.Name = "StepPanel";
            this.StepPanel.Size = new System.Drawing.Size(628, 510);
            this.StepPanel.TabIndex = 0;
            this.StepPanel.Visible = false;
            // 
            // glowingLine1
            // 
            this.glowingLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glowingLine1.BackColor = System.Drawing.Color.Transparent;
            this.glowingLine1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.glowingLine1.Location = new System.Drawing.Point(0, 31);
            this.glowingLine1.Margin = new System.Windows.Forms.Padding(0);
            this.glowingLine1.Name = "glowingLine1";
            this.glowingLine1.Size = new System.Drawing.Size(1052, 2);
            this.glowingLine1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.glowingLine1.TabIndex = 37;
            this.glowingLine1.TabStop = false;
            // 
            // glowingLine0
            // 
            this.glowingLine0.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glowingLine0.BackColor = System.Drawing.Color.Transparent;
            this.glowingLine0.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.glowingLine0.Location = new System.Drawing.Point(0, 0);
            this.glowingLine0.Margin = new System.Windows.Forms.Padding(0);
            this.glowingLine0.Name = "glowingLine0";
            this.glowingLine0.Size = new System.Drawing.Size(1052, 2);
            this.glowingLine0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.glowingLine0.TabIndex = 36;
            this.glowingLine0.TabStop = false;
            // 
            // game_listSizer
            // 
            this.game_listSizer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.game_listSizer.BackColor = System.Drawing.Color.Transparent;
            this.game_listSizer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.game_listSizer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.game_listSizer.Controls.Add(this.list_Games);
            this.game_listSizer.Location = new System.Drawing.Point(3, 38);
            this.game_listSizer.Name = "game_listSizer";
            this.game_listSizer.Size = new System.Drawing.Size(203, 510);
            this.game_listSizer.TabIndex = 35;
            // 
            // list_Games
            // 
            this.list_Games.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.list_Games.AutoScroll = true;
            this.list_Games.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.list_Games.BackColor = System.Drawing.Color.Transparent;
            this.list_Games.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.list_Games.Border = 0;
            this.list_Games.ContextMenuStrip = this.gameContextMenuStrip;
            this.list_Games.Cursor = System.Windows.Forms.Cursors.Hand;
            this.list_Games.Location = new System.Drawing.Point(0, 0);
            this.list_Games.Margin = new System.Windows.Forms.Padding(0);
            this.list_Games.Name = "list_Games";
            this.list_Games.Offset = new System.Drawing.Size(0, 0);
            this.list_Games.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.list_Games.Size = new System.Drawing.Size(219, 510);
            this.list_Games.TabIndex = 2;
            this.list_Games.SelectedChanged += new System.Action<object, System.Windows.Forms.Control>(this.list_Games_SelectedChanged);
            // 
            // rightFrame
            // 
            this.rightFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rightFrame.BackColor = System.Drawing.Color.Transparent;
            this.rightFrame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.rightFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rightFrame.Controls.Add(this.buttonContainer);
            this.rightFrame.Controls.Add(this.scriptAuthorTxtSizer);
            this.rightFrame.Controls.Add(this.cover);
            this.rightFrame.Controls.Add(this.txt_GameDescSizer);
            this.rightFrame.Location = new System.Drawing.Point(845, 38);
            this.rightFrame.Margin = new System.Windows.Forms.Padding(0);
            this.rightFrame.Name = "rightFrame";
            this.rightFrame.Size = new System.Drawing.Size(203, 510);
            this.rightFrame.TabIndex = 34;
            this.rightFrame.Visible = false;
            // 
            // buttonContainer
            // 
            this.buttonContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonContainer.Controls.Add(this.btn_GameDesc);
            this.buttonContainer.Controls.Add(this.btn_gameOptions);
            this.buttonContainer.Controls.Add(this.btn_scriptAuthorTxt);
            this.buttonContainer.Controls.Add(this.button_UpdateAvailable);
            this.buttonContainer.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonContainer.Location = new System.Drawing.Point(0, 441);
            this.buttonContainer.Name = "buttonContainer";
            this.buttonContainer.Size = new System.Drawing.Size(203, 67);
            this.buttonContainer.TabIndex = 0;
            // 
            // btn_GameDesc
            // 
            this.btn_GameDesc.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_GameDesc.BackColor = System.Drawing.Color.Transparent;
            this.btn_GameDesc.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_GameDesc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_GameDesc.Enabled = false;
            this.btn_GameDesc.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_GameDesc.FlatAppearance.BorderSize = 0;
            this.btn_GameDesc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_GameDesc.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_GameDesc.Location = new System.Drawing.Point(192, -16);
            this.btn_GameDesc.Margin = new System.Windows.Forms.Padding(2);
            this.btn_GameDesc.Name = "btn_GameDesc";
            this.btn_GameDesc.Size = new System.Drawing.Size(166, 13);
            this.btn_GameDesc.TabIndex = 29;
            this.btn_GameDesc.Text = "Game Description";
            this.btn_GameDesc.UseVisualStyleBackColor = false;
            this.btn_GameDesc.Visible = false;
            this.btn_GameDesc.Click += new System.EventHandler(this.btn_GameDesc_Click);
            // 
            // btn_gameOptions
            // 
            this.btn_gameOptions.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_gameOptions.BackColor = System.Drawing.Color.Transparent;
            this.btn_gameOptions.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_gameOptions.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_gameOptions.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_gameOptions.FlatAppearance.BorderSize = 0;
            this.btn_gameOptions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_gameOptions.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_gameOptions.Location = new System.Drawing.Point(16, 3);
            this.btn_gameOptions.Margin = new System.Windows.Forms.Padding(2);
            this.btn_gameOptions.Name = "btn_gameOptions";
            this.btn_gameOptions.Size = new System.Drawing.Size(169, 23);
            this.btn_gameOptions.TabIndex = 21;
            this.btn_gameOptions.Text = "Game Options";
            this.btn_gameOptions.UseVisualStyleBackColor = false;
            this.btn_gameOptions.Visible = false;
            this.btn_gameOptions.Click += new System.EventHandler(this.GameOptions_Click);
            // 
            // btn_scriptAuthorTxt
            // 
            this.btn_scriptAuthorTxt.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_scriptAuthorTxt.BackColor = System.Drawing.Color.Transparent;
            this.btn_scriptAuthorTxt.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_scriptAuthorTxt.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_scriptAuthorTxt.Enabled = false;
            this.btn_scriptAuthorTxt.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_scriptAuthorTxt.FlatAppearance.BorderSize = 0;
            this.btn_scriptAuthorTxt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_scriptAuthorTxt.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_scriptAuthorTxt.Location = new System.Drawing.Point(17, -31);
            this.btn_scriptAuthorTxt.Margin = new System.Windows.Forms.Padding(2);
            this.btn_scriptAuthorTxt.Name = "btn_scriptAuthorTxt";
            this.btn_scriptAuthorTxt.Size = new System.Drawing.Size(166, 11);
            this.btn_scriptAuthorTxt.TabIndex = 30;
            this.btn_scriptAuthorTxt.Text = "Game Handler Instructions";
            this.btn_scriptAuthorTxt.UseVisualStyleBackColor = false;
            this.btn_scriptAuthorTxt.Visible = false;
            this.btn_scriptAuthorTxt.Click += new System.EventHandler(this.btn_scriptAuthorTxt_Click);
            // 
            // button_UpdateAvailable
            // 
            this.button_UpdateAvailable.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_UpdateAvailable.BackColor = System.Drawing.Color.Transparent;
            this.button_UpdateAvailable.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_UpdateAvailable.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_UpdateAvailable.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.button_UpdateAvailable.FlatAppearance.BorderSize = 0;
            this.button_UpdateAvailable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_UpdateAvailable.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_UpdateAvailable.ForeColor = System.Drawing.Color.Yellow;
            this.button_UpdateAvailable.Location = new System.Drawing.Point(20, 33);
            this.button_UpdateAvailable.Margin = new System.Windows.Forms.Padding(2);
            this.button_UpdateAvailable.Name = "button_UpdateAvailable";
            this.button_UpdateAvailable.Size = new System.Drawing.Size(166, 23);
            this.button_UpdateAvailable.TabIndex = 23;
            this.button_UpdateAvailable.Text = "New Handler Available!";
            this.button_UpdateAvailable.UseVisualStyleBackColor = false;
            this.button_UpdateAvailable.Visible = false;
            this.button_UpdateAvailable.Click += new System.EventHandler(this.button_UpdateAvailable_Click);
            // 
            // scriptAuthorTxtSizer
            // 
            this.scriptAuthorTxtSizer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptAuthorTxtSizer.AutoScroll = true;
            this.scriptAuthorTxtSizer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.scriptAuthorTxtSizer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.scriptAuthorTxtSizer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scriptAuthorTxtSizer.Controls.Add(this.scriptAuthorTxt);
            this.scriptAuthorTxtSizer.Font = new System.Drawing.Font("Constantia", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scriptAuthorTxtSizer.Location = new System.Drawing.Point(17, 240);
            this.scriptAuthorTxtSizer.Margin = new System.Windows.Forms.Padding(5);
            this.scriptAuthorTxtSizer.Name = "scriptAuthorTxtSizer";
            this.scriptAuthorTxtSizer.Size = new System.Drawing.Size(169, 193);
            this.scriptAuthorTxtSizer.TabIndex = 31;
            // 
            // scriptAuthorTxt
            // 
            this.scriptAuthorTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptAuthorTxt.BackColor = System.Drawing.Color.Black;
            this.scriptAuthorTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scriptAuthorTxt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.scriptAuthorTxt.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scriptAuthorTxt.ForeColor = System.Drawing.Color.White;
            this.scriptAuthorTxt.Location = new System.Drawing.Point(0, 0);
            this.scriptAuthorTxt.Margin = new System.Windows.Forms.Padding(0);
            this.scriptAuthorTxt.MinimumSize = new System.Drawing.Size(185, 178);
            this.scriptAuthorTxt.Multiline = true;
            this.scriptAuthorTxt.Name = "scriptAuthorTxt";
            this.scriptAuthorTxt.ReadOnly = true;
            this.scriptAuthorTxt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.scriptAuthorTxt.Size = new System.Drawing.Size(185, 193);
            this.scriptAuthorTxt.TabIndex = 13;
            // 
            // cover
            // 
            this.cover.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cover.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cover.BackColor = System.Drawing.Color.Black;
            this.cover.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cover.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cover.Controls.Add(this.coverFrame);
            this.cover.Location = new System.Drawing.Point(17, 7);
            this.cover.Name = "cover";
            this.cover.Size = new System.Drawing.Size(169, 216);
            this.cover.TabIndex = 27;
            // 
            // coverFrame
            // 
            this.coverFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.coverFrame.BackColor = System.Drawing.Color.Transparent;
            this.coverFrame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.coverFrame.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F);
            this.coverFrame.Location = new System.Drawing.Point(0, 0);
            this.coverFrame.Margin = new System.Windows.Forms.Padding(0);
            this.coverFrame.Name = "coverFrame";
            this.coverFrame.Size = new System.Drawing.Size(167, 214);
            this.coverFrame.TabIndex = 26;
            this.coverFrame.Visible = false;
            // 
            // txt_GameDescSizer
            // 
            this.txt_GameDescSizer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_GameDescSizer.AutoScroll = true;
            this.txt_GameDescSizer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txt_GameDescSizer.BackColor = System.Drawing.Color.Transparent;
            this.txt_GameDescSizer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.txt_GameDescSizer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_GameDescSizer.Controls.Add(this.txt_GameDesc);
            this.txt_GameDescSizer.Font = new System.Drawing.Font("Constantia", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_GameDescSizer.Location = new System.Drawing.Point(140, 240);
            this.txt_GameDescSizer.Margin = new System.Windows.Forms.Padding(5);
            this.txt_GameDescSizer.MinimumSize = new System.Drawing.Size(169, 178);
            this.txt_GameDescSizer.Name = "txt_GameDescSizer";
            this.txt_GameDescSizer.Size = new System.Drawing.Size(169, 178);
            this.txt_GameDescSizer.TabIndex = 28;
            this.txt_GameDescSizer.Visible = false;
            // 
            // txt_GameDesc
            // 
            this.txt_GameDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_GameDesc.BackColor = System.Drawing.Color.Black;
            this.txt_GameDesc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_GameDesc.Cursor = System.Windows.Forms.Cursors.Default;
            this.txt_GameDesc.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_GameDesc.ForeColor = System.Drawing.Color.White;
            this.txt_GameDesc.Location = new System.Drawing.Point(0, 0);
            this.txt_GameDesc.Margin = new System.Windows.Forms.Padding(0);
            this.txt_GameDesc.MinimumSize = new System.Drawing.Size(185, 178);
            this.txt_GameDesc.Multiline = true;
            this.txt_GameDesc.Name = "txt_GameDesc";
            this.txt_GameDesc.ReadOnly = true;
            this.txt_GameDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_GameDesc.Size = new System.Drawing.Size(185, 178);
            this.txt_GameDesc.TabIndex = 27;
            this.txt_GameDesc.Visible = false;
            // 
            // mainButtonFrame
            // 
            this.mainButtonFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainButtonFrame.BackColor = System.Drawing.Color.Transparent;
            this.mainButtonFrame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.mainButtonFrame.Controls.Add(this.btn_dlFromHub);
            this.mainButtonFrame.Controls.Add(this.btn_Download);
            this.mainButtonFrame.Controls.Add(this.btn_Extract);
            this.mainButtonFrame.Controls.Add(this.label_StepTitle);
            this.mainButtonFrame.Controls.Add(this.btnSearch);
            this.mainButtonFrame.Controls.Add(this.btnBack);
            this.mainButtonFrame.Controls.Add(this.btn_Play);
            this.mainButtonFrame.Controls.Add(this.btn_Next);
            this.mainButtonFrame.Controls.Add(this.btnAutoSearch);
            this.mainButtonFrame.Location = new System.Drawing.Point(0, 2);
            this.mainButtonFrame.Name = "mainButtonFrame";
            this.mainButtonFrame.Size = new System.Drawing.Size(1050, 31);
            this.mainButtonFrame.TabIndex = 33;
            // 
            // btn_Download
            // 
            this.btn_Download.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Download.BackColor = System.Drawing.Color.Transparent;
            this.btn_Download.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Download.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Download.Enabled = false;
            this.btn_Download.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_Download.FlatAppearance.BorderSize = 0;
            this.btn_Download.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Download.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Download.Location = new System.Drawing.Point(374, 2);
            this.btn_Download.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Download.Name = "btn_Download";
            this.btn_Download.Size = new System.Drawing.Size(159, 22);
            this.btn_Download.TabIndex = 22;
            this.btn_Download.Text = " Download Game Handler";
            this.btn_Download.UseVisualStyleBackColor = false;
            this.btn_Download.Click += new System.EventHandler(this.btn_Download_Click);
            // 
            // btn_Extract
            // 
            this.btn_Extract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Extract.BackColor = System.Drawing.Color.Transparent;
            this.btn_Extract.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Extract.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Extract.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_Extract.FlatAppearance.BorderSize = 0;
            this.btn_Extract.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Extract.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Extract.Location = new System.Drawing.Point(538, 2);
            this.btn_Extract.Name = "btn_Extract";
            this.btn_Extract.Size = new System.Drawing.Size(110, 22);
            this.btn_Extract.TabIndex = 0;
            this.btn_Extract.Text = "Extract Handler";
            this.btn_Extract.UseVisualStyleBackColor = false;
            this.btn_Extract.Click += new System.EventHandler(this.btn_Extract_Click);
            // 
            // label_StepTitle
            // 
            this.label_StepTitle.AutoSize = true;
            this.label_StepTitle.BackColor = System.Drawing.Color.Transparent;
            this.label_StepTitle.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.label_StepTitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label_StepTitle.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_StepTitle.Location = new System.Drawing.Point(6, 5);
            this.label_StepTitle.Margin = new System.Windows.Forms.Padding(0);
            this.label_StepTitle.Name = "label_StepTitle";
            this.label_StepTitle.Size = new System.Drawing.Size(100, 17);
            this.label_StepTitle.TabIndex = 3;
            this.label_StepTitle.Text = "Select a game";
            this.label_StepTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.BackColor = System.Drawing.Color.Transparent;
            this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.Location = new System.Drawing.Point(653, 2);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(2);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(106, 22);
            this.btnSearch.TabIndex = 7;
            this.btnSearch.Text = "Search Game";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBack.BackColor = System.Drawing.Color.Transparent;
            this.btnBack.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Enabled = false;
            this.btnBack.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Location = new System.Drawing.Point(896, 2);
            this.btnBack.Margin = new System.Windows.Forms.Padding(2);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(22, 22);
            this.btnBack.TabIndex = 9;
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.arrow_Back_Click);
            // 
            // btn_Play
            // 
            this.btn_Play.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Play.BackColor = System.Drawing.Color.Transparent;
            this.btn_Play.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Play.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Play.Enabled = false;
            this.btn_Play.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_Play.FlatAppearance.BorderSize = 0;
            this.btn_Play.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Play.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Play.ForeColor = System.Drawing.Color.Lime;
            this.btn_Play.Location = new System.Drawing.Point(922, 1);
            this.btn_Play.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Play.Name = "btn_Play";
            this.btn_Play.Size = new System.Drawing.Size(100, 22);
            this.btn_Play.TabIndex = 4;
            this.btn_Play.Text = "P L A Y";
            this.btn_Play.UseVisualStyleBackColor = false;
            this.btn_Play.Click += new System.EventHandler(this.btn_Play_Click);
            // 
            // btn_Next
            // 
            this.btn_Next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Next.BackColor = System.Drawing.Color.Transparent;
            this.btn_Next.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Next.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Next.Enabled = false;
            this.btn_Next.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btn_Next.FlatAppearance.BorderSize = 0;
            this.btn_Next.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Next.Location = new System.Drawing.Point(1026, 2);
            this.btn_Next.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Next.Name = "btn_Next";
            this.btn_Next.Size = new System.Drawing.Size(22, 22);
            this.btn_Next.TabIndex = 11;
            this.btn_Next.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.btn_Next.UseVisualStyleBackColor = false;
            this.btn_Next.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnAutoSearch
            // 
            this.btnAutoSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAutoSearch.BackColor = System.Drawing.Color.Transparent;
            this.btnAutoSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAutoSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAutoSearch.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnAutoSearch.FlatAppearance.BorderSize = 0;
            this.btnAutoSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAutoSearch.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoSearch.Location = new System.Drawing.Point(763, 2);
            this.btnAutoSearch.Margin = new System.Windows.Forms.Padding(2);
            this.btnAutoSearch.Name = "btnAutoSearch";
            this.btnAutoSearch.Size = new System.Drawing.Size(121, 22);
            this.btnAutoSearch.TabIndex = 10;
            this.btnAutoSearch.Text = "Auto Search Game";
            this.btnAutoSearch.UseVisualStyleBackColor = false;
            this.btnAutoSearch.Click += new System.EventHandler(this.btnAutoSearch_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1050, 602);
            this.ControlBox = false;
            this.Controls.Add(this.splash);
            this.Controls.Add(this.third_party_tools_container);
            this.Controls.Add(this.clientAreaPanel);
            this.Controls.Add(this.btn_noHub);
            this.Controls.Add(this.btn_reddit);
            this.Controls.Add(this.btn_Discord);
            this.Controls.Add(this.btn_SplitCalculator);
            this.Controls.Add(this.btn_thirdPartytools);
            this.Controls.Add(this.btn_faq);
            this.Controls.Add(this.txt_version);
            this.Controls.Add(this.btn_downloadAssets);
            this.Controls.Add(this.btn_settings);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.minimizeBtn);
            this.Controls.Add(this.logo);
            this.Controls.Add(this.maximizeBtn);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nucleus Co-op";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.MouseHover += new System.EventHandler(this.MainForm_MouseHover);
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            this.gameContextMenuStrip.ResumeLayout(false);
            this.third_party_tools_container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splash)).EndInit();
            this.clientAreaPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.stepPanelPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.glowingLine1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.glowingLine0)).EndInit();
            this.game_listSizer.ResumeLayout(false);
            this.rightFrame.ResumeLayout(false);
            this.buttonContainer.ResumeLayout(false);
            this.scriptAuthorTxtSizer.ResumeLayout(false);
            this.scriptAuthorTxtSizer.PerformLayout();
            this.cover.ResumeLayout(false);
            this.txt_GameDescSizer.ResumeLayout(false);
            this.txt_GameDescSizer.PerformLayout();
            this.mainButtonFrame.ResumeLayout(false);
            this.mainButtonFrame.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private BufferedClientAreaPanel StepPanel;
        private Gaming.ControlListBox list_Games;
        private System.Windows.Forms.Label label_StepTitle;
        private System.Windows.Forms.Button btn_Play;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnAutoSearch;
        private System.Windows.Forms.Button btn_Next;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip gameContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem nullToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem detailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDataFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem changeIconToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scriptNotesToolStripMenuItem;
        private System.Windows.Forms.Button btn_gameOptions;
        private System.Windows.Forms.TextBox scriptAuthorTxt;
        private System.Windows.Forms.ToolStripMenuItem openOrigExePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteContentFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteUserProfileConfigPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openUserProfileSavePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteUserProfileSavePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.Button btn_Download;
        private System.Windows.Forms.ToolStripMenuItem openDocumentConfigPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteDocumentConfigPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDocumentSavePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteDocumentSavePathToolStripMenuItem;
		private System.Windows.Forms.Button button_UpdateAvailable;
		private System.Windows.Forms.PictureBox stepPanelPictureBox;
        private Button minimizeBtn;
        private Button maximizeBtn;
        private Button closeBtn;
        private Button btn_settings;
        private PictureBox logo;
        private PictureBox splash;
        private BufferedClientAreaPanel coverFrame;
        private Button btn_scriptAuthorTxt;
        private BufferedClientAreaPanel scriptAuthorTxtSizer;
        private BufferedClientAreaPanel mainButtonFrame;
        private BufferedClientAreaPanel rightFrame;
        private BufferedClientAreaPanel game_listSizer;
        private BufferedClientAreaPanel cover;
        private BufferedClientAreaPanel buttonContainer;
        private Button btn_downloadAssets;
        private Label txt_version;
        public PictureBox glowingLine0;
        public PictureBox glowingLine1;
        private Button btn_faq;
        private Button btn_thirdPartytools;
        private Panel third_party_tools_container;
        private LinkLabel linkLabel3;
        private LinkLabel linkLabel2;
        private LinkLabel linkLabel1;
        private Button btn_Extract;
        private Button btn_SplitCalculator;
        private LinkLabel linkLabel4;
        private Button btn_Discord;
        private Button btn_reddit;
        private Button btn_noHub;
        public TextBox txt_GameDesc;
        public BufferedClientAreaPanel txt_GameDescSizer;
        public Button btn_GameDesc;
        public BufferedClientAreaPanel clientAreaPanel;
        private Button btn_dlFromHub;
    }
}