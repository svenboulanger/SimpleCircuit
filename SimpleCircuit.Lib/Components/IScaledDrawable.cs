﻿namespace SimpleCircuit.Components
{
    /// <summary>
    /// A drawable that supports scaling.
    /// </summary>
    public interface IScaledDrawable : IDrawable, ITransformingDrawable
    {
        /// <summary>
        /// Gets or sets the scale of the drawable.
        /// </summary>
        public double Scale { get; set; }
    }
}
