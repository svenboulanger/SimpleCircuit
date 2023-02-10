using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Describes a tracker for <see cref="ILexer"/>.
    /// </summary>
    public readonly struct Tracker : IEquatable<Tracker>
    {
        /// <summary>
        /// Gets the index in the source.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the text location in the source.
        /// </summary>
        public TextLocation Location { get; }

        /// <summary>
        /// Creates a new tracker for lexers.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="location">The location.</param>
        public Tracker(int index, TextLocation location)
        {
            Index = index;
            Location = location;
        }

        /// <inheritdoc />
        public override int GetHashCode() => (Index * 1023) ^ Location.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Tracker && Equals((Tracker)obj);

        /// <inheritdoc />
        public bool Equals(Tracker other)
        {
            if (Index != other.Index)
                return false;
            if (!Location.Equals(other.Location))
                return false;
            return true;
        }

        /// <summary>
        /// Gets the tracker as a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => Location.ToString();
    }
}
