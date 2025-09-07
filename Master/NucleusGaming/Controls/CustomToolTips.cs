using Nucleus.Coop;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls
{
    public static class CustomToolTips
    {
        private static ConcurrentDictionary<string, CustomToolTip> tooltipList = new ConcurrentDictionary<string, CustomToolTip>();

        public class CustomToolTip : ToolTip
        {
            public string Id;
        }

        public static void SetToolTip(Control control, string text, string id, int[] rgbBackColor, int[] rgbForeColor, int delay = 100)
        {
            //Avoid Tooltips duplication
            CustomToolTip tooltipToRemove;
            tooltipList.TryRemove(id, out tooltipToRemove);

            tooltipToRemove?.Dispose();

            CustomToolTip tooltip = new CustomToolTip
            {
                InitialDelay = delay,
                ReshowDelay = 1500,
                AutoPopDelay = 4000,
                OwnerDraw = true,
                BackColor = Color.FromArgb(255, rgbBackColor[1], rgbBackColor[2], rgbBackColor[3]),
                ForeColor = Color.FromArgb(255, rgbForeColor[1], rgbForeColor[2], rgbForeColor[3]),
                UseAnimation = false,
                ShowAlways = true,
                UseFading = true,//Default setting
                Id = id
            };

            tooltip.Draw += Tooltip_Draw;
            tooltip.SetToolTip(control, text);
            tooltipList.TryAdd(id, tooltip);
        }


        public static CustomToolTip GetControlToolTip(string toolTipName)
        {
           return tooltipList.TryGetValue(toolTipName, out CustomToolTip tooltip) ? tooltip : null;
        }

        private static void Tooltip_Draw(object sender, DrawToolTipEventArgs e)
        {
            CustomToolTip tooltip = sender as CustomToolTip;

            e.DrawBackground();
            e.DrawBorder();
            SolidBrush brush = new SolidBrush(tooltip.ForeColor);
            e.Graphics.DrawString(e.ToolTipText, e.Font, brush, 2, 2);

            //if (e.AssociatedControl.GetType() == typeof(GameControl))
            //{
            //    foreach (KeyValuePair<Control, ToolTip> t in tooltipList)
            //    {
            //        if (t.Key.GetType() == typeof(GameControl))
            //        {
            //            t.Value.InitialDelay += 200;
            //        }
            //    }
            //}
            //else
            //{
            //    tooltip.InitialDelay += 100;
            //}

            brush.Dispose();
        }

    }
}
