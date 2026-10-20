using System.Windows.Forms;

namespace Nucleus.Coop
{
    partial class SearchDisksForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchDisksForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.checkboxFoundGames = new System.Windows.Forms.CheckedListBox();
            this.txt_Path = new System.Windows.Forms.TextBox();
            this.btn_deselectAll = new System.Windows.Forms.Button();
            this.btn_selectAll = new System.Windows.Forms.Button();
            this.btn_addSelection = new System.Windows.Forms.Button();
            this.btn_customPath = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.disksBox = new System.Windows.Forms.CheckedListBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.closeBtn);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(930, 30);
            this.panel1.TabIndex = 19;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Franklin Gothic Medium", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(467, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Found games";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Franklin Gothic Medium", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Paths to scan";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // closeBtn
            // 
            this.closeBtn.BackColor = System.Drawing.Color.Transparent;
            this.closeBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("closeBtn.BackgroundImage")));
            this.closeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(907, 3);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(20, 20);
            this.closeBtn.TabIndex = 16;
            this.closeBtn.UseVisualStyleBackColor = false;
            this.closeBtn.Click += new System.EventHandler(this.closeButton);
            this.closeBtn.MouseEnter += new System.EventHandler(this.closeBtn_MouseEnter);
            this.closeBtn.MouseLeave += new System.EventHandler(this.closeBtn_MouseLeave);
            // 
            // checkboxFoundGames
            // 
            this.checkboxFoundGames.BackColor = System.Drawing.Color.Black;
            this.checkboxFoundGames.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkboxFoundGames.CheckOnClick = true;
            this.checkboxFoundGames.Enabled = false;
            this.checkboxFoundGames.ForeColor = System.Drawing.Color.White;
            this.checkboxFoundGames.FormattingEnabled = true;
            this.checkboxFoundGames.HorizontalScrollbar = true;
            this.checkboxFoundGames.IntegralHeight = false;
            this.checkboxFoundGames.Location = new System.Drawing.Point(470, 30);
            this.checkboxFoundGames.Name = "checkboxFoundGames";
            this.checkboxFoundGames.Size = new System.Drawing.Size(450, 351);
            this.checkboxFoundGames.TabIndex = 10;
            // 
            // txt_Path
            // 
            this.txt_Path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Path.BackColor = System.Drawing.Color.Black;
            this.txt_Path.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_Path.Enabled = false;
            this.txt_Path.Font = new System.Drawing.Font("Franklin Gothic Medium", 6.25F);
            this.txt_Path.Location = new System.Drawing.Point(9, 440);
            this.txt_Path.Name = "txt_Path";
            this.txt_Path.Size = new System.Drawing.Size(929, 17);
            this.txt_Path.TabIndex = 16;
            this.txt_Path.Visible = false;
            // 
            // btn_deselectAll
            // 
            this.btn_deselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_deselectAll.BackColor = System.Drawing.Color.Black;
            this.btn_deselectAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_deselectAll.FlatAppearance.BorderSize = 0;
            this.btn_deselectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_deselectAll.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_deselectAll.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btn_deselectAll.Location = new System.Drawing.Point(592, 391);
            this.btn_deselectAll.Name = "btn_deselectAll";
            this.btn_deselectAll.Size = new System.Drawing.Size(116, 30);
            this.btn_deselectAll.TabIndex = 14;
            this.btn_deselectAll.Text = "Deselect All";
            this.btn_deselectAll.UseVisualStyleBackColor = false;
            this.btn_deselectAll.Click += new System.EventHandler(this.Btn_deselectAll_Click);
            // 
            // btn_selectAll
            // 
            this.btn_selectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_selectAll.BackColor = System.Drawing.Color.Black;
            this.btn_selectAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_selectAll.FlatAppearance.BorderSize = 0;
            this.btn_selectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_selectAll.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_selectAll.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btn_selectAll.Location = new System.Drawing.Point(470, 391);
            this.btn_selectAll.Name = "btn_selectAll";
            this.btn_selectAll.Size = new System.Drawing.Size(116, 30);
            this.btn_selectAll.TabIndex = 13;
            this.btn_selectAll.Text = "Select All";
            this.btn_selectAll.UseVisualStyleBackColor = false;
            this.btn_selectAll.Click += new System.EventHandler(this.Btn_selectAll_Click);
            // 
            // btn_addSelection
            // 
            this.btn_addSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_addSelection.BackColor = System.Drawing.Color.Black;
            this.btn_addSelection.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_addSelection.FlatAppearance.BorderSize = 0;
            this.btn_addSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_addSelection.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_addSelection.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btn_addSelection.Location = new System.Drawing.Point(714, 391);
            this.btn_addSelection.Name = "btn_addSelection";
            this.btn_addSelection.Size = new System.Drawing.Size(206, 30);
            this.btn_addSelection.TabIndex = 11;
            this.btn_addSelection.Text = "Add Selected Games";
            this.btn_addSelection.UseVisualStyleBackColor = false;
            this.btn_addSelection.Click += new System.EventHandler(this.Btn_addSelection_Click);
            // 
            // btn_customPath
            // 
            this.btn_customPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_customPath.BackColor = System.Drawing.Color.Black;
            this.btn_customPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_customPath.FlatAppearance.BorderSize = 0;
            this.btn_customPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_customPath.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_customPath.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btn_customPath.Location = new System.Drawing.Point(9, 391);
            this.btn_customPath.Name = "btn_customPath";
            this.btn_customPath.Size = new System.Drawing.Size(223, 30);
            this.btn_customPath.TabIndex = 9;
            this.btn_customPath.Text = "Manage Search Paths";
            this.btn_customPath.UseVisualStyleBackColor = false;
            this.btn_customPath.Click += new System.EventHandler(this.Btn_customPath_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSearch.BackColor = System.Drawing.Color.Black;
            this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSearch.Location = new System.Drawing.Point(236, 391);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(223, 30);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Start Games Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
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
            this.disksBox.Location = new System.Drawing.Point(9, 31);
            this.disksBox.Name = "disksBox";
            this.disksBox.Size = new System.Drawing.Size(450, 351);
            this.disksBox.Sorted = true;
            this.disksBox.TabIndex = 0;
            // 
            // SearchDisksForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(930, 430);
            this.Controls.Add(this.checkboxFoundGames);
            this.Controls.Add(this.disksBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txt_Path);
            this.Controls.Add(this.btn_deselectAll);
            this.Controls.Add(this.btn_selectAll);
            this.Controls.Add(this.btn_addSelection);
            this.Controls.Add(this.btn_customPath);
            this.Controls.Add(this.btnSearch);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SearchDisksForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Auto Search Games";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SearchDisksForm_Paint);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btn_customPath;
        private System.Windows.Forms.Button btn_addSelection;
        private System.Windows.Forms.Button btn_selectAll;
        private System.Windows.Forms.Button btn_deselectAll;
        private System.Windows.Forms.TextBox txt_Path;
        private CheckedListBox checkboxFoundGames;
        private Panel panel1;
        private CheckedListBox disksBox;
    }
}