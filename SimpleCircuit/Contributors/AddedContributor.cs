using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Contributors
{
    /// <summary>
    /// A contributor that adds two contributions.
    /// </summary>
    /// <seealso cref="Contributor" />
    public class AddedContributor : Contributor
    {
        private readonly Contributor _a, _b;

        /// <inheritdoc/>
        public override UnknownTypes Type { get; }

        /// <inheritdoc/>
        public override double Value => _a.Value + _b.Value;

        /// <inheritdoc/>
        public override bool IsFixed => _a.IsFixed && _b.IsFixed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddedContributor"/> class.
        /// </summary>
        /// <param name="a">The first contributor.</param>
        /// <param name="b">The second contributor.</param>
        /// <param name="type">The unknown type.</param>
        public AddedContributor(Contributor a, Contributor b, UnknownTypes type)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
            Type = type;
        }

        /// <inheritdoc/>
        public override IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
        {
            return new AddedContribution(
                _a.CreateContribution(solver, row, map),
                _b.CreateContribution(solver, row, map));
        }

        /// <inheritdoc/>
        public override bool Fix(double value)
        {
            if (_a.IsFixed && !_b.IsFixed)
                return _b.Fix(value - _a.Value);
            if (_b.IsFixed && !_a.IsFixed)
                return _a.Fix(value - _b.Value);
            return false;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            _a.Reset();
            _b.Reset();
        }

        /// <inheritdoc/>
        public override IEnumerable<int> GetUnknowns(UnknownSolverMap map)
        {
            var combined = new HashSet<int>();
            combined.UnionWith(_a.GetUnknowns(map) ?? Enumerable.Empty<int>());
            combined.UnionWith(_b.GetUnknowns(map) ?? Enumerable.Empty<int>());
            return combined;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{_a}+{_b}";
        }
    }
}
