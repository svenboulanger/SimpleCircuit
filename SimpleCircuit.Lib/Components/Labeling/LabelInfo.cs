﻿namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Label information.
    /// </summary>
    public class LabelInfo
    {
        /// <summary>
        /// Gets or sets the value of the label.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the location index of the label that is used as the reference.
        /// </summary>
        public int? Location { get; set; }

        /// <summary>
        /// Gets or sets the offset of the label compared to the default location.
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Gets or sets the expansion direction of the label.
        /// If <c>null</c>, the default expansion direction is used.
        /// </summary>
        public Vector2? Expand { get; set; }
    }
}