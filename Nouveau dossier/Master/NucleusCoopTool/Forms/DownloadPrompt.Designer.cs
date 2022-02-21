using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Nucleus.Gaming;
namespace Nucleus.Coop.Forms
{
    partial class DownloadPrompt
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
        
		private readonly IniFile ini = new Gaming.IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
		
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadPrompt));
            this.lbl_ProgPerc = new System.Windows.Forms.Label();
            this.lbl_Handler = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.prog_DownloadBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // lbl_ProgPerc
            // 
            this.lbl_ProgPerc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_ProgPerc.BackColor = System.Drawing.Color.Transparent;
            this.lbl_ProgPerc.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lbl_ProgPerc.ForeColor = System.Drawing.SystemColors.Window;
            this.lbl_ProgPerc.Location = new System.Drawing.Point(225, 38);
            this.lbl_ProgPerc.Name = "lbl_ProgPerc";
            this.lbl_ProgPerc.Size = new System.Drawing.Size(93, 19);
            this.lbl_ProgPerc.TabIndex = 3;
            this.lbl_ProgPerc.Text = "%";
            this.lbl_ProgPerc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_Handler
            // 
            this.lbl_Handler.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_Handler.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Handler.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_Handler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbl_Handler.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lbl_Handler.ForeColor = System.Drawing.SystemColors.Window;
            this.lbl_Handler.Location = new System.Drawing.Point(29, 57);
            this.lbl_Handler.Name = "lbl_Handler";
            this.lbl_Handler.Size = new System.Drawing.Size(289, 19);
            this.lbl_Handler.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label1.ForeColor = System.Drawing.SystemColors.Window;
            this.label1.Location = new System.Drawing.Point(25, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "Downloading:";
            // 
            // prog_DownloadBar
            // 
            this.prog_DownloadBar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.prog_DownloadBar.Location = new System.Drawing.Point(29, 80);
            this.prog_DownloadBar.MaximumSize = new System.Drawing.Size(289, 23);
            this.prog_DownloadBar.MinimumSize = new System.Drawing.Size(289, 23);
            this.prog_DownloadBar.Name = "prog_DownloadBar";
            this.prog_DownloadBar.Size = new System.Drawing.Size(289, 23);
            this.prog_DownloadBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prog_DownloadBar.TabIndex = 0;
            // 
            // DownloadPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(356, 148);
            this.Controls.Add(this.lbl_ProgPerc);
            this.Controls.Add(this.lbl_Handler);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.prog_DownloadBar);
            this.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DownloadPrompt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Downloading Game Handler";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar prog_DownloadBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_Handler;
        private System.Windows.Forms.Label lbl_ProgPerc;
    }
}