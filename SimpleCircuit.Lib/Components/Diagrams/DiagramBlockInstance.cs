using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Diagrams
{
    /// <summary>
    /// A diagram block.
    /// </summary>
    public abstract class DiagramBlockInstance : ILocatedDrawable, IScaledDrawable
    {
        private readonly PinCollection _pins;

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
        public string Name { get; }

        /// <inheritdoc />
        public IEnumerable<string[]> Properties => Drawable.GetProperties(this);

        /// <inheritdoc />
        /// <remarks>Diagram blocks cannot be oriented.</remarks>
        public int OrientationDegreesOfFreedom => 0;

        /// <inheritdoc />
        public Bounds Bounds { get; private set; }

        /// <summary>
        /// Creates a new instance for a block diagram.
        /// </summary>
        /// <param name="name">The name of the instance.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected DiagramBlockInstance(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            _pins = new(this);
            X = $"{Name}.x";
            Y = $"{Name}.y";
            GroupClasses = new List<string>() { "diagram" };
        }

        /// <inheritdoc />
        public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
            => Drawable.SetProperty(this, propertyToken, value, diagnostics);

        /// <inheritdoc />
        public bool Reset(IResetContext context)
        {
            _pins.SortClockwise();
            foreach (var pin in _pins)
            {
                if (!pin.Reset(context))
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            UpdatePins(_pins.Cast<LooselyOrientedPin>().ToList());
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Render(SvgDrawing drawing)
        {
            drawing.RequiredCSS.Add(".diagram { fill: white; }");

            // Group all elements
            var go = new GraphicOptions() { Id = Name };
            if (!string.IsNullOrWhiteSpace(Type))
                go.Classes.Add(Type.ToLower());
            foreach (string name in Variants)
                go.Classes.Add(name.ToLower());
            if (GroupClasses != null)
            {
                foreach (string name in GroupClasses)
                    go.Classes.Add(name);
            }
            drawing.BeginGroup(go);

            // Transform all the elements inside the drawing method
            drawing.BeginTransform(new(Location, Matrix2.Scale(Scale)));
            Draw(drawing);
            drawing.EndTransform();

            // Stop grouping elements
            Bounds = drawing.EndGroup();
        }

        /// <summary>
        /// Draws the diagram block.
        /// </summary>
        /// <param name="drawing"></param>
        protected abstract void Draw(SvgDrawing drawing);

        /// <inheritdoc />
        public bool DiscoverNodeRelationships(IRelationshipContext context)
        {
            foreach (var pin in _pins)
            {
                if (!pin.DiscoverNodeRelationships(context))
                    return false;
            }

            switch (context.Mode)
            {
                case NodeRelationMode.Groups:
                    context.Link(X, Y);
                    break;
            }
            return true;
        }

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
