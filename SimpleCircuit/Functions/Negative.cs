﻿using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that represents the negative operator.
    /// </summary>
    /// <seealso cref="Function" />
    public class Negative : Function
    {
        private readonly Function _a;
        private class RowEquation : IRowEquation
        {
            private readonly IRowEquation _a;
            public double Value { get; private set; }
            public RowEquation(IRowEquation a)
            {
                _a = a;
            }
            public void Apply(double derivative, Element<double> rhs) => _a.Apply(-derivative, rhs);
            public void Update()
            {
                _a.Update();
                Value = -_a.Value;
            }
        }

        /// <inheritdoc/>
        public override double Value => -_a.Value;

        /// <inheritdoc/>
        public override bool IsFixed => _a.IsFixed;

        /// <inheritdoc/>
        public override bool IsConstant => _a.IsConstant;

        /// <summary>
        /// Initializes a new instance of the <see cref="Negative"/> class.
        /// </summary>
        /// <param name="a">The first argument.</param>
        public Negative(Function a)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
        }

        /// <inheritdoc/>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            if (_a.IsFixed)
                return;
            if (coefficient == null)
                _a.Differentiate(null, equations);
            else
                _a.Differentiate(-coefficient, equations);
        }

        /// <inheritdoc/>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver) => new RowEquation(_a.CreateEquation(row, mapper, solver));

        /// <inheritdoc/>
        public override bool Resolve(double value) => _a.Resolve(-value);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"-({_a})";
    }
}
