using SimpleCircuit.Algebra;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// Represents the contribution of a variable to the Newton-Raphson method.
    /// </summary>
    public interface IContribution
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        double Value { get; }

        /// <summary>
        /// Gets the row that the contribution applies to.
        /// </summary>
        /// <value>
        /// The row.
        /// </value>
        int Row { get; }

        /// <summary>
        /// Gets the unknowns that this contribution is using.
        /// </summary>
        /// <value>
        /// The unknowns.
        /// </value>
        IEnumerable<int> Unknowns { get; }

        /// <summary>
        /// Gets the solver that the contribution applies to.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        ISparseSolver<double> Solver { get; }

        /// <summary>
        /// Update the contribution for the next iteration using the current solution.
        /// This can be used to cache some CPU-intensive calculations.
        /// </summary>
        void Update(IVector<double> solution);

        /// <summary>
        /// Applies the contribution to the matrix.
        /// </summary>
        /// <param name="derivative">The factor of the derivatives.</param>
        /// <param name="rhs">The right-hand side vector element.</param>
        /// <remarks>
        /// The contribution is additive. In other words, f = derivative * g(...) with g the contribution that this
        /// instance implements. If g(...) is nonlinear, then it means that it needs to specify a right-hand side
        /// vector that needs to be set to J*X-F(X).
        /// </remarks>
        void Add(double derivative, Element<double> rhs);
    }
}
