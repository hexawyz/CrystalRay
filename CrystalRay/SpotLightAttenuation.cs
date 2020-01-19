using System;
using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SpotLightAttenuation
	{
		public double Constant, Linear, Quadratic;
		public double Phi, Theta, Falloff;

		public SpotLightAttenuation(double constant, double linear, double quadratic, double phi, double theta, double falloff)
		{
			if (theta < 0 || theta > Math.PI)
				throw new ArgumentOutOfRangeException("theta");
			if (phi < theta || phi > Math.PI)
				throw new ArgumentOutOfRangeException("phi");
			Constant = constant;
			Linear = linear;
			Quadratic = quadratic;
			Phi = phi;
			Theta = theta;
			Falloff = falloff;
		}
	}
}
