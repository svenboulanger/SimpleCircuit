using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An MOS transistor.
    /// </summary>
    [Drawable(new[] { "MN", "NMOS" }, "An n-type mosfet. The bulk connection is optional.", new[] { "Analog" })]
    [Drawable(new[] { "MP", "PMOS" }, "A p-type mosfet. The bulk connection is optional.", new[] { "Analog" })]
    public class Mofset : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            switch (key)
            {
                case "MN":
                case "NMOS":
                    return new Nmos(name, options);
                case "MP":
                case "PMOS":
                    return new Pmos(name, options);
                default:
                    throw new ArgumentException($"Could not recognize key '{key}' for a mosfet.");
            }
        }

        private class Nmos : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the transistor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "nmos";

            public Nmos(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("source", "The source.", this, new Vector2(-4, 0), new Vector2(-1, 0)), "s", "source");
                Pins.Add(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 6), new Vector2(0, 1)), "g", "gate");
                Pins.Add(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 4), new Vector2(0, -1)), "b", "bulk");
                Pins.Add(new FixedOrientedPin("drain", "The drain", this, new Vector2(4, 0), new Vector2(1, 0)), "d", "drain");

                PinUpdate = Variant.Map("packaged", UpdatePins);
                DrawingVariants = Variant.If("packaged").Then(DrawPackaged).Else(DrawRegular);

                if (options?.PackagedTransistors ?? false)
                    AddVariant("packaged");
            }

            private void DrawRegular(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-4, 0), new(-8, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(0, 6), new(0, 8), new("wire"));
                if (Pins[3].Connections == 0)
                    drawing.Line(new(4, 0), new(8, 0), new("wire"));

                // Gate
                drawing.Path(b => b.MoveTo(-6, 4).LineTo(6, 4).MoveTo(-6, 6).LineTo(6, 6), new("gate"));

                // Source and drain
                drawing.Line(new(-4, 0), new(-4, 4), new("source"));
                drawing.Line(new(4, 0), new(4, 4), new("drain"));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                {
                    if (Pins["b"].Connections > 0)
                        drawing.Text(Label, new(-3, -3), new(-1, -1));
                    else
                        drawing.Text(Label, new(0, -3), new(0, -1));
                }
            }
            private void DrawPackaged(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-5, 0), new(-8, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(0, 6), new(0, 8), new("wire"));
                if (Pins[3].Connections == 0)
                    drawing.Line(new(5, 0), new(8, 0), new("wire"));

                // Gate
                drawing.Path(b => b.MoveTo(-6, 6).LineTo(6, 6)
                    .MoveTo(-7, 4).LineTo(-4, 4)
                    .MoveTo(-2, 4).LineTo(2, 4)
                    .MoveTo(4, 4).LineTo(7, 4), new("gate"));

                // Drain, source and gate
                drawing.Line(new(-5, 0), new(-5, 4), new("source"));
                drawing.Line(new(5, 0), new(5, 4), new("drain"));
                drawing.Polyline(new Vector2[] { new(-5, 0), new(0, 0), new(0, 4) }, new("bulk") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });

                // Packaged
                drawing.Circle(new(0, 3), 8.0);

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(3, -10), new(1, 1));
            }
            private void UpdatePins(bool packaged)
            {
                void SetPin(int index, Vector2 offset)
                {
                    ((FixedOrientedPin)Pins[index]).Offset = offset;
                }
                if (packaged)
                {
                    SetPin(0, new(-5, 0));
                    SetPin(3, new(5, 0));
                }    
                else
                {
                    SetPin(0, new(-4, 0));
                    SetPin(3, new(4, 0));
                }
            }
        }
        private class Pmos : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the transistor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "pmos";

            public Pmos(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("drain", "The drain", this, new Vector2(4, 0), new Vector2(1, 0)), "d", "drain");
                Pins.Add(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 8), new Vector2(0, 1)), "g", "gate");
                Pins.Add(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 0), new Vector2(0, -1)), "b", "bulk");
                Pins.Add(new FixedOrientedPin("source", "The source.", this, new Vector2(-4, 0), new Vector2(-1, 0)), "s", "source");

                PinUpdate = Variant.Map("packaged", UpdatePins);
                DrawingVariants = Variant.If("packaged").Then(DrawPackaged).Else(DrawRegular);

                if (options?.PackagedTransistors ?? false)
                    AddVariant("packaged");
            }
            private void DrawRegular(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(4, 0), new(8, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(0, 7), new(0, 9), new("wire"));
                if (Pins[3].Connections == 0)
                    drawing.Line(new(-4, 0), new(-8, 0), new("wire"));

                // The gate
                drawing.Path(b => b.MoveTo(0, 11).LineTo(0, 9)
                    .MoveTo(-6, 6).LineTo(6, 6)
                    .MoveTo(-6, 4).LineTo(6, 4), new("gate"));
                drawing.Circle(new Vector2(0, 7.5), 1.5);

                // Source and drain
                drawing.Line(new(-4, 0), new(-4, 4), new("source"));
                drawing.Line(new(4, 0), new(4, 4), new("drain"));

                // Label
                if (Pins["b"].Connections > 0)
                {
                    drawing.Line(new Vector2(0, 4), new Vector2(0, 0));
                    if (!string.IsNullOrEmpty(Label))
                        drawing.Text(Label, new Vector2(-3, -3), new Vector2(-1, -1));
                }
                else if (!string.IsNullOrEmpty(Label))
                    drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));
            }
            private void DrawPackaged(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(5, 0), new(8, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(0, 6), new(0, 8), new("wire"));
                if (Pins[3].Connections == 0)
                    drawing.Line(new(-5, 0), new(-8, 0), new("wire"));

                // Gate
                drawing.Path(b => b.MoveTo(-6, 6).LineTo(6, 6)
                    .MoveTo(-7, 4).LineTo(-4, 4)
                    .MoveTo(-2, 4).LineTo(2, 4)
                    .MoveTo(4, 4).LineTo(7, 4), new("gate"));

                // Drain, source and gate
                drawing.Line(new(-5, 0), new(-5, 4), new("source"));
                drawing.Line(new(5, 0), new(5, 4), new("drain"));
                drawing.Arrow(new(0, 4), new(0, 0), new("bulk"));
                drawing.Line(new(0, 0), new(-5, 0), new("bulk"));

                // Packaged
                drawing.Circle(new(0, 3), 8.0);

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(3, -10), new(1, 1));
            }
            private void UpdatePins(bool packaged)
            {
                void SetPin(int index, Vector2 offset)
                {
                    ((FixedOrientedPin)Pins[index]).Offset = offset;
                }
                if (packaged)
                {
                    SetPin(0, new(5, 0));
                    SetPin(1, new(0, 6));
                    SetPin(3, new(-5, 0));
                }
                else
                {
                    SetPin(0, new(4, 0));
                    SetPin(1, new(0, 9));
                    SetPin(3, new(-4, 0));
                }
            }
        }
    }
}
