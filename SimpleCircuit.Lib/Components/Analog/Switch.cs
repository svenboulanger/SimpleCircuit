﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Markers;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A switch.
    /// </summary>
    [Drawable("S", "A switch. The controlling pin is optional.", "Analog", "push lamp window toggle knife reed arei")]
    public class Switch : DrawableFactory
    {
        private const string _closed = "closed";
        private const string _invert = "invert";
        private const string _push = "push";
        private const string _knife = "knife";
        private const string _closing = "closing";
        private const string _opening = "opening";
        private const string _reed = "reed";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            [Description("The number of poles on the switch.")]
            [Alias("p")]
            public int Poles { get; set; }

            /// <inheritdoc />
            public override string Type => "switch";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, -6), new(0, -1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("backside", "The backside controlling pin. Can be used to link multiple switches.", this, new(0, -6), new(0, 1)), "c2", "ctrl2");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");
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
                        SetPinOffset(0, new(-6, 0));
                        SetPinOffset(3, new(6, 0));
                        if (Variants.Contains(_invert))
                        {
                            if (Variants.Contains(_closed))
                            {
                                SetPinOffset(1, new(0, -2));
                                SetPinOffset(2, new());
                            }
                            else
                            {
                                SetPinOffset(1, new(0, -4.25));
                                SetPinOffset(2, new(0, -2));
                            }
                        }
                        else
                        {
                            if (Variants.Contains(_closed))
                            {
                                SetPinOffset(1, new());
                                SetPinOffset(2, new());
                            }
                            else
                            {
                                SetPinOffset(1, new(0, -2));
                                SetPinOffset(2, new(0, -2));
                            }
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                switch (Variants.Select(_knife, _push))
                {
                    case 0: // Knife switch
                        DrawKnifeSwitch(builder, style);
                        break;

                    case 1: // Push switch
                        DrawPushSwitch(builder, style);
                        break;

                    default:
                        DrawRegularSwitch(builder, style);
                        break;
                }
                _anchors.Draw(builder, this, style);
            }

            private void DrawKnifeSwitch(IGraphicsBuilder builder, IStyle style)
            {
                if (Variants.Contains(_closed))
                {
                    builder.Circle(new(-5, 0), 1, style);
                    builder.Circle(new(5, 0), 1, style);
                    builder.Line(new(-4, 0), new(4, 0), style);
                    builder.Line(new(0, 2), new(0, -2), style);
                }
                else
                {
                    builder.Polyline([new(-6, 0), new(-4, 0), new(2, -4)], style);
                    builder.Line(new(4, 0), new(6, 0), style);
                    builder.Line(new(-0.5, -4), new(1.5, -1.5), style);
                }

                _anchors[0] = new LabelAnchorPoint(new(3, -3), new(1, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1));
            }
            private void DrawRegularSwitch(IGraphicsBuilder builder, IStyle style)
            {
                if (Variants.Contains(_reed))
                {
                    // Reed switch / pill shape (background)
                    builder.Path(b =>
                    {
                        b.MoveTo(new(-5, -6));
                        b.LineTo(new(5, -6));
                        b.CurveTo(new(8.3, -6), new(11, -3.3), new(11, 0));
                        b.SmoothTo(new(8.3, 6), new(5, 6));
                        b.LineTo(new(-5, 6));
                        b.CurveTo(new(-8.3, 6), new(-11, 3.3), new(-11, 0));
                        b.SmoothTo(new(-8.3, -6), new(-5, -6));
                        b.Close();
                    }, style);

                    // Update labels
                    _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                }
                else
                {
                    // Default labels
                    _anchors[0] = new LabelAnchorPoint(new(0, -1.5), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 1.5), new(0, 1));
                }

                builder.ExtendPins(Pins, style, 2, "a", "b");

                // Switch terminals
                builder.Circle(new Vector2(-5, 0), 1, style);
                builder.Circle(new Vector2(5, 0), 1, style);

                if (Variants.Contains(_closed))
                {
                    // Closed switch
                    if (Variants.Contains(_invert))
                    {
                        // Inverted switch
                        builder.Circle(new(0, -1), 1, style);

                        // Update labels
                        if (_anchors[0].Location.Y > -2.5)
                            _anchors[0] = new LabelAnchorPoint(new(0, -2.5), new(0, -1));
                    }

                    // The switch
                    builder.Line(new(-4, 0), new(4, 0), style);
                }
                else
                {
                    // Open switch
                    if (Variants.Contains(_invert))
                        builder.Circle(new(0, -3.25), 1, style);

                    // The switch
                    builder.Line(new(-4, 0), new(4, -4), style);

                    // Update labels
                    if (_anchors[0].Location.Y > -5)
                        _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                }

                switch (Variants.Select(_closing, _opening))
                {
                    case 0:
                        // Close arrow
                        builder.Path(b =>
                        {
                            b.MoveTo(new(-3, -5))
                            .CurveTo(new(1, -3), new(1, -2), new(2, 2));
                            var marker = new Arrow(b.End, b.EndNormal);
                            marker.Draw(builder, style);
                        }, style.AsStroke());

                        // Update labels
                        if (_anchors[0].Location.Y > -6)
                            _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                        if (_anchors[1].Location.Y < 3)
                            _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1));
                        break;

                    case 1:
                        // Open arrow
                        builder.Path(b =>
                        {
                            b.MoveTo(new(-4, -6)).CurveTo(new(1, -3), new(1, -2), new(2, 1));
                            var marker = new Arrow(b.End, b.EndNormal);
                            marker.Draw(builder, style);
                        }, style.AsStroke());

                        // Update labels
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                        if (_anchors[1].Location.Y < 2)
                            _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1));
                        break;
                }
            }
            private void DrawPushSwitch(IGraphicsBuilder builder, IStyle style)
            {
                // Switch terminals
                builder.Circle(new Vector2(-5, 0), 1, style);
                builder.Circle(new Vector2(5, 0), 1, style);

                // Initialize labels
                _anchors[0] = new LabelAnchorPoint(new(0, -2), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1));

                if (Variants.Contains(_closed))
                {
                    builder.Line(new(-4, 0), new(4, 0), style);
                    if (Variants.Contains(_invert))
                    {
                        // Inverted
                        builder.Circle(new(0, -1), 1, style);
                        builder.Line(new(0, -2), new(0, -6), style);

                        // Update labels
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    }
                    else
                    {
                        // Non-inverted
                        builder.Line(new(0, 0), new(0, -6), style);

                        // Update labels
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    }
                }
                else
                {
                    builder.Line(new(-5, -4), new(5, -4), style);
                    if (Variants.Contains(_invert))
                    {
                        // Inverted
                        builder.Circle(new(0, -5), 1, style);

                        // Update labels
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    }
                    else
                    {
                        // Non-inverted
                        builder.Line(new(0, -4), new(0, -6), style);

                        // Update labels
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    }
                }

                if (_anchors[1].Location.Y < 2)
                    _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1));
            }
        }
    }
}