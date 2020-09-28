using SimpleCircuit.Algebra;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that will result in a scalar value.
    /// </summary>
    public abstract class Function
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public abstract double Value { get; }

        /// <summary>
        /// Gets a flag that shows whether or not the function is a constant value.
        /// </summary>
        /// <value>
        ///   <c>true</c> the function is constant; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsConstant { get; }

        /// <summary>
        /// Sets up the function for the specified solver.
        /// </summary>
        /// <param name="coefficient">The coefficient.</param>
        /// <param name="mapper">The unknown mapper.</param>
        /// <param name="equations">The produced equations for each unknown.</param>
        public abstract void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations);

        /// <summary>
        /// Creates a row equation at the specified row, in the specified solver.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="solver">The solver.</param>
        /// <returns>The row equation.</returns>
        public abstract IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver);

        /// <summary>
        /// Tries to resolve unknowns.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if one ore more unknowns were resolved; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool Resolve(double value);

        public static Function operator -(Function a)
        {
            if (a.IsConstant)
                return new ConstantFunction(-a.Value);
            return new Negative(a);
        }
        public static Function operator +(Function a, Function b)
        {
            if (a.IsConstant)
            {
                if (b.IsConstant)
                    return new ConstantFunction(a.Value + b.Value);
                if (a.Value.IsZero())
                    return b;
            }
            else if (b.IsConstant)
            {
                if (b.Value.IsZero())
                    return a;
            }
            return new Addition(a, b);
        }
        public static Function operator -(Function a, Function b)
        {
            if (a.IsConstant)
            {
                if (b.IsConstant)
                    return new ConstantFunction(a.Value - b.Value);
                if (a.Value.IsZero())
                    return -b;
            }
            else if (b.IsConstant)
            {
                if (b.Value.IsZero())
                    return a;
            }
            return new Subtraction(a, b);
        }
        public static Function operator *(Function a, Function b)
        {
            if (a.IsConstant && b.IsConstant)
                return new ConstantFunction(a.Value * b.Value);
            if (a.IsConstant)
            {
                if (a.Value.IsZero())
                    return 0.0;
                if ((a.Value - 1).IsZero())
                    return b;
            }
            if (b.IsConstant)
            {
                if (b.Value.IsZero())
                    return 0.0;
                if ((b.Value - 1).IsZero())
                    return a;
            }
            return new Multiplication(a, b);
        }
        public static Function operator /(Function a, Function b)
        {
            if (a.IsConstant)
            {
                if (b.IsConstant)
                    return new ConstantFunction(a.Value / b.Value);
                if (a.Value.IsZero())
                    return 0.0;
            }
            else if (b.IsConstant)
            {
                if ((b.Value - 1).IsZero())
                    return a;
            }
            return new Division(a, b);
        }
        public static implicit operator Function(double value)
            => new ConstantFunction(value);
        public static implicit operator Function(Unknown unknown)
        {
            if (unknown.IsFixed)
                return new ConstantFunction(unknown.Value);
            return new UnknownFunction(unknown);
        }
    }
}
