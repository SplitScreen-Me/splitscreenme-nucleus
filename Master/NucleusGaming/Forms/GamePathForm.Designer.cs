using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    partial class GamePathForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.autoSearchChkBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_delPath = new System.Windows.Forms.Button();
            this.closeBtn = new System.Windows.Forms.Button();
            this.btn_customPath = new System.Windows.Forms.Button();
            this.disksBox = new System.Windows.Forms.CheckedListBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.autoSearchChkBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btn_delPath);
            this.panel1.Controls.Add(this.closeBtn);
            this.panel1.Controls.Add(this.btn_customPath);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(930, 30);
            this.panel1.TabIndex = 19;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            // 
            // autoSearchChkBox
            // 
            this.autoSearchChkBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.autoSearchChkBox.AutoSize = true;
            this.autoSearchChkBox.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoSearchChkBox.ForeColor = System.Drawing.Color.Orange;
            this.autoSearchChkBox.Location = new System.Drawing.Point(346, 6);
            this.autoSearchChkBox.Name = "autoSearchChkBox";
            this.autoSearchChkBox.Size = new System.Drawing.Size(252, 21);
            this.autoSearchChkBox.TabIndex = 17;
            this.autoSearchChkBox.Text = "Enable auto game executable scan";
            this.autoSearchChkBox.UseVisualStyleBackColor = true;
            this.autoSearchChkBox.CheckedChanged += new System.EventHandler(this.AutoSearchChkBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Franklin Gothic Medium", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Paths list";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_delPath
            // 
            this.btn_delPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_delPath.BackColor = System.Drawing.Color.Transparent;
            this.btn_delPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_delPath.FlatAppearance.BorderSize = 0;
            this.btn_delPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delPath.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_delPath.Location = new System.Drawing.Point(718, 4);
            this.btn_delPath.Name = "btn_delPath";
            this.btn_delPath.Size = new System.Drawing.Size(123, 23);
            this.btn_delPath.TabIndex = 12;
            this.btn_delPath.Text = "Delete Unselected";
            this.btn_delPath.UseVisualStyleBackColor = false;
            this.btn_delPath.Click += new System.EventHandler(this.Btn_delPath_Click);
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.BackColor = System.Drawing.Color.Transparent;
            this.closeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeBtn.Location = new System.Drawing.Point(847, 4);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(73, 23);
            this.closeBtn.TabIndex = 16;
            this.closeBtn.Text = "Back";
            this.closeBtn.UseVisualStyleBackColor = false;
            this.closeBtn.Click += new System.EventHandler(this.closeButton);
            this.closeBtn.MouseEnter += new System.EventHandler(this.closeBtn_MouseEnter);
            this.closeBtn.MouseLeave += new System.EventHandler(this.closeBtn_MouseLeave);
            // 
            // btn_customPath
            // 
            this.btn_customPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_customPath.BackColor = System.Drawing.Color.Transparent;
            this.btn_customPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_customPath.FlatAppearance.BorderSize = 0;
            this.btn_customPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_customPath.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_customPath.Location = new System.Drawing.Point(604, 4);
            this.btn_customPath.Name = "btn_customPath";
            this.btn_customPath.Size = new System.Drawing.Size(108, 23);
            this.btn_customPath.TabIndex = 9;
            this.btn_customPath.Text = "Add Path";
            this.btn_customPath.UseVisualStyleBackColor = false;
            this.btn_customPath.Click += new System.EventHandler(this.Btn_customPath_Click);
            // 
            // disksBox
            // 
            this.disksBox.BackColor = System.Drawing.Color.Black;
            this.disksBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.disksBox.CheckOnClick = true;
            this.disksBox.ForeColor = System.Drawing.Color.White;
            this.disksBox.FormattingEnabled = true;
            this.disksBox.HorizontalScrollbar = true;
            this.disksBox.IntegralHeight = false;
            this.disksBox.Location = new System.Drawing.Point(11, 31);
            this.disksBox.Name = "disksBox";
            this.disksBox.Size = new System.Drawing.Size(908, 382);
            this.disksBox.TabIndex = 0;
            // 
            // GamePathForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(930, 430);
            this.Controls.Add(this.disksBox);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "GamePathForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Add games search pathes";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GamePathForm_Paint);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_customPath;
        private System.Windows.Forms.Button btn_delPath;
        private CheckedListBox disksBox;
        private Panel panel1;
        private CheckBox autoSearchChkBox;
    }
}