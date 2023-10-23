using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Parser.Markers
{
    /// <summary>
    /// Token types for parsing markers.
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
