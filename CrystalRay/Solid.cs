using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public abstract class Solid : Element
	{
		Material material;

		public Solid()
			: this(Material.Default)
		{
		}

		public Solid(Material material)
		{
			this.material = material;
		}

		public virtual bool Filled
		{
			get
			{
				return true;
			}
		}

		public Material Material
		{
			get
			{
				return material;
			}
			set
			{
				if (value == null)
					material = Material.Default;
				else
					material = value;
			}
		}

		public abstract Ray? Intersects(Ray ray);
	}
}
