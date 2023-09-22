using SimpleCircuit.Drawing;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A class for managing the labels of a component.
    /// </summary>
    public class Labels
    {
        /// <summary>
        /// Class name for labels.
        /// </summary>
        public const string LabelClass = "lbl";

        private readonly int _max;
        private readonly List<string> _labels;
        private readonly List<Vector2> _offsets;

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
                if (index < 0 || index >= _max)
                    return;
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
        /// Gets the maximum number of labels that can be supported.
        /// </summary>
        public int Maximum => _max;

        /// <summary>
        /// Creates a new <see cref="Labels"/> collection.
        /// </summary>
        /// <param name="maximum">The maximum.</param>
        public Labels(int maximum = 1)
        {
            _max = maximum;
            _labels = new();
            _offsets = new();
        }

        /// <summary>
        /// Gets the offset of the label at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The offset of the label.</returns>
        public Vector2 GetOffset(int index)
        {
            if (index < 0 || index > _offsets.Count)
                return default;
            return _offsets[index];
        }

        /// <summary>
        /// Sets the offset of the label at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The offset.</param>
        public void SetOffset(int index, Vector2 value)
        {
            if (index < 0 || index > _max)
                return;
            while (_offsets.Count <= index)
                _offsets.Add(default);
            _offsets[index] = value;
        }

        /// <summary>
        /// Draws the label at the specified index.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="index">The index.</param>
        /// <param name="location">The location.</param>
        /// <param name="expand">The expansion direction.</param>
        /// <param name="options">The graphic options.</param>
        public void Draw(SvgDrawing drawing, int index, Vector2 location, Vector2 expand, GraphicOptions options = null)
        {
            if (index < 0 || index >= _labels.Count)
                return;
            if (index < _offsets.Count)
                location += _offsets[index];

            // Apply some default options if nothing is given
            if (options == null)
            {
                string c = LabelClass;
                if (index > 0)
                    c += (index + 1).ToString();
                options = new GraphicOptions(c);
            }

            drawing.Text(_labels[index], location, expand, options);
        }
    }
}
