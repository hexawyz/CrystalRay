using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CrystalRay
{
	// ⚠️ AVX required
	[DebuggerDisplay("{_xyzw}")]
	[StructLayout(LayoutKind.Sequential, Size = 32)]
	public readonly struct Quaternion
	{
		public static readonly Quaternion Zero;

		internal readonly Vector256<double> _xyzw;

		public double X => _xyzw.GetElement(0);
		public double Y => _xyzw.GetElement(1);
		public double Z => _xyzw.GetElement(2);
		public double W => _xyzw.GetElement(3);

		private Quaternion(Vector256<double> xyzw)
			=> _xyzw = xyzw;

		public Quaternion(double x, double y, double z, double w)
			: this(Vector256.Create(x, y, z, w)) { }

		public Quaternion(Vector4 vector4)
			=> _xyzw = vector4._xyzw;

		public static Quaternion RotateX(double θ)
		{
			double angle = 0.5 * θ;
			var (cos, sin) = (Math.Cos(angle), Math.Sin(angle));

			return new Quaternion(Vector256.Create(sin, 0, 0, cos));
		}

		public static Quaternion RotateY(double θ)
		{
			double angle = 0.5 * θ;
			var (cos, sin) = (Math.Cos(angle), Math.Sin(angle));

			return new Quaternion(Vector256.Create(0, sin, 0, cos));
		}

		public static Quaternion RotateZ(double θ)
		{
			double angle = 0.5 * θ;
			var (cos, sin) = (Math.Cos(angle), Math.Sin(angle));

			return new Quaternion(Vector256.Create(0, 0, sin, cos));
		}

		public static Quaternion Rotate(Vector3 v, double θ)
		{
			double angle = 0.5 * θ;
			var (cos, sin) = (Math.Cos(angle), Math.Sin(angle));

			return new Quaternion(Vector256.Create(Sse2.Multiply(Vector128.Create(sin), v._xy), Vector128.Create(sin * v.Z, cos)));
		}

		public Quaternion Conjugate()
			=> new Quaternion(Avx.Xor(_xyzw, Vector256.Create(long.MinValue).AsDouble().WithElement(3, 0)));

		public static Quaternion operator *(Quaternion a, Quaternion b)
		{
			// Formula:
			// wx + xw + yz - zy
			// wy - xz + yw + zx
			// wz + xy - yx + zw
			// ww - xx - yy - zz

			// Current implementation: Calculate the vectors for each column,
			// then sum them up with a series of vector add/sub, using appropriate permutations.
			// AddSubtract(<0, 0, 0, 0>, <1, 1, 1, 1>) = <-1, 1, -1, 1>

			// I sincerely hope it is possible to do better using AVX, but this will do for now.

			var c = Avx.Permute2x128(b._xyzw, b._xyzw, 1);
			var x = Avx.Permute(Avx.Multiply(Vector256.Create(a.X), c), 0b_0101);
			var y = Avx.Multiply(Vector256.Create(a.Y), c);
			var z = Avx.Permute(Avx.Multiply(Vector256.Create(a.Z), b._xyzw), 0b_0101);
			var w = Avx.Multiply(Vector256.Create(a.W), b._xyzw);

			return new Quaternion
			(
				Avx.Permute
				(
					Avx.AddSubtract
					(
						Avx.Permute(w, 0b_0101),
						Avx.Permute
						(
							Avx.AddSubtract
							(
								Avx.Permute(x, 0b_1001),
								Avx.Permute
								(
									Avx.AddSubtract(y, z),
									0b_1001
								)
							),
							0b_0110
						)
					),
					0b0101
				)
			);
		}
	}
}
