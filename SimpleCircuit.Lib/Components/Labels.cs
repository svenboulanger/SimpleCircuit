using SimpleCircuit.Drawing;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A class for managing the labels of a component.
    /// </summary>
    public class Labels
    {
        private readonly List<string> _labels;
        private readonly List<Placement> _placements;

        /// <summary>
        /// Gets the default graphic options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new GraphicOptions(LabelClass);

        /// <summary>
        /// Class name for labels.
        /// </summary>
        public const string LabelClass = "lbl";

        /// <summary>
        /// Describes the placement of a label.
        /// </summary>
        private class Placement
        {
            private Vector2? _location, _expand, _offset;
            private GraphicOptions _options;

            /// <summary>
            /// Gets or sets the default location if it is not overridden.
            /// </summary>
            public Vector2 DefaultLocation { get; set; }

            /// <summary>
            /// The location of the label, or <c>null</c> if no location is given.
            /// </summary>
            public Vector2? Location
            {
                get => _location ?? DefaultLocation;
                set => _location = value;
            }

            /// <summary>
            /// Gets or sets the default expansion direction if it is not overridden.
            /// </summary>
            public Vector2 DefaultExpand { get; set; }

            /// <summary>
            /// The expansion direction of the label, or <c>null</c> if no direction is given.
            /// </summary>
            public Vector2? Expand
            {
                get => _expand ?? DefaultExpand;
                set => _expand = value;
            }

            /// <summary>
            /// Gets or sets the offset if it is not overridden.
            /// </summary>
            public Vector2 DefaultOffset { get; set; }

            /// <summary>
            /// The offset of the label, or <c>null</c> if no offset is given.
            /// </summary>
            public Vector2? Offset
            {
                get => _offset ?? DefaultOffset;
                set => _offset = value;
            }

            /// <summary>
            /// Gets or sets the options of the label, if no options are given.
            /// </summary>
            public GraphicOptions DefaultOptions { get; set; }

            /// <summary>
            /// The graphic options for the label, or <c>null</c> if no options are given.
            /// </summary>
            public GraphicOptions Options
            {
                get => _options ?? DefaultOptions;
                set => _options = value;
            }
        }

        /// <summary>
        /// Gets or sets the label at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The label.</returns>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _labels.Count)
                    return null;
                return _labels[index];
            }
            set
            {
                while (_labels.Count <= index)
                    _labels.Add(null);
                _labels[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of labels specified.
        /// </summary>
        public int Count => _labels.Count;

        /// <summary>
        /// Creates a new <see cref="Labels"/> collection.
        /// </summary>
        public Labels()
        {
            _labels = new List<string>();
            _placements = new List<Placement>();
        }

        /// <summary>
        /// Sets the default pin placement.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <param name="location">The location.</param>
        /// <param name="expand">The expansion direction.</param>
        /// <param name="offset">The pin offset from its location.</param>
        /// <param name="options">The options for the pin.</param>
        public void SetDefaultPin(int index, Vector2? location = null, Vector2? expand = null, Vector2? offset = null, GraphicOptions options = null)
        {
            if (index < 0)
            {
                while (_placements.Count < _labels.Count)
                    _placements.Add(new Placement());

                // Set the data on all of them
                for (int i = 0; i < _placements.Count; i++)
                {
                    var placement = _placements[i];
                    if (location != null)
                        placement.DefaultLocation = location.Value;
                    if (expand != null)
                        placement.DefaultExpand = expand.Value;
                    if (offset != null)
                        placement.DefaultOffset = offset.Value;
                    if (options != null)
                        placement.DefaultOptions = options;
                }
            }
            else
            {
                while (_placements.Count <= index)
                    _placements.Add(new Placement());

                var placement = _placements[index];
                if (location != null)
                    placement.DefaultLocation = location.Value;
                if (expand != null)
                    placement.DefaultExpand = expand.Value;
                if (offset != null)
                    placement.DefaultOffset = offset.Value;
                if (options != null)
                    placement.DefaultOptions = options;
            }
        }

        /// <summary>
        /// Sets the pin information that would override the default placement.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <param name="location">The location.</param>
        /// <param name="expand">The expansion direction.</param>
        /// <param name="offset">The pin offset from its location.</param>
        /// <param name="options">The options for the pin.</param>
        public void SetPin(int index, Vector2? location = null, Vector2? expand = null, Vector2? offset = null, GraphicOptions options = null)
        {
            if (index < 0)
            {
                while (_placements.Count < _labels.Count)
                    _placements.Add(new Placement());

                // Set the data on all of them
                for (int i = 0; i < _placements.Count; i++)
                {
                    var placement = _placements[i];
                    if (location != null)
                        placement.Location = location;
                    if (expand != null)
                        placement.Expand = expand;
                    if (offset != null)
                        placement.Offset = offset;
                    if (options != null)
                        placement.Options = options;
                }
            }
            else
            {
                while (_placements.Count <= index)
                    _placements.Add(new Placement());

                var placement = _placements[index];
                if (location != null)
                    placement.Location = location;
                if (expand != null)
                    placement.Expand = expand;
                if (offset != null)
                    placement.Offset = offset;
                if (options != null)
                    placement.Options = options;
            }
        }

        /// <summary>
        /// Clears the labels.
        /// </summary>
        public void Clear()
        {
            _labels.Clear();
            _placements.Clear();
        }

        /// <summary>
        /// Draws all the labels.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        public void Draw(SvgDrawing drawing)
        {
            GraphicOptions defaultOptions = null;
            for (int i = 0; i < _labels.Count; i++)
            {
                // Determine the placement of the label
                Placement placement = null;
                if (i < _placements.Count)
                    placement = _placements[i];

                var location = placement?.Location ?? default;
                location += placement?.Offset ?? default;
                var expand = placement?.Expand ?? default;
                var options = placement?.Options ?? defaultOptions;
                if (options == null)
                    options = defaultOptions = new GraphicOptions(LabelClass);

                // Draw the label
                drawing.Text(_labels[i], location, expand, options);
            }
        }
    }
}
