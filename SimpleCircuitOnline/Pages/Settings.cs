namespace SimpleCircuitOnline.Pages
{
    /// <summary>
    /// Settings.
    /// </summary>
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
        /// Gets or sets whether the viewed item should be in dark mode.
        /// </summary>
        public bool ViewDarkMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the dark mode figure should be exported.
        /// </summary>
        public bool ExportDarkMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the light mode figure should be exported.
        /// </summary>
        public bool ExportLightMode { get; set; } = true;
    }
}
