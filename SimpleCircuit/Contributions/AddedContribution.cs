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
        private readonly IContribution[] _contributions;
        private readonly double[] _coefficients;

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row => _contributions[0].Row;

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _contributions[0].Solver;

        /// <inheritdoc/>
        public HashSet<int> Unknowns
        {
            get
            {
                var combined = new HashSet<int>();
                foreach (var c in _contributions)
                    combined.UnionWith(c.Unknowns ?? Enumerable.Empty<int>());
                return combined;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddedContribution"/> class.
        /// </summary>
        /// <param name="k">The coefficient.</param>
        /// <param name="contribution">The contribution.</param>
        public AddedContribution(double k, IContribution contribution)
        {
            _contributions = new[] { contribution ?? throw new ArgumentNullException(nameof(contribution)) };
            _coefficients = new[] { k };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddedContribution"/> class.
        /// </summary>
        /// <param name="k1">The k1.</param>
        /// <param name="c1">The c1.</param>
        /// <param name="k2">The k2.</param>
        /// <param name="c2">The c2.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="c1"/> or <paramref name="c2"/> is <c>null</c>.</exception>
        public AddedContribution(double k1, IContribution c1, double k2, IContribution c2)
        {
            _contributions = new[]
            { 
                c1 ?? throw new ArgumentNullException(nameof(c1)),
                c2 ?? throw new ArgumentNullException(nameof(c2))
            };
            _coefficients = new[] { k1, k2 };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddedContribution"/> class.
        /// </summary>
        /// <param name="coefficients">The coefficients.</param>
        /// <param name="contributions">The contributions.</param>
        public AddedContribution(IEnumerable<double> coefficients, IEnumerable<IContribution> contributions)
        {
            _coefficients = coefficients?.ToArray() ?? throw new ArgumentNullException(nameof(coefficients));
            _contributions = contributions?.ToArray() ?? throw new ArgumentNullException(nameof(contributions));
            if (_contributions.Length == 0 || _coefficients.Length == 0)
                throw new ArgumentException("Empty list.");
            if (_contributions.Length != _coefficients.Length)
                throw new ArgumentException("Coefficients and contributions do not have the same length.");
            for (var i = 0; i < _contributions.Length; i++)
            {
                if (_contributions[i] == null)
                    throw new ArgumentNullException($"{nameof(contributions)}[{i}]");
                if (_contributions[i].Solver != _contributions[0].Solver || _contributions[i].Row != _contributions[0].Row)
                    throw new ArgumentException($"Cannot combine contributions from different solvers and/or rows.");
            }
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            // Everything is a linear combination, so let's just apply all with the coefficients
            for (var i = 0; i < _contributions.Length; i++)
                _contributions[i].Add(derivative * _coefficients[i], rhs);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            Value = 0.0;
            for (var i = 0; i < _contributions.Length; i++)
            {
                _contributions[i].Update(solution);
                Value += _coefficients[i] * _contributions[i].Value;
            }
        }

        /// <summary>
        /// Merges two <see cref="AddedContribution"/> together.
        /// </summary>
        /// <param name="a">The first paramers.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static AddedContribution Add(double ca, IContribution a, double cb, IContribution b)
        {
            IEnumerable<double> coefficients;
            IEnumerable<IContribution> contributions;
            if (a is AddedContribution ac)
            {
                coefficients = ac._coefficients;
                contributions = ac._contributions;
            }    
            else
            {
                coefficients = new[] { ca };
                contributions = new[] { a };
            }
            if (b is AddedContribution bc)
            {
                coefficients = coefficients.Union(bc._coefficients);
                contributions = contributions.Union(bc._contributions);
            }
            else
            {
                coefficients = coefficients.Union(new[] { cb });
                contributions = contributions.Union(new[] { b });
            }
            return new AddedContribution(coefficients, contributions);
        }
    }
}
