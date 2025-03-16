using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Generic.Step;
using Nucleus.Gaming.UI;
using SplitTool.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public class JSUserInputControl : UserInputControl
    {
        private bool canProceed;
        private bool canPlay;

        public CustomStep CustomStep;
        public ContentManager Content;

        public override bool CanProceed => canProceed;
        public override string Title => CustomStep.Title;
        public override bool CanPlay => canPlay;
        private CoolListControl toSelect;

        public bool HasProperty(IDictionary<string, object> expando, string key)
        {
            return expando.ContainsKey(key);
        }

        private IList collection;

        public override void Initialize(UserGameInfo game, GameProfile profile)
        {
            base.Initialize(game, profile);

            toSelect = null;

            foreach(Control c in Controls){ c.Dispose();}

            // grab the CustomStep and extract what we have to show from it
            GameOption option = CustomStep.Option;

            if (option.List != null)
            {
                ControlListBox list = new ControlListBox
                {
                    Size = Size,
                    AutoScroll = true
                };

                list.Cursor = Theme_Settings.Default_Cursor;
                Controls.Add(list);

                collection = option.List;
                for (int i = 0; i < collection.Count; i++)
                {
                    object val = collection[i];

                    if (!(val is IDictionary<string, object>))
                    {
                        continue;
                    }

                    CoolListControl control = new CoolListControl(true)
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                        Size = new Size(list.Width, 120),
                        Data = val
                    };

                    control.OnSelected += Control_OnSelected;
                    control.Cursor = Theme_Settings.Hand_Cursor;

                    IDictionary<string, object> value = (IDictionary<string, object>)val;
                    string name = value["Name"].ToString();

                    control.Title = name;

                    string details = "";
                    if (value.TryGetValue("Details", out object detailsObj))
                    {
                        details = detailsObj.ToString();

                        control.Details = details;
                    }

                    value.TryGetValue("ImageUrl", out object imageUrlObj);
                    
                    if (imageUrlObj != null)
                    {
                        string imageUrl = imageUrlObj.ToString();

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            control.ImageUrl = imageUrl;
 
                            PictureBox box = new PictureBox
                            {
                                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                                Size = new Size(140, 80),
                                Name = "pictureBox"
                            };

                            box.Location = new Point(list.Width - box.Width - 10, 10);
                            box.SizeMode = PictureBoxSizeMode.Zoom;
                            box.Image = Content.LoadImage(imageUrl);
                      
                            control.Controls.Add(box);
                        }
                        else
                        {
                            control.ImageUrl = "dummy";
                        }
                    }

                    list.Controls.Add(control);

                    foreach (KeyValuePair<string, object> opt in profile.Options)
                    {
                        if (opt.Value.ToString().Contains(control.Title))
                        {
                            toSelect = control;
                        }
                    }
                }

                if (toSelect != null)
                    Control_AutoSelect();
            }
        }

        public void Control_AutoSelect()
        {
            if (toSelect == null)
            {
                return;
            }

            toSelect.BackColor = Color.DodgerBlue;
            toSelect.Title = $"{toSelect.Title} (Auto Selection)";
            Control_OnSelected(toSelect);
        }

        private void Control_OnSelected(object obj)
        {
            if (obj.GetType() == typeof(CoolListControl))
            {
                CoolListControl c = obj as CoolListControl;
                profile.Options[CustomStep.Option.Key] = c.Data;
            }
            else
            {
                profile.Options[CustomStep.Option.Key] = obj;
            }

            canProceed = true;
            CanPlayUpdated(true, true);
        }

    }
}
