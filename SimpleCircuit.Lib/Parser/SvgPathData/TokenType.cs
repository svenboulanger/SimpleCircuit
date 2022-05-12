using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Parser.SvgPathData
{
    public enum TokenType
    {
        /// <summary>
        /// End of the content.
        /// </summary>
        EndOfContent = 0,

        /// <summary>
        /// A path data command.
        /// </summary>
        Command = 0x01,

        /// <summary>
        /// A number.
        /// </summary>
        Number = 0x02,

        /// <summary>
        /// An unknown token.
        /// </summary>
        Unknown = 0x04,

        /// <summary>
        /// All tokens.
        /// </summary>
        All = -1
    }
}
