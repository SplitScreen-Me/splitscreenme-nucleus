using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Generic.Step;
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

        private Font nameFont;
        private Font detailsFont;

        public CustomStep CustomStep;
        public ContentManager Content;


        public override bool CanProceed => canProceed;
        public override string Title => CustomStep.Title;
        public override bool CanPlay => canPlay;

        public override bool CanProceed
        {
            get { return canProceed; }
        }
        public override string Title
        {
            get { return CustomStep.Title; }
        }
        public override bool CanPlay
        {
            get { return canPlay; }
        }



        public bool HasProperty(IDictionary<string, object> expando, string key)
        {
            return expando.ContainsKey(key);
        }

        private IList collection;

        public override void Initialize(UserGameInfo game, GameProfile profile)
        {
            base.Initialize(game, profile);

            Controls.Clear();

            // grab the CustomStep and extract what we have to show from it
            GameOption option = CustomStep.Option;

            if (option.List == null)
            {

            }
            else
            {
                ControlListBox list = new ControlListBox
                {
                    Size = Size,
                    AutoScroll = true
                };

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
                        BackColor = Color.FromArgb(30, 30, 30),
                        Size = new Size(list.Width, 120),
                        Data = val
                    };
                    control.OnSelected += Control_OnSelected;

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
                            Image img = Content.LoadImage(imageUrl);

                            PictureBox box = new PictureBox
                            {
                                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                                Size = new Size(140, 80)
                            };
                            box.Location = new Point(list.Width - box.Width - 10, 20);
                            box.SizeMode = PictureBoxSizeMode.Zoom;
                            box.Image = img;
                            control.Controls.Add(box);
                        }
                    }

                    list.Controls.Add(control);
                }
            }
        }

        private void Control_OnSelected(object obj)
        {
            profile.Options[CustomStep.Option.Key] = obj;

            canProceed = true;
            CanPlayUpdated(true, true);
        }

    }
}
