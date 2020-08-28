using SimpleCircuit.Contributions;
using SimpleCircuit.Contributions.Contributors;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Represents the pin of an <see cref="IComponent"/>.
    /// </summary>
    /// <seealso cref="IItem" />
    public interface IPin
    {
        /// <summary>
        /// Gets the parent of the pin.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        IComponent Parent { get; }

        /// <summary>
        /// Gets the x-coordinate of the pin.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        IContributor X { get; }

        /// <summary>
        /// Gets the y-coordinate of the pin.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        IContributor Y { get; }

        /// <summary>
        /// Determines whether the pin is identified by the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the pin can be identified with the specified name; otherwise, <c>false</c>.
        /// </returns>
        bool Is(string name, IEqualityComparer<string> comparer = null);

        /// <summary>
        /// Projections the specified normal.
        /// </summary>
        /// <param name="normal">The normal.</param>
        /// <returns>The contributor that gets the projection on the normal.</returns>
        IContributor Projection(Vector2 normal);
    }
}
