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
        public View(Vector Center, double Angle, double Zoom)
        {
            this.Center = Center;
            this.Angle = Angle;
            this.Zoom = Zoom;
        }

        /// <summary>
        /// Converts a coordinate in view space ((0, 0) for top-left corner, (1, 1) for bottom-right corner)
        /// to world coordiantes.
        /// </summary>
        public Vector ToWorld(double AspectRatio, Vector View)
        {
            if (AspectRatio > 1.0)
            {
                View.X *= AspectRatio;
                View.X -= (AspectRatio - 1.0) / 2.0;
            }
            else
            {
                AspectRatio = 1.0 / AspectRatio;
                View.Y *= AspectRatio;
                View.Y -= (AspectRatio - 1.0) / 2.0;
            }
            View.Y = 1.0 - View.Y;
            AfflineMatrix mat = AfflineMatrix.Rotation(-this.Angle) * (1.0 / this.Zoom);
            return mat * (View * 2.0 - new Vector(1.0, 1.0)) + this.Center;
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
            GL.Rotate(this.Angle * 180 / Math.PI, 0.0, 0.0, 1.0);
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
            GL.Vertex2(-hs + fx, hs + fy); GL.TexCoord2(1f, 0f);
            GL.Vertex2(hs + fx, hs + fy); GL.TexCoord2(1f, 1f);
            GL.Vertex2(hs + fx, -hs + fy); GL.TexCoord2(0f, 1f);
            GL.End();
        }

        /// <summary>
        /// The point in the center of the view.
        /// </summary>
        public Vector Center;

        /// <summary>
        /// The rotation of the view, in radians.
        /// </summary>
        public double Angle;

        /// <summary>
        /// The zoom level of the view.
        /// </summary>
        public double Zoom;
    }
}