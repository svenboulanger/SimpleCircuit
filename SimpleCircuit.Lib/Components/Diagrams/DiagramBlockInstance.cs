﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components.Diagrams
{
    /// <summary>
    /// A diagram block.
    /// </summary>
    public abstract class DiagramBlockInstance : ILocatedDrawable, IScaledDrawable
    {
        private readonly PinCollection _pins;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public List<TextLocation> Sources { get; } = [];

        /// <summary>
        /// Gets the type of the diagram block.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Allows adding classes for the group node that groups all drawing elements.
        /// </summary>
        protected virtual IEnumerable<string> GroupClasses { get; }

        /// <inheritdoc />
        public VariantSet Variants { get; } = [];

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        [Description("The scale of the block.")]
        public double Scale { get; set; } = 1.0;

        /// <inheritdoc />
        IPinCollection IDrawable.Pins => _pins;

        /// <inheritdoc />
        public string X { get; }

        /// <inheritdoc />
        public string Y { get; }

        /// <inheritdoc />
        public Vector2 Location { get; protected set; }

        /// <inheritdoc />
        public IEnumerable<string[]> Properties => Drawable.GetProperties(this);

        /// <inheritdoc />
        /// <remarks>Diagram blocks cannot be oriented.</remarks>
        public int OrientationDegreesOfFreedom => 0;

        /// <inheritdoc />
        public Bounds Bounds { get; private set; }

        /// <inheritdoc />
        public (string X, string Y) CoordinateGroup { get; private set; } = ("0", "0");

        /// <inheritdoc />
        public Labels Labels { get; }

        /// <inheritdoc />
        public IStyleModifier Style { get; set; }

        /// <summary>
        /// Creates a new instance for a block diagram.
        /// </summary>
        /// <param name="name">The name of the instance.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected DiagramBlockInstance(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Labels = new();
            Name = name;
            _pins = new(this);
            X = $"{Name}.x";
            Y = $"{Name}.y";
            GroupClasses = ["diagram"];
        }

        /// <inheritdoc />
        public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
            => Drawable.SetProperty(this, propertyToken, value, diagnostics);

        /// <inheritdoc />
        public virtual PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    _pins.SortClockwise();
                    break;

                case PreparationMode.Offsets:
                    context.Offsets.Add(X);
                    context.Offsets.Add(Y);
                    UpdatePins([.. _pins.Cast<LooselyOrientedPin>()]);
                    break;

                case PreparationMode.Sizes:
                    Labels.Format(context.TextFormatter, Style?.Apply(context.Style) ?? context.Style);
                    break;

                case PreparationMode.DrawableGroups:
                    context.GroupDrawableTo(this, X, Y);
                    break;
            }

            var result = PresenceResult.Success;
            foreach (var pin in _pins)
            {
                var r = pin.Prepare(context);
                if (r == PresenceResult.GiveUp)
                    return PresenceResult.GiveUp;
                else if (r == PresenceResult.Incomplete)
                    result = PresenceResult.Incomplete;
            }
            return result;
        }

        /// <inheritdoc />
        public void Render(IGraphicsBuilder builder)
        {
            // Group all elements
            var classes = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(Type))
                classes.Add(Type.ToLower());
            foreach (string name in Variants)
                classes.Add(name.ToLower());
            if (GroupClasses != null)
            {
                foreach (string name in GroupClasses)
                    classes.Add(name);
            }
            builder.BeginGroup(Name, classes);

            // Transform all the elements inside the drawing method
            builder.BeginTransform(new(Location, Matrix2.Scale(Scale)));
            Draw(builder);
            builder.EndTransform();

            // Stop grouping elements
            builder.EndGroup();
        }

        /// <summary>
        /// Draws the diagram block.
        /// </summary>
        /// <param name="builder"></param>
        protected abstract void Draw(IGraphicsBuilder builder);

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
            foreach (var pin in _pins)
                pin.Register(context);
        }

        /// <summary>
        /// Updates the pin location accordingly.
        /// </summary>
        /// <param name="pins">The pins.</param>
        protected abstract void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins);

        /// <inheritdoc />
        public void Update(IUpdateContext context)
        {
            Location = context.GetValue(X, Y);
            foreach (var pin in _pins)
                pin.Update(context);
        }

        /// <inheritdoc />
        public Vector2 TransformOffset(Vector2 local) => local * Scale;

        /// <inheritdoc />
        public Vector2 TransformNormal(Vector2 local) => local;
    }
}
