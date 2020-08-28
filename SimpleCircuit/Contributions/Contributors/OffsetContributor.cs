using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// A simple contributor for a variable that is simply the offset to another one.
    /// </summary>
    /// <seealso cref="IContributor" />
    public class OffsetContributor : IContributor
    {
        private IContributor _parent;
        private readonly double _offset;

        /// <inheritdoc/>
        public UnknownTypes Type { get; }

        /// <inheritdoc/>
        public double Value => _parent.Value + _offset;

        /// <inheritdoc/>
        public bool IsFixed => _parent.IsFixed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetContributor"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="type">The unknown type.</param>
        /// <exception cref="ArgumentNullException">parent</exception>
        public OffsetContributor(IContributor parent, double offset, UnknownTypes type)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _offset = offset;
            Type = type;
        }

        /// <inheritdoc/>
        public IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
            => new OffsetContribution(_parent.CreateContribution(solver, row, map), _offset, Type);

        /// <inheritdoc/>
        public bool Fix(double value) => _parent.Fix(value - _offset);

        /// <inheritdoc/>
        public void Reset() => _parent.Reset();

        /// <inheritdoc/>
        public IEnumerable<int> GetUnknowns(UnknownSolverMap map) => _parent.GetUnknowns(map);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{_parent}+{_offset:G4}";
    }
}
