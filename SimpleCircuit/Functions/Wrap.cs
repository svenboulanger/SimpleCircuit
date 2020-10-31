using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Wrap function.
    /// </summary>
    /// <seealso cref="Function"/>
    public class Wrap : Function
    {
        private readonly Function _a, _b;

        /// <inheritdoc/>
        public override double Value => Utility.Wrap(_a.Value, _b.Value);

        /// <inheritdoc/>
        public override bool IsFixed => _a.IsFixed && _b.IsFixed;

        /// <inheritdoc/>
        public override bool IsConstant => _a.IsConstant && _b.IsConstant;

        /// <summary>
        /// Wraps a function using another function as the modulus.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        public Wrap(Function a, Function b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
            if (!_b.IsConstant)
                throw new ArgumentException("Only a constant can be used as the modulus for Wrap().");
        }

        /// <inheritdoc/>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
            => _a.CreateEquation(row, mapper, solver);

        /// <inheritdoc/>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
            => _a.Differentiate(coefficient, equations);

        /// <inheritdoc/>
        public override bool Resolve(double value)
        {
            if (_b.IsFixed)
                return _a.Resolve(Utility.Wrap(value, _b.Value));
            return false;
        }

        /// <summary>
        /// Convers to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"Wrap({_a}, {_b})";
    }
}
