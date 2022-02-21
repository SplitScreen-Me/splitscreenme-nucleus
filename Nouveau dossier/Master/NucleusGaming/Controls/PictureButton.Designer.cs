namespace Nucleus.Gaming
{
    partial class PictureButton
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_Picture = new System.Windows.Forms.Button();
            this.button_Btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_Picture
            // 
            this.button_Picture.Location = new System.Drawing.Point(0, 0);
            this.button_Picture.Name = "button_Picture";
            this.button_Picture.Size = new System.Drawing.Size(90, 100);
            this.button_Picture.TabIndex = 0;
            this.button_Picture.UseVisualStyleBackColor = true;
            this.button_Picture.Click += new System.EventHandler(this.button2_Click);
            // 
            // button_Btn
            // 
            this.button_Btn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Btn.Location = new System.Drawing.Point(87, 0);
            this.button_Btn.Name = "button_Btn";
            this.button_Btn.Size = new System.Drawing.Size(290, 100);
            this.button_Btn.TabIndex = 1;
            this.button_Btn.Text = "button1";
            this.button_Btn.UseVisualStyleBackColor = true;
            this.button_Btn.Click += new System.EventHandler(this.button2_Click);
            // 
            // PictureButton
            // 
            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button_Btn);
            this.Controls.Add(this.button_Picture);
            this.Name = "PictureButton";
            this.Size = new System.Drawing.Size(377, 100);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Picture;
        private System.Windows.Forms.Button button_Btn;
    }
}
