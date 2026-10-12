using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace Nucleus.Coop.Services.Settings;

/// <summary>
/// Draws the two images of the in-game shortcuts reminder (<c>ShortcutsReminder</c>, shown during sessions):
/// <c>gui\shortcuts\KbShortcutsReminder.jpeg</c> and <c>XShortcutsReminder.jpeg</c>. The legacy forms took
/// screenshots of their WinForms panels; these are drawn with System.Drawing as a plain list. The reminder window
/// stretches them into 343×283 and 277×453 boxes, so they keep those proportions (at 1.5×).
/// </summary>
public static class ShortcutsReminderRenderer
{
    /// <summary>Size of the keyboard image.</summary>
    public static readonly Size KeyboardSize = new Size(514, 424);

    /// <summary>Size of the gamepad image.</summary>
    public static readonly Size GamepadSize = new Size(416, 680);

    private static readonly Color Background = Color.FromArgb(18, 18, 20);
    private static readonly Color Title = Color.FromArgb(240, 240, 240);
    private static readonly Color LabelColor = Color.FromArgb(200, 200, 205);
    private static readonly Color KeyBackground = Color.FromArgb(44, 46, 52);
    private static readonly Color KeyBorder = Color.FromArgb(90, 94, 104);
    private static readonly Color KeyText = Color.White;
    private static readonly Color Accent = Color.FromArgb(120, 200, 80);

    /// <summary>Keyboard shortcuts: label and combination ("Ctrl+Q", "End").</summary>
    public static void RenderKeyboard(string path, IReadOnlyList<(string Label, string Keys)> rows)
    {
        Render(path, KeyboardSize, "Keyboard shortcuts", rows.Count, (g, i, rect, font) =>
        {
            DrawLabel(g, rows[i].Label, rect, font);
            string[] keys = rows[i].Keys.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            if (rows[i].Keys.EndsWith("+", StringComparison.Ordinal))
            {
                keys = keys.Concat(new[] { "+" }).ToArray(); // the "+" lock key
            }

            DrawKeycaps(g, keys.Select(k => (k.Trim(), (Image?)null)).ToList(), rect, font);
        });
    }

    /// <summary>
    /// Gamepad shortcuts: label and button codes. Button pictures come from <paramref name="buttonImagesFolder"/>
    /// (<c>gui\gamepads\&lt;type&gt;</c>) when present, names otherwise.
    /// </summary>
    public static void RenderGamepad(string path, IReadOnlyList<(string Label, int[] Buttons)> rows, string gamepadType, string? buttonImagesFolder)
    {
        var images = new Dictionary<int, Image>();
        try
        {
            foreach (var button in GamepadComboFormat.Buttons)
            {
                string file = buttonImagesFolder == null ? "" : Path.Combine(buttonImagesFolder, button.Image + ".png");
                if (File.Exists(file))
                {
                    images[button.Code] = Image.FromStream(new MemoryStream(File.ReadAllBytes(file)));
                }
            }

            Render(path, GamepadSize, "Gamepad shortcuts", rows.Count, (g, i, rect, font) =>
            {
                DrawLabel(g, rows[i].Label, rect, font);
                DrawKeycaps(g, rows[i].Buttons.Select(code => (GamepadComboFormat.Name(code, gamepadType), images.TryGetValue(code, out Image? image) ? image : null)).ToList(), rect, font);
            });
        }
        finally
        {
            foreach (Image image in images.Values)
            {
                image.Dispose();
            }
        }
    }

    private static void Render(string path, Size size, string title, int rowCount, Action<Graphics, int, RectangleF, Font> drawRow)
    {
        using var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Background);

            const float margin = 18;
            using var titleFont = new Font("Segoe UI Semibold", 20, FontStyle.Bold, GraphicsUnit.Pixel);
            using var titleBrush = new SolidBrush(Title);
            using var accent = new SolidBrush(Accent);
            g.DrawString(title, titleFont, titleBrush, margin, margin - 4);
            g.FillRectangle(accent, margin, margin + 30, 48, 3);

            float top = margin + 46;
            float rowHeight = rowCount == 0 ? 0 : Math.Min(60, (size.Height - top - margin) / rowCount);
            using var font = new Font("Segoe UI", Math.Max(12, Math.Min(17, rowHeight * 0.42f)), FontStyle.Regular, GraphicsUnit.Pixel);
            using var separator = new Pen(Color.FromArgb(34, 36, 40));

            for (int i = 0; i < rowCount; i++)
            {
                var rect = new RectangleF(margin, top + (i * rowHeight), size.Width - (2 * margin), rowHeight);
                if (i > 0)
                {
                    g.DrawLine(separator, rect.Left, rect.Top, rect.Right, rect.Top);
                }

                drawRow(g, i, rect, font);
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".tmp";
        ImageCodecInfo jpeg = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        using (var parameters = new EncoderParameters(1))
        {
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, 92L);
            bitmap.Save(temp, jpeg, parameters);
        }

        if (File.Exists(path))
        {
            File.Replace(temp, path, null);
        }
        else
        {
            File.Move(temp, path);
        }
    }

    private static void DrawLabel(Graphics g, string label, RectangleF rect, Font font)
    {
        using var brush = new SolidBrush(LabelColor);
        using var format = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        g.DrawString(label, font, brush, new RectangleF(rect.Left, rect.Top, rect.Width * 0.5f, rect.Height), format);
    }

    // Right-aligned keycaps (or button pictures) separated by "+".
    private static void DrawKeycaps(Graphics g, List<(string Text, Image? Image)> keys, RectangleF rect, Font font)
    {
        float height = Math.Min(rect.Height - 10, 30);
        float y = rect.Top + ((rect.Height - height) / 2);
        float plusWidth = g.MeasureString("+", font).Width + 4;

        var widths = keys.Select(k => k.Image != null ? height : Math.Max(height, g.MeasureString(k.Text, font).Width + 14)).ToList();
        float total = widths.Sum() + (plusWidth * Math.Max(0, keys.Count - 1));
        float x = rect.Right - total;

        using var keyBrush = new SolidBrush(KeyBackground);
        using var border = new Pen(KeyBorder);
        using var textBrush = new SolidBrush(KeyText);
        using var plusBrush = new SolidBrush(LabelColor);
        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        for (int i = 0; i < keys.Count; i++)
        {
            var box = new RectangleF(x, y, widths[i], height);
            if (keys[i].Image != null)
            {
                g.DrawImage(keys[i].Image!, box);
            }
            else
            {
                using GraphicsPath shape = RoundedRect(box, 6);
                g.FillPath(keyBrush, shape);
                g.DrawPath(border, shape);
                g.DrawString(keys[i].Text, font, textBrush, box, center);
            }

            x += widths[i];
            if (i < keys.Count - 1)
            {
                g.DrawString("+", font, plusBrush, new RectangleF(x, y, plusWidth, height), center);
                x += plusWidth;
            }
        }
    }

    private static GraphicsPath RoundedRect(RectangleF r, float radius)
    {
        float d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(r.Left, r.Top, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
