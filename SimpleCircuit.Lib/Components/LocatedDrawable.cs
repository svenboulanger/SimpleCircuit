using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A generic implementation of a component.
    /// </summary>
    public abstract class LocatedDrawable : Drawable, ILocatedDrawable
    {
        /// <inheritdoc />
        public string X { get; }

        /// <inheritdoc />
        public string Y { get; }

        /// <inheritdoc />
        public Vector2 Location { get; private set; }

        /// <inheritdoc />
        protected override Transform CreateTransform() => new(Location, Matrix2.Identity);

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedDrawable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="point">The names of the coordinate of the drawable.</param>
        protected LocatedDrawable(string name, (string X, string Y) point = default)
            : base(name)
        {
            X = point.X ?? $"{name}.x";
            Y = point.Y ?? $"{name}.y";
        }

        /// <inheritdoc />
        public override bool Reset(IResetContext context)
        {
            if (!base.Reset(context))
                return false;
            Location = new();
            return true;
        }

        /// <inheritdoc />
        public override void Update(IUpdateContext context)
        {
            Location = context.GetValue(X, Y);

            // Give the pins a chance to update as well
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Update(context);
        }

        /// <inheritdoc />
        public override bool DiscoverNodeRelationships(IRelationshipContext context)
        {
            if (!base.DiscoverNodeRelationships(context))
                return false;
            for (int i = 0; i < Pins.Count; i++)
            {
                if (!Pins[i].DiscoverNodeRelationships(context))
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
        public override void Register(IRegisterContext context)
        {
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Register(context);
        }
    }
}
