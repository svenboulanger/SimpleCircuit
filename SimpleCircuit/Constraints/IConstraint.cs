using SimpleCircuit.Algebra;
using SimpleCircuit.Contributors;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// Represents a constraint.
    /// </summary>
    public interface IConstraint
    {
        /// <summary>
        /// Gets a value indicating whether the constraint is already resolved.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the constraint is already resolved; otherwise, <c>false</c>.
        /// </value>
        bool IsResolved { get; }

        /// <summary>
        /// Tries to resolve the constraint.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the constraint was resolved; otherwise, <c>false</c>.
        /// </returns>
        bool TryResolve();

        /// <summary>
        /// Sets up the constraint for the specified solver.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The row.</param>
        /// <param name="map">The unknown solver map.</param>
        /// <returns>
        /// <c>true</c> if the constraint has some unknowns in it; otherwise, <c>false</c>.
        /// </returns>
        bool Setup(ISparseSolver<double> solver, int row, UnknownSolverMap map);

        /// <summary>
        /// Updates the constraint for the next iteration.
        /// </summary>
        void Update(IVector<double> solution);

        /// <summary>
        /// Applies the constraint.
        /// </summary>
        void Apply();
    }
}
