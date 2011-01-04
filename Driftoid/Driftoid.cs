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
        public Driftoid(MotionState MotionState, double Mass, double Radius)
        {
            this._MotionState = MotionState;
            this._Mass = Mass;
            this._Radius = Radius;
        }

        private class _MaskDrawer : Drawer
        {
            public override Color AtPoint(Vector Point)
            {
                double dis = (Point - new Vector(0.5, 0.5)).Length;
                if (dis <= 0.5)
                {
                    return Color.RGB(1.0, 1.0, 1.0);
                }
                return Color.RGBA(1.0, 1.0, 1.0, 0.0);
            }
        }

        /// <summary>
        /// A texture for a circular mask (using alpha values) in the shape of driftoids.
        /// </summary>
        public static readonly Texture Mask = Texture.Define(new _MaskDrawer(), 1.0, 0.2);

        /// <summary>
        /// Creates a driftoid using a driftoid constructor.
        /// </summary>
        public static Driftoid Make(DriftoidConstructor Constructor, MotionState MotionState)
        {
            return Constructor.Construct(MotionState, Constructor.Mass, Constructor.Radius);
        }

        /// <summary>
        /// Creates a driftoid at the specified position.
        /// </summary>
        public static Driftoid Make(DriftoidConstructor Constructor, Vector Position)
        {
            return Make(Constructor, new MotionState(Position));
        }

        /// <summary>
        /// Gets a static visual which can be used to draw the driftoid.
        /// </summary>
        public virtual Visual Visual
        {
            get
            {
                return null;
            }
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
        /// Gets the angle of the driftoid.
        /// </summary>
        public double Angle
        {
            get
            {
                return this._MotionState.Angle;
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
        public DriftoidConstructor(ConstructFunc Construct, double Mass, double Radius)
        {
            this.Construct = Construct;
            this.Radius = Radius;
            this.Mass = Mass;
        }

        /// <summary>
        /// A function that can create a driftoid of a certain type given its motion state.
        /// </summary>
        public delegate Driftoid ConstructFunc(MotionState MotionState, double Mass, double Radius);

        public ConstructFunc Construct;
        public double Radius = 1.0;
        public double Mass = 1.0;
    }
}