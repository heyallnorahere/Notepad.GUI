using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Notepad.GUI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2 : IEquatable<Vector2>
    {
        public Vector2()
        {
            X = Y = 0;
        }

        public Vector2(int scalar)
        {
            X = Y = scalar;
        }

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X, Y;

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is Vector2 vector)
            {
                return Equals(vector);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static implicit operator Vector2((int x, int y) tuple) => new(tuple.x, tuple.y);
        public static implicit operator (int, int)(Vector2 vector) => (vector.X, vector.Y);

        public static Vector2 operator +(Vector2 lhs, Vector2 rhs) => (lhs.X + rhs.X, lhs.Y + rhs.Y);
        public static Vector2 operator +(Vector2 lhs, int rhs) => lhs + (rhs, rhs);
        public static Vector2 operator +(int lhs, Vector2 rhs) => (lhs, lhs) + rhs;
        public static Vector2 operator -(Vector2 vector) => (-vector.X, -vector.Y);
        public static Vector2 operator -(Vector2 lhs, Vector2 rhs) => lhs + -rhs;
        public static Vector2 operator -(Vector2 lhs, int rhs) => lhs + -rhs;
        public static Vector2 operator -(int lhs, Vector2 rhs) => lhs + -rhs;

        public static bool operator ==(Vector2 lhs, Vector2 rhs) => lhs.Equals(rhs);
        public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

    }
}
