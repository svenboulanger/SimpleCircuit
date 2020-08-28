using NUnit.Framework;
using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuitTests.Contributions.Contributors
{
    [TestFixture]
    public class ConstantContributorTests
    {
        [Test]
        [TestCase(1.0)]
        [TestCase(0.0)]
        [TestCase(-10.0)]
        public void When_Fixed_Expect_Reference(double value)
        {
            var c = new ConstantContributor(UnknownTypes.Scalar, value);
            Assert.AreEqual(c.Fix(value), false);
            Assert.AreEqual(c.IsFixed, true);
            Assert.AreEqual(c.Value, value, 1e-9);
        }

        [Test]
        [TestCase(1.0)]
        [TestCase(0.0)]
        [TestCase(-10.0)]
        public void When_Applied_Expect_Reference(double value)
        {
            var solver = new SparseRealSolver();
            var map = new UnknownSolverMap();
            var d = new ConstantContributor(UnknownTypes.Scalar, value);
            var con = d.CreateContribution(solver, 1, map);
            Assert.IsInstanceOf<ConstantContribution>(con);
            Assert.AreEqual(con.Value, value, 1e-9);
        }
    }
}
