using System;
using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2
	{
		public static readonly Vector2 Empty = new Vector2();

		public double X, Y;

		public Vector2(double x, double y)
		{
			X = x;
			Y = y;
		}

		#region Operators

		public static Vector2 operator +(Vector2 v)
		{
			return v;
		}

		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X + b.X, a.Y + b.Y);
		}

		public static Vector2 operator -(Vector2 v)
		{
			return new Vector2(-v.X, -v.Y);
		}

		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X - b.X, a.Y - b.Y);
		}

		public static Vector2 operator *(double a, Vector2 b)
		{
			return new Vector2(a * b.X, a * b.Y);
		}

		public static Vector2 operator *(Vector2 a, double b)
		{
			return new Vector2(a.X * b, a.Y * b);
		}

		public static Vector2 operator /(Vector2 a, double b)
		{
			return new Vector2(a.X / b, a.Y / b);
		}

		public static bool operator ==(Vector2 a, Vector2 b)
		{
			return (a.X == b.X) && (a.Y == b.Y);
		}

		public static bool operator !=(Vector2 a, Vector2 b)
		{
			return (a.X != b.X) || (a.Y != b.Y);
		}

		#endregion

		#region Instance Methods

		public double Length()
		{
			return (double)Math.Sqrt(X * X + Y * Y);
		}

		public double LengthSquarred()
		{
			return X * X + Y * Y;
		}

		public void Normalize()
		{
			double l = X * X + Y * Y;

			if (l != 0)
			{
				l = (double)(1 / Math.Sqrt(l));
				X *= l;
				Y *= l;
			}
		}

		#endregion

		#region Static Methods

		public static Vector2 Normalize(Vector2 v)
		{
			double l = v.X * v.X + v.Y * v.Y;

			if (l != 0)
			{
				l = (double)(1 / Math.Sqrt(l));
				return l * v;
			}
			else
				return v;
		}

		public static double DotProduct(Vector2 a, Vector2 b)
		{
			return a.X * b.X + a.Y * b.Y;
		}

		public static Vector2 Lerp(double f, Vector2 a, Vector2 b)
		{
			double fi = 1 - f;

			return new Vector2(f * a.X + fi * b.X, f * a.Y + fi * b.Y);
		}

		public static Vector2 Lerp(double f, double g, Vector2 a, Vector2 b)
		{
			return new Vector2(f * a.X + (1 - f) * b.X, g * a.Y + (1 - g) * b.Y);
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (obj is Vector2)
			{
				Vector2 v = (Vector2)obj;

				return (v.X == X) && (v.Y == Y);
			}
			else
				return false;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public override string ToString()
		{
			return "{ X = " + X.ToString() + "; Y = " + Y.ToString() + " }";
		}
	}
}
