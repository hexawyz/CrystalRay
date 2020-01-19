using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public sealed class Scene
	{
		#region ElementCollection Class

		public sealed class ElementCollection : Collection<Element>
		{
			Scene scene;

			public ElementCollection(Scene scene)
				: base(scene.elementList)
			{
				this.scene = scene;
			}

			protected override void ClearItems()
			{
				lock (scene.syncRoot)
				{
					scene.solidList.Clear();
					scene.lightList.Clear();

					for (int i = 0; i < scene.elementList.Count; i++)
						scene.elementList[i].Scene = null;

					scene.elementList.Clear();
				}
			}

			protected override void InsertItem(int index, Element item)
			{
				lock (scene.syncRoot)
				{
					if (item.Scene != null)
						throw new InvalidOperationException();

					scene.elementList.Insert(index, item);

					if (item is Solid)
						scene.solidList.Add((Solid)item);
					else if (item is Light)
						scene.lightList.Add((Light)item);
				}
			}

			protected override void SetItem(int index, Element item)
			{
				lock (scene.syncRoot)
				{
					Element element = scene.elementList[index];

					if (element != item)
					{
						if (item.Scene != null)
							throw new InvalidOperationException();

						if (element is Solid)
							scene.solidList.Remove((Solid)element);
						else if (element is Light)
							scene.lightList.Remove((Light)element);

						item.Scene = scene;
						scene.elementList[index] = item;
						element.Scene = null;

						if (item is Solid)
							scene.solidList.Add((Solid)item);
						else if (item is Light)
							scene.lightList.Add((Light)item);
					}
				}
			}

			protected override void RemoveItem(int index)
			{
				lock (scene.syncRoot)
				{
					Element element = scene.elementList[index];

					scene.elementList.RemoveAt(index);

					if (element is Solid)
						scene.solidList.Remove((Solid)element);
					else if (element is Light)
						scene.lightList.Remove((Light)element);

					element.Scene = null;
				}
			}
		}

		#endregion

		// We use this value of epsilon for distance comparisons
		// A too small epsilon will give bad results for the lighting (black dots)
		// A too big epsilon will interfere with the ray tracing algorithm itself
		const double epsilon = 0.000001;

		List<Element> elementList;
		List<Solid> solidList;
		List<Light> lightList;
		ElementCollection elementCollection;
		Vector4 ambient;
		double refractiveIndex;
		object syncRoot;

		public Scene()
		{
			elementList = new List<Element>();
			solidList = new List<Solid>();
			lightList = new List<Light>();
			elementCollection = new ElementCollection(this);
			syncRoot = new object();
			Ambient = Vector4.Empty;
			refractiveIndex = 1.0f;
		}

		public ElementCollection Elements
		{
			get
			{
				return elementCollection;
			}
		}

		public Vector4 Ambient
		{
			get
			{
				return ambient;
			}
			set
			{
				ambient = value;
			}
		}

		public double RefractionIndex
		{
			get
			{
				return refractiveIndex;
			}
			set
			{
				refractiveIndex = value;
			}
		}

		public Vector4 Cast(Ray ray, Camera camera, int maxBounces)
		{
			Stack<double> indexStack = new Stack<double>(maxBounces);

			indexStack.Push(refractiveIndex);

			return Cast(ray, camera, indexStack, maxBounces);
		}

		public Vector4 Cast(Ray ray, Camera camera, ref Stack<double> indexStack, int maxBounces)
		{
			if (indexStack == null)
				indexStack = new Stack<double>(maxBounces);
			else
				indexStack.Clear();

			indexStack.Push(refractiveIndex);

			return Cast(ray, camera, indexStack, maxBounces);
		}

		private Vector4 Cast(Ray ray, Camera camera, Stack<double> indexStack, int nBounces)
		{
			Ray? normalRay;
			// We measure squarred distances here because we only need to compare them
			double distance,
				minDistance = double.PositiveInfinity;
			Solid nearestSolid = null;
			Ray nearestNormalRay = Ray.Empty;
			bool outgoing = false;

			// Checks every element for an intersection with the ray
			for (int i = 0; i < solidList.Count; i++)
			{
				normalRay = solidList[i].Intersects(ray);

				// If there was an intersection
				if (normalRay != null)
				{
					distance = (normalRay.Value.Origin - ray.Origin).LengthSquarred();

					// If distance is lesser we have a new nearest element
					if (distance > epsilon && distance < minDistance)
					{
						minDistance = distance;
						nearestSolid = solidList[i];
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
						reflectedColor = Vector4.Empty;

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
							newIndex = indexStack.Peek();

						refractedDirection = Refract(nearestNormalRay.Direction, ray.Direction, indexStack.Peek(), newIndex);

						if (refractedDirection != null)
							refractedColor = Cast(DisplaceRay(nearestNormalRay.Origin, refractedDirection.Value), camera, indexStack, nBounces - 1);
						else
							//refractedColor = Cast(DisplaceRay(nearestNormalRay.Origin, reflectedDirection), camera, refractiveIndex, nBounces - 1);
							refractedColor = Vector4.Empty;

						return LightPoint(ray, nearestNormalRay, nearestSolid, camera) * nearestSolid.Material.Diffuse.W
							+ nearestSolid.Material.Reflectivity * nearestSolid.Material.Diffuse * reflectedColor
							+ refractedColor * (1 - nearestSolid.Material.Diffuse.W);
					}
					else
						return LightPoint(ray, nearestNormalRay, nearestSolid, camera)
							+ nearestSolid.Material.Reflectivity * nearestSolid.Material.Diffuse * reflectedColor;
				}
				else
					return LightPoint(ray, nearestNormalRay, nearestSolid, camera);
			}
			else
				return Vector4.Empty;
		}

		Ray DisplaceRay(Vector3 origin, Vector3 direction)
		{
			return new Ray(origin - 0.5 * epsilon * direction, direction);
		}

		Vector3 Tangent(Vector3 normal, Vector3 direction)
		{
			return direction - Vector3.DotProduct(direction, normal) * normal;
		}

		Vector3 Reflect(Vector3 normal, Vector3 direction)
		{
			return direction - 2 * Vector3.DotProduct(direction, normal) * normal;
		}

		Vector3? Refract(Vector3 normal, Vector3 direction, double n1, double n2)
		{
			Vector3 tangent = Tangent(normal, direction);
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

		Vector4 LightPoint(Ray incomingRay, Ray normalRay, Solid solid, Camera camera)
		{
			Vector4 diffuse, specular;
			double baseDistance, distance, specularCoefficient;
			Ray? intersectionNormalRay;
			Vector3 viewerDirection, reflectedDirection;

			diffuse = Vector4.Empty;
			specular = Vector4.Empty;
			reflectedDirection = Vector3.Empty;

			viewerDirection = camera.Position - normalRay.Origin;
			viewerDirection.Normalize();

			// We will now try to light the point with every light in the scene
			for (int i = 0; i < lightList.Count; i++)
			{
				ColoredRay? coloredRay;

				coloredRay = lightList[i].GetLightRay(normalRay);

				// The light might not cast any ray towards our point
				if (coloredRay != null)
				{
					// Compute the distance from our point to the light
					baseDistance = (normalRay.Origin - coloredRay.Value.Ray.Origin).LengthSquarred();

					// We now check every element for ensuring the ray can travel to our point freely
					for (int j = 0; j < solidList.Count; j++)
					{
						intersectionNormalRay = solidList[j].Intersects(coloredRay.Value.Ray);

						// If there is an intesection, check the length (maybe our point is nearer)
						if (intersectionNormalRay != null)
						{
							if (solidList[j] == solid)
								reflectedDirection = coloredRay.Value.Ray.Direction - 2 * Vector3.DotProduct(coloredRay.Value.Ray.Direction, intersectionNormalRay.Value.Direction) * intersectionNormalRay.Value.Direction;

							distance = (intersectionNormalRay.Value.Origin - coloredRay.Value.Ray.Origin).LengthSquarred();
							// If the object is between the light and our point, it won't be lit
							if (distance + epsilon < baseDistance)
							    goto NotLit;
						}
					}

					specularCoefficient = (double)(Math.Pow(Vector3.DotProduct(reflectedDirection, viewerDirection), solid.Material.Shininess));

					// Accumulate the light
					diffuse += coloredRay.Value.Color;
					specular += specularCoefficient * coloredRay.Value.Color;
				}
			NotLit: ;
			}

			return (Ambient + diffuse) * solid.Material.Diffuse + specular * solid.Material.Specular + solid.Material.Emissive;
		}
	}
}
