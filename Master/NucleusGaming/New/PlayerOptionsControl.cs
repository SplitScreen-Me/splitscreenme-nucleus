using Nucleus.Gaming;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop;
using SplitTool.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    public class PlayerOptionsControl : UserInputControl, IDynamicSized
    {
        private ControlListBox list;
        private Font nameFont;
        private Font detailsFont;
    
        public override bool CanProceed => true;

        public override bool CanPlay => true;

        public override string Title => "Player Options";

        public PlayerOptionsControl()
        {
            nameFont = new Font("Segoe UI", 18);
            detailsFont = new Font("Segoe UI", 12);
        }
        public override void Initialize(UserGameInfo game, GameProfile profile)
        {
            base.Initialize(game, profile);
           
            Controls.Clear();
           
            int wid = 200;

            list = new ControlListBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = Size,
                AutoScroll = true,
            };
            
            List<GameOption> options = game.Game.Options;
            Dictionary<string, object> vals = profile.Options;
            for (int j = 0; j < options.Count; j++)
            {
                GameOption opt = options[j];
                if (opt.Hidden)
                { continue; }

                if (!vals.TryGetValue(opt.Key, out object val))
                {
                    continue;
                }

                CoolListControl cool = new CoolListControl(false)
                {
                    Title = opt.Name,
                    Details = opt.Description,
                    Width = list.Width,
                    Font = new Font("Segoe UI", 8)
                };

                CoolListControl cool = new CoolListControl(false);
                cool.Title = opt.Name;
                cool.Details = opt.Description;
                cool.Width = list.Width;
                cool.Font = new Font("Segoe UI", 11);

                list.Controls.Add(cool);

                // Check the value type and add a control for it
                if (opt.Value is Enum || opt.List != null)
                {
                    ComboBox box = new ComboBox();
                    int border = 10;

                    object value;
                    object defaultValue;
                    IList values;
                    if (opt.Value is Enum)
                    {
                        value = (Enum)val;
                        values = Enum.GetValues(value.GetType());
                        defaultValue = opt.DefaultValue;
                    }
                    else
                    {
                        value = opt.List[0];
                        values = opt.List;
                        defaultValue = opt.DefaultValue;
                    }

                    for (int i = 0; i < values.Count; i++)
                    {
                        box.Items.Add(values[i]);
                        
                    }

                    if (defaultValue != null)
                    {
                        box.SelectedIndex = box.Items.IndexOf(defaultValue);
                        if (box.SelectedIndex == -1)
                        {
                            box.SelectedIndex = box.Items.IndexOf(value);
                        }
                    }
                    else
                    {
                        box.SelectedIndex = box.Items.IndexOf(value);
                    }

                    box.Width = wid;
                    box.Height = 40;
                    box.Left = cool.Width - box.Width - border;
                    box.Top = (cool.Height / 2) - (box.Height / 2);
                    box.Anchor = AnchorStyles.Right;
                    box.DropDownStyle = ComboBoxStyle.DropDownList;
                    cool.Controls.Add(box);

                    box.Tag = opt;
                    box.SelectedValueChanged += box_SelectedValueChanged;
                    ChangeOption(box.Tag, box.SelectedItem);
                }
                else if (opt.Value is bool)
                {
                    SizeableCheckbox box = new SizeableCheckbox();
                    int border = 10;

                    box.Checked = (bool)val;
                    box.Width = 40;
                    box.Height = 40;
                    box.Left = cool.Width - box.Width - border;
                    box.Top = (cool.Height / 2) - (box.Height / 2);
                    box.Anchor = AnchorStyles.Right;
                    cool.Controls.Add(box);

                    box.Tag = opt;
                    box.CheckedChanged += box_CheckedChanged;
                    ChangeOption(box.Tag, box.Checked);
                }
                else if (opt.Value is int || opt.Value is double)
                {
                    NumericUpDown num = new NumericUpDown();
                    int border = 10;

                    int value = (int)(double)val;
                    if (value < num.Minimum)
                    {
                        num.Minimum = value;
                    }

                    num.Value = value;

                    num.Width = wid;
                    num.Height = 40;
                    num.Left = cool.Width - num.Width - border;
                    num.Top = (cool.Height / 2) - (num.Height / 2);
                    num.Anchor = AnchorStyles.Right;
                    cool.Controls.Add(num);

                    num.Tag = opt;
                    num.ValueChanged += num_ValueChanged;
                    ChangeOption(num.Tag, num.Value);
                }
                else if (opt.Value is GameOptionValue)
                {
                    ComboBox box = new ComboBox();
                    int border = 10;

                    GameOptionValue value = (GameOptionValue)val;
                    PropertyInfo[] props = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static);

                    for (int i = 0; i < props.Length; i++)
                    {
                        PropertyInfo prop = props[i];
                        box.Items.Add(prop.GetValue(null, null));
                    }
                    box.SelectedIndex = box.Items.IndexOf(value);

                    box.Width = wid;
                    box.Height = 40;
                    box.Left = cool.Width - box.Width - border;
                    box.Top = (cool.Height / 2) - (box.Height / 2);
                    box.Anchor = AnchorStyles.Right;
                    //box.DropDownStyle = ComboBoxStyle.DropDownList;
                    cool.Controls.Add(box);

                    box.Tag = opt;
                    box.SelectedValueChanged += box_SelectedValueChanged;
                    ChangeOption(box.Tag, box.SelectedItem);
                }
            }

            Controls.Add(list);
            list.UpdateSizes();

            CanPlayUpdated(true, false);
            DPIManager.Register(this);       
            DPIManager.Update(this);
        }

        private void ChangeOption(object tag, object value)
        {
            // boxing but wahtever
            GameOption option = (GameOption)tag;
            profile.Options[option.Key] = value;
        }

        private void box_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox check = (ComboBox)sender;
            if (check.SelectedItem == null)
            {
                return;
            }
            ChangeOption(check.Tag, check.SelectedItem);
        }

        private void num_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown check = (NumericUpDown)sender;
            ChangeOption(check.Tag, check.Value);
        }

        private void box_CheckedChanged(object sender, EventArgs e)
        {
            SizeableCheckbox check = (SizeableCheckbox)sender;
            ChangeOption(check.Tag, check.Checked);
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }
            
            if (scale > 1.0F)
            {
                float newFontSize = 7 * scale;

                foreach (Control c in Controls)
                {
                    foreach (Control child in c.Controls)
                    {
                        SuspendLayout();
                        child.Font = new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                        ResumeLayout();
                    }
                }
            }

        }

    }
}
