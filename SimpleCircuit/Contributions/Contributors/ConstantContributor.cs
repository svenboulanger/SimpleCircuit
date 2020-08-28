using SimpleCircuit.Algebra;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// A contributor for a constant value.
    /// </summary>
    /// <seealso cref="IContributor" />
    public class ConstantContributor : IContributor
    {
        /// <inheritdoc/>
        public UnknownTypes Type { get; }

        /// <inheritdoc/>
        public double Value { get; }

        /// <inheritdoc/>
        public bool IsFixed => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantContributor"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ConstantContributor(UnknownTypes type, double value)
        {
            Type = type;
            Value = value;
        }

        /// <inheritdoc/>
        public IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
            => new ConstantContribution(solver, row, Value, Type);

        /// <inheritdoc/>
        public IEnumerable<int> GetUnknowns(UnknownSolverMap map)
        {
            yield break;
        }

        /// <inheritdoc/>
        public bool Fix(double value) => false;

        /// <inheritdoc/>
        public void Reset() { }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{Value:G4}";
    }
}
