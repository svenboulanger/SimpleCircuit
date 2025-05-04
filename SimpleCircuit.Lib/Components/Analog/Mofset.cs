using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Appearance;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An MOS transistor.
    /// </summary>
    [Drawable("MN", "An n-type mosfet. The bulk connection is optional.", "Analog", "packaged depletion")]
    [Drawable("NMOS", "An n-type mosfet. The bulk connection is optional.", "Analog", "packaged depletion")]
    [Drawable("MP", "A p-type mosfet. The bulk connection is optional.", "Analog", "packaged depletion")]
    [Drawable("PMOS", "A p-type mosfet. The bulk connection is optional.", "Analog", "packaged depletion")]
    public class Mofset : DrawableFactory
    {
        private const string _packaged = "packaged";
        private const string _depletion = "depletion";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            return key switch
            {
                "MN" or "NMOS" => new Nmos(name),
                "MP" or "PMOS" => new Pmos(name),
                _ => throw new ArgumentException($"Could not recognize key '{key}' for a mosfet.")
            };
        }

        private class Nmos : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public override string Type => "nmos";

            /// <summary>
            /// Creates a new <see cref="Nmos"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Nmos(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("source", "The source.", this, new Vector2(-4, 0), new Vector2(-1, 0)), "s", "source");
                Pins.Add(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 6), new Vector2(0, 1)), "g", "gate");
                Pins.Add(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 4), new Vector2(0, -1)), "b", "bulk");
                Pins.Add(new FixedOrientedPin("drain", "The drain", this, new Vector2(4, 0), new Vector2(1, 0)), "d", "drain");
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        if (Variants.Contains(_packaged))
                        {
                            SetPinOffset(0, new(-5, 0));
                            SetPinOffset(3, new(5, 0));
                        }
                        else
                        {
                            SetPinOffset(0, new(-4, 0));
                            SetPinOffset(3, new(4, 0));
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                if (Variants.Contains(_packaged))
                    DrawPackaged(builder);
                else
                    DrawRegular(builder);
            }
            private void DrawRegular(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, 4, "s", "d");
                builder.ExtendPin(Pins["g"], Appearance);

                // Gate
                builder.Path(b => b.MoveTo(new(-6, 4)).LineTo(new(6, 4)).MoveTo(new(-6, 6)).LineTo(new(6, 6)), new("gate"));

                // Source and drain
                builder.Line(new(-4, 0), new(-4, 4), Appearance);
                builder.Line(new(4, 0), new(4, 4), Appearance);

                if (Variants.Contains(_depletion))
                    builder.Rectangle(-4, 2.5, 8, 1.5, options: Appearance.CreatePathOptions(this));

                // Label
                if (Pins["b"].Connections > 0)
                {
                    _anchors[0] = new LabelAnchorPoint(new(-3, -3), new(-1, -1), Appearance);
                    _anchors[1] = new LabelAnchorPoint(new(3, -3), new(1, -1), Appearance);
                }
                else
                {
                    _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1), Appearance);
                    _anchors[1] = new LabelAnchorPoint(new(0, -3), new(0, -1), Appearance);
                }
                _anchors.Draw(builder, Labels);
            }
            private void DrawPackaged(IGraphicsBuilder drawing)
            {
                drawing.ExtendPins(Pins, Appearance, 3, "s", "d");
                drawing.ExtendPin(Pins["g"], Appearance);

                // Gate
                drawing.Path(b => b.MoveTo(new(-6, 6)).LineTo(new(6, 6))
                    .MoveTo(new(-7, 4)).LineTo(new(-4, 4))
                    .MoveTo(new(-2, 4)).LineTo(new(2, 4))
                    .MoveTo(new(4, 4)).LineTo(new(7, 4)), new("gate"));

                // Drain, source and gate
                drawing.Line(new(-5, 0), new(-5, 4), Appearance);
                drawing.Line(new(5, 0), new(5, 4), Appearance);
                drawing.Polyline([
                    new(-5, 0),
                    new(0, 0),
                    new(0, 4)
                ], Appearance);

                var marker = new Arrow(new(0, 4), new(0, 1));
                marker.Draw(drawing, Appearance);

                // Packaged
                drawing.Circle(new(0, 3), 8.0, Appearance);

                // Label
                _anchors[0] = new LabelAnchorPoint(new(3, -11), new(1, 1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(-3, -11), new(-1, 1), Appearance);
                _anchors.Draw(drawing, Labels);
            }
        }
        private class Pmos : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public override string Type => "pmos";

            /// <summary>
            /// Creates a new <see cref="Pmos"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Pmos(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("drain", "The drain", this, new Vector2(4, 0), new Vector2(1, 0)), "d", "drain");
                Pins.Add(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 9), new Vector2(0, 1)), "g", "gate");
                Pins.Add(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 0), new Vector2(0, -1)), "b", "bulk");
                Pins.Add(new FixedOrientedPin("source", "The source.", this, new Vector2(-4, 0), new Vector2(-1, 0)), "s", "source");
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        if (Variants.Contains(_packaged))
                        {
                            SetPinOffset(0, new(5, 0));
                            SetPinOffset(1, new(0, 6));
                            SetPinOffset(3, new(-5, 0));
                        }
                        else
                        {
                            SetPinOffset(0, new(4, 0));
                            SetPinOffset(1, new(0, 9));
                            SetPinOffset(3, new(-4, 0));
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder drawing)
            {
                if (Variants.Contains(_packaged))
                    DrawPackaged(drawing);
                else
                    DrawRegular(drawing);
            }
            private void DrawRegular(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, 4, "s", "d");
                builder.ExtendPin(Pins["g"], Appearance);

                // Gate
                builder.Path(b => b.MoveTo(new(-6, 4)).LineTo(new(6, 4)).MoveTo(new(-6, 6)).LineTo(new(6, 6)), new("gate"));
                builder.Circle(new Vector2(0, 7.5), 1.5, Appearance);

                // Source and drain
                builder.Line(new(-4, 0), new(-4, 4), Appearance);
                builder.Line(new(4, 0), new(4, 4), Appearance);

                if (Variants.Contains(_depletion))
                    builder.Rectangle(-4, 2.5, 8, 1.5, options: Appearance.CreatePathOptions(this));

                // Label
                if (Pins["b"].Connections > 0)
                {
                    _anchors[0] = new LabelAnchorPoint(new(-3, -3), new(-1, -1), Appearance);
                    _anchors[1] = new LabelAnchorPoint(new(3, -3), new(1, -1), Appearance);
                }
                else
                {
                    _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1), Appearance);
                    _anchors[1] = new LabelAnchorPoint(new(0, -3), new(0, -1), Appearance);
                }
                _anchors.Draw(builder, Labels);
            }
            private void DrawPackaged(IGraphicsBuilder drawing)
            {
                drawing.ExtendPins(Pins, Appearance, 4, "s", "d");
                drawing.ExtendPin(Pins["g"], Appearance);

                // Gate
                drawing.Path(b => b.MoveTo(new(-6, 6)).LineTo(new(6, 6))
                    .MoveTo(new(-7, 4)).LineTo(new(-4, 4))
                    .MoveTo(new(-2, 4)).LineTo(new(2, 4))
                    .MoveTo(new(4, 4)).LineTo(new(7, 4)), new("gate"));

                // Drain, source and gate
                drawing.Line(new(-5, 0), new(-5, 4), Appearance);
                drawing.Line(new(5, 0), new(5, 4), Appearance);
                drawing.Arrow(new(0, 4), new(0, 0), Appearance);
                drawing.Line(new(0, 0), new(-5, 0), Appearance);

                // Packaged
                drawing.Circle(new(0, 3), 8.0, Appearance);

                // Label
                _anchors[0] = new LabelAnchorPoint(new(3, -11), new(1, 1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(-3, -11), new(-1, 1), Appearance);
                _anchors.Draw(drawing, Labels);
            }
        }
    }
}
