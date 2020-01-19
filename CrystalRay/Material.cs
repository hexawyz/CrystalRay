using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public sealed class Material
	{
		static Material defaultMaterial = new Material();

		public static Material Default
		{
			get
			{
				return defaultMaterial;
			}
		}

		public Vector4 Diffuse, Specular, Emissive;
		public double Shininess, Reflectivity, RefractiveIndex;

		public Material()
			: this (new Vector4(1, 1, 1, 1))
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

		public Material(Vector4 diffuse, Vector4 specular, double shininess)
			: this(diffuse, specular, Vector4.Empty, shininess)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, Vector4 emissive, double shininess)
			: this(diffuse, specular, emissive, shininess, 0)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, double shininess, double reflectivity)
			: this(diffuse, specular, Vector4.Empty, shininess, reflectivity)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, double shininess, double reflectivity, double refractiveIndex)
			: this(diffuse, specular, Vector4.Empty, shininess, reflectivity, refractiveIndex)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, Vector4 emissive, double shininess, double reflectivity)
			: this(diffuse, specular, emissive, shininess, reflectivity, 1)
		{
		}

		public Material(Vector4 diffuse, Vector4 specular, Vector4 emissive, double shininess, double reflectivity, double refractiveIndex)
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
