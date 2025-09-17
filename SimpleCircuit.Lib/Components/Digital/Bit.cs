using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A bit vector.
    /// </summary>
    [Drawable("BIT", "A bit vector.", "Digital", "bits literal binary bin")]
    public class Bit : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var inst = new Instance(name);
            return inst;
        }

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ScaledOrientedDrawable(name)
        {
            private readonly List<string[]> _bits = [];
            private int _maxWidth = 0;

            /// <inheritdoc />
            public override string Type => "bit";

            [Description("The separator for the bit vector (default is an empty string, for which every character is a new bit).")]
            public string Separator { get; set; } = "";

            [Description("The block size for a single bit.")]
            public double BlockSize { get; set; } = 8;

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        // Update the pins
                        _maxWidth = 0;
                        for (int i = 0; i < Labels.Count; i++)
                        {
                            string label = Labels[i].Value;
                            if (label is null)
                                continue;
                            if (Separator is null)
                                _bits.Add(["0"]);
                            else if (Separator.Length == 0)
                                _bits.Add([.. label.Select(c => c.ToString())]);
                            else
                                _bits.Add(Regex.Split(label, Separator));
                            _maxWidth = Math.Max(_maxWidth, _bits[^1].Length);
                        }
                        if (_bits.Count == 0)
                            _bits.Add(["0"]);

                        double hw = 0.5 * BlockSize;

                        // Left pin
                        Pins.Clear();

                        // Left
                        for (int i = 0; i < _bits.Count; i++)
                            Pins.Add(new FixedOrientedPin($"left{i}", $"The left pin of row {i + 1}.", this, new((_maxWidth - _bits[i].Length) * BlockSize, BlockSize * i + hw), new(-1, 0)), $"left{i}", $"l{i}", $"w{i}");

                        // Top
                        int row = 0, col = 0;
                        while (row < _bits.Count)
                        {
                            while (col < _bits[row].Length)
                            {
                                Pins.Add(new FixedOrientedPin($"top{col}", $"The top pin of column {col + 1}.", this, new(BlockSize * col + hw, BlockSize * row), new(0, -1)), $"top{col}", $"t{col}", $"n{col}");
                                col++;
                            }
                            while (row < _bits.Count && col >= _bits[row].Length)
                                row++;
                        }

                        // Bottom
                        row = _bits.Count - 1; col = 0;
                        while (row >= 0)
                        {
                            while (col < _bits[row].Length)
                            {
                                Pins.Add(new FixedOrientedPin($"bottom{col}", $"The bottom pin of column {col + 1}.", this, new(BlockSize * col + hw, BlockSize * (row + 1)), new(0, 1)), $"bottom{col}", $"b{col}", $"s{col}");
                                col++;
                            }
                            while (row >= 0 && col >= _bits[row].Length)
                                row--;
                        }

                        // Right
                        for (int i = 0; i < _bits.Count; i++)
                            Pins.Add(new FixedOrientedPin($"right{i}", $"The right pin of row {i + 1}", this, new(BlockSize * _bits[i].Length, BlockSize * i + hw), new(1, 0)), $"right{i}", $"r{i}", $"e{i}");

                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                // Update the pins
                if (_bits == null)
                    return;

                var style = builder.Style.ModifyDashedDotted(this);
                for (int row = 0; row < _bits.Count; row++)
                {
                    var bits = _bits[row];
                    for (int col = 0; col < bits.Length; col++)
                    {
                        builder.Rectangle(BlockSize * col, BlockSize * row, BlockSize, BlockSize, style);
                        var span = builder.TextFormatter.Format(bits[col], style);
                        builder.Text(span, new Vector2(BlockSize * (col + 0.5), BlockSize * (row + 0.5)) - new Vector2(span.Bounds.Bounds.Center.X, -style.FontSize * 0.5), Vector2.UX, TextOrientationType.Transformed);
                    }
                }
            }
        }
    }
}
