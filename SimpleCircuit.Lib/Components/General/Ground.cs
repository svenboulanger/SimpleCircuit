﻿using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    [Drawable("GND", "A common ground symbol.", "General", "earth chassis vss vee")]
    [Drawable("SGND", "A signal ground symbol.", "General", "earth chassis vss vee")]
    public class Ground : DrawableFactory
    {
        private const string _earth = "earth";
        private const string _chassis = "chassis";
        private const string _signal = "signal";
        private const string _noiseless = "noiseless";
        private const string _protective = "protective";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var device = new Instance(name);
            if (key == "SGND")
                device.Variants.Add("signal");
            return device;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(-6, 0), new(-1, 0)),
                new LabelAnchorPoint(new(6, 0), new(1, 0)));
            /// <inheritdoc />
            public override string Type => "ground";

            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                _anchors[0] = new LabelAnchorPoint(new(-6, 0), new(-1, 0));
                _anchors[1] = new LabelAnchorPoint(new(6, 0), new(1, 0));
                switch (Variants.Select(_earth, _chassis, _signal))
                {
                    case 0:
                    case 1: DrawEarth(drawing); break;
                    case 2: DrawSignalGround(drawing); break;
                    default: DrawGround(drawing); break;
                }

                _anchors.Draw(drawing, this);
            }
            private void DrawGround(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, Variants.Contains(_protective) ? 9 : 3);

                if (Variants.Contains(_noiseless))
                {
                    drawing.ExtendPins(Pins, 6);
                    drawing.Arc(new(0, 4), -Math.PI, 0, 8, new("shield"));
                    if (_anchors[0].Location.X > -9)
                        _anchors[0] = new LabelAnchorPoint(new(-9, 0), new(-1, 0));
                    if (_anchors[0].Location.X < 9)
                        _anchors[1] = new LabelAnchorPoint(new(9, 0), new(1, 0));
                }
                if (Variants.Contains(_protective))
                {
                    drawing.ExtendPins(Pins, 7.5);
                    drawing.Circle(new(0, -1), 6.5, new("shield"));
                    if (_anchors[0].Location.X > -7.5) 
                        _anchors[0] = new LabelAnchorPoint(new(-7.5, 0), new(-1, 0));
                    if (_anchors[1].Location.X < 7.5)
                        _anchors[1] = new LabelAnchorPoint(new(7.5, 0), new(1, 0));
                }
                else
                {
                    drawing.ExtendPins(Pins, 3);
                }
                drawing.Path(b => b.MoveTo(-5, 0).LineTo(5, 0).MoveTo(-3, 2).LineTo(3, 2).MoveTo(-1, 4).LineTo(1, 4));
            }
            private void DrawEarth(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3);

                // Ground segments
                drawing.Path(b => b.MoveTo(-5, 0).LineTo(5, 0)
                    .MoveTo(-5, 0).Line(-2, 4)
                    .MoveTo(0, 0).Line(-2, 4)
                    .MoveTo(5, 0).Line(-2, 4));

                if (_anchors[0].Location.X > -7)
                    _anchors[0] = new LabelAnchorPoint(new(-7, 0), new(-1, 0));
            }
            private void DrawSignalGround(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3);

                // Ground
                drawing.Polygon(new Vector2[]
                {
                    new(-5, 0), new(5, 0), new(0, 4)
                });
            }
        }
    }
}
