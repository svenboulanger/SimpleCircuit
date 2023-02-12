using System;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Enumeration of the directions along which the virtual wire acts
    /// </summary>
    [Flags]
    public enum Axis
    {
        None = 0x00,
        X = 0x01,
        Y = 0x02,
        XY = X | Y,
    }
}
