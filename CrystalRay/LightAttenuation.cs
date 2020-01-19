using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct LightAttenuation
	{
		public readonly float Constant, Linear, Quadratic;

		public LightAttenuation(float constant, float linear, float quadratic)
		{
			Constant = constant;
			Linear = linear;
			Quadratic = quadratic;
		}
	}
}
