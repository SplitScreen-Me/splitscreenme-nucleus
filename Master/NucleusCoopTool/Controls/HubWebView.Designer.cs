namespace Nucleus.Coop.Forms
{
    partial class HubWebView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HubWebView));
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.closeBtn = new System.Windows.Forms.Button();
            this.back = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.home = new System.Windows.Forms.Button();
            this.button_Panel = new System.Windows.Forms.Panel();
            this.modal = new DoubleBufferPanel();
            this.modalControlsContainer = new DoubleBufferPanel();
            this.modal_text = new System.Windows.Forms.Label();
            this.modal_yes = new System.Windows.Forms.Button();
            this.modal_no = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.button_Panel.SuspendLayout();
            this.modal.SuspendLayout();
            this.modalControlsContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 0);
            this.webView.MinimumSize = new System.Drawing.Size(250, 250);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(1166, 655);
            this.webView.TabIndex = 0;
            this.webView.ZoomFactor = 1D;
            this.webView.CoreWebView2InitializationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>(this.WebView_CoreWebView2InitializationCompleted);
            // 
            // closeBtn
            // 
            this.closeBtn.BackColor = System.Drawing.Color.Gray;
            this.closeBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("closeBtn.BackgroundImage")));
            this.closeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(2, 83);
            this.closeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(20, 20);
            this.closeBtn.TabIndex = 17;
            this.closeBtn.UseVisualStyleBackColor = false;
            this.closeBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // back
            // 
            this.back.BackColor = System.Drawing.Color.Gray;
            this.back.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.back.FlatAppearance.BorderSize = 0;
            this.back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.back.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.back.ForeColor = System.Drawing.Color.Black;
            this.back.Location = new System.Drawing.Point(2, 58);
            this.back.Name = "back";
            this.back.Size = new System.Drawing.Size(20, 20);
            this.back.TabIndex = 22;
            this.back.UseVisualStyleBackColor = false;
            this.back.Click += new System.EventHandler(this.Back_Click);
            // 
            // next
            // 
            this.next.BackColor = System.Drawing.Color.Gray;
            this.next.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.next.FlatAppearance.BorderSize = 0;
            this.next.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.next.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.next.ForeColor = System.Drawing.Color.Black;
            this.next.Location = new System.Drawing.Point(2, 32);
            this.next.Name = "next";
            this.next.Size = new System.Drawing.Size(20, 20);
            this.next.TabIndex = 23;
            this.next.UseVisualStyleBackColor = false;
            this.next.Click += new System.EventHandler(this.Next_Click);
            // 
            // home
            // 
            this.home.BackColor = System.Drawing.Color.Gray;
            this.home.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.home.FlatAppearance.BorderSize = 0;
            this.home.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.home.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.home.ForeColor = System.Drawing.Color.Black;
            this.home.Location = new System.Drawing.Point(2, 6);
            this.home.Name = "home";
            this.home.Size = new System.Drawing.Size(20, 20);
            this.home.TabIndex = 24;
            this.home.UseVisualStyleBackColor = false;
            this.home.Click += new System.EventHandler(this.Home_Click);
            // 
            // button_Panel
            // 
            this.button_Panel.BackColor = System.Drawing.Color.White;
            this.button_Panel.Controls.Add(this.closeBtn);
            this.button_Panel.Controls.Add(this.next);
            this.button_Panel.Controls.Add(this.back);
            this.button_Panel.Controls.Add(this.home);
            this.button_Panel.Location = new System.Drawing.Point(5, 44);
            this.button_Panel.Name = "button_Panel";
            this.button_Panel.Size = new System.Drawing.Size(24, 107);
            this.button_Panel.TabIndex = 25;
            // 
            // modal
            // 
            this.modal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.modal.Controls.Add(this.modalControlsContainer);
            this.modal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modal.Location = new System.Drawing.Point(0, 0);
            this.modal.Name = "modal";
            this.modal.Size = new System.Drawing.Size(1166, 655);
            this.modal.TabIndex = 21;
            this.modal.Visible = false;
            // 
            // modalControlsContainer
            // 
            this.modalControlsContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modalControlsContainer.Controls.Add(this.modal_text);
            this.modalControlsContainer.Controls.Add(this.modal_yes);
            this.modalControlsContainer.Controls.Add(this.modal_no);
            this.modalControlsContainer.Location = new System.Drawing.Point(35, 44);
            this.modalControlsContainer.Name = "modalControlsContainer";
            this.modalControlsContainer.Size = new System.Drawing.Size(1096, 565);
            this.modalControlsContainer.TabIndex = 24;
            // 
            // modal_text
            // 
            this.modal_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modal_text.AutoEllipsis = true;
            this.modal_text.BackColor = System.Drawing.Color.Transparent;
            this.modal_text.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.modal_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modal_text.ForeColor = System.Drawing.Color.White;
            this.modal_text.Location = new System.Drawing.Point(226, 92);
            this.modal_text.Name = "modal_text";
            this.modal_text.Size = new System.Drawing.Size(643, 238);
            this.modal_text.TabIndex = 23;
            this.modal_text.Text = "label1";
            this.modal_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // modal_yes
            // 
            this.modal_yes.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.modal_yes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.modal_yes.BackColor = System.Drawing.Color.Transparent;
            this.modal_yes.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.modal_yes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.modal_yes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modal_yes.ForeColor = System.Drawing.Color.White;
            this.modal_yes.Location = new System.Drawing.Point(252, 399);
            this.modal_yes.Name = "modal_yes";
            this.modal_yes.Size = new System.Drawing.Size(214, 66);
            this.modal_yes.TabIndex = 21;
            this.modal_yes.Text = "Yes";
            this.modal_yes.UseVisualStyleBackColor = false;
            // 
            // modal_no
            // 
            this.modal_no.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.modal_no.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.modal_no.BackColor = System.Drawing.Color.Transparent;
            this.modal_no.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.modal_no.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.modal_no.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modal_no.ForeColor = System.Drawing.Color.White;
            this.modal_no.Location = new System.Drawing.Point(632, 399);
            this.modal_no.Name = "modal_no";
            this.modal_no.Size = new System.Drawing.Size(214, 66);
            this.modal_no.TabIndex = 22;
            this.modal_no.Text = "No";
            this.modal_no.UseVisualStyleBackColor = false;
            // 
            // HubWebView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.button_Panel);
            this.Controls.Add(this.modal);
            this.Controls.Add(this.webView);
            this.DoubleBuffered = true;
            this.Name = "HubWebView";
            this.Size = new System.Drawing.Size(1166, 655);
            this.Resize += new System.EventHandler(this.HubWebView_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.button_Panel.ResumeLayout(false);
            this.modal.ResumeLayout(false);
            this.modalControlsContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Button closeBtn;
        private DoubleBufferPanel modal;
        private System.Windows.Forms.Button back;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Button home;
        private System.Windows.Forms.Panel button_Panel;
        private System.Windows.Forms.Label modal_text;
        private System.Windows.Forms.Button modal_no;
        private System.Windows.Forms.Button modal_yes;
        private DoubleBufferPanel modalControlsContainer;
    }
}