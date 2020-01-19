namespace CrystalRay
{
	public abstract class Light : Element
	{
		public Vector4 Color;

		protected Light()
			: this(new Vector4(1, 1, 1, 1))
		{
		}

		public Light(Vector4 color) => Color = color;

		public abstract ColoredRay? GetLightRay(Ray normalRay);
	}
}
