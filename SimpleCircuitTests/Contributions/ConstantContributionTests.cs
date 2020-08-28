using NUnit.Framework;
using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuitTests.Contributions
{
    [TestFixture]
    public class ConstantContributionTests
    {
        [Test]
        [TestCase(0.0, 10.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(2.0, 1.0)]
        [TestCase(3.0, -1.0)]
        [TestCase(4.0, 5.0)]
        public void When_Applied_Expect_Reference(double factor, double value)
        {
            var solver = new SparseRealSolver();
            var c = new ConstantContribution(solver, 1, value, UnknownTypes.Scalar);
            c.Add(factor, null);

            if (value.Equals(0.0))
                Assert.AreEqual(solver.Size, 0);
            else
            {
                Assert.AreEqual(solver.Size, 1);
                Assert.AreEqual(solver.FindElement(new MatrixLocation(1, 1)), null);
                Assert.AreEqual(solver.FindElement(1).Value, -factor * value);
            }
        }
    }
}
