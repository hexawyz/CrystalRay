using System;

namespace CrystalRay
{
	public sealed class SpotLight : Light
	{
		public Ray CenterRay;
		private SpotLightAttenuation _attenuation;
		private double _cosHalfTheta, _cosHalfPhi, _cosDiff;

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
			get => _attenuation;
			set
			{
				_attenuation = value;
				_cosHalfPhi = Math.Cos(0.5 * _attenuation.Phi);
				_cosHalfTheta = Math.Cos(0.5 * _attenuation.Theta);
				_cosDiff = _cosHalfTheta - _cosHalfPhi;
			}
		}

		public override ColoredRay? GetLightRay(Ray normalRay)
		{
			var direction = normalRay.Origin - CenterRay.Origin;
			double l2, l1, i, cos, a, r;

			i = -Vector3.DotProduct(direction, normalRay.Direction);
			cos = Vector3.DotProduct(Vector3.Normalize(direction), CenterRay.Direction);

			// If intensity is negative, the light doesn't point in the right direction
			if (i < 0)
				return null;

			// If we the point is outside of the cone, we return no ray
			if (cos <= _cosHalfPhi)
				r = 0;
			else if (cos > _cosHalfTheta)
				r = 1;
			else
				r = Math.Pow((cos - _cosHalfPhi) / _cosDiff, _attenuation.Falloff);

			l2 = direction.LengthSquarred();
			l1 = Math.Sqrt(l2);

			a = Attenuation.Constant + l1 * Attenuation.Linear + l2 * Attenuation.Quadratic;

			return new ColoredRay(new Ray(CenterRay.Origin, direction), r * i / a * Color);
		}
	}
}
