using System;
using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3
	{
		public static readonly Vector3 Empty = new Vector3();

		public double X, Y, Z;

		public Vector3(Vector2 v)
		{
			X = v.X;
			Y = v.Y;
			Z = 0.0f;
		}

		public Vector3(Vector2 v, double z)
		{
			X = v.X;
			Y = v.Y;
			Z = z;
		}

		public Vector3(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		#region Operators

		public static Vector3 operator +(Vector3 v)
		{
			return v;
		}

		public static Vector3 operator +(Vector3 a, Vector3 b)
		{
			return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vector3 operator -(Vector3 v)
		{
			return new Vector3(-v.X, -v.Y, -v.Z);
		}

		public static Vector3 operator -(Vector3 a, Vector3 b)
		{
			return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vector3 operator *(Vector3 a, Vector3 b)
		{
			return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}

		public static Vector3 operator *(double a, Vector3 b)
		{
			return new Vector3(a * b.X, a * b.Y, a * b.Z);
		}

		public static Vector3 operator *(Vector3 a, double b)
		{
			return new Vector3(a.X * b, a.Y * b, a.Z * b);
		}

		public static Vector3 operator /(Vector3 a, double b)
		{
			return new Vector3(a.X / b, a.Y / b, a.Z / b);
		}

		public static explicit operator Vector2(Vector3 v)
		{
			return new Vector2(v.X, v.Y);
		}

		public static bool operator ==(Vector3 a, Vector3 b)
		{
			return (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z);
		}

		public static bool operator !=(Vector3 a, Vector3 b)
		{
			return (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z);
		}

		#endregion

		#region Instance Methods

		public double Length()
		{
			return (double)Math.Sqrt(X * X + Y * Y + Z * Z);
		}

		public double LengthSquarred()
		{
			return X * X + Y * Y + Z * Z;
		}

		public void Normalize()
		{
			double l = X * X + Y * Y + Z * Z;

			if (l != 0)
			{
				l = (double)(1 / Math.Sqrt(l));
				X *= l;
				Y *= l;
				Z *= l;
			}
		}

		#endregion

		#region Static Methods

		public static Vector3 Normalize(Vector3 v)
		{
			double l = v.X * v.X + v.Y * v.Y + v.Z * v.Z;

			if (l > 0)
			{
				l = (double)(1 / Math.Sqrt(l));
				return l * v;
			}
			else
				return v;
		}

		public static double DotProduct(Vector3 a, Vector3 b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}

		public static Vector3 CrossProduct(Vector3 a, Vector3 b)
		{
			return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		}

		public static Vector3 Lerp(double f, Vector3 a, Vector3 b)
		{
			double fi = 1 - f;

			return new Vector3(f * a.X + fi * b.X, f * a.Y + fi * b.Y, f * a.Z + fi * b.Z);
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (obj is Vector3)
			{
				Vector3 v = (Vector3)obj;

				return (v.X == X) && (v.Y == Y) && (v.Z == Z);
			}
			else
				return false;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
		}

		public override string ToString()
		{
			return "{ X = " + X.ToString() + "; Y = " + Y.ToString() + "; Z = " + Z.ToString() + " }";
		}
	}
}
