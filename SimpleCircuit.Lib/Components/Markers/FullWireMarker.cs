using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Markers
{
    /// <summary>
    /// A class that can be applied to a full wire.
    /// </summary>
    /// <param name="start">The start marker.</param>
    /// <param name="end">The end marker.</param>
    public abstract class FullWireMarker(SegmentMarker start, SegmentMarker end) : Marker
    {
        /// <summary>
        /// Gets the start marker.
        /// </summary>
        public SegmentMarker Start { get; } = start;

        /// <summary>
        /// Gets the end marker.
        /// </summary>
        public SegmentMarker End { get; } = end;

        /// <inheritdoc />
        public override void Draw(IGraphicsBuilder builder, IStyle style, IReadOnlyList<SegmentInfo> segments)
        {
            // Draw the start marker
            if (Start is not null)
            {
                Start.Segment = 0;
                Start.Draw(builder, style, segments);
            }

            // Draw the end marker
            if (End is not null)
            {
                End.Segment = segments.Count;
                End.Draw(builder, style, segments);
            }
        }
    }

    /// <summary>
    /// An ERD one-to-one shorthand marker.
    /// </summary>
    [Drawable("one-to-one", "A one to one ERD wire shorthand", "ERD")]
    public class OneToOne() :
        FullWireMarker(new ERDOnlyOne(), new ERDOnlyOne());

    /// <summary>
    /// An ERD one-to-zeroone shorthand marker.
    /// </summary>
    [Drawable("one-to-zeroone", "A one to zero-or-one ERD wire shorthand", "ERD")]
    public class OneToZeroOne() :
        FullWireMarker(new ERDOnlyOne(), new ERDZeroOne());

    [Drawable("zeroone-to-one", "A zeroone-to-one ERD shorthand", "ERD")]
    public class ZeroOneToOne() :
        FullWireMarker(new ERDZeroOne(), new ERDOnlyOne());

    [Drawable("one-to-onemany", "A one-to-onemany ERD shorthand", "ERD")]
    public class OneToOneMany() :
        FullWireMarker(new ERDOnlyOne(), new ERDOneMany());

    [Drawable("onemany-to-one", "A onemany-to-one ERD shorthand", "ERD")]
    public class OneManyToOne() :
        FullWireMarker(new ERDOneMany(), new ERDOnlyOne());

    [Drawable("one-to-many", "A one-to-many ERD shorthand", "ERD")]
    public class OneToMany() :
        FullWireMarker(new ERDOnlyOne(), new ERDZeroMany());

    [Drawable("many-to-one", "A many-to-one ERD shorthand", "ERD")]
    public class ManyToOne() :
        FullWireMarker(new ERDZeroMany(), new ERDOnlyOne());

    [Drawable("zeroone-to-zeroone", "A zeroone-to-zeroone ERD shorthand", "ERD")]
    public class ZeroOneToZeroOne() :
        FullWireMarker(new ERDZeroOne(), new ERDZeroOne());

    [Drawable("zeroone-to-onemany", "A zeroone-to-onemany ERD shorthand", "ERD")]
    public class ZeroOneToOneMany() :
        FullWireMarker(new ERDZeroOne(), new ERDOneMany());

    [Drawable("onemany-to-zeroone", "A onemany-to-zeroone ERD shorthand", "ERD")]
    public class OneManyToZeroOne() :
        FullWireMarker(new ERDOneMany(), new ERDZeroOne());

    [Drawable("zeroone-to-many", "A zeroone-to-many ERD shorthand", "ERD")]
    public class ZeroOneToMany() :
        FullWireMarker(new ERDZeroOne(), new ERDZeroMany());

    [Drawable("many-to-zeroone", "A many-to-zeroone ERD shorthand", "ERD")]
    public class ManyToZeroOne() :
        FullWireMarker(new ERDZeroMany(), new ERDZeroOne());

    [Drawable("onemany-to-onemany", "A onemany-to-onemany ERD shorthand", "ERD")]
    public class OneManyToOneMany() :
        FullWireMarker(new ERDOneMany(), new ERDOneMany());

    [Drawable("onemany-to-many", "A onemany-to-many ERD shorthand", "ERD")]
    public class OneManyToMany() :
        FullWireMarker(new ERDOneMany(), new ERDZeroMany());

    [Drawable("many-to-onemany", "A many-to-onemany ERD shorthand", "ERD")]
    public class ManyToOneMany() :
        FullWireMarker(new ERDZeroMany(), new ERDOneMany());

    [Drawable("many-to-many", "A many-to-many ERD shorthand", "ERD")]
    public class ManyToMany() :
        FullWireMarker(new ERDZeroMany(), new ERDZeroMany());

    [Drawable("fwd-arrow", "A forward arrow", "General")]
    public class ForwardArrow() :
        FullWireMarker(null, new Arrow());

    [Drawable("bck-arrow", "A backward arrow", "General")]
    public class BackwardArrow() :
        FullWireMarker(new Arrow(), null);

    [Drawable("dbl-arrow", "A bidirectional arrow", "General")]
    public class BidirectionalArrow() :
        FullWireMarker(new Arrow(), new Arrow());
}
