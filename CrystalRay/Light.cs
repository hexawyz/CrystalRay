using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public abstract class Light : Element
	{
		public Vector4 Color;

		public Light()
			: this(new Vector4(1, 1, 1, 1))
		{
		}

		public Light(Vector4 color)
		{
			Color = color;
		}

		public abstract ColoredRay? GetLightRay(Ray normalRay);
	}
}
