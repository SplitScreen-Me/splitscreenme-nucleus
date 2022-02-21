namespace SplitTool
{
    partial class AutoFindGames
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
            this.list_FoldersToSearch = new System.Windows.Forms.ListBox();
            this.button_auto = new System.Windows.Forms.Button();
            this.button_Remove = new System.Windows.Forms.Button();
            this.btn_Update = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // list_FoldersToSearch
            // 
            this.list_FoldersToSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.list_FoldersToSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.list_FoldersToSearch.Font = new System.Drawing.Font("Segoe UI Symbol", 12F);
            this.list_FoldersToSearch.ForeColor = System.Drawing.Color.DimGray;
            this.list_FoldersToSearch.FormattingEnabled = true;
            this.list_FoldersToSearch.ItemHeight = 21;
            this.list_FoldersToSearch.Location = new System.Drawing.Point(12, 12);
            this.list_FoldersToSearch.Name = "list_FoldersToSearch";
            this.list_FoldersToSearch.Size = new System.Drawing.Size(567, 296);
            this.list_FoldersToSearch.TabIndex = 0;
            // 
            // button_auto
            // 
            this.button_auto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_auto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.button_auto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_auto.Font = new System.Drawing.Font("Segoe UI Symbol", 12F);
            this.button_auto.ForeColor = System.Drawing.Color.DimGray;
            this.button_auto.Location = new System.Drawing.Point(12, 314);
            this.button_auto.Name = "button_auto";
            this.button_auto.Size = new System.Drawing.Size(147, 31);
            this.button_auto.TabIndex = 5;
            this.button_auto.Text = "Add Folder";
            this.button_auto.UseVisualStyleBackColor = false;
            this.button_auto.Click += new System.EventHandler(this.button_auto_Click);
            // 
            // button_Remove
            // 
            this.button_Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_Remove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.button_Remove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Remove.Font = new System.Drawing.Font("Segoe UI Symbol", 12F);
            this.button_Remove.ForeColor = System.Drawing.Color.DimGray;
            this.button_Remove.Location = new System.Drawing.Point(165, 314);
            this.button_Remove.Name = "button_Remove";
            this.button_Remove.Size = new System.Drawing.Size(147, 31);
            this.button_Remove.TabIndex = 6;
            this.button_Remove.Text = "Remove";
            this.button_Remove.UseVisualStyleBackColor = false;
            this.button_Remove.Click += new System.EventHandler(this.button_Remove_Click);
            // 
            // btn_Update
            // 
            this.btn_Update.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Update.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.btn_Update.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Update.Font = new System.Drawing.Font("Segoe UI Symbol", 12F);
            this.btn_Update.ForeColor = System.Drawing.Color.DimGray;
            this.btn_Update.Location = new System.Drawing.Point(432, 312);
            this.btn_Update.Name = "btn_Update";
            this.btn_Update.Size = new System.Drawing.Size(147, 31);
            this.btn_Update.TabIndex = 7;
            this.btn_Update.Text = "Update";
            this.btn_Update.UseVisualStyleBackColor = false;
            this.btn_Update.Click += new System.EventHandler(this.btn_Update_Click);
            // 
            // AutoFindGames
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.ClientSize = new System.Drawing.Size(591, 355);
            this.Controls.Add(this.btn_Update);
            this.Controls.Add(this.button_Remove);
            this.Controls.Add(this.button_auto);
            this.Controls.Add(this.list_FoldersToSearch);
            this.Name = "AutoFindGames";
            this.Text = "AutoFindGames";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox list_FoldersToSearch;
        private System.Windows.Forms.Button button_auto;
        private System.Windows.Forms.Button button_Remove;
        private System.Windows.Forms.Button btn_Update;

    }
}