using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CrystalRay
{
	// ⚠️ AVX required
	[StructLayout(LayoutKind.Sequential, Size = 32)]
	public struct Vector4 : IEquatable<Vector4>
	{
		public static readonly Vector4 Zero = new Vector4();

		private readonly Vector256<double> _xyzw;

		public double X => _xyzw.GetElement(0);
		public double Y => _xyzw.GetElement(1);
		public double Z => _xyzw.GetElement(2);
		public double W => _xyzw.GetElement(3);

		private Vector4(Vector256<double> xyzw)
			=> _xyzw = xyzw;

		public Vector4(Vector3 v)
			: this(v, 1f) { }

		public Vector4(Vector3 v, double w)
			: this(Vector256.Create(v._xy, Vector128.Create(v.Z, w))) { }

		public Vector4(double x, double y, double z, double w)
			: this(Vector256.Create(x, y, z, w)) { }

		#region Operators

		public static Vector4 operator +(Vector4 v) => v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(Avx.Add(a._xyzw, b._xyzw));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 operator -(Vector4 v) => new Vector4(Avx.Xor(v._xyzw, Vector256.Create(long.MinValue).AsDouble()));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(Avx.Subtract(a._xyzw, b._xyzw));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 operator *(Vector4 a, Vector4 b) => new Vector4(Avx.Multiply(a._xyzw, b._xyzw));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 operator *(double a, Vector4 b) => new Vector4(Avx.Multiply(Vector256.Create(a), b._xyzw));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 operator *(Vector4 a, double b) => new Vector4(Avx.Multiply(a._xyzw, Vector256.Create(b)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 operator /(Vector4 a, double b) => new Vector4(Avx.Divide(a._xyzw, Vector256.Create(b)));

		public static explicit operator Vector3(Vector4 v) => new Vector3(v._xyzw.GetLower(), v._xyzw.GetElement(2));

		public static bool operator ==(Vector4 left, Vector4 right) => left.Equals(right);
		public static bool operator !=(Vector4 left, Vector4 right) => !(left == right);

		#endregion

		#region Instance Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Length()
			=> Math.Sqrt(LengthSquared());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double LengthSquared()
		{
			var product = Avx.Multiply(_xyzw, _xyzw);
			var partialSums = Sse2.Add(product.GetUpper(), product.GetLower());
			return partialSums.GetElement(0) + partialSums.GetElement(1);
		}

		#endregion

		#region Static Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 Normalize(Vector4 v)
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
		public static double DotProduct(Vector4 a, Vector4 b)
		{
			var product = Avx.Multiply(a._xyzw, b._xyzw);
			var partialSums = Sse2.Add(product.GetUpper(), product.GetLower());
			return partialSums.GetElement(0) + partialSums.GetElement(1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 Lerp(double f, Vector4 a, Vector4 b)
		{
			var fv = Vector256.Create(f);
			var ifv = Avx.Subtract(Vector256.Create(1d), fv);
			return new Vector4(Avx.Add(Avx.Multiply(fv, a._xyzw), Avx.Multiply(ifv, b._xyzw)));
		}

		#endregion

		public override bool Equals(object obj) => obj is Vector4 vector && Equals(vector);

		public bool Equals(Vector4 other)
		{
			var r = Avx.Xor(_xyzw, other._xyzw).AsUInt64();
			return Avx.TestZ(r, r);
		}

		public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

		public override string ToString()
			=> $"{{ X = {X.ToString()}; Y = {Y.ToString()}; Z = {Z.ToString()}; W = {W.ToString()} }}";
	}
}
