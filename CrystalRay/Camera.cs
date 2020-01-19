using System;
using System.Numerics;

namespace CrystalRay
{
	public sealed class Camera
	{
		public static readonly Vector2 DefaultFieldOfVision = new Vector2(MathF.PI / 2, 3 * MathF.PI / 8);
		public static readonly Vector3 DefaultPosition = new Vector3(0, 0, -1);
		public static readonly Vector3 DefaultDirection = new Vector3(0, 0, 1);

		private Vector3 _direction;

		public Camera()
			: this(DefaultPosition, DefaultDirection, DefaultFieldOfVision)
		{
		}

		public Camera(Vector2 fieldOfVision)
			: this(DefaultPosition, DefaultDirection, fieldOfVision)
		{
		}

		public Camera(Vector3 position, Vector3 direction)
			: this(position, direction, DefaultFieldOfVision)
		{
		}

		public Camera(Vector3 position, Vector3 direction, Vector2 fieldOfVision)
		{
			Position = position;
			_direction = direction;
			FieldOfVision = fieldOfVision;
		}

		public Vector2 FieldOfVision { get; set; }

		public Vector3 Position { get; set; }

		public Vector3 Direction
		{
			get => _direction;
			set => _direction = Vector3.Normalize(value);
		}
	}
}
