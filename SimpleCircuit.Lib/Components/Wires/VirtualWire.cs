using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Wires;


public class VirtualWire(string name, ILocatedPresence start, IEnumerable<WireSegmentInfo> segments, ILocatedPresence end, VirtualChainConstraints flags) : ICircuitSolverPresence
{
    private readonly ILocatedPresence _start = start, _end = end;
    private string _startX, _startY, _endX, _endY;
    private readonly List<WireSegmentInfo> _segments = segments?.ToList() ?? [];
    private readonly VirtualChainConstraints _axis = flags;

    /// <inheritdoc />
    public string Name { get; } = name;

    /// <inheritdoc />
    public List<TextLocation> Sources { get; }

    /// <inheritdoc />
    public int Order => 200;

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

    /// <summary>
    /// Gets or sets the minimum length of wire segments.
    /// </summary>
    [Description("The minimum length of a wire segment. The default is 10.")]
    [Alias("ml")]
    public double MinimumLength { get; set; } = 10.0;

    /// <inheritdoc />
    public PresenceResult Prepare(IPrepareContext context)
    {
        switch (context.Mode)
        {
            case PreparationMode.Find:
                // Find the anchor points, such that all bounded components know whether they should compute their bounds
                if (_segments[0].UsesBounds && _start is IBoundedComponent boundedStart)
                {
                    var orientation = _segments[0].Orientation;
                    if (orientation.IsZero())
                    {
                        _startX = _start.X;
                        _startY = _start.Y;
                    }
                    else if (orientation.X.IsZero())
                    {
                        _startX = _start.X;
                        _startY = orientation.Y > 0.0 ? boundedStart.Bottom : boundedStart.Top;
                    }
                    else if (orientation.Y.IsZero())
                    {
                        _startX = orientation.X > 0.0 ? boundedStart.Right : boundedStart.Left;
                        _startY = _start.Y;
                    }
                    else
                    {
                        _startX = orientation.X > 0.0 ? boundedStart.Right : boundedStart.Left;
                        _startY = orientation.Y > 0.0 ? boundedStart.Bottom : boundedStart.Top;
                    }
                }
                else
                {
                    _startX = _start.X;
                    _startY = _start.Y;
                }

                if (_segments[^1].UsesBounds && _end is IBoundedComponent boundedEnd)
                {
                    var orientation = _segments[0].Orientation;
                    if (orientation.IsZero())
                    {
                        _endX = _end.X;
                        _endY = _end.Y;
                    }
                    else if (orientation.X.IsZero())
                    {
                        _endX = _end.X;
                        _endY = orientation.Y > 0.0 ? boundedEnd.Top : boundedEnd.Bottom;
                    }
                    else if (orientation.Y.IsZero())
                    {
                        _endX = orientation.X > 0.0 ? boundedEnd.Left : boundedEnd.Right;
                        _endY = _end.Y;
                    }
                    else
                    {
                        _endX = orientation.X > 0.0 ? boundedEnd.Left : boundedEnd.Right;
                        _endY = orientation.Y > 0.0 ? boundedEnd.Top : boundedEnd.Bottom;
                    }
                }
                else
                {
                    _endX = _end.X;
                    _endY = _end.Y;
                }
                break;

            case PreparationMode.Offsets:
                if (Helpers.PrepareEntryOffset(_segments[0].Source, _startX, _startY, StartX, StartY, _axis, context) == PresenceResult.GiveUp)
                    return PresenceResult.GiveUp;
                if (Helpers.PrepareEntryOffset(_segments[^1].Source, _endX, _endY, EndX, EndY, _axis, context) == PresenceResult.GiveUp)
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
            double length = segment.Length < 0 ? MinimumLength : segment.Length;

            if (doY && orientation.X.IsZero() && !StringComparer.Ordinal.Equals(fromY.Representative, toY.Representative))
            {
                if (!MinimumConstraint.AddDirectionalMinimum(context.Circuit, y, fromY, toY, orientation.Y, length))
                    context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
            }
            if (doX && orientation.Y.IsZero() && !StringComparer.Ordinal.Equals(fromX.Representative, toX.Representative))
            {
                if (!MinimumConstraint.AddDirectionalMinimum(context.Circuit, x, fromX, toX, orientation.X, length))
                    context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
            }

            if (!orientation.X.IsZero() && !orientation.Y.IsZero())
            {
                // The wire definition is an odd angle, the axis becomes important
                if (doX && doY)
                    MinimumConstraint.AddDirectionalMinimum(context.Circuit, $"{x}.{y}", fromX, fromY, toX, toY, orientation, length);
                else if (doX)
                {
                    if (!MinimumConstraint.AddDirectionalMinimum(context.Circuit, x, fromX, toX, orientation.X, length))
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
                }
                else
                {
                    if (!MinimumConstraint.AddDirectionalMinimum(context.Circuit, y, fromY, toY, orientation.Y, length))
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
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
