using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that can be set or gotten, but can't partipicate in the solver.
    /// </summary>
    public class UnsolvableFunction : Function
    {
        private readonly Action<double> _setter;
        private readonly Func<double> _getter;
        private class RowEquation : IRowEquation
        {
            private readonly Element<double> _rhs;
            private readonly Func<double> _getter;
            public double Value => _getter();
            public RowEquation(int row, ISparseSolver<double> solver, Func<double> getter)
            {
                _rhs = solver.GetElement(row);
                _getter = getter;
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs == null)
                    _rhs?.Subtract(derivative * Value);
            }
            public void Update() { }
        }

        /// <inheritdoc/>
        public override double Value => _getter();

        /// <inheritdoc/>
        public override bool IsFixed => true;

        /// <inheritdoc/>
        public override bool IsConstant => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsolvableFunction"/> class.
        /// </summary>
        /// <param name="setter">The getter.</param>
        /// <param name="getter">The setter.</param>
        public UnsolvableFunction(Action<double> setter, Func<double> getter)
        {
            _getter = getter;
            _setter = setter;
        }

        /// <inheritdoc/>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
            => new RowEquation(row, solver, _getter);

        /// <inheritdoc/>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
        }

        /// <inheritdoc/>
        public override bool Resolve(double value)
        {
            _setter(value);
            return true;
        }
    }
}
