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
                return MakeTexture(bm);
            }
        }

        /// <summary>
        /// Creates a texture for a bitmap.
        /// </summary>
        public static int MakeTexture(Bitmap Source)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData bd = Source.LockBits(
                new Rectangle(0, 0, Source.Width, Source.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);

            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba,
                bd.Width, bd.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            Source.UnlockBits(bd);
            return id;
        }
    }
}