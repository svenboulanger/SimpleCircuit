namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// An expandable line.
    /// </summary>
    public class ExpandableLine
    {
        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        public double Minimum { get; private set; } = double.MaxValue;

        /// <summary>
        /// Get the maximum value.
        /// </summary>
        public double Maximum { get; private set; } = double.MinValue;

        /// <summary>
        /// Expands the line.
        /// </summary>
        /// <param name="value">The value to include.</param>
        public void Expand(double value)
        {
            if (value < Minimum)
                Minimum = value;
            if (value > Maximum)
                Maximum = value;
        }

        /// <inheritdoc />
        public override string ToString() => $"({Minimum}; {Maximum})";
    }
}
