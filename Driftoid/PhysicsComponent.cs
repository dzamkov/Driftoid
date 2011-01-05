using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// An interface to a circular physical object in a physical location within a world.
    /// </summary>
    public sealed class PhysicsComponent : Component
    {
        public PhysicsComponent(double Mass, double Radius, MotionState MotionState)
        {
            this._Mass = Mass;
            this._Radius = Radius;
            this._MotionState = MotionState;
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
        /// Responds (by correcting positions and velocities) to the collision of two driftoids.
        /// </summary>
        internal static void _CollisionResponse(PhysicsComponent A, PhysicsComponent B, Vector Difference, double Distance, double Factor)
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

        private double _Mass;
        private double _Radius;
        private MotionState _MotionState;
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
}