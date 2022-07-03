namespace SimpleCircuit.Components
{
    /// <summary>
    /// The possible modes for presences to run in.
    /// </summary>
    public enum PresenceMode
    {
        /// <summary>
        /// The presence is run for the first time.
        /// </summary>
        Normal,

        /// <summary>
        /// The solver only has presences that require the circuit to be fixed.
        /// </summary>
        Fix,

        /// <summary>
        /// The solver will give up and the presence has a last chance to output diagnostics.
        /// </summary>
        GiveUp
    }
}
