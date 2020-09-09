using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using SimpleCircuit.Contributors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// Describes a constraint where two contributors need to be kept equal.
    /// </summary>
    /// <seealso cref="IConstraint" />
    public class EqualsConstraint : IConstraint
    {
        private readonly Contributor _a, _b;
        private IContribution _ca, _cb;

        /// <inheritdoc/>
        public bool IsResolved => _a.IsFixed && _b.IsFixed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsConstraint"/> class.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="a"/> or <paramref name="b"/> is <c>null</c>.</exception>
        public EqualsConstraint(Contributor a, Contributor b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
        }

        /// <inheritdoc/>
        public void Apply()
        {
            _ca.Add(1.0, null);
            _cb.Add(-1.0, null);
        }

        /// <inheritdoc/>
        public bool Setup(ISparseSolver<double> solver, int row, UnknownSolverMap map)
        {
            _ca = _a.CreateContribution(solver, row, map);
            _cb = _b.CreateContribution(solver, row, map);

            // Get the number of variables
            var variables = _ca.Unknowns.Union(_cb.Unknowns).Distinct().Count();
            if (variables == 0)
                return false;
            return true;
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _ca.Update(solution);
            _cb.Update(solution);
        }

        /// <inheritdoc/>
        public bool TryResolve()
        {
            if (_a.IsFixed && !_b.IsFixed)
                return _b.Fix(_a.Value);
            if (_b.IsFixed && !_a.IsFixed)
                return _a.Fix(_b.Value);
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{_a} = {_b}";
    }
}
