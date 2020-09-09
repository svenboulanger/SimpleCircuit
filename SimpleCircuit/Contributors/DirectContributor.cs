using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System.Collections.Generic;

namespace SimpleCircuit.Contributors
{
    /// <summary>
    /// A contributor for an unknown that represents the unknown itself.
    /// </summary>
    /// <seealso cref="Contributor" />
    public class DirectContributor : Contributor
    {
        private readonly string _name;
        private double _fixedValue;
        private bool _isFixed;
        private IContribution _contribution;

        /// <inheritdoc/>
        public override UnknownTypes Type { get; }

        /// <inheritdoc/>
        public override double Value => IsFixed ? _fixedValue : _contribution?.Value ?? 0.0;

        /// <inheritdoc/>
        public override bool IsFixed => _isFixed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectContributor"/> class.
        /// </summary>
        public DirectContributor(string name, UnknownTypes type)
        {
            Type = type;
            _name = name;
        }

        /// <inheritdoc/>
        public override IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
            => _contribution = IsFixed ? 
                (IContribution)new ConstantContribution(solver, row, _fixedValue, Type) :
                (IContribution)new DirectContribution(solver, row, map.GetUnknown(this, Type), Type);

        /// <inheritdoc/>
        public override void Reset()
        {
            _isFixed = false;
            _contribution = null;
        }

        /// <inheritdoc/>
        public override bool Fix(double value)
        {
            if (IsFixed)
                return false;
            _fixedValue = value;
            _isFixed = true;
            return true;
        }

        /// <inheritdoc/>
        public override IEnumerable<int> GetUnknowns(UnknownSolverMap map)
        {
            if (map.TryGetIndex(this, Type, out var index))
                yield return index;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => _name;
    }
}
