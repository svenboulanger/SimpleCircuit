using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// Represents a linear combination of contributions.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class AddedContribution : IContribution
    {
        private readonly IContribution _a, _b;

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row => _a.Row;

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _a.Solver;

        /// <inheritdoc/>
        public IEnumerable<int> Unknowns => _a.Unknowns.Union(_b.Unknowns);

        /// <summary>
        /// Initializes a new instance of the <see cref="AddedContribution"/> class.
        /// </summary>
        /// <param name="k1">The k1.</param>
        /// <param name="c1">The c1.</param>
        /// <param name="k2">The k2.</param>
        /// <param name="c2">The c2.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="c1"/> or <paramref name="c2"/> is <c>null</c>.</exception>
        public AddedContribution(IContribution a, IContribution b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            _a.Add(derivative, rhs);
            _b.Add(derivative, rhs);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _a.Update(solution);
            _b.Update(solution);
            Value = _a.Value + _b.Value;
        }
    }
}
