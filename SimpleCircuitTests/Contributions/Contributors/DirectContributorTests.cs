using NUnit.Framework;
using SimpleCircuit;
using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuitTests.Contributions.Contributors
{
    [TestFixture]
    public class DirectContributorTests
    {
        [Test]
        [TestCase(1.0)]
        [TestCase(0.0)]
        [TestCase(-10.0)]
        public void When_Fixed_Expect_Reference(double value)
        {
            var c = new DirectContributor("x", UnknownTypes.X);
            Assert.AreEqual(c.Fix(value), true);
            Assert.AreEqual(c.IsFixed, true);
            Assert.AreEqual(c.Value, value, 1e-9);

            var solver = new SparseRealSolver();
            var map = new UnknownSolverMap();
            var con = c.CreateContribution(solver, 1, map);
            Assert.IsInstanceOf<ConstantContribution>(con);
            Assert.AreEqual(con.Value, value);
        }

        [Test]
        public void When_Applied_Expect_Reference()
        {
            var solver = new SparseRealSolver();
            var map = new UnknownSolverMap();
            var d = new DirectContributor("x", UnknownTypes.X);
            var con = d.CreateContribution(solver, 1, map);
            Assert.IsInstanceOf<DirectContribution>(con);
            Assert.AreEqual(con.Row, 1);
            Assert.AreEqual(con.Solver, solver);
        }
    }
}
