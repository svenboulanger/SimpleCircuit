using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;

namespace SimpleCircuit
{
    /// <summary>
    /// A direct contribution for an unknown.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class DirectContribution : IContribution
    {
        private readonly int _index;
        private readonly UnknownTypes _type;
        private readonly Element<double> _d;

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row { get; }

        /// <inheritdoc/>
        public HashSet<int> Unknowns => new HashSet<int>() { _index };

        /// <inheritdoc/>
        public ISparseSolver<double> Solver { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectContribution"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The row index.</param>
        /// <param name="index">The column index.</param>
        /// <param name="type">The type of unknown.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solver"/> is <c>null</c>.</exception>
        public DirectContribution(ISparseSolver<double> solver, int row, int index, UnknownTypes type)
        {
            Solver = solver;
            _d = solver.GetElement(new MatrixLocation(row, index));
            Row = row;
            _index = index;
            _type = type;
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            _d.Add(derivative);
            rhs?.Add(derivative * Value);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            Value = solution[_index];
            switch (_type)
            {
                case UnknownTypes.Angle: Value = Utility.Wrap(Value); break;
                case UnknownTypes.Length: Value = Math.Min(Value, 0); break;
            }
        }
    }
}
