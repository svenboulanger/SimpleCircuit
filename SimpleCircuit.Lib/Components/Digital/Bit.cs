using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A bit vector.
    /// </summary>
    [Drawable("BIT", "A bit vector.", "Digital")]
    public class Bit : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private string[] _bits = null;

            /// <inheritdoc />
            public override string Type => "bit";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The separator for the bit vector (default is an empty string, for which every character is a new bit).")]
            public string Separator { get; set; } = "";

            [Description("The block size for a single bit.")]
            public double BlockSize = 8;

            [Description("If true, the vector defines pin indices from right to left (default), otherwise the numbering happens from left to right.")]
            public bool MsbFirst { get; set; } = true;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                // Update the pins
                if (Labels.Count > 0)
                {
                    if (Separator == null)
                        _bits = new[] { "0" };
                    else if (Separator.Length == 0)
                        _bits = Labels[0].Select(c => c.ToString()).ToArray();
                    else
                        _bits = Regex.Split(Labels[0], Separator);
                }
                if (_bits == null || _bits.Length == 0)
                    _bits = new[] { "0" };

                double hw = 0.5 * BlockSize;
                double x = -hw * (_bits.Length - 1);

                // Left pin
                Pins.Clear();
                Pins.Add(new FixedOrientedPin("left", "The left pin", this, new(-hw * _bits.Length, 0), new(-1, 0)), "left", "l", "w");
                for (int i = 0; i < _bits.Length; i++)
                {
                    // Make a top pin
                    int bit = MsbFirst ? _bits.Length - i - 1 : i;
                    Pins.Add(new FixedOrientedPin($"top{bit}", $"Top pin of bit {bit}.", this, new(x, -hw), new(0, -1)), $"t{bit}", $"u{bit}");
                    Pins.Add(new FixedOrientedPin($"bottom{bit}", $"Bottom pin of bit {bit}.", this, new(x, hw), new(0, 1)), $"b{bit}", $"d{bit}");
                    x += BlockSize;
                }
                Pins.Add(new FixedOrientedPin("right", "The right pin", this, new(hw * _bits.Length, 0), new(1, 0)), "right", "r", "e");
                return true;
            }

            protected override void Draw(SvgDrawing drawing)
            {
                // Update the pins
                if (_bits == null)
                    return;
                double hw = 0.5 * BlockSize;
                double x = -hw * (_bits.Length - 1);
                for (int i = 0; i < _bits.Length; i++)
                {
                    drawing.Rectangle(BlockSize, BlockSize, new(x, 0));
                    drawing.Text(_bits[i], new(x, 0), new());
                    x += BlockSize;
                }
            }
        }
    }
}
