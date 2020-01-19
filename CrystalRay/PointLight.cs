using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public sealed class PointLight : Light
	{
		public Vector3 Position;
		public LightAttenuation Attenuation;

		public PointLight(Vector3 position)
			: this(position, new Vector4(1, 1, 1, 1))
		{
		}

		public PointLight(Vector3 position, Vector4 color)
			: this(position, color, new LightAttenuation(1, 0, 0))
		{
		}

		public PointLight(Vector3 position, Vector4 color, LightAttenuation attenuation)
			: base(color)
		{
			Position = position;
			Attenuation = attenuation;
		}

		public override ColoredRay? GetLightRay(Ray normalRay)
		{
			Vector3 direction = normalRay.Origin - Position;
			double l2, l1, i, a;

			i = -Vector3.DotProduct(direction, normalRay.Direction);

			// If intensity is negative, the light doesn't point in the right direction
			if (i < 0)
				return null;

			l2 = direction.LengthSquarred();
			l1 = (double)Math.Sqrt(l2);

			a = Attenuation.Constant + l1 * Attenuation.Linear + l2 * Attenuation.Quadratic;

			return new ColoredRay(new Ray(Position, direction), (i / a) * Color);
		}
	}
}
