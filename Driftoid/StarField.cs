using System;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// The background for the game.
    /// </summary>
    public class Starfield
    {
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
                return Driftoid.MakeTexture(bm);
            }
        }

    }
}