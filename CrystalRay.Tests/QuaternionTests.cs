using System;
using Xunit;

namespace CrystalRay.Tests
{
	public sealed class QuaternionTests
	{
		[Fact]
		public void ShouldRotateCorrectly()
		{
			// Given the complexity of the formula, this simple test should detect most errors.
			// It is, however, not exhaustive, so let's do better later on.
			var v = new Quaternion(1, 0, 0, 0);
			var q = Quaternion.RotateZ(Math.PI / 2);
			var r = q * v * q.Conjugate();

			Assert.Equal(0, r.X, 15);
			Assert.Equal(1, r.Y, 15);
			Assert.Equal(0, r.Z, 15);
			Assert.Equal(0, r.W, 15);
		}
	}
}
