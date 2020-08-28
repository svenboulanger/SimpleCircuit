using NUnit.Framework;
using SimpleCircuit;
using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;

namespace SimpleCircuitTests.Contributions
{
    [TestFixture]
    public class DirectContributionTests
    {
        [Test]
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(0.0, 1.0, 0.0)]
        [TestCase(0.0, 0.0, 1.0)]
        [TestCase(1.0, 2.0, 3.0)]
        public void When_Applied_Expect_Reference(double derivative, double sol, double value)
        {
            var solver = new SparseRealSolver();
            var solution = new DenseVector<double>(1);
            solution[1] = sol;
            var c = new DirectContribution(solver, 1, 1, UnknownTypes.Scalar);
            Element<double> rhs = null;
            if (!value.Equals(0.0))
            {
                rhs = solver.GetElement(1);
                rhs.Subtract(value);
            }
            c.Update(solution);
            c.Add(derivative, rhs);

            Assert.AreEqual(1, c.Unknowns.Count);
            Assert.AreEqual(1, solver.Size);
            Assert.AreEqual(derivative, solver.FindElement(new MatrixLocation(1, 1)).Value);
            if (value.Equals(0.0))
                Assert.AreEqual(null, solver.FindElement(1));
            else
                Assert.AreEqual(derivative * sol - value, solver.FindElement(1).Value);
        }

        [Test]
        public void When_Solved1D_Expect_Reference()
        {
            var solver = new SparseRealSolver();
            var solution = new DenseVector<double>(1);
            var c1 = new DirectContribution(solver, 1, 1, UnknownTypes.Scalar);
            var c2 = new ConstantContribution(solver, 1, 1.0, UnknownTypes.Scalar);
            c1.Update(solution);
            c1.Add(1.0, null);
            c2.Add(-1.0, null);
            Assert.AreEqual(solver.OrderAndFactor(), 1);
            solver.Solve(solution);
            Assert.AreEqual(solution[1], 1.0, 1e-9);
        }

        [Test]
        public void When_Solved2D_Expect_Reference()
        {
            var solver = new SparseRealSolver();
            var solution = new DenseVector<double>(2);
            var c11 = new DirectContribution(solver, 1, 1, UnknownTypes.Scalar);
            var c12 = new DirectContribution(solver, 1, 2, UnknownTypes.Scalar);
            var c22 = new DirectContribution(solver, 2, 2, UnknownTypes.Scalar);
            var c2 = new ConstantContribution(solver, 2, 3, UnknownTypes.Scalar);

            // row 1: x - y = 0
            c11.Add(1.0, null);
            c12.Add(-1.0, null);

            // row 2: y - c = 0
            c22.Add(1.0, null);
            c2.Add(-1.0, null);

            Assert.AreEqual(solver.OrderAndFactor(), 2);
            solver.Solve(solution);

            Assert.AreEqual(solution[1], 3.0, 1e-9);
            Assert.AreEqual(solution[2], 3.0, 1e-9);
        }
    }
}
