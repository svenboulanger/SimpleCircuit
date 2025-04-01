using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// Default implementation for pins.
    /// </summary>
    public abstract class Pin : IPin
    {
        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public List<TextLocation> Sources { get; } = [];

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public ILocatedDrawable Owner { get; }

        /// <inheritdoc />
        public int Connections { get; set; }

        /// <inheritdoc />
        public string X { get; }

        /// <inheritdoc />
        public string Y { get; }

        /// <inheritdoc />
        public Vector2 Location { get; protected set; }

        protected Pin(string name, string description, ILocatedDrawable owner)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Owner = owner ?? throw new ArgumentNullException(nameof(name));
            X = $"{Owner.Name}[{Name}].x";
            Y = $"{Owner.Name}[{Name}].y";
        }

        /// <inheritdoc />
        public virtual PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    Location = new();
                    break;
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public abstract void Register(IRegisterContext context);

        /// <inheritdoc />
        public void Update(IUpdateContext context)
            => Location = context.GetValue(X, Y);

        /// <inheritdoc />
        public override string ToString()
            => $"{Owner.Name}[{Name}]";
    }
}
