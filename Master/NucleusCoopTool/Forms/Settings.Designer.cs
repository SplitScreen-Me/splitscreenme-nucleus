using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Nucleus.Gaming.Windows.Interop;
using WindowScrape.Constants;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Text;
using System.Runtime.ExceptionServices;
using System.Reflection;
using Nucleus.Coop.Forms;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming;
namespace Nucleus.Coop
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.offlineMod = new System.Windows.Forms.ComboBox();
            this.OfflineModLabel = new System.Windows.Forms.Label();
            this.cmb_EpicLang = new System.Windows.Forms.ComboBox();
            this.label46 = new System.Windows.Forms.Label();
            this.ignoreInputLockReminderCheckbox = new System.Windows.Forms.CheckBox();
            this.label35 = new System.Windows.Forms.Label();
            this.nucUserPassTxt = new System.Windows.Forms.TextBox();
            this.keepAccountsCheck = new System.Windows.Forms.CheckBox();
            this.statusCheck = new System.Windows.Forms.CheckBox();
            this.cmb_Lang = new System.Windows.Forms.ComboBox();
            this.label34 = new System.Windows.Forms.Label();
            this.cmb_Network = new System.Windows.Forms.ComboBox();
            this.label32 = new System.Windows.Forms.Label();
            this.debugLogCheck = new System.Windows.Forms.CheckBox();
            this.useNicksCheck = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox_lockKey = new System.Windows.Forms.ComboBox();
            this.label_lockKey = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.settingsTopCmb = new System.Windows.Forms.ComboBox();
            this.settingsStopCmb = new System.Windows.Forms.ComboBox();
            this.settingsTopTxt = new System.Windows.Forms.TextBox();
            this.settingsStopTxt = new System.Windows.Forms.TextBox();
            this.settingsCloseCmb = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.settingsCloseHKTxt = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.label31 = new System.Windows.Forms.Label();
            this.SplitDiv = new System.Windows.Forms.CheckBox();
            this.label49 = new System.Windows.Forms.Label();
            this.SplitColors = new System.Windows.Forms.ComboBox();
            this.label27 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.numVerDiv = new System.Windows.Forms.NumericUpDown();
            this.numHorDiv = new System.Windows.Forms.NumericUpDown();
            this.numMaxPlyrs = new System.Windows.Forms.NumericUpDown();
            this.layoutSizer = new System.Windows.Forms.Panel();
            this.label30 = new System.Windows.Forms.Label();
            this.enableCustomCheckbox = new System.Windows.Forms.CheckBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.label21 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.controllerSixteenNick = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.controllerFifteenNick = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.controllerFourteenNick = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.controllerThirteenNick = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.controllerTwelveNick = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.controllerElevenNick = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.controllerTenNick = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.controllerNineNick = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.controllerEightNick = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.controllerSevenNick = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.controllerSixNick = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.controllerFiveNick = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.controllerFourNick = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.controllerThreeNick = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.controllerTwoNick = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.controllerOneNick = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label39 = new System.Windows.Forms.Label();
            this.audioDefaultDevice = new System.Windows.Forms.Label();
            this.audioCustomSettingsBox = new System.Windows.Forms.GroupBox();
            this.AudioInstance8 = new System.Windows.Forms.ComboBox();
            this.label45 = new System.Windows.Forms.Label();
            this.AudioInstance7 = new System.Windows.Forms.ComboBox();
            this.label44 = new System.Windows.Forms.Label();
            this.AudioInstance6 = new System.Windows.Forms.ComboBox();
            this.label43 = new System.Windows.Forms.Label();
            this.AudioInstance5 = new System.Windows.Forms.ComboBox();
            this.label42 = new System.Windows.Forms.Label();
            this.AudioInstance4 = new System.Windows.Forms.ComboBox();
            this.label41 = new System.Windows.Forms.Label();
            this.AudioInstance3 = new System.Windows.Forms.ComboBox();
            this.label40 = new System.Windows.Forms.Label();
            this.AudioInstance2 = new System.Windows.Forms.ComboBox();
            this.AudioInstance1 = new System.Windows.Forms.ComboBox();
            this.label37 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.audioCustomSettingsRadio = new System.Windows.Forms.RadioButton();
            this.audioDefaultSettingsRadio = new System.Windows.Forms.RadioButton();
            this.audioRefresh = new System.Windows.Forms.Button();
            this.label33 = new System.Windows.Forms.Label();
            this.btn_credits = new System.Windows.Forms.Button();
            this.settingsCloseBtn = new System.Windows.Forms.Button();
            this.settingsSaveBtn = new System.Windows.Forms.Button();
            this.setting_Label = new System.Windows.Forms.Panel();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numVerDiv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHorDiv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxPlyrs)).BeginInit();
            this.tabPage5.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.audioCustomSettingsBox.SuspendLayout();
            this.setting_Label.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabControl2.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl2.ItemSize = new System.Drawing.Size(52, 23);
            this.tabControl2.Location = new System.Drawing.Point(26, 11);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl2.Multiline = true;
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(519, 271);
            this.tabControl2.TabIndex = 36;
            this.tabControl2.Tag = "";
            this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl2_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.Transparent;
            this.tabPage3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tabPage3.Controls.Add(this.offlineMod);
            this.tabPage3.Controls.Add(this.OfflineModLabel);
            this.tabPage3.Controls.Add(this.cmb_EpicLang);
            this.tabPage3.Controls.Add(this.label46);
            this.tabPage3.Controls.Add(this.ignoreInputLockReminderCheckbox);
            this.tabPage3.Controls.Add(this.label35);
            this.tabPage3.Controls.Add(this.nucUserPassTxt);
            this.tabPage3.Controls.Add(this.keepAccountsCheck);
            this.tabPage3.Controls.Add(this.statusCheck);
            this.tabPage3.Controls.Add(this.cmb_Lang);
            this.tabPage3.Controls.Add(this.label34);
            this.tabPage3.Controls.Add(this.cmb_Network);
            this.tabPage3.Controls.Add(this.label32);
            this.tabPage3.Controls.Add(this.debugLogCheck);
            this.tabPage3.Controls.Add(this.useNicksCheck);
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage3.Location = new System.Drawing.Point(4, 27);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage3.Size = new System.Drawing.Size(511, 240);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Settings";
            // 
            // offlineMod
            // 
            this.offlineMod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.offlineMod.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.offlineMod.FormattingEnabled = true;
            this.offlineMod.Items.AddRange(new object[] {
            "Off",
            "On"});
            this.offlineMod.Location = new System.Drawing.Point(399, 38);
            this.offlineMod.Margin = new System.Windows.Forms.Padding(0);
            this.offlineMod.MaxDropDownItems = 10;
            this.offlineMod.Name = "offlineMod";
            this.offlineMod.Size = new System.Drawing.Size(108, 21);
            this.offlineMod.TabIndex = 80;
            this.offlineMod.Visible = false;
            this.offlineMod.SelectedIndexChanged += new System.EventHandler(this.OfflineMod_SelectedIndexChanged);
            // 
            // OfflineModLabel
            // 
            this.OfflineModLabel.AutoSize = true;
            this.OfflineModLabel.BackColor = System.Drawing.Color.Transparent;
            this.OfflineModLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OfflineModLabel.Location = new System.Drawing.Point(272, 40);
            this.OfflineModLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.OfflineModLabel.Name = "OfflineModLabel";
            this.OfflineModLabel.Size = new System.Drawing.Size(125, 15);
            this.OfflineModLabel.TabIndex = 79;
            this.OfflineModLabel.Text = "Nucleus Ofline Mode:";
            this.OfflineModLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.OfflineModLabel.Visible = false;
            // 
            // cmb_EpicLang
            // 
            this.cmb_EpicLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_EpicLang.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_EpicLang.FormattingEnabled = true;
            this.cmb_EpicLang.Items.AddRange(new object[] {
            "Arabic",
            "Brazilian",
            "Bulgarian",
            "Chinese",
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
            "Norwegian",
            "Polish",
            "Portuguese",
            "Romanian",
            "Russian",
            "Spanish",
            "Swedish",
            "Thai",
            "Turkish",
            "Ukrainian"});
            this.cmb_EpicLang.Location = new System.Drawing.Point(125, 38);
            this.cmb_EpicLang.Margin = new System.Windows.Forms.Padding(0);
            this.cmb_EpicLang.MaxDropDownItems = 10;
            this.cmb_EpicLang.Name = "cmb_EpicLang";
            this.cmb_EpicLang.Size = new System.Drawing.Size(142, 21);
            this.cmb_EpicLang.TabIndex = 78;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.BackColor = System.Drawing.Color.Transparent;
            this.label46.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label46.Location = new System.Drawing.Point(4, 40);
            this.label46.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(119, 15);
            this.label46.TabIndex = 77;
            this.label46.Text = "Epic/Gog Language:";
            this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ignoreInputLockReminderCheckbox
            // 
            this.ignoreInputLockReminderCheckbox.AutoSize = true;
            this.ignoreInputLockReminderCheckbox.Location = new System.Drawing.Point(20, 146);
            this.ignoreInputLockReminderCheckbox.Margin = new System.Windows.Forms.Padding(2);
            this.ignoreInputLockReminderCheckbox.Name = "ignoreInputLockReminderCheckbox";
            this.ignoreInputLockReminderCheckbox.Size = new System.Drawing.Size(148, 17);
            this.ignoreInputLockReminderCheckbox.TabIndex = 76;
            this.ignoreInputLockReminderCheckbox.Text = "Ignore input lock reminder";
            this.ignoreInputLockReminderCheckbox.UseVisualStyleBackColor = true;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.BackColor = System.Drawing.Color.Transparent;
            this.label35.Location = new System.Drawing.Point(16, 190);
            this.label35.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(166, 13);
            this.label35.TabIndex = 75;
            this.label35.Text = "Nucleus User Account Password:";
            // 
            // nucUserPassTxt
            // 
            this.nucUserPassTxt.Location = new System.Drawing.Point(20, 207);
            this.nucUserPassTxt.Margin = new System.Windows.Forms.Padding(2);
            this.nucUserPassTxt.MaxLength = 127;
            this.nucUserPassTxt.Name = "nucUserPassTxt";
            this.nucUserPassTxt.PasswordChar = '*';
            this.nucUserPassTxt.Size = new System.Drawing.Size(160, 20);
            this.nucUserPassTxt.TabIndex = 74;
            // 
            // keepAccountsCheck
            // 
            this.keepAccountsCheck.AutoSize = true;
            this.keepAccountsCheck.Location = new System.Drawing.Point(20, 169);
            this.keepAccountsCheck.Margin = new System.Windows.Forms.Padding(2);
            this.keepAccountsCheck.Name = "keepAccountsCheck";
            this.keepAccountsCheck.Size = new System.Drawing.Size(166, 17);
            this.keepAccountsCheck.TabIndex = 73;
            this.keepAccountsCheck.Text = "Keep Nucleus User Accounts";
            this.keepAccountsCheck.UseVisualStyleBackColor = true;
            this.keepAccountsCheck.Click += new System.EventHandler(this.keepAccountsCheck_Click);
            // 
            // statusCheck
            // 
            this.statusCheck.AutoSize = true;
            this.statusCheck.Location = new System.Drawing.Point(20, 123);
            this.statusCheck.Margin = new System.Windows.Forms.Padding(2);
            this.statusCheck.Name = "statusCheck";
            this.statusCheck.Size = new System.Drawing.Size(197, 17);
            this.statusCheck.TabIndex = 72;
            this.statusCheck.Text = "Show Status Window (Experimental)";
            this.statusCheck.UseVisualStyleBackColor = true;
            // 
            // cmb_Lang
            // 
            this.cmb_Lang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Lang.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_Lang.FormattingEnabled = true;
            this.cmb_Lang.Items.AddRange(new object[] {
            "Automatic",
            "Arabic",
            "Brazilian",
            "Bulgarian",
            "Schinese",
            "Tchinese",
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
            "Norwegian",
            "Polish",
            "Portuguese",
            "Romanian",
            "Russian",
            "Spanish",
            "Swedish",
            "Thai",
            "Turkish",
            "Ukrainian"});
            this.cmb_Lang.Location = new System.Drawing.Point(125, 10);
            this.cmb_Lang.Margin = new System.Windows.Forms.Padding(0);
            this.cmb_Lang.MaxDropDownItems = 10;
            this.cmb_Lang.Name = "cmb_Lang";
            this.cmb_Lang.Size = new System.Drawing.Size(142, 21);
            this.cmb_Lang.TabIndex = 71;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.BackColor = System.Drawing.Color.Transparent;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.Location = new System.Drawing.Point(4, 12);
            this.label34.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(105, 15);
            this.label34.TabIndex = 70;
            this.label34.Text = "Steam Language:";
            this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmb_Network
            // 
            this.cmb_Network.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_Network.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Network.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_Network.FormattingEnabled = true;
            this.cmb_Network.ItemHeight = 13;
            this.cmb_Network.Items.AddRange(new object[] {
            "Automatic"});
            this.cmb_Network.Location = new System.Drawing.Point(320, 10);
            this.cmb_Network.Margin = new System.Windows.Forms.Padding(0);
            this.cmb_Network.MaxDropDownItems = 10;
            this.cmb_Network.Name = "cmb_Network";
            this.cmb_Network.Size = new System.Drawing.Size(187, 21);
            this.cmb_Network.TabIndex = 69;
            // 
            // label32
            // 
            this.label32.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(272, 14);
            this.label32.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(50, 13);
            this.label32.TabIndex = 68;
            this.label32.Text = "Network:";
            this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // debugLogCheck
            // 
            this.debugLogCheck.AutoSize = true;
            this.debugLogCheck.BackColor = System.Drawing.Color.Transparent;
            this.debugLogCheck.Location = new System.Drawing.Point(20, 100);
            this.debugLogCheck.Margin = new System.Windows.Forms.Padding(2);
            this.debugLogCheck.Name = "debugLogCheck";
            this.debugLogCheck.Size = new System.Drawing.Size(115, 17);
            this.debugLogCheck.TabIndex = 66;
            this.debugLogCheck.Text = "Enable Debug Log";
            this.debugLogCheck.UseVisualStyleBackColor = false;
            // 
            // useNicksCheck
            // 
            this.useNicksCheck.AutoSize = true;
            this.useNicksCheck.BackColor = System.Drawing.Color.Transparent;
            this.useNicksCheck.Location = new System.Drawing.Point(20, 77);
            this.useNicksCheck.Margin = new System.Windows.Forms.Padding(2);
            this.useNicksCheck.Name = "useNicksCheck";
            this.useNicksCheck.Size = new System.Drawing.Size(144, 17);
            this.useNicksCheck.TabIndex = 67;
            this.useNicksCheck.Text = "Use Nicknames In-Game";
            this.useNicksCheck.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.comboBox_lockKey);
            this.groupBox1.Controls.Add(this.label_lockKey);
            this.groupBox1.Controls.Add(this.label38);
            this.groupBox1.Controls.Add(this.settingsTopCmb);
            this.groupBox1.Controls.Add(this.settingsStopCmb);
            this.groupBox1.Controls.Add(this.settingsTopTxt);
            this.groupBox1.Controls.Add(this.settingsStopTxt);
            this.groupBox1.Controls.Add(this.settingsCloseCmb);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.settingsCloseHKTxt);
            this.groupBox1.Location = new System.Drawing.Point(271, 77);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(236, 122);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            // 
            // comboBox_lockKey
            // 
            this.comboBox_lockKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_lockKey.FormattingEnabled = true;
            this.comboBox_lockKey.Items.AddRange(new object[] {
            "Default(End key)",
            "Home",
            "Delete",
            "Multiply",
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "F10",
            "F11",
            "F12",
            "+",
            "-",
            "Numpad 0",
            "Numpad 1",
            "Numpad 2",
            "Numpad 3",
            "Numpad 4",
            "Numpad 5",
            "Numpad 6",
            "Numpad 7",
            "Numpad 8",
            "Numpad 9"});
            this.comboBox_lockKey.Location = new System.Drawing.Point(102, 89);
            this.comboBox_lockKey.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_lockKey.Name = "comboBox_lockKey";
            this.comboBox_lockKey.Size = new System.Drawing.Size(123, 21);
            this.comboBox_lockKey.TabIndex = 30;
            this.comboBox_lockKey.SelectedIndexChanged += new System.EventHandler(this.comboBox_lockKey_SelectedIndexChanged);
            // 
            // label_lockKey
            // 
            this.label_lockKey.Location = new System.Drawing.Point(8, 90);
            this.label_lockKey.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_lockKey.Name = "label_lockKey";
            this.label_lockKey.Size = new System.Drawing.Size(86, 15);
            this.label_lockKey.TabIndex = 29;
            this.label_lockKey.Text = "Lock Input Key:";
            this.label_lockKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label38.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label38.Location = new System.Drawing.Point(1, 7);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(57, 15);
            this.label38.TabIndex = 28;
            this.label38.Text = "Hotkeys ";
            // 
            // settingsTopCmb
            // 
            this.settingsTopCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.settingsTopCmb.FormattingEnabled = true;
            this.settingsTopCmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.settingsTopCmb.Location = new System.Drawing.Point(102, 67);
            this.settingsTopCmb.Margin = new System.Windows.Forms.Padding(2);
            this.settingsTopCmb.Name = "settingsTopCmb";
            this.settingsTopCmb.Size = new System.Drawing.Size(67, 21);
            this.settingsTopCmb.TabIndex = 5;
            // 
            // settingsStopCmb
            // 
            this.settingsStopCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.settingsStopCmb.FormattingEnabled = true;
            this.settingsStopCmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.settingsStopCmb.Location = new System.Drawing.Point(102, 45);
            this.settingsStopCmb.Margin = new System.Windows.Forms.Padding(2);
            this.settingsStopCmb.Name = "settingsStopCmb";
            this.settingsStopCmb.Size = new System.Drawing.Size(67, 21);
            this.settingsStopCmb.TabIndex = 3;
            // 
            // settingsTopTxt
            // 
            this.settingsTopTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.settingsTopTxt.Location = new System.Drawing.Point(189, 68);
            this.settingsTopTxt.Margin = new System.Windows.Forms.Padding(2);
            this.settingsTopTxt.MaxLength = 1;
            this.settingsTopTxt.Name = "settingsTopTxt";
            this.settingsTopTxt.ShortcutsEnabled = false;
            this.settingsTopTxt.Size = new System.Drawing.Size(36, 20);
            this.settingsTopTxt.TabIndex = 6;
            this.settingsTopTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // settingsStopTxt
            // 
            this.settingsStopTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.settingsStopTxt.Location = new System.Drawing.Point(189, 46);
            this.settingsStopTxt.Margin = new System.Windows.Forms.Padding(2);
            this.settingsStopTxt.MaxLength = 1;
            this.settingsStopTxt.Name = "settingsStopTxt";
            this.settingsStopTxt.ShortcutsEnabled = false;
            this.settingsStopTxt.Size = new System.Drawing.Size(36, 20);
            this.settingsStopTxt.TabIndex = 4;
            this.settingsStopTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // settingsCloseCmb
            // 
            this.settingsCloseCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.settingsCloseCmb.FormattingEnabled = true;
            this.settingsCloseCmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.settingsCloseCmb.Location = new System.Drawing.Point(102, 23);
            this.settingsCloseCmb.Margin = new System.Windows.Forms.Padding(2);
            this.settingsCloseCmb.Name = "settingsCloseCmb";
            this.settingsCloseCmb.Size = new System.Drawing.Size(67, 21);
            this.settingsCloseCmb.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(172, 69);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 13);
            this.label6.TabIndex = 27;
            this.label6.Text = "+";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(172, 47);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(13, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "+";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(172, 26);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "+";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(0, 69);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 15);
            this.label4.TabIndex = 24;
            this.label4.Text = "Toggle Top Most:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(14, 46);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 15);
            this.label3.TabIndex = 22;
            this.label3.Text = "Stop Session:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(11, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 15);
            this.label1.TabIndex = 20;
            this.label1.Text = "Close Nucleus:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // settingsCloseHKTxt
            // 
            this.settingsCloseHKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.settingsCloseHKTxt.Location = new System.Drawing.Point(189, 24);
            this.settingsCloseHKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.settingsCloseHKTxt.MaxLength = 1;
            this.settingsCloseHKTxt.Name = "settingsCloseHKTxt";
            this.settingsCloseHKTxt.ShortcutsEnabled = false;
            this.settingsCloseHKTxt.Size = new System.Drawing.Size(36, 20);
            this.settingsCloseHKTxt.TabIndex = 2;
            this.settingsCloseHKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.Transparent;
            this.tabPage4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tabPage4.Controls.Add(this.label31);
            this.tabPage4.Controls.Add(this.SplitDiv);
            this.tabPage4.Controls.Add(this.label49);
            this.tabPage4.Controls.Add(this.SplitColors);
            this.tabPage4.Controls.Add(this.label27);
            this.tabPage4.Controls.Add(this.label29);
            this.tabPage4.Controls.Add(this.label28);
            this.tabPage4.Controls.Add(this.panel2);
            this.tabPage4.Controls.Add(this.layoutSizer);
            this.tabPage4.Controls.Add(this.label30);
            this.tabPage4.Controls.Add(this.enableCustomCheckbox);
            this.tabPage4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage4.Location = new System.Drawing.Point(4, 27);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage4.Size = new System.Drawing.Size(511, 240);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Custom Layout";
            this.tabPage4.Click += new System.EventHandler(this.tabPage4_Click);
            // 
            // label31
            // 
            this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label31.BackColor = System.Drawing.Color.Transparent;
            this.label31.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label31.ForeColor = System.Drawing.Color.Red;
            this.label31.Location = new System.Drawing.Point(1, 165);
            this.label31.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(237, 15);
            this.label31.TabIndex = 86;
            this.label31.Text = "Splitscreen division may not work for all games.";
            this.label31.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label31.UseMnemonic = false;
            // 
            // SplitDiv
            // 
            this.SplitDiv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SplitDiv.AutoSize = true;
            this.SplitDiv.Cursor = System.Windows.Forms.Cursors.Default;
            this.SplitDiv.Location = new System.Drawing.Point(4, 118);
            this.SplitDiv.Margin = new System.Windows.Forms.Padding(2);
            this.SplitDiv.Name = "SplitDiv";
            this.SplitDiv.Size = new System.Drawing.Size(116, 17);
            this.SplitDiv.TabIndex = 85;
            this.SplitDiv.Text = "Splitscreen division";
            this.SplitDiv.UseVisualStyleBackColor = true;
            this.SplitDiv.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label49
            // 
            this.label49.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label49.AutoSize = true;
            this.label49.BackColor = System.Drawing.Color.Transparent;
            this.label49.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label49.Location = new System.Drawing.Point(1, 137);
            this.label49.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(106, 15);
            this.label49.TabIndex = 84;
            this.label49.Text = "Background color:";
            this.label49.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SplitColors
            // 
            this.SplitColors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SplitColors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SplitColors.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SplitColors.FormattingEnabled = true;
            this.SplitColors.Items.AddRange(new object[] {
            "Black",
            "Gray",
            "White",
            "Dark Blue",
            "Blue",
            "Purple",
            "Pink",
            "Red",
            "Orange",
            "Yellow    ",
            "Green"});
            this.SplitColors.Location = new System.Drawing.Point(112, 137);
            this.SplitColors.Margin = new System.Windows.Forms.Padding(0);
            this.SplitColors.MaxDropDownItems = 10;
            this.SplitColors.Name = "SplitColors";
            this.SplitColors.Size = new System.Drawing.Size(78, 21);
            this.SplitColors.TabIndex = 82;
            this.SplitColors.SelectedIndexChanged += new System.EventHandler(this.SplitColors_SelectedIndexChanged);
            // 
            // label27
            // 
            this.label27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label27.AutoSize = true;
            this.label27.BackColor = System.Drawing.Color.Transparent;
            this.label27.Location = new System.Drawing.Point(2, 65);
            this.label27.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(124, 13);
            this.label27.TabIndex = 28;
            this.label27.Text = "# of Horizontal Divisions:";
            this.label27.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label29
            // 
            this.label29.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label29.AutoSize = true;
            this.label29.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label29.Location = new System.Drawing.Point(59, 82);
            this.label29.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(67, 13);
            this.label29.TabIndex = 30;
            this.label29.Text = "Max Players:";
            this.label29.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label28
            // 
            this.label28.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(16, 48);
            this.label28.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(112, 13);
            this.label28.TabIndex = 29;
            this.label28.Text = "# of Vertical Divisions:";
            this.label28.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.numVerDiv);
            this.panel2.Controls.Add(this.numHorDiv);
            this.panel2.Controls.Add(this.numMaxPlyrs);
            this.panel2.Location = new System.Drawing.Point(135, 48);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(55, 56);
            this.panel2.TabIndex = 39;
            // 
            // numVerDiv
            // 
            this.numVerDiv.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numVerDiv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numVerDiv.Location = new System.Drawing.Point(9, 2);
            this.numVerDiv.Margin = new System.Windows.Forms.Padding(2);
            this.numVerDiv.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numVerDiv.Name = "numVerDiv";
            this.numVerDiv.ReadOnly = true;
            this.numVerDiv.Size = new System.Drawing.Size(46, 16);
            this.numVerDiv.TabIndex = 32;
            this.numVerDiv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numVerDiv.ValueChanged += new System.EventHandler(this.NumVerDiv_ValueChanged);
            // 
            // numHorDiv
            // 
            this.numHorDiv.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numHorDiv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numHorDiv.Location = new System.Drawing.Point(9, 20);
            this.numHorDiv.Margin = new System.Windows.Forms.Padding(2);
            this.numHorDiv.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numHorDiv.Name = "numHorDiv";
            this.numHorDiv.ReadOnly = true;
            this.numHorDiv.Size = new System.Drawing.Size(46, 16);
            this.numHorDiv.TabIndex = 31;
            this.numHorDiv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numHorDiv.ValueChanged += new System.EventHandler(this.NumHorDiv_ValueChanged);
            // 
            // numMaxPlyrs
            // 
            this.numMaxPlyrs.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numMaxPlyrs.BackColor = System.Drawing.SystemColors.ControlDark;
            this.numMaxPlyrs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numMaxPlyrs.Enabled = false;
            this.numMaxPlyrs.Location = new System.Drawing.Point(9, 38);
            this.numMaxPlyrs.Margin = new System.Windows.Forms.Padding(2);
            this.numMaxPlyrs.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numMaxPlyrs.Name = "numMaxPlyrs";
            this.numMaxPlyrs.ReadOnly = true;
            this.numMaxPlyrs.Size = new System.Drawing.Size(46, 16);
            this.numMaxPlyrs.TabIndex = 33;
            this.numMaxPlyrs.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMaxPlyrs.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // layoutSizer
            // 
            this.layoutSizer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutSizer.BackColor = System.Drawing.Color.Transparent;
            this.layoutSizer.Location = new System.Drawing.Point(238, 28);
            this.layoutSizer.MaximumSize = new System.Drawing.Size(245, 170);
            this.layoutSizer.MinimumSize = new System.Drawing.Size(245, 170);
            this.layoutSizer.Name = "layoutSizer";
            this.layoutSizer.Size = new System.Drawing.Size(245, 170);
            this.layoutSizer.TabIndex = 36;
            this.layoutSizer.Paint += new System.Windows.Forms.PaintEventHandler(this.layoutSizer_Paint);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(235, 4);
            this.label30.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(48, 13);
            this.label30.TabIndex = 35;
            this.label30.Text = "Preview:";
            this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // enableCustomCheckbox
            // 
            this.enableCustomCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.enableCustomCheckbox.AutoSize = true;
            this.enableCustomCheckbox.Checked = true;
            this.enableCustomCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableCustomCheckbox.Enabled = false;
            this.enableCustomCheckbox.Location = new System.Drawing.Point(4, 4);
            this.enableCustomCheckbox.Margin = new System.Windows.Forms.Padding(2);
            this.enableCustomCheckbox.Name = "enableCustomCheckbox";
            this.enableCustomCheckbox.Size = new System.Drawing.Size(132, 17);
            this.enableCustomCheckbox.TabIndex = 0;
            this.enableCustomCheckbox.Text = "Enable Custom Layout";
            this.enableCustomCheckbox.UseVisualStyleBackColor = true;
            this.enableCustomCheckbox.Visible = false;
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.Color.Transparent;
            this.tabPage5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tabPage5.Controls.Add(this.label21);
            this.tabPage5.Controls.Add(this.label25);
            this.tabPage5.Controls.Add(this.btn_Refresh);
            this.tabPage5.Controls.Add(this.controllerSixteenNick);
            this.tabPage5.Controls.Add(this.label17);
            this.tabPage5.Controls.Add(this.controllerFifteenNick);
            this.tabPage5.Controls.Add(this.label18);
            this.tabPage5.Controls.Add(this.controllerFourteenNick);
            this.tabPage5.Controls.Add(this.label19);
            this.tabPage5.Controls.Add(this.controllerThirteenNick);
            this.tabPage5.Controls.Add(this.label20);
            this.tabPage5.Controls.Add(this.controllerTwelveNick);
            this.tabPage5.Controls.Add(this.label22);
            this.tabPage5.Controls.Add(this.controllerElevenNick);
            this.tabPage5.Controls.Add(this.label23);
            this.tabPage5.Controls.Add(this.controllerTenNick);
            this.tabPage5.Controls.Add(this.label24);
            this.tabPage5.Controls.Add(this.controllerNineNick);
            this.tabPage5.Controls.Add(this.label26);
            this.tabPage5.Controls.Add(this.controllerEightNick);
            this.tabPage5.Controls.Add(this.label16);
            this.tabPage5.Controls.Add(this.controllerSevenNick);
            this.tabPage5.Controls.Add(this.label15);
            this.tabPage5.Controls.Add(this.controllerSixNick);
            this.tabPage5.Controls.Add(this.label11);
            this.tabPage5.Controls.Add(this.controllerFiveNick);
            this.tabPage5.Controls.Add(this.label9);
            this.tabPage5.Controls.Add(this.label13);
            this.tabPage5.Controls.Add(this.controllerFourNick);
            this.tabPage5.Controls.Add(this.label14);
            this.tabPage5.Controls.Add(this.controllerThreeNick);
            this.tabPage5.Controls.Add(this.label12);
            this.tabPage5.Controls.Add(this.controllerTwoNick);
            this.tabPage5.Controls.Add(this.label10);
            this.tabPage5.Controls.Add(this.label8);
            this.tabPage5.Controls.Add(this.controllerOneNick);
            this.tabPage5.Controls.Add(this.label7);
            this.tabPage5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage5.Location = new System.Drawing.Point(4, 27);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(511, 240);
            this.tabPage5.TabIndex = 2;
            this.tabPage5.Text = "Nicknames";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(259, 12);
            this.label21.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(50, 13);
            this.label21.TabIndex = 109;
            this.label21.Text = "Player ID";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(342, 12);
            this.label25.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(55, 13);
            this.label25.TabIndex = 108;
            this.label25.Text = "Nickname";
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Refresh.Location = new System.Drawing.Point(215, 209);
            this.btn_Refresh.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(80, 23);
            this.btn_Refresh.TabIndex = 16;
            this.btn_Refresh.Text = "Refresh";
            this.btn_Refresh.Click += new System.EventHandler(this.Btn_Refresh_Click);
            // 
            // controllerSixteenNick
            // 
            this.controllerSixteenNick.BackColor = System.Drawing.Color.White;
            this.controllerSixteenNick.Location = new System.Drawing.Point(345, 169);
            this.controllerSixteenNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerSixteenNick.MaxLength = 9;
            this.controllerSixteenNick.Name = "controllerSixteenNick";
            this.controllerSixteenNick.Size = new System.Drawing.Size(69, 20);
            this.controllerSixteenNick.TabIndex = 105;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(259, 170);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(57, 13);
            this.label17.TabIndex = 106;
            this.label17.Text = "Player 16 :";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerFifteenNick
            // 
            this.controllerFifteenNick.BackColor = System.Drawing.Color.White;
            this.controllerFifteenNick.Location = new System.Drawing.Point(345, 150);
            this.controllerFifteenNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerFifteenNick.MaxLength = 9;
            this.controllerFifteenNick.Name = "controllerFifteenNick";
            this.controllerFifteenNick.Size = new System.Drawing.Size(69, 20);
            this.controllerFifteenNick.TabIndex = 102;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(259, 150);
            this.label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(57, 13);
            this.label18.TabIndex = 103;
            this.label18.Text = "Player 15 :";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerFourteenNick
            // 
            this.controllerFourteenNick.BackColor = System.Drawing.Color.White;
            this.controllerFourteenNick.Location = new System.Drawing.Point(345, 131);
            this.controllerFourteenNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerFourteenNick.MaxLength = 9;
            this.controllerFourteenNick.Name = "controllerFourteenNick";
            this.controllerFourteenNick.Size = new System.Drawing.Size(69, 20);
            this.controllerFourteenNick.TabIndex = 99;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(259, 131);
            this.label19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(57, 13);
            this.label19.TabIndex = 100;
            this.label19.Text = "Player 14 :";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerThirteenNick
            // 
            this.controllerThirteenNick.BackColor = System.Drawing.Color.White;
            this.controllerThirteenNick.Location = new System.Drawing.Point(345, 112);
            this.controllerThirteenNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerThirteenNick.MaxLength = 9;
            this.controllerThirteenNick.Name = "controllerThirteenNick";
            this.controllerThirteenNick.Size = new System.Drawing.Size(69, 20);
            this.controllerThirteenNick.TabIndex = 96;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(259, 112);
            this.label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(57, 13);
            this.label20.TabIndex = 97;
            this.label20.Text = "Player 13 :";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerTwelveNick
            // 
            this.controllerTwelveNick.BackColor = System.Drawing.Color.White;
            this.controllerTwelveNick.Location = new System.Drawing.Point(345, 93);
            this.controllerTwelveNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerTwelveNick.MaxLength = 9;
            this.controllerTwelveNick.Name = "controllerTwelveNick";
            this.controllerTwelveNick.Size = new System.Drawing.Size(69, 20);
            this.controllerTwelveNick.TabIndex = 92;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(259, 93);
            this.label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(57, 13);
            this.label22.TabIndex = 93;
            this.label22.Text = "Player 12 :";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerElevenNick
            // 
            this.controllerElevenNick.BackColor = System.Drawing.Color.White;
            this.controllerElevenNick.Location = new System.Drawing.Point(345, 74);
            this.controllerElevenNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerElevenNick.MaxLength = 9;
            this.controllerElevenNick.Name = "controllerElevenNick";
            this.controllerElevenNick.Size = new System.Drawing.Size(69, 20);
            this.controllerElevenNick.TabIndex = 91;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(259, 74);
            this.label23.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(57, 13);
            this.label23.TabIndex = 89;
            this.label23.Text = "Player 11 :";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerTenNick
            // 
            this.controllerTenNick.BackColor = System.Drawing.Color.White;
            this.controllerTenNick.Location = new System.Drawing.Point(345, 55);
            this.controllerTenNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerTenNick.MaxLength = 9;
            this.controllerTenNick.Name = "controllerTenNick";
            this.controllerTenNick.Size = new System.Drawing.Size(69, 20);
            this.controllerTenNick.TabIndex = 88;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(259, 54);
            this.label24.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(57, 13);
            this.label24.TabIndex = 85;
            this.label24.Text = "Player 10 :";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerNineNick
            // 
            this.controllerNineNick.BackColor = System.Drawing.Color.White;
            this.controllerNineNick.Location = new System.Drawing.Point(345, 36);
            this.controllerNineNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerNineNick.MaxLength = 9;
            this.controllerNineNick.Name = "controllerNineNick";
            this.controllerNineNick.Size = new System.Drawing.Size(69, 20);
            this.controllerNineNick.TabIndex = 87;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(265, 36);
            this.label26.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(51, 13);
            this.label26.TabIndex = 82;
            this.label26.Text = "Player 9 :";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerEightNick
            // 
            this.controllerEightNick.BackColor = System.Drawing.Color.White;
            this.controllerEightNick.Location = new System.Drawing.Point(90, 166);
            this.controllerEightNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerEightNick.MaxLength = 9;
            this.controllerEightNick.Name = "controllerEightNick";
            this.controllerEightNick.Size = new System.Drawing.Size(69, 20);
            this.controllerEightNick.TabIndex = 79;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(4, 170);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(51, 13);
            this.label16.TabIndex = 80;
            this.label16.Text = "Player 8 :";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerSevenNick
            // 
            this.controllerSevenNick.BackColor = System.Drawing.Color.White;
            this.controllerSevenNick.Location = new System.Drawing.Point(90, 147);
            this.controllerSevenNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerSevenNick.MaxLength = 9;
            this.controllerSevenNick.Name = "controllerSevenNick";
            this.controllerSevenNick.Size = new System.Drawing.Size(69, 20);
            this.controllerSevenNick.TabIndex = 76;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(4, 150);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(51, 13);
            this.label15.TabIndex = 77;
            this.label15.Text = "Player 7 :";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerSixNick
            // 
            this.controllerSixNick.BackColor = System.Drawing.Color.White;
            this.controllerSixNick.Location = new System.Drawing.Point(90, 128);
            this.controllerSixNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerSixNick.MaxLength = 9;
            this.controllerSixNick.Name = "controllerSixNick";
            this.controllerSixNick.Size = new System.Drawing.Size(69, 20);
            this.controllerSixNick.TabIndex = 73;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 131);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 74;
            this.label11.Text = "Player 6 :";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerFiveNick
            // 
            this.controllerFiveNick.BackColor = System.Drawing.Color.White;
            this.controllerFiveNick.Location = new System.Drawing.Point(90, 109);
            this.controllerFiveNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerFiveNick.MaxLength = 9;
            this.controllerFiveNick.Name = "controllerFiveNick";
            this.controllerFiveNick.Size = new System.Drawing.Size(69, 20);
            this.controllerFiveNick.TabIndex = 70;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 112);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 13);
            this.label9.TabIndex = 71;
            this.label9.Text = "Player 5 :";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(4, 12);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 13);
            this.label13.TabIndex = 69;
            this.label13.Text = "Player ID";
            // 
            // controllerFourNick
            // 
            this.controllerFourNick.BackColor = System.Drawing.Color.White;
            this.controllerFourNick.Location = new System.Drawing.Point(90, 90);
            this.controllerFourNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerFourNick.MaxLength = 9;
            this.controllerFourNick.Name = "controllerFourNick";
            this.controllerFourNick.Size = new System.Drawing.Size(69, 20);
            this.controllerFourNick.TabIndex = 66;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(4, 93);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(51, 13);
            this.label14.TabIndex = 67;
            this.label14.Text = "Player 4 :";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerThreeNick
            // 
            this.controllerThreeNick.BackColor = System.Drawing.Color.White;
            this.controllerThreeNick.Location = new System.Drawing.Point(90, 71);
            this.controllerThreeNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerThreeNick.MaxLength = 9;
            this.controllerThreeNick.Name = "controllerThreeNick";
            this.controllerThreeNick.Size = new System.Drawing.Size(69, 20);
            this.controllerThreeNick.TabIndex = 65;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(4, 74);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 13);
            this.label12.TabIndex = 63;
            this.label12.Text = "Player 3 :";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // controllerTwoNick
            // 
            this.controllerTwoNick.BackColor = System.Drawing.Color.White;
            this.controllerTwoNick.Location = new System.Drawing.Point(90, 52);
            this.controllerTwoNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerTwoNick.MaxLength = 9;
            this.controllerTwoNick.Name = "controllerTwoNick";
            this.controllerTwoNick.Size = new System.Drawing.Size(69, 20);
            this.controllerTwoNick.TabIndex = 62;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 54);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(51, 13);
            this.label10.TabIndex = 59;
            this.label10.Text = "Player 2 :";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(87, 12);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 58;
            this.label8.Text = "Nickname";
            // 
            // controllerOneNick
            // 
            this.controllerOneNick.BackColor = System.Drawing.Color.White;
            this.controllerOneNick.Location = new System.Drawing.Point(90, 33);
            this.controllerOneNick.Margin = new System.Windows.Forms.Padding(2);
            this.controllerOneNick.MaxLength = 9;
            this.controllerOneNick.Name = "controllerOneNick";
            this.controllerOneNick.Size = new System.Drawing.Size(69, 20);
            this.controllerOneNick.TabIndex = 61;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 35);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(51, 13);
            this.label7.TabIndex = 56;
            this.label7.Text = "Player 1 :";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Black;
            this.tabPage1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tabPage1.Controls.Add(this.label39);
            this.tabPage1.Controls.Add(this.audioDefaultDevice);
            this.tabPage1.Controls.Add(this.audioCustomSettingsBox);
            this.tabPage1.Controls.Add(this.audioCustomSettingsRadio);
            this.tabPage1.Controls.Add(this.audioDefaultSettingsRadio);
            this.tabPage1.Controls.Add(this.audioRefresh);
            this.tabPage1.Location = new System.Drawing.Point(4, 27);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(511, 240);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "Audio";
            // 
            // label39
            // 
            this.label39.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label39.BackColor = System.Drawing.Color.Transparent;
            this.label39.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label39.ForeColor = System.Drawing.Color.Red;
            this.label39.Location = new System.Drawing.Point(281, 37);
            this.label39.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(226, 15);
            this.label39.TabIndex = 7;
            this.label39.Text = "Note: this feature may not work for all games";
            this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label39.UseMnemonic = false;
            // 
            // audioDefaultDevice
            // 
            this.audioDefaultDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioDefaultDevice.BackColor = System.Drawing.Color.Transparent;
            this.audioDefaultDevice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.audioDefaultDevice.ForeColor = System.Drawing.Color.White;
            this.audioDefaultDevice.Location = new System.Drawing.Point(281, 14);
            this.audioDefaultDevice.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.audioDefaultDevice.Name = "audioDefaultDevice";
            this.audioDefaultDevice.Size = new System.Drawing.Size(226, 15);
            this.audioDefaultDevice.TabIndex = 6;
            this.audioDefaultDevice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // audioCustomSettingsBox
            // 
            this.audioCustomSettingsBox.BackColor = System.Drawing.Color.Transparent;
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance8);
            this.audioCustomSettingsBox.Controls.Add(this.label45);
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance7);
            this.audioCustomSettingsBox.Controls.Add(this.label44);
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance6);
            this.audioCustomSettingsBox.Controls.Add(this.label43);
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance5);
            this.audioCustomSettingsBox.Controls.Add(this.label42);
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance4);
            this.audioCustomSettingsBox.Controls.Add(this.label41);
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance3);
            this.audioCustomSettingsBox.Controls.Add(this.label40);
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance2);
            this.audioCustomSettingsBox.Controls.Add(this.AudioInstance1);
            this.audioCustomSettingsBox.Controls.Add(this.label37);
            this.audioCustomSettingsBox.Controls.Add(this.label36);
            this.audioCustomSettingsBox.Location = new System.Drawing.Point(4, 54);
            this.audioCustomSettingsBox.Margin = new System.Windows.Forms.Padding(2);
            this.audioCustomSettingsBox.Name = "audioCustomSettingsBox";
            this.audioCustomSettingsBox.Padding = new System.Windows.Forms.Padding(2);
            this.audioCustomSettingsBox.Size = new System.Drawing.Size(503, 151);
            this.audioCustomSettingsBox.TabIndex = 4;
            this.audioCustomSettingsBox.TabStop = false;
            // 
            // AudioInstance8
            // 
            this.AudioInstance8.BackColor = System.Drawing.Color.White;
            this.AudioInstance8.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance8.FormattingEnabled = true;
            this.AudioInstance8.Location = new System.Drawing.Point(316, 84);
            this.AudioInstance8.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance8.Name = "AudioInstance8";
            this.AudioInstance8.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance8.TabIndex = 29;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(257, 85);
            this.label45.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(60, 13);
            this.label45.TabIndex = 28;
            this.label45.Text = "Instance 8:";
            // 
            // AudioInstance7
            // 
            this.AudioInstance7.BackColor = System.Drawing.Color.White;
            this.AudioInstance7.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance7.FormattingEnabled = true;
            this.AudioInstance7.Location = new System.Drawing.Point(316, 62);
            this.AudioInstance7.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance7.Name = "AudioInstance7";
            this.AudioInstance7.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance7.TabIndex = 27;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(257, 64);
            this.label44.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(60, 13);
            this.label44.TabIndex = 26;
            this.label44.Text = "Instance 7:";
            // 
            // AudioInstance6
            // 
            this.AudioInstance6.BackColor = System.Drawing.Color.White;
            this.AudioInstance6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance6.FormattingEnabled = true;
            this.AudioInstance6.Location = new System.Drawing.Point(316, 40);
            this.AudioInstance6.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance6.Name = "AudioInstance6";
            this.AudioInstance6.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance6.TabIndex = 25;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(257, 42);
            this.label43.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(60, 13);
            this.label43.TabIndex = 24;
            this.label43.Text = "Instance 6:";
            // 
            // AudioInstance5
            // 
            this.AudioInstance5.BackColor = System.Drawing.Color.White;
            this.AudioInstance5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance5.FormattingEnabled = true;
            this.AudioInstance5.Location = new System.Drawing.Point(316, 18);
            this.AudioInstance5.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance5.Name = "AudioInstance5";
            this.AudioInstance5.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance5.TabIndex = 23;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(257, 20);
            this.label42.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(60, 13);
            this.label42.TabIndex = 22;
            this.label42.Text = "Instance 5:";
            // 
            // AudioInstance4
            // 
            this.AudioInstance4.BackColor = System.Drawing.Color.White;
            this.AudioInstance4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance4.FormattingEnabled = true;
            this.AudioInstance4.Location = new System.Drawing.Point(65, 84);
            this.AudioInstance4.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance4.Name = "AudioInstance4";
            this.AudioInstance4.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance4.TabIndex = 21;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(6, 86);
            this.label41.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(60, 13);
            this.label41.TabIndex = 20;
            this.label41.Text = "Instance 4:";
            // 
            // AudioInstance3
            // 
            this.AudioInstance3.BackColor = System.Drawing.Color.White;
            this.AudioInstance3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance3.FormattingEnabled = true;
            this.AudioInstance3.Location = new System.Drawing.Point(65, 62);
            this.AudioInstance3.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance3.Name = "AudioInstance3";
            this.AudioInstance3.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance3.TabIndex = 19;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(6, 65);
            this.label40.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(60, 13);
            this.label40.TabIndex = 18;
            this.label40.Text = "Instance 3:";
            // 
            // AudioInstance2
            // 
            this.AudioInstance2.BackColor = System.Drawing.Color.White;
            this.AudioInstance2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance2.FormattingEnabled = true;
            this.AudioInstance2.Location = new System.Drawing.Point(65, 40);
            this.AudioInstance2.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance2.Name = "AudioInstance2";
            this.AudioInstance2.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance2.TabIndex = 6;
            this.AudioInstance2.DropDown += new System.EventHandler(this.audioBox_DropDown);
            // 
            // AudioInstance1
            // 
            this.AudioInstance1.BackColor = System.Drawing.Color.White;
            this.AudioInstance1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance1.FormattingEnabled = true;
            this.AudioInstance1.Items.AddRange(new object[] {
            "Default"});
            this.AudioInstance1.Location = new System.Drawing.Point(65, 18);
            this.AudioInstance1.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance1.Name = "AudioInstance1";
            this.AudioInstance1.Size = new System.Drawing.Size(170, 21);
            this.AudioInstance1.TabIndex = 1;
            this.AudioInstance1.DropDown += new System.EventHandler(this.audioBox_DropDown);
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(6, 43);
            this.label37.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(60, 13);
            this.label37.TabIndex = 5;
            this.label37.Text = "Instance 2:";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(6, 21);
            this.label36.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(60, 13);
            this.label36.TabIndex = 0;
            this.label36.Text = "Instance 1:";
            // 
            // audioCustomSettingsRadio
            // 
            this.audioCustomSettingsRadio.AutoSize = true;
            this.audioCustomSettingsRadio.BackColor = System.Drawing.Color.Transparent;
            this.audioCustomSettingsRadio.Location = new System.Drawing.Point(6, 32);
            this.audioCustomSettingsRadio.Margin = new System.Windows.Forms.Padding(2);
            this.audioCustomSettingsRadio.Name = "audioCustomSettingsRadio";
            this.audioCustomSettingsRadio.Size = new System.Drawing.Size(99, 17);
            this.audioCustomSettingsRadio.TabIndex = 3;
            this.audioCustomSettingsRadio.Text = "Custom settings";
            this.audioCustomSettingsRadio.UseVisualStyleBackColor = false;
            this.audioCustomSettingsRadio.CheckedChanged += new System.EventHandler(this.audioCustomSettingsRadio_CheckedChanged);
            // 
            // audioDefaultSettingsRadio
            // 
            this.audioDefaultSettingsRadio.AutoSize = true;
            this.audioDefaultSettingsRadio.BackColor = System.Drawing.Color.Transparent;
            this.audioDefaultSettingsRadio.Checked = true;
            this.audioDefaultSettingsRadio.Location = new System.Drawing.Point(6, 12);
            this.audioDefaultSettingsRadio.Margin = new System.Windows.Forms.Padding(2);
            this.audioDefaultSettingsRadio.Name = "audioDefaultSettingsRadio";
            this.audioDefaultSettingsRadio.Size = new System.Drawing.Size(217, 17);
            this.audioDefaultSettingsRadio.TabIndex = 2;
            this.audioDefaultSettingsRadio.TabStop = true;
            this.audioDefaultSettingsRadio.Text = "Use default audio playback device for all";
            this.audioDefaultSettingsRadio.UseVisualStyleBackColor = false;
            // 
            // audioRefresh
            // 
            this.audioRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.audioRefresh.Location = new System.Drawing.Point(215, 209);
            this.audioRefresh.Margin = new System.Windows.Forms.Padding(2);
            this.audioRefresh.Name = "audioRefresh";
            this.audioRefresh.Size = new System.Drawing.Size(80, 23);
            this.audioRefresh.TabIndex = 17;
            this.audioRefresh.Text = "Refresh";
            this.audioRefresh.Click += new System.EventHandler(this.audioRefresh_Click);
            // 
            // label33
            // 
            this.label33.BackColor = System.Drawing.Color.Transparent;
            this.label33.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.label33.Cursor = System.Windows.Forms.Cursors.Default;
            this.label33.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label33.Location = new System.Drawing.Point(0, 0);
            this.label33.Margin = new System.Windows.Forms.Padding(0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(248, 15);
            this.label33.TabIndex = 62;
            this.label33.Text = "Save settings in order for them to take effect.";
            this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_credits
            // 
            this.btn_credits.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_credits.BackColor = System.Drawing.Color.Transparent;
            this.btn_credits.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_credits.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_credits.FlatAppearance.BorderSize = 0;
            this.btn_credits.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_credits.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_credits.Location = new System.Drawing.Point(251, 303);
            this.btn_credits.Margin = new System.Windows.Forms.Padding(2);
            this.btn_credits.Name = "btn_credits";
            this.btn_credits.Size = new System.Drawing.Size(80, 22);
            this.btn_credits.TabIndex = 21;
            this.btn_credits.Text = "Credits";
            this.btn_credits.UseVisualStyleBackColor = false;
            this.btn_credits.Click += new System.EventHandler(this.Btn_credits_Click);
            // 
            // settingsCloseBtn
            // 
            this.settingsCloseBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.settingsCloseBtn.BackColor = System.Drawing.Color.Transparent;
            this.settingsCloseBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.settingsCloseBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.settingsCloseBtn.FlatAppearance.BorderSize = 0;
            this.settingsCloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsCloseBtn.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsCloseBtn.Location = new System.Drawing.Point(335, 303);
            this.settingsCloseBtn.Margin = new System.Windows.Forms.Padding(2);
            this.settingsCloseBtn.Name = "settingsCloseBtn";
            this.settingsCloseBtn.Size = new System.Drawing.Size(80, 22);
            this.settingsCloseBtn.TabIndex = 9;
            this.settingsCloseBtn.Text = "Close";
            this.settingsCloseBtn.UseVisualStyleBackColor = false;
            this.settingsCloseBtn.Click += new System.EventHandler(this.SettingsCloseBtn_Click);
            // 
            // settingsSaveBtn
            // 
            this.settingsSaveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.settingsSaveBtn.BackColor = System.Drawing.Color.Transparent;
            this.settingsSaveBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.settingsSaveBtn.FlatAppearance.BorderSize = 0;
            this.settingsSaveBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsSaveBtn.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsSaveBtn.Location = new System.Drawing.Point(167, 303);
            this.settingsSaveBtn.Margin = new System.Windows.Forms.Padding(2);
            this.settingsSaveBtn.Name = "settingsSaveBtn";
            this.settingsSaveBtn.Size = new System.Drawing.Size(80, 22);
            this.settingsSaveBtn.TabIndex = 8;
            this.settingsSaveBtn.Text = "Save";
            this.settingsSaveBtn.UseVisualStyleBackColor = false;
            this.settingsSaveBtn.Click += new System.EventHandler(this.SettingsSaveBtn_Click);
            // 
            // setting_Label
            // 
            this.setting_Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.setting_Label.BackColor = System.Drawing.Color.Transparent;
            this.setting_Label.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.setting_Label.Controls.Add(this.label33);
            this.setting_Label.Location = new System.Drawing.Point(165, 287);
            this.setting_Label.Name = "setting_Label";
            this.setting_Label.Size = new System.Drawing.Size(248, 15);
            this.setting_Label.TabIndex = 37;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(572, 336);
            this.ControlBox = false;
            this.Controls.Add(this.setting_Label);
            this.Controls.Add(this.settingsSaveBtn);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.settingsCloseBtn);
            this.Controls.Add(this.btn_credits);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Settings";
            this.TopMost = true;
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numVerDiv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHorDiv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxPlyrs)).EndInit();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.audioCustomSettingsBox.ResumeLayout(false);
            this.audioCustomSettingsBox.PerformLayout();
            this.setting_Label.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button settingsSaveBtn;
        private System.Windows.Forms.Button settingsCloseBtn;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.Button btn_credits;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox settingsTopCmb;
        private System.Windows.Forms.ComboBox settingsStopCmb;
        private System.Windows.Forms.TextBox settingsTopTxt;
        private System.Windows.Forms.TextBox settingsStopTxt;
        private System.Windows.Forms.ComboBox settingsCloseCmb;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox settingsCloseHKTxt;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.NumericUpDown numMaxPlyrs;
        private System.Windows.Forms.CheckBox enableCustomCheckbox;
        private System.Windows.Forms.NumericUpDown numVerDiv;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.NumericUpDown numHorDiv;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox controllerSixteenNick;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox controllerFifteenNick;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox controllerFourteenNick;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox controllerThirteenNick;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox controllerTwelveNick;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox controllerElevenNick;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox controllerTenNick;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox controllerNineNick;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox controllerEightNick;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox controllerSevenNick;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox controllerSixNick;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox controllerFiveNick;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox controllerFourNick;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox controllerThreeNick;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox controllerTwoNick;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox controllerOneNick;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox statusCheck;
        private System.Windows.Forms.ComboBox cmb_Lang;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.ComboBox cmb_Network;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.CheckBox debugLogCheck;
        private System.Windows.Forms.CheckBox useNicksCheck;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.CheckBox keepAccountsCheck;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.TextBox nucUserPassTxt;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RadioButton audioCustomSettingsRadio;
        private System.Windows.Forms.RadioButton audioDefaultSettingsRadio;
        private System.Windows.Forms.ComboBox AudioInstance1;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.GroupBox audioCustomSettingsBox;
        private System.Windows.Forms.ComboBox AudioInstance2;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Button audioRefresh;
        private System.Windows.Forms.Label audioDefaultDevice;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.ComboBox AudioInstance8;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.ComboBox AudioInstance7;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.ComboBox AudioInstance6;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.ComboBox AudioInstance5;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.ComboBox AudioInstance4;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.ComboBox AudioInstance3;
        private System.Windows.Forms.Label label40;
		private System.Windows.Forms.CheckBox ignoreInputLockReminderCheckbox;
        private Panel layoutSizer;
        private Panel panel2;
        private Label label38;
        private Panel setting_Label;
        private ComboBox cmb_EpicLang;
        private Label label46;
        private ComboBox offlineMod;
        private Label OfflineModLabel;
        private CheckBox SplitDiv;
        private Label label49;
        private ComboBox SplitColors;
        private Label label31;
        private ComboBox comboBox_lockKey;
        private Label label_lockKey;
    }
}