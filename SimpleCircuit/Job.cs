namespace SimpleCircuit
{
    /// <summary>
    /// A job for conversion of a SimpleCircuit script to SVG.
    /// </summary>
    public class Job
    {
        /// <summary>
        /// Gets or sets the directory of the input file.
        /// </summary>
        public string InputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the filename of the input.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets the directory of the output.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the filename of the output.
        /// </summary>
        public string Output { get; set; }
    }
}
