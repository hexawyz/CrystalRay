using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct LightAttenuation
	{
		public double Constant, Linear, Quadratic;

		public LightAttenuation(double constant, double linear, double quadratic)
		{
			Constant = constant;
			Linear = linear;
			Quadratic = quadratic;
		}
	}
}
