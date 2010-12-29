using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// The background for the game.
    /// </summary>
    public struct Starfield
    {
        public Starfield(IEnumerable<Layer> Layers)
        {
            this.Layers = Layers;
        }

        /// <summary>
        /// Creates the starfield with the default settings.
        /// </summary>
        public static Starfield CreateDefault(int TextureSize, int TextureAmount)
        {
            Random r = new Random();

            List<Layer> layers = new List<Layer>();
            double offset = 0.98;
            double scale = 5.0;
            int[] texs = new int[TextureAmount];
            for (int t = 0; t < TextureAmount; t++)
            {
                texs[t] = CreateStarfieldTexture(TextureSize, 20, 0.1f, 0.8f, 0.02f, Color.Gray, r);
            }
            int last = 0;
            for (int t = 0; t < 6; t++)
            {
                int tex = (r.Next(TextureAmount - 1) + last) % TextureAmount;
                last = tex;
                layers.Add(new Layer()
                {
                    Scale = scale,
                    Offset = offset,
                    TextureID = texs[tex]
                });
                offset *= 0.9;
                scale /= 0.6;
            }

            return new Starfield(layers);
        }

        /// <summary>
        /// Suggested background color.
        /// </summary>
        public static readonly Color Background = Color.FromArgb(255, 0, 32, 0);

        /// <summary>
        /// Draws the starfield with the specified view, reseting the current view.
        /// </summary>
        public void Draw(View View, double AspectRatio)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Texture);
            double zoom = View.Zoom;

            foreach (Layer l in this.Layers)
            {
                double visibility = 0.7 - Math.Abs(Math.Log10(l.Scale * 2.0) - Math.Log10(1.0 / zoom)) * 1.6;
                if (visibility > 0.0)
                {
                    GL.LoadIdentity();
                    GL.Scale(1.0 / l.Scale, 1.0 / l.Scale, 1.0);
                    GL.Translate(View.Center.X / 2.0 * l.Offset, -View.Center.Y / 2.0 * l.Offset, 0.0);
                    GL.Rotate(View.Angle * 180 / Math.PI, 0.0, 0.0, 1.0);
                    if (AspectRatio > 1.0)
                    {
                        GL.Scale(AspectRatio / zoom, 1.0 / zoom, 1.0);
                    }
                    else
                    {
                        GL.Scale(1.0 / zoom, 1.0 / zoom / AspectRatio, 1.0);
                    }
                    GL.Translate(-0.5, -0.5, 1.0);

                    Texture.Bind2D(l.TextureID);
                    GL.Begin(BeginMode.Quads);
                    GL.Color4(1.0, 1.0, 1.0, visibility);
                    GL.Vertex2(-1.0f, -1.0f); GL.TexCoord2(0f, 0f);
                    GL.Vertex2(-1.0f, 1.0f); GL.TexCoord2(1f, 0f);
                    GL.Vertex2(1.0f, 1.0f); GL.TexCoord2(1f, 1f);
                    GL.Vertex2(1.0f, -1.0f); GL.TexCoord2(0f, 1f);
                    GL.End();
                }
            }
        }

        /// <summary>
        /// Creates a texture for a starfield (bunch of dots).
        /// </summary>
        /// <param name="TextureSize"></param>
        /// <param name="Radius"></param>
        /// <returns></returns>
        public static int CreateStarfieldTexture(
            int TextureSize,
            int GridSize,
            float Density,
            float Drift,
            float StarRadius,
            Color StarColor,
            Random Random)
        {
            using (Bitmap bm = new Bitmap(TextureSize, TextureSize))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    g.Clear(Color.Transparent);
                    float actualsize = (float)TextureSize;
                    float actualradius = StarRadius * actualsize;
                    float gridinterval = actualsize / GridSize;
                    float gridoffset = gridinterval / 2.0f;
                    float maxmove = gridinterval * Drift;
                    for (int gx = 0; gx < GridSize; gx++)
                    {
                        for (int gy = 0; gy < GridSize; gy++)
                        {
                            if (Random.NextDouble() < Density)
                            {
                                float x = gridoffset + (gridinterval * gx) + ((float)Random.NextDouble() - 0.5f) * maxmove;
                                float y = gridoffset + (gridinterval * gy) + ((float)Random.NextDouble() - 0.5f) * maxmove;
                                for (float r = 0f; r < 1f; r += 1f / actualradius)
                                {
                                    float rad = r * actualradius;
                                    using (Pen p = new Pen(
                                        Color.FromArgb(
                                        (byte)((float)StarColor.A * (1f - r)),
                                        StarColor.R,
                                        StarColor.G,
                                        StarColor.B), 1.0f))
                                    {
                                        g.DrawEllipse(p, x - rad / 2, y - rad / 2, rad, rad);
                                    }
                                }
                            }
                        }
                    }
                }
                return Texture.Create(bm);
            }
        }

        /// <summary>
        /// A single layer of the starfield.
        /// </summary>
        public struct Layer
        {
            public int TextureID;
            public double Scale;
            public double Offset;
        }

        public IEnumerable<Layer> Layers;
    }
}