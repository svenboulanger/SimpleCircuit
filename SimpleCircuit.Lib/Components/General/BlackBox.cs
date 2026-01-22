using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;
using SpiceSharp.General;

namespace SimpleCircuit.Components;

/// <summary>
/// A generic black box.
/// </summary>
[Drawable("BB", "A black box. Pins are created on the fly, where the first character (n, s, e, w for north, south, east and west respectively) indicates the side of the pin.", "General")]
public partial class BlackBox : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    /// <inheritdoc />
    public override IDrawable Create(string key, string name, Options options, Scope scope, IDiagnosticHandler diagnostics)
    {
        var instance = (Instance)base.Create(key, name, options, scope, diagnostics);
        return instance;
    }

    protected class Instance : ILocatedDrawable, IBoxDrawable, IRoundedBox
    {
        private readonly PinCollection _pins;

        /// <inheritdoc />
        public VariantSet Variants { get; } = [];

        /// <inheritdoc />
        public List<TextLocation> Sources { get; } = [];

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        IPinCollection IDrawable.Pins => _pins;

        /// <inheritdoc />
        public string X { get; }

        /// <inheritdoc />
        public string Y { get; }

        /// <inheritdoc />
        public Vector2 Location { get; protected set; }

        /// <inheritdoc />
        public Vector2 EndLocation { get; protected set; }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets or sets the margin for each pin label.
        /// </summary>
        [Description("The margins for pin names.")]
        [Alias("pm")]
        public Margins PinMargins { get; set; } = new(2, 2, 2, 2);

        [Description("The minimum width.")]
        [Alias("minw")]
        public double MinWidth { get; set; } = 10.0;

        [Description("The minimum height.")]
        [Alias("minh")]
        public double MinHeight { get; set; } = 10.0;

        [Description("The margin for outside labels to the black box edge.")]
        [Alias("om")]
        public double OuterMargin { get; set; } = 1.0;

        [Description("The margin for inside labels to the black box edge.")]
        [Alias("im")]
        public Margins InnerMargins { get; set; } = new(1, 1, 1, 1);

        /// <inheritdoc />
        Bounds IBoxDrawable.OuterBounds => new(Location, EndLocation);

        /// <inheritdoc />
        Bounds IBoxDrawable.InnerBounds => new Bounds(Location, EndLocation).Shrink(_pins.InnerMargins);

        [Description("The round-off corner radius.")]
        [Alias("r")]
        [Alias("radius")]
        public double CornerRadius { get; set; }

        /// <inheritdoc />
        public Labels Labels { get; } = new();

        /// <inheritdoc />
        public IEnumerable<string[]> Properties => Drawable.GetProperties(this);

        /// <inheritdoc />
        public Bounds Bounds { get; private set; }

        /// <inheritdoc />
        public IStyleModifier Modifier { get; set; }

        /// <summary>
        /// Creates a new black box.
        /// </summary>
        /// <param name="name">The name.</param>
        public Instance(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            _pins = new(this);
            X = $"{Name}.x";
            Y = $"{Name}.y";
            EndLocation = new(20, 20);
        }

        /// <inheritdoc />
        public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
            => Drawable.SetProperty(this, propertyToken, value, diagnostics);

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            var result = _pins.Prepare(context);

            // Group the drawable
            switch (context.Mode)
            {
                case PreparationMode.Offsets:
                    context.Offsets.Add(X);
                    context.Offsets.Add(Y);
                    context.Offsets.Add(_pins.Right);
                    context.Offsets.Add(_pins.Bottom);
                    break;

                case PreparationMode.Sizes:
                    var style = context.Style.ModifyDashedDotted(this);
                    Labels.Format(context.TextFormatter, style);
                    var bounds = BoxLabelAnchorPoints.Default.CalculateSize(context.TextFormatter, this, style);
                    _pins.InnerBounds = bounds.Expand(InnerMargins);
                    break;

                case PreparationMode.DrawableGroups:
                    context.GroupDrawableTo(this, X, Y);
                    break;
            }

            return result;
        }

        /// <inheritdoc />
        public void Render(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            builder.BeginGroup(Name, ["blackbox"]);
            var size = EndLocation - Location;
            builder.Rectangle(Location.X, Location.Y, size.X, size.Y, style, CornerRadius, CornerRadius);

            // Draw the label
            BoxLabelAnchorPoints.Default.Draw(builder, this, style);

            // Draw the port names
            _pins.Render(builder, style);
            builder.EndGroup();
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
            _pins.Register(context);
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context)
        {
            Location = context.GetValue(X, Y);
            EndLocation = context.GetValue(_pins.Right, _pins.Bottom);

            // Update all pin locations as well
            foreach (var pin in _pins)
                pin.Update(context);

            // Update the bounds
            Bounds = new(Location, EndLocation);
        }
    }
}