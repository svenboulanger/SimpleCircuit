using SimpleCircuit.Algebra;
using SimpleCircuit.Variables;
using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// A simple constraint for a variable.
    /// </summary>
    /// <seealso cref="IConstraint" />
    public class VariableConstraint : IConstraint
    {
        private readonly IVariable _variable;
        private readonly double _value;
        private Element<double> _rhs;
        private VariableContribution _dv;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableConstraint"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variable"/> is <c>null</c>.</exception>
        public VariableConstraint(VariableContribution contribution, double value)
        {
            _variable = variable ?? throw new ArgumentNullException(nameof(variable));
            _value = value;
        }

        void Setup(ISparseSolver<double> solver, int row, VariableMap variables)
        {
            _dv = 
        }

        /// <summary>
        /// Applies the constraint.
        /// </summary>
        public void Apply()
        {
            _dv.Add(1);
            _rhs?.Add(_value);
        }

        /// <summary>
        /// Creates the variables.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public void GetVariables(VariableMap variables)
        {
        }

        /// <summary>
        /// Sets up the elements necessary for the constraint.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The current row.</param>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            _dv = _variable.GetDerivative(solver, row);
            if (!_value.Equals(0.0))
                _rhs = solver.GetElement(row);
            else
                _rhs = null;
        }

        public override string ToString()
        {
            return $"Fixes variable '{_variable}' of '{_variable.Owner}' to {_value}.";
        }
    }
}
