using System.Numerics;
using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ColoredRay
	{
		public Ray Ray;
		public Vector4 Color;

		public ColoredRay(Ray ray, Vector4 color)
		{
			Ray = ray;
			Color = color;
		}
	}
}
