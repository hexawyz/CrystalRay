using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2 : IEquatable<Vector2>
	{
		public static readonly Vector2 Zero = new Vector2();

		internal readonly Vector128<double> _xy;

		public double X => _xy.GetElement(0);
		public double Y => _xy.GetElement(1);

		internal Vector2(Vector128<double> xy) => _xy = xy;

		public Vector2(double x, double y) : this(Vector128.Create(x, y)) { }

		#region Operators

		public static Vector2 operator +(Vector2 v) => v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(Sse2.Add(a._xy, b._xy));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator -(Vector2 v) => new Vector2(Sse2.Xor(v._xy.AsInt64(), Vector128.Create(long.MinValue)).AsDouble());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(Sse2.Subtract(a._xy, b._xy));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(Sse2.Multiply(a._xy, b._xy));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator *(double a, Vector2 b) => new Vector2(Sse2.Multiply(Vector128.Create(a), b._xy));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator *(Vector2 a, double b) => new Vector2(Sse2.Multiply(a._xy, Vector128.Create(b)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator /(Vector2 a, double b) => new Vector2(Sse2.Divide(a._xy, Vector128.Create(b)));

		public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
		public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);

		#endregion

		#region Instance Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Length()
			=> Math.Sqrt(LengthSquared());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double LengthSquared()
			=> Sse41.DotProduct(_xy, _xy, 0b_11_00_01).ToScalar();

		#endregion

		#region Static Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Normalize(Vector2 v)
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
		public static double DotProduct(Vector2 a, Vector2 b)
			=> Sse41.DotProduct(a._xy, b._xy, 0b_11_00_01).ToScalar();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Lerp(double f, Vector2 a, Vector2 b)
		{
			var fv = Vector128.Create(f);
			var ifv = Sse2.Subtract(Vector128.Create(1d), fv);
			return new Vector2(Sse2.Add(Sse2.Multiply(fv, a._xy), Sse2.Multiply(ifv, b._xy)));
		}

		#endregion

		public override bool Equals(object obj) => obj is Vector2 vector && Equals(vector);

		public bool Equals(Vector2 other)
		{
			var r = Sse2.Xor(_xy, other._xy).AsUInt64();
			return Sse41.TestZ(r, r);
		}

		public override int GetHashCode() => HashCode.Combine(X, Y);

		public override string ToString()
			=> $"{{ X = {X.ToString()}; Y = {Y.ToString()} }}";
	}
}
