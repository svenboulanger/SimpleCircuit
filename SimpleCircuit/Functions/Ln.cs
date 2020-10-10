using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Logarithmic function.
    /// </summary>
    /// <seealso cref="Function" />
    public class Ln : Function
    {
        private readonly Function _a;
        private class RowEquation : IRowEquation
        {
            private readonly IRowEquation _a;
            private readonly Element<double> _rhs;
            public double Value { get; private set; }
            public RowEquation(IRowEquation a, ISparseSolver<double> solver, int row)
            {
                _a = a;
                _rhs = solver.GetElement(row);
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs == null)
                {
                    rhs = _rhs;
                    rhs.Subtract(derivative * Value);
                }
                _a.Apply(derivative / _a.Value, _rhs);
            }
            public void Update()
            {
                _a.Update();
                Value = Math.Log(_a.Value);
            }
        }

        public override double Value => Math.Log(_a.Value);

        public Ln(Function a)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
        }

        public override bool IsConstant => _a.IsConstant;

        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
            => new RowEquation(_a.CreateEquation(row, mapper, solver), solver, row);

        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            if (_a.IsConstant)
                return;
            if (coefficient == null)
                _a.Differentiate(1.0 / _a, equations);
            else
                _a.Differentiate(coefficient / _a, equations);
        }

        public override bool Resolve(double value)
            => _a.Resolve(Math.Exp(value));

        public override string ToString() => $"ln({_a})";
    }
}
