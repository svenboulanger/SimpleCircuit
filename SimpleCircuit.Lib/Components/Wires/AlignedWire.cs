using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire that simply alignes multiple circuit presences.
    /// </summary>
    public class AlignedWire : ICircuitSolverPresence
    {
        private readonly IEnumerable<ILocatedPresence> _presences;
        private readonly Axis _axis;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public int Order => 2;

        /// <summary>
        /// Creates a new <see cref="AlignedWire"/>.
        /// </summary>
        /// <param name="name">The name of the alignment.</param>
        /// <param name="presences">The presences that need to be aligned.</param>
        /// <param name="axis">The axis along which the alignment takes place.</param>
        public AlignedWire(string name, IEnumerable<ILocatedPresence> presences, Axis axis)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            _presences = presences ?? throw new ArgumentNullException(nameof(presences));
            _axis = axis;
        }


        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Offsets:
                    bool doX = (_axis & Axis.X) != 0;
                    bool doY = (_axis & Axis.Y) != 0;
                    ILocatedPresence last = null;
                    foreach (var presence in _presences)
                    {
                        if (last != null)
                        {
                            if (doX && !context.Offsets.Group(last.X, presence.X, 0.0))
                            {
                                context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongX, last.X, presence.X);
                                return PresenceResult.GiveUp;
                            }
                            if (doY && !context.Offsets.Group(last.Y, presence.Y, 0.0))
                            {
                                context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongY, last.Y, presence.Y);
                                return PresenceResult.GiveUp;
                            }
                        }
                        last = presence;
                    }
                    break;
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context)
        {
        }

        /// <inheritdoc />
        public bool Reset(IResetContext context) => true;
    }
}
