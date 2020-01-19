using System.Numerics;

namespace CrystalRay
{
	public sealed class Material
	{
		public static Material Default { get; } = new Material();

		public Vector4 Diffuse, Specular, Emissive;
		public float Shininess, Reflectivity, RefractiveIndex;

		public Material()
			: this(new Vector4(1, 1, 1, 1))
		{
		}

		public Material(Vector4 diffuse)
			: this(diffuse, new Vector4(1, 1, 1, 1))
		{
		}

		public Material(Vector4 diffuse, Vector4 specular)
			: this(diffuse, specular, 50)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, Vector4 emissive)
			: this(diffuse, specular, emissive, 50)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, float shininess)
			: this(diffuse, specular, Vector4.Zero, shininess)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, Vector4 emissive, float shininess)
			: this(diffuse, specular, emissive, shininess, 0)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, float shininess, float reflectivity)
			: this(diffuse, specular, Vector4.Zero, shininess, reflectivity)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, float shininess, float reflectivity, float refractiveIndex)
			: this(diffuse, specular, Vector4.Zero, shininess, reflectivity, refractiveIndex)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, Vector4 emissive, float shininess, float reflectivity)
			: this(diffuse, specular, emissive, shininess, reflectivity, 1)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, Vector4 emissive, float shininess, float reflectivity, float refractiveIndex)
		{
			Diffuse = diffuse;
			Specular = specular;
			Emissive = emissive;
			Shininess = shininess;
			Reflectivity = reflectivity;
			RefractiveIndex = refractiveIndex;
		}

		public void Copy(Material original)
		{
			Diffuse = original.Diffuse;
			Specular = original.Specular;
			RefractiveIndex = original.RefractiveIndex;
		}
	}
}
