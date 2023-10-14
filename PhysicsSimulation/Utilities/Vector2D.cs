using System;
using System.Windows;

namespace PhysicsSimulation.Utilities
{
    public class Vector2D
    {
        #region Properties

        public float X { get; protected set; }

        public float Y { get; protected set; }

        public float Length { get { return FastSquareRoot.Sqrt(LengthSquared); } }

        public float LengthSquared { get { return (X * X) + (Y * Y); } }

        #endregion

        #region Constructor

        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        #endregion

        #region Methods

        public static bool AreEqual(Vector2D v1, Vector2D v2)
        {
            var xEqual = Math.Abs(v1.X - v2.X) < Double.Epsilon;
            var yEqual = Math.Abs(v1.Y - v2.Y) < Double.Epsilon;

            return xEqual && yEqual;
        }

        public static Vector2D NormalizedCopy(Vector2D v)
        {
            if (Math.Abs(v.LengthSquared) < Double.Epsilon)
                return new Vector2D(0, 0);
            
            return v / v.LengthSquared;
        }

        public static Vector2D[] ComputeIntersection(Vector2D start, Vector2D end, Vector2D center, float radius, out int numSolutions, out int nearestSolution)
        {
            var intersections = new Vector2D[2];
            
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            var a = dx * dx + dy * dy;
            var b = 2 * (dx * (start.X - center.X) + dy * (start.Y - center.Y));
            var c = (start.X - center.X) * (start.X - center.X) + (start.Y - center.Y) * (start.Y - center.Y) - radius * radius;

            var det = b * b - 4 * a * c;
            if ((a <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersections[0] = new Vector2D(float.NaN, float.NaN);
                intersections[1] = new Vector2D(float.NaN, float.NaN);
                numSolutions = 0;
                nearestSolution = -1;

            }
            else if (Math.Abs(det) < Double.Epsilon)
            {
                // One solution.
                var t = -b / (2 * a);
                intersections[0] = new Vector2D(start.X + t * dx, start.Y + t * dy);
                intersections[1] = new Vector2D(float.NaN, float.NaN);
                numSolutions = 1;
                nearestSolution = 0;
            }
            else
            {
                // Two solutions.
                var t1 = ((-b + FastSquareRoot.Sqrt(det)) / (2 * a));
                intersections[0] = new Vector2D(start.X + t1 * dx, start.Y + t1 * dy);
                var t2 = ((-b - FastSquareRoot.Sqrt(det)) / (2 * a));
                intersections[1] = new Vector2D(start.X + t2 * dx, start.Y + t2 * dy);
                numSolutions = 2;
                nearestSolution = t1 < t2 ? 0 : 1;

            }
            
            return intersections;
        }

        public Vector2D NormalizedCopy()
        {
            if (Math.Abs(LengthSquared) < float.Epsilon)
                return new Vector2D(0, 0);
            return this / LengthSquared;
        }

        public Vector2D ClampToBounds(float minX, float minY, float maxX, float maxY)
        {
            var clampX = (X - float.Epsilon < minX) ? minX : ((X + float.Epsilon > maxX) ? maxX : X);
            var clampY = (Y - float.Epsilon < minY) ? minY : ((Y + float.Epsilon > maxY) ? maxY : Y);
            var result = new Vector2D(clampX, clampY);
            return result;
        }

        /// <summary>
        /// returns the clamped vector which is not larger than specified by <param name="maxLength">maxLength</param>.
        /// </summary>
        /// <param name="original">Vector to be clamped.</param>
        /// <param name="maxLength">maximum length of the vector. If vector length exceeds this value, it is shortened to match this length.</param>
        /// <param name="useLengthSquared">if true, all computations are done with LengthSquared (due to performance reasons). <param name="maxLength">maxLength</param> is treated as squared length !</param>
        /// <returns></returns>
        public static Vector2D ClampToMaxLength(Vector2D original, float maxLength, bool useLengthSquared = false)
        {
            var clampedVec = original;
            var length = useLengthSquared ? original.LengthSquared : original.Length;
            var lengthLimit = useLengthSquared ? maxLength*maxLength : maxLength;

            if (length > lengthLimit)
            {
                clampedVec = original.NormalizedCopy()*maxLength;
            }

            return clampedVec;
        }

        /// <summary>
        /// returns the clamped vector which is not larger than specified by <param name="maxLength">maxLength</param>.
        /// </summary>
        /// <param name="maxLength">maximum length of the vector. If vector length exceeds this value, it is shortened to match this length.</param>
        /// <param name="useLengthSquared">if true, all computations are done with LengthSquared (due to performance reasons). <param name="maxLength">maxLength</param> is treated as squared length !</param>
        /// <returns></returns>
        public Vector2D ClampToMaxLength(float maxLength, bool useLengthSquared = false)
        {
            return ClampToMaxLength(this, maxLength, useLengthSquared);
        }

        public override string ToString()
        {
            return "[ " + X + " | " + Y + " ]";
        }

        #endregion

        #region Operator overloading

        public static Vector2D operator -(Vector2D v)
        {
            return new Vector2D(-v.X, -v.Y);
        }

        public static Vector2D operator +(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X + w.X, v.Y + w.Y);
        }

        public static Vector2D operator -(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X - w.X, v.Y - w.Y);
        }

        public static Vector2D operator *(float s, Vector2D v)
        {
            return new Vector2D(s * v.X, s * v.Y);
        }

        public static Vector2D operator *(Vector2D v, float s)
        {
            return s * v;
        }

        public static double operator *(Vector2D v, Vector2D w)
        {
            return (v.X * w.X) + (v.Y * w.Y);
        }

        public static Vector2D operator /(Vector2D v, float s)
        {
            if (Math.Abs(s) < Double.Epsilon)
                return new Vector2D(0, 0);
            return new Vector2D(v.X / s, v.Y / s);
        }

        #endregion

        #region Methods for Conversion

        public static Point CreatePointFromVector(Vector2D src)
        {
            return new Point(src.X, src.Y);
        }

        #endregion

    }
}
