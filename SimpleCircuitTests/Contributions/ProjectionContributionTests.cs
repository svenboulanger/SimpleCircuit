using NUnit.Framework;
using SimpleCircuit;
using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;

namespace SimpleCircuitTests.Contributions
{
    [TestFixture]
    public class ProjectionContributionTests
    {
        [Test]
        [TestCaseSource(nameof(Data))]
        public void When_Applied_Expect_Reference(double sx, double sy, double orientation, double angle, Vector2 normal)
        {
            var solver = new SparseRealSolver();
            var solution = new DenseVector<double>(3);
            solution[1] = sx;
            solution[2] = sy;
            solution[3] = orientation;
            normal /= normal.Length;
            var csx = new DirectContribution(solver, 1, 1, UnknownTypes.ScaleX);
            var csy = new DirectContribution(solver, 1, 2, UnknownTypes.ScaleY);
            var ca = new DirectContribution(solver, 1, 3, UnknownTypes.Angle);
            var con = new ProjectionContribution(csx, csy, ca, angle, normal);

            var value = Vector2.Normal(angle).Scale(sx, sy).Rotate(orientation) * normal;
            var dfda = Vector2.Normal(angle).Scale(sx, sy).Rotate(orientation + Math.PI / 2) * normal;
            var dfdsx = Math.Cos(angle) * (Vector2.Normal(orientation) * normal);
            var dfdsy = Math.Sin(angle) * (Vector2.Normal(orientation + Math.PI / 2) * normal);

            // Normalize
            con.Update(solution);
            con.Add(1.0, null);

            Assert.AreEqual(3, con.Unknowns.Count);
            Assert.AreEqual(3, solver.Size);
            con.Update(solution);
            Assert.AreEqual(dfdsx, solver.FindElement(new MatrixLocation(1, 1)).Value, 1e-9);
            Assert.AreEqual(dfdsy, solver.FindElement(new MatrixLocation(1, 2)).Value, 1e-9);
            Assert.AreEqual(dfda, solver.FindElement(new MatrixLocation(1, 3)).Value, 1e-9);
            Assert.AreEqual(dfdsx * sx + dfdsy * sy + dfda * orientation - value, solver.FindElement(1).Value, 1e-9);
        }

        public static IEnumerable<TestCaseData> Data
        {
            get
            {
                yield return new TestCaseData(1.0, 1.0, 0.0, 0.0, new Vector2(1, 0));
                yield return new TestCaseData(1.0, 1.0, Math.PI, 0.0, new Vector2(1, 0));
                yield return new TestCaseData(0.5, 1.0, Math.PI / 2, 1.0, new Vector2(1, 1));
                yield return new TestCaseData(-0.5, 0.75, -1, 0.5, new Vector2(2, 1));
            }
        }
            
    }
}
