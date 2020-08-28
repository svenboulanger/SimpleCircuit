using SimpleCircuit.Algebra;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// An instance that can contribute to the Newton-Raphson method.
    /// </summary>
    public interface IContributor
    {
        /// <summary>
        /// Gets the type of contributor.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        UnknownTypes Type { get; }

        /// <summary>
        /// Gets the value of the last created contribution.
        /// </summary>
        double Value { get; }

        /// <summary>
        /// Gets whether or not the contributor has a fixed value.
        /// </summary>
        bool IsFixed { get; }

        /// <summary>
        /// Fixes the contributor to a fixed value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the fix was succesful.</returns>
        bool Fix(double value);

        /// <summary>
        /// Resets the contributor.
        /// </summary>
        void Reset();

        /// <summary>
        /// Gets the unknowns that this contributor is using.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>The indexes of the variables that are being used.</returns>
        IEnumerable<int> GetUnknowns(UnknownSolverMap map);

        /// <summary>
        /// Creates a contribution for the contributor at the specified row.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The row.</param>
        /// <param name="map">The unknown solver map.</param>
        /// <returns>The contribution.</returns>
        IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map);
    }
}
