﻿using SimpleCircuit.Circuits.Contexts;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A class for managing the labels of a component.
    /// </summary>
    public class Labels : IReadOnlyList<LabelInfo>
    {
        private readonly List<LabelInfo> _labels = [];

        /// <summary>
        /// Gets or sets the default text size.
        /// </summary>
        public double FontSize { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the default line spacing for labels.
        /// </summary>
        public double LineSpacing { get; set; } = 1.5;

        /// <inheritdoc />
        public LabelInfo this[int index]
        {
            get
            {
                if (index < 0)
                    return null;
                if (index >= _labels.Count)
                {
                    for (int i = _labels.Count; i <= index; i++)
                        _labels.Add(null);
                }

                LabelInfo result;
                if (_labels[index] == null)
                {
                    result = new LabelInfo()
                    {
                        Size = FontSize,
                        LineSpacing = LineSpacing
                    };
                    _labels[index] = result;
                }
                else
                    result = _labels[index];
                return result;
            }
        }

        /// <inheritdoc />
        public int Count => _labels.Count;

        /// <summary>
        /// Formats the labels.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Format(IPrepareContext context)
        {
            foreach (var label in _labels)
                label.Format(context);
        }

        /// <inheritdoc />
        public IEnumerator<LabelInfo> GetEnumerator() => _labels.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
