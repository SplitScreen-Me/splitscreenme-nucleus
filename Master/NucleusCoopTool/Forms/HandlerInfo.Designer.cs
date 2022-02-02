using Nucleus.Gaming;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{
    partial class HandlerInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HandlerInfo));
            this.linkLabel_MoreInfo = new System.Windows.Forms.LinkLabel();
            this.txt_Updated = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txt_Created = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txt_GameDesc = new System.Windows.Forms.RichTextBox();
            this.txt_AuthDesc = new System.Windows.Forms.RichTextBox();
            this.txt_Comm = new System.Windows.Forms.RichTextBox();
            this.txt_Verified = new System.Windows.Forms.TextBox();
            this.txt_Likes = new System.Windows.Forms.TextBox();
            this.txt_Down = new System.Windows.Forms.TextBox();
            this.txt_Version = new System.Windows.Forms.TextBox();
            this.txt_GameName = new System.Windows.Forms.TextBox();
            this.btn_Close = new System.Windows.Forms.Button();
            this.btn_Download = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pic_GameCover = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pic_GameCover)).BeginInit();
            this.SuspendLayout();
            // 
            // linkLabel_MoreInfo
            // 
            this.linkLabel_MoreInfo.ActiveLinkColor = System.Drawing.Color.LawnGreen;
            this.linkLabel_MoreInfo.AutoSize = true;
            this.linkLabel_MoreInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabel_MoreInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel_MoreInfo.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.linkLabel_MoreInfo.Location = new System.Drawing.Point(504, 25);
            this.linkLabel_MoreInfo.Name = "linkLabel_MoreInfo";
            this.linkLabel_MoreInfo.Size = new System.Drawing.Size(58, 13);
            this.linkLabel_MoreInfo.TabIndex = 52;
            this.linkLabel_MoreInfo.TabStop = true;
            this.linkLabel_MoreInfo.Text = "More Info";
            this.linkLabel_MoreInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel_MoreInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_MoreInfo_LinkClicked);
            // 
            // txt_Updated
            // 
            this.txt_Updated.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Updated.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txt_Updated.Location = new System.Drawing.Point(380, 185);
            this.txt_Updated.Name = "txt_Updated";
            this.txt_Updated.ReadOnly = true;
            this.txt_Updated.Size = new System.Drawing.Size(166, 20);
            this.txt_Updated.TabIndex = 50;
            this.txt_Updated.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(275, 186);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(99, 18);
            this.label10.TabIndex = 49;
            this.label10.Text = "Last Updated:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_Created
            // 
            this.txt_Created.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Created.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txt_Created.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_Created.Location = new System.Drawing.Point(100, 185);
            this.txt_Created.Name = "txt_Created";
            this.txt_Created.ReadOnly = true;
            this.txt_Created.Size = new System.Drawing.Size(166, 20);
            this.txt_Created.TabIndex = 48;
            this.txt_Created.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(19, 186);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 18);
            this.label9.TabIndex = 47;
            this.label9.Text = "Uploaded:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_GameDesc
            // 
            this.txt_GameDesc.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.txt_GameDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_GameDesc.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txt_GameDesc.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_GameDesc.Location = new System.Drawing.Point(127, 62);
            this.txt_GameDesc.Name = "txt_GameDesc";
            this.txt_GameDesc.ReadOnly = true;
            this.txt_GameDesc.Size = new System.Drawing.Size(426, 78);
            this.txt_GameDesc.TabIndex = 46;
            this.txt_GameDesc.Text = "";
            // 
            // txt_AuthDesc
            // 
            this.txt_AuthDesc.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.txt_AuthDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_AuthDesc.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txt_AuthDesc.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_AuthDesc.Location = new System.Drawing.Point(19, 243);
            this.txt_AuthDesc.Name = "txt_AuthDesc";
            this.txt_AuthDesc.ReadOnly = true;
            this.txt_AuthDesc.Size = new System.Drawing.Size(533, 93);
            this.txt_AuthDesc.TabIndex = 45;
            this.txt_AuthDesc.Text = "";
            this.txt_AuthDesc.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.txt_AuthDesc_LinkClicked);
            // 
            // txt_Comm
            // 
            this.txt_Comm.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.txt_Comm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_Comm.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txt_Comm.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_Comm.Location = new System.Drawing.Point(19, 371);
            this.txt_Comm.Name = "txt_Comm";
            this.txt_Comm.ReadOnly = true;
            this.txt_Comm.Size = new System.Drawing.Size(533, 145);
            this.txt_Comm.TabIndex = 44;
            this.txt_Comm.Text = "";
            this.txt_Comm.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.txt_Comm_LinkClicked);
            // 
            // txt_Verified
            // 
            this.txt_Verified.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Verified.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txt_Verified.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_Verified.Location = new System.Drawing.Point(495, 154);
            this.txt_Verified.Name = "txt_Verified";
            this.txt_Verified.ReadOnly = true;
            this.txt_Verified.Size = new System.Drawing.Size(60, 20);
            this.txt_Verified.TabIndex = 43;
            this.txt_Verified.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txt_Verified.Visible = false;
            // 
            // txt_Likes
            // 
            this.txt_Likes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Likes.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txt_Likes.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_Likes.Location = new System.Drawing.Point(366, 154);
            this.txt_Likes.Name = "txt_Likes";
            this.txt_Likes.ReadOnly = true;
            this.txt_Likes.Size = new System.Drawing.Size(60, 20);
            this.txt_Likes.TabIndex = 42;
            this.txt_Likes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txt_Down
            // 
            this.txt_Down.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Down.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txt_Down.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_Down.Location = new System.Drawing.Point(239, 153);
            this.txt_Down.Name = "txt_Down";
            this.txt_Down.ReadOnly = true;
            this.txt_Down.Size = new System.Drawing.Size(60, 20);
            this.txt_Down.TabIndex = 41;
            this.txt_Down.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txt_Version
            // 
            this.txt_Version.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Version.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txt_Version.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_Version.Location = new System.Drawing.Point(83, 153);
            this.txt_Version.Name = "txt_Version";
            this.txt_Version.ReadOnly = true;
            this.txt_Version.Size = new System.Drawing.Size(60, 20);
            this.txt_Version.TabIndex = 40;
            this.txt_Version.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txt_GameName
            // 
            this.txt_GameName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_GameName.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_GameName.ForeColor = System.Drawing.SystemColors.Window;
            this.txt_GameName.Location = new System.Drawing.Point(127, 12);
            this.txt_GameName.Name = "txt_GameName";
            this.txt_GameName.ReadOnly = true;
            this.txt_GameName.Size = new System.Drawing.Size(362, 26);
            this.txt_GameName.TabIndex = 39;
            // 
            // btn_Close
            // 
            this.btn_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Close.BackColor = System.Drawing.Color.Transparent;
            this.btn_Close.BackgroundImage = global::Nucleus.Coop.Properties.Resources.close_button_alpha;
            this.btn_Close.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Close.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Close.FlatAppearance.BorderSize = 0;
            this.btn_Close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Close.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Close.Location = new System.Drawing.Point(542, 3);
            this.btn_Close.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(20, 20);
            this.btn_Close.TabIndex = 38;
            this.btn_Close.UseVisualStyleBackColor = false;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // btn_Download
            // 
            this.btn_Download.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Download.BackColor = System.Drawing.Color.Transparent;
            this.btn_Download.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Download.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Download.FlatAppearance.BorderSize = 0;
            this.btn_Download.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Download.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Download.Location = new System.Drawing.Point(19, 989);
            this.btn_Download.Name = "btn_Download";
            this.btn_Download.Size = new System.Drawing.Size(120, 35);
            this.btn_Download.TabIndex = 37;
            this.btn_Download.Text = "Download";
            this.btn_Download.UseVisualStyleBackColor = false;
            this.btn_Download.Visible = false;
            this.btn_Download.Click += new System.EventHandler(this.btn_Download_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(15, 348);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 18);
            this.label8.TabIndex = 34;
            this.label8.Text = "Comments:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(15, 154);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 18);
            this.label7.TabIndex = 33;
            this.label7.Text = "Version:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(313, 154);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 18);
            this.label6.TabIndex = 32;
            this.label6.Text = "Likes:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(146, 154);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 18);
            this.label5.TabIndex = 31;
            this.label5.Text = "Downloads:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(429, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 18);
            this.label3.TabIndex = 30;
            this.label3.Text = "Verified:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(15, 220);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(195, 18);
            this.label2.TabIndex = 28;
            this.label2.Text = "Uploader Description/Notes:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label1.Location = new System.Drawing.Point(123, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 20);
            this.label1.TabIndex = 27;
            this.label1.Text = "Description:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pic_GameCover
            // 
            this.pic_GameCover.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pic_GameCover.Location = new System.Drawing.Point(19, 12);
            this.pic_GameCover.Name = "pic_GameCover";
            this.pic_GameCover.Size = new System.Drawing.Size(90, 128);
            this.pic_GameCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_GameCover.TabIndex = 0;
            this.pic_GameCover.TabStop = false;
            // 
            // HandlerInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(565, 528);
            this.ControlBox = false;
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.linkLabel_MoreInfo);
            this.Controls.Add(this.txt_Updated);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txt_Created);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txt_GameDesc);
            this.Controls.Add(this.txt_AuthDesc);
            this.Controls.Add(this.txt_Comm);
            this.Controls.Add(this.txt_Verified);
            this.Controls.Add(this.txt_Likes);
            this.Controls.Add(this.txt_Down);
            this.Controls.Add(this.txt_Version);
            this.Controls.Add(this.txt_GameName);
            this.Controls.Add(this.btn_Download);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pic_GameCover);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HandlerInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Handler Details";
            ((System.ComponentModel.ISupportInitialize)(this.pic_GameCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pic_GameCover;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.Button btn_Download;
        private System.Windows.Forms.TextBox txt_GameName;
        private System.Windows.Forms.TextBox txt_Version;
        private System.Windows.Forms.TextBox txt_Down;
        private System.Windows.Forms.TextBox txt_Likes;
        private System.Windows.Forms.TextBox txt_Verified;
        private System.Windows.Forms.RichTextBox txt_Comm;
        private System.Windows.Forms.RichTextBox txt_AuthDesc;
        private System.Windows.Forms.RichTextBox txt_GameDesc;
        private System.Windows.Forms.TextBox txt_Created;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txt_Updated;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.LinkLabel linkLabel_MoreInfo;
    }
}