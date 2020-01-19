using System;
using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector4 : IEquatable<Vector4>
	{
		public static readonly Vector4 Empty = new Vector4();

		public double X, Y, Z, W;

		public Vector4(Vector3 v)
		{
			X = v.X;
			Y = v.Y;
			Z = v.Z;
			W = 1.0f;
		}

		public Vector4(Vector3 v, double w)
		{
			X = v.X;
			Y = v.Y;
			Z = v.Z;
			W = w;
		}

		public Vector4(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		#region Operators

		public static Vector4 operator +(Vector4 v) => v;
		public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
		public static Vector4 operator -(Vector4 v) => new Vector4(-v.X, -v.Y, -v.Z, -v.W);
		public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
		public static Vector4 operator *(Vector4 a, Vector4 b) => new Vector4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
		public static Vector4 operator *(double a, Vector4 b) => new Vector4(a * b.X, a * b.Y, a * b.Z, a * b.W);
		public static Vector4 operator *(Vector4 a, double b) => new Vector4(a.X * b, a.Y * b, a.Z * b, a.W * b);
		public static Vector4 operator /(Vector4 a, double b) => new Vector4(a.X / b, a.Y / b, a.Z / b, a.W / b);

		public static explicit operator Vector3(Vector4 v) => new Vector3(v.X, v.Y, v.Z);

		public static bool operator ==(Vector4 left, Vector4 right) => left.Equals(right);
		public static bool operator !=(Vector4 left, Vector4 right) => !(left == right);

		#endregion

		#region Instance Methods

		public double Length() => (double)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

		public double LengthSquarred() => X * X + Y * Y + Z * Z + W * W;

		public void Normalize()
		{
			double l = X * X + Y * Y + Z * Z + W * W;

			if (l != 0)
			{
				l = (double)(1 / Math.Sqrt(l));
				X *= l;
				Y *= l;
				Z *= l;
				W *= l;
			}
		}

		#endregion

		#region Static Methods

		public static Vector4 Normalize(Vector4 v)
		{
			double l = v.X * v.X + v.Y * v.Y + v.Z * v.Z + v.W * v.W;

			if (l != 0)
			{
				l = (double)(1 / Math.Sqrt(l));
				return l * v;
			}
			else
				return v;
		}

		public static double DotProduct(Vector4 a, Vector4 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

		public static Vector4 CrossProduct(Vector4 a, Vector4 b) => new Vector4(a.Y * b.Z - a.Z * b.Y, a.Z * b.W - a.W * b.Z, a.W * b.X - a.X * b.W, a.X * b.Y - a.Y * b.W);

		public static Vector4 Lerp(double f, Vector4 a, Vector4 b)
		{
			double fi = 1 - f;

			return new Vector4(f * a.X + fi * b.X, f * a.Y + fi * b.Y, f * a.Z + fi * b.Z, f * a.W + fi * b.W);
		}

		#endregion

		public override bool Equals(object obj) => obj is Vector4 vector && Equals(vector);
		public bool Equals(Vector4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

		public override string ToString()
			=> $"{{ X = {X.ToString()}; Y = {Y.ToString()}; Z = {Z.ToString()}; W = {W.ToString()} }}";
	}
}
