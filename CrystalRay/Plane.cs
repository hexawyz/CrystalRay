namespace CrystalRay
{
	public sealed class Plane : Solid
	{
		public Ray NormalRay;

		public Plane()
		{
		}

		public Plane(Material material)
			: base(material)
		{
		}

		public Plane(Ray normalRay)
			: this(normalRay, Material.Default)
		{
		}

		public Plane(Ray normalRay, Material material)
			: base(material)
			=> NormalRay = normalRay;

		public override bool Filled => false;

		public override Ray? Intersects(Ray ray)
		{
			Vector3 v, x;
			double l1, l2;

			// If the ray is parallel or included in the plane, it doesn't intersect
			if ((l1 = Vector3.DotProduct(NormalRay.Direction, ray.Direction)) == 0)
			{
				return null;
			}
			else
			{
				v = ray.Origin - NormalRay.Origin;
				l2 = Vector3.DotProduct(NormalRay.Direction, v);

				// If l1 and l2 have the same sign, the ray doesn't point towards the plane
				if (l1 > 0 && l2 > 0 || l1 < 0 && l2 < 0)
					return null;

				// Compute the intersection point
				x = ray.Origin - l2 / l1 * ray.Direction;

				// Now choose a normal that points in the opposite direction as the ray, and return it
				if (l1 < 0) // The normal is already correct
					return new Ray(x, NormalRay.Direction);
				else // We just invert the normal
					return new Ray(x, -NormalRay.Direction);
			}
		}
	}
}
