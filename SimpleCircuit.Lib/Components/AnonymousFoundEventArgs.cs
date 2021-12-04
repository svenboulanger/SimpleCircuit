using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Event arguments used when an anonymous drawable name has been found.
    /// </summary>
    public class AnonymousFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the key of the new drawable.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets or sets the new name of the anonymous method.
        /// </summary>
        public string NewName { get; set; }

        /// <summary>
        /// Creates new event arguments.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        public AnonymousFoundEventArgs(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }
    }
}
