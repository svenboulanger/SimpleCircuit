using System;

namespace SimpleCircuitOnline.Shared
{
    /// <summary>
    /// Event arguments for a download event.
    /// </summary>
    public class DownloadEventArgs : EventArgs
    {
        /// <summary>
        /// Possible export types.
        /// </summary>
        public enum Types
        {
            Svg,
            Png,
            Jpeg,
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public Types Type { get; }

        /// <summary>
        /// Creates a new download event argument.
        /// </summary>
        /// <param name="type"></param>
        public DownloadEventArgs(Types type)
        {
            Type = type;
        }
    }
}
