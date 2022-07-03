namespace SimpleCircuit.Components
{
    /// <summary>
    /// The possible results returned by a circuit presence.
    /// </summary>
    public enum PresenceResult
    {
        /// <summary>
        /// The presence has successfully completed.
        /// </summary>
        Success,

        /// <summary>
        /// The presence could not completely finish and possibly requires information from
        /// other presences.
        /// </summary>
        Incomplete,

        /// <summary>
        /// The presence has given up.
        /// </summary>
        GiveUp
    }
}
