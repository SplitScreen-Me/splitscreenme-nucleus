using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Nucleus.Gaming;
namespace Nucleus.Coop.Forms
{
    partial class DependenciesDownloader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DependenciesDownloader));
            this.lbl_ProgPerc = new System.Windows.Forms.Label();
            this.lbl_Handler = new System.Windows.Forms.Label();
            this.prog_DownloadBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // lbl_ProgPerc
            // 
            this.lbl_ProgPerc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_ProgPerc.BackColor = System.Drawing.Color.Transparent;
            this.lbl_ProgPerc.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lbl_ProgPerc.ForeColor = System.Drawing.SystemColors.Window;
            this.lbl_ProgPerc.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_ProgPerc.Location = new System.Drawing.Point(266, 12);
            this.lbl_ProgPerc.Name = "lbl_ProgPerc";
            this.lbl_ProgPerc.Size = new System.Drawing.Size(78, 20);
            this.lbl_ProgPerc.TabIndex = 6;
            this.lbl_ProgPerc.Text = "%";
            this.lbl_ProgPerc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_Handler
            // 
            this.lbl_Handler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl_Handler.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Handler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbl_Handler.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lbl_Handler.ForeColor = System.Drawing.SystemColors.Window;
            this.lbl_Handler.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Handler.Location = new System.Drawing.Point(12, 12);
            this.lbl_Handler.Name = "lbl_Handler";
            this.lbl_Handler.Size = new System.Drawing.Size(218, 20);
            this.lbl_Handler.TabIndex = 5;
            // 
            // prog_DownloadBar
            // 
            this.prog_DownloadBar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.prog_DownloadBar.Location = new System.Drawing.Point(29, 47);
            this.prog_DownloadBar.MaximumSize = new System.Drawing.Size(289, 23);
            this.prog_DownloadBar.MinimumSize = new System.Drawing.Size(289, 23);
            this.prog_DownloadBar.Name = "prog_DownloadBar";
            this.prog_DownloadBar.Size = new System.Drawing.Size(289, 23);
            this.prog_DownloadBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prog_DownloadBar.TabIndex = 4;
            // 
            // DependenciesDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(356, 92);
            this.Controls.Add(this.lbl_ProgPerc);
            this.Controls.Add(this.lbl_Handler);
            this.Controls.Add(this.prog_DownloadBar);
            this.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DependenciesDownloader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private Label lbl_ProgPerc;
        private Label lbl_Handler;
        private ProgressBar prog_DownloadBar;
    }
}