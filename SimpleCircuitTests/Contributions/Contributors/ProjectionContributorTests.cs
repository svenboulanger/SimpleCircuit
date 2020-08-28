using NUnit.Framework;
using SimpleCircuit;
using SimpleCircuit.Contributions;
using System;

namespace SimpleCircuitTests.Contributions.Contributors
{
    [TestFixture]
    public class ProjectionContributorTests
    {
        [Test]
        public void When_FixOrientation_Expect_Reference()
        {
            // The one to fix
            IContributor a = new DirectContributor("a", UnknownTypes.Angle);
            IContributor sx = new DirectContributor("sx", UnknownTypes.ScaleX);
            IContributor sy = new DirectContributor("sy", UnknownTypes.ScaleY);

            // Case 1
            var c = new ProjectionContributor(1.0.SX(), sy, a, 0.0, new Vector2(1.0, 0.0));
            Assert.AreEqual(true, c.Fix(1.0));
            Assert.AreEqual(true, a.IsFixed);
            Assert.AreEqual(false, sy.IsFixed);
            Assert.AreEqual(0.0, a.Value, 1e-9);
            c.Reset();
            Assert.AreEqual(false, a.IsFixed);

            // Case 2
            c = new ProjectionContributor(1.0.SX(), sy, a, 0.0, new Vector2(0.0, 1.0));
            Assert.AreEqual(true, c.Fix(1.0));
            Assert.AreEqual(true, a.IsFixed);
            Assert.AreEqual(false, sy.IsFixed);
            Assert.AreEqual(Math.PI / 2, a.Value, 1e-9);
            c.Reset();
        }
    }
}
