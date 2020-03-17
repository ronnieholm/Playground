using System;
using System.Runtime.CompilerServices;

namespace Graphics.Engine
{
    // In C#, we cannot create a performance Vec3<T> where T could be int, long,
    // double, double. Using where to constrain T by design has no "where numeric
    // type". It's still possible to create such Vec3<T>, but dispatch based on
    // type must happen runtime. For some types this is okay, but not for a Vec3
    // would sit on the hot path.
    //
    // To work around this issue, we could generate the Vector using T4:
    // https://www.youtube.com/watch?v=H4PGnWIytLw and
    // https://www.youtube.com/watch?v=FBl8Eaa3GX8
    //
    // To avoid heap allocation, Vec3f is implement as a struct instead of a
    // class. That way allocations happen on the stack instead.
    struct Vec3d
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        public double R => X;
        public double G => Y;
        public double B => Z;

        public Vec3d(double e0, double e1, double e2)
        {
            X = e0;
            Y = e1;
            Z = e2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double SquaredLength() => X * X + Y * Y + Z * Z;

        // In C#, overloading a binary operator automatically overloads its
        // compound equivalent, i.e., we get both Vec3f + Vec3f and Vec3f +=
        // Vec3f with a single overload.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator+(Vec3d v1, Vec3d v2) => new Vec3d(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator-(Vec3d v1, Vec3d v2) => new Vec3d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator*(Vec3d v1, Vec3d v2) => new Vec3d(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator/(Vec3d v1, Vec3d v2) => new Vec3d(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator*(Vec3d v, double t) => new Vec3d(v.X * t, v.Y * t, v.Z * t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator*(double t, Vec3d v) => new Vec3d(v.X * t, v.Y * t, v.Z * t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator/(Vec3d v, double t) => new Vec3d(v.X / t, v.Y / t, v.Z / t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d UnitVector(Vec3d v) => v / v.Length();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vec3d v1, Vec3d v2) => v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d Cross(Vec3d v1, Vec3d v2) =>
            new Vec3d(v1.Y * v2.Z - v1.Z * v2.Y,
                      v1.Z * v2.X - v1.X * v2.Z,
                      v1.X * v2.Y - v1.Y * v2.X);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Add(Vec3d v)
        {
            X += v.X;
            Y += v.Y;
            Z += v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Sub(Vec3d v)
        {
            X -= v.X;
            Y -= v.Y;
            Z -= v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Mul(Vec3d v)
        {
            X *= v.X;
            Y *= v.Y;
            Z *= v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Div(Vec3d v)
        {
            X /= v.X;
            Y /= v.Y;
            Z /= v.Z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Mul(double t)
        {
            X *= t;
            Y *= t;
            Z *= t;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Div(double t)
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