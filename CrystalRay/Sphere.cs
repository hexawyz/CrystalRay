using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public sealed class Sphere : Solid
	{
		public Vector3 Center;
		public double Radius;

		public Sphere()
		{
		}

		public Sphere(Material material)
			: base(material)
		{
		}

		public Sphere(Vector3 center, double radius)
			: this(center, radius, Material.Default)
		{
		}

		public Sphere(Vector3 center, double radius, Material material)
			: base(material)
		{
			Center = center;
			Radius = radius;
		}

		public override Ray? Intersects(Ray ray)
		{
			double b, c, d, x, x1, x2;
			Vector3 v;

			// Finding an intersection here is basically solving a 2nd degree equation

			v = ray.Origin - Center;

			// a = ray.Direction.LengthSquarred() = 1.0f
			b = 2 * Vector3.DotProduct(ray.Direction, v);
			c = v.LengthSquarred() - Radius * Radius;

			d = b * b - 4 * c;

			if (d < 0) // We have no real solution if d < 0
				return null;
			else if (d == 0) // Only one intersection if d = 0
				x1 = x2 = -0.5f * b;
			else // And two possible intersections if d > 0
			{
				d = (double)Math.Sqrt(d);

				// a = 1.0f => -1 / 2a = -0.5f
				x1 = -0.5f * (b + d);
				x2 = -0.5f * (b - d);
			}

			// Now we need to find the nearest intersection
			// We need x > 0 and x minimum

			if (x1 < 0 && x2 < 0)
				return null;

			if (x1 > 0 && x2 > 0)
				x = Math.Min(x1, x2);
			else
				x = Math.Max(x1, x2);

			// Now we have the intersection point
			v = ray.Origin + x * ray.Direction;

			return new Ray(v, v - Center);
		}
	}
}
