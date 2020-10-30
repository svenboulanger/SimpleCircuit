using System;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Function flags.
    /// </summary>
    [Flags]
    public enum FunctionFlags
    {
        /// <summary>
        /// Nothing special...
        /// </summary>
        None = 0x00,

        /// <summary>
        /// A fixed value (it contains unknowns, but the variables are all fixed).
        /// </summary>
        Fixed = 0x01,

        /// <summary>
        /// A constant value (there are no unknowns).
        /// </summary>
        Constant = 0x02,

        /// <summary>
        /// The result is an angle.
        /// </summary>
        Angle = 0x04,
    }
}
