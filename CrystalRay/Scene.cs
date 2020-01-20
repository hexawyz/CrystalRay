using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CrystalRay
{
	public sealed class Scene
	{
		#region ElementCollection Class

		public sealed class ElementCollection : Collection<Element>
		{
			private readonly Scene _scene;

			public ElementCollection(Scene scene)
				: base(scene._elementList) => _scene = scene;

			protected override void ClearItems()
			{
				lock (_scene._syncRoot)
				{
					_scene._solidList.Clear();
					_scene._lightList.Clear();

					for (int i = 0; i < _scene._elementList.Count; i++)
						_scene._elementList[i].Scene = null;

					_scene._elementList.Clear();
				}
			}

			protected override void InsertItem(int index, Element item)
			{
				lock (_scene._syncRoot)
				{
					if (item.Scene != null)
						throw new InvalidOperationException();

					_scene._elementList.Insert(index, item);
					AddToScene(item);
				}
			}

			protected override void SetItem(int index, Element item)
			{
				lock (_scene._syncRoot)
				{
					var element = _scene._elementList[index];

					if (element != item)
					{
						if (item.Scene != null)
							throw new InvalidOperationException();

						RemoveFromScene(element);

						item.Scene = _scene;
						_scene._elementList[index] = item;
						element.Scene = null;

						AddToScene(item);
					}
				}
			}

			protected override void RemoveItem(int index)
			{
				lock (_scene._syncRoot)
				{
					var element = _scene._elementList[index];

					_scene._elementList.RemoveAt(index);

					RemoveFromScene(element);

					element.Scene = null;
				}
			}

			private void AddToScene(Element element)
			{
				if (element is Solid solid)
					_scene._solidList.Add(solid);
				else if (element is Light light)
					_scene._lightList.Add(light);
			}

			private void RemoveFromScene(Element element)
			{
				if (element is Solid solid)
					_scene._solidList.Remove(solid);
				else if (element is Light light)
					_scene._lightList.Remove(light);
			}
		}

		#endregion

		// We use this value of epsilon for distance comparisons
		// A too small epsilon will give bad results for the lighting (black dots)
		// A too big epsilon will interfere with the ray tracing algorithm itself
		private const double Epsilon = 0.000001;

		private readonly List<Element> _elementList;
		private readonly List<Solid> _solidList;
		private readonly List<Light> _lightList;
		private readonly object _syncRoot;

		public Scene()
		{
			_elementList = new List<Element>();
			_solidList = new List<Solid>();
			_lightList = new List<Light>();
			Elements = new ElementCollection(this);
			_syncRoot = new object();
			Ambient = Vector4.Zero;
			RefractionIndex = 1.0f;
		}

		public ElementCollection Elements { get; }

		public Vector4 Ambient { get; set; }

		public double RefractionIndex { get; set; }

		public Vector4 Cast(Ray ray, Camera camera, int maxBounces)
		{
			var indexStack = new Stack<double>(maxBounces);

			indexStack.Push(RefractionIndex);

			return Cast(ray, camera, indexStack, maxBounces);
		}

		public Vector4 Cast(Ray ray, Camera camera, ref Stack<double> indexStack, int maxBounces)
		{
			if (indexStack == null)
				indexStack = new Stack<double>(maxBounces);
			else
				indexStack.Clear();

			indexStack.Push(RefractionIndex);

			return Cast(ray, camera, indexStack, maxBounces);
		}

		private Vector4 Cast(Ray ray, Camera camera, Stack<double> indexStack, int nBounces)
		{
			Ray? normalRay;
			// We measure squarred distances here because we only need to compare them
			double distance,
				minDistance = double.PositiveInfinity;
			Solid nearestSolid = null;
			var nearestNormalRay = Ray.Empty;
			bool outgoing = false;

			// Checks every element for an intersection with the ray
			for (int i = 0; i < _solidList.Count; i++)
			{
				normalRay = _solidList[i].Intersects(ray);

				// If there was an intersection
				if (normalRay != null)
				{
					distance = (normalRay.Value.Origin - ray.Origin).LengthSquared();

					// If distance is lesser we have a new nearest element
					if (distance > Epsilon && distance < minDistance)
					{
						minDistance = distance;
						nearestSolid = _solidList[i];
						nearestNormalRay = normalRay.Value;
					}
				}
			}

			if (nearestSolid != null)
			{
				Vector3 reflectedDirection;
				Vector3? refractedDirection;
				Vector4 reflectedColor, refractedColor;

				// Determine if the ray is incoming or outgoing by comparing the normal and the ray directions
				if (Vector3.DotProduct(nearestNormalRay.Direction, ray.Direction) > 0)
				{
					outgoing = true;
					nearestNormalRay.Direction = -nearestNormalRay.Direction;
				}

				if (nBounces > 0 && (nearestSolid.Material.Diffuse.W < 1 || nearestSolid.Material.Reflectivity > 0))
				{
					reflectedDirection = Reflect(nearestNormalRay.Direction, ray.Direction);

					if (nearestSolid.Material.Reflectivity > 0)
						reflectedColor = Cast(DisplaceRay(nearestNormalRay.Origin, reflectedDirection), camera, indexStack, nBounces - 1);
					else
						reflectedColor = Vector4.Zero;

					if (nearestSolid.Material.Diffuse.W < 1)
					{
						double newIndex;

						if (nearestSolid.Filled)
						{
							if (outgoing)
							{
								if (indexStack.Count > 1)
									indexStack.Pop();
								newIndex = indexStack.Peek();
							}
							else
							{
								newIndex = nearestSolid.Material.RefractiveIndex;
								indexStack.Push(newIndex);
							}
						}
						else
						{
							newIndex = indexStack.Peek();
						}

						refractedDirection = Refract(nearestNormalRay.Direction, ray.Direction, indexStack.Peek(), newIndex);

						if (refractedDirection != null)
						{
							refractedColor = Cast(DisplaceRay(nearestNormalRay.Origin, refractedDirection.Value), camera, indexStack, nBounces - 1);
						}
						else
						{
							refractedColor = Cast(DisplaceRay(nearestNormalRay.Origin, reflectedDirection), camera, indexStack, nBounces - 1);
							//refractedColor = Vector4.Zero;
						}

						return LightPoint(ray, nearestNormalRay, nearestSolid, camera) * nearestSolid.Material.Diffuse.W
							+ nearestSolid.Material.Reflectivity * nearestSolid.Material.Diffuse * reflectedColor
							+ refractedColor * (1 - nearestSolid.Material.Diffuse.W);
					}
					else
					{
						return LightPoint(ray, nearestNormalRay, nearestSolid, camera)
							+ nearestSolid.Material.Reflectivity * nearestSolid.Material.Diffuse * reflectedColor;
					}
				}
				else
				{
					return LightPoint(ray, nearestNormalRay, nearestSolid, camera);
				}
			}
			else
			{
				return Vector4.Zero;
			}
		}

		private Ray DisplaceRay(Vector3 origin, Vector3 direction) => new Ray(origin - 0.5 * Epsilon * direction, direction);

		private Vector3 Tangent(Vector3 normal, Vector3 direction) => direction - Vector3.DotProduct(direction, normal) * normal;

		private Vector3 Reflect(Vector3 normal, Vector3 direction) => direction - 2 * Vector3.DotProduct(direction, normal) * normal;

		private Vector3? Refract(Vector3 normal, Vector3 direction, double n1, double n2)
		{
			var tangent = Tangent(normal, direction);
			double sin, cos;

			if (n1 == n2)
				return direction;

			sin = n1 * Vector3.DotProduct(direction, tangent) / n2;
			if (sin < -1 || sin > 1)
				return null;
			cos = (double)Math.Sqrt(1 - sin * sin);

			return sin * tangent - cos * normal;
			//return direction + (n1 - n2) * normal;
		}

		private Vector4 LightPoint(Ray incomingRay, Ray normalRay, Solid solid, Camera camera)
		{
			Vector4 diffuse;
			Vector4 specular;
			double baseDistance;
			double distance;
			double specularCoefficient;
			Ray? intersectionNormalRay;
			Vector3 viewerDirection;
			Vector3 reflectedDirection;

			diffuse = Vector4.Zero;
			specular = Vector4.Zero;
			reflectedDirection = Vector3.Zero;

			viewerDirection = camera.Position - normalRay.Origin;
			viewerDirection = Vector3.Normalize(viewerDirection);

			// We will now try to light the point with every light in the scene
			for (int i = 0; i < _lightList.Count; i++)
			{
				var coloredRay = _lightList[i].GetLightRay(normalRay);

				// The light might not cast any ray towards our point
				if (coloredRay != null)
				{
					// Compute the distance from our point to the light
					baseDistance = (normalRay.Origin - coloredRay.Value.Ray.Origin).LengthSquared();

					// We now check every element for ensuring the ray can travel to our point freely
					for (int j = 0; j < _solidList.Count; j++)
					{
						intersectionNormalRay = _solidList[j].Intersects(coloredRay.Value.Ray);

						// If there is an intesection, check the length (maybe our point is nearer)
						if (intersectionNormalRay != null && _solidList[j].Material.Diffuse.W >= 1d) // NB: Hack for translucent materials…
						{
							if (_solidList[j] == solid)
								reflectedDirection = coloredRay.Value.Ray.Direction - 2 * Vector3.DotProduct(coloredRay.Value.Ray.Direction, intersectionNormalRay.Value.Direction) * intersectionNormalRay.Value.Direction;

							distance = (intersectionNormalRay.Value.Origin - coloredRay.Value.Ray.Origin).LengthSquared();
							// If the object is between the light and our point, it won't be lit
							if (distance + Epsilon < baseDistance)
								goto NotLit;
						}
					}

					specularCoefficient = Math.Pow(Vector3.DotProduct(reflectedDirection, viewerDirection), solid.Material.Shininess);

					// Accumulate the light
					diffuse += coloredRay.Value.Color;
					specular += specularCoefficient * coloredRay.Value.Color;
				}
			NotLit:;
			}

			return (Ambient + diffuse) * solid.Material.Diffuse + specular * solid.Material.Specular + solid.Material.Emissive;
		}
	}
}
