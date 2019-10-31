using System;
using System.Runtime.CompilerServices;

namespace RaytracingInOneWeekend
{
    // In C#, we cannot create a performance Vec3<T> where T could be int, long,
    // float, double. Using where to constrain T by design has no "where numeric
    // type". It's still possible to create such Vec3<T>, but dispatch based on
    // type must happen runtime. For some types this is okay, but not for a Vec3
    // would sit on the hot path.
    //
    // To avoid heap allocation, Vec3f is implement as a struct instead of a
    // class. That way allocations happen on the stack instead.
    struct Vec3f
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public float R => X;
        public float G => Y;
        public float B => Z;

        public Vec3f(float e0, float e1, float e2)
        {
            X = e0;
            Y = e1;
            Z = e2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float SquaredLength() => X * X + Y * Y + Z * Z;

        // In C#, overloading a binary operator automatically overloads its
        // compound equivalent, i.e., we get both Vec3f + Vec3f and Vec3f +=
        // Vec3f with a single overload.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f operator+(Vec3f v1, Vec3f v2) => new Vec3f(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f operator-(Vec3f v1, Vec3f v2) => new Vec3f(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f operator*(Vec3f v1, Vec3f v2) => new Vec3f(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f operator/(Vec3f v1, Vec3f v2) => new Vec3f(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f operator*(Vec3f v, float t) => new Vec3f(v.X * t, v.Y * t, v.Z * t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f operator*(float t, Vec3f v) => new Vec3f(v.X * t, v.Y * t, v.Z * t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f operator/(Vec3f v, float t) => new Vec3f(v.X / t, v.Y / t, v.Z / t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f UnitVector(Vec3f v) => v / v.Length();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vec3f v1, Vec3f v2) => v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3f Cross(Vec3f v1, Vec3f v2) =>
            new Vec3f(v1.Y * v2.Z - v1.Z * v2.Y,
                      v1.Z * v2.X - v1.X * v2.Z,
                      v1.X * v2.Y - v1.Y * v2.X);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3f Add(Vec3f v)
        {
            X += v.X;
            Y += v.Y;
            Z += v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3f Sub(Vec3f v)
        {
            X -= v.X;
            Y -= v.Y;
            Z -= v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3f Mul(Vec3f v)
        {
            X *= v.X;
            Y *= v.Y;
            Z *= v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3f Div(Vec3f v)
        {
            X /= v.X;
            Y /= v.Y;
            Z /= v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3f Mul(float t)
        {
            X *= t;
            Y *= t;
            Z *= t;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3f Div(float t)
        {
            X *= t;
            Y *= t;
            Z *= t;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeUnitVector()
        {
            var k = 1f / Length();
            X *= k;
            Y *= k;
            Z *= k;
        }
    }
}