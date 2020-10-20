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
        /// Gets a flag that shows whether or not the function has been fixed to a certain value.
        /// </summary>
        /// <value>
        ///   <c>true</c> the function is constant; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsFixed { get; }

        /// <summary>
        /// Gets a flag that shows whether or not the function is constant.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the function doesn't contain any unknowns; otherwise, <c>false</c>.
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

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Function operator -(Function a)
        {
            if (a.IsFixed)
                return new ConstantFunction(-a.Value);
            return new Negative(a);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Function operator +(Function a, Function b)
        {
            if (a.IsFixed)
            {
                if (b.IsFixed)
                    return new ConstantFunction(a.Value + b.Value);
                if (a.Value.IsZero())
                    return b;
            }
            else if (b.IsFixed)
            {
                if (b.Value.IsZero())
                    return a;
            }
            return new Addition(a, b);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Function operator -(Function a, Function b)
        {
            if (a.IsFixed)
            {
                if (b.IsFixed)
                    return new ConstantFunction(a.Value - b.Value);
                if (a.Value.IsZero())
                    return -b;
            }
            else if (b.IsFixed)
            {
                if (b.Value.IsZero())
                    return a;
            }
            return new Subtraction(a, b);
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Function operator *(Function a, Function b)
        {
            if (a.IsFixed && b.IsFixed)
                return new ConstantFunction(a.Value * b.Value);
            if (a.IsFixed)
            {
                if (a.Value.IsZero())
                    return 0.0;
                if ((a.Value - 1).IsZero())
                    return b;
            }
            if (b.IsFixed)
            {
                if (b.Value.IsZero())
                    return 0.0;
                if ((b.Value - 1).IsZero())
                    return a;
            }
            return new Multiplication(a, b);
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Function operator /(Function a, Function b)
        {
            if (a.IsFixed)
            {
                if (b.IsFixed)
                    return new ConstantFunction(a.Value / b.Value);
                if (a.Value.IsZero())
                    return 0.0;
            }
            else if (b.IsFixed)
            {
                if ((b.Value - 1).IsZero())
                    return a;
            }
            return new Division(a, b);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Double"/> to <see cref="Function"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Function(double value)
            => new ConstantFunction(value);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Unknown"/> to <see cref="Function"/>.
        /// </summary>
        /// <param name="unknown">The unknown.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Function(Unknown unknown)
        {
            if (unknown.IsFixed)
                return new ConstantFunction(unknown.Value);
            return new UnknownFunction(unknown);
        }
    }
}
