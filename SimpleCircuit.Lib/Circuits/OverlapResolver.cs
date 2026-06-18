using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;

namespace SimpleCircuit;

/// <summary>
/// Detects overlapping components inside connected groups and builds <see cref="MinimumConstraint"/>s
/// that push them apart. The constraints are fed back into the solver so wires re-route consistently.
/// </summary>
/// <remarks>
/// See <c>OVERLAP_SPACING_PLAN.md</c> for the design rationale.
/// </remarks>
internal static class OverlapResolver
{
    // Two expanded bounding boxes are only treated as overlapping when they interpenetrate by more
    // than this amount. This makes a pair that was just separated to exactly the requested gap stop
    // triggering (the expanded boxes then merely touch).
    private const double _overlapTolerance = 1e-6;

    /// <summary>
    /// Describes a pair of overlapping drawables together with their world-space bounds.
    /// </summary>
    public readonly struct OverlapPair(ILocatedDrawable a, Bounds boundsA, ILocatedDrawable b, Bounds boundsB)
    {
        /// <summary>Gets the first drawable.</summary>
        public ILocatedDrawable A { get; } = a;

        /// <summary>Gets the world-space bounds of the first drawable.</summary>
        public Bounds BoundsA { get; } = boundsA;

        /// <summary>Gets the second drawable.</summary>
        public ILocatedDrawable B { get; } = b;

        /// <summary>Gets the world-space bounds of the second drawable.</summary>
        public Bounds BoundsB { get; } = boundsB;
    }

    private readonly struct Item(ILocatedDrawable drawable, Bounds bounds, Bounds expanded)
    {
        public ILocatedDrawable Drawable { get; } = drawable;
        public Bounds Bounds { get; } = bounds;
        public Bounds Expanded { get; } = expanded;
    }

    /// <summary>
    /// Detects overlapping pairs of components within each connected group. Only the component
    /// bounding boxes are used. Uses a sweep-line so the cost is O(n log n) per group.
    /// </summary>
    /// <param name="circuit">The graphical circuit (used for the text formatter).</param>
    /// <param name="prepareContext">The prepare context with the connectivity groups.</param>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <param name="margin">The desired separation margin in X- and Y-direction.</param>
    /// <returns>Returns the list of overlapping pairs.</returns>
    public static List<OverlapPair> DetectOverlaps(GraphicalCircuit circuit, PrepareContext prepareContext, IDiagnosticHandler diagnostics, Vector2 margin)
    {
        var result = new List<OverlapPair>();
        var half = new Margins(margin.X * 0.5, margin.Y * 0.5, margin.X * 0.5, margin.Y * 0.5);
        BoundsBuilder builder = null;
        var items = new List<Item>();
        var active = new List<Item>();

        foreach (var pair in prepareContext.DrawnGroups.Groups)
        {
            var drawables = pair.Value.Drawables;
            if (drawables.Count < 2)
                continue; // A single component cannot overlap itself.

            // Gather located drawables together with their world-space bounds. Rendering a drawable
            // updates its Bounds with the solved transform, which is exactly what we need.
            items.Clear();
            foreach (var drawable in drawables)
            {
                if (drawable is not ILocatedDrawable located)
                    continue;
                builder ??= new BoundsBuilder(circuit.TextFormatter, prepareContext.Style, diagnostics);
                drawable.Render(builder);
                items.Add(new Item(located, drawable.Bounds, drawable.Bounds.Expand(half)));
            }
            if (items.Count < 2)
                continue;

            // Sweep-line over X using the expanded bounds.
            items.Sort(static (x, y) => x.Expanded.Left.CompareTo(y.Expanded.Left));
            active.Clear();
            foreach (var item in items)
            {
                double left = item.Expanded.Left;

                // Drop active items that can no longer overlap in X.
                for (int i = active.Count - 1; i >= 0; i--)
                {
                    if (active[i].Expanded.Right < left)
                        active.RemoveAt(i);
                }

                // The remaining active items overlap in X by construction; test the Y-overlap.
                foreach (var other in active)
                {
                    double penX = Math.Min(item.Expanded.Right, other.Expanded.Right) - Math.Max(item.Expanded.Left, other.Expanded.Left);
                    double penY = Math.Min(item.Expanded.Bottom, other.Expanded.Bottom) - Math.Max(item.Expanded.Top, other.Expanded.Top);
                    if (penX > _overlapTolerance && penY > _overlapTolerance)
                        result.Add(new OverlapPair(other.Drawable, other.Bounds, item.Drawable, item.Bounds));
                }
                active.Add(item);
            }
        }
        return result;
    }

    /// <summary>
    /// Builds the minimum constraints that separate the detected overlapping pairs. At most one
    /// constraint is created per pair (along the least-disruptive axis).
    /// </summary>
    /// <param name="overlaps">The overlapping pairs.</param>
    /// <param name="prepareContext">The prepare context.</param>
    /// <param name="solvedCircuit">The just-solved circuit, used to derive the existing ordering.</param>
    /// <param name="margin">The desired separation margin in X- and Y-direction.</param>
    /// <param name="constrainedPairs">
    /// The set of pairs that already received an overlap constraint. Pairs in this set are skipped and
    /// newly-constrained pairs are added. This guarantees the resolution loop terminates.
    /// </param>
    /// <param name="counter">A counter used to generate unique constraint names.</param>
    /// <returns>Returns the new constraints.</returns>
    public static List<ICircuitPresence> BuildConstraints(
        List<OverlapPair> overlaps,
        PrepareContext prepareContext,
        IEntityCollection solvedCircuit,
        Vector2 margin,
        HashSet<string> constrainedPairs,
        ref int counter)
    {
        var result = new List<ICircuitPresence>();
        var graph = BuildOrderingGraph(solvedCircuit, prepareContext);

        foreach (var overlap in overlaps)
        {
            string key = PairKey(overlap.A, overlap.B);
            if (constrainedPairs.Contains(key))
                continue;

            var constraint = BuildConstraint(overlap, prepareContext, graph, margin, ref counter);
            if (constraint is not null)
            {
                constrainedPairs.Add(key);
                result.Add(constraint);
            }
        }
        return result;
    }

    private static MinimumConstraint BuildConstraint(OverlapPair pair, PrepareContext context, OrderingGraph graph, Vector2 margin, ref int counter)
    {
        var a = pair.A;
        var b = pair.B;
        var ba = pair.BoundsA;
        var bb = pair.BoundsB;

        // Representatives for rigidity testing: equal representatives mean the relative position on
        // that axis is fixed and cannot be moved by the solver.
        string repAx = context.Offsets.GetRepresentative(a.X);
        string repBx = context.Offsets.GetRepresentative(b.X);
        string repAy = context.Offsets.GetRepresentative(a.Y);
        string repBy = context.Offsets.GetRepresentative(b.Y);
        bool rigidX = StringComparer.OrdinalIgnoreCase.Equals(repAx, repBx);
        bool rigidY = StringComparer.OrdinalIgnoreCase.Equals(repAy, repBy);
        if (rigidX && rigidY)
            return null; // Nothing we can move - leave the overlap as-is.

        // Penetration depths including the margin (smaller penetration = least-disruptive push).
        double penX = Math.Min(ba.Right, bb.Right) - Math.Max(ba.Left, bb.Left) + margin.X;
        double penY = Math.Min(ba.Bottom, bb.Bottom) - Math.Max(ba.Top, bb.Top) + margin.Y;

        // Choose the separation axis, avoiding axes that are rigid.
        bool useX;
        if (rigidX)
            useX = false;
        else if (rigidY)
            useX = true;
        else if (penX < penY)
            useX = true;
        else if (penY < penX)
            useX = false;
        else
        {
            // Tie: prefer the axis with the larger centre separation, falling back to X.
            double sepX = Math.Abs(ba.Center.X - bb.Center.X);
            double sepY = Math.Abs(ba.Center.Y - bb.Center.Y);
            useX = sepX >= sepY;
        }

        if (useX)
        {
            // B.Left >= A.Right + margin  (when A is the left/near side) and the mirror image.
            double minWhenALow = (ba.Right - a.Location.X) - (bb.Left - b.Location.X) + margin.X;
            double minWhenBLow = (bb.Right - b.Location.X) - (ba.Left - a.Location.X) + margin.X;
            return Decide(repAx, repBx, a.X, b.X, ba.Center.X, bb.Center.X, minWhenALow, minWhenBLow, a, b, graph, ref counter);
        }
        else
        {
            // B.Top >= A.Bottom + margin  (when A is the upper/near side; smaller Y is higher).
            double minWhenALow = (ba.Bottom - a.Location.Y) - (bb.Top - b.Location.Y) + margin.Y;
            double minWhenBLow = (bb.Bottom - b.Location.Y) - (ba.Top - a.Location.Y) + margin.Y;
            return Decide(repAy, repBy, a.Y, b.Y, ba.Center.Y, bb.Center.Y, minWhenALow, minWhenBLow, a, b, graph, ref counter);
        }
    }

    /// <summary>
    /// Decides the direction of the separation so it agrees with the existing partial order, then
    /// creates the constraint and records the new edge to keep later pairs consistent.
    /// </summary>
    private static MinimumConstraint Decide(
        string repA, string repB, string nodeA, string nodeB,
        double centerA, double centerB, double minWhenALow, double minWhenBLow,
        ILocatedDrawable a, ILocatedDrawable b, OrderingGraph graph, ref int counter)
    {
        bool aReachesB = graph.Reachable(repA, repB); // B already constrained to be >= A.
        bool bReachesA = graph.Reachable(repB, repA);

        bool aLow;
        if (aReachesB && !bReachesA)
            aLow = true; // Existing order A <= B - just widen the gap.
        else if (bReachesA && !aReachesB)
            aLow = false; // Existing order B <= A.
        else if (!aReachesB && !bReachesA)
        {
            // Incomparable: choose by the current centres, tie-break by name for reproducibility.
            if (centerA < centerB)
                aLow = true;
            else if (centerA > centerB)
                aLow = false;
            else
                aLow = string.CompareOrdinal(a.Name, b.Name) <= 0;
        }
        else
            return null; // Already constrained in both directions (should not happen) - bail out.

        string lowRep = aLow ? repA : repB;
        string highRep = aLow ? repB : repA;
        string lowNode = aLow ? nodeA : nodeB;
        string highNode = aLow ? nodeB : nodeA;
        double minimum = aLow ? minWhenALow : minWhenBLow;

        // Keep the per-axis ordering acyclic for the remaining pairs in this pass.
        graph.AddEdge(lowRep, highRep);
        return new MinimumConstraint($"overlap.{counter++}", lowNode, highNode, minimum);
    }

    private static OrderingGraph BuildOrderingGraph(IEntityCollection circuit, PrepareContext context)
    {
        var graph = new OrderingGraph();
        if (circuit is null)
            return graph;

        foreach (var entity in circuit)
        {
            switch (entity)
            {
                case Components.Constraints.MinimumConstraints.MinimumConstraint mc:
                    // Nodes[0] is the highest coordinate, Nodes[1] the lowest -> edge lowest -> highest.
                    graph.AddEdge(
                        context.Offsets.GetRepresentative(mc.Nodes[1]),
                        context.Offsets.GetRepresentative(mc.Nodes[0]));
                    break;

                case Components.Constraints.SlopedMinimumConstraints.SlopedMinimumConstraint smc:
                    // Nodes are (x1, y1, x2, y2). Only axis-aligned normals map to a clean per-axis edge.
                    var normal = smc.Parameters.Normal;
                    if (normal.Y.IsZero() && !normal.X.IsZero())
                    {
                        string x1 = context.Offsets.GetRepresentative(smc.Nodes[0]);
                        string x2 = context.Offsets.GetRepresentative(smc.Nodes[2]);
                        if (normal.X > 0)
                            graph.AddEdge(x1, x2);
                        else
                            graph.AddEdge(x2, x1);
                    }
                    else if (normal.X.IsZero() && !normal.Y.IsZero())
                    {
                        string y1 = context.Offsets.GetRepresentative(smc.Nodes[1]);
                        string y2 = context.Offsets.GetRepresentative(smc.Nodes[3]);
                        if (normal.Y > 0)
                            graph.AddEdge(y1, y2);
                        else
                            graph.AddEdge(y2, y1);
                    }
                    // Diagonal normals couple both axes and imply no hard per-axis order -> skipped.
                    break;
            }
        }
        return graph;
    }

    private static string PairKey(ILocatedDrawable a, ILocatedDrawable b)
        => string.CompareOrdinal(a.Name, b.Name) <= 0 ? $"{a.Name}\0{b.Name}" : $"{b.Name}\0{a.Name}";

    /// <summary>
    /// A directed graph over node representatives used to capture the existing minimum-distance
    /// ordering and to test reachability (so overlap constraints never contradict it).
    /// </summary>
    private sealed class OrderingGraph
    {
        private readonly Dictionary<string, HashSet<string>> _adjacency = new(StringComparer.OrdinalIgnoreCase);

        public void AddEdge(string from, string to)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(from, to))
                return;
            if (!_adjacency.TryGetValue(from, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _adjacency.Add(from, set);
            }
            set.Add(to);
        }

        public bool Reachable(string from, string to)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(from, to))
                return true;
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { from };
            var queue = new Queue<string>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                string node = queue.Dequeue();
                if (!_adjacency.TryGetValue(node, out var set))
                    continue;
                foreach (string next in set)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(next, to))
                        return true;
                    if (visited.Add(next))
                        queue.Enqueue(next);
                }
            }
            return false;
        }
    }
}
