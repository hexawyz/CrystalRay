using System;
using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Ray
	{
		public static readonly Ray Empty = new Ray();

		Vector3 origin, direction;

		public Ray(Vector3 origin, Vector3 direction)
		{
			this.origin = origin;
			this.direction = Vector3.Normalize(direction);
		}

		/// <summary>
		/// Gets or sets the origin of the ray
		/// </summary>
		public Vector3 Origin
		{
			get
			{
				return origin;
			}
			set
			{
				origin = value;
			}
		}

		/// <summary>
		/// Gets or sets the direction
		/// </summary>
		/// <remarks>
		/// The direction is always normalized
		/// </remarks>
		public Vector3 Direction
		{
			get
			{
				return direction;
			}
			set
			{
				direction = Vector3.Normalize(value);
			}
		}

		public override string ToString()
		{
			return "{ Origin = " + Origin.ToString() + "; Direction = " + Direction.ToString() + " }";
		}
	}
}
