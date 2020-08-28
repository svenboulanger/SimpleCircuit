using SimpleCircuit.Algebra;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// A contributor for an unknown that represents the unknown itself.
    /// </summary>
    /// <seealso cref="IContributor" />
    public class DirectContributor : IContributor
    {
        private readonly string _name;
        private double _fixedValue;
        private IContribution _contribution;

        /// <inheritdoc/>
        public UnknownTypes Type { get; }

        /// <inheritdoc/>
        public double Value => IsFixed ? _fixedValue : _contribution?.Value ?? 0.0;

        /// <inheritdoc/>
        public bool IsFixed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectContributor"/> class.
        /// </summary>
        public DirectContributor(string name, UnknownTypes type)
        {
            Type = type;
            _name = name;
        }

        /// <inheritdoc/>
        public IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
            => _contribution = IsFixed ? 
                (IContribution)new ConstantContribution(solver, row, _fixedValue, Type) :
                (IContribution)new DirectContribution(solver, row, map.GetUnknown(this, Type), Type);

        /// <inheritdoc/>
        public void Reset()
        {
            IsFixed = false;
            _contribution = null;
        }

        /// <inheritdoc/>
        public bool Fix(double value)
        {
            if (IsFixed)
                return false;
            _fixedValue = value;
            IsFixed = true;
            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<int> GetUnknowns(UnknownSolverMap map)
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
