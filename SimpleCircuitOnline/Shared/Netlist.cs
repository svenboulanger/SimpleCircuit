namespace SimpleCircuitOnline.Shared
{
    /// <summary>
    /// A struct for a netlist.
    /// </summary>
    public readonly struct Netlist
    {
        /// <summary>
        /// Gets the script part of the netlist.
        /// </summary>
        public string Script { get; }

        /// <summary>
        /// Gets the style part of the netlist.
        /// </summary>
        public string Style { get; }

        /// <summary>
        /// Creates a new <see cref="Netlist"/>.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="style">The style.</param>
        public Netlist(string script, string style)
        {
            Script = script;
            Style = style;
        }
    }
}
