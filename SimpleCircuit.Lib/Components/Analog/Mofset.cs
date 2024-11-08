﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Markers;
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

        private class Nmos : ScaledOrientedDrawable, ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
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
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (Variants.Contains(_packaged))
                    DrawPackaged(drawing);
                else
                    DrawRegular(drawing);
            }
            private void DrawRegular(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 4, "s", "d");
                drawing.ExtendPin(Pins["g"]);

                // Gate
                drawing.Path(b => b.MoveTo(-6, 4).LineTo(6, 4).MoveTo(-6, 6).LineTo(6, 6), new("gate"));

                // Source and drain
                drawing.Line(new(-4, 0), new(-4, 4), new("source"));
                drawing.Line(new(4, 0), new(4, 4), new("drain"));

                if (Variants.Contains(_depletion))
                    drawing.Rectangle(-4, 2.5, 8, 1.5, options: new("marker"));

                // Label
                if (Pins["b"].Connections > 0)
                {
                    _anchors[0] = new LabelAnchorPoint(new(-3, -3), new(-1, -1));
                    _anchors[1] = new LabelAnchorPoint(new(3, -3), new(1, -1));
                }
                else
                {
                    _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, -3), new(0, -1));
                }
                _anchors.Draw(drawing, this);
            }
            private void DrawPackaged(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3, "s", "d");
                drawing.ExtendPin(Pins["g"]);

                // Gate
                drawing.Path(b => b.MoveTo(-6, 6).LineTo(6, 6)
                    .MoveTo(-7, 4).LineTo(-4, 4)
                    .MoveTo(-2, 4).LineTo(2, 4)
                    .MoveTo(4, 4).LineTo(7, 4), new("gate"));

                // Drain, source and gate
                drawing.Line(new(-5, 0), new(-5, 4), new("source"));
                drawing.Line(new(5, 0), new(5, 4), new("drain"));
                drawing.Polyline(new Vector2[] { new(-5, 0), new(0, 0), new(0, 4) }, new("bulk"));

                var marker = new Arrow(new(0, 4), new(0, 1));
                marker.Draw(drawing);

                // Packaged
                drawing.Circle(new(0, 3), 8.0);

                // Label
                _anchors[0] = new LabelAnchorPoint(new(3, -11), new(1, 1));
                _anchors[1] = new LabelAnchorPoint(new(-3, -11), new(-1, 1));
                _anchors.Draw(drawing, this);
            }
        }
        private class Pmos : ScaledOrientedDrawable, ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
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
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (Variants.Contains(_packaged))
                    DrawPackaged(drawing);
                else
                    DrawRegular(drawing);
            }
            private void DrawRegular(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 4, "s", "d");
                drawing.ExtendPin(Pins["g"]);

                // Gate
                drawing.Path(b => b.MoveTo(-6, 4).LineTo(6, 4).MoveTo(-6, 6).LineTo(6, 6), new("gate"));
                drawing.Circle(new Vector2(0, 7.5), 1.5);

                // Source and drain
                drawing.Line(new(-4, 0), new(-4, 4), new("source"));
                drawing.Line(new(4, 0), new(4, 4), new("drain"));

                if (Variants.Contains(_depletion))
                    drawing.Rectangle(-4, 2.5, 8, 1.5, options: new("marker"));

                // Label
                if (Pins["b"].Connections > 0)
                {
                    _anchors[0] = new LabelAnchorPoint(new(-3, -3), new(-1, -1));
                    _anchors[1] = new LabelAnchorPoint(new(3, -3), new(1, -1));
                }
                else
                {
                    _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, -3), new(0, -1));
                }
                _anchors.Draw(drawing, this);
            }
            private void DrawPackaged(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 4, "s", "d");
                drawing.ExtendPin(Pins["g"]);

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
                _anchors[0] = new LabelAnchorPoint(new(3, -11), new(1, 1));
                _anchors[1] = new LabelAnchorPoint(new(-3, -11), new(-1, 1));
                _anchors.Draw(drawing, this);
            }
        }
    }
}
