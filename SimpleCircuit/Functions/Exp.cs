using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Exponential function.
    /// </summary>
    /// <seealso cref="Function" />
    public class Exp : Function
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
                _a.Apply(derivative * Value, rhs);
            }
            public void Update()
            {
                _a.Update();
                Value = Math.Exp(_a.Value);
                if (Value > 1e20)
                    Value = 1e20;
            }
        }

        public override double Value => Math.Exp(_a.Value);

        public override bool IsConstant => _a.IsConstant;

        public Exp(Function a)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
        }

        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
            => new RowEquation(_a.CreateEquation(row, mapper, solver), solver, row);

        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            if (coefficient == null)
                _a.Differentiate(new Exp(_a), equations);
            else
                _a.Differentiate(coefficient * new Exp(_a), equations);
        }

        public override bool Resolve(double value)
        {
            if (value <= 0)
                return false;
            return _a.Resolve(Math.Log(value));
        }

        public override string ToString() => $"exp({_a})";
    }
}
