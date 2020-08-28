using SimpleCircuit.Algebra;
using SimpleCircuit.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Constraints
{
    public class EqualConstraint : IConstraint
    {


        public EqualConstraint(VariableContribution a, VariableContribution b)
        {
        }

        public void Apply()
        {
            
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Setup(ISparseSolver<double> solver, int row, VariableMap variables)
        {
            throw new NotImplementedException();
        }
    }
}
