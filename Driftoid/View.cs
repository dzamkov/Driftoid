using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Driftoid
{
    /// <summary>
    /// Represents a view of an area.
    /// </summary>
    public struct View
    {
        public View(Vector Center, double Rotation, double Zoom)
        {
            this.Center = Center;
            this.Rotation = Rotation;
            this.Zoom = Zoom;
        }

        /// <summary>
        /// Sets this view as the current one for future rendering use.
        /// </summary>
        public void Setup(double AspectRatio)
        {
            double vsw;
            double vsh;
            if (AspectRatio > 1.0)
            {
                vsw = 1.0 / AspectRatio;
                vsh = 1.0;
            }
            else
            {
                vsh = AspectRatio;
                vsw = 1.0;
            }
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Scale(vsw, vsh, 1.0);
            GL.Rotate(this.Rotation * 180 / Math.PI, 0.0, 0.0, 1.0);
            GL.Scale(this.Zoom, this.Zoom, 1.0);
            GL.Translate(-this.Center.X, -this.Center.Y, 0.0);
        }

        /// <summary>
        /// Draws a textured square at the specified location using the currently set view.
        /// </summary>
        /// <param name="Size">Twice the edge length of the square, the radius in the case of the circle.</param>
        public static void DrawTexturedSquare(Vector Location, double Size)
        {
            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();

            float fx = (float)Location.X;
            float fy = (float)Location.Y;
            float hs = (float)Size;
            GL.Begin(BeginMode.Quads);
            GL.Color4(1.0, 1.0, 1.0, 1.0);
            GL.Vertex2(-hs + fx, -hs + fy); GL.TexCoord2(0f, 0f);
            GL.Vertex2(-hs + fx, hs + fy); GL.TexCoord2(0f, 1f);
            GL.Vertex2(hs + fx, hs + fy); GL.TexCoord2(1f, 1f);
            GL.Vertex2(hs + fx, -hs + fy); GL.TexCoord2(1f, 0f);
            GL.End();
        }

        /// <summary>
        /// The point in the center of the view.
        /// </summary>
        public Vector Center;

        /// <summary>
        /// The rotation of the view, in radians.
        /// </summary>
        public double Rotation;

        /// <summary>
        /// The zoom level of the view.
        /// </summary>
        public double Zoom;
    }
}