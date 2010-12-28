using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Driftoid
{
    /// <summary>
    /// A circular, drifting object.
    /// </summary>
    public class Driftoid
    {
        /// <summary>
        /// Creates a texture for a solid driftoid (circle with a border).
        /// </summary>
        public static int CreateSolidTexture(
            int TextureSize,
            float BorderSize,
            Color BorderColor,
            Color InteriorColor)
        {
            using (Bitmap bm = new Bitmap(TextureSize, TextureSize))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    float actualsize = (float)TextureSize;
                    float actualborder = BorderSize * actualsize;
                    g.Clear(Color.FromArgb(0, BorderColor.R, BorderColor.G, BorderColor.B));
                    using (Brush fillbrush = new SolidBrush(InteriorColor))
                    {
                        g.FillEllipse(fillbrush, new RectangleF(
                            actualborder / 2.0f, actualborder / 2.0f, actualsize - actualborder, actualsize - actualborder));

                    }
                    for (float f = 0.5f; f < actualborder - 0.5f; f += 0.5f)
                    {
                        float eb = f;
                        float vis = Math.Min(Math.Min(f, (actualborder - f)) / 4.0f, 1f);
                        using (Pen borderpen = new Pen(
                            Color.FromArgb((int)(vis * (float)BorderColor.A), BorderColor.R, BorderColor.G, BorderColor.B), 
                            0.5f))
                        {
                            g.DrawEllipse(borderpen,
                                new RectangleF(
                                    eb, eb,
                                    actualsize - eb * 2.0f, actualsize - eb * 2.0f));
                        }
                    }
                }
                return Texture.Create(bm);
            }
        }
    }
}