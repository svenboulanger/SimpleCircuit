using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;
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
            inst.Variants.Add(Instance.Msb);
            return inst;
        }

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ScaledOrientedDrawable(name)
        {
            private readonly List<string[]> _bits = [];
            private int _maxWidth;

            /// <inheritdoc />
            public override string Type => "bit";

            /// <summary>
            /// The variant for when the full array should be filled.
            /// </summary>
            public const string Full = "full";

            /// <summary>
            /// The variant for when the most-significant bit should come first.
            /// </summary>
            public const string Msb = "msb";

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
                            if (Separator == null)
                                _bits.Add(["0"]);
                            else if (Separator.Length == 0)
                                _bits.Add(label.Select(c => c.ToString()).ToArray());
                            else
                                _bits.Add(Regex.Split(label, Separator));
                            _maxWidth = Math.Max(_maxWidth, _bits[^1].Length);
                        }
                        if (_bits.Count == 0)
                            _bits.Add(["0"]);
                        if (_maxWidth == 0)
                            _maxWidth = 1;

                        double hw = 0.5 * BlockSize;

                        // Left pin
                        Pins.Clear();

                        // Left
                        for (int i = 0; i < _bits.Count; i++)
                            Pins.Add(new FixedOrientedPin($"left{i + 1}", $"The left pin of row {i + 1}.", this, new(-hw * _maxWidth, BlockSize * i), new(-1, 0)), $"left{i + 1}", $"l{i + 1}", $"w{i + 1}");

                        // Top
                        double x = -hw * (_maxWidth - 1);
                        int width = Variants.Contains(Full) ? _maxWidth : _bits[0].Length;
                        for (int i = 0; i < width; i++)
                        {
                            // Make a top pin
                            int bit = Variants.Contains(Msb) ? _maxWidth - i - 1 : i;
                            Pins.Add(new FixedOrientedPin($"top{bit}", $"Top pin of bit {bit}.", this, new(x, -hw), new(0, -1)), $"t{bit}", $"u{bit}");
                        }

                        // Bottom
                        width = Variants.Contains(Full) ? _maxWidth : _bits[^1].Length;
                        for (int i = 0; i < width; i++)
                        {
                            int bit = Variants.Contains(Msb) ? _maxWidth - i - 1 : i;
                            Pins.Add(new FixedOrientedPin($"bottom{bit}", $"Bottom pin of bit {bit}.", this, new(x, BlockSize * _bits.Count - hw), new(0, 1)), $"b{bit}", $"d{bit}");
                            x += BlockSize;
                        }

                        // Right
                        for (int i = 0; i < _bits.Count; i++)
                            Pins.Add(new FixedOrientedPin($"right{i + 1}", $"The right pin of row {i + 1}", this, new(hw * _maxWidth, BlockSize * i), new(1, 0)), $"right{i + 1}", $"r{i + 1}", $"e{i + 1}");
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

                double hw = 0.5 * BlockSize;
                double y = 0.0;
                for (int i = 0; i < _bits.Count; i++)
                {
                    double x = -hw * (_maxWidth - 1);
                    var bits = _bits[i];
                    for (int j = 0; j < _maxWidth; j++)
                    {
                        if (j < bits.Length || Variants.Contains(Full))
                            builder.Rectangle(x - hw, y - hw, BlockSize, BlockSize, style);
                        if (j < bits.Length)
                        {
                            var span = builder.TextFormatter.Format(bits[j], style);
                            var bounds = span.Bounds.Bounds;
                            builder.Text(span, new(x - bounds.Left - 0.5 * bounds.Width, y - bounds.Top - 0.5 * bounds.Height), TextOrientation.Transformed);
                        }
                        x += BlockSize;
                    }
                    y += BlockSize;
                }
            }
        }
    }
}
