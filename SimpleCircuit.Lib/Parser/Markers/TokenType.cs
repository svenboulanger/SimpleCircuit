namespace SimpleCircuit.Parser.Markers
{
    /// <summary>
    /// Token type for parsing markers.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Describes the end of content.
        /// </summary>
        EndOfContent = 0,

        /// <summary>
        /// The marker.
        /// </summary>
        Marker = 0x01,
    }
}
