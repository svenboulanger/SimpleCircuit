using System;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Event arguments that can be used when the path command has changed.
    /// </summary>
    public class PathCommandAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the start point.
        /// </summary>
        public Vector2 Start { get; }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        public Vector2 End { get; }

        /// <summary>
        /// Gets the start normal.
        /// </summary>
        public Vector2 StartNormal { get; }

        /// <summary>
        /// Gets the end normal.
        /// </summary>
        public Vector2 EndNormal { get; }

        /// <summary>
        /// Creates a new <see cref="PathCommandAddedEventArgs"/>.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="startNormal">The start normal.</param>
        /// <param name="endNormal">The end normal.</param>
        public PathCommandAddedEventArgs(Vector2 start, Vector2 end, Vector2 startNormal, Vector2 endNormal)
        {
            Start = start;
            End = end;
            StartNormal = startNormal;
            EndNormal = endNormal;
        }
    }
}
