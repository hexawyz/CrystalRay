using System.Runtime.InteropServices;

namespace CrystalRay
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Ray
	{
		public static readonly Ray Empty = new Ray();

		private Vector3 _direction;

		public Ray(Vector3 origin, Vector3 direction)
		{
			Origin = origin;
			_direction = Vector3.Normalize(direction);
		}

		/// <summary>
		/// Gets or sets the origin of the ray
		/// </summary>
		public Vector3 Origin { get; set; }

		/// <summary>
		/// Gets or sets the direction
		/// </summary>
		/// <remarks>
		/// The direction is always normalized
		/// </remarks>
		public Vector3 Direction
		{
			get => _direction;
			set => _direction = Vector3.Normalize(value);
		}

		public override string ToString()
			=> $"{{ Origin = {Origin.ToString()}; Direction = {Direction.ToString()} }}";
	}
}
