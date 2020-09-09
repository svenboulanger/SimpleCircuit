using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Contributors
{
    /// <summary>
    /// An exponential contributor.
    /// </summary>
    /// <seealso cref="Contributor" />
    public class SkewedContributor : Contributor
    {
        private static readonly double _norm = Math.Log(2);
        private readonly Contributor _a;

        /// <inheritdoc/>
        public override UnknownTypes Type { get; }

        /// <inheritdoc/>
        public override double Value => Math.Log(1 + Math.Exp(_a.Value)) / _norm;

        /// <inheritdoc/>
        public override bool IsFixed => _a.IsFixed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkewedContributor"/> class.
        /// </summary>
        /// <param name="a">The contributor.</param>
        /// <param name="factor">The exponential factor.</param>
        public SkewedContributor(Contributor a)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            Type = a.Type;
        }

        /// <inheritdoc/>
        public override IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
            => new SkewedContribution(_a.CreateContribution(solver, row, map));

        /// <inheritdoc/>
        public override void Reset() => _a.Reset();

        /// <inheritdoc/>
        public override bool Fix(double value)
        {
            if (value <= 0)
                throw new ArgumentException("The value should be greater than 0.");
            var x = Math.Log(Math.Exp(value * _norm) - 1);
            return _a.Fix(x);
        }

        /// <inheritdoc/>
        public override IEnumerable<int> GetUnknowns(UnknownSolverMap map)
            => _a.GetUnknowns(map);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"skew({_a})";
    }
}
