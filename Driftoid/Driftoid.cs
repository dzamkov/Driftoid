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
        public Driftoid(DriftoidConstructorInfo Info)
        {
            this._Radius = Info.Radius;
            this._MotionState = Info.MotionState;
            this._Kind = Info.Kind;
            this._Mass = Info.Mass;
        }

        /// <summary>
        /// Creates a driftoid using a driftoid constructor.
        /// </summary>
        public static LinkedDriftoid Make(DriftoidConstructor Constructor, MotionState MotionState)
        {
            return new LinkedDriftoid(new DriftoidConstructorInfo()
            {
                Kind = Constructor.Kind,
                Mass = Constructor.Mass,
                Radius = Constructor.Radius,
                MotionState = MotionState
            });
        }

        /// <summary>
        /// Creates a driftoid using a driftoid constructor.
        /// </summary>
        public static LinkedDriftoid Make(DriftoidConstructor Constructor, Vector Position)
        {
            return Make(Constructor, new MotionState(Position));
        }

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
        /// Draws a texture relative to this driftoid.
        /// </summary>
        public void DrawTexture(int TextureID, double Size, double Angle)
        {
            Texture.Bind2D(TextureID);
            GL.PushMatrix();
            GL.Translate(this._MotionState.Position.X, this._MotionState.Position.Y, 0.0);
            GL.Scale(this._Radius * Size, this._Radius * Size, 1.0);
            GL.Rotate(this._MotionState.Angle + Angle, 0.0, 0.0, 1.0);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(-1.0f, -1.0f); GL.TexCoord2(0f, 0f);
            GL.Vertex2(-1.0f, 1.0f); GL.TexCoord2(1f, 0f);
            GL.Vertex2(1.0f, 1.0f); GL.TexCoord2(1f, 1f);
            GL.Vertex2(1.0f, -1.0f); GL.TexCoord2(0f, 1f);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// A drawer for a solid driftoid.
        /// </summary>
        public class SolidDrawer : Drawer
        {
            public override Color AtPoint(Vector Point)
            {
                double dis = (Point - new Vector(0.5, 0.5)).Length;
                double trans = 0.5 - BorderSize;
                if (dis > 0.5)
                {
                    return Color.Transparent;
                }
                if (dis > trans)
                {
                    return this.BorderColor;
                }
                return this.InteriorColor;
            }

            public Color BorderColor;
            public Color InteriorColor;
            public double BorderSize;
        }

        /// <summary>
        /// Draws a blank circle to the specified graphics context.
        /// </summary>
        public static void DrawSolid(
            Bitmap Bitmap, double BorderSize, 
            Color BorderColor, Color InteriorColor)
        {
            new SolidDrawer()
            {
                BorderColor = BorderColor,
                InteriorColor = InteriorColor,
                BorderSize = BorderSize
            }.Draw(Bitmap);
        }

        /// <summary>
        /// Gets the state of the motion of the driftoid.
        /// </summary>
        public MotionState MotionState
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

        /// <summary>
        /// Gets the current position of the driftoid.
        /// </summary>
        public Vector Position
        {
            get
            {
                return this._MotionState.Position;
            }
        }

        /// <summary>
        /// Gets the mass of the driftoid.
        /// </summary>
        public double Mass
        {
            get
            {
                return this._Mass;
            }
        }

        /// <summary>
        /// Gets the kind of this driftoid.
        /// </summary>
        public Kind Kind
        {
            get
            {
                return this._Kind;
            }
        }

        /// <summary>
        /// Applies a force, in mass * unit/second^2, to the driftoid.
        /// </summary>
        internal void _ApplyForce(double Time, Vector Force)
        {
            this._MotionState.ApplyForce(Time, Force * (1.0 / this._Mass));
        }


        /// <summary>
        /// Directly applies a velocity.
        /// </summary>
        internal void _ApplyImpulse(Vector Impulse)
        {
            this._MotionState.Velocity += Impulse * (1.0 / this._Mass);
        }

        /// <summary>
        /// Responds to the collision of two driftoids.
        /// </summary>
        internal static void _CollisionResponse(Driftoid A, Driftoid B, Vector Difference, double Distance, double Factor)
        {
            double trad = A.Radius + B.Radius;
            Vector ncol = Difference * (1.0 / Difference.Length);
            double pen = trad - Distance;
            double ima = 1.0 / A._Mass;
            double imb = 1.0 / B._Mass;
            Vector sep = ncol * (pen / (ima + imb)) * Factor;
            A._MotionState.Position -= sep * ima;
            B._MotionState.Position += sep * imb;

            Vector vcol = B._MotionState.Velocity - A._MotionState.Velocity;
            double impact = Vector.Dot(vcol, ncol);
            if (impact < 0.0)
            {
                double cor = 0.5;
                double j = -(1.0f + cor) * (impact) / (ima + imb);
                Vector impulse = ncol * j;
                A._MotionState.Velocity -= impulse * ima;
                B._MotionState.Velocity += impulse * imb;
            }
        }

        internal double _Radius;
        internal double _Mass;
        internal MotionState _MotionState;
        internal Kind _Kind;
    }

    /// <summary>
    /// The physical/motion state of a driftoid.
    /// </summary>
    public struct MotionState
    {
        public MotionState(Vector Position)
        {
            this.Position = Position;
            this.Angle = 0.0;
            this.AngularVelocity = 0.0;
            this.Velocity = new Vector();
        }

        public MotionState(Vector Position, double Angle)
        {
            this.Position = Position;
            this.Angle = Angle;
            this.AngularVelocity = 0.0;
            this.Velocity = new Vector();
        }

        public MotionState(Vector Position, Vector Velocity, double Angle, double AngularVelocity)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Angle = Angle;
            this.AngularVelocity = AngularVelocity;
        }

        /// <summary>
        /// Updates the motion state.
        /// </summary>
        /// <param name="Time">Time in seconds to update.</param>
        /// <param name="LinearFriction">Factor between 0.0 and 1.0 that indicates how much velocity
        /// remains over a second.</param>
        /// <param name="AngularFriction">Factor between 0.0 and 1.0 that indicates how much angular velocity
        /// remains over a second.</param>
        public void Update(double Time, double LinearFriction, double AngularFriction)
        {
            this.Position += this.Velocity * Time;
            this.Angle += this.AngularVelocity * Time;
            this.Velocity *= Math.Pow(LinearFriction, Time);
            this.AngularVelocity *= Math.Pow(AngularFriction, Time);
        }

        /// <summary>
        /// Applies a force, in units/second^2.
        /// </summary>
        public void ApplyForce(double Time, Vector Force)
        {
            this.Velocity += Force * Time;
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

    /// <summary>
    /// Describes the initial properties of a driftoid, allowing the creation of one with any motion state.
    /// </summary>
    public class DriftoidConstructor
    {
        public DriftoidConstructor()
        {

        }

        public DriftoidConstructor(Kind Kind)
        {
            this.Kind = Kind;
        }

        public DriftoidConstructor(Kind Kind, double Radius, double Mass)
        {
            this.Kind = Kind;
            this.Radius = Radius;
            this.Mass = Mass;
        }

        public Kind Kind;
        public double Radius = 1.0;
        public double Mass = 1.0;
    }

    /// <summary>
    /// Parameter for construction of driftoids.
    /// </summary>
    public class DriftoidConstructorInfo
    {
        /// <summary>
        /// Initial motion state of the driftoid.
        /// </summary>
        public MotionState MotionState;

        /// <summary>
        /// The initial mass of the driftoid.
        /// </summary>
        public double Mass = 1.0;

        /// <summary>
        /// The initial radius of the driftoid.
        /// </summary>
        public double Radius = 1.0;

        /// <summary>
        /// The kind of this driftoid.
        /// </summary>
        public Kind Kind = null;
    }
}