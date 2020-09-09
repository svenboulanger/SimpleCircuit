using SimpleCircuit.Algebra;
using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Contributors
{
    /// <summary>
    /// An instance that can contribute to the Newton-Raphson method.
    /// </summary>
    public abstract class Contributor
    {
        /// <summary>
        /// Gets the type of contributor.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public abstract UnknownTypes Type { get; }

        /// <summary>
        /// Gets the value of the last created contribution.
        /// </summary>
        public abstract double Value { get; }

        /// <summary>
        /// Gets whether or not the contributor has a fixed value.
        /// </summary>
        public abstract bool IsFixed { get; }

        /// <summary>
        /// Fixes the contributor to a fixed value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the fix was succesful.</returns>
        public abstract bool Fix(double value);

        /// <summary>
        /// Resets the contributor.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Gets the unknowns that this contributor is using.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>The indexes of the variables that are being used.</returns>
        public abstract IEnumerable<int> GetUnknowns(UnknownSolverMap map);

        /// <summary>
        /// Creates a contribution for the contributor at the specified row.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The row.</param>
        /// <param name="map">The unknown solver map.</param>
        /// <returns>The contribution.</returns>
        public abstract IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map);

        public static Contributor operator -(Contributor a) => new MultipliedContributor(new ConstantContributor(UnknownTypes.Scalar, -1.0), a, a.Type);
        public static Contributor operator +(Contributor a, Contributor b) => new AddedContributor(a, b, a.Type);
        public static Contributor operator -(Contributor a, Contributor b) => a + (-b);
        public static Contributor operator *(Contributor a, Contributor b)
        {
            UnknownTypes type;
            if (a.Type == UnknownTypes.Scalar)
                type = b.Type;
            else if (b.Type == UnknownTypes.Scalar)
                type = a.Type;
            else
                throw new ArgumentException("Unit does not match.");
            return new MultipliedContributor(a, b, type);
        }
        public static Contributor operator +(Contributor a, double b) => new AddedContributor(a, new ConstantContributor(a.Type, b), a.Type);
        public static Contributor operator -(Contributor a, double b) => new AddedContributor(a, -new ConstantContributor(a.Type, b), a.Type);
        public static Contributor operator +(double a, Contributor b) => new AddedContributor(new ConstantContributor(b.Type, a), b, b.Type);
        public static Contributor operator -(double a, Contributor b) => new AddedContributor(new ConstantContributor(b.Type, a), -b, b.Type);
        public static Contributor operator *(Contributor a, double b) => new MultipliedContributor(a, new ConstantContributor(UnknownTypes.Scalar, b), a.Type);
        public static Contributor operator *(double a, Contributor b) => new MultipliedContributor(new ConstantContributor(UnknownTypes.Scalar, a), b, b.Type);
    }
}
