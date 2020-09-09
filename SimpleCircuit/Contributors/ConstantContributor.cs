using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System.Collections.Generic;

namespace SimpleCircuit.Contributors
{
    /// <summary>
    /// A contributor for a constant value.
    /// </summary>
    /// <seealso cref="Contributor" />
    public class ConstantContributor : Contributor
    {
        /// <inheritdoc/>
        public override UnknownTypes Type { get; }

        /// <inheritdoc/>
        public override double Value { get; }

        /// <inheritdoc/>
        public override bool IsFixed => true;

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
        public override IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
            => new ConstantContribution(solver, row, Value, Type);

        /// <inheritdoc/>
        public override IEnumerable<int> GetUnknowns(UnknownSolverMap map)
        {
            yield break;
        }

        /// <inheritdoc/>
        public override bool Fix(double value) => false;

        /// <inheritdoc/>
        public override void Reset() { }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{Value:G4}";
    }
}
