using Nucleus.Coop.Controls;
using System.Windows.Forms;

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
            this.closeBtnPicture = new System.Windows.Forms.PictureBox();
            this.layoutBtnPicture = new System.Windows.Forms.PictureBox();
            this.settingsBtnPicture = new System.Windows.Forms.PictureBox();
            this.playersBtnPicture = new System.Windows.Forms.PictureBox();
            this.audioBtnPicture = new System.Windows.Forms.PictureBox();
            this.layoutTabBtn = new System.Windows.Forms.Button();
            this.audioTabBtn = new System.Windows.Forms.Button();
            this.playersTabBtn = new System.Windows.Forms.Button();
            this.settingsTabBtn = new System.Windows.Forms.Button();
            this.settingsTab = new System.Windows.Forms.Panel();
            this.btn_Gb_Update = new System.Windows.Forms.Button();
            this.disableGameProfiles = new Nucleus.Coop.Controls.CustomCheckBox();
            this.keepAccountsCheck = new Nucleus.Coop.Controls.CustomCheckBox();
            this.ctrlr_shorcutsBtn = new System.Windows.Forms.Button();
            this.nucUserPassTxt = new FlatTextBox();
            this.password_Label = new System.Windows.Forms.Label();
            this.themeLabel = new System.Windows.Forms.Label();
            this.themeCbx = new FlatCombo();
            this.cmb_EpicLang = new FlatCombo();
            this.label1 = new System.Windows.Forms.Label();
            this.ignoreInputLockReminderCheckbox = new Nucleus.Coop.Controls.CustomCheckBox();
            this.cmb_Lang = new FlatCombo();
            this.label2 = new System.Windows.Forms.Label();
            this.debugLogCheck = new Nucleus.Coop.Controls.CustomCheckBox();
            this.hotkeyBox = new System.Windows.Forms.GroupBox();
            this.rm_Cmb = new FlatCombo();
            this.plus8 = new System.Windows.Forms.Label();
            this.hkLabel2 = new System.Windows.Forms.Label();
            this.rm_HKTxt = new FlatTextBox();
            this.hkLabel4 = new System.Windows.Forms.Label();
            this.hkLabel3 = new System.Windows.Forms.Label();
            this.swl_Cmb = new FlatCombo();
            this.plus7 = new System.Windows.Forms.Label();
            this.hkLabel7 = new System.Windows.Forms.Label();
            this.swl_HKTxt = new FlatTextBox();
            this.csm_Cmb = new FlatCombo();
            this.hkLabel = new System.Windows.Forms.Label();
            this.plus6 = new System.Windows.Forms.Label();
            this.csm_HKTxt = new FlatTextBox();
            this.hkLabel8 = new System.Windows.Forms.Label();
            this.rw_Cmb = new FlatCombo();
            this.plus5 = new System.Windows.Forms.Label();
            this.hkLabel1 = new System.Windows.Forms.Label();
            this.tu_Cmb = new FlatCombo();
            this.plus1 = new System.Windows.Forms.Label();
            this.hkLabel5 = new System.Windows.Forms.Label();
            this.rw_HKTxt = new FlatTextBox();
            this.hotkeysLabel = new System.Windows.Forms.Label();
            this.hkLabel6 = new System.Windows.Forms.Label();
            this.tu_HKTxt = new FlatTextBox();
            this.lockKey_Cmb = new FlatCombo();
            this.ttm_Cmb = new FlatCombo();
            this.ss_Cmb = new FlatCombo();
            this.ttm_HKTxt = new FlatTextBox();
            this.ss_HKTxt = new FlatTextBox();
            this.cn_Cmb = new FlatCombo();
            this.plus4 = new System.Windows.Forms.Label();
            this.plus3 = new System.Windows.Forms.Label();
            this.plus2 = new System.Windows.Forms.Label();
            this.cn_HKTxt = new FlatTextBox();
            this.scaleOptionCbx = new Nucleus.Coop.Controls.CustomCheckBox();
            this.useNicksCheck = new Nucleus.Coop.Controls.CustomCheckBox();
            this.cmb_Network = new FlatCombo();
            this.label32 = new System.Windows.Forms.Label();
            this.playersTab = new System.Windows.Forms.Panel();
            this.gamepadsAssignMethods = new Nucleus.Coop.Controls.CustomCheckBox();
            this.btnNext = new System.Windows.Forms.Button();
            this.page1 = new System.Windows.Forms.Panel();
            this.label48 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.steamid16 = new FlatCombo();
            this.steamid15 = new FlatCombo();
            this.steamid14 = new FlatCombo();
            this.steamid13 = new FlatCombo();
            this.steamid12 = new FlatCombo();
            this.steamid11 = new FlatCombo();
            this.steamid10 = new FlatCombo();
            this.steamid9 = new FlatCombo();
            this.steamid8 = new FlatCombo();
            this.steamid7 = new FlatCombo();
            this.steamid6 = new FlatCombo();
            this.steamid5 = new FlatCombo();
            this.steamid4 = new FlatCombo();
            this.steamid3 = new FlatCombo();
            this.steamid2 = new FlatCombo();
            this.steamid1 = new FlatCombo();
            this.player16N = new FlatCombo();
            this.label17 = new System.Windows.Forms.Label();
            this.player15N = new FlatCombo();
            this.label18 = new System.Windows.Forms.Label();
            this.player14N = new FlatCombo();
            this.label19 = new System.Windows.Forms.Label();
            this.player13N = new FlatCombo();
            this.label20 = new System.Windows.Forms.Label();
            this.player12N = new FlatCombo();
            this.label22 = new System.Windows.Forms.Label();
            this.player11N = new FlatCombo();
            this.label23 = new System.Windows.Forms.Label();
            this.player10N = new FlatCombo();
            this.label24 = new System.Windows.Forms.Label();
            this.player9N = new FlatCombo();
            this.label26 = new System.Windows.Forms.Label();
            this.player8N = new FlatCombo();
            this.label16 = new System.Windows.Forms.Label();
            this.player7N = new FlatCombo();
            this.label15 = new System.Windows.Forms.Label();
            this.player6N = new FlatCombo();
            this.label11 = new System.Windows.Forms.Label();
            this.player5N = new FlatCombo();
            this.label9 = new System.Windows.Forms.Label();
            this.player4N = new FlatCombo();
            this.label14 = new System.Windows.Forms.Label();
            this.player3N = new FlatCombo();
            this.label12 = new System.Windows.Forms.Label();
            this.player2N = new FlatCombo();
            this.label10 = new System.Windows.Forms.Label();
            this.player1N = new FlatCombo();
            this.label7 = new System.Windows.Forms.Label();
            this.page2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label62 = new System.Windows.Forms.Label();
            this.label63 = new System.Windows.Forms.Label();
            this.label64 = new System.Windows.Forms.Label();
            this.label65 = new System.Windows.Forms.Label();
            this.label66 = new System.Windows.Forms.Label();
            this.steamid32 = new FlatCombo();
            this.steamid17 = new FlatCombo();
            this.steamid31 = new FlatCombo();
            this.label61 = new System.Windows.Forms.Label();
            this.steamid30 = new FlatCombo();
            this.player17N = new FlatCombo();
            this.steamid29 = new FlatCombo();
            this.label59 = new System.Windows.Forms.Label();
            this.steamid28 = new FlatCombo();
            this.player18N = new FlatCombo();
            this.steamid27 = new FlatCombo();
            this.label58 = new System.Windows.Forms.Label();
            this.steamid26 = new FlatCombo();
            this.player19N = new FlatCombo();
            this.steamid25 = new FlatCombo();
            this.label57 = new System.Windows.Forms.Label();
            this.steamid24 = new FlatCombo();
            this.player20N = new FlatCombo();
            this.steamid23 = new FlatCombo();
            this.label55 = new System.Windows.Forms.Label();
            this.steamid22 = new FlatCombo();
            this.player21N = new FlatCombo();
            this.steamid21 = new FlatCombo();
            this.label54 = new System.Windows.Forms.Label();
            this.steamid20 = new FlatCombo();
            this.player22N = new FlatCombo();
            this.steamid19 = new FlatCombo();
            this.label53 = new System.Windows.Forms.Label();
            this.steamid18 = new FlatCombo();
            this.player23N = new FlatCombo();
            this.label52 = new System.Windows.Forms.Label();
            this.player32N = new FlatCombo();
            this.player24N = new FlatCombo();
            this.label31 = new System.Windows.Forms.Label();
            this.label51 = new System.Windows.Forms.Label();
            this.player31N = new FlatCombo();
            this.player25N = new FlatCombo();
            this.label33 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.player30N = new FlatCombo();
            this.player26N = new FlatCombo();
            this.label34 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.player29N = new FlatCombo();
            this.player27N = new FlatCombo();
            this.label35 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.player28N = new FlatCombo();
            this.audioTab = new System.Windows.Forms.Panel();
            this.audioWarningLabel = new System.Windows.Forms.Label();
            this.audioDefaultDevice = new System.Windows.Forms.Label();
            this.audioCustomSettingsBox = new System.Windows.Forms.GroupBox();
            this.AudioInstance8 = new FlatCombo();
            this.label45 = new System.Windows.Forms.Label();
            this.AudioInstance7 = new FlatCombo();
            this.label44 = new System.Windows.Forms.Label();
            this.AudioInstance6 = new FlatCombo();
            this.label43 = new System.Windows.Forms.Label();
            this.AudioInstance5 = new FlatCombo();
            this.label42 = new System.Windows.Forms.Label();
            this.AudioInstance4 = new FlatCombo();
            this.label41 = new System.Windows.Forms.Label();
            this.AudioInstance3 = new FlatCombo();
            this.label40 = new System.Windows.Forms.Label();
            this.AudioInstance2 = new FlatCombo();
            this.AudioInstance1 = new FlatCombo();
            this.label37 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.audioCustomSettingsRadio = new Nucleus.Coop.Controls.CustomRadio();
            this.audioDefaultSettingsRadio = new Nucleus.Coop.Controls.CustomRadio();
            this.audioRefresh = new System.Windows.Forms.Button();
            this.layoutTab = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.DivLabel = new System.Windows.Forms.Label();
            this.smfw_Cmb = new FlatCombo();
            this.plus9 = new System.Windows.Forms.Label();
            this.mergerShortcutLabel = new System.Windows.Forms.Label();
            this.smfw_HKTxt = new FlatTextBox();
            this.refreshScreenDatasButton = new System.Windows.Forms.Button();
            this.mergerResSelectorLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.mergerSettingsLabel = new System.Windows.Forms.Label();
            this.losslessHook = new Nucleus.Coop.Controls.CustomCheckBox();
            this.selectedRes = new FlatTextBox();
            this.screen_panel = new System.Windows.Forms.Panel();
            this.enable_WMerger = new Nucleus.Coop.Controls.CustomCheckBox();
            this.hideDesktop = new Nucleus.Coop.Controls.CustomCheckBox();
            this.numMaxPlyrs = new Nucleus.Gaming.Controls.CustomNumericUpDown();
            this.numUpDownVer = new Nucleus.Gaming.Controls.CustomNumericUpDown();
            this.label29 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.numUpDownHor = new Nucleus.Gaming.Controls.CustomNumericUpDown();
            this.cutsceneLabel = new System.Windows.Forms.Label();
            this.cts_Panel = new System.Windows.Forms.Panel();
            this.cts_unfocus = new Nucleus.Coop.Controls.CustomCheckBox();
            this.cts_kar = new Nucleus.Coop.Controls.CustomCheckBox();
            this.cts_Mute = new Nucleus.Coop.Controls.CustomCheckBox();
            this.SplitColors = new FlatCombo();
            this.splitDiv = new Nucleus.Coop.Controls.CustomCheckBox();
            this.label49 = new System.Windows.Forms.Label();
            this.layoutSizer = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.closeBtnPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutBtnPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingsBtnPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.playersBtnPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioBtnPicture)).BeginInit();
            this.settingsTab.SuspendLayout();
            this.hotkeyBox.SuspendLayout();
            this.playersTab.SuspendLayout();
            this.page1.SuspendLayout();
            this.page2.SuspendLayout();
            this.audioTab.SuspendLayout();
            this.audioCustomSettingsBox.SuspendLayout();
            this.layoutTab.SuspendLayout();
            this.cts_Panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // closeBtnPicture
            // 
            this.closeBtnPicture.BackColor = System.Drawing.Color.Transparent;
            this.closeBtnPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.closeBtnPicture.Location = new System.Drawing.Point(652, 3);
            this.closeBtnPicture.Name = "closeBtnPicture";
            this.closeBtnPicture.Size = new System.Drawing.Size(19, 19);
            this.closeBtnPicture.TabIndex = 298;
            this.closeBtnPicture.TabStop = false;
            this.closeBtnPicture.Click += new System.EventHandler(this.CloseBtnPicture_Click);
            this.closeBtnPicture.MouseEnter += new System.EventHandler(this.CloseBtnPicture_MouseEnter);
            this.closeBtnPicture.MouseLeave += new System.EventHandler(this.CloseBtnPicture_MouseLeave);
            // 
            // layoutBtnPicture
            // 
            this.layoutBtnPicture.BackColor = System.Drawing.Color.Transparent;
            this.layoutBtnPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.layoutBtnPicture.Location = new System.Drawing.Point(384, 4);
            this.layoutBtnPicture.Name = "layoutBtnPicture";
            this.layoutBtnPicture.Size = new System.Drawing.Size(19, 19);
            this.layoutBtnPicture.TabIndex = 297;
            this.layoutBtnPicture.TabStop = false;
            this.layoutBtnPicture.Tag = "layoutTab";
            // 
            // settingsBtnPicture
            // 
            this.settingsBtnPicture.BackColor = System.Drawing.Color.Transparent;
            this.settingsBtnPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.settingsBtnPicture.Location = new System.Drawing.Point(81, 4);
            this.settingsBtnPicture.Name = "settingsBtnPicture";
            this.settingsBtnPicture.Size = new System.Drawing.Size(19, 19);
            this.settingsBtnPicture.TabIndex = 296;
            this.settingsBtnPicture.TabStop = false;
            this.settingsBtnPicture.Tag = "settingsTab";
            // 
            // playersBtnPicture
            // 
            this.playersBtnPicture.BackColor = System.Drawing.Color.Transparent;
            this.playersBtnPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.playersBtnPicture.Location = new System.Drawing.Point(182, 4);
            this.playersBtnPicture.Name = "playersBtnPicture";
            this.playersBtnPicture.Size = new System.Drawing.Size(19, 19);
            this.playersBtnPicture.TabIndex = 295;
            this.playersBtnPicture.TabStop = false;
            this.playersBtnPicture.Tag = "playersTab";
            // 
            // audioBtnPicture
            // 
            this.audioBtnPicture.BackColor = System.Drawing.Color.Transparent;
            this.audioBtnPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.audioBtnPicture.Location = new System.Drawing.Point(283, 4);
            this.audioBtnPicture.Name = "audioBtnPicture";
            this.audioBtnPicture.Size = new System.Drawing.Size(19, 19);
            this.audioBtnPicture.TabIndex = 294;
            this.audioBtnPicture.TabStop = false;
            this.audioBtnPicture.Tag = "audioTab";
            // 
            // layoutTabBtn
            // 
            this.layoutTabBtn.BackColor = System.Drawing.Color.Transparent;
            this.layoutTabBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.layoutTabBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.layoutTabBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.layoutTabBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutTabBtn.Location = new System.Drawing.Point(303, 2);
            this.layoutTabBtn.Margin = new System.Windows.Forms.Padding(0);
            this.layoutTabBtn.Name = "layoutTabBtn";
            this.layoutTabBtn.Size = new System.Drawing.Size(80, 23);
            this.layoutTabBtn.TabIndex = 45;
            this.layoutTabBtn.Tag = "layoutTab";
            this.layoutTabBtn.Text = "Layout";
            this.layoutTabBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.layoutTabBtn.UseVisualStyleBackColor = false;
            // 
            // audioTabBtn
            // 
            this.audioTabBtn.BackColor = System.Drawing.Color.Transparent;
            this.audioTabBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.audioTabBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.audioTabBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.audioTabBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.audioTabBtn.Location = new System.Drawing.Point(202, 2);
            this.audioTabBtn.Margin = new System.Windows.Forms.Padding(0);
            this.audioTabBtn.Name = "audioTabBtn";
            this.audioTabBtn.Size = new System.Drawing.Size(80, 23);
            this.audioTabBtn.TabIndex = 44;
            this.audioTabBtn.Tag = "audioTab";
            this.audioTabBtn.Text = "Audio";
            this.audioTabBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.audioTabBtn.UseVisualStyleBackColor = false;
            this.audioTabBtn.Click += new System.EventHandler(this.AudioBtnPicture_Click);
            // 
            // playersTabBtn
            // 
            this.playersTabBtn.BackColor = System.Drawing.Color.Transparent;
            this.playersTabBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.playersTabBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.playersTabBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.playersTabBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playersTabBtn.Location = new System.Drawing.Point(101, 2);
            this.playersTabBtn.Margin = new System.Windows.Forms.Padding(0);
            this.playersTabBtn.Name = "playersTabBtn";
            this.playersTabBtn.Size = new System.Drawing.Size(80, 23);
            this.playersTabBtn.TabIndex = 43;
            this.playersTabBtn.Tag = "playersTab";
            this.playersTabBtn.Text = "Players";
            this.playersTabBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.playersTabBtn.UseVisualStyleBackColor = false;
            // 
            // settingsTabBtn
            // 
            this.settingsTabBtn.BackColor = System.Drawing.Color.Transparent;
            this.settingsTabBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.settingsTabBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsTabBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsTabBtn.Location = new System.Drawing.Point(0, 2);
            this.settingsTabBtn.Margin = new System.Windows.Forms.Padding(0);
            this.settingsTabBtn.Name = "settingsTabBtn";
            this.settingsTabBtn.Size = new System.Drawing.Size(80, 23);
            this.settingsTabBtn.TabIndex = 38;
            this.settingsTabBtn.Tag = "settingsTab";
            this.settingsTabBtn.Text = "Settings";
            this.settingsTabBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.settingsTabBtn.UseVisualStyleBackColor = false;
            // 
            // settingsTab
            // 
            this.settingsTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.settingsTab.Controls.Add(this.btn_Gb_Update);
            this.settingsTab.Controls.Add(this.disableGameProfiles);
            this.settingsTab.Controls.Add(this.keepAccountsCheck);
            this.settingsTab.Controls.Add(this.ctrlr_shorcutsBtn);
            this.settingsTab.Controls.Add(this.nucUserPassTxt);
            this.settingsTab.Controls.Add(this.password_Label);
            this.settingsTab.Controls.Add(this.themeLabel);
            this.settingsTab.Controls.Add(this.themeCbx);
            this.settingsTab.Controls.Add(this.cmb_EpicLang);
            this.settingsTab.Controls.Add(this.label1);
            this.settingsTab.Controls.Add(this.ignoreInputLockReminderCheckbox);
            this.settingsTab.Controls.Add(this.cmb_Lang);
            this.settingsTab.Controls.Add(this.label2);
            this.settingsTab.Controls.Add(this.debugLogCheck);
            this.settingsTab.Controls.Add(this.hotkeyBox);
            this.settingsTab.Controls.Add(this.scaleOptionCbx);
            this.settingsTab.Controls.Add(this.useNicksCheck);
            this.settingsTab.Controls.Add(this.cmb_Network);
            this.settingsTab.Controls.Add(this.label32);
            this.settingsTab.Location = new System.Drawing.Point(1, 24);
            this.settingsTab.Name = "settingsTab";
            this.settingsTab.Size = new System.Drawing.Size(674, 391);
            this.settingsTab.TabIndex = 42;
            // 
            // btn_Gb_Update
            // 
            this.btn_Gb_Update.BackColor = System.Drawing.Color.Transparent;
            this.btn_Gb_Update.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Gb_Update.Location = new System.Drawing.Point(401, 349);
            this.btn_Gb_Update.Name = "btn_Gb_Update";
            this.btn_Gb_Update.Size = new System.Drawing.Size(258, 27);
            this.btn_Gb_Update.TabIndex = 169;
            this.btn_Gb_Update.Text = "Goldberg Steam Emulator Updater";
            this.btn_Gb_Update.UseVisualStyleBackColor = false;
            this.btn_Gb_Update.Click += new System.EventHandler(this.Btn_Gb_Update_Click);
            // 
            // disableGameProfiles
            // 
            this.disableGameProfiles.BackColor = System.Drawing.Color.Transparent;
            this.disableGameProfiles.BorderColor = System.Drawing.Color.Blue;
            this.disableGameProfiles.CheckColor = System.Drawing.Color.Blue;
            this.disableGameProfiles.Checked = true;
            this.disableGameProfiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.disableGameProfiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disableGameProfiles.ForeColor = System.Drawing.Color.White;
            this.disableGameProfiles.Location = new System.Drawing.Point(14, 317);
            this.disableGameProfiles.Margin = new System.Windows.Forms.Padding(2);
            this.disableGameProfiles.Name = "disableGameProfiles";
            this.disableGameProfiles.SelectionColor = System.Drawing.Color.LightBlue;
            this.disableGameProfiles.Size = new System.Drawing.Size(148, 17);
            this.disableGameProfiles.TabIndex = 167;
            this.disableGameProfiles.Tag = "";
            this.disableGameProfiles.Text = "Disable Game Profiles";
            this.disableGameProfiles.UseVisualStyleBackColor = false;
            // 
            // keepAccountsCheck
            // 
            this.keepAccountsCheck.BackColor = System.Drawing.Color.Transparent;
            this.keepAccountsCheck.BorderColor = System.Drawing.Color.Blue;
            this.keepAccountsCheck.CheckColor = System.Drawing.Color.Blue;
            this.keepAccountsCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.keepAccountsCheck.ForeColor = System.Drawing.Color.White;
            this.keepAccountsCheck.Location = new System.Drawing.Point(14, 275);
            this.keepAccountsCheck.Margin = new System.Windows.Forms.Padding(2);
            this.keepAccountsCheck.Name = "keepAccountsCheck";
            this.keepAccountsCheck.SelectionColor = System.Drawing.Color.LightBlue;
            this.keepAccountsCheck.Size = new System.Drawing.Size(166, 17);
            this.keepAccountsCheck.TabIndex = 166;
            this.keepAccountsCheck.Text = "Keep Nucleus User Accounts";
            this.keepAccountsCheck.UseVisualStyleBackColor = false;
            this.keepAccountsCheck.Click += new System.EventHandler(this.KeepAccountsCheck_Click);
            // 
            // ctrlr_shorcutsBtn
            // 
            this.ctrlr_shorcutsBtn.BackColor = System.Drawing.Color.Transparent;
            this.ctrlr_shorcutsBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.ctrlr_shorcutsBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ctrlr_shorcutsBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ctrlr_shorcutsBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ctrlr_shorcutsBtn.Location = new System.Drawing.Point(401, 316);
            this.ctrlr_shorcutsBtn.Name = "ctrlr_shorcutsBtn";
            this.ctrlr_shorcutsBtn.Size = new System.Drawing.Size(258, 27);
            this.ctrlr_shorcutsBtn.TabIndex = 164;
            this.ctrlr_shorcutsBtn.Text = "Controller Shortcuts Setup";
            this.ctrlr_shorcutsBtn.UseVisualStyleBackColor = false;
            this.ctrlr_shorcutsBtn.Click += new System.EventHandler(this.Ctrlr_shorcuts_Click);
            // 
            // nucUserPassTxt
            // 
            this.nucUserPassTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nucUserPassTxt.BorderColor = System.Drawing.Color.Blue;
            this.nucUserPassTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nucUserPassTxt.Hint = null;
            this.nucUserPassTxt.Location = new System.Drawing.Point(402, 31);
            this.nucUserPassTxt.Margin = new System.Windows.Forms.Padding(2);
            this.nucUserPassTxt.MaxLength = 127;
            this.nucUserPassTxt.Name = "nucUserPassTxt";
            this.nucUserPassTxt.PasswordChar = '*';
            this.nucUserPassTxt.Size = new System.Drawing.Size(197, 21);
            this.nucUserPassTxt.TabIndex = 150;
            this.nucUserPassTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // password_Label
            // 
            this.password_Label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.password_Label.AutoSize = true;
            this.password_Label.BackColor = System.Drawing.Color.Transparent;
            this.password_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.password_Label.Location = new System.Drawing.Point(399, 12);
            this.password_Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.password_Label.Name = "password_Label";
            this.password_Label.Size = new System.Drawing.Size(159, 15);
            this.password_Label.TabIndex = 151;
            this.password_Label.Text = "Nucleus User Password";
            this.password_Label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // themeLabel
            // 
            this.themeLabel.AutoSize = true;
            this.themeLabel.BackColor = System.Drawing.Color.Transparent;
            this.themeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.themeLabel.Location = new System.Drawing.Point(12, 105);
            this.themeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.themeLabel.Name = "themeLabel";
            this.themeLabel.Size = new System.Drawing.Size(51, 15);
            this.themeLabel.TabIndex = 163;
            this.themeLabel.Text = "Theme";
            this.themeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // themeCbx
            // 
            this.themeCbx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.themeCbx.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.themeCbx.FormattingEnabled = true;
            this.themeCbx.ItemHeight = 15;
            this.themeCbx.Location = new System.Drawing.Point(14, 124);
            this.themeCbx.Margin = new System.Windows.Forms.Padding(0);
            this.themeCbx.MaxDropDownItems = 10;
            this.themeCbx.Name = "themeCbx";
            this.themeCbx.Size = new System.Drawing.Size(142, 23);
            this.themeCbx.TabIndex = 162;
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
            this.cmb_EpicLang.Location = new System.Drawing.Point(14, 78);
            this.cmb_EpicLang.Margin = new System.Windows.Forms.Padding(0);
            this.cmb_EpicLang.MaxDropDownItems = 10;
            this.cmb_EpicLang.Name = "cmb_EpicLang";
            this.cmb_EpicLang.Size = new System.Drawing.Size(142, 23);
            this.cmb_EpicLang.TabIndex = 159;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 59);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 15);
            this.label1.TabIndex = 158;
            this.label1.Text = "Epic/Gog Language";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ignoreInputLockReminderCheckbox
            // 
            this.ignoreInputLockReminderCheckbox.BackColor = System.Drawing.Color.Transparent;
            this.ignoreInputLockReminderCheckbox.BorderColor = System.Drawing.Color.Blue;
            this.ignoreInputLockReminderCheckbox.CheckColor = System.Drawing.Color.Blue;
            this.ignoreInputLockReminderCheckbox.ForeColor = System.Drawing.Color.White;
            this.ignoreInputLockReminderCheckbox.Location = new System.Drawing.Point(14, 252);
            this.ignoreInputLockReminderCheckbox.Margin = new System.Windows.Forms.Padding(2);
            this.ignoreInputLockReminderCheckbox.Name = "ignoreInputLockReminderCheckbox";
            this.ignoreInputLockReminderCheckbox.SelectionColor = System.Drawing.Color.LightBlue;
            this.ignoreInputLockReminderCheckbox.Size = new System.Drawing.Size(178, 19);
            this.ignoreInputLockReminderCheckbox.TabIndex = 157;
            this.ignoreInputLockReminderCheckbox.Text = "Ignore Input Lock Reminder";
            this.ignoreInputLockReminderCheckbox.UseVisualStyleBackColor = false;
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
            "Latam",
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
            this.cmb_Lang.Location = new System.Drawing.Point(15, 31);
            this.cmb_Lang.Margin = new System.Windows.Forms.Padding(0);
            this.cmb_Lang.MaxDropDownItems = 10;
            this.cmb_Lang.Name = "cmb_Lang";
            this.cmb_Lang.Size = new System.Drawing.Size(142, 23);
            this.cmb_Lang.TabIndex = 155;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 15);
            this.label2.TabIndex = 154;
            this.label2.Text = "Steam Language";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // debugLogCheck
            // 
            this.debugLogCheck.BackColor = System.Drawing.Color.Transparent;
            this.debugLogCheck.BorderColor = System.Drawing.Color.Blue;
            this.debugLogCheck.CheckColor = System.Drawing.Color.Blue;
            this.debugLogCheck.ForeColor = System.Drawing.Color.White;
            this.debugLogCheck.Location = new System.Drawing.Point(14, 229);
            this.debugLogCheck.Margin = new System.Windows.Forms.Padding(2);
            this.debugLogCheck.Name = "debugLogCheck";
            this.debugLogCheck.SelectionColor = System.Drawing.Color.LightBlue;
            this.debugLogCheck.Size = new System.Drawing.Size(129, 19);
            this.debugLogCheck.TabIndex = 153;
            this.debugLogCheck.Text = "Enable Debug Log";
            this.debugLogCheck.UseVisualStyleBackColor = false;
            // 
            // hotkeyBox
            // 
            this.hotkeyBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hotkeyBox.BackColor = System.Drawing.Color.Transparent;
            this.hotkeyBox.Controls.Add(this.rm_Cmb);
            this.hotkeyBox.Controls.Add(this.plus8);
            this.hotkeyBox.Controls.Add(this.hkLabel2);
            this.hotkeyBox.Controls.Add(this.rm_HKTxt);
            this.hotkeyBox.Controls.Add(this.hkLabel4);
            this.hotkeyBox.Controls.Add(this.hkLabel3);
            this.hotkeyBox.Controls.Add(this.swl_Cmb);
            this.hotkeyBox.Controls.Add(this.plus7);
            this.hotkeyBox.Controls.Add(this.hkLabel7);
            this.hotkeyBox.Controls.Add(this.swl_HKTxt);
            this.hotkeyBox.Controls.Add(this.csm_Cmb);
            this.hotkeyBox.Controls.Add(this.hkLabel);
            this.hotkeyBox.Controls.Add(this.plus6);
            this.hotkeyBox.Controls.Add(this.csm_HKTxt);
            this.hotkeyBox.Controls.Add(this.hkLabel8);
            this.hotkeyBox.Controls.Add(this.rw_Cmb);
            this.hotkeyBox.Controls.Add(this.plus5);
            this.hotkeyBox.Controls.Add(this.hkLabel1);
            this.hotkeyBox.Controls.Add(this.tu_Cmb);
            this.hotkeyBox.Controls.Add(this.plus1);
            this.hotkeyBox.Controls.Add(this.hkLabel5);
            this.hotkeyBox.Controls.Add(this.rw_HKTxt);
            this.hotkeyBox.Controls.Add(this.hotkeysLabel);
            this.hotkeyBox.Controls.Add(this.hkLabel6);
            this.hotkeyBox.Controls.Add(this.tu_HKTxt);
            this.hotkeyBox.Controls.Add(this.lockKey_Cmb);
            this.hotkeyBox.Controls.Add(this.ttm_Cmb);
            this.hotkeyBox.Controls.Add(this.ss_Cmb);
            this.hotkeyBox.Controls.Add(this.ttm_HKTxt);
            this.hotkeyBox.Controls.Add(this.ss_HKTxt);
            this.hotkeyBox.Controls.Add(this.cn_Cmb);
            this.hotkeyBox.Controls.Add(this.plus4);
            this.hotkeyBox.Controls.Add(this.plus3);
            this.hotkeyBox.Controls.Add(this.plus2);
            this.hotkeyBox.Controls.Add(this.cn_HKTxt);
            this.hotkeyBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.hotkeyBox.Location = new System.Drawing.Point(401, 56);
            this.hotkeyBox.Margin = new System.Windows.Forms.Padding(2);
            this.hotkeyBox.Name = "hotkeyBox";
            this.hotkeyBox.Padding = new System.Windows.Forms.Padding(2);
            this.hotkeyBox.Size = new System.Drawing.Size(257, 254);
            this.hotkeyBox.TabIndex = 152;
            this.hotkeyBox.TabStop = false;
            // 
            // rm_Cmb
            // 
            this.rm_Cmb.BackColor = System.Drawing.Color.White;
            this.rm_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rm_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rm_Cmb.FormattingEnabled = true;
            this.rm_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.rm_Cmb.Location = new System.Drawing.Point(119, 196);
            this.rm_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.rm_Cmb.Name = "rm_Cmb";
            this.rm_Cmb.Size = new System.Drawing.Size(67, 23);
            this.rm_Cmb.TabIndex = 98;
            // 
            // plus8
            // 
            this.plus8.AutoSize = true;
            this.plus8.ForeColor = System.Drawing.Color.Black;
            this.plus8.Location = new System.Drawing.Point(189, 201);
            this.plus8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus8.Name = "plus8";
            this.plus8.Size = new System.Drawing.Size(14, 15);
            this.plus8.TabIndex = 101;
            this.plus8.Text = "+";
            this.plus8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hkLabel2
            // 
            this.hkLabel2.AutoSize = true;
            this.hkLabel2.Location = new System.Drawing.Point(0, 198);
            this.hkLabel2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel2.Name = "hkLabel2";
            this.hkLabel2.Size = new System.Drawing.Size(114, 15);
            this.hkLabel2.TabIndex = 100;
            this.hkLabel2.Text = "Hotkeys Reminder :";
            this.hkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rm_HKTxt
            // 
            this.rm_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.rm_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.rm_HKTxt.Hint = null;
            this.rm_HKTxt.Location = new System.Drawing.Point(205, 197);
            this.rm_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.rm_HKTxt.MaxLength = 1;
            this.rm_HKTxt.Name = "rm_HKTxt";
            this.rm_HKTxt.ShortcutsEnabled = false;
            this.rm_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.rm_HKTxt.TabIndex = 99;
            this.rm_HKTxt.Tag = "HotKeyTextBox";
            this.rm_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // hkLabel4
            // 
            this.hkLabel4.AutoSize = true;
            this.hkLabel4.Location = new System.Drawing.Point(9, 102);
            this.hkLabel4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel4.Name = "hkLabel4";
            this.hkLabel4.Size = new System.Drawing.Size(105, 15);
            this.hkLabel4.TabIndex = 24;
            this.hkLabel4.Text = "Toggle Top Most :";
            this.hkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hkLabel3
            // 
            this.hkLabel3.AutoSize = true;
            this.hkLabel3.Location = new System.Drawing.Point(29, 78);
            this.hkLabel3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel3.Name = "hkLabel3";
            this.hkLabel3.Size = new System.Drawing.Size(85, 15);
            this.hkLabel3.TabIndex = 22;
            this.hkLabel3.Text = "Stop Session :";
            this.hkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // swl_Cmb
            // 
            this.swl_Cmb.BackColor = System.Drawing.Color.White;
            this.swl_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.swl_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.swl_Cmb.FormattingEnabled = true;
            this.swl_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.swl_Cmb.Location = new System.Drawing.Point(119, 172);
            this.swl_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.swl_Cmb.Name = "swl_Cmb";
            this.swl_Cmb.Size = new System.Drawing.Size(67, 23);
            this.swl_Cmb.TabIndex = 94;
            // 
            // plus7
            // 
            this.plus7.AutoSize = true;
            this.plus7.ForeColor = System.Drawing.Color.Black;
            this.plus7.Location = new System.Drawing.Point(189, 177);
            this.plus7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus7.Name = "plus7";
            this.plus7.Size = new System.Drawing.Size(14, 15);
            this.plus7.TabIndex = 97;
            this.plus7.Text = "+";
            this.plus7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hkLabel7
            // 
            this.hkLabel7.AutoSize = true;
            this.hkLabel7.Location = new System.Drawing.Point(20, 174);
            this.hkLabel7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel7.Name = "hkLabel7";
            this.hkLabel7.Size = new System.Drawing.Size(94, 15);
            this.hkLabel7.TabIndex = 96;
            this.hkLabel7.Text = "Switch Layouts :";
            this.hkLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // swl_HKTxt
            // 
            this.swl_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.swl_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.swl_HKTxt.Hint = null;
            this.swl_HKTxt.Location = new System.Drawing.Point(205, 173);
            this.swl_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.swl_HKTxt.MaxLength = 1;
            this.swl_HKTxt.Name = "swl_HKTxt";
            this.swl_HKTxt.ShortcutsEnabled = false;
            this.swl_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.swl_HKTxt.TabIndex = 95;
            this.swl_HKTxt.Tag = "HotKeyTextBox";
            this.swl_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // csm_Cmb
            // 
            this.csm_Cmb.BackColor = System.Drawing.Color.White;
            this.csm_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.csm_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.csm_Cmb.FormattingEnabled = true;
            this.csm_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.csm_Cmb.Location = new System.Drawing.Point(119, 148);
            this.csm_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.csm_Cmb.Name = "csm_Cmb";
            this.csm_Cmb.Size = new System.Drawing.Size(67, 23);
            this.csm_Cmb.TabIndex = 90;
            // 
            // hkLabel
            // 
            this.hkLabel.AutoSize = true;
            this.hkLabel.Location = new System.Drawing.Point(22, 54);
            this.hkLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel.Name = "hkLabel";
            this.hkLabel.Size = new System.Drawing.Size(92, 15);
            this.hkLabel.TabIndex = 20;
            this.hkLabel.Text = "Close Nucleus :";
            this.hkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // plus6
            // 
            this.plus6.AutoSize = true;
            this.plus6.ForeColor = System.Drawing.Color.Black;
            this.plus6.Location = new System.Drawing.Point(189, 153);
            this.plus6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus6.Name = "plus6";
            this.plus6.Size = new System.Drawing.Size(14, 15);
            this.plus6.TabIndex = 93;
            this.plus6.Text = "+";
            this.plus6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // csm_HKTxt
            // 
            this.csm_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.csm_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.csm_HKTxt.Hint = null;
            this.csm_HKTxt.Location = new System.Drawing.Point(205, 149);
            this.csm_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.csm_HKTxt.MaxLength = 1;
            this.csm_HKTxt.Name = "csm_HKTxt";
            this.csm_HKTxt.ShortcutsEnabled = false;
            this.csm_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.csm_HKTxt.TabIndex = 91;
            this.csm_HKTxt.Tag = "HotKeyTextBox";
            this.csm_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // hkLabel8
            // 
            this.hkLabel8.AutoSize = true;
            this.hkLabel8.Location = new System.Drawing.Point(22, 224);
            this.hkLabel8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel8.Name = "hkLabel8";
            this.hkLabel8.Size = new System.Drawing.Size(92, 15);
            this.hkLabel8.TabIndex = 29;
            this.hkLabel8.Text = "Lock Input Key :";
            this.hkLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rw_Cmb
            // 
            this.rw_Cmb.BackColor = System.Drawing.Color.White;
            this.rw_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rw_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rw_Cmb.FormattingEnabled = true;
            this.rw_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.rw_Cmb.Location = new System.Drawing.Point(119, 124);
            this.rw_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.rw_Cmb.Name = "rw_Cmb";
            this.rw_Cmb.Size = new System.Drawing.Size(67, 23);
            this.rw_Cmb.TabIndex = 86;
            // 
            // plus5
            // 
            this.plus5.AutoSize = true;
            this.plus5.ForeColor = System.Drawing.Color.Black;
            this.plus5.Location = new System.Drawing.Point(189, 127);
            this.plus5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus5.Name = "plus5";
            this.plus5.Size = new System.Drawing.Size(14, 15);
            this.plus5.TabIndex = 89;
            this.plus5.Text = "+";
            this.plus5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hkLabel1
            // 
            this.hkLabel1.AutoSize = true;
            this.hkLabel1.Location = new System.Drawing.Point(15, 30);
            this.hkLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel1.Name = "hkLabel1";
            this.hkLabel1.Size = new System.Drawing.Size(99, 15);
            this.hkLabel1.TabIndex = 33;
            this.hkLabel1.Text = "Toggle Unfocus :";
            this.hkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tu_Cmb
            // 
            this.tu_Cmb.BackColor = System.Drawing.Color.White;
            this.tu_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tu_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tu_Cmb.FormattingEnabled = true;
            this.tu_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.tu_Cmb.Location = new System.Drawing.Point(119, 28);
            this.tu_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.tu_Cmb.Name = "tu_Cmb";
            this.tu_Cmb.Size = new System.Drawing.Size(67, 23);
            this.tu_Cmb.TabIndex = 31;
            // 
            // plus1
            // 
            this.plus1.AutoSize = true;
            this.plus1.ForeColor = System.Drawing.Color.Black;
            this.plus1.Location = new System.Drawing.Point(189, 31);
            this.plus1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus1.Name = "plus1";
            this.plus1.Size = new System.Drawing.Size(14, 15);
            this.plus1.TabIndex = 34;
            this.plus1.Text = "+";
            this.plus1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hkLabel5
            // 
            this.hkLabel5.AutoSize = true;
            this.hkLabel5.Location = new System.Drawing.Point(16, 127);
            this.hkLabel5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel5.Name = "hkLabel5";
            this.hkLabel5.Size = new System.Drawing.Size(98, 15);
            this.hkLabel5.TabIndex = 88;
            this.hkLabel5.Text = "Reset Windows :";
            this.hkLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rw_HKTxt
            // 
            this.rw_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.rw_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.rw_HKTxt.Hint = null;
            this.rw_HKTxt.Location = new System.Drawing.Point(205, 125);
            this.rw_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.rw_HKTxt.MaxLength = 1;
            this.rw_HKTxt.Name = "rw_HKTxt";
            this.rw_HKTxt.ShortcutsEnabled = false;
            this.rw_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.rw_HKTxt.TabIndex = 87;
            this.rw_HKTxt.Tag = "HotKeyTextBox";
            this.rw_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // hotkeysLabel
            // 
            this.hotkeysLabel.AutoSize = true;
            this.hotkeysLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.hotkeysLabel.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hotkeysLabel.Location = new System.Drawing.Point(3, 10);
            this.hotkeysLabel.Name = "hotkeysLabel";
            this.hotkeysLabel.Size = new System.Drawing.Size(57, 15);
            this.hotkeysLabel.TabIndex = 28;
            this.hotkeysLabel.Text = "Hotkeys ";
            this.hotkeysLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hkLabel6
            // 
            this.hkLabel6.AutoSize = true;
            this.hkLabel6.Location = new System.Drawing.Point(9, 150);
            this.hkLabel6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hkLabel6.Name = "hkLabel6";
            this.hkLabel6.Size = new System.Drawing.Size(105, 15);
            this.hkLabel6.TabIndex = 92;
            this.hkLabel6.Text = "Cutscenes Mode :";
            this.hkLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tu_HKTxt
            // 
            this.tu_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.tu_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.tu_HKTxt.Hint = null;
            this.tu_HKTxt.Location = new System.Drawing.Point(205, 29);
            this.tu_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.tu_HKTxt.MaxLength = 1;
            this.tu_HKTxt.Name = "tu_HKTxt";
            this.tu_HKTxt.ShortcutsEnabled = false;
            this.tu_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.tu_HKTxt.TabIndex = 32;
            this.tu_HKTxt.Tag = "HotKeyTextBox";
            this.tu_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lockKey_Cmb
            // 
            this.lockKey_Cmb.BackColor = System.Drawing.Color.White;
            this.lockKey_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lockKey_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lockKey_Cmb.FormattingEnabled = true;
            this.lockKey_Cmb.Items.AddRange(new object[] {
            "End",
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
            this.lockKey_Cmb.Location = new System.Drawing.Point(119, 222);
            this.lockKey_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.lockKey_Cmb.Name = "lockKey_Cmb";
            this.lockKey_Cmb.Size = new System.Drawing.Size(123, 23);
            this.lockKey_Cmb.TabIndex = 30;
            // 
            // ttm_Cmb
            // 
            this.ttm_Cmb.BackColor = System.Drawing.Color.White;
            this.ttm_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ttm_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ttm_Cmb.FormattingEnabled = true;
            this.ttm_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.ttm_Cmb.Location = new System.Drawing.Point(119, 100);
            this.ttm_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.ttm_Cmb.Name = "ttm_Cmb";
            this.ttm_Cmb.Size = new System.Drawing.Size(67, 23);
            this.ttm_Cmb.TabIndex = 5;
            // 
            // ss_Cmb
            // 
            this.ss_Cmb.BackColor = System.Drawing.Color.White;
            this.ss_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ss_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ss_Cmb.FormattingEnabled = true;
            this.ss_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.ss_Cmb.Location = new System.Drawing.Point(119, 76);
            this.ss_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.ss_Cmb.Name = "ss_Cmb";
            this.ss_Cmb.Size = new System.Drawing.Size(67, 23);
            this.ss_Cmb.TabIndex = 3;
            // 
            // ttm_HKTxt
            // 
            this.ttm_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.ttm_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.ttm_HKTxt.Hint = null;
            this.ttm_HKTxt.Location = new System.Drawing.Point(205, 101);
            this.ttm_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.ttm_HKTxt.MaxLength = 1;
            this.ttm_HKTxt.Name = "ttm_HKTxt";
            this.ttm_HKTxt.ShortcutsEnabled = false;
            this.ttm_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.ttm_HKTxt.TabIndex = 6;
            this.ttm_HKTxt.Tag = "HotKeyTextBox";
            this.ttm_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ss_HKTxt
            // 
            this.ss_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.ss_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.ss_HKTxt.Hint = null;
            this.ss_HKTxt.Location = new System.Drawing.Point(205, 77);
            this.ss_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.ss_HKTxt.MaxLength = 1;
            this.ss_HKTxt.Name = "ss_HKTxt";
            this.ss_HKTxt.ShortcutsEnabled = false;
            this.ss_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.ss_HKTxt.TabIndex = 4;
            this.ss_HKTxt.Tag = "HotKeyTextBox";
            this.ss_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cn_Cmb
            // 
            this.cn_Cmb.BackColor = System.Drawing.Color.White;
            this.cn_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cn_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cn_Cmb.FormattingEnabled = true;
            this.cn_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.cn_Cmb.Location = new System.Drawing.Point(119, 52);
            this.cn_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.cn_Cmb.Name = "cn_Cmb";
            this.cn_Cmb.Size = new System.Drawing.Size(67, 23);
            this.cn_Cmb.TabIndex = 1;
            // 
            // plus4
            // 
            this.plus4.AutoSize = true;
            this.plus4.ForeColor = System.Drawing.Color.Black;
            this.plus4.Location = new System.Drawing.Point(189, 103);
            this.plus4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus4.Name = "plus4";
            this.plus4.Size = new System.Drawing.Size(14, 15);
            this.plus4.TabIndex = 27;
            this.plus4.Text = "+";
            this.plus4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // plus3
            // 
            this.plus3.AutoSize = true;
            this.plus3.ForeColor = System.Drawing.Color.Black;
            this.plus3.Location = new System.Drawing.Point(189, 78);
            this.plus3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus3.Name = "plus3";
            this.plus3.Size = new System.Drawing.Size(14, 15);
            this.plus3.TabIndex = 26;
            this.plus3.Text = "+";
            this.plus3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // plus2
            // 
            this.plus2.AutoSize = true;
            this.plus2.ForeColor = System.Drawing.Color.Black;
            this.plus2.Location = new System.Drawing.Point(189, 55);
            this.plus2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus2.Name = "plus2";
            this.plus2.Size = new System.Drawing.Size(14, 15);
            this.plus2.TabIndex = 25;
            this.plus2.Text = "+";
            this.plus2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cn_HKTxt
            // 
            this.cn_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.cn_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.cn_HKTxt.Hint = null;
            this.cn_HKTxt.Location = new System.Drawing.Point(205, 53);
            this.cn_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.cn_HKTxt.MaxLength = 1;
            this.cn_HKTxt.Name = "cn_HKTxt";
            this.cn_HKTxt.ShortcutsEnabled = false;
            this.cn_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.cn_HKTxt.TabIndex = 2;
            this.cn_HKTxt.Tag = "HotKeyTextBox";
            this.cn_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // scaleOptionCbx
            // 
            this.scaleOptionCbx.BackColor = System.Drawing.Color.Transparent;
            this.scaleOptionCbx.BorderColor = System.Drawing.Color.Blue;
            this.scaleOptionCbx.CheckColor = System.Drawing.Color.Blue;
            this.scaleOptionCbx.Checked = true;
            this.scaleOptionCbx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.scaleOptionCbx.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scaleOptionCbx.ForeColor = System.Drawing.Color.White;
            this.scaleOptionCbx.Location = new System.Drawing.Point(14, 296);
            this.scaleOptionCbx.Margin = new System.Windows.Forms.Padding(2);
            this.scaleOptionCbx.Name = "scaleOptionCbx";
            this.scaleOptionCbx.SelectionColor = System.Drawing.Color.LightBlue;
            this.scaleOptionCbx.Size = new System.Drawing.Size(185, 17);
            this.scaleOptionCbx.TabIndex = 148;
            this.scaleOptionCbx.Text = "Auto Set Desktop Scale To 100%";
            this.scaleOptionCbx.UseVisualStyleBackColor = false;
            // 
            // useNicksCheck
            // 
            this.useNicksCheck.BackColor = System.Drawing.Color.Transparent;
            this.useNicksCheck.BorderColor = System.Drawing.Color.Blue;
            this.useNicksCheck.CheckColor = System.Drawing.Color.Blue;
            this.useNicksCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.useNicksCheck.ForeColor = System.Drawing.Color.White;
            this.useNicksCheck.Location = new System.Drawing.Point(14, 206);
            this.useNicksCheck.Margin = new System.Windows.Forms.Padding(2);
            this.useNicksCheck.Name = "useNicksCheck";
            this.useNicksCheck.SelectionColor = System.Drawing.Color.LightBlue;
            this.useNicksCheck.Size = new System.Drawing.Size(164, 19);
            this.useNicksCheck.TabIndex = 146;
            this.useNicksCheck.Text = "Use Nicknames In-Game";
            this.useNicksCheck.UseVisualStyleBackColor = false;
            // 
            // cmb_Network
            // 
            this.cmb_Network.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Network.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_Network.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_Network.Location = new System.Drawing.Point(15, 170);
            this.cmb_Network.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmb_Network.Name = "cmb_Network";
            this.cmb_Network.Size = new System.Drawing.Size(198, 21);
            this.cmb_Network.TabIndex = 136;
            this.cmb_Network.DropDown += new System.EventHandler(this.Cmb_Network_DropDown);
            this.cmb_Network.DropDownClosed += new System.EventHandler(this.Cmb_Network_DropDownClosed);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label32.Location = new System.Drawing.Point(12, 151);
            this.label32.Margin = new System.Windows.Forms.Padding(0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(59, 15);
            this.label32.TabIndex = 109;
            this.label32.Text = "Network";
            this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // playersTab
            // 
            this.playersTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playersTab.BackColor = System.Drawing.Color.PaleGreen;
            this.playersTab.Controls.Add(this.gamepadsAssignMethods);
            this.playersTab.Controls.Add(this.btnNext);
            this.playersTab.Controls.Add(this.page1);
            this.playersTab.Controls.Add(this.page2);
            this.playersTab.Location = new System.Drawing.Point(-1, 26);
            this.playersTab.Name = "playersTab";
            this.playersTab.Size = new System.Drawing.Size(675, 391);
            this.playersTab.TabIndex = 132;
            // 
            // gamepadsAssignMethods
            // 
            this.gamepadsAssignMethods.BackColor = System.Drawing.Color.DimGray;
            this.gamepadsAssignMethods.BorderColor = System.Drawing.Color.Blue;
            this.gamepadsAssignMethods.CheckColor = System.Drawing.Color.Blue;
            this.gamepadsAssignMethods.ForeColor = System.Drawing.Color.White;
            this.gamepadsAssignMethods.Location = new System.Drawing.Point(16, 34);
            this.gamepadsAssignMethods.Name = "gamepadsAssignMethods";
            this.gamepadsAssignMethods.SelectionColor = System.Drawing.Color.LightBlue;
            this.gamepadsAssignMethods.Size = new System.Drawing.Size(202, 19);
            this.gamepadsAssignMethods.TabIndex = 295;
            this.gamepadsAssignMethods.Text = "Follow Gamepad API Indexes";
            this.gamepadsAssignMethods.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.Color.Transparent;
            this.btnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnNext.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Location = new System.Drawing.Point(9, 80);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(38, 22);
            this.btnNext.TabIndex = 294;
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            // 
            // page1
            // 
            this.page1.Controls.Add(this.label48);
            this.page1.Controls.Add(this.label47);
            this.page1.Controls.Add(this.label21);
            this.page1.Controls.Add(this.label25);
            this.page1.Controls.Add(this.label13);
            this.page1.Controls.Add(this.label8);
            this.page1.Controls.Add(this.steamid16);
            this.page1.Controls.Add(this.steamid15);
            this.page1.Controls.Add(this.steamid14);
            this.page1.Controls.Add(this.steamid13);
            this.page1.Controls.Add(this.steamid12);
            this.page1.Controls.Add(this.steamid11);
            this.page1.Controls.Add(this.steamid10);
            this.page1.Controls.Add(this.steamid9);
            this.page1.Controls.Add(this.steamid8);
            this.page1.Controls.Add(this.steamid7);
            this.page1.Controls.Add(this.steamid6);
            this.page1.Controls.Add(this.steamid5);
            this.page1.Controls.Add(this.steamid4);
            this.page1.Controls.Add(this.steamid3);
            this.page1.Controls.Add(this.steamid2);
            this.page1.Controls.Add(this.steamid1);
            this.page1.Controls.Add(this.player16N);
            this.page1.Controls.Add(this.label17);
            this.page1.Controls.Add(this.player15N);
            this.page1.Controls.Add(this.label18);
            this.page1.Controls.Add(this.player14N);
            this.page1.Controls.Add(this.label19);
            this.page1.Controls.Add(this.player13N);
            this.page1.Controls.Add(this.label20);
            this.page1.Controls.Add(this.player12N);
            this.page1.Controls.Add(this.label22);
            this.page1.Controls.Add(this.player11N);
            this.page1.Controls.Add(this.label23);
            this.page1.Controls.Add(this.player10N);
            this.page1.Controls.Add(this.label24);
            this.page1.Controls.Add(this.player9N);
            this.page1.Controls.Add(this.label26);
            this.page1.Controls.Add(this.player8N);
            this.page1.Controls.Add(this.label16);
            this.page1.Controls.Add(this.player7N);
            this.page1.Controls.Add(this.label15);
            this.page1.Controls.Add(this.player6N);
            this.page1.Controls.Add(this.label11);
            this.page1.Controls.Add(this.player5N);
            this.page1.Controls.Add(this.label9);
            this.page1.Controls.Add(this.player4N);
            this.page1.Controls.Add(this.label14);
            this.page1.Controls.Add(this.player3N);
            this.page1.Controls.Add(this.label12);
            this.page1.Controls.Add(this.player2N);
            this.page1.Controls.Add(this.label10);
            this.page1.Controls.Add(this.player1N);
            this.page1.Controls.Add(this.label7);
            this.page1.Location = new System.Drawing.Point(9, 101);
            this.page1.Name = "page1";
            this.page1.Size = new System.Drawing.Size(634, 217);
            this.page1.TabIndex = 293;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label48.Location = new System.Drawing.Point(536, 9);
            this.label48.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(49, 13);
            this.label48.TabIndex = 296;
            this.label48.Text = "Steam Id";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label47.Location = new System.Drawing.Point(176, 9);
            this.label47.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(49, 13);
            this.label47.TabIndex = 295;
            this.label47.Text = "Steam Id";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(366, 9);
            this.label21.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(48, 13);
            this.label21.TabIndex = 294;
            this.label21.Text = "Player Id";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.Location = new System.Drawing.Point(428, 9);
            this.label25.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(55, 13);
            this.label25.TabIndex = 293;
            this.label25.Text = "Nickname";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(4, 9);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(51, 13);
            this.label13.TabIndex = 292;
            this.label13.Text = " Player Id";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(68, 9);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 291;
            this.label8.Text = "Nickname";
            // 
            // steamid16
            // 
            this.steamid16.BackColor = System.Drawing.Color.White;
            this.steamid16.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid16.ForeColor = System.Drawing.Color.Black;
            this.steamid16.Location = new System.Drawing.Point(495, 186);
            this.steamid16.Margin = new System.Windows.Forms.Padding(0);
            this.steamid16.MaxLength = 17;
            this.steamid16.Name = "steamid16";
            this.steamid16.Size = new System.Drawing.Size(133, 21);
            this.steamid16.TabIndex = 290;
            // 
            // steamid15
            // 
            this.steamid15.BackColor = System.Drawing.Color.White;
            this.steamid15.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid15.ForeColor = System.Drawing.Color.Black;
            this.steamid15.Location = new System.Drawing.Point(495, 164);
            this.steamid15.Margin = new System.Windows.Forms.Padding(0);
            this.steamid15.MaxLength = 17;
            this.steamid15.Name = "steamid15";
            this.steamid15.Size = new System.Drawing.Size(133, 21);
            this.steamid15.TabIndex = 289;
            // 
            // steamid14
            // 
            this.steamid14.BackColor = System.Drawing.Color.White;
            this.steamid14.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid14.ForeColor = System.Drawing.Color.Black;
            this.steamid14.Location = new System.Drawing.Point(495, 142);
            this.steamid14.Margin = new System.Windows.Forms.Padding(0);
            this.steamid14.MaxLength = 17;
            this.steamid14.Name = "steamid14";
            this.steamid14.Size = new System.Drawing.Size(133, 21);
            this.steamid14.TabIndex = 288;
            // 
            // steamid13
            // 
            this.steamid13.BackColor = System.Drawing.Color.White;
            this.steamid13.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid13.ForeColor = System.Drawing.Color.Black;
            this.steamid13.Location = new System.Drawing.Point(495, 120);
            this.steamid13.Margin = new System.Windows.Forms.Padding(0);
            this.steamid13.MaxLength = 17;
            this.steamid13.Name = "steamid13";
            this.steamid13.Size = new System.Drawing.Size(133, 21);
            this.steamid13.TabIndex = 287;
            // 
            // steamid12
            // 
            this.steamid12.BackColor = System.Drawing.Color.White;
            this.steamid12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid12.ForeColor = System.Drawing.Color.Black;
            this.steamid12.Location = new System.Drawing.Point(495, 98);
            this.steamid12.Margin = new System.Windows.Forms.Padding(0);
            this.steamid12.MaxLength = 17;
            this.steamid12.Name = "steamid12";
            this.steamid12.Size = new System.Drawing.Size(133, 21);
            this.steamid12.TabIndex = 286;
            // 
            // steamid11
            // 
            this.steamid11.BackColor = System.Drawing.Color.White;
            this.steamid11.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid11.ForeColor = System.Drawing.Color.Black;
            this.steamid11.Location = new System.Drawing.Point(495, 76);
            this.steamid11.Margin = new System.Windows.Forms.Padding(0);
            this.steamid11.MaxLength = 17;
            this.steamid11.Name = "steamid11";
            this.steamid11.Size = new System.Drawing.Size(133, 21);
            this.steamid11.TabIndex = 285;
            // 
            // steamid10
            // 
            this.steamid10.BackColor = System.Drawing.Color.White;
            this.steamid10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid10.ForeColor = System.Drawing.Color.Black;
            this.steamid10.Location = new System.Drawing.Point(495, 54);
            this.steamid10.Margin = new System.Windows.Forms.Padding(0);
            this.steamid10.MaxLength = 17;
            this.steamid10.Name = "steamid10";
            this.steamid10.Size = new System.Drawing.Size(133, 21);
            this.steamid10.TabIndex = 284;
            // 
            // steamid9
            // 
            this.steamid9.BackColor = System.Drawing.Color.White;
            this.steamid9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid9.ForeColor = System.Drawing.Color.Black;
            this.steamid9.Location = new System.Drawing.Point(495, 32);
            this.steamid9.Margin = new System.Windows.Forms.Padding(0);
            this.steamid9.MaxLength = 17;
            this.steamid9.Name = "steamid9";
            this.steamid9.Size = new System.Drawing.Size(133, 21);
            this.steamid9.TabIndex = 283;
            // 
            // steamid8
            // 
            this.steamid8.BackColor = System.Drawing.Color.White;
            this.steamid8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid8.ForeColor = System.Drawing.Color.Black;
            this.steamid8.Location = new System.Drawing.Point(135, 186);
            this.steamid8.Margin = new System.Windows.Forms.Padding(0);
            this.steamid8.MaxLength = 17;
            this.steamid8.Name = "steamid8";
            this.steamid8.Size = new System.Drawing.Size(133, 21);
            this.steamid8.TabIndex = 282;
            // 
            // steamid7
            // 
            this.steamid7.BackColor = System.Drawing.Color.White;
            this.steamid7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid7.ForeColor = System.Drawing.Color.Black;
            this.steamid7.Location = new System.Drawing.Point(135, 164);
            this.steamid7.Margin = new System.Windows.Forms.Padding(0);
            this.steamid7.MaxLength = 17;
            this.steamid7.Name = "steamid7";
            this.steamid7.Size = new System.Drawing.Size(133, 21);
            this.steamid7.TabIndex = 281;
            // 
            // steamid6
            // 
            this.steamid6.BackColor = System.Drawing.Color.White;
            this.steamid6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid6.ForeColor = System.Drawing.Color.Black;
            this.steamid6.Location = new System.Drawing.Point(135, 142);
            this.steamid6.Margin = new System.Windows.Forms.Padding(0);
            this.steamid6.MaxLength = 17;
            this.steamid6.Name = "steamid6";
            this.steamid6.Size = new System.Drawing.Size(133, 21);
            this.steamid6.TabIndex = 280;
            // 
            // steamid5
            // 
            this.steamid5.BackColor = System.Drawing.Color.White;
            this.steamid5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid5.ForeColor = System.Drawing.Color.Black;
            this.steamid5.Location = new System.Drawing.Point(135, 120);
            this.steamid5.Margin = new System.Windows.Forms.Padding(0);
            this.steamid5.MaxLength = 17;
            this.steamid5.Name = "steamid5";
            this.steamid5.Size = new System.Drawing.Size(133, 21);
            this.steamid5.TabIndex = 279;
            // 
            // steamid4
            // 
            this.steamid4.BackColor = System.Drawing.Color.White;
            this.steamid4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid4.ForeColor = System.Drawing.Color.Black;
            this.steamid4.Location = new System.Drawing.Point(135, 98);
            this.steamid4.Margin = new System.Windows.Forms.Padding(0);
            this.steamid4.MaxLength = 17;
            this.steamid4.Name = "steamid4";
            this.steamid4.Size = new System.Drawing.Size(133, 21);
            this.steamid4.TabIndex = 278;
            // 
            // steamid3
            // 
            this.steamid3.BackColor = System.Drawing.Color.White;
            this.steamid3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid3.ForeColor = System.Drawing.Color.Black;
            this.steamid3.Location = new System.Drawing.Point(135, 76);
            this.steamid3.Margin = new System.Windows.Forms.Padding(0);
            this.steamid3.MaxLength = 17;
            this.steamid3.Name = "steamid3";
            this.steamid3.Size = new System.Drawing.Size(133, 21);
            this.steamid3.TabIndex = 277;
            // 
            // steamid2
            // 
            this.steamid2.BackColor = System.Drawing.Color.White;
            this.steamid2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid2.ForeColor = System.Drawing.Color.Black;
            this.steamid2.Location = new System.Drawing.Point(135, 54);
            this.steamid2.Margin = new System.Windows.Forms.Padding(0);
            this.steamid2.MaxLength = 17;
            this.steamid2.Name = "steamid2";
            this.steamid2.Size = new System.Drawing.Size(133, 21);
            this.steamid2.TabIndex = 276;
            // 
            // steamid1
            // 
            this.steamid1.BackColor = System.Drawing.Color.White;
            this.steamid1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid1.ForeColor = System.Drawing.Color.Black;
            this.steamid1.Location = new System.Drawing.Point(135, 32);
            this.steamid1.Margin = new System.Windows.Forms.Padding(0);
            this.steamid1.MaxLength = 17;
            this.steamid1.Name = "steamid1";
            this.steamid1.Size = new System.Drawing.Size(133, 21);
            this.steamid1.TabIndex = 275;
            // 
            // player16N
            // 
            this.player16N.BackColor = System.Drawing.Color.White;
            this.player16N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player16N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player16N.ForeColor = System.Drawing.Color.Black;
            this.player16N.Location = new System.Drawing.Point(419, 186);
            this.player16N.Margin = new System.Windows.Forms.Padding(0);
            this.player16N.MaxLength = 9;
            this.player16N.Name = "player16N";
            this.player16N.Size = new System.Drawing.Size(69, 21);
            this.player16N.TabIndex = 273;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(361, 189);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(54, 13);
            this.label17.TabIndex = 274;
            this.label17.Text = "Player 16:";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player15N
            // 
            this.player15N.BackColor = System.Drawing.Color.White;
            this.player15N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player15N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player15N.ForeColor = System.Drawing.Color.Black;
            this.player15N.Location = new System.Drawing.Point(419, 164);
            this.player15N.Margin = new System.Windows.Forms.Padding(0);
            this.player15N.MaxLength = 9;
            this.player15N.Name = "player15N";
            this.player15N.Size = new System.Drawing.Size(69, 21);
            this.player15N.TabIndex = 271;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(361, 168);
            this.label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(54, 13);
            this.label18.TabIndex = 272;
            this.label18.Text = "Player 15:";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player14N
            // 
            this.player14N.BackColor = System.Drawing.Color.White;
            this.player14N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player14N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player14N.ForeColor = System.Drawing.Color.Black;
            this.player14N.Location = new System.Drawing.Point(419, 142);
            this.player14N.Margin = new System.Windows.Forms.Padding(0);
            this.player14N.MaxLength = 9;
            this.player14N.Name = "player14N";
            this.player14N.Size = new System.Drawing.Size(69, 21);
            this.player14N.TabIndex = 269;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(361, 145);
            this.label19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(54, 13);
            this.label19.TabIndex = 270;
            this.label19.Text = "Player 14:";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player13N
            // 
            this.player13N.BackColor = System.Drawing.Color.White;
            this.player13N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player13N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player13N.ForeColor = System.Drawing.Color.Black;
            this.player13N.Location = new System.Drawing.Point(419, 120);
            this.player13N.Margin = new System.Windows.Forms.Padding(0);
            this.player13N.MaxLength = 9;
            this.player13N.Name = "player13N";
            this.player13N.Size = new System.Drawing.Size(69, 21);
            this.player13N.TabIndex = 267;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(361, 124);
            this.label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(54, 13);
            this.label20.TabIndex = 268;
            this.label20.Text = "Player 13:";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player12N
            // 
            this.player12N.BackColor = System.Drawing.Color.White;
            this.player12N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player12N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player12N.ForeColor = System.Drawing.Color.Black;
            this.player12N.Location = new System.Drawing.Point(419, 98);
            this.player12N.Margin = new System.Windows.Forms.Padding(0);
            this.player12N.MaxLength = 9;
            this.player12N.Name = "player12N";
            this.player12N.Size = new System.Drawing.Size(69, 21);
            this.player12N.TabIndex = 265;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(361, 101);
            this.label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(54, 13);
            this.label22.TabIndex = 266;
            this.label22.Text = "Player 12:";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player11N
            // 
            this.player11N.BackColor = System.Drawing.Color.White;
            this.player11N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player11N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player11N.ForeColor = System.Drawing.Color.Black;
            this.player11N.Location = new System.Drawing.Point(419, 76);
            this.player11N.Margin = new System.Windows.Forms.Padding(0);
            this.player11N.MaxLength = 9;
            this.player11N.Name = "player11N";
            this.player11N.Size = new System.Drawing.Size(69, 21);
            this.player11N.TabIndex = 264;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(361, 79);
            this.label23.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(54, 13);
            this.label23.TabIndex = 263;
            this.label23.Text = "Player 11:";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player10N
            // 
            this.player10N.BackColor = System.Drawing.Color.White;
            this.player10N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player10N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player10N.ForeColor = System.Drawing.Color.Black;
            this.player10N.Location = new System.Drawing.Point(419, 54);
            this.player10N.Margin = new System.Windows.Forms.Padding(0);
            this.player10N.MaxLength = 9;
            this.player10N.Name = "player10N";
            this.player10N.Size = new System.Drawing.Size(69, 21);
            this.player10N.TabIndex = 262;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(361, 57);
            this.label24.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(54, 13);
            this.label24.TabIndex = 260;
            this.label24.Text = "Player 10:";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player9N
            // 
            this.player9N.BackColor = System.Drawing.Color.White;
            this.player9N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player9N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player9N.ForeColor = System.Drawing.Color.Black;
            this.player9N.Location = new System.Drawing.Point(419, 32);
            this.player9N.Margin = new System.Windows.Forms.Padding(0);
            this.player9N.MaxLength = 9;
            this.player9N.Name = "player9N";
            this.player9N.Size = new System.Drawing.Size(69, 21);
            this.player9N.TabIndex = 261;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.Location = new System.Drawing.Point(367, 35);
            this.label26.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(48, 13);
            this.label26.TabIndex = 259;
            this.label26.Text = "Player 9:";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player8N
            // 
            this.player8N.BackColor = System.Drawing.Color.White;
            this.player8N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player8N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player8N.ForeColor = System.Drawing.Color.Black;
            this.player8N.Location = new System.Drawing.Point(59, 186);
            this.player8N.Margin = new System.Windows.Forms.Padding(0);
            this.player8N.MaxLength = 9;
            this.player8N.Name = "player8N";
            this.player8N.Size = new System.Drawing.Size(69, 21);
            this.player8N.TabIndex = 257;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(7, 189);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(48, 13);
            this.label16.TabIndex = 258;
            this.label16.Text = "Player 8:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player7N
            // 
            this.player7N.BackColor = System.Drawing.Color.White;
            this.player7N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player7N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player7N.ForeColor = System.Drawing.Color.Black;
            this.player7N.Location = new System.Drawing.Point(59, 164);
            this.player7N.Margin = new System.Windows.Forms.Padding(0);
            this.player7N.MaxLength = 9;
            this.player7N.Name = "player7N";
            this.player7N.Size = new System.Drawing.Size(69, 21);
            this.player7N.TabIndex = 255;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(7, 167);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(48, 13);
            this.label15.TabIndex = 256;
            this.label15.Text = "Player 7:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player6N
            // 
            this.player6N.BackColor = System.Drawing.Color.White;
            this.player6N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player6N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player6N.ForeColor = System.Drawing.Color.Black;
            this.player6N.Location = new System.Drawing.Point(59, 142);
            this.player6N.Margin = new System.Windows.Forms.Padding(0);
            this.player6N.MaxLength = 9;
            this.player6N.Name = "player6N";
            this.player6N.Size = new System.Drawing.Size(69, 21);
            this.player6N.TabIndex = 253;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(7, 145);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(48, 13);
            this.label11.TabIndex = 254;
            this.label11.Text = "Player 6:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player5N
            // 
            this.player5N.BackColor = System.Drawing.Color.White;
            this.player5N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player5N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player5N.ForeColor = System.Drawing.Color.Black;
            this.player5N.Location = new System.Drawing.Point(59, 120);
            this.player5N.Margin = new System.Windows.Forms.Padding(0);
            this.player5N.MaxLength = 9;
            this.player5N.Name = "player5N";
            this.player5N.Size = new System.Drawing.Size(69, 21);
            this.player5N.TabIndex = 251;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(7, 123);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 13);
            this.label9.TabIndex = 252;
            this.label9.Text = "Player 5:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player4N
            // 
            this.player4N.BackColor = System.Drawing.Color.White;
            this.player4N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player4N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player4N.ForeColor = System.Drawing.Color.Black;
            this.player4N.Location = new System.Drawing.Point(59, 98);
            this.player4N.Margin = new System.Windows.Forms.Padding(0);
            this.player4N.MaxLength = 9;
            this.player4N.Name = "player4N";
            this.player4N.Size = new System.Drawing.Size(69, 21);
            this.player4N.TabIndex = 249;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(7, 101);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(48, 13);
            this.label14.TabIndex = 250;
            this.label14.Text = "Player 4:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player3N
            // 
            this.player3N.BackColor = System.Drawing.Color.White;
            this.player3N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player3N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player3N.ForeColor = System.Drawing.Color.Black;
            this.player3N.Location = new System.Drawing.Point(59, 76);
            this.player3N.Margin = new System.Windows.Forms.Padding(0);
            this.player3N.MaxLength = 9;
            this.player3N.Name = "player3N";
            this.player3N.Size = new System.Drawing.Size(69, 21);
            this.player3N.TabIndex = 248;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(7, 79);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(48, 13);
            this.label12.TabIndex = 247;
            this.label12.Text = "Player 3:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player2N
            // 
            this.player2N.BackColor = System.Drawing.Color.White;
            this.player2N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player2N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player2N.ForeColor = System.Drawing.Color.Black;
            this.player2N.Location = new System.Drawing.Point(59, 54);
            this.player2N.Margin = new System.Windows.Forms.Padding(0);
            this.player2N.MaxLength = 9;
            this.player2N.Name = "player2N";
            this.player2N.Size = new System.Drawing.Size(69, 21);
            this.player2N.TabIndex = 246;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(7, 57);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 13);
            this.label10.TabIndex = 244;
            this.label10.Text = "Player 2:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player1N
            // 
            this.player1N.BackColor = System.Drawing.Color.White;
            this.player1N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player1N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player1N.ForeColor = System.Drawing.Color.Black;
            this.player1N.Location = new System.Drawing.Point(59, 32);
            this.player1N.Margin = new System.Windows.Forms.Padding(0);
            this.player1N.MaxLength = 9;
            this.player1N.Name = "player1N";
            this.player1N.Size = new System.Drawing.Size(69, 21);
            this.player1N.TabIndex = 245;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(7, 35);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 243;
            this.label7.Text = "Player 1:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // page2
            // 
            this.page2.Controls.Add(this.label3);
            this.page2.Controls.Add(this.label62);
            this.page2.Controls.Add(this.label63);
            this.page2.Controls.Add(this.label64);
            this.page2.Controls.Add(this.label65);
            this.page2.Controls.Add(this.label66);
            this.page2.Controls.Add(this.steamid32);
            this.page2.Controls.Add(this.steamid17);
            this.page2.Controls.Add(this.steamid31);
            this.page2.Controls.Add(this.label61);
            this.page2.Controls.Add(this.steamid30);
            this.page2.Controls.Add(this.player17N);
            this.page2.Controls.Add(this.steamid29);
            this.page2.Controls.Add(this.label59);
            this.page2.Controls.Add(this.steamid28);
            this.page2.Controls.Add(this.player18N);
            this.page2.Controls.Add(this.steamid27);
            this.page2.Controls.Add(this.label58);
            this.page2.Controls.Add(this.steamid26);
            this.page2.Controls.Add(this.player19N);
            this.page2.Controls.Add(this.steamid25);
            this.page2.Controls.Add(this.label57);
            this.page2.Controls.Add(this.steamid24);
            this.page2.Controls.Add(this.player20N);
            this.page2.Controls.Add(this.steamid23);
            this.page2.Controls.Add(this.label55);
            this.page2.Controls.Add(this.steamid22);
            this.page2.Controls.Add(this.player21N);
            this.page2.Controls.Add(this.steamid21);
            this.page2.Controls.Add(this.label54);
            this.page2.Controls.Add(this.steamid20);
            this.page2.Controls.Add(this.player22N);
            this.page2.Controls.Add(this.steamid19);
            this.page2.Controls.Add(this.label53);
            this.page2.Controls.Add(this.steamid18);
            this.page2.Controls.Add(this.player23N);
            this.page2.Controls.Add(this.label52);
            this.page2.Controls.Add(this.player32N);
            this.page2.Controls.Add(this.player24N);
            this.page2.Controls.Add(this.label31);
            this.page2.Controls.Add(this.label51);
            this.page2.Controls.Add(this.player31N);
            this.page2.Controls.Add(this.player25N);
            this.page2.Controls.Add(this.label33);
            this.page2.Controls.Add(this.label50);
            this.page2.Controls.Add(this.player30N);
            this.page2.Controls.Add(this.player26N);
            this.page2.Controls.Add(this.label34);
            this.page2.Controls.Add(this.label46);
            this.page2.Controls.Add(this.player29N);
            this.page2.Controls.Add(this.player27N);
            this.page2.Controls.Add(this.label35);
            this.page2.Controls.Add(this.label38);
            this.page2.Controls.Add(this.player28N);
            this.page2.Location = new System.Drawing.Point(9, 100);
            this.page2.Name = "page2";
            this.page2.Size = new System.Drawing.Size(634, 217);
            this.page2.TabIndex = 292;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(536, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 355;
            this.label3.Text = "Steam Id";
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label62.Location = new System.Drawing.Point(176, 9);
            this.label62.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(49, 13);
            this.label62.TabIndex = 354;
            this.label62.Text = "Steam Id";
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label63.Location = new System.Drawing.Point(366, 9);
            this.label63.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(48, 13);
            this.label63.TabIndex = 353;
            this.label63.Text = "Player Id";
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label64.Location = new System.Drawing.Point(428, 9);
            this.label64.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(55, 13);
            this.label64.TabIndex = 352;
            this.label64.Text = "Nickname";
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label65.Location = new System.Drawing.Point(4, 9);
            this.label65.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(51, 13);
            this.label65.TabIndex = 351;
            this.label65.Text = " Player Id";
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label66.Location = new System.Drawing.Point(68, 9);
            this.label66.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(55, 13);
            this.label66.TabIndex = 350;
            this.label66.Text = "Nickname";
            // 
            // steamid32
            // 
            this.steamid32.BackColor = System.Drawing.Color.White;
            this.steamid32.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid32.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid32.ForeColor = System.Drawing.Color.Black;
            this.steamid32.Location = new System.Drawing.Point(495, 186);
            this.steamid32.Margin = new System.Windows.Forms.Padding(0);
            this.steamid32.MaxLength = 17;
            this.steamid32.Name = "steamid32";
            this.steamid32.Size = new System.Drawing.Size(133, 21);
            this.steamid32.TabIndex = 349;
            // 
            // steamid17
            // 
            this.steamid17.BackColor = System.Drawing.Color.White;
            this.steamid17.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid17.ForeColor = System.Drawing.Color.Black;
            this.steamid17.Location = new System.Drawing.Point(135, 32);
            this.steamid17.Margin = new System.Windows.Forms.Padding(0);
            this.steamid17.MaxLength = 17;
            this.steamid17.Name = "steamid17";
            this.steamid17.Size = new System.Drawing.Size(133, 21);
            this.steamid17.TabIndex = 334;
            // 
            // steamid31
            // 
            this.steamid31.BackColor = System.Drawing.Color.White;
            this.steamid31.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid31.ForeColor = System.Drawing.Color.Black;
            this.steamid31.Location = new System.Drawing.Point(495, 164);
            this.steamid31.Margin = new System.Windows.Forms.Padding(0);
            this.steamid31.MaxLength = 17;
            this.steamid31.Name = "steamid31";
            this.steamid31.Size = new System.Drawing.Size(133, 21);
            this.steamid31.TabIndex = 348;
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label61.Location = new System.Drawing.Point(1, 35);
            this.label61.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(54, 13);
            this.label61.TabIndex = 302;
            this.label61.Text = "Player 17:";
            this.label61.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // steamid30
            // 
            this.steamid30.BackColor = System.Drawing.Color.White;
            this.steamid30.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid30.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid30.ForeColor = System.Drawing.Color.Black;
            this.steamid30.Location = new System.Drawing.Point(495, 142);
            this.steamid30.Margin = new System.Windows.Forms.Padding(0);
            this.steamid30.MaxLength = 17;
            this.steamid30.Name = "steamid30";
            this.steamid30.Size = new System.Drawing.Size(133, 21);
            this.steamid30.TabIndex = 347;
            // 
            // player17N
            // 
            this.player17N.BackColor = System.Drawing.Color.White;
            this.player17N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player17N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player17N.ForeColor = System.Drawing.Color.Black;
            this.player17N.Location = new System.Drawing.Point(59, 32);
            this.player17N.Margin = new System.Windows.Forms.Padding(0);
            this.player17N.MaxLength = 9;
            this.player17N.Name = "player17N";
            this.player17N.Size = new System.Drawing.Size(69, 21);
            this.player17N.TabIndex = 304;
            // 
            // steamid29
            // 
            this.steamid29.BackColor = System.Drawing.Color.White;
            this.steamid29.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid29.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid29.ForeColor = System.Drawing.Color.Black;
            this.steamid29.Location = new System.Drawing.Point(495, 120);
            this.steamid29.Margin = new System.Windows.Forms.Padding(0);
            this.steamid29.MaxLength = 17;
            this.steamid29.Name = "steamid29";
            this.steamid29.Size = new System.Drawing.Size(133, 21);
            this.steamid29.TabIndex = 346;
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label59.Location = new System.Drawing.Point(1, 57);
            this.label59.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(54, 13);
            this.label59.TabIndex = 303;
            this.label59.Text = "Player 18:";
            this.label59.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // steamid28
            // 
            this.steamid28.BackColor = System.Drawing.Color.White;
            this.steamid28.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid28.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid28.ForeColor = System.Drawing.Color.Black;
            this.steamid28.Location = new System.Drawing.Point(495, 98);
            this.steamid28.Margin = new System.Windows.Forms.Padding(0);
            this.steamid28.MaxLength = 17;
            this.steamid28.Name = "steamid28";
            this.steamid28.Size = new System.Drawing.Size(133, 21);
            this.steamid28.TabIndex = 345;
            // 
            // player18N
            // 
            this.player18N.BackColor = System.Drawing.Color.White;
            this.player18N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player18N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player18N.ForeColor = System.Drawing.Color.Black;
            this.player18N.Location = new System.Drawing.Point(59, 54);
            this.player18N.Margin = new System.Windows.Forms.Padding(0);
            this.player18N.MaxLength = 9;
            this.player18N.Name = "player18N";
            this.player18N.Size = new System.Drawing.Size(69, 21);
            this.player18N.TabIndex = 305;
            // 
            // steamid27
            // 
            this.steamid27.BackColor = System.Drawing.Color.White;
            this.steamid27.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid27.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid27.ForeColor = System.Drawing.Color.Black;
            this.steamid27.Location = new System.Drawing.Point(495, 76);
            this.steamid27.Margin = new System.Windows.Forms.Padding(0);
            this.steamid27.MaxLength = 17;
            this.steamid27.Name = "steamid27";
            this.steamid27.Size = new System.Drawing.Size(133, 21);
            this.steamid27.TabIndex = 344;
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label58.Location = new System.Drawing.Point(1, 79);
            this.label58.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(54, 13);
            this.label58.TabIndex = 306;
            this.label58.Text = "Player 19:";
            this.label58.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // steamid26
            // 
            this.steamid26.BackColor = System.Drawing.Color.White;
            this.steamid26.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid26.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid26.ForeColor = System.Drawing.Color.Black;
            this.steamid26.Location = new System.Drawing.Point(495, 54);
            this.steamid26.Margin = new System.Windows.Forms.Padding(0);
            this.steamid26.MaxLength = 17;
            this.steamid26.Name = "steamid26";
            this.steamid26.Size = new System.Drawing.Size(133, 21);
            this.steamid26.TabIndex = 343;
            // 
            // player19N
            // 
            this.player19N.BackColor = System.Drawing.Color.White;
            this.player19N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player19N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player19N.ForeColor = System.Drawing.Color.Black;
            this.player19N.Location = new System.Drawing.Point(59, 76);
            this.player19N.Margin = new System.Windows.Forms.Padding(0);
            this.player19N.MaxLength = 9;
            this.player19N.Name = "player19N";
            this.player19N.Size = new System.Drawing.Size(69, 21);
            this.player19N.TabIndex = 307;
            // 
            // steamid25
            // 
            this.steamid25.BackColor = System.Drawing.Color.White;
            this.steamid25.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid25.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid25.ForeColor = System.Drawing.Color.Black;
            this.steamid25.Location = new System.Drawing.Point(495, 32);
            this.steamid25.Margin = new System.Windows.Forms.Padding(0);
            this.steamid25.MaxLength = 17;
            this.steamid25.Name = "steamid25";
            this.steamid25.Size = new System.Drawing.Size(133, 21);
            this.steamid25.TabIndex = 342;
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label57.Location = new System.Drawing.Point(1, 101);
            this.label57.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(54, 13);
            this.label57.TabIndex = 309;
            this.label57.Text = "Player 20:";
            this.label57.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // steamid24
            // 
            this.steamid24.BackColor = System.Drawing.Color.White;
            this.steamid24.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid24.ForeColor = System.Drawing.Color.Black;
            this.steamid24.Location = new System.Drawing.Point(135, 186);
            this.steamid24.Margin = new System.Windows.Forms.Padding(0);
            this.steamid24.MaxLength = 17;
            this.steamid24.Name = "steamid24";
            this.steamid24.Size = new System.Drawing.Size(133, 21);
            this.steamid24.TabIndex = 341;
            // 
            // player20N
            // 
            this.player20N.BackColor = System.Drawing.Color.White;
            this.player20N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player20N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player20N.ForeColor = System.Drawing.Color.Black;
            this.player20N.Location = new System.Drawing.Point(59, 98);
            this.player20N.Margin = new System.Windows.Forms.Padding(0);
            this.player20N.MaxLength = 9;
            this.player20N.Name = "player20N";
            this.player20N.Size = new System.Drawing.Size(69, 21);
            this.player20N.TabIndex = 308;
            // 
            // steamid23
            // 
            this.steamid23.BackColor = System.Drawing.Color.White;
            this.steamid23.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid23.ForeColor = System.Drawing.Color.Black;
            this.steamid23.Location = new System.Drawing.Point(135, 164);
            this.steamid23.Margin = new System.Windows.Forms.Padding(0);
            this.steamid23.MaxLength = 17;
            this.steamid23.Name = "steamid23";
            this.steamid23.Size = new System.Drawing.Size(133, 21);
            this.steamid23.TabIndex = 340;
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label55.Location = new System.Drawing.Point(1, 122);
            this.label55.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(54, 13);
            this.label55.TabIndex = 311;
            this.label55.Text = "Player 21:";
            this.label55.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // steamid22
            // 
            this.steamid22.BackColor = System.Drawing.Color.White;
            this.steamid22.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid22.ForeColor = System.Drawing.Color.Black;
            this.steamid22.Location = new System.Drawing.Point(135, 142);
            this.steamid22.Margin = new System.Windows.Forms.Padding(0);
            this.steamid22.MaxLength = 17;
            this.steamid22.Name = "steamid22";
            this.steamid22.Size = new System.Drawing.Size(133, 21);
            this.steamid22.TabIndex = 339;
            // 
            // player21N
            // 
            this.player21N.BackColor = System.Drawing.Color.White;
            this.player21N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player21N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player21N.ForeColor = System.Drawing.Color.Black;
            this.player21N.Location = new System.Drawing.Point(59, 120);
            this.player21N.Margin = new System.Windows.Forms.Padding(0);
            this.player21N.MaxLength = 9;
            this.player21N.Name = "player21N";
            this.player21N.Size = new System.Drawing.Size(69, 21);
            this.player21N.TabIndex = 310;
            // 
            // steamid21
            // 
            this.steamid21.BackColor = System.Drawing.Color.White;
            this.steamid21.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid21.ForeColor = System.Drawing.Color.Black;
            this.steamid21.Location = new System.Drawing.Point(135, 120);
            this.steamid21.Margin = new System.Windows.Forms.Padding(0);
            this.steamid21.MaxLength = 17;
            this.steamid21.Name = "steamid21";
            this.steamid21.Size = new System.Drawing.Size(133, 21);
            this.steamid21.TabIndex = 338;
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label54.Location = new System.Drawing.Point(1, 145);
            this.label54.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(54, 13);
            this.label54.TabIndex = 313;
            this.label54.Text = "Player 22:";
            this.label54.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // steamid20
            // 
            this.steamid20.BackColor = System.Drawing.Color.White;
            this.steamid20.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid20.ForeColor = System.Drawing.Color.Black;
            this.steamid20.Location = new System.Drawing.Point(135, 98);
            this.steamid20.Margin = new System.Windows.Forms.Padding(0);
            this.steamid20.MaxLength = 17;
            this.steamid20.Name = "steamid20";
            this.steamid20.Size = new System.Drawing.Size(133, 21);
            this.steamid20.TabIndex = 337;
            // 
            // player22N
            // 
            this.player22N.BackColor = System.Drawing.Color.White;
            this.player22N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player22N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player22N.ForeColor = System.Drawing.Color.Black;
            this.player22N.Location = new System.Drawing.Point(59, 142);
            this.player22N.Margin = new System.Windows.Forms.Padding(0);
            this.player22N.MaxLength = 9;
            this.player22N.Name = "player22N";
            this.player22N.Size = new System.Drawing.Size(69, 21);
            this.player22N.TabIndex = 312;
            // 
            // steamid19
            // 
            this.steamid19.BackColor = System.Drawing.Color.White;
            this.steamid19.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid19.ForeColor = System.Drawing.Color.Black;
            this.steamid19.Location = new System.Drawing.Point(135, 76);
            this.steamid19.Margin = new System.Windows.Forms.Padding(0);
            this.steamid19.MaxLength = 17;
            this.steamid19.Name = "steamid19";
            this.steamid19.Size = new System.Drawing.Size(133, 21);
            this.steamid19.TabIndex = 336;
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label53.Location = new System.Drawing.Point(1, 166);
            this.label53.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(54, 13);
            this.label53.TabIndex = 315;
            this.label53.Text = "Player 23:";
            this.label53.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // steamid18
            // 
            this.steamid18.BackColor = System.Drawing.Color.White;
            this.steamid18.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.steamid18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steamid18.ForeColor = System.Drawing.Color.Black;
            this.steamid18.Location = new System.Drawing.Point(135, 54);
            this.steamid18.Margin = new System.Windows.Forms.Padding(0);
            this.steamid18.MaxLength = 17;
            this.steamid18.Name = "steamid18";
            this.steamid18.Size = new System.Drawing.Size(133, 21);
            this.steamid18.TabIndex = 335;
            // 
            // player23N
            // 
            this.player23N.BackColor = System.Drawing.Color.White;
            this.player23N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player23N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player23N.ForeColor = System.Drawing.Color.Black;
            this.player23N.Location = new System.Drawing.Point(59, 164);
            this.player23N.Margin = new System.Windows.Forms.Padding(0);
            this.player23N.MaxLength = 9;
            this.player23N.Name = "player23N";
            this.player23N.Size = new System.Drawing.Size(69, 21);
            this.player23N.TabIndex = 314;
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label52.Location = new System.Drawing.Point(1, 187);
            this.label52.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(54, 13);
            this.label52.TabIndex = 317;
            this.label52.Text = "Player 24:";
            this.label52.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player32N
            // 
            this.player32N.BackColor = System.Drawing.Color.White;
            this.player32N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player32N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player32N.ForeColor = System.Drawing.Color.Black;
            this.player32N.Location = new System.Drawing.Point(419, 186);
            this.player32N.Margin = new System.Windows.Forms.Padding(0);
            this.player32N.MaxLength = 9;
            this.player32N.Name = "player32N";
            this.player32N.Size = new System.Drawing.Size(69, 21);
            this.player32N.TabIndex = 332;
            // 
            // player24N
            // 
            this.player24N.BackColor = System.Drawing.Color.White;
            this.player24N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player24N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player24N.ForeColor = System.Drawing.Color.Black;
            this.player24N.Location = new System.Drawing.Point(59, 186);
            this.player24N.Margin = new System.Windows.Forms.Padding(0);
            this.player24N.MaxLength = 9;
            this.player24N.Name = "player24N";
            this.player24N.Size = new System.Drawing.Size(69, 21);
            this.player24N.TabIndex = 316;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(361, 189);
            this.label31.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(54, 13);
            this.label31.TabIndex = 333;
            this.label31.Text = "Player 32:";
            this.label31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label51.Location = new System.Drawing.Point(361, 35);
            this.label51.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(54, 13);
            this.label51.TabIndex = 318;
            this.label51.Text = "Player 25:";
            this.label51.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player31N
            // 
            this.player31N.BackColor = System.Drawing.Color.White;
            this.player31N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player31N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player31N.ForeColor = System.Drawing.Color.Black;
            this.player31N.Location = new System.Drawing.Point(419, 164);
            this.player31N.Margin = new System.Windows.Forms.Padding(0);
            this.player31N.MaxLength = 9;
            this.player31N.Name = "player31N";
            this.player31N.Size = new System.Drawing.Size(69, 21);
            this.player31N.TabIndex = 330;
            // 
            // player25N
            // 
            this.player25N.BackColor = System.Drawing.Color.White;
            this.player25N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player25N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player25N.ForeColor = System.Drawing.Color.Black;
            this.player25N.Location = new System.Drawing.Point(419, 32);
            this.player25N.Margin = new System.Windows.Forms.Padding(0);
            this.player25N.MaxLength = 9;
            this.player25N.Name = "player25N";
            this.player25N.Size = new System.Drawing.Size(69, 21);
            this.player25N.TabIndex = 320;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.Location = new System.Drawing.Point(361, 168);
            this.label33.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(54, 13);
            this.label33.TabIndex = 331;
            this.label33.Text = "Player 31:";
            this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label50.Location = new System.Drawing.Point(361, 57);
            this.label50.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(54, 13);
            this.label50.TabIndex = 319;
            this.label50.Text = "Player 26:";
            this.label50.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player30N
            // 
            this.player30N.BackColor = System.Drawing.Color.White;
            this.player30N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player30N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player30N.ForeColor = System.Drawing.Color.Black;
            this.player30N.Location = new System.Drawing.Point(419, 142);
            this.player30N.Margin = new System.Windows.Forms.Padding(0);
            this.player30N.MaxLength = 9;
            this.player30N.Name = "player30N";
            this.player30N.Size = new System.Drawing.Size(69, 21);
            this.player30N.TabIndex = 328;
            // 
            // player26N
            // 
            this.player26N.BackColor = System.Drawing.Color.White;
            this.player26N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player26N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player26N.ForeColor = System.Drawing.Color.Black;
            this.player26N.Location = new System.Drawing.Point(419, 54);
            this.player26N.Margin = new System.Windows.Forms.Padding(0);
            this.player26N.MaxLength = 9;
            this.player26N.Name = "player26N";
            this.player26N.Size = new System.Drawing.Size(69, 21);
            this.player26N.TabIndex = 321;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.Location = new System.Drawing.Point(361, 145);
            this.label34.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(54, 13);
            this.label34.TabIndex = 329;
            this.label34.Text = "Player 30:";
            this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label46.Location = new System.Drawing.Point(361, 79);
            this.label46.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(54, 13);
            this.label46.TabIndex = 322;
            this.label46.Text = "Player 27:";
            this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player29N
            // 
            this.player29N.BackColor = System.Drawing.Color.White;
            this.player29N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player29N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player29N.ForeColor = System.Drawing.Color.Black;
            this.player29N.Location = new System.Drawing.Point(419, 120);
            this.player29N.Margin = new System.Windows.Forms.Padding(0);
            this.player29N.MaxLength = 9;
            this.player29N.Name = "player29N";
            this.player29N.Size = new System.Drawing.Size(69, 21);
            this.player29N.TabIndex = 326;
            // 
            // player27N
            // 
            this.player27N.BackColor = System.Drawing.Color.White;
            this.player27N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player27N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player27N.ForeColor = System.Drawing.Color.Black;
            this.player27N.Location = new System.Drawing.Point(419, 76);
            this.player27N.Margin = new System.Windows.Forms.Padding(0);
            this.player27N.MaxLength = 9;
            this.player27N.Name = "player27N";
            this.player27N.Size = new System.Drawing.Size(69, 21);
            this.player27N.TabIndex = 323;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label35.Location = new System.Drawing.Point(361, 124);
            this.label35.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(54, 13);
            this.label35.TabIndex = 327;
            this.label35.Text = "Player 29:";
            this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label38.Location = new System.Drawing.Point(361, 101);
            this.label38.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(54, 13);
            this.label38.TabIndex = 325;
            this.label38.Text = "Player 28:";
            this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // player28N
            // 
            this.player28N.BackColor = System.Drawing.Color.White;
            this.player28N.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.player28N.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player28N.ForeColor = System.Drawing.Color.Black;
            this.player28N.Location = new System.Drawing.Point(419, 98);
            this.player28N.Margin = new System.Windows.Forms.Padding(0);
            this.player28N.MaxLength = 9;
            this.player28N.Name = "player28N";
            this.player28N.Size = new System.Drawing.Size(69, 21);
            this.player28N.TabIndex = 324;
            // 
            // audioTab
            // 
            this.audioTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioTab.BackColor = System.Drawing.Color.RosyBrown;
            this.audioTab.Controls.Add(this.audioWarningLabel);
            this.audioTab.Controls.Add(this.audioDefaultDevice);
            this.audioTab.Controls.Add(this.audioCustomSettingsBox);
            this.audioTab.Controls.Add(this.audioCustomSettingsRadio);
            this.audioTab.Controls.Add(this.audioDefaultSettingsRadio);
            this.audioTab.Controls.Add(this.audioRefresh);
            this.audioTab.Location = new System.Drawing.Point(1, 34);
            this.audioTab.Name = "audioTab";
            this.audioTab.Size = new System.Drawing.Size(674, 391);
            this.audioTab.TabIndex = 292;
            // 
            // audioWarningLabel
            // 
            this.audioWarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioWarningLabel.AutoSize = true;
            this.audioWarningLabel.BackColor = System.Drawing.Color.Transparent;
            this.audioWarningLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.audioWarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.audioWarningLabel.ForeColor = System.Drawing.Color.Yellow;
            this.audioWarningLabel.Location = new System.Drawing.Point(230, 22);
            this.audioWarningLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.audioWarningLabel.Name = "audioWarningLabel";
            this.audioWarningLabel.Size = new System.Drawing.Size(273, 15);
            this.audioWarningLabel.TabIndex = 22;
            this.audioWarningLabel.Text = "This Feature May Not Work For All Games";
            this.audioWarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.audioWarningLabel.UseMnemonic = false;
            // 
            // audioDefaultDevice
            // 
            this.audioDefaultDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioDefaultDevice.AutoSize = true;
            this.audioDefaultDevice.BackColor = System.Drawing.Color.Transparent;
            this.audioDefaultDevice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.audioDefaultDevice.ForeColor = System.Drawing.Color.White;
            this.audioDefaultDevice.Location = new System.Drawing.Point(86, 136);
            this.audioDefaultDevice.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.audioDefaultDevice.Name = "audioDefaultDevice";
            this.audioDefaultDevice.Size = new System.Drawing.Size(0, 15);
            this.audioDefaultDevice.TabIndex = 21;
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
            this.audioCustomSettingsBox.Location = new System.Drawing.Point(88, 156);
            this.audioCustomSettingsBox.Margin = new System.Windows.Forms.Padding(2);
            this.audioCustomSettingsBox.Name = "audioCustomSettingsBox";
            this.audioCustomSettingsBox.Padding = new System.Windows.Forms.Padding(2);
            this.audioCustomSettingsBox.Size = new System.Drawing.Size(503, 121);
            this.audioCustomSettingsBox.TabIndex = 20;
            this.audioCustomSettingsBox.TabStop = false;
            // 
            // AudioInstance8
            // 
            this.AudioInstance8.BackColor = System.Drawing.Color.White;
            this.AudioInstance8.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance8.FormattingEnabled = true;
            this.AudioInstance8.Location = new System.Drawing.Point(321, 84);
            this.AudioInstance8.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance8.Name = "AudioInstance8";
            this.AudioInstance8.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance8.TabIndex = 29;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.AudioInstance7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance7.FormattingEnabled = true;
            this.AudioInstance7.Location = new System.Drawing.Point(321, 62);
            this.AudioInstance7.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance7.Name = "AudioInstance7";
            this.AudioInstance7.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance7.TabIndex = 27;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.AudioInstance6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance6.FormattingEnabled = true;
            this.AudioInstance6.Location = new System.Drawing.Point(321, 40);
            this.AudioInstance6.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance6.Name = "AudioInstance6";
            this.AudioInstance6.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance6.TabIndex = 25;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.AudioInstance5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance5.FormattingEnabled = true;
            this.AudioInstance5.Location = new System.Drawing.Point(321, 18);
            this.AudioInstance5.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance5.Name = "AudioInstance5";
            this.AudioInstance5.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance5.TabIndex = 23;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.AudioInstance4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance4.FormattingEnabled = true;
            this.AudioInstance4.Location = new System.Drawing.Point(70, 84);
            this.AudioInstance4.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance4.Name = "AudioInstance4";
            this.AudioInstance4.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance4.TabIndex = 21;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.AudioInstance3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance3.FormattingEnabled = true;
            this.AudioInstance3.Location = new System.Drawing.Point(70, 62);
            this.AudioInstance3.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance3.Name = "AudioInstance3";
            this.AudioInstance3.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance3.TabIndex = 19;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.AudioInstance2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance2.FormattingEnabled = true;
            this.AudioInstance2.Location = new System.Drawing.Point(70, 40);
            this.AudioInstance2.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance2.Name = "AudioInstance2";
            this.AudioInstance2.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance2.TabIndex = 6;
            // 
            // AudioInstance1
            // 
            this.AudioInstance1.BackColor = System.Drawing.Color.White;
            this.AudioInstance1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AudioInstance1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AudioInstance1.FormattingEnabled = true;
            this.AudioInstance1.Items.AddRange(new object[] {
            "Default"});
            this.AudioInstance1.Location = new System.Drawing.Point(70, 18);
            this.AudioInstance1.Margin = new System.Windows.Forms.Padding(2);
            this.AudioInstance1.Name = "AudioInstance1";
            this.AudioInstance1.Size = new System.Drawing.Size(170, 23);
            this.AudioInstance1.TabIndex = 1;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.label36.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label36.Location = new System.Drawing.Point(6, 21);
            this.label36.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(60, 13);
            this.label36.TabIndex = 0;
            this.label36.Text = "Instance 1:";
            // 
            // audioCustomSettingsRadio
            // 
            this.audioCustomSettingsRadio.BackColor = System.Drawing.Color.Transparent;
            this.audioCustomSettingsRadio.BorderColor = System.Drawing.Color.Blue;
            this.audioCustomSettingsRadio.CheckColor = System.Drawing.Color.Blue;
            this.audioCustomSettingsRadio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.audioCustomSettingsRadio.ForeColor = System.Drawing.Color.White;
            this.audioCustomSettingsRadio.Location = new System.Drawing.Point(88, 109);
            this.audioCustomSettingsRadio.Margin = new System.Windows.Forms.Padding(2);
            this.audioCustomSettingsRadio.Name = "audioCustomSettingsRadio";
            this.audioCustomSettingsRadio.SelectionColor = System.Drawing.Color.LightBlue;
            this.audioCustomSettingsRadio.Size = new System.Drawing.Size(99, 17);
            this.audioCustomSettingsRadio.TabIndex = 19;
            this.audioCustomSettingsRadio.Text = "Custom settings";
            this.audioCustomSettingsRadio.UseVisualStyleBackColor = false;
            this.audioCustomSettingsRadio.CheckedChanged += new System.EventHandler(this.AudioCustomSettingsRadio_CheckedChanged);
            // 
            // audioDefaultSettingsRadio
            // 
            this.audioDefaultSettingsRadio.BackColor = System.Drawing.Color.Transparent;
            this.audioDefaultSettingsRadio.BorderColor = System.Drawing.Color.Blue;
            this.audioDefaultSettingsRadio.CheckColor = System.Drawing.Color.Blue;
            this.audioDefaultSettingsRadio.Checked = true;
            this.audioDefaultSettingsRadio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.audioDefaultSettingsRadio.ForeColor = System.Drawing.Color.White;
            this.audioDefaultSettingsRadio.Location = new System.Drawing.Point(88, 89);
            this.audioDefaultSettingsRadio.Margin = new System.Windows.Forms.Padding(2);
            this.audioDefaultSettingsRadio.Name = "audioDefaultSettingsRadio";
            this.audioDefaultSettingsRadio.SelectionColor = System.Drawing.Color.LightBlue;
            this.audioDefaultSettingsRadio.Size = new System.Drawing.Size(217, 17);
            this.audioDefaultSettingsRadio.TabIndex = 18;
            this.audioDefaultSettingsRadio.TabStop = true;
            this.audioDefaultSettingsRadio.Text = "Use default audio playback device for all";
            this.audioDefaultSettingsRadio.UseVisualStyleBackColor = false;
            // 
            // audioRefresh
            // 
            this.audioRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioRefresh.BackColor = System.Drawing.Color.Orange;
            this.audioRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.audioRefresh.FlatAppearance.BorderSize = 0;
            this.audioRefresh.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.audioRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.audioRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.audioRefresh.Location = new System.Drawing.Point(318, 349);
            this.audioRefresh.Margin = new System.Windows.Forms.Padding(2);
            this.audioRefresh.Name = "audioRefresh";
            this.audioRefresh.Size = new System.Drawing.Size(26, 23);
            this.audioRefresh.TabIndex = 23;
            this.audioRefresh.UseVisualStyleBackColor = false;
            this.audioRefresh.Click += new System.EventHandler(this.AudioRefresh_Click);
            // 
            // layoutTab
            // 
            this.layoutTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutTab.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.layoutTab.Controls.Add(this.label5);
            this.layoutTab.Controls.Add(this.DivLabel);
            this.layoutTab.Controls.Add(this.smfw_Cmb);
            this.layoutTab.Controls.Add(this.plus9);
            this.layoutTab.Controls.Add(this.mergerShortcutLabel);
            this.layoutTab.Controls.Add(this.smfw_HKTxt);
            this.layoutTab.Controls.Add(this.refreshScreenDatasButton);
            this.layoutTab.Controls.Add(this.mergerResSelectorLabel);
            this.layoutTab.Controls.Add(this.label4);
            this.layoutTab.Controls.Add(this.mergerSettingsLabel);
            this.layoutTab.Controls.Add(this.losslessHook);
            this.layoutTab.Controls.Add(this.selectedRes);
            this.layoutTab.Controls.Add(this.screen_panel);
            this.layoutTab.Controls.Add(this.enable_WMerger);
            this.layoutTab.Controls.Add(this.hideDesktop);
            this.layoutTab.Controls.Add(this.numMaxPlyrs);
            this.layoutTab.Controls.Add(this.numUpDownVer);
            this.layoutTab.Controls.Add(this.label29);
            this.layoutTab.Controls.Add(this.label27);
            this.layoutTab.Controls.Add(this.label28);
            this.layoutTab.Controls.Add(this.numUpDownHor);
            this.layoutTab.Controls.Add(this.cutsceneLabel);
            this.layoutTab.Controls.Add(this.cts_Panel);
            this.layoutTab.Controls.Add(this.SplitColors);
            this.layoutTab.Controls.Add(this.splitDiv);
            this.layoutTab.Controls.Add(this.label49);
            this.layoutTab.Controls.Add(this.layoutSizer);
            this.layoutTab.Location = new System.Drawing.Point(0, 25);
            this.layoutTab.Name = "layoutTab";
            this.layoutTab.Size = new System.Drawing.Size(674, 391);
            this.layoutTab.TabIndex = 300;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(280, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 13);
            this.label5.TabIndex = 179;
            this.label5.Text = "Custom Screen Layout";
            // 
            // DivLabel
            // 
            this.DivLabel.AutoSize = true;
            this.DivLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DivLabel.Location = new System.Drawing.Point(13, 8);
            this.DivLabel.Name = "DivLabel";
            this.DivLabel.Size = new System.Drawing.Size(169, 13);
            this.DivLabel.TabIndex = 178;
            this.DivLabel.Text = "Splitscreen Division Settings";
            // 
            // smfw_Cmb
            // 
            this.smfw_Cmb.BackColor = System.Drawing.Color.White;
            this.smfw_Cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.smfw_Cmb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.smfw_Cmb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smfw_Cmb.FormattingEnabled = true;
            this.smfw_Cmb.Items.AddRange(new object[] {
            "Ctrl",
            "Alt",
            "Shift"});
            this.smfw_Cmb.Location = new System.Drawing.Point(14, 359);
            this.smfw_Cmb.Margin = new System.Windows.Forms.Padding(2);
            this.smfw_Cmb.Name = "smfw_Cmb";
            this.smfw_Cmb.Size = new System.Drawing.Size(67, 23);
            this.smfw_Cmb.TabIndex = 174;
            // 
            // plus9
            // 
            this.plus9.AutoSize = true;
            this.plus9.ForeColor = System.Drawing.Color.Black;
            this.plus9.Location = new System.Drawing.Point(84, 362);
            this.plus9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.plus9.Name = "plus9";
            this.plus9.Size = new System.Drawing.Size(14, 15);
            this.plus9.TabIndex = 177;
            this.plus9.Text = "+";
            this.plus9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mergerShortcutLabel
            // 
            this.mergerShortcutLabel.AutoSize = true;
            this.mergerShortcutLabel.Location = new System.Drawing.Point(12, 341);
            this.mergerShortcutLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.mergerShortcutLabel.Name = "mergerShortcutLabel";
            this.mergerShortcutLabel.Size = new System.Drawing.Size(204, 15);
            this.mergerShortcutLabel.TabIndex = 176;
            this.mergerShortcutLabel.Text = "Change Childs Foreground Window:";
            this.mergerShortcutLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // smfw_HKTxt
            // 
            this.smfw_HKTxt.BorderColor = System.Drawing.Color.Blue;
            this.smfw_HKTxt.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.smfw_HKTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smfw_HKTxt.Hint = null;
            this.smfw_HKTxt.Location = new System.Drawing.Point(98, 360);
            this.smfw_HKTxt.Margin = new System.Windows.Forms.Padding(2);
            this.smfw_HKTxt.MaxLength = 1;
            this.smfw_HKTxt.Name = "smfw_HKTxt";
            this.smfw_HKTxt.ShortcutsEnabled = false;
            this.smfw_HKTxt.Size = new System.Drawing.Size(36, 21);
            this.smfw_HKTxt.TabIndex = 175;
            this.smfw_HKTxt.Tag = "HotKeyTextBox";
            this.smfw_HKTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // refreshScreenDatasButton
            // 
            this.refreshScreenDatasButton.BackColor = System.Drawing.Color.Transparent;
            this.refreshScreenDatasButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.refreshScreenDatasButton.FlatAppearance.BorderSize = 0;
            this.refreshScreenDatasButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.refreshScreenDatasButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.refreshScreenDatasButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshScreenDatasButton.Location = new System.Drawing.Point(461, 234);
            this.refreshScreenDatasButton.Name = "refreshScreenDatasButton";
            this.refreshScreenDatasButton.Size = new System.Drawing.Size(18, 18);
            this.refreshScreenDatasButton.TabIndex = 173;
            this.refreshScreenDatasButton.UseVisualStyleBackColor = false;
            this.refreshScreenDatasButton.Click += new System.EventHandler(this.RefreshScreenDatasButton_Click);
            // 
            // mergerResSelectorLabel
            // 
            this.mergerResSelectorLabel.AutoSize = true;
            this.mergerResSelectorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mergerResSelectorLabel.Location = new System.Drawing.Point(280, 237);
            this.mergerResSelectorLabel.Name = "mergerResSelectorLabel";
            this.mergerResSelectorLabel.Size = new System.Drawing.Size(176, 13);
            this.mergerResSelectorLabel.TabIndex = 172;
            this.mergerResSelectorLabel.Text = "Select Windows Merger Resolution ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 273);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 171;
            this.label4.Text = "Window Resolution";
            // 
            // mergerSettingsLabel
            // 
            this.mergerSettingsLabel.AutoSize = true;
            this.mergerSettingsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mergerSettingsLabel.Location = new System.Drawing.Point(12, 228);
            this.mergerSettingsLabel.Name = "mergerSettingsLabel";
            this.mergerSettingsLabel.Size = new System.Drawing.Size(151, 13);
            this.mergerSettingsLabel.TabIndex = 170;
            this.mergerSettingsLabel.Text = "Windows Merger Settings";
            // 
            // losslessHook
            // 
            this.losslessHook.BackColor = System.Drawing.Color.Transparent;
            this.losslessHook.BorderColor = System.Drawing.Color.Blue;
            this.losslessHook.CheckColor = System.Drawing.Color.Blue;
            this.losslessHook.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.losslessHook.ForeColor = System.Drawing.Color.White;
            this.losslessHook.Location = new System.Drawing.Point(15, 320);
            this.losslessHook.Margin = new System.Windows.Forms.Padding(0);
            this.losslessHook.Name = "losslessHook";
            this.losslessHook.SelectionColor = System.Drawing.Color.LightBlue;
            this.losslessHook.Size = new System.Drawing.Size(131, 17);
            this.losslessHook.TabIndex = 169;
            this.losslessHook.Text = "Enable Lossless Hook";
            this.losslessHook.UseVisualStyleBackColor = false;
            this.losslessHook.CheckedChanged += new System.EventHandler(this.LosslessHook_CheckedChanged);
            // 
            // selectedRes
            // 
            this.selectedRes.BackColor = System.Drawing.SystemColors.MenuText;
            this.selectedRes.BorderColor = System.Drawing.Color.Blue;
            this.selectedRes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.selectedRes.ForeColor = System.Drawing.SystemColors.Menu;
            this.selectedRes.Hint = null;
            this.selectedRes.Location = new System.Drawing.Point(14, 291);
            this.selectedRes.Name = "selectedRes";
            this.selectedRes.ReadOnly = true;
            this.selectedRes.ShortcutsEnabled = false;
            this.selectedRes.Size = new System.Drawing.Size(99, 21);
            this.selectedRes.TabIndex = 168;
            // 
            // screen_panel
            // 
            this.screen_panel.Location = new System.Drawing.Point(283, 254);
            this.screen_panel.Name = "screen_panel";
            this.screen_panel.Size = new System.Drawing.Size(378, 131);
            this.screen_panel.TabIndex = 167;
            this.screen_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.Screen_panel_Paint);
            // 
            // enable_WMerger
            // 
            this.enable_WMerger.BackColor = System.Drawing.Color.DimGray;
            this.enable_WMerger.BorderColor = System.Drawing.Color.Blue;
            this.enable_WMerger.CheckColor = System.Drawing.Color.Blue;
            this.enable_WMerger.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.enable_WMerger.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enable_WMerger.ForeColor = System.Drawing.Color.White;
            this.enable_WMerger.Location = new System.Drawing.Point(15, 250);
            this.enable_WMerger.Margin = new System.Windows.Forms.Padding(0);
            this.enable_WMerger.Name = "enable_WMerger";
            this.enable_WMerger.SelectionColor = System.Drawing.Color.LightBlue;
            this.enable_WMerger.Size = new System.Drawing.Size(142, 17);
            this.enable_WMerger.TabIndex = 166;
            this.enable_WMerger.Text = "Enable Windows Merger";
            this.enable_WMerger.UseVisualStyleBackColor = true;
            // 
            // hideDesktop
            // 
            this.hideDesktop.BackColor = System.Drawing.Color.DimGray;
            this.hideDesktop.BorderColor = System.Drawing.Color.Blue;
            this.hideDesktop.CheckColor = System.Drawing.Color.Blue;
            this.hideDesktop.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.hideDesktop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hideDesktop.ForeColor = System.Drawing.Color.White;
            this.hideDesktop.Location = new System.Drawing.Point(16, 56);
            this.hideDesktop.Margin = new System.Windows.Forms.Padding(0);
            this.hideDesktop.Name = "hideDesktop";
            this.hideDesktop.SelectionColor = System.Drawing.Color.LightBlue;
            this.hideDesktop.Size = new System.Drawing.Size(115, 17);
            this.hideDesktop.TabIndex = 162;
            this.hideDesktop.Text = "Hide Desktop Only";
            this.hideDesktop.UseVisualStyleBackColor = true;
            // 
            // numMaxPlyrs
            // 
            this.numMaxPlyrs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numMaxPlyrs.BackColor = System.Drawing.Color.Transparent;
            this.numMaxPlyrs.Enabled = false;
            this.numMaxPlyrs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numMaxPlyrs.Location = new System.Drawing.Point(300, 169);
            this.numMaxPlyrs.Margin = new System.Windows.Forms.Padding(0);
            this.numMaxPlyrs.Name = "numMaxPlyrs";
            this.numMaxPlyrs.Size = new System.Drawing.Size(48, 20);
            this.numMaxPlyrs.TabIndex = 161;
            this.numMaxPlyrs.UpdownBackColor = System.Drawing.Color.Blue;
            this.numMaxPlyrs.Value = 0;
            // 
            // numUpDownVer
            // 
            this.numUpDownVer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numUpDownVer.BackColor = System.Drawing.Color.Transparent;
            this.numUpDownVer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numUpDownVer.Location = new System.Drawing.Point(298, 71);
            this.numUpDownVer.Margin = new System.Windows.Forms.Padding(0);
            this.numUpDownVer.Name = "numUpDownVer";
            this.numUpDownVer.Size = new System.Drawing.Size(48, 20);
            this.numUpDownVer.TabIndex = 159;
            this.numUpDownVer.UpdownBackColor = System.Drawing.Color.Blue;
            this.numUpDownVer.Value = 0;
            // 
            // label29
            // 
            this.label29.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label29.AutoSize = true;
            this.label29.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label29.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label29.Location = new System.Drawing.Point(290, 146);
            this.label29.Margin = new System.Windows.Forms.Padding(0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(64, 13);
            this.label29.TabIndex = 158;
            this.label29.Text = "Max Players";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label27
            // 
            this.label27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label27.AutoSize = true;
            this.label27.BackColor = System.Drawing.Color.Transparent;
            this.label27.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label27.Location = new System.Drawing.Point(290, 99);
            this.label27.Margin = new System.Windows.Forms.Padding(0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(99, 13);
            this.label27.TabIndex = 156;
            this.label27.Text = "Horizontal Divisions";
            this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label28
            // 
            this.label28.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label28.AutoSize = true;
            this.label28.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label28.Location = new System.Drawing.Point(290, 52);
            this.label28.Margin = new System.Windows.Forms.Padding(0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(87, 13);
            this.label28.TabIndex = 157;
            this.label28.Text = "Vertical Divisions";
            this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numUpDownHor
            // 
            this.numUpDownHor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numUpDownHor.BackColor = System.Drawing.Color.Transparent;
            this.numUpDownHor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numUpDownHor.Location = new System.Drawing.Point(299, 118);
            this.numUpDownHor.Margin = new System.Windows.Forms.Padding(0);
            this.numUpDownHor.Name = "numUpDownHor";
            this.numUpDownHor.Size = new System.Drawing.Size(48, 20);
            this.numUpDownHor.TabIndex = 160;
            this.numUpDownHor.UpdownBackColor = System.Drawing.Color.Blue;
            this.numUpDownHor.Value = 0;
            // 
            // cutsceneLabel
            // 
            this.cutsceneLabel.AutoSize = true;
            this.cutsceneLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cutsceneLabel.Location = new System.Drawing.Point(12, 128);
            this.cutsceneLabel.Name = "cutsceneLabel";
            this.cutsceneLabel.Size = new System.Drawing.Size(151, 13);
            this.cutsceneLabel.TabIndex = 155;
            this.cutsceneLabel.Text = "Cutscenes Mode Settings";
            // 
            // cts_Panel
            // 
            this.cts_Panel.BackColor = System.Drawing.Color.Transparent;
            this.cts_Panel.Controls.Add(this.cts_unfocus);
            this.cts_Panel.Controls.Add(this.cts_kar);
            this.cts_Panel.Controls.Add(this.cts_Mute);
            this.cts_Panel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cts_Panel.Location = new System.Drawing.Point(11, 144);
            this.cts_Panel.Name = "cts_Panel";
            this.cts_Panel.Size = new System.Drawing.Size(200, 74);
            this.cts_Panel.TabIndex = 154;
            // 
            // cts_unfocus
            // 
            this.cts_unfocus.BackColor = System.Drawing.Color.DimGray;
            this.cts_unfocus.BorderColor = System.Drawing.Color.Blue;
            this.cts_unfocus.CheckColor = System.Drawing.Color.Blue;
            this.cts_unfocus.ForeColor = System.Drawing.Color.White;
            this.cts_unfocus.Location = new System.Drawing.Point(4, 54);
            this.cts_unfocus.Name = "cts_unfocus";
            this.cts_unfocus.SelectionColor = System.Drawing.Color.LightBlue;
            this.cts_unfocus.Size = new System.Drawing.Size(138, 17);
            this.cts_unfocus.TabIndex = 142;
            this.cts_unfocus.Text = "Auto Unfocus Windows";
            this.cts_unfocus.UseVisualStyleBackColor = true;
            // 
            // cts_kar
            // 
            this.cts_kar.BackColor = System.Drawing.Color.DimGray;
            this.cts_kar.BorderColor = System.Drawing.Color.Blue;
            this.cts_kar.CheckColor = System.Drawing.Color.Blue;
            this.cts_kar.ForeColor = System.Drawing.Color.White;
            this.cts_kar.Location = new System.Drawing.Point(4, 28);
            this.cts_kar.Name = "cts_kar";
            this.cts_kar.SelectionColor = System.Drawing.Color.LightBlue;
            this.cts_kar.Size = new System.Drawing.Size(154, 17);
            this.cts_kar.TabIndex = 141;
            this.cts_kar.Text = "Keep Original Window Size";
            this.cts_kar.UseVisualStyleBackColor = true;
            // 
            // cts_Mute
            // 
            this.cts_Mute.BackColor = System.Drawing.Color.DimGray;
            this.cts_Mute.BorderColor = System.Drawing.Color.Blue;
            this.cts_Mute.CheckColor = System.Drawing.Color.Blue;
            this.cts_Mute.ForeColor = System.Drawing.Color.White;
            this.cts_Mute.Location = new System.Drawing.Point(4, 5);
            this.cts_Mute.Name = "cts_Mute";
            this.cts_Mute.SelectionColor = System.Drawing.Color.LightBlue;
            this.cts_Mute.Size = new System.Drawing.Size(104, 17);
            this.cts_Mute.TabIndex = 140;
            this.cts_Mute.Text = "Mute Audio Only";
            this.cts_Mute.UseVisualStyleBackColor = true;
            this.cts_Mute.CheckedChanged += new System.EventHandler(this.Cts_Mute_CheckedChanged);
            // 
            // SplitColors
            // 
            this.SplitColors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SplitColors.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SplitColors.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SplitColors.Location = new System.Drawing.Point(17, 97);
            this.SplitColors.Margin = new System.Windows.Forms.Padding(0);
            this.SplitColors.Name = "SplitColors";
            this.SplitColors.Size = new System.Drawing.Size(119, 24);
            this.SplitColors.TabIndex = 153;
            // 
            // splitDiv
            // 
            this.splitDiv.BackColor = System.Drawing.Color.DimGray;
            this.splitDiv.BorderColor = System.Drawing.Color.Blue;
            this.splitDiv.CheckColor = System.Drawing.Color.Blue;
            this.splitDiv.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.splitDiv.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.splitDiv.ForeColor = System.Drawing.Color.White;
            this.splitDiv.Location = new System.Drawing.Point(16, 31);
            this.splitDiv.Margin = new System.Windows.Forms.Padding(0);
            this.splitDiv.Name = "splitDiv";
            this.splitDiv.SelectionColor = System.Drawing.Color.LightBlue;
            this.splitDiv.Size = new System.Drawing.Size(118, 17);
            this.splitDiv.TabIndex = 149;
            this.splitDiv.Text = "Splitscreen Division";
            this.splitDiv.UseVisualStyleBackColor = true;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.BackColor = System.Drawing.Color.Transparent;
            this.label49.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label49.Location = new System.Drawing.Point(14, 78);
            this.label49.Margin = new System.Windows.Forms.Padding(0);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(105, 15);
            this.label49.TabIndex = 148;
            this.label49.Text = "Background Color";
            this.label49.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // layoutSizer
            // 
            this.layoutSizer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutSizer.BackColor = System.Drawing.Color.Transparent;
            this.layoutSizer.Location = new System.Drawing.Point(416, 33);
            this.layoutSizer.Margin = new System.Windows.Forms.Padding(0);
            this.layoutSizer.MaximumSize = new System.Drawing.Size(245, 170);
            this.layoutSizer.MinimumSize = new System.Drawing.Size(245, 170);
            this.layoutSizer.Name = "layoutSizer";
            this.layoutSizer.Size = new System.Drawing.Size(245, 170);
            this.layoutSizer.TabIndex = 147;
            this.layoutSizer.Paint += new System.Windows.Forms.PaintEventHandler(this.LayoutSizer_Paint);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Gray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(674, 418);
            this.ControlBox = false;
            this.Controls.Add(this.closeBtnPicture);
            this.Controls.Add(this.layoutBtnPicture);
            this.Controls.Add(this.settingsBtnPicture);
            this.Controls.Add(this.playersBtnPicture);
            this.Controls.Add(this.audioBtnPicture);
            this.Controls.Add(this.layoutTabBtn);
            this.Controls.Add(this.audioTabBtn);
            this.Controls.Add(this.playersTabBtn);
            this.Controls.Add(this.settingsTabBtn);
            this.Controls.Add(this.playersTab);
            this.Controls.Add(this.audioTab);
            this.Controls.Add(this.layoutTab);
            this.Controls.Add(this.settingsTab);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Settings";
            this.VisibleChanged += new System.EventHandler(this.Settings_VisibleChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Settings_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Settings_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.closeBtnPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutBtnPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingsBtnPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.playersBtnPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioBtnPicture)).EndInit();
            this.settingsTab.ResumeLayout(false);
            this.settingsTab.PerformLayout();
            this.hotkeyBox.ResumeLayout(false);
            this.hotkeyBox.PerformLayout();
            this.playersTab.ResumeLayout(false);
            this.page1.ResumeLayout(false);
            this.page1.PerformLayout();
            this.page2.ResumeLayout(false);
            this.page2.PerformLayout();
            this.audioTab.ResumeLayout(false);
            this.audioTab.PerformLayout();
            this.audioCustomSettingsBox.ResumeLayout(false);
            this.audioCustomSettingsBox.PerformLayout();
            this.layoutTab.ResumeLayout(false);
            this.layoutTab.PerformLayout();
            this.cts_Panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Button settingsTabBtn;
        private Button playersTabBtn;
        private Button audioTabBtn;
        private Button layoutTabBtn;
        private Panel audioTab;
        private Label audioWarningLabel;
        private Label audioDefaultDevice;
        private GroupBox audioCustomSettingsBox;
        private FlatCombo AudioInstance8;
        private Label label45;
        private FlatCombo AudioInstance7;
        private Label label44;
        private FlatCombo AudioInstance6;
        private Label label43;
        private FlatCombo AudioInstance5;
        private Label label42;
        private FlatCombo AudioInstance4;
        private Label label41;
        private FlatCombo AudioInstance3;
        private Label label40;
        private FlatCombo AudioInstance2;
        private FlatCombo AudioInstance1;
        private Label label37;
        private Label label36;
        private CustomRadio audioCustomSettingsRadio;
        private CustomRadio audioDefaultSettingsRadio;
        private Button audioRefresh;
        private PictureBox audioBtnPicture;
        private PictureBox playersBtnPicture;
        private PictureBox settingsBtnPicture;
        private PictureBox layoutBtnPicture;
        private Panel settingsTab;
        private Label label32;
        private Panel playersTab;
        private PictureBox closeBtnPicture;
        private FlatCombo cmb_Network;
        private Panel layoutTab;
        private CustomCheckBox useNicksCheck;
        private CustomCheckBox scaleOptionCbx;
        private Label cutsceneLabel;
        private Panel cts_Panel;
        private CustomCheckBox cts_unfocus;
        private CustomCheckBox cts_kar;
        private CustomCheckBox cts_Mute;
        private FlatCombo SplitColors;
        private CustomCheckBox splitDiv;
        private Label label49;
        public Panel layoutSizer;
        private CustomCheckBox keepAccountsCheck;
        private Button ctrlr_shorcutsBtn;
        private FlatTextBox nucUserPassTxt;
        private Label password_Label;
        private Label themeLabel;
        private FlatCombo themeCbx;
        private FlatCombo cmb_EpicLang;
        private Label label1;
        private CustomCheckBox ignoreInputLockReminderCheckbox;
        private FlatCombo cmb_Lang;
        private Label label2;
        private CustomCheckBox debugLogCheck;
        private GroupBox hotkeyBox;
        private FlatCombo swl_Cmb;
        private Label plus7;
        private Label hkLabel7;
        private FlatTextBox swl_HKTxt;
        private FlatCombo csm_Cmb;
        private Label plus6;
        private Label hkLabel6;
        private FlatTextBox csm_HKTxt;
        private FlatCombo rw_Cmb;
        private Label plus5;
        private FlatCombo tu_Cmb;
        private Label hkLabel5;
        private Label plus1;
        private FlatTextBox rw_HKTxt;
        private Label hotkeysLabel;
        private Label hkLabel1;
        private FlatTextBox tu_HKTxt;
        private FlatCombo lockKey_Cmb;
        private Label hkLabel8;
        private FlatCombo ttm_Cmb;
        private FlatCombo ss_Cmb;
        private FlatTextBox ttm_HKTxt;
        private FlatTextBox ss_HKTxt;
        private FlatCombo cn_Cmb;
        private Label plus4;
        private Label plus3;
        private Label plus2;
        private Label hkLabel4;
        private Label hkLabel3;
        private Label hkLabel;
        private FlatTextBox cn_HKTxt;
        private CustomCheckBox disableGameProfiles;
        private Button btnNext;
        private Panel page1;
        private Label label48;
        private Label label47;
        private Label label21;
        private Label label25;
        private Label label13;
        private Label label8;
        private FlatCombo steamid16;
        private FlatCombo steamid15;
        private FlatCombo steamid14;
        private FlatCombo steamid13;
        private FlatCombo steamid12;
        private FlatCombo steamid11;
        private FlatCombo steamid10;
        private FlatCombo steamid9;
        private FlatCombo steamid8;
        private FlatCombo steamid7;
        private FlatCombo steamid6;
        private FlatCombo steamid5;
        private FlatCombo steamid4;
        private FlatCombo steamid3;
        private FlatCombo steamid2;
        private FlatCombo steamid1;
        private FlatCombo player16N;
        private Label label17;
        private FlatCombo player15N;
        private Label label18;
        private FlatCombo player14N;
        private Label label19;
        private FlatCombo player13N;
        private Label label20;
        private FlatCombo player12N;
        private Label label22;
        private FlatCombo player11N;
        private Label label23;
        private FlatCombo player10N;
        private Label label24;
        private FlatCombo player9N;
        private Label label26;
        private FlatCombo player8N;
        private Label label16;
        private FlatCombo player7N;
        private Label label15;
        private FlatCombo player6N;
        private Label label11;
        private FlatCombo player5N;
        private Label label9;
        private FlatCombo player4N;
        private Label label14;
        private FlatCombo player3N;
        private Label label12;
        private FlatCombo player2N;
        private Label label10;
        private FlatCombo player1N;
        private Label label7;
        private Panel page2;
        private Label label3;
        private Label label62;
        private Label label63;
        private Label label64;
        private Label label65;
        private Label label66;
        private FlatCombo steamid32;
        private FlatCombo steamid17;
        private FlatCombo steamid31;
        private Label label61;
        private FlatCombo steamid30;
        private FlatCombo player17N;
        private FlatCombo steamid29;
        private Label label59;
        private FlatCombo steamid28;
        private FlatCombo player18N;
        private FlatCombo steamid27;
        private Label label58;
        private FlatCombo steamid26;
        private FlatCombo player19N;
        private FlatCombo steamid25;
        private Label label57;
        private FlatCombo steamid24;
        private FlatCombo player20N;
        private FlatCombo steamid23;
        private Label label55;
        private FlatCombo steamid22;
        private FlatCombo player21N;
        private FlatCombo steamid21;
        private Label label54;
        private FlatCombo steamid20;
        private FlatCombo player22N;
        private FlatCombo steamid19;
        private Label label53;
        private FlatCombo steamid18;
        private FlatCombo player23N;
        private Label label52;
        private FlatCombo player32N;
        private FlatCombo player24N;
        private Label label31;
        private Label label51;
        private FlatCombo player31N;
        private FlatCombo player25N;
        private Label label33;
        private Label label50;
        private FlatCombo player30N;
        private FlatCombo player26N;
        private Label label34;
        private Label label46;
        private FlatCombo player29N;
        private FlatCombo player27N;
        private Label label35;
        private Label label38;
        private FlatCombo player28N;
        private Gaming.Controls.CustomNumericUpDown numMaxPlyrs;
        private Gaming.Controls.CustomNumericUpDown numUpDownVer;
        private Label label29;
        private Label label27;
        private Label label28;
        private Gaming.Controls.CustomNumericUpDown numUpDownHor;
        private Button btn_Gb_Update;
        private CustomCheckBox gamepadsAssignMethods;
        private CustomCheckBox hideDesktop;
        private CustomCheckBox enable_WMerger;
        private FlatCombo rm_Cmb;
        private Label plus8;
        private Label hkLabel2;
        private FlatTextBox rm_HKTxt;
        private Panel screen_panel;
        private FlatTextBox selectedRes;
        private CustomCheckBox losslessHook;
        private Label label4;
        private Label mergerSettingsLabel;
        private Label mergerResSelectorLabel;
        private Button refreshScreenDatasButton;
        private FlatCombo smfw_Cmb;
        private Label plus9;
        private Label mergerShortcutLabel;
        private FlatTextBox smfw_HKTxt;
        private Label DivLabel;
        private Label label5;
    }
}