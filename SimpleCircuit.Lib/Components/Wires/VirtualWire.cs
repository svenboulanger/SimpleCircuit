using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SpiceSharp.Components;
using System;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A virtual wire.
    /// </summary>
    public class VirtualWire : ICircuitSolverPresence
    {
        private ILocatedPresence _start, _end;
        private readonly PinInfo _startInfo, _endInfo;
        private readonly WireInfo _info;
        private readonly Axis _direction;

        /// <inheritdoc />
        public string Name { get; }

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
        public string EndX => GetXName(_info.Segments.Count - 1);

        /// <summary>
        /// Gets the Y-coordinate name of the last point of the wire.
        /// </summary>
        public string EndY => GetYName(_info.Segments.Count - 1);

        /// <summary>
        /// Creates a new <see cref="VirtualWire"/>.
        /// </summary>
        /// <param name="name">The name of the virtual wire.</param>
        /// <param name="pinToWire">The pin starting the virtual wire.</param>
        /// <param name="info">The wire information.</param>
        /// <param name="wireToPin">The pin ending the virtual wire.</param>
        public VirtualWire(string name, PinInfo pinToWire, WireInfo info, PinInfo wireToPin, Axis axis)
        {
            Name = name;
            _info = info;
            _startInfo = pinToWire;
            _endInfo = wireToPin;
            _direction = axis;
        }

        /// <inheritdoc />
        public bool Reset(IResetContext context)
        {
            _start = null;
            _end = null;
            return true;
        }

        private ILocatedPresence FindPin(IPrepareContext context, PinInfo pin, int defaultIndex)
        {
            if (context.Find(pin.Component.Fullname) is not ILocatedDrawable p)
            {
                context.Diagnostics?.Post(pin.Component.Name, ErrorCodes.ComponentWithoutLocation, pin.Component.Name.Content);
                return null;
            }
            if (pin.Pin.Content.Length == 0)
                return p;
            else
                return pin.Find(p, context.Diagnostics, defaultIndex);
        }

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            if (context.Mode == PreparationMode.Offsets)
            {
                // Find the start and end locations
                _start = FindPin(context, _startInfo, -1);
                _end = FindPin(context, _endInfo, 0);
                if (_start == null || _end == null)
                    return PresenceResult.GiveUp;

                for (int i = 0; i < _info.Segments.Count; i++)
                {
                    var segment = _info.Segments[i];
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
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public bool DiscoverNodeRelationships(IRelationshipContext context)
        {
            bool doX = (_direction & Axis.X) != 0;
            bool doY = (_direction & Axis.Y) != 0;

            string x, y;
            switch (context.Mode)
            {
                case NodeRelationMode.Offsets:
                    if (_start != null)
                    {
                        if (doX)
                        {
                            if (!context.Offsets.Group(_start.X, StartX, 0.0))
                            {
                                context.Diagnostics?.Post(_info.Segments[0].Source, ErrorCodes.CannotAlignAlongX, _start.X, StartX);
                                return false;
                            }
                        }
                        if (doY)
                        {
                            if (!context.Offsets.Group(_start.Y, StartY, 0.0))
                            {
                                context.Diagnostics?.Post(_info.Segments[0].Source, ErrorCodes.CannotAlignAlongY, _start.Y, StartY);
                                return false;
                            }
                        }
                    }
                    if (_end != null)
                    {
                        if (doX)
                        {
                            if (!context.Offsets.Group(_end.X, EndX, 0.0))
                            {
                                context.Diagnostics?.Post(_info.Segments[^1].Source, ErrorCodes.CannotAlignAlongX, _end.X, EndX);
                                return false;
                            }
                        }
                        if (doY)
                        {
                            if (!context.Offsets.Group(_end.Y, EndY, 0.0))
                            {
                                context.Diagnostics?.Post(_info.Segments[^1].Source, ErrorCodes.CannotAlignAlongY, _end.Y, EndY);
                                return false;
                            }
                        }
                    }

                    // Deal with the horizontal and vertical segments
                    x = StartX;
                    y = StartY;
                    for (int i = 0; i < _info.Segments.Count; i++)
                    {
                        string tx = GetXName(i);
                        string ty = GetYName(i);
                        var segment = _info.Segments[i];
                        if (segment.IsFixed)
                        {
                            if (doX)
                            {
                                if (!context.Offsets.Group(x, tx, segment.Orientation.X * segment.Length))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(segment.Orientation.X * segment.Length), segment.Source.Content);
                                    return false;
                                }
                            }
                            if (doY)
                            {
                                if (!context.Offsets.Group(y, ty, segment.Orientation.Y * segment.Length))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(segment.Orientation.Y * segment.Length), segment.Source.Content);
                                    return false;
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
                                    return false;
                                }
                            }
                            if (segment.Orientation.Y.IsZero())
                            {
                                if (!context.Offsets.Group(y, ty, 0.0))
                                {
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, 0.0, segment.Source.Content);
                                    return false;
                                }
                            }
                        }
                        x = tx;
                        y = ty;
                    }
                    break;

                case NodeRelationMode.Links:
                    var offsetX = context.Offsets[StartX];
                    var offsetY = context.Offsets[StartY];
                    for (int i = 0; i < _info.Segments.Count; i++)
                    {
                        var toOffsetX = context.Offsets[GetXName(i)];
                        var toOffsetY = context.Offsets[GetYName(i)];
                        var segment = _info.Segments[i];
                        if (!segment.IsFixed)
                        {
                            if (doX && !segment.Orientation.X.IsZero())
                            {
                                if (!MinimumConstraint.MinimumDirectionalLink(context, offsetX, toOffsetX, segment.Orientation.X * segment.Length))
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumOfForInX, Math.Abs(segment.Orientation.X * segment.Length), segment.Source.Content);
                            }
                            if (doY && !segment.Orientation.Y.IsZero())
                            {
                                if (!MinimumConstraint.MinimumDirectionalLink(context, offsetY, toOffsetY, segment.Orientation.Y * segment.Length))
                                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumOfForInY, Math.Abs(segment.Orientation.Y * segment.Length), segment.Source.Content);
                            }
                        }
                        offsetX = toOffsetX;
                        offsetY = toOffsetY;
                    }
                    break;
            }
            return true;
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
            for (int i = 0; i < _info.Segments.Count; i++)
            {
                string x = GetXName(i);
                string y = GetYName(i);
                var toX = map[x];
                var toY = map[y];
                var segment = _info.Segments[i];
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
                        {
                            // Both axis alignment
                            string ox, oy;
                            double dx = fromX.Offset - toX.Offset;
                            double dy = fromY.Offset - toY.Offset;
                            if (dx.IsZero())
                                ox = fromX.Representative;
                            else
                            {
                                ox = $"{x}.ox";
                                context.Circuit.Add(new VoltageSource($"V{x}.ox", ox, fromX.Representative, fromX.Offset - toX.Offset));
                            }
                            if (dy.IsZero())
                                oy = fromY.Representative;
                            else
                            {
                                oy = $"{y}.oy";
                                context.Circuit.Add(new VoltageSource($"V{y}.oy", oy, fromY.Representative, fromY.Offset - toY.Offset));
                            }
                            string tx = toX.Representative;
                            string ty = toY.Representative;

                            // General case, in any direction
                            // Link the X and Y coordinates such that the slope remains correct
                            string inside = $"{x}.xc";
                            MinimumConstraint.AddRectifyingElement(context.Circuit, $"D{inside}", inside, tx);
                            context.Circuit.Add(new VoltageControlledVoltageSource($"E{inside}", inside, ox, ty, oy, segment.Orientation.X / segment.Orientation.Y));

                            inside = $"{y}.c";
                            MinimumConstraint.AddRectifyingElement(context.Circuit, $"D{inside}", inside, ty);
                            context.Circuit.Add(new VoltageControlledVoltageSource($"E{inside}", inside, oy, tx, ox, segment.Orientation.Y / segment.Orientation.X));

                            // Make sure the X and Y length cannot go below their theoretical minimum
                            MinimumConstraint.AddDirectionalMinimum(context.Circuit, $"{x}.mx", ox, tx, segment.Orientation.X * segment.Length);
                            MinimumConstraint.AddDirectionalMinimum(context.Circuit, $"{y}.my", oy, ty, segment.Orientation.Y * segment.Length);
                        }
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
