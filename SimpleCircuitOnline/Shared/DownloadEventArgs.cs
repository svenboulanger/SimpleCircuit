using System;

namespace SimpleCircuitOnline.Shared
{
    /// <summary>
    /// Event arguments for a download event.
    /// </summary>
    /// <remarks>
    /// Creates a new download event argument.
    /// </remarks>
    /// <param name="type"></param>
    public class DownloadEventArgs(DownloadEventArgs.Types type) : EventArgs
    {
        /// <summary>
        /// Possible export types.
        /// </summary>
        public enum Types
        {
            Svg,
            Png,
            Jpeg,
            Link
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public Types Type { get; } = type;
    }
}
