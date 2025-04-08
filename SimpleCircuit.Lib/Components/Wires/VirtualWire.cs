using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Wires
{

    public class VirtualWire(string name, ILocatedPresence start, IEnumerable<WireSegmentInfo> segments, ILocatedPresence end, VirtualChainConstraints flags) : ICircuitSolverPresence
    {
        private readonly ILocatedPresence _start = start, _end = end;
        private readonly List<WireSegmentInfo> _segments = segments?.ToList() ?? [];
        private readonly VirtualChainConstraints _axis = flags;

        /// <inheritdoc />
        public string Name { get; } = name;

        /// <inheritdoc />
        public List<TextLocation> Sources { get; }

        /// <inheritdoc />
        public int Order => 1;

        /// <summary>
        /// Gets the X-coordinate name of the first point of the wire.
        /// </summary>
        public string StartX => GetXName(-1);

        /// <summary>
        /// Gets the Y-coordinate name of the first point of the wire.
        /// </summary>
        public string StartY => GetYName(-1);

        /// <summary>
        /// Gets the X-coordinate name of the last point of the wire.
        /// </summary>
        public string EndX => GetXName(_segments.Count - 1);

        /// <summary>
        /// Gets the Y-coordinate name of the last point of the wire.
        /// </summary>
        public string EndY => GetYName(_segments.Count - 1);

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Offsets:
                    if (Helpers.PrepareEntryOffset(_segments[0].Source, _start, StartX, StartY, _axis, context) == PresenceResult.GiveUp)
                        return PresenceResult.GiveUp;
                    if (Helpers.PrepareEntryOffset(_segments[^1].Source, _end, EndX, EndY, _axis, context) == PresenceResult.GiveUp)
                        return PresenceResult.GiveUp;
                    return Helpers.PrepareSegmentsOffset(
                        GetXName, GetYName, i => _segments[i].Orientation,
                        _segments, _axis, context);

                case PreparationMode.Groups:
                    return Helpers.PrepareSegmentsGroup(
                        GetXName, GetYName, _segments, _axis, context);
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
            // Determine the active axis
            bool doX = (_axis & VirtualChainConstraints.X) != 0;
            bool doY = (_axis & VirtualChainConstraints.Y) != 0;
            if (!doX && !doY)
                return;

            var fromX = context.GetOffset(StartX);
            var fromY = context.GetOffset(StartY);
            for (int i = 0; i < _segments.Count; i++)
            {
                string x = GetXName(i);
                string y = GetYName(i);
                var toX = context.GetOffset(x);
                var toY = context.GetOffset(y);
                var segment = _segments[i];
                var orientation = segment.Orientation;

                if (doY && segment.Orientation.X.IsZero() && fromY.Representative != toY.Representative)
                    MinimumConstraint.AddDirectionalMinimum(context.Circuit, y, fromY, toY, segment.Orientation.Y * segment.Length);
                if (doX && segment.Orientation.Y.IsZero() && fromX.Representative != toX.Representative)
                    MinimumConstraint.AddDirectionalMinimum(context.Circuit, x, fromX, toX, segment.Orientation.X * segment.Length);

                if (!segment.Orientation.X.IsZero() && !segment.Orientation.Y.IsZero())
                {
                    // The wire definition is an odd angle, the axis becomes important
                    if (doX && doY)
                        MinimumConstraint.AddDirectionalMinimum(context.Circuit, $"{x}.{y}", fromX, fromY, toX, toY, segment.Orientation, segment.Length);
                    else if (doX)
                        MinimumConstraint.AddDirectionalMinimum(context.Circuit, x, fromX, toX, segment.Orientation.X * segment.Length);
                    else
                        MinimumConstraint.AddDirectionalMinimum(context.Circuit, y, fromY, toY, segment.Orientation.Y * segment.Length);
                }
                fromX = toX;
                fromY = toY;
            }
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context)
        {
        }

        private string GetXName(int index) => $"{Name}.{index + 1}.x";
        private string GetYName(int index) => $"{Name}.{index + 1}.y";
    }
}
