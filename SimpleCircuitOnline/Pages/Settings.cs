namespace SimpleCircuitOnline.Pages
{
    public class Settings
    {
        /// <summary>
        /// Gets or sets the current file name.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets whether the preview should be shrunk to the viewport in X-direction.
        /// </summary>
        public bool ShrinkX { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the preview should be shrunk to the viewport in Y-direction.
        /// </summary>
        public bool ShrinkY { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the preview should be updated automatically when the script changes.
        /// </summary>
        public bool AutoUpdate { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the bounds of elements should be included in the preview.
        /// </summary>
        public bool RenderBounds { get; set; } = true;
    }
}
