using System;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Represents an unknown.
    /// </summary>
    public class Unknown
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public UnknownTypes Type { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the value of the unknown.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this unknown has been fixed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has been fixed; otherwise, <c>false</c>.
        /// </value>
        public bool IsFixed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Unknown"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Unknown(string name, UnknownTypes type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;

        public static Function operator -(Unknown a)
            => new Negative(a);
        public static Function operator +(Unknown a, Unknown b)
            => new Addition(a, b);
        public static Function operator +(Unknown a, double b)
            => new Addition(a, b);
        public static Function operator -(Unknown a, Unknown b)
            => new Subtraction(a, b);
        public static Function operator -(Unknown a, double b)
            => new Subtraction(a, b);
        public static Function operator *(Unknown a, Unknown b)
            => new Multiplication(a, b);
        public static Function operator *(Unknown a, double b)
            => new Multiplication(a, b);
        public static Function operator /(Unknown a, Unknown b)
            => new Division(a, b);
        public static Function operator /(Unknown a, double b)
            => new Division(a, b);
    }
}
