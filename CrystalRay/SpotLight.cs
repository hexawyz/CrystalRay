using System;
using System.Numerics;

namespace CrystalRay
{
	public sealed class SpotLight : Light
	{
		public Ray CenterRay;
		private SpotLightAttenuation _attenuation;
		private float _cosHalfTheta;
		private float _cosHalfPhi;
		private float _cosDiff;

		public SpotLight(Ray centerRay)
			: this(centerRay, new Vector4(1, 1, 1, 1))
		{
		}

		public SpotLight(Ray centerRay, Vector4 color)
			: this(centerRay, color, new SpotLightAttenuation(1, 0, 0, 0.5f * MathF.PI, MathF.PI, 1.0f))
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
				_cosHalfPhi = MathF.Cos(0.5f * _attenuation.Phi);
				_cosHalfTheta = MathF.Cos(0.5f * _attenuation.Theta);
				_cosDiff = _cosHalfTheta - _cosHalfPhi;
			}
		}

		public override ColoredRay? GetLightRay(Ray normalRay)
		{
			var direction = normalRay.Origin - CenterRay.Origin;
			float l2, l1, i, cos, a, r;

			i = -Vector3.Dot(direction, normalRay.Direction);
			cos = Vector3.Dot(Vector3.Normalize(direction), CenterRay.Direction);

			// If intensity is negative, the light doesn't point in the right direction
			if (i < 0)
				return null;

			// If we the point is outside of the cone, we return no ray
			if (cos <= _cosHalfPhi)
				r = 0;
			else if (cos > _cosHalfTheta)
				r = 1;
			else
				r = MathF.Pow((cos - _cosHalfPhi) / _cosDiff, _attenuation.Falloff);

			l2 = direction.LengthSquared();
			l1 = MathF.Sqrt(l2);

			a = Attenuation.Constant + l1 * Attenuation.Linear + l2 * Attenuation.Quadratic;

			return new ColoredRay(new Ray(CenterRay.Origin, direction), r * i / a * Color);
		}
	}
}
