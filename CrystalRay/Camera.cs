using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public sealed class Camera
	{
		public static readonly Vector2 DefaultFieldOfVision = new Vector2((double)(Math.PI / 2), (double)(3 * Math.PI / 8));
		public static readonly Vector3 DefaultPosition = new Vector3(0, 0, -1);
		public static readonly Vector3 DefaultDirection = new Vector3(0, 0, 1);

		Vector3 position, direction;
		Vector2 fieldOfVision;

		public Camera()
			:this(DefaultPosition, DefaultDirection, DefaultFieldOfVision)
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
			this.position = position;
			this.direction = direction;
			this.fieldOfVision = fieldOfVision;
		}

		public Vector2 FieldOfVision
		{
			get
			{
				return fieldOfVision;
			}
			set
			{
				fieldOfVision = value;
			}
		}

		public Vector3 Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
			}
		}

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
	}
}
