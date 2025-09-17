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

            /// <summary>
            /// The variant name for MSB-first labels.
            /// </summary>
            public const string MsbFirst = "msbfirst";

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
                        if (Variants.Contains(MsbFirst))
                        {
                            for (int i = 0; i < _bits.Count; i++)
                            {
                                for (int j = 0; j < _bits[i].Length / 2; j++)
                                    (_bits[i][j], _bits[i][_bits[i].Length - 1 - j]) = (_bits[i][_bits[i].Length - 1 - j], _bits[i][j]);
                            }
                        }

                        double hw = 0.5 * BlockSize;

                        // Left pin
                        Pins.Clear();

                        // Left
                        int row, col;
                        for (row = 0; row < _bits.Count; row++)
                            Pins.Add(new FixedOrientedPin($"left{row + 1}", $"The left pin of row {row}.", this, new((_maxWidth - _bits[row].Length) * BlockSize, BlockSize * row + hw), new(-1, 0)), $"left{row}", $"l{row}", $"w{row}");

                        // Top
                        row = 0; col = 0;
                        while (row < _bits.Count)
                        {
                            while (col < _bits[row].Length)
                            {
                                Pins.Add(new FixedOrientedPin($"top{col + 1}", $"The top pin of column {col}.", this, new(BlockSize * col + hw, BlockSize * row), new(0, -1)), $"top{col}", $"t{col}", $"n{col}");
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
                                Pins.Add(new FixedOrientedPin($"bottom{col + 1}", $"The bottom pin of column {col + 1}.", this, new(BlockSize * col + hw, BlockSize * (row + 1)), new(0, 1)), $"bottom{col}", $"b{col}", $"s{col}");
                                col++;
                            }
                            while (row >= 0 && col >= _bits[row].Length)
                                row--;
                        }

                        // Right
                        for (row = 0; row < _bits.Count; row++)
                            Pins.Add(new FixedOrientedPin($"right{row + 1}", $"The right pin of row {row + 1}", this, new(BlockSize * _bits[row].Length, BlockSize * row + hw), new(1, 0)), $"right{row}", $"r{row}", $"e{row}");

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
