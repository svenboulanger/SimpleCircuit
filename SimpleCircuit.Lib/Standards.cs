using System;

namespace SimpleCircuit
{
    /// <summary>
    /// Enumerates the different styles.
    /// </summary>
    [Flags]
    public enum Standards
    {
        /// <summary>
        /// Uses the native style of SimpleCircuit.
        /// </summary>
        Native = 0x00,

        /// <summary>
        /// AREI style (Algemeen Reglement op de Elektrische Installaties).
        /// </summary>
        AREI = 0x01,

        /// <summary>
        /// ANSI style (American National Standards Institute).
        /// </summary>
        ANSI = 0x02,

        /// <summary>
        /// IEC style (International Electrotechnical Commission).
        /// </summary>
        IEC = 0x04
    }
}
