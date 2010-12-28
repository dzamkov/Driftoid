using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
                            0.0f, 0.0f, actualsize, actualsize));

                    }
                    using (Pen borderpen = new Pen(BorderColor, actualborder))
                    {
                        g.DrawEllipse(borderpen,
                            new RectangleF(
                                actualborder / 2, actualborder / 2,
                                actualsize - actualborder, actualsize - actualborder));
                    }
                }
                return Texture.Create(bm);
            }
        }
    }
}