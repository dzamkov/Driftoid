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
        /// Gets the square of the length of this vector. This function is quicker to compute than the actual length
        /// because it avoids a square root, which may be costly.
        /// </summary>
        public double SquareLength
        {
            get
            {
                return this.X * this.X + this.Y * this.Y;
            }
        }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(this.SquareLength);
            }
        }

        /// <summary>
        /// Creates a unit vector for the specified angle.
        /// </summary>
        public static Vector Unit(double Angle)
        {
            return new Vector(Math.Sin(Angle), Math.Cos(Angle));
        }

        /// <summary>
        /// Gets the angle of this vector.
        /// </summary>
        public double Angle
        {
            get
            {
                return Math.Atan2(this.Y, this.X);
            }
        }

        /// <summary>
        /// Gets if the given angle is an interior angle of the two specified angles. All angles must be
        /// normalized.
        /// </summary>
        public static bool AngleBetween(double Angle, double LowAngle, double HighAngle)
        {
            if (LowAngle < HighAngle && Angle >= LowAngle && Angle <= HighAngle)
            {
                return true;
            }
            if (LowAngle > HighAngle && (Angle >= LowAngle || Angle <= HighAngle))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the dot product of two vectors.
        /// </summary>
        public static double Dot(Vector A, Vector B)
        {
            return A.X * B.X + A.Y * B.Y;
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
            return new Vector(A.X - B.X, A.Y - B.Y);
        }

        public static Vector operator -(Vector A)
        {
            return new Vector(-A.X, -A.Y);
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

    /// <summary>
    /// A 2d matrix that specifies a scale/rotation/skew transformation.
    /// </summary>
    public struct AfflineMatrix
    {
        public AfflineMatrix(
            double M11, double M21,
            double M12, double M22)
        {
            this.M11 = M11;
            this.M21 = M21;
            this.M12 = M12;
            this.M22 = M22;
        }

        /// <summary>
        /// Creates a rotation matrix with the specified angle.
        /// </summary>
        public static AfflineMatrix Rotation(double Angle)
        {
            double cosang = Math.Cos(Angle);
            double sinang = Math.Sin(Angle);
            return new AfflineMatrix(
                cosang, -sinang,
                sinang, cosang);
        }

        public static Vector operator *(AfflineMatrix Matrix, Vector Vector)
        {
            return new Vector(
                Matrix.M11 * Vector.X + Matrix.M21 * Vector.Y,
                Matrix.M12 * Vector.X + Matrix.M22 * Vector.Y);
        }

        public static AfflineMatrix operator *(AfflineMatrix Matrix, double Amount)
        {
            return new AfflineMatrix(
                Matrix.M11 * Amount, Matrix.M21 * Amount,
                Matrix.M12 * Amount, Matrix.M22 * Amount);
        }

        public double M11;
        public double M21;
        public double M12;
        public double M22;
    }
}