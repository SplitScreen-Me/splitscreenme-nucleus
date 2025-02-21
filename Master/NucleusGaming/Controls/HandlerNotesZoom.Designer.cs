namespace Nucleus.Gaming.Controls
{
    partial class HandlerNotesZoom
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.close_Btn = new System.Windows.Forms.Button();
            this.Warning = new System.Windows.Forms.Label();
            this.TextBox = new Nucleus.Gaming.Controls.TransparentRichTextBox();
            this.SuspendLayout();
            // 
            // close_Btn
            // 
            this.close_Btn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.close_Btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.close_Btn.FlatAppearance.BorderSize = 0;
            this.close_Btn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.close_Btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.close_Btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.close_Btn.Location = new System.Drawing.Point(382, 3);
            this.close_Btn.Name = "close_Btn";
            this.close_Btn.Size = new System.Drawing.Size(20, 20);
            this.close_Btn.TabIndex = 1;
            this.close_Btn.UseVisualStyleBackColor = true;
            this.close_Btn.Click += new System.EventHandler(this.Close_Btn_Click);
            this.close_Btn.MouseEnter += new System.EventHandler(this.Close_Btn_MouseEnter);
            this.close_Btn.MouseLeave += new System.EventHandler(this.Close_Btn_MouseLeave);
            // 
            // Warning
            // 
            this.Warning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Warning.BackColor = System.Drawing.Color.Transparent;
            this.Warning.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Warning.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.5F, System.Drawing.FontStyle.Bold);
            this.Warning.ForeColor = System.Drawing.Color.Red;
            this.Warning.Location = new System.Drawing.Point(0, 3);
            this.Warning.Name = "Warning";
            this.Warning.Size = new System.Drawing.Size(768, 20);
            this.Warning.TabIndex = 2;
            this.Warning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBox
            // 
            this.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox.BackColor = System.Drawing.Color.CadetBlue;
            this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBox.ForeColor = System.Drawing.Color.White;
            this.TextBox.Location = new System.Drawing.Point(0, 22);
            this.TextBox.Margin = new System.Windows.Forms.Padding(0);
            this.TextBox.Name = "TextBox";
            this.TextBox.ReadOnly = true;
            this.TextBox.Size = new System.Drawing.Size(768, 449);
            this.TextBox.TabIndex = 0;
            this.TextBox.Text = "";
            this.TextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.TextBox_LinkClicked);
            // 
            // HandlerNotesZoom
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.Warning);
            this.Controls.Add(this.close_Btn);
            this.Controls.Add(this.TextBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Size = new System.Drawing.Size(768, 471);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.HandlerNotesZoom_Paint);
            this.ResumeLayout(false);

        }

        #endregion
        public Gaming.Controls.TransparentRichTextBox TextBox;
        private System.Windows.Forms.Button close_Btn;
        public System.Windows.Forms.Label Warning;
    }
}
