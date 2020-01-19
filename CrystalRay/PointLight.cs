using System;
using System.Numerics;

namespace CrystalRay
{
	public sealed class PointLight : Light
	{
		public Vector3 Position;
		public readonly LightAttenuation Attenuation;

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
			var direction = normalRay.Origin - Position;
			float l2, l1, i, a;

			i = -Vector3.Dot(direction, normalRay.Direction);

			// If intensity is negative, the light doesn't point in the right direction
			if (i < 0)
				return null;

			l2 = direction.LengthSquared();
			l1 = MathF.Sqrt(l2);

			a = Attenuation.Constant + l1 * Attenuation.Linear + l2 * Attenuation.Quadratic;

			return new ColoredRay(new Ray(Position, direction), i / a * Color);
		}
	}
}
