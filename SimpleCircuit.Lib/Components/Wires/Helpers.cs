using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.Nodes;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Helper methods for wires.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Prepares the offsets between an entry to a wire.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="located">The presence.</param>
        /// <param name="x">The X-coordinate.</param>
        /// <param name="y">The Y-coordinate.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="context">The context.</param>
        /// <returns>Returns the result.</returns>
        public static PresenceResult PrepareEntryOffset(TextLocation source, ILocatedPresence located, string x, string y, VirtualChainConstraints axis, IPrepareContext context)
        {
            if ((axis & VirtualChainConstraints.X) != 0)
            {
                if (!context.Offsets.Group(located.X, x, 0.0))
                {
                    context.Diagnostics?.Post(source, ErrorCodes.CannotAlignAlongX, located.X, x);
                    return PresenceResult.GiveUp;
                }
            }
            if ((axis & VirtualChainConstraints.Y) != 0)
            {
                if (!context.Offsets.Group(located.Y, y, 0.0))
                {
                    context.Diagnostics?.Post(source, ErrorCodes.CannotAlignAlongX, located.X, x);
                    return PresenceResult.GiveUp;
                }
            }
            return PresenceResult.Success;
        }

        /// <summary>
        /// Prepares the offsets between nodes of segments.
        /// </summary>
        /// <param name="nameX">The function for generating the name of an X-node.</param>
        /// <param name="nameY">The function for generating the name of an Y-node.</param>
        /// <param name="orientation">The function for generating the orientation of segment.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="context">The context.</param>
        /// <returns>Returns the result.</returns>
        public static PresenceResult PrepareSegmentsOffset(
            Func<int, string> nameX, Func<int, string> nameY, Func<int, Vector2> orientation,
            IReadOnlyList<WireSegmentInfo> segments, VirtualChainConstraints axis, IPrepareContext context)
        {
            var result = PresenceResult.Success;
            string x = nameX(-1);
            string y = nameY(-1);
            for (int i = 0; i < segments.Count; i++)
            {
                string tx = nameX(i);
                string ty = nameY(i);
                var n = orientation(i);

                // Ignore unconstrained wires
                if (double.IsNaN(n.X) || double.IsNaN(n.Y))
                    continue;

                switch (PrepareSegmentOffset(x, y, tx, ty, segments[i], n, axis, context))
                {
                    case PresenceResult.GiveUp: return PresenceResult.GiveUp;
                    case PresenceResult.Incomplete: result = PresenceResult.Incomplete; break;
                }
                x = tx;
                y = ty;
            }
            return result;
        }

        /// <summary>
        /// Prepares the offsets between nodes.
        /// </summary>
        /// <param name="x">The first X-coordinate.</param>
        /// <param name="y">The first Y-coordinate.</param>
        /// <param name="tx">The second X-coordinate.</param>
        /// <param name="ty">The second Y-coordinate.</param>
        /// <param name="segment">The wire segment.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="context">The prepare context.</param>
        /// <returns>Returns the result.</returns>
        public static PresenceResult PrepareSegmentOffset(string x, string y, string tx, string ty,
            WireSegmentInfo segment, Vector2 orientation, VirtualChainConstraints axis, IPrepareContext context)
        {
            // Ignore any unconstrained wires (no offsets can be defined then)
            if (double.IsNaN(segment.Orientation.X) || double.IsNaN(segment.Orientation.Y))
                return PresenceResult.Success;

            double l = segment.Length;
            if (!segment.IsMinimum)
            {
                // Fixed length wire
                if ((axis & VirtualChainConstraints.X) != 0 &&
                    !context.Offsets.Group(x, tx, orientation.X * l))
                {
                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(orientation.X * l));
                    return PresenceResult.GiveUp;
                }
                if ((axis & VirtualChainConstraints.Y) != 0 &&
                    !context.Offsets.Group(y, ty, orientation.Y * l))
                {
                    context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(orientation.Y * l));
                    return PresenceResult.GiveUp;
                }
                return PresenceResult.Success;
            }
            else
            {
                // Wire has minimum length
                if (orientation.X.IsZero())
                {
                    if ((axis & VirtualChainConstraints.X) != 0)
                    {
                        if (!context.Offsets.Group(x, tx, 0.0))
                        {
                            context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, Math.Abs(orientation.X * l));
                            return PresenceResult.GiveUp;
                        }
                    }
                    if ((axis & VirtualChainConstraints.Y) != 0)
                        context.Offsets.Add(ty);
                }
                else if (orientation.Y.IsZero())
                {
                    if ((axis & VirtualChainConstraints.Y) != 0)
                    {
                        if (!context.Offsets.Group(y, ty, 0.0))
                        {
                            context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotAlignAlongY, y, ty);
                            return PresenceResult.GiveUp;
                        }
                    }
                    if ((axis & VirtualChainConstraints.X) != 0)
                        context.Offsets.Add(tx);
                }
                else
                {
                    // Perform a check for slanted angles
                    if ((axis & VirtualChainConstraints.X) != 0 &&
                        (axis & VirtualChainConstraints.Y) != 0)
                    {
                        bool isFixedX = context.Offsets.AreGrouped(x, tx);
                        bool isFixedY = context.Offsets.AreGrouped(y, ty);
                        if (isFixedX && isFixedY)
                        {
                            // The coordinates are already completely constrained
                            double dx = context.Offsets.GetValue(tx) - context.Offsets.GetValue(x);
                            double dy = context.Offsets.GetValue(ty) - context.Offsets.GetValue(y);
                            var actualOrientation = new Vector2(dx, dy);
                            actualOrientation /= actualOrientation.Length;
                            if (orientation.Dot(actualOrientation) < 0.999)
                            {
                                context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotAlignDirection);
                                return PresenceResult.GiveUp;
                            }

                            // Check whether the minimum distance is OK
                            if (dx * dx + dy * dy > segment.Length * segment.Length + 0.001)
                                context.Diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumDistance);
                        }
                        else if (isFixedX)
                        {

                            // This should lead to a fixed Y as well
                            double dx = context.Offsets.GetValue(tx) - context.Offsets.GetValue(x);
                            double dy = dx / orientation.X * orientation.Y;
                            if (!context.Offsets.Group(y, ty, dy))
                            {
                                context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, dy);
                                return PresenceResult.GiveUp;
                            }

                            // Check whether the minimum distance is OK
                            if (dx * dx + dy * dy > segment.Length * segment.Length + 0.001)
                                context.Diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumDistance);
                        }
                        else if (isFixedY)
                        {
                            // This should lead to a fixed X as well
                            double dy = context.Offsets.GetValue(ty) - context.Offsets.GetValue(y);
                            double dx = dy / orientation.Y * orientation.X;
                            if (!context.Offsets.Group(x, tx, dx))
                            {
                                context.Diagnostics?.Post(segment.Source, ErrorCodes.CannotResolveFixedOffsetFor, dy);
                                return PresenceResult.GiveUp;
                            }

                            // Check whether the minimum distance is OK
                            if (dx * dx + dy * dy > segment.Length + 0.001)
                                context.Diagnostics?.Post(segment.Source, ErrorCodes.CouldNotSatisfyMinimumOfForInY);
                        }
                        else if (context.Desparateness == DesperatenessLevel.Normal)
                            return PresenceResult.Incomplete;
                        else
                        {
                            context.Offsets.Add(tx);
                            context.Offsets.Add(ty);
                        }
                    }
                }
                return PresenceResult.Success;
            }
        }

        /// <summary>
        /// Prepares the segments grouping.
        /// </summary>
        /// <param name="nameX">The function for getting the X-node name.</param>
        /// <param name="nameY">The function for getting the Y-node name.</param>
        /// <param name="segments">The wire segments.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="context">The context.</param>
        /// <returns>Returns the result.</returns>
        public static PresenceResult PrepareSegmentsGroup(
            Func<int, string> nameX, Func<int, string> nameY, IReadOnlyList<WireSegmentInfo> segments,
            VirtualChainConstraints axis, IPrepareContext context)
        {
            var result = PresenceResult.Success;
            string sx = nameX(-1);
            string sy = nameY(-1);
            for (int i = 0; i < segments.Count; i++)
            {
                switch (PrepareSegmentGroup(sx, sy, nameX(i), nameY(i), axis, context))
                {
                    case PresenceResult.GiveUp: return PresenceResult.GiveUp;
                    case PresenceResult.Incomplete: result = PresenceResult.Incomplete; break;
                }
            }
            return result;
        }

        /// <summary>
        /// Prepares the groups between nodes.
        /// </summary>
        /// <param name="x">The first X-coordinate.</param>
        /// <param name="y">The first Y-coordinate.</param>
        /// <param name="tx">The second X-coordinate.</param>
        /// <param name="ty">The second Y-coordinate.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="context">The prepare context.</param>
        /// <returns>Returns the result.</returns>
        public static PresenceResult PrepareSegmentGroup(string x, string y, string tx, string ty, VirtualChainConstraints axis, IPrepareContext context)
        {
            if ((axis & VirtualChainConstraints.X) != 0)
                context.Group(x, tx);
            if ((axis & VirtualChainConstraints.Y) != 0)
                context.Group(y, ty);
            return PresenceResult.Success;
        }
    }
}
