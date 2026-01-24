using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Markers;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;

namespace SimpleCircuit.Components.Analog;

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
        private readonly CustomLabelAnchorPoints _anchors = new(1);

        /// <inheritdoc />
        public override string Type => "nmos";

        [Description("The margin for labels.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Nmos"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public Nmos(string name)
            : base(name)
        {
            AddPin(new FixedOrientedPin("source", "The source.", this, new Vector2(-4, 0), new Vector2(-1, 0)), "s", "source");
            AddPin(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 6), new Vector2(0, 1)), "g", "gate");
            AddPin(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 4), new Vector2(0, -1)), "b", "bulk");
            AddPin(new FixedOrientedPin("drain", "The drain", this, new Vector2(4, 0), new Vector2(1, 0)), "d", "drain");
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
            var style = builder.Style.ModifyDashedDotted(this);

            builder.ExtendPins(Pins, style, 4, "s", "d");
            builder.ExtendPin(Pins["g"], style);

            // Gate
            builder.Path(b => b.MoveTo(new(-6, 4)).LineTo(new(6, 4)).MoveTo(new(-6, 6)).LineTo(new(6, 6)), style);

            // Source and drain
            builder.Line(new(-4, 0), new(-4, 4), style);
            builder.Line(new(4, 0), new(4, 4), style);

            if (Variants.Contains(_depletion))
                builder.Rectangle(-4, 2.5, 8, 1.5, style);

            // Label
            double m = style.LineThickness * 0.5 + LabelMargin;
            if (Pins["b"].Connections > 0)
                _anchors[0] = new LabelAnchorPoint(new(-3, -m), new(1, -1));
            else
                _anchors[0] = new LabelAnchorPoint(new(0, -m), new(0, -1));
            _anchors.Draw(builder, this, style);
        }
        private void DrawPackaged(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            // Packaged
            builder.Circle(new(0, 3), 8.0, style);

            builder.ExtendPins(Pins, style, 3, "s", "d");
            builder.ExtendPin(Pins["g"], style);

            // Gate
            builder.Path(b => b.MoveTo(new(-6, 6)).LineTo(new(6, 6))
                .MoveTo(new(-7, 4)).LineTo(new(-4, 4))
                .MoveTo(new(-2, 4)).LineTo(new(2, 4))
                .MoveTo(new(4, 4)).LineTo(new(7, 4)), style);

            // Drain, source and gate
            builder.Line(new(-5, 0), new(-5, 4), style);
            builder.Line(new(5, 0), new(5, 4), style);
            builder.Polyline([
                new(-5, 0),
                new(0, 0),
                new(0, 4)
            ], style);

            var marker = new Arrow(new(0, 4), new(0, 1));
            marker.Draw(builder, style);

            // Label
            double m = 0.5 * style.LineThickness + LabelMargin;
            _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
            _anchors.Draw(builder, this, style);
        }
    }
    private class Pmos : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(1);

        /// <inheritdoc />
        public override string Type => "pmos";

        [Description("The margin for labels.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Pmos"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public Pmos(string name)
            : base(name)
        {
            AddPin(new FixedOrientedPin("drain", "The drain", this, new Vector2(4, 0), new Vector2(1, 0)), "d", "drain");
            AddPin(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 9), new Vector2(0, 1)), "g", "gate");
            AddPin(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 0), new Vector2(0, -1)), "b", "bulk");
            AddPin(new FixedOrientedPin("source", "The source.", this, new Vector2(-4, 0), new Vector2(-1, 0)), "s", "source");
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
        protected override void Draw(IGraphicsBuilder builder)
        {
            if (Variants.Contains(_packaged))
                DrawPackaged(builder);
            else
                DrawRegular(builder);
        }
        private void DrawRegular(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            builder.ExtendPins(Pins, style, 4, "s", "d");
            builder.ExtendPin(Pins["g"], style);

            // Gate
            builder.Path(b => b.MoveTo(new(-6, 4)).LineTo(new(6, 4)).MoveTo(new(-6, 6)).LineTo(new(6, 6)), style);
            builder.Circle(new Vector2(0, 7.5), 1.5, style);

            // Source and drain
            builder.Line(new(-4, 0), new(-4, 4), style);
            builder.Line(new(4, 0), new(4, 4), style);

            if (Variants.Contains(_depletion))
                builder.Rectangle(-4, 2.5, 8, 1.5, style);

            // Label
            double m = style.LineThickness * 0.5 + LabelMargin;
            if (Pins["b"].Connections > 0)
                _anchors[0] = new LabelAnchorPoint(new(-3, -m), new(1, -1));
            else
                _anchors[0] = new LabelAnchorPoint(new(0, -m), new(0, -1));
            _anchors.Draw(builder, this, style);
        }
        private void DrawPackaged(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            // Packaged
            builder.Circle(new(0, 3), 8.0, style);

            builder.ExtendPins(Pins, style, 4, "s", "d");
            builder.ExtendPin(Pins["g"], style);

            // Gate
            builder.Path(b => b.MoveTo(new(-6, 6)).LineTo(new(6, 6))
                .MoveTo(new(-7, 4)).LineTo(new(-4, 4))
                .MoveTo(new(-2, 4)).LineTo(new(2, 4))
                .MoveTo(new(4, 4)).LineTo(new(7, 4)), style);

            // Drain, source and gate
            builder.Line(new(-5, 0), new(-5, 4), style);
            builder.Line(new(5, 0), new(5, 4), style);
            builder.Arrow(new(0, 4), new(0, 0), style);
            builder.Line(new(0, 0), new(-5, 0), style);

            // Label
            double m = 0.5 * style.LineThickness + LabelMargin;
            _anchors[0] = new LabelAnchorPoint(new(0, -5 - m), new(0, -1));
            _anchors.Draw(builder, this, style);
        }
    }
}
