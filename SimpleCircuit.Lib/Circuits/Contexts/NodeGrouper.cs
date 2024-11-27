using System;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A class for grouping nodes together.
    /// </summary>
    public class NodeGrouper : Grouper<string, int>
    {
        /// <inheritdoc />
        protected override int Self => 0;

        /// <summary>
        /// Creates a new <see cref="NodeGrouper"/>
        /// </summary>
        public NodeGrouper()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <inheritdoc />
        protected override int Invert(int link) => 0;

        /// <inheritdoc />
        protected override bool IsDuplicate(GroupItem a, GroupItem b, int link) => true;

        /// <inheritdoc />
        protected override int MoveLink(int linkBase, int linkMerged, int linkCurrent, int link) => 0;

        /// <inheritdoc />
        protected override int NewLink(int @base, int link) => 0;
    }
}
