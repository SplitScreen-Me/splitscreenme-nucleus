using Nucleus.Gaming;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using SplitTool.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    public class PlayerOptionsControl : UserInputControl, IDynamicSized
    {
        private ControlListBox list;
        public override bool CanProceed => true;
        public override bool CanPlay => true;
        private float _scale;
        public override string Title => "Handler Options";
        private Dictionary<string, object> vals;

        public PlayerOptionsControl()
        {
            BackColor = Color.Transparent;
            DPIManager.Register(this);
            DPIManager.Update(this);
        }

        public override void Initialize(UserGameInfo game, GameProfile profile)
        {
            base.Initialize(game, profile);
            this.game = game;

            foreach (Control c in Controls) {c.Dispose();}

            int wid = 200;

            list = new ControlListBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = Size,
                AutoScroll = true,
            };

            list.Cursor = Theme_Settings.Default_Cursor;

            List<GameOption> options = game.Game.Options;
            vals = profile.Options;

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
                    Font = new Font("Segoe UI", 8 * _scale)
                };

                list.Controls.Add(cool);

                // Check the value type and add a control for it
                if (opt.Value is Enum || opt.List != null && opt.List.Count != 0)
                {
                    ComboBox box = new ComboBox();
                    box.BackColor = Color.Black;
                    box.ForeColor = Color.White;
                    box.FlatStyle = FlatStyle.Flat;

                    int border = 10;

                    object value;
                    object defaultValue = vals[opt.Key];
                    IList values;

                    if (opt.Value is Enum)
                    {
                        value = (Enum)val;
                        values = Enum.GetValues(value.GetType());
                    }
                    else
                    {
                        value = opt.List[0];
                        values = opt.List;
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
                        if (value != null)
                        {
                            box.SelectedIndex = box.Items.IndexOf(value);
                        }
                    }

                    box.Width = (int)(wid * _scale);
                    box.Height = (int)(40 * _scale);
                    box.Left = cool.Width - box.Width - border;
                    box.Top = (cool.Height / 2) - (box.Height / 2);
                    box.Anchor = AnchorStyles.Right;
                    box.DropDownStyle = ComboBoxStyle.DropDownList;
                    box.Cursor = Theme_Settings.Hand_Cursor;

                    cool.Controls.Add(box);

                    box.Tag = opt;
                    box.SelectedValueChanged += box_SelectedValueChanged;
                    ChangeOption(box.Tag, box.SelectedItem);
                }
                else if (opt.Value is bool && opt.List.Count != 0)
                {
                    SizeableCheckbox box = new SizeableCheckbox();
                    box.BackColor = Color.Black;
                    box.ForeColor = Color.White;
                    box.FlatStyle = FlatStyle.Flat;

                    int border = 10;

                    box.Checked = (bool)val;
                    box.Width = (int)(40 * _scale);
                    box.Height = (int)(40 * _scale);
                    box.Left = cool.Width - box.Width - border;
                    box.Top = (cool.Height / 2) - (box.Height / 2);
                    box.Anchor = AnchorStyles.Right;
                    box.Cursor = Theme_Settings.Hand_Cursor;

                    cool.Controls.Add(box);

                    box.Tag = opt;
                    box.CheckedChanged += box_CheckedChanged;
                    ChangeOption(box.Tag, box.Checked);
                }
                else if ((opt.Value is int || opt.Value is double) && opt.List.Count != 0)
                {
                    NumericUpDown num = new NumericUpDown();
                    num.BackColor = Color.Black;
                    num.ForeColor = Color.White;

                    int border = 10;

                    int value = (int)(double)val;
                    if (value < num.Minimum)
                    {
                        num.Minimum = value;
                    }

                    num.Value = value;

                    num.Width = (int)(wid * _scale);
                    num.Height = (int)(40 * _scale);
                    num.Left = cool.Width - num.Width - border;
                    num.Top = (cool.Height / 2) - (num.Height / 2);
                    num.Anchor = AnchorStyles.Right;
                    num.Cursor = Theme_Settings.Hand_Cursor;

                    cool.Controls.Add(num);

                    num.Tag = opt;
                    num.ValueChanged += num_ValueChanged;
                    ChangeOption(num.Tag, num.Value);
                }
                else if (opt.Value is GameOptionValue && opt.List.Count != 0)
                {
                    ComboBox box = new ComboBox();
                    box.BackColor = Color.Black;
                    box.ForeColor = Color.White;
                    box.FlatStyle = FlatStyle.Flat;

                    int border = 10;

                    GameOptionValue value = (GameOptionValue)val;
                    PropertyInfo[] props = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static);

                    for (int i = 0; i < props.Length; i++)
                    {
                        PropertyInfo prop = props[i];
                        box.Items.Add(prop.GetValue(null, null));
                    }

                    box.SelectedIndex = box.Items.IndexOf(value);

                    box.Width = (int)(wid * _scale);
                    box.Height = (int)(40 * _scale);
                    box.Left = cool.Width - box.Width - border;
                    box.Top = (cool.Height / 2) - (box.Height / 2);
                    box.Anchor = AnchorStyles.Right;
                    box.Cursor = Theme_Settings.Hand_Cursor;

                    cool.Controls.Add(box);

                    box.Tag = opt;
                    box.SelectedValueChanged += box_SelectedValueChanged;

                    ChangeOption(box.Tag, box.SelectedItem);
                }
                else if (opt.List.Count == 0)
                {
                    TextBox box = new TextBox();
                    box.BackColor = Color.Black;
                    box.ForeColor = Color.White;

                    int border = 10;

                    box.TextChanged += box_TextChanged;
                    box.Width = (int)(wid * _scale);
                    box.Height = (int)(40 * _scale);
                    box.Left = cool.Width - box.Width - border;
                    box.Top = (cool.Height / 2) - (box.Height / 2);
                    box.Anchor = AnchorStyles.Right;
                    box.WordWrap = true;
                    box.Cursor = Theme_Settings.Hand_Cursor;

                    cool.Controls.Add(box);
                    box.Tag = opt;

                    ///Check if some custom values were added 
                    ///before going back to "player setup screen" and add them back in their respective TextBox.
                    GameOption cast = box.Tag as GameOption;

                    if (vals.Any(v => (string)v.Key == (string)cast.Key))
                    {
                        if (vals[cast.Key].ToString() != "")
                        {
                            box.Text = vals[cast.Key].ToString();
                        }
                    }
                }
            }

            Controls.Add(list);
            list.UpdateSizes();
            CanPlayUpdated(true, false);
        }

        private void box_TextChanged(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;
            ///Cache custom user values in case of user going back to "player setup screen" 
            ///so they are automatically re-added when coming back to options screen,
            ///reseted if an other game is selected.
            GameOption cast = box.Tag as GameOption;
            vals[cast.Key] = box.Text;
            ChangeOption(box.Tag, box.Text);
        }

        private void ChangeOption(object tag, object value)
        {
            //boxing but whatever
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

            _scale = scale;
        }

    }
}
