namespace SimpleCircuitOnline.Shared
{
    /// <summary>
    /// A struct for a netlist.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="Netlist"/>.
    /// </remarks>
    /// <param name="script">The script.</param>
    /// <param name="style">The style.</param>
    public readonly struct Netlist(string script, string style)
    {
        /// <summary>
        /// Gets the script part of the netlist.
        /// </summary>
        public string Script { get; } = script;

        /// <summary>
        /// Gets the style part of the netlist.
        /// </summary>
        public string Style { get; } = style;
    }
}
