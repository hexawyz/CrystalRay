using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3 : IEquatable<Vector3>
	{
		public static readonly Vector3 Zero = new Vector3();

		internal readonly Vector128<double> _xy;
		private readonly double _z;

		public double X => _xy.GetElement(0);
		public double Y => _xy.GetElement(1);
		public double Z => _z;

		internal Vector3(Vector128<double> xy, double z)
			=> (_xy, _z) = (xy, z);

		public Vector3(Vector2 v)
			: this(v, 1f) { }

		public Vector3(Vector2 v, double z)
			: this(v._xy, z) { }

		public Vector3(double x, double y, double z)
			: this(Vector128.Create(x, y), z) { }

		private Vector256<double> Xyz => Vector256.Create(_xy, Vector128.CreateScalarUnsafe(_z));

		#region Operators

		public static Vector3 operator +(Vector3 v) => v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(Sse2.Add(a._xy, b._xy), a._z + b._z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator -(Vector3 v) => new Vector3(Sse2.Xor(v._xy.AsInt64(), Vector128.Create(long.MinValue)).AsDouble(), -v._z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(Sse2.Subtract(a._xy, b._xy), a._z - b._z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(Sse2.Multiply(a._xy, b._xy), a._z * b._z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(double a, Vector3 b) => new Vector3(Sse2.Multiply(Vector128.Create(a), b._xy), a * b._z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(Vector3 a, double b) => new Vector3(Sse2.Multiply(a._xy, Vector128.Create(b)), a._z * b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator /(Vector3 a, double b) => new Vector3(Sse2.Divide(a._xy, Vector128.Create(b)), a._z / b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Vector2(Vector3 v) => new Vector2(v._xy);

		public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
		public static bool operator !=(Vector3 left, Vector3 right) => !(left == right);

		#endregion

		#region Instance Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Length()
			=> Math.Sqrt(LengthSquared());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double LengthSquared()
			=> DotProduct(this, this);

		#endregion

		#region Static Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Normalize(Vector3 v)
		{
			double l = v.LengthSquared();

			if (l != 0)
			{
				return v / Math.Sqrt(l);
			}
			else
			{
				return v;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double DotProduct(Vector3 a, Vector3 b)
		{
			var xy = Sse2.Multiply(a._xy, b._xy);
			return xy.GetElement(0) + xy.GetElement(1) + a._z * b._z;
		}

		public static Vector3 CrossProduct(Vector3 a, Vector3 b)
			=> new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Lerp(double f, Vector3 a, Vector3 b)
		{
			double fi = 1 - f;
			return new Vector3(Sse2.Add(Sse2.Multiply(Vector128.Create(f), a._xy), Sse2.Multiply(Vector128.Create(fi), b._xy)), f * a._z + fi * b._z);
		}

		#endregion

		public override bool Equals(object obj) => obj is Vector3 vector && Equals(vector);
		public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;
		public override int GetHashCode() => HashCode.Combine(X, Y, Z);

		public override string ToString() => $"{{ X = {X.ToString()}; Y = {Y.ToString()}; Z = {Z.ToString()} }}";
	}
}
