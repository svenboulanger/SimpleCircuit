using System;

namespace SimpleCircuit.Circuits.Contexts;

/// <summary>
/// A class for grouping nodes together where we track offsets to the representative.
/// </summary>
public class NodeOffsetGrouper : Grouper<string, double>
{
    /// <inheritdoc />
    protected override double Self => 0.0;

    /// <summary>
    /// Creates a new <see cref="NodeOffsetGrouper"/>.
    /// </summary>
    public NodeOffsetGrouper()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    /// <inheritdoc />
    protected override double Invert(double link) => -link;

    /// <inheritdoc />
    protected override bool IsDuplicate(GroupItem a, GroupItem b, double link)
        => (b.Value - a.Value - link).IsZero();

    /// <inheritdoc />
    protected override double MoveLink(double linkReference, double linkMerged, double linkCurrent, double link)
        => linkReference - linkMerged + linkCurrent + link;

    /// <inheritdoc />
    protected override double NewLink(double linkReference, double link)
        => linkReference + link;
}
