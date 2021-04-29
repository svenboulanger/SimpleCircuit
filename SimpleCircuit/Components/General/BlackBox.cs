using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A generic black box.
    /// </summary>
    [SimpleKey("BB", "Black box")]
    public partial class BlackBox : IComponent, ITranslating, ISizeable, ILabeled
    {
        private readonly PinCollection _pins;

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        IPinCollection IComponent.Pins => _pins;

        private Unknown _northSpace, _southSpace, _eastSpace, _westSpace, _width, _height, _x, _y;

        /// <inheritdoc/>
        public Function X => _x;

        /// <inheritdoc/>
        public Function Y => _y;

        /// <inheritdoc/>
        public Function Width => _width;

        /// <inheritdoc/>
        public Function Height => _height;

        /// <summary>
        /// Creates a new black box.
        /// </summary>
        /// <param name="name">The name.</param>
        public BlackBox(string name)
        {
            Name = name;
            _pins = new PinCollection(this);
            _northSpace = new Unknown($"{name}.northspace", UnknownTypes.Length);
            _southSpace = new Unknown($"{name}.southspace", UnknownTypes.Length);
            _eastSpace = new Unknown($"{name}.eastspace", UnknownTypes.Length);
            _westSpace = new Unknown($"{name}.westspace", UnknownTypes.Length);
            _x = new Unknown($"{name}.x", UnknownTypes.X);
            _y = new Unknown($"{name}.y", UnknownTypes.Y);
            _width = new Unknown($"{name}.width", UnknownTypes.Width)
            {
                Value = 10.0
            };
            _height = new Unknown($"{name}.height", UnknownTypes.Height)
            {
                Value = 10.0
            };
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            double width = Width.Value;
            double height = Height.Value;
            drawing.Polygon(new[]
            {
                new Vector2(0, 0),
                new Vector2(width, 0),
                new Vector2(width, height),
                new Vector2(0, height),
                new Vector2(0, 0)
            });

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(width / 2, height / 2), new Vector2(0, 0));

            var elt = _pins.North;
            while (elt != null)
            {
                drawing.Text(elt.Name, new Vector2(elt.X.Value, elt.Y.Value + 2), new Vector2(0, 1));
                elt = elt.Previous;
            }
            elt = _pins.East;
            while (elt != null)
            {
                drawing.Text(elt.Name, new Vector2(elt.X.Value - 2, elt.Y.Value), new Vector2(-1, 0));
                elt = elt.Previous;
            }
            elt = _pins.South;
            while (elt != null)
            {
                drawing.Text(elt.Name, new Vector2(elt.X.Value, elt.Y.Value - 2), new Vector2(0, -1));
                elt = elt.Previous;
            }
            elt = _pins.West;
            while (elt != null)
            {
                drawing.Text(elt.Name, new Vector2(elt.X.Value + 2, elt.Y.Value), new Vector2(1, 0));
                elt = elt.Previous;
            }
        }

        /// <inheritdoc/>
        public void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(X * 1e-3) + new Squared(Y * 1e-3);

            // We basically treat this as a lot of "wires" with minimum length...
            void ApplySide(Function total, Unknown space, Pin elt)
            {
                var sum = total - space;
                var x = space - 1.0;
                minimizer.Minimize += 1e-3 * x + new Squared(1e-6 * x) + new Exp(-x);
                space.Value = 1.0;
                minimizer.AddMinimum(space, 0);
                while (elt != null)
                {
                    minimizer.AddMinimum(elt.Length, 0.0);
                    sum -= elt.Length;
                    x = elt.Length - 1.0;
                    minimizer.Minimize += 1e-3 * x + new Squared(1e-6 * x) + new Exp(-x);
                    elt.Length.Value = 1.0;
                    elt = elt.Previous;
                }
                minimizer.AddConstraint(sum);
            }

            ApplySide(Width, _northSpace, _pins.North);
            ApplySide(Width, _southSpace, _pins.South);
            ApplySide(Height, _eastSpace, _pins.East);
            ApplySide(Height, _westSpace, _pins.West);
        }
    }
}
