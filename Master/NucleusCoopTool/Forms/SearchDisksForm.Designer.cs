using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Nucleus.Gaming.Windows.Interop;
using WindowScrape.Constants;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Text;
using System.Runtime.ExceptionServices;
using System.Reflection;
using Nucleus.Coop.Forms;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming;
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
            this.checkBoxSizer2 = new System.Windows.Forms.Panel();
            this.checkboxFoundGames = new System.Windows.Forms.CheckedListBox();
            this.checkBoxSizer1 = new System.Windows.Forms.Panel();
            this.disksBox = new System.Windows.Forms.CheckedListBox();
            this.txt_Path = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.txt_Stage = new System.Windows.Forms.TextBox();
            this.btn_deselectAll = new System.Windows.Forms.Button();
            this.btn_selectAll = new System.Windows.Forms.Button();
            this.btn_delPath = new System.Windows.Forms.Button();
            this.btn_addSelection = new System.Windows.Forms.Button();
            this.btn_customPath = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.checkBoxSizer2.SuspendLayout();
            this.checkBoxSizer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.closeBtn);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(479, 30);
            this.panel1.TabIndex = 19;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Enabled = false;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(247, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Found Games";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Paths to Search";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // closeBtn
            // 
            this.closeBtn.BackColor = System.Drawing.Color.Black;
            this.closeBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("closeBtn.BackgroundImage")));
            this.closeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.closeBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(456, 3);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(20, 20);
            this.closeBtn.TabIndex = 16;
            this.closeBtn.UseVisualStyleBackColor = false;
            this.closeBtn.Click += new System.EventHandler(this.closeButton);
            // 
            // checkBoxSizer2
            // 
            this.checkBoxSizer2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSizer2.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxSizer2.Controls.Add(this.checkboxFoundGames);
            this.checkBoxSizer2.Location = new System.Drawing.Point(250, 34);
            this.checkBoxSizer2.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxSizer2.Name = "checkBoxSizer2";
            this.checkBoxSizer2.Size = new System.Drawing.Size(220, 234);
            this.checkBoxSizer2.TabIndex = 18;
            // 
            // checkboxFoundGames
            // 
            this.checkboxFoundGames.BackColor = System.Drawing.Color.White;
            this.checkboxFoundGames.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkboxFoundGames.CheckOnClick = true;
            this.checkboxFoundGames.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkboxFoundGames.Enabled = false;
            this.checkboxFoundGames.FormattingEnabled = true;
            this.checkboxFoundGames.HorizontalScrollbar = true;
            this.checkboxFoundGames.IntegralHeight = false;
            this.checkboxFoundGames.Location = new System.Drawing.Point(0, 0);
            this.checkboxFoundGames.Margin = new System.Windows.Forms.Padding(0);
            this.checkboxFoundGames.Name = "checkboxFoundGames";
            this.checkboxFoundGames.Size = new System.Drawing.Size(221, 236);
            this.checkboxFoundGames.TabIndex = 10;
            // 
            // checkBoxSizer1
            // 
            this.checkBoxSizer1.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxSizer1.Controls.Add(this.disksBox);
            this.checkBoxSizer1.Location = new System.Drawing.Point(9, 33);
            this.checkBoxSizer1.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxSizer1.Name = "checkBoxSizer1";
            this.checkBoxSizer1.Size = new System.Drawing.Size(220, 234);
            this.checkBoxSizer1.TabIndex = 17;
            // 
            // disksBox
            // 
            this.disksBox.BackColor = System.Drawing.Color.White;
            this.disksBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.disksBox.CheckOnClick = true;
            this.disksBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.disksBox.FormattingEnabled = true;
            this.disksBox.HorizontalScrollbar = true;
            this.disksBox.IntegralHeight = false;
            this.disksBox.Location = new System.Drawing.Point(0, 1);
            this.disksBox.Margin = new System.Windows.Forms.Padding(0);
            this.disksBox.Name = "disksBox";
            this.disksBox.Size = new System.Drawing.Size(220, 236);
            this.disksBox.TabIndex = 0;
            this.disksBox.SelectedIndexChanged += new System.EventHandler(this.disksBox_SelectedIndexChanged);
            // 
            // txt_Path
            // 
            this.txt_Path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Path.BackColor = System.Drawing.Color.Black;
            this.txt_Path.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_Path.Enabled = false;
            this.txt_Path.Location = new System.Drawing.Point(9, 370);
            this.txt_Path.Name = "txt_Path";
            this.txt_Path.Size = new System.Drawing.Size(462, 20);
            this.txt_Path.TabIndex = 16;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.BackColor = System.Drawing.Color.White;
            this.progressBar1.ForeColor = System.Drawing.Color.Black;
            this.progressBar1.Location = new System.Drawing.Point(120, 343);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(350, 21);
            this.progressBar1.TabIndex = 6;
            // 
            // txt_Stage
            // 
            this.txt_Stage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txt_Stage.BackColor = System.Drawing.Color.White;
            this.txt_Stage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Stage.Enabled = false;
            this.txt_Stage.Font = new System.Drawing.Font("Franklin Gothic Medium", 12.5F);
            this.txt_Stage.ForeColor = System.Drawing.Color.Black;
            this.txt_Stage.Location = new System.Drawing.Point(9, 344);
            this.txt_Stage.Margin = new System.Windows.Forms.Padding(0);
            this.txt_Stage.Name = "txt_Stage";
            this.txt_Stage.Size = new System.Drawing.Size(105, 19);
            this.txt_Stage.TabIndex = 15;
            this.txt_Stage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txt_Stage.TextChanged += new System.EventHandler(this.txt_Stage_TextChanged);
            // 
            // btn_deselectAll
            // 
            this.btn_deselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_deselectAll.BackColor = System.Drawing.Color.Transparent;
            this.btn_deselectAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_deselectAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_deselectAll.Enabled = false;
            this.btn_deselectAll.FlatAppearance.BorderSize = 0;
            this.btn_deselectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_deselectAll.Location = new System.Drawing.Point(250, 306);
            this.btn_deselectAll.Name = "btn_deselectAll";
            this.btn_deselectAll.Size = new System.Drawing.Size(220, 30);
            this.btn_deselectAll.TabIndex = 14;
            this.btn_deselectAll.Text = "Deselect All";
            this.btn_deselectAll.UseVisualStyleBackColor = false;
            this.btn_deselectAll.Click += new System.EventHandler(this.Btn_deselectAll_Click);
            // 
            // btn_selectAll
            // 
            this.btn_selectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_selectAll.BackColor = System.Drawing.Color.Transparent;
            this.btn_selectAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_selectAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_selectAll.Enabled = false;
            this.btn_selectAll.FlatAppearance.BorderSize = 0;
            this.btn_selectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_selectAll.Location = new System.Drawing.Point(362, 271);
            this.btn_selectAll.Name = "btn_selectAll";
            this.btn_selectAll.Size = new System.Drawing.Size(108, 30);
            this.btn_selectAll.TabIndex = 13;
            this.btn_selectAll.Text = "Select All";
            this.btn_selectAll.UseVisualStyleBackColor = false;
            this.btn_selectAll.Click += new System.EventHandler(this.Btn_selectAll_Click);
            // 
            // btn_delPath
            // 
            this.btn_delPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_delPath.BackColor = System.Drawing.Color.Transparent;
            this.btn_delPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_delPath.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_delPath.FlatAppearance.BorderSize = 0;
            this.btn_delPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delPath.Location = new System.Drawing.Point(9, 306);
            this.btn_delPath.Name = "btn_delPath";
            this.btn_delPath.Size = new System.Drawing.Size(220, 30);
            this.btn_delPath.TabIndex = 12;
            this.btn_delPath.Text = "Delete Selected";
            this.btn_delPath.UseVisualStyleBackColor = false;
            this.btn_delPath.Click += new System.EventHandler(this.Btn_delPath_Click);
            // 
            // btn_addSelection
            // 
            this.btn_addSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_addSelection.BackColor = System.Drawing.Color.Transparent;
            this.btn_addSelection.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_addSelection.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_addSelection.Enabled = false;
            this.btn_addSelection.FlatAppearance.BorderSize = 0;
            this.btn_addSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_addSelection.Location = new System.Drawing.Point(250, 271);
            this.btn_addSelection.Name = "btn_addSelection";
            this.btn_addSelection.Size = new System.Drawing.Size(108, 30);
            this.btn_addSelection.TabIndex = 11;
            this.btn_addSelection.Text = "Add Selected";
            this.btn_addSelection.UseVisualStyleBackColor = false;
            this.btn_addSelection.Click += new System.EventHandler(this.Btn_addSelection_Click);
            // 
            // btn_customPath
            // 
            this.btn_customPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_customPath.BackColor = System.Drawing.Color.Transparent;
            this.btn_customPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_customPath.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_customPath.FlatAppearance.BorderSize = 0;
            this.btn_customPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_customPath.Location = new System.Drawing.Point(9, 271);
            this.btn_customPath.Name = "btn_customPath";
            this.btn_customPath.Size = new System.Drawing.Size(108, 30);
            this.btn_customPath.TabIndex = 9;
            this.btn_customPath.Text = "Add a Path";
            this.btn_customPath.UseVisualStyleBackColor = false;
            this.btn_customPath.Click += new System.EventHandler(this.Btn_customPath_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSearch.BackColor = System.Drawing.Color.Transparent;
            this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Location = new System.Drawing.Point(121, 271);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(108, 30);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // SearchDisksForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(479, 399);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.checkBoxSizer2);
            this.Controls.Add(this.checkBoxSizer1);
            this.Controls.Add(this.txt_Path);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.txt_Stage);
            this.Controls.Add(this.btn_deselectAll);
            this.Controls.Add(this.btn_selectAll);
            this.Controls.Add(this.btn_delPath);
            this.Controls.Add(this.btn_addSelection);
            this.Controls.Add(this.btn_customPath);
            this.Controls.Add(this.btnSearch);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SearchDisksForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Search for Games";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SearchDisksForm_FormClosing);
            this.Load += new System.EventHandler(this.SearchDisksForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.checkBoxSizer2.ResumeLayout(false);
            this.checkBoxSizer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btn_customPath;
        private System.Windows.Forms.Button btn_addSelection;
        private System.Windows.Forms.Button btn_delPath;
        private System.Windows.Forms.Button btn_selectAll;
        private System.Windows.Forms.Button btn_deselectAll;
        private System.Windows.Forms.TextBox txt_Stage;
        private System.Windows.Forms.TextBox txt_Path;
        private CheckedListBox checkboxFoundGames;
        private CheckedListBox disksBox;
        private Panel checkBoxSizer1;
        private Panel checkBoxSizer2;
        private Panel panel1;
    }
}