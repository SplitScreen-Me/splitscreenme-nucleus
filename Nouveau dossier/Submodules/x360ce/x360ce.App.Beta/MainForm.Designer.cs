﻿using System.Windows.Forms;
using System.Drawing;
using System;
namespace x360ce.App
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.MainTabControl = new System.Windows.Forms.TabControl();
			this.Pad1TabPage = new System.Windows.Forms.TabPage();
			this.Pad2TabPage = new System.Windows.Forms.TabPage();
			this.Pad3TabPage = new System.Windows.Forms.TabPage();
			this.Pad4TabPage = new System.Windows.Forms.TabPage();
			this.OptionsTabPage = new System.Windows.Forms.TabPage();
			this.OptionsPanel = new x360ce.App.Controls.OptionsControl();
			this.GamesTabPage = new System.Windows.Forms.TabPage();
			this.GameSettingsPanel = new x360ce.App.Controls.GameSettingsUserControl();
			this.ControllersTabPage = new System.Windows.Forms.TabPage();
			this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
			this.controllersUserControl1 = new x360ce.App.ControllersUserControl();
			this.HardwareTabPage = new System.Windows.Forms.TabPage();
			this.HardwarePanel = new JocysCom.ClassLibrary.IO.HardwareControl();
			this.SettingsTabPage = new System.Windows.Forms.TabPage();
			this.SettingsDatabasePanel = new x360ce.App.Controls.ControllerSettingsUserControl();
			this.IniTabPage = new System.Windows.Forms.TabPage();
			this.IniTextBox = new System.Windows.Forms.TextBox();
			this.HelpTabPage = new System.Windows.Forms.TabPage();
			this.HelpRichTextBox = new System.Windows.Forms.RichTextBox();
			this.AboutTabPage = new System.Windows.Forms.TabPage();
			this.BuletImageList = new System.Windows.Forms.ImageList(this.components);
			this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
			this.StatusTimerLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusEventsLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusSaveLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusIsAdminLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusIniLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusDllLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.HelpBodyLabel = new System.Windows.Forms.Label();
			this.HelpPictureBox = new System.Windows.Forms.PictureBox();
			this.HelpSubjectLabel = new System.Windows.Forms.Label();
			this.HeaderPictureBox = new System.Windows.Forms.PictureBox();
			this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.TrayNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.TrayContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.OpenApplicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.GameToCustomizeComboBox = new System.Windows.Forms.ComboBox();
			this.BusyLoadingCircle = new MRG.Controls.UI.LoadingCircle();
			this.MainTabControl.SuspendLayout();
			this.OptionsTabPage.SuspendLayout();
			this.GamesTabPage.SuspendLayout();
			this.ControllersTabPage.SuspendLayout();
			this.HardwareTabPage.SuspendLayout();
			this.SettingsTabPage.SuspendLayout();
			this.IniTabPage.SuspendLayout();
			this.HelpTabPage.SuspendLayout();
			this.MainStatusStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderPictureBox)).BeginInit();
			this.TrayContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainTabControl.Controls.Add(this.Pad1TabPage);
			this.MainTabControl.Controls.Add(this.Pad2TabPage);
			this.MainTabControl.Controls.Add(this.Pad3TabPage);
			this.MainTabControl.Controls.Add(this.Pad4TabPage);
			this.MainTabControl.Controls.Add(this.OptionsTabPage);
			this.MainTabControl.Controls.Add(this.GamesTabPage);
			this.MainTabControl.Controls.Add(this.ControllersTabPage);
			this.MainTabControl.Controls.Add(this.HardwareTabPage);
			this.MainTabControl.Controls.Add(this.SettingsTabPage);
			this.MainTabControl.Controls.Add(this.IniTabPage);
			this.MainTabControl.Controls.Add(this.HelpTabPage);
			this.MainTabControl.Controls.Add(this.AboutTabPage);
			this.MainTabControl.ImageList = this.BuletImageList;
			this.MainTabControl.Location = new System.Drawing.Point(6, 97);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = new System.Drawing.Size(776, 624);
			this.MainTabControl.TabIndex = 1;
			// 
			// Pad1TabPage
			// 
			this.Pad1TabPage.ImageKey = "bullet_square_glass_grey.png";
			this.Pad1TabPage.Location = new System.Drawing.Point(4, 23);
			this.Pad1TabPage.Name = "Pad1TabPage";
			this.Pad1TabPage.Size = new System.Drawing.Size(768, 597);
			this.Pad1TabPage.TabIndex = 0;
			this.Pad1TabPage.Text = "Controller 1";
			// 
			// Pad2TabPage
			// 
			this.Pad2TabPage.ImageKey = "bullet_square_glass_grey.png";
			this.Pad2TabPage.Location = new System.Drawing.Point(4, 23);
			this.Pad2TabPage.Name = "Pad2TabPage";
			this.Pad2TabPage.Size = new System.Drawing.Size(768, 597);
			this.Pad2TabPage.TabIndex = 0;
			this.Pad2TabPage.Text = "Controller 2";
			// 
			// Pad3TabPage
			// 
			this.Pad3TabPage.ImageKey = "bullet_square_glass_grey.png";
			this.Pad3TabPage.Location = new System.Drawing.Point(4, 23);
			this.Pad3TabPage.Name = "Pad3TabPage";
			this.Pad3TabPage.Size = new System.Drawing.Size(768, 597);
			this.Pad3TabPage.TabIndex = 0;
			this.Pad3TabPage.Text = "Controller 3";
			// 
			// Pad4TabPage
			// 
			this.Pad4TabPage.ImageKey = "bullet_square_glass_grey.png";
			this.Pad4TabPage.Location = new System.Drawing.Point(4, 23);
			this.Pad4TabPage.Name = "Pad4TabPage";
			this.Pad4TabPage.Size = new System.Drawing.Size(768, 597);
			this.Pad4TabPage.TabIndex = 0;
			this.Pad4TabPage.Text = "Controller 4";
			// 
			// OptionsTabPage
			// 
			this.OptionsTabPage.BackColor = System.Drawing.Color.Transparent;
			this.OptionsTabPage.Controls.Add(this.OptionsPanel);
			this.OptionsTabPage.Location = new System.Drawing.Point(4, 23);
			this.OptionsTabPage.Name = "OptionsTabPage";
			this.OptionsTabPage.Size = new System.Drawing.Size(768, 597);
			this.OptionsTabPage.TabIndex = 0;
			this.OptionsTabPage.Text = "Options";
			// 
			// OptionsPanel
			// 
			this.OptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionsPanel.Location = new System.Drawing.Point(0, 0);
			this.OptionsPanel.Name = "OptionsPanel";
			this.OptionsPanel.Size = new System.Drawing.Size(768, 597);
			this.OptionsPanel.TabIndex = 30;
			// 
			// GamesTabPage
			// 
			this.GamesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GamesTabPage.Controls.Add(this.GameSettingsPanel);
			this.GamesTabPage.Location = new System.Drawing.Point(4, 23);
			this.GamesTabPage.Name = "GamesTabPage";
			this.GamesTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.GamesTabPage.Size = new System.Drawing.Size(768, 597);
			this.GamesTabPage.TabIndex = 2;
			this.GamesTabPage.Text = "Games";
			// 
			// GameSettingsPanel
			// 
			this.GameSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GameSettingsPanel.Location = new System.Drawing.Point(3, 3);
			this.GameSettingsPanel.Name = "GameSettingsPanel";
			this.GameSettingsPanel.Size = new System.Drawing.Size(762, 591);
			this.GameSettingsPanel.TabIndex = 1;
			// 
			// ControllersTabPage
			// 
			this.ControllersTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ControllersTabPage.Controls.Add(this.elementHost1);
			this.ControllersTabPage.Location = new System.Drawing.Point(4, 23);
			this.ControllersTabPage.Name = "ControllersTabPage";
			this.ControllersTabPage.Size = new System.Drawing.Size(768, 597);
			this.ControllersTabPage.TabIndex = 4;
			this.ControllersTabPage.Text = "Controllers";
			// 
			// elementHost1
			// 
			this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementHost1.Location = new System.Drawing.Point(0, 0);
			this.elementHost1.Name = "elementHost1";
			this.elementHost1.Size = new System.Drawing.Size(768, 597);
			this.elementHost1.TabIndex = 0;
			this.elementHost1.Text = "elementHost1";
			this.elementHost1.Child = this.controllersUserControl1;
			// 
			// HardwareTabPage
			// 
			this.HardwareTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.HardwareTabPage.Controls.Add(this.HardwarePanel);
			this.HardwareTabPage.Location = new System.Drawing.Point(4, 23);
			this.HardwareTabPage.Name = "HardwareTabPage";
			this.HardwareTabPage.Size = new System.Drawing.Size(768, 597);
			this.HardwareTabPage.TabIndex = 5;
			this.HardwareTabPage.Text = "Hardware";
			// 
			// HardwarePanel
			// 
			this.HardwarePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HardwarePanel.Location = new System.Drawing.Point(0, 0);
			this.HardwarePanel.Name = "HardwarePanel";
			this.HardwarePanel.Size = new System.Drawing.Size(768, 597);
			this.HardwarePanel.TabIndex = 1;
			// 
			// SettingsTabPage
			// 
			this.SettingsTabPage.Controls.Add(this.SettingsDatabasePanel);
			this.SettingsTabPage.Location = new System.Drawing.Point(4, 23);
			this.SettingsTabPage.Name = "SettingsTabPage";
			this.SettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.SettingsTabPage.Size = new System.Drawing.Size(768, 597);
			this.SettingsTabPage.TabIndex = 1;
			this.SettingsTabPage.Text = "Settings";
			// 
			// SettingsDatabasePanel
			// 
			this.SettingsDatabasePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SettingsDatabasePanel.Location = new System.Drawing.Point(3, 3);
			this.SettingsDatabasePanel.Name = "SettingsDatabasePanel";
			this.SettingsDatabasePanel.Size = new System.Drawing.Size(762, 591);
			this.SettingsDatabasePanel.TabIndex = 0;
			// 
			// IniTabPage
			// 
			this.IniTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.IniTabPage.Controls.Add(this.IniTextBox);
			this.IniTabPage.Location = new System.Drawing.Point(4, 23);
			this.IniTabPage.Name = "IniTabPage";
			this.IniTabPage.Size = new System.Drawing.Size(768, 597);
			this.IniTabPage.TabIndex = 3;
			this.IniTabPage.Text = "INI";
			// 
			// IniTextBox
			// 
			this.IniTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.IniTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IniTextBox.Location = new System.Drawing.Point(0, 0);
			this.IniTextBox.Multiline = true;
			this.IniTextBox.Name = "IniTextBox";
			this.IniTextBox.ReadOnly = true;
			this.IniTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.IniTextBox.Size = new System.Drawing.Size(768, 597);
			this.IniTextBox.TabIndex = 1;
			// 
			// HelpTabPage
			// 
			this.HelpTabPage.Controls.Add(this.HelpRichTextBox);
			this.HelpTabPage.Location = new System.Drawing.Point(4, 23);
			this.HelpTabPage.Name = "HelpTabPage";
			this.HelpTabPage.Size = new System.Drawing.Size(768, 597);
			this.HelpTabPage.TabIndex = 0;
			this.HelpTabPage.Text = "Help";
			// 
			// HelpRichTextBox
			// 
			this.HelpRichTextBox.BackColor = System.Drawing.Color.White;
			this.HelpRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.HelpRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HelpRichTextBox.Location = new System.Drawing.Point(0, 0);
			this.HelpRichTextBox.Margin = new System.Windows.Forms.Padding(0);
			this.HelpRichTextBox.Name = "HelpRichTextBox";
			this.HelpRichTextBox.ReadOnly = true;
			this.HelpRichTextBox.Size = new System.Drawing.Size(768, 597);
			this.HelpRichTextBox.TabIndex = 0;
			this.HelpRichTextBox.Text = "";
			// 
			// AboutTabPage
			// 
			this.AboutTabPage.BackColor = System.Drawing.Color.Transparent;
			this.AboutTabPage.Location = new System.Drawing.Point(4, 23);
			this.AboutTabPage.Name = "AboutTabPage";
			this.AboutTabPage.Size = new System.Drawing.Size(768, 597);
			this.AboutTabPage.TabIndex = 0;
			this.AboutTabPage.Text = "About";
			// 
			// BuletImageList
			// 
			this.BuletImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("BuletImageList.ImageStream")));
			this.BuletImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.BuletImageList.Images.SetKeyName(0, "bullet_square_glass_blue.png");
			this.BuletImageList.Images.SetKeyName(1, "bullet_square_glass_green.png");
			this.BuletImageList.Images.SetKeyName(2, "bullet_square_glass_grey.png");
			this.BuletImageList.Images.SetKeyName(3, "bullet_square_glass_red.png");
			this.BuletImageList.Images.SetKeyName(4, "bullet_square_glass_yellow.png");
			// 
			// MainStatusStrip
			// 
			this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusTimerLabel,
            this.toolStripStatusLabel2,
            this.StatusEventsLabel,
            this.StatusSaveLabel,
            this.StatusIsAdminLabel,
            this.StatusIniLabel,
            this.StatusDllLabel});
			this.MainStatusStrip.Location = new System.Drawing.Point(0, 728);
			this.MainStatusStrip.Name = "MainStatusStrip";
			this.MainStatusStrip.Size = new System.Drawing.Size(786, 24);
			this.MainStatusStrip.SizingGrip = false;
			this.MainStatusStrip.TabIndex = 0;
			this.MainStatusStrip.Text = "statusStrip1";
			// 
			// StatusTimerLabel
			// 
			this.StatusTimerLabel.Name = "StatusTimerLabel";
			this.StatusTimerLabel.Size = new System.Drawing.Size(98, 19);
			this.StatusTimerLabel.Text = "StatusTimerLabel";
			// 
			// toolStripStatusLabel2
			// 
			this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
			this.toolStripStatusLabel2.Size = new System.Drawing.Size(189, 19);
			this.toolStripStatusLabel2.Spring = true;
			// 
			// StatusEventsLabel
			// 
			this.StatusEventsLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.StatusEventsLabel.Name = "StatusEventsLabel";
			this.StatusEventsLabel.Size = new System.Drawing.Size(105, 19);
			this.StatusEventsLabel.Text = "StatusEventsLabel";
			// 
			// StatusSaveLabel
			// 
			this.StatusSaveLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.StatusSaveLabel.Name = "StatusSaveLabel";
			this.StatusSaveLabel.Size = new System.Drawing.Size(95, 19);
			this.StatusSaveLabel.Text = "StatusSaveLabel";
			// 
			// StatusIsAdminLabel
			// 
			this.StatusIsAdminLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.StatusIsAdminLabel.Name = "StatusIsAdminLabel";
			this.StatusIsAdminLabel.Size = new System.Drawing.Size(115, 19);
			this.StatusIsAdminLabel.Text = "StatusIsAdminLabel";
			// 
			// StatusIniLabel
			// 
			this.StatusIniLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.StatusIniLabel.Name = "StatusIniLabel";
			this.StatusIniLabel.Size = new System.Drawing.Size(84, 19);
			this.StatusIniLabel.Text = "StatusIniLabel";
			// 
			// StatusDllLabel
			// 
			this.StatusDllLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.StatusDllLabel.Name = "StatusDllLabel";
			this.StatusDllLabel.Size = new System.Drawing.Size(85, 19);
			this.StatusDllLabel.Text = "StatusDllLabel";
			// 
			// HelpBodyLabel
			// 
			this.HelpBodyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HelpBodyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HelpBodyLabel.Location = new System.Drawing.Point(42, 29);
			this.HelpBodyLabel.Name = "HelpBodyLabel";
			this.HelpBodyLabel.Size = new System.Drawing.Size(582, 32);
			this.HelpBodyLabel.TabIndex = 7;
			this.HelpBodyLabel.Text = resources.GetString("HelpBodyLabel.Text");
			// 
			// HelpPictureBox
			// 
			this.HelpPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("HelpPictureBox.Image")));
			this.HelpPictureBox.Location = new System.Drawing.Point(6, 29);
			this.HelpPictureBox.Name = "HelpPictureBox";
			this.HelpPictureBox.Size = new System.Drawing.Size(24, 24);
			this.HelpPictureBox.TabIndex = 8;
			this.HelpPictureBox.TabStop = false;
			// 
			// HelpSubjectLabel
			// 
			this.HelpSubjectLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HelpSubjectLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HelpSubjectLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpSubjectLabel.Location = new System.Drawing.Point(6, 9);
			this.HelpSubjectLabel.Name = "HelpSubjectLabel";
			this.HelpSubjectLabel.Size = new System.Drawing.Size(618, 20);
			this.HelpSubjectLabel.TabIndex = 5;
			this.HelpSubjectLabel.Text = "Controller 1 - General";
			// 
			// HeaderPictureBox
			// 
			this.HeaderPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("HeaderPictureBox.Image")));
			this.HeaderPictureBox.Location = new System.Drawing.Point(630, 9);
			this.HeaderPictureBox.Name = "HeaderPictureBox";
			this.HeaderPictureBox.Size = new System.Drawing.Size(48, 48);
			this.HeaderPictureBox.TabIndex = 6;
			this.HeaderPictureBox.TabStop = false;
			// 
			// TrayNotifyIcon
			// 
			this.TrayNotifyIcon.ContextMenuStrip = this.TrayContextMenuStrip;
			this.TrayNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayNotifyIcon.Icon")));
			this.TrayNotifyIcon.Visible = true;
			this.TrayNotifyIcon.DoubleClick += new System.EventHandler(this.TrayNotifyIcon_DoubleClick);
			// 
			// TrayContextMenuStrip
			// 
			this.TrayContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenApplicationToolStripMenuItem,
            this.ExitToolStripMenuItem});
			this.TrayContextMenuStrip.Name = "TrayContextMenuStrip";
			this.TrayContextMenuStrip.Size = new System.Drawing.Size(170, 48);
			// 
			// OpenApplicationToolStripMenuItem
			// 
			this.OpenApplicationToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OpenApplicationToolStripMenuItem.Image = global::x360ce.App.Properties.Resources.app_16x16;
			this.OpenApplicationToolStripMenuItem.Name = "OpenApplicationToolStripMenuItem";
			this.OpenApplicationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
			this.OpenApplicationToolStripMenuItem.Text = "Open Application";
			this.OpenApplicationToolStripMenuItem.Click += new System.EventHandler(this.OpenApplicationToolStripMenuItem_Click);
			// 
			// ExitToolStripMenuItem
			// 
			this.ExitToolStripMenuItem.Image = global::x360ce.App.Properties.Resources.exit_16x16;
			this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
			this.ExitToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
			this.ExitToolStripMenuItem.Text = "Exit";
			this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 73);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(140, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Select a game to customize:";
			// 
			// GameToCustomizeComboBox
			// 
			this.GameToCustomizeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GameToCustomizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.GameToCustomizeComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.GameToCustomizeComboBox.FormattingEnabled = true;
			this.GameToCustomizeComboBox.Location = new System.Drawing.Point(149, 70);
			this.GameToCustomizeComboBox.Name = "GameToCustomizeComboBox";
			this.GameToCustomizeComboBox.Size = new System.Drawing.Size(631, 21);
			this.GameToCustomizeComboBox.TabIndex = 7;
			this.GameToCustomizeComboBox.SelectedIndexChanged += new System.EventHandler(this.GameToCustomizeComboBox_SelectedIndexChanged);
			// 
			// BusyLoadingCircle
			// 
			this.BusyLoadingCircle.Active = false;
			this.BusyLoadingCircle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BusyLoadingCircle.Color = System.Drawing.Color.SteelBlue;
			this.BusyLoadingCircle.InnerCircleRadius = 8;
			this.BusyLoadingCircle.Location = new System.Drawing.Point(630, 9);
			this.BusyLoadingCircle.Name = "BusyLoadingCircle";
			this.BusyLoadingCircle.NumberSpoke = 24;
			this.BusyLoadingCircle.OuterCircleRadius = 9;
			this.BusyLoadingCircle.RotationSpeed = 30;
			this.BusyLoadingCircle.Size = new System.Drawing.Size(48, 48);
			this.BusyLoadingCircle.SpokeThickness = 4;
			this.BusyLoadingCircle.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
			this.BusyLoadingCircle.TabIndex = 9;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(786, 752);
			this.Controls.Add(this.GameToCustomizeComboBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.MainStatusStrip);
			this.Controls.Add(this.MainTabControl);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(700, 790);
			this.Name = "MainForm";
			this.Text = "TocaEdit Xbox 360 Controller Emulator Application";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusStrip, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.GameToCustomizeComboBox, 0);
			this.MainTabControl.ResumeLayout(false);
			this.OptionsTabPage.ResumeLayout(false);
			this.GamesTabPage.ResumeLayout(false);
			this.ControllersTabPage.ResumeLayout(false);
			this.HardwareTabPage.ResumeLayout(false);
			this.SettingsTabPage.ResumeLayout(false);
			this.IniTabPage.ResumeLayout(false);
			this.IniTabPage.PerformLayout();
			this.HelpTabPage.ResumeLayout(false);
			this.MainStatusStrip.ResumeLayout(false);
			this.MainStatusStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderPictureBox)).EndInit();
			this.TrayContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		TabPage AboutTabPage;
		StatusStrip MainStatusStrip;
        TabPage OptionsTabPage;
		TabPage Pad1TabPage;
		TabPage Pad2TabPage;
		TabPage Pad3TabPage;
		TabPage Pad4TabPage;
		ImageList BuletImageList;
		public ToolStripStatusLabel StatusTimerLabel;
		ToolStripStatusLabel StatusEventsLabel;
		ToolStripStatusLabel StatusSaveLabel;
		ToolStripStatusLabel toolStripStatusLabel2;
		TabPage HelpTabPage;
        RichTextBox HelpRichTextBox;
		ToolStripStatusLabel StatusIsAdminLabel;
		ToolStripStatusLabel StatusIniLabel;
        ToolStripStatusLabel StatusDllLabel;
        TabPage SettingsTabPage;
        public Controls.ControllerSettingsUserControl SettingsDatabasePanel;
        public TabControl MainTabControl;
		TabPage GamesTabPage;
        public Controls.OptionsControl OptionsPanel;
        public ToolTip ToolTip;
		public Controls.GameSettingsUserControl GameSettingsPanel;
		private NotifyIcon TrayNotifyIcon;
		private ContextMenuStrip TrayContextMenuStrip;
		private ToolStripMenuItem OpenApplicationToolStripMenuItem;
		private ToolStripMenuItem ExitToolStripMenuItem;
		private Label label1;
		private ComboBox GameToCustomizeComboBox;
		internal Label HelpBodyLabel;
		internal PictureBox HelpPictureBox;
		internal Label HelpSubjectLabel;
		internal PictureBox HeaderPictureBox;
		private MRG.Controls.UI.LoadingCircle BusyLoadingCircle;
		private TabPage IniTabPage;
		private TextBox IniTextBox;
		private TabPage ControllersTabPage;
		private System.Windows.Forms.Integration.ElementHost elementHost1;
		private ControllersUserControl controllersUserControl1;
		private TabPage HardwareTabPage;
		private JocysCom.ClassLibrary.IO.HardwareControl HardwarePanel;
	}
}