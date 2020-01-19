using System;
using System.Numerics;

namespace CrystalRay
{
	public sealed class Sphere : Solid
	{
		public Vector3 Center;
		private float _radius;
		private float _radiusSquared;

		public Sphere()
		{
		}

		public Sphere(Material material)
			: base(material)
		{
		}

		public Sphere(Vector3 center, float radius)
			: this(center, radius, Material.Default)
		{
		}

		public Sphere(Vector3 center, float radius, Material material)
			: base(material)
		{
			Center = center;
			Radius = radius;
		}

		public float Radius
		{
			get => _radius;
			set
			{
				_radius = value;
				_radiusSquared = value * value;
			}
		}

		public override Ray? Intersects(Ray ray)
		{
			float b, c, d, x, x1, x2;

			// Finding an intersection here is basically solving a 2nd degree equation

			var v = ray.Origin - Center;

			// a = ray.Direction.LengthSquarred() = 1.0f
			b = 2 * Vector3.Dot(ray.Direction, v);
			c = v.LengthSquared() - _radiusSquared;

			d = b * b - 4 * c;

			if (d < 0) // We have no real solution if d < 0
			{
				return null;
			}
			else if (d == 0) // Only one intersection if d = 0
			{
				x1 = x2 = -0.5f * b;
			}
			else // And two possible intersections if d > 0
			{
				d = MathF.Sqrt(d);

				// a = 1.0f => -1 / 2a = -0.5f
				x1 = -0.5f * (b + d);
				x2 = -0.5f * (b - d);
			}

			// Now we need to find the nearest intersection
			// We need x > 0 and x minimum

			if (x1 < 0 && x2 < 0)
				return null;

			if (x1 > 0 && x2 > 0)
				x = MathF.Min(x1, x2);
			else
				x = MathF.Max(x1, x2);

			// Now we have the intersection point
			v = ray.Origin + x * ray.Direction;

			return new Ray(v, v - Center);
		}
	}
}
