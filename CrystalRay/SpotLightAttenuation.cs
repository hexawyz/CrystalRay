using System;
using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct SpotLightAttenuation
	{
		public readonly float Constant;
		public readonly float Linear;
		public readonly float Quadratic;
		public readonly float Phi;
		public readonly float Theta;
		public readonly float Falloff;

		public SpotLightAttenuation(float constant, float linear, float quadratic, float phi, float theta, float falloff)
		{
			if (theta < 0 || theta > MathF.PI)
				throw new ArgumentOutOfRangeException(nameof(theta));
			if (phi < theta || phi > MathF.PI)
				throw new ArgumentOutOfRangeException(nameof(phi));
			Constant = constant;
			Linear = linear;
			Quadratic = quadratic;
			Phi = phi;
			Theta = theta;
			Falloff = falloff;
		}
	}
}
