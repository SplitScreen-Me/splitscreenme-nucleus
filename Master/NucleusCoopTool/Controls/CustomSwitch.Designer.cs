namespace Nucleus.Coop.Controls
{
    partial class CustomSwitch
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
            this.label = new System.Windows.Forms.Label();
            this.tick = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label.Location = new System.Drawing.Point(61, 1);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(44, 16);
            this.label.TabIndex = 0;
            this.label.Text = "label1";
            this.label.MouseEnter += new System.EventHandler(this.Label_MouseEnter);
            this.label.MouseHover += new System.EventHandler(this.Label_MouseHover);
            // 
            // tick
            // 
            this.tick.AutoSize = true;
            this.tick.Location = new System.Drawing.Point(0, 0);
            this.tick.Margin = new System.Windows.Forms.Padding(0);
            this.tick.Name = "tick";
            this.tick.Size = new System.Drawing.Size(58, 26);
            this.tick.TabIndex = 1;
            this.tick.Click += new System.EventHandler(this.Tick_Click);
            this.tick.Paint += new System.Windows.Forms.PaintEventHandler(this.Tick_Paint);
            this.tick.MouseEnter += new System.EventHandler(this.Tick_MouseEnter);
            // 
            // CustomRadioButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.tick);
            this.Controls.Add(this.label);
            this.Name = "CustomRadioButton";
            this.Size = new System.Drawing.Size(108, 26);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CustomRadioButton_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Panel tick;
    }
}
