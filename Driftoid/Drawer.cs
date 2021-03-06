﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Driftoid
{
    /// <summary>
    /// Describes an image that can be converted to a texture or a bitmap. Drawers are slow when used directly.
    /// </summary>
    public abstract class Drawer
    {
        /// <summary>
        /// Gets the final color of the image at the specified point between (0.0, 0.0) and (1.0, 1.0).
        /// </summary>
        public abstract Color AtPoint(Vector Point);

        /// <summary>
        /// Draws to a bitmap, completely overwriting it.
        /// </summary>
        public void Draw(Bitmap Bitmap)
        {
            BitmapData bd = Bitmap.LockBits(
                new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), 
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            this.Draw(bd, Bitmap.Width, Bitmap.Height);
            Bitmap.UnlockBits(bd);
        }

        /// <summary>
        /// Draws to bitmap data.
        /// </summary>
        public unsafe void Draw(BitmapData Data, int Width, int Height)
        {
            Vector delta = new Vector(1.0 / (double)Width, 1.0 / (double)Height);
            Vector initial = delta * 0.5;
            byte* dataloc = (byte*)Data.Scan0.ToPointer();
            for (int y = 0; y < Width; y++)
            {
                for (int x = 0; x < Height; x++)
                {
                    Vector point = initial + new Vector((double)x * delta.X, (double)y * delta.Y);
                    Color col = this.AtPoint(point);
                    dataloc[3] = (byte)(col.A * 255.0);
                    dataloc[2] = (byte)(col.R * 255.0);
                    dataloc[1] = (byte)(col.G * 255.0);
                    dataloc[0] = (byte)(col.B * 255.0);
                    dataloc += 4;
                }
            }
        }

        /// <summary>
        /// Performs antialiasing on a drawer.
        /// </summary>
        public static AntiAliasingDrawer Antialias(Drawer Source, double Size)
        {
            return new AntiAliasingDrawer(Source, Size, 3);
        }

        /// <summary>
        /// Creates a texture for this drawer.
        /// </summary>
        public int CreateTexture(int Size)
        {
            using (Bitmap bm = new Bitmap(Size, Size))
            {
                this.Draw(bm);
                return Texture.Create(bm);
            }
        }

        /// <summary>
        /// A function with a purpose equivalent to Drawer.
        /// </summary>
        public delegate Color DrawerHandler(Vector Point);

        /// <summary>
        /// Creates a handler from a function.
        /// </summary>
        public static Drawer Create(DrawerHandler Handler)
        {
            return new _FuncDrawer()
            {
                Handler = Handler
            };
        }

        private sealed class _FuncDrawer : Drawer
        {
            public override Color AtPoint(Vector Point)
            {
                return this.Handler(Point);
            }

            public DrawerHandler Handler;
        }
    }

    /// <summary>
    /// A drawer that performs antialiasing on a drawer it is provided.
    /// </summary>
    public class AntiAliasingDrawer : Drawer
    {
        public AntiAliasingDrawer(Drawer Source, double Size, int Quality)
        {
            this._Samples = new List<Sample>(Quality * Quality);
            double delta = 1.0 / (double)Quality;
            double inital = -0.5 + delta * 0.5;
            double totalintensity = 0.0;
            for (int x = 0; x < Quality; x++)
            {
                for (int y = 0; y < Quality; y++)
                {
                    Vector point = new Vector((double)x * delta + inital, (double)y * delta + inital);
                    double intensity = IntensityAtDistance(point.Length * 2.0);
                    totalintensity += intensity;
                    this._Samples.Add(new Sample()
                    {
                        Offset = point * Size,
                        Intensity = intensity
                    });
                }
            }
            this._Source = Source;
        }

        public static double IntensityAtDistance(double Distance)
        {
            return Math.Cos(Distance * (2.0 / Math.PI));
        }

        public override Color AtPoint(Vector Point)
        {
            Color col = new Color();
            double totalint = 0.0;
            foreach (Sample samp in this._Samples)
            {
                Color sampcol = this._Source.AtPoint(Point + samp.Offset);
                col.A += sampcol.A * samp.Intensity;
                col.R += sampcol.R * samp.Intensity;
                col.G += sampcol.G * samp.Intensity;
                col.B += sampcol.B * samp.Intensity;
                totalint += samp.Intensity;
            }
            double itotalint = 1.0 / totalint;
            col.A *= itotalint;
            col.R *= itotalint;
            col.G *= itotalint;
            col.B *= itotalint;
            return col;
        }

        /// <summary>
        /// Represents a sample point used for antialiasing.
        /// </summary>
        public struct Sample
        {
            public Vector Offset;
            public double Intensity;
        }

        private List<Sample> _Samples;
        private Drawer _Source;
    }
}