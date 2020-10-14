using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Represents the subtraction.
    /// </summary>
    /// <seealso cref="Function" />
    public class Subtraction : Function
    {
        private readonly Function _a, _b;
        private class RowEquation : IRowEquation
        {
            private readonly IRowEquation _a, _b;
            public double Value { get; private set; }
            public RowEquation(IRowEquation a, IRowEquation b)
            {
                _a = a;
                _b = b;
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                _a.Apply(derivative, rhs);
                _b.Apply(-derivative, rhs);
            }

            public void Update()
            {
                _a.Update();
                _b.Update();
                Value = _a.Value - _b.Value;
            }
        }

        /// <inheritdoc/>
        public override double Value => _a.Value - _b.Value;

        /// <inheritdoc/>
        public override bool IsConstant => _a.IsConstant && _b.IsConstant;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subtraction"/> class.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        public Subtraction(Function a, Function b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
        }

        /// <inheritdoc/>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            if (coefficient == null)
            {
                if (!_a.IsConstant)
                    _a.Differentiate(null, equations);
                if (!_b.IsConstant)
                    _b.Differentiate(-1.0, equations);
            }
            else
            {
                if (!_a.IsConstant)
                    _a.Differentiate(coefficient, equations);
                if (!_b.IsConstant)
                    _b.Differentiate(-coefficient, equations);
            }
        }

        /// <inheritdoc/>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
        {
            return new RowEquation(
                _a.CreateEquation(row, mapper, solver),
                _b.CreateEquation(row, mapper, solver));
        }

        /// <inheritdoc/>
        public override bool Resolve(double value)
        {
            // value = a - b
            if (_a.IsConstant)
                return _b.Resolve(_a.Value - value);
            if (_b.IsConstant)
                return _a.Resolve(_b.Value + value);
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"({_a})-({_b})";
    }
}
