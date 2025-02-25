using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace Nucleus.Gaming.Controls
{

    public partial class CustomComboBox : UserControl, IDynamicSized
    {
        public FlatStyle FlatStyle;

        private List<string> items = new List<string>();
        [Browsable(false)]
        public List<string> Items
        {
            get
            {            
                return items;
            }

            set
            {
                items = value;
            }
        }

        private string itemsCollection;
        [Browsable(true)]
        public string ItemsCollection
        {
            get => itemsCollection;
            set
            {
                items.Clear();
                if (value != string.Empty)
                {
                    var all = value.Split(',');
                    foreach (var val in all)
                    {
                        items.Add(val);
                    }
                }

                itemsCollection = value;
            }
        }

        [Browsable(true)]
        public int MaxDropDownItems
        {
            get;
            set;
        }

        [Browsable(true)]
        public int MaxLength
        {
            get;
            set;
        }


        [Browsable(true)]
        public int SelectedIndex
        {
            get;
            set;
        }

        private ControlListBox dropDownList;

        private object selectedItem;
        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = (string)value;
                MainItem.Text = (string)value;
            }
        }

        private int Delta = 0;
        private int ItemIndex = 0;

        private int scrollOffset = 5;
        public int ScrollOffset
        {
            get => scrollOffset;
            set => scrollOffset = value;
        }

        private float _scale;
        private int defaultHeight;

        public TransparentRichTextBox MainItem;
        private Label Expand;
        private Font itemFont;
        private FontFamily fontFamily;

        public CustomComboBox()
        {
            InitializeComponent();

            BackColor = Color.FromArgb(100, 0, 0, 0);
            BorderStyle = BorderStyle.FixedSingle;

            fontFamily = Font.FontFamily;
            
            Expand = new Label()
            {
                Name = "Expand",
                Dock = DockStyle.Right,
                BackgroundImageLayout = ImageLayout.Zoom,
                BackgroundImage = new Bitmap(Properties.Resources.title_dropdown_closed),
                BorderStyle = BorderStyle.None,
                Font = this.Font,
                BackColor = Color.FromArgb(0, 0, 0, 0),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            Expand.Size = new Size(Height, Height);
            Expand.Click += new EventHandler(Expand_Click);
            Expand.Location = new Point(Width - Height,0);
            Controls.Add(Expand);

            MainItem = new TransparentRichTextBox()
            {
                Name = "Main",
                BorderStyle = BorderStyle.None,
                Font = this.Font,
                ForeColor = Color.White,
                BackColor =Color.DarkGoldenrod,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                MaxLength = MaxLength
            };

            MainItem.Multiline = false;

            MainItem.AutoSize = true;
            MainItem.Location = new Point(0, 0);
            MainItem.LostFocus += Main_LostFocus;
            LostFocus += _LostFocus;

            MainItem.Click += Main_Click;

            Controls.Add(MainItem);

            dropDownList = new ControlListBox();
            
            dropDownList.Visible = false;
            dropDownList.Width = Width;
            dropDownList.Location = new Point(0, Height);

            
            dropDownList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            
            dropDownList.BringToFront();
            dropDownList.Scroll += Scrolling;

            Controls.Add(dropDownList);

            MainItem.MouseWheel += new MouseEventHandler(Scrolling);

            if(MaxDropDownItems == 0)
            {
                MaxDropDownItems = 3;
            }

            if(items.Count > 0)
            {
                MainItem.Text = items[SelectedIndex];
            }

            BringToFront();
            DPIManager.Register(this);
        }

        private void Scrolling(object sender, ScrollEventArgs e)
        {
            Control V = sender as Control;
    
        }
        private void Main_Click(object sender, EventArgs e)
        {
            Expand_Click(false, null);
        }

        private void _LostFocus(object sender, EventArgs e)
        {
            Expand_Click(false, null);       
        }

        private void Main_LostFocus(object sender,EventArgs e)
        {
            TransparentRichTextBox txt = sender as TransparentRichTextBox;
            if(!Items.Contains(txt.Text) && txt.Text != "")
            {
                Items.Add(txt.Text);
                Expand_Click(true,null);
            }
        }

        public void Expand_Click(object sender, EventArgs e)//refresh the whole list
        {
            dropDownList.Visible =  dropDownList.Visible ? false : true;

            bool update = sender != null && sender is bool ? (bool)sender : false;

            if(dropDownList.Visible || update)
            {
                dropDownList.Visible = false;
                Expand.BackgroundImage = new Bitmap(Properties.Resources.title_dropdown_opened);
               

                foreach(Control item in dropDownList.Controls)
                {
                    item.Dispose();
                }

                dropDownList.Controls.Clear();
                dropDownList.Height = 0;

                for (int i = 0; i < Items.Count; i++)
                {
                    string text = (string)Items[i];

                    Label item = new Label
                    {
                        Name = (string)Items[i],
                        BorderStyle = BorderStyle.None,
                        Font = itemFont,
                        BackColor = Color.Transparent,
                        ForeColor = Color.White,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Text = text,
                        MinimumSize = new Size(dropDownList.Width, (int)(defaultHeight * _scale)),
                        //Size = new Size(dropDownList.Width, (int)(defaultHeight * _scale)),
                        AutoSize = true
                    };

                    item.Click += Item_Click;
                     
                    if (i <= MaxDropDownItems)
                    {
                        dropDownList.Size = new Size(Width, dropDownList.Height += item.ClientRectangle.Height);
                    }

                    dropDownList.Controls.Add(item);
                }

                dropDownList.Location = new Point(0, Height);
                Height = defaultHeight + dropDownList.Height;
                dropDownList.Visible = true;
                dropDownList.BringToFront();
            }
            else
            {
                Height = defaultHeight;
                Expand.BackgroundImage = new Bitmap(Properties.Resources.title_dropdown_closed);
            }

            if (items.Count > 0)
            {
                MainItem.Text = items[SelectedIndex];
            }

            BringToFront();
            Update();
        }


        private void ReadOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void Item_Click(object sender, EventArgs e)
        {
            Label item = sender as Label;
            MainItem.Text = item.Text;
            selectedItem = item.Text;
            Expand_Click(null, null);
        }

        private void Scrolling(object sender, MouseEventArgs e)
        {
            Delta = (int)e.Delta;

            if (Delta > 0)
            {
                if (ItemIndex >= 1)
                {
                    ItemIndex--;
                    MainItem.Text = (string)Items[ItemIndex];
                }
            }

            if (Delta < 0)
            {
                if (ItemIndex < Items.Count)
                {
                    MainItem.Text = (string)Items[ItemIndex];
                    ItemIndex++;
                }
            }

            selectedItem = MainItem.Text;
        }

        public void SelectedDefaultText(string text)
        {
            MainItem.Text = text;
        }

       
        private bool scaled = false;

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            if (!scaled)
            {
                
                float ratio = ((float)Height / (float)MainItem.Height);
                MainItem.Font = new Font(fontFamily, (Font.Size + ratio ) * scale  , FontStyle.Regular, GraphicsUnit.Pixel, 0);


                Height = MainItem.ClientRectangle.Bottom + 7;
                Expand.Size = new Size(Height,Height);
                Expand.Location = new Point(Width - Expand.Width, 0);
                MainItem.Width = Width - Expand.Width;
                MainItem.Location = new Point(0, 3);
                defaultHeight = Height;              
                dropDownList.Location = new Point(0, Height);
                itemFont = new Font(fontFamily, Font.Size + ratio, FontStyle.Regular, GraphicsUnit.Pixel, 0);

            }

            scaled = true;
            _scale = scale;
        }


        //protected override void OnPaint(PaintEventArgs e)///voir pour utiliser drawstring seulement si readonly
        //{

        //}

    }
}
