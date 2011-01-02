using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace Driftoid
{
    /// <summary>
    /// Represents a static texture in graphics memory. Mangement of textures can be completely handled by this class. Also contains texture-related functions.
    /// </summary>
    public class Texture
    {
        private Texture()
        {

        }

        /// <summary>
        /// Gets the id of the texture.
        /// </summary>
        public int ID
        {
            get
            {
                if (this._ID < 0)
                {
                    using (Bitmap bm = new Bitmap(256, 256))
                    {
                        this._Source.Draw(bm);
                        this._ID = Create(bm);
                    }
                }
                return this._ID;
            }
        }

        /// <summary>
        /// Defines a texture based on a drawer source.
        /// </summary>
        /// <param name="AspectRatio">The target aspect ratio for the image.</param>
        /// <param name="Detail">A relative estimate of the detail level required for the texture.</param>
        public static Texture Define(Drawer Source, double AspectRatio, double Detail)
        {
            return new Texture()
            {
                _Source = Source,
                _AspectRatio = AspectRatio,
                _Detail = Detail,
                _ID = -1
            };
        }

        /// <summary>
        /// Binds the specified 2D texture.
        /// </summary>
        public static void Bind2D(int Texture)
        {
            GL.BindTexture(TextureTarget.Texture2D, Texture);
        }

        /// <summary>
        /// Creates a texture for a bitmap.
        /// </summary>
        public static int Create(Bitmap Source)
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

        private Drawer _Source;
        private double _AspectRatio;
        private double _Detail;
        private int _ID;
    }
}