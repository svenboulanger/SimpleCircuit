using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// The Atan2 function on normals.
    /// </summary>
    /// <seealso cref="Function" />
    public class NormalAtan2 : Function
    {
        private readonly Function _nx, _ny;
        private class RowEquation : IRowEquation
        {
            private readonly IRowEquation _x, _y;
            private readonly Element<double> _rhs;
            private double _sq;
            public double Value { get; private set; }
            public RowEquation(IRowEquation x, IRowEquation y, ISparseSolver<double> solver, int row)
            {
                _x = x;
                _y = y;
                _rhs = solver.GetElement(row);
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs == null)
                {
                    rhs = _rhs;
                    rhs.Subtract(derivative * Value);
                }
                _x.Apply(-derivative * _y.Value / _sq, rhs);
                _y.Apply(derivative * _x.Value / _sq, rhs);
            }
            public void Update()
            {
                _x.Update();
                _y.Update();
                Value = Math.Atan2(_y.Value, _x.Value);
                _sq = _x.Value * _x.Value + _y.Value * _y.Value;
            }
        }

        /// <inheritdoc/>
        public override double Value => Math.Atan2(_ny.Value, _nx.Value);

        /// <inheritdoc/>
        public override bool IsConstant => _nx.IsConstant && _ny.IsConstant;

        /// <summary>
        /// Initializes a new instance of the <see cref="Atan2"/> class.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <param name="x">The x.</param>
        public NormalAtan2(Function ny, Function nx)
        {
            _nx = nx ?? throw new ArgumentNullException(nameof(nx));
            _ny = ny ?? throw new ArgumentNullException(nameof(ny));
        }

        /// <inheritdoc/>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            var sq = new Squared(_nx) + new Squared(_ny);
            if (coefficient == null)
            {
                _nx.Differentiate(-_ny / sq, equations);
                _ny.Differentiate(_nx / sq, equations);
            }
            else
            {
                _nx.Differentiate(-coefficient * _ny / sq, equations);
                _ny.Differentiate(coefficient * _nx / sq, equations);
            }
        }

        /// <inheritdoc/>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
        {
            var x = _nx.CreateEquation(row, mapper, solver);
            var y = _ny.CreateEquation(row, mapper, solver);
            return new RowEquation(x, y, solver, row);
        }

        /// <inheritdoc/>
        public override bool Resolve(double value)
        {
            var rc = _nx.Resolve(Math.Cos(value));
            var rs = _ny.Resolve(Math.Sin(value));
            return rc || rs;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Atan2({_ny}, {_nx})";
    }
}
