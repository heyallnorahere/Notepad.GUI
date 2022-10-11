/*
   Copyright 2022 Nora Beda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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
