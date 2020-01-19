using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public abstract class Element
	{
		Scene scene;

		internal Element()
		{
		}

		public Scene Scene
		{
			get
			{
				return scene;
			}
			internal set
			{
				scene = value;
			}
		}
	}
}
