
namespace Nucleus.Gaming.Coop.Generic
{
    partial class ProcessPicker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessPicker));
            this.refrshBtn = new System.Windows.Forms.Button();
            this.selBtn = new System.Windows.Forms.Button();
            this.ppDesc = new System.Windows.Forms.Label();
            this.pplistBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // refrshBtn
            // 
            this.refrshBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.refrshBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.refrshBtn.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.refrshBtn.ForeColor = System.Drawing.Color.White;
            this.refrshBtn.Location = new System.Drawing.Point(0, 189);
            this.refrshBtn.Margin = new System.Windows.Forms.Padding(0);
            this.refrshBtn.Name = "refrshBtn";
            this.refrshBtn.Size = new System.Drawing.Size(196, 20);
            this.refrshBtn.TabIndex = 0;
            this.refrshBtn.Text = "Refresh";
            this.refrshBtn.UseVisualStyleBackColor = false;
            // 
            // selBtn
            // 
            this.selBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.selBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.selBtn.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.selBtn.ForeColor = System.Drawing.Color.White;
            this.selBtn.Location = new System.Drawing.Point(0, 209);
            this.selBtn.Margin = new System.Windows.Forms.Padding(0);
            this.selBtn.Name = "selBtn";
            this.selBtn.Size = new System.Drawing.Size(196, 20);
            this.selBtn.TabIndex = 1;
            this.selBtn.Text = "Select";
            this.selBtn.UseVisualStyleBackColor = false;
            // 
            // ppDesc
            // 
            this.ppDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ppDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ppDesc.Dock = System.Windows.Forms.DockStyle.Top;
            this.ppDesc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ppDesc.ForeColor = System.Drawing.Color.White;
            this.ppDesc.Location = new System.Drawing.Point(0, 0);
            this.ppDesc.Margin = new System.Windows.Forms.Padding(0);
            this.ppDesc.Name = "ppDesc";
            this.ppDesc.Size = new System.Drawing.Size(196, 30);
            this.ppDesc.TabIndex = 2;
            this.ppDesc.Text = "Select a process for Nucleus to use for process manipulation. ";
            // 
            // pplistBox
            // 
            this.pplistBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pplistBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pplistBox.FormattingEnabled = true;
            this.pplistBox.ItemHeight = 18;
            this.pplistBox.Location = new System.Drawing.Point(0, 30);
            this.pplistBox.Name = "pplistBox";
            this.pplistBox.ScrollAlwaysVisible = true;
            this.pplistBox.Size = new System.Drawing.Size(196, 159);
            this.pplistBox.TabIndex = 3;
            // 
            // ProcessPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(196, 229);
            this.Controls.Add(this.pplistBox);
            this.Controls.Add(this.refrshBtn);
            this.Controls.Add(this.selBtn);
            this.Controls.Add(this.ppDesc);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProcessPicker";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process Picker";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label ppDesc;
        public System.Windows.Forms.ListBox pplistBox;
        public System.Windows.Forms.Button refrshBtn;
        public System.Windows.Forms.Button selBtn;
    }
}