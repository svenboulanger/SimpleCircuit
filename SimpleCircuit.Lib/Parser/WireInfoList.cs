using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A list of wire info's, each separated by a queued anonymous point.
    /// </summary>
    public class WireInfoList : IEnumerable<WireInfo>
    {
        private readonly List<WireInfo> _wires = [];

        /// <summary>
        /// Gets the wire info at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The wire info.</returns>
        public WireInfo this[int index] => _wires[index];

        /// <summary>
        /// Gets the number of wires.
        /// </summary>
        public int Count => _wires.Count;

        /// <summary>
        /// Adds a wire info to the list. If wires already exist, the end point of the last
        /// wire is used as the start point of the this wire.
        /// </summary>
        /// <param name="wire">The wire info.</param>
        public void Add(WireInfo wire, Token queued, ParsingContext context)
        {
            if (_wires.Count > 0)
            {
                var lw = _wires[^1];
                lw.WireToPin ??= new PinInfo(context.CreateQueuedPoint(default), queued);
                wire.PinToWire = new PinInfo(lw.WireToPin.Component, default);
            }
            _wires.Add(wire);
        }

        /// <inheritdoc />
        public IEnumerator<WireInfo> GetEnumerator() => _wires.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
