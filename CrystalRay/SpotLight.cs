using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public sealed class SpotLight : Light
	{
		public Ray CenterRay;
		SpotLightAttenuation attenuation;
		double cosHalfTheta, cosHalfPhi, cosDiff;

		public SpotLight(Ray centerRay)
			: this(centerRay, new Vector4(1, 1, 1, 1))
		{
		}

		public SpotLight(Ray centerRay, Vector4 color)
			: this(centerRay, color, new SpotLightAttenuation(1, 0, 0, 0.5 * Math.PI, Math.PI, 1.0))
		{
		}

		public SpotLight(Ray centerRay, Vector4 color, SpotLightAttenuation attenuation)
			: base(color)
		{
			CenterRay = centerRay;
			Attenuation = attenuation;
		}

		public SpotLightAttenuation Attenuation
		{
			get
			{
				return attenuation;
			}
			set
			{
				attenuation = value;
				cosHalfPhi = (double)Math.Cos(0.5 * attenuation.Phi);
				cosHalfTheta = (double)Math.Cos(0.5 * attenuation.Theta);
				cosDiff = cosHalfTheta - cosHalfPhi;
			}
		}

		public override ColoredRay? GetLightRay(Ray normalRay)
		{
			Vector3 direction = normalRay.Origin - CenterRay.Origin;
			double l2, l1, i, cos, a, r;

			i = -Vector3.DotProduct(direction, normalRay.Direction);
			cos = Vector3.DotProduct(Vector3.Normalize(direction), CenterRay.Direction);

			// If intensity is negative, the light doesn't point in the right direction
			if (i < 0)
				return null;

			// If we the point is outside of the cone, we return no ray
			if (cos <= cosHalfPhi)
				r = 0;
			else if (cos > cosHalfTheta)
				r = 1;
			else
				r = (double)Math.Pow((cos - cosHalfPhi) / cosDiff, attenuation.Falloff);

			l2 = direction.LengthSquarred();
			l1 = (double)Math.Sqrt(l2);

			a = Attenuation.Constant + l1 * Attenuation.Linear + l2 * Attenuation.Quadratic;

			return new ColoredRay(new Ray(CenterRay.Origin, direction), (r * i / a) * Color);
		}
	}
}
