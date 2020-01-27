using System;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace CrystalRay.Tests
{
    public class Vector4Tests
    {
        [Property]
        public void ShouldConstructWithValues(double x, double y, double z, double w)
        {
			var v = new Vector4(x, y, z, w);

			Assert.Equal(x, v.X);
			Assert.Equal(y, v.Y);
			Assert.Equal(z, v.Z);
			Assert.Equal(w, v.W);
		}

		[Property]
		public void ShouldCompareEqual(double x, double y, double z, double w)
		{
			var v1 = new Vector4(x, y, z, w);
			var v2 = new Vector4(x, y, z, w);

			Assert.True(v1.Equals(v2));
			Assert.True(v2.Equals(v1));
			Assert.True(v1 == v2);
			Assert.False(v1 != v2);
			Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
		}

		[Fact]
		public void ShouldCompareNotEqual()
		{
			var arb = Arb.Default
				.Float()
				.Generator
				.Four()
				.Two()
				.Where(v => v.Item1 != v.Item2).ToArbitrary();

			Prop.ForAll
			(
				arb,
				t =>
				{
					var (v1, v2) = t;

					Assert.False(v1.Equals(v2));
					Assert.False(v2.Equals(v1));
					Assert.False(v1 == v2);
					Assert.True(v1 != v2);
				}
			);
		}

		[Property]
		public void ShouldNegate(double x, double y, double z, double w)
		{
			var v = -new Vector4(x, y, z, w);

			Assert.Equal(-x, v.X);
			Assert.Equal(-y, v.Y);
			Assert.Equal(-z, v.Z);
			Assert.Equal(-w, v.W);
		}
	}
}
