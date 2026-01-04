using System;
using System.Numerics;

namespace TargetArrow
{
    static class VectorMath
    {
        public static Vector2 Rotate(Vector2 point, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            return new Vector2(
                point.X * cos - point.Y * sin,
                point.X * sin + point.Y * cos
            );
        }

        public static Vector2[] RotateQuad(
           Vector2 center,
           Vector2 size,
           float radians
       )
        {
            Vector2 half = size / 2;

            // Local corners relative to center
            Vector2[] corners =
            {
                new(-half.X, -half.Y), // top-left
                new( half.X, -half.Y), // top-right
                new( half.X,  half.Y), // bottom-right
                new(-half.X,  half.Y), // bottom-left
            };

            for (int i = 0; i < corners.Length; i++)
                corners[i] = Rotate(corners[i], radians) + center;

            return corners;
        }

        public static float SignedAngle(Vector2 a, Vector2 b)
        {
            var l = a.X * b.Y - a.Y * b.X;
            var r = a.X * b.X + a.Y * b.Y;
            return MathF.Atan2(l, r);

        }
    }
}

