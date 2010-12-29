using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace Driftoid
{
    /// <summary>
    /// A two-dimensional floating point vector.
    /// </summary>
    public struct Vector
    {
        public Vector(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Creates a unit vector for the specified angle.
        /// </summary>
        public static Vector Unit(double Angle)
        {
            return new Vector(Math.Sin(Angle), Math.Cos(Angle));
        }

        public static implicit operator Vector2(Vector Vector)
        {
            return new Vector2((float)Vector.X, (float)Vector.Y);
        }

        public static implicit operator PointF(Vector Vector)
        {
            return new PointF((float)Vector.X, (float)Vector.Y);
        }

        public static Vector operator -(Vector A, Vector B)
        {
            return new Vector(A.X + B.X, A.Y + B.Y);
        }

        public static Vector operator +(Vector A, Vector B)
        {
            return new Vector(A.X + B.X, A.Y + B.Y);
        }

        public static Vector operator *(Vector A, double B)
        {
            return new Vector(A.X * B, A.Y * B);
        }

        public double X;
        public double Y;
    }
}