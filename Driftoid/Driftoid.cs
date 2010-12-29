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
    public abstract class Driftoid
    {
        public Driftoid(double Radius, DriftoidState MotionState)
        {
            this._Radius = Radius;
            this._MotionState = MotionState;
        }

        /// <summary>
        /// Gets the texture ID that should be used to render the driftoid.
        /// </summary>
        public abstract int TextureID { get; }

        /// <summary>
        /// Prepares the current GL context for drawing. Should be called after setting up the view.
        /// </summary>
        public static void SetupDraw()
        {
            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Color4(1.0, 1.0, 1.0, 1.0);
        }

        /// <summary>
        /// Draws the driftoid to the current view.
        /// </summary>
        public void Draw()
        {
            Texture.Bind2D(this.TextureID);
            GL.PushMatrix();
            GL.Scale(this._Radius, this._Radius, 1.0);
            GL.Translate(this._MotionState.Position.X, this._MotionState.Position.Y, 0.0);
            GL.Rotate(this._MotionState.Angle, 0.0, 0.0, 1.0);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(-1.0f, -1.0f); GL.TexCoord2(0f, 0f);
            GL.Vertex2(-1.0f, 1.0f); GL.TexCoord2(1f, 0f);
            GL.Vertex2(1.0f, 1.0f); GL.TexCoord2(1f, 1f);
            GL.Vertex2(1.0f, -1.0f); GL.TexCoord2(0f, 1f);
            GL.End();
            GL.PopMatrix();
        }

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
                    DrawSolid(g, TextureSize, BorderSize, BorderColor, InteriorColor);
                }
                return Texture.Create(bm);
            }
        }

        /// <summary>
        /// Draws a blank circle to the specified graphics context.
        /// </summary>
        public static void DrawSolid(
            Graphics Context, int Size, float BorderSize, 
            Color BorderColor, Color InteriorColor)
        {
            float actualsize = (float)Size;
            float actualborder = BorderSize * actualsize;
            Context.Clear(Color.FromArgb(0, BorderColor.R, BorderColor.G, BorderColor.B));
            using (Brush fillbrush = new SolidBrush(InteriorColor))
            {
                Context.FillEllipse(fillbrush, new RectangleF(
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
                    Context.DrawEllipse(borderpen,
                        new RectangleF(
                            eb, eb,
                            actualsize - eb * 2.0f, actualsize - eb * 2.0f));
                }
            }
        }

        /// <summary>
        /// Gets the state of the motion of the driftoid.
        /// </summary>
        public DriftoidState MotionState
        {
            get
            {
                return this._MotionState;
            }
        }

        /// <summary>
        /// Gets the radius of the driftoid.
        /// </summary>
        public double Radius
        {
            get
            {
                return this._Radius;
            }
        }

        private double _Radius;
        private DriftoidState _MotionState;
    }

    /// <summary>
    /// The physical/motion state of a driftoid.
    /// </summary>
    public struct DriftoidState
    {
        public DriftoidState(Vector Position)
        {
            this.Position = Position;
            this.Angle = 0.0;
            this.AngularVelocity = 0.0;
            this.Velocity = new Vector();
        }

        public DriftoidState(Vector Position, double Angle)
        {
            this.Position = Position;
            this.Angle = Angle;
            this.AngularVelocity = 0.0;
            this.Velocity = new Vector();
        }

        public DriftoidState(Vector Position, Vector Velocity, double Angle, double AngularVelocity)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Angle = Angle;
            this.AngularVelocity = AngularVelocity;
        }

        /// <summary>
        /// Position of the driftoid.
        /// </summary>
        public Vector Position;

        /// <summary>
        /// Velocity of the driftoid in units per second.
        /// </summary>
        public Vector Velocity;

        /// <summary>
        /// Angle of the driftoid in radians.
        /// </summary>
        public double Angle;

        /// <summary>
        /// Spin of the driftoid in radians per second.
        /// </summary>
        public double AngularVelocity;
    }
}