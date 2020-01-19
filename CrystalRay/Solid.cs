namespace CrystalRay
{
	public abstract class Solid : Element
	{
		private Material _material;

		public Solid()
			: this(Material.Default)
		{
		}

		public Solid(Material material) => _material = material;

		public virtual bool Filled => true;

		public Material Material
		{
			get => _material;
			set => _material = value ?? Material.Default;
		}

		public abstract Ray? Intersects(Ray ray);
	}
}
