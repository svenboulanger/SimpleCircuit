using SimpleCircuit.Algebra;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Represents the equation in a solver.
    /// </summary>
    public interface IRowEquation
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        double Value { get; }

        /// <summary>
        /// Applies the equation to the row.
        /// </summary>
        /// <param name="derivative">The factor.</param>
        /// <param name="rhs">The right-hand side vector element, or <c>null</c> when the equation is linear.</param>
        void Apply(double derivative, Element<double> rhs);

        /// <summary>
        /// Updates the values in the row equation.
        /// </summary>
        void Update();
    }
}
