using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A virtual wire.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="VirtualWire"/>.
    /// </remarks>
    /// <param name="name">The name of the virtual wire.</param>
    /// <param name="pinToWire">The pin starting the virtual wire.</param>
    /// <param name="segments">The wire segments.</param>
    /// <param name="wireToPin">The pin ending the virtual wire.</param>
    /// <param name="axis">The axis along which to align the items.</param>
    public class VirtualWire(string name, PinInfo pinToWire, IEnumerable<WireSegmentInfo> segments, PinInfo wireToPin, Axis axis) : ICircuitSolverPresence
    {
        private ILocatedPresence _start, _end;
        private readonly PinInfo _startInfo = pinToWire, _endInfo = wireToPin;
        private readonly List<WireSegmentInfo> _segments = segments?.ToList() ?? [];
        private readonly Axis _direction = axis;

        /// <inheritdoc />
        public string Name { get; } = name;

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
        public bool Reset(IResetContext context)
        {
            _start = null;
            _end = null;
            return true;
        }

        private ILocatedPresence FindPin(IPrepareContext context, PinInfo pin, int defaultIndex)
        {
            // Finding a pin for virtual wires works slightly different than normal:
            // If a pin is not named, then we actually want to refer to the origin of the component!
            var drawable = pin.Component.Get(context);
            if (drawable == null)
                return null;

            if (pin.Name.Content.Length == 0)
            {
                // We are referring to the origin of the component, not a pin
                if (drawable is not ILocatedDrawable ld)
                {
                    context.Diagnostics?.Post(pin.Component.Source, ErrorCodes.ComponentWithoutLocation, pin.Component.Source.Content);
                    return null;
                }
                return ld;
            }
            else
            {
                return pin.GetOrCreate(context.Diagnostics, defaultIndex);
            }
        }

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            switch (context.Mode)
            {
                case PreparationMode.Offsets:
                    // Find the start and end locations
                    _start = FindPin(context, _startInfo, -1);
                    _end = FindPin(context, _endInfo, 0);
                    if (_start == null || _end == null)
                        return PresenceResult.GiveUp;

                    // Directions
                    bool doX = (_direction & Axis.X) != 0;
                    bool doY = (_direction & Axis.Y) != 0;
                    string x, y;

                    // Go through the segments
                    for (int i = 0; i < _segments.Count; i++)
                    {
                        var segment = _segments[i];
                        if (segment.IsUnconstrained)
                        {
                            context.Diagnostics?.Post(segment.Source, ErrorCodes.VirtualWireUnconstrainedSegment);
                            return PresenceResult.GiveUp;
                        }
                        else if (segment.Orientation.X.IsZero() && segment.Orientation.Y.IsZero())
                        {
                            context.Diagnostics?.Post(segment.Source, ErrorCodes.VirtualWireUnknownSegment);
                            return PresenceResult.GiveUp;
                        }
                    }
                    if (_start != null)
                    {
                        // Align starting pin
                        if (doX)
                        {
                            if (!context.Offsets.Group(_start.X, StartX, 0.0))
                            {
                                context.Diagnostics?.Post(_segments[0].Source, ErrorCodes.CannotAlignAlongX, _start.X, StartX);
                                return PresenceResult.GiveUp;
                            }
                        }
                        if (doY)
                        {
                            if (!context.Offsets.Group(_start.Y, StartY, 0.0))
                            {
                                context.Diagnostics?.Post(_segments[0].Source, ErrorCodes.CannotAlignAlongY, _start.Y, StartY);
                                return PresenceResult.GiveUp;
                            }
                        }
                    }
                    if (_end != null)
                    {
                        // Align end pin
                        if (doX)
                        {
                            if (!context.Offsets.Group(_end.X, EndX, 0.0))
                            {
                                context.Diagnostics?.Post(_segments[^1].Source, ErrorCodes.CannotAlignAlongX, _end.X, EndX);
                                return PresenceResult.GiveUp;
                            }
                        }
                        if (doY)
                        {
                            if (!context.Offsets.Group(_end.Y, EndY, 0.0))
                            {
                                context.Diagnostics?.Post(_segments[^1].Source, ErrorCodes.CannotAlignAlongY, _end.Y, EndY);
                                return PresenceResult.GiveUp;
                            }
                        }
                    }

                    // Deal with the horizontal and vertical segments
                    x = StartX;
                    y = StartY;
                    for (int i = 0; i < _segments.Count; i++)
                    {
                        string tx = GetXName(i);
                        string ty = GetYName(i);
                        var segment = _segments[i];
                        if (segment.IsFixed)
                        {
                            if (doX)
                            {
                                if (!context.Offsets.Group(x, tx, segment.Orientation.X * segment.Length))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(segment.Orientation.X * segment.Length), segment.Source.Content);
                                    return PresenceResult.GiveUp;
                                }
                            }
                            if (doY)
                            {
                                if (!context.Offsets.Group(y, ty, segment.Orientation.Y * segment.Length))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(segment.Orientation.Y * segment.Length), segment.Source.Content);
                                    return PresenceResult.GiveUp;
                                }
                            }
                        }
                        else
                        {
                            if (segment.Orientation.X.IsZero())
                            {
                                if (!context.Offsets.Group(x, tx, 0.0))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, 0.0, segment.Source.Content);
                                    return PresenceResult.GiveUp;
                                }
                            }
                            if (segment.Orientation.Y.IsZero())
                            {
                                if (!context.Offsets.Group(y, ty, 0.0))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, 0.0, segment.Source.Content);
                                    return PresenceResult.GiveUp;
                                }
                            }
                        }
                        x = tx;
                        y = ty;
                    }
                    break;

                case PreparationMode.Groups:
                    x = context.Offsets[StartX].Representative;
                    y = context.Offsets[StartY].Representative;
                    for (int i = 0; i < _segments.Count; i++)
                    {
                        var segment = _segments[i];
                        string tx = context.Offsets[GetXName(i)].Representative;
                        string ty = context.Offsets[GetYName(i)].Representative;
                        if (!segment.IsUnconstrained)
                        {
                            context.Groups.Group(x, tx);
                            context.Groups.Group(y, ty);
                        }
                    }
                    break;
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
            // Determine the active axis
            bool doX = (_direction & Axis.X) != 0;
            bool doY = (_direction & Axis.Y) != 0;
            if (!doX && !doY)
                return;

            var map = context.Relationships.Offsets;
            var fromX = map[StartX];
            var fromY = map[StartY];
            for (int i = 0; i < _segments.Count; i++)
            {
                string x = GetXName(i);
                string y = GetYName(i);
                var toX = map[x];
                var toY = map[y];
                var segment = _segments[i];
                if (!segment.IsFixed)
                {
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
