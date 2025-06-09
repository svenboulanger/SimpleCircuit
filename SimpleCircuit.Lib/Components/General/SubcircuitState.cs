using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A state that will represent all the inputs to a subcircuit definition.
    /// Can be used to differentiate between subcircuits with the same name, but different parameters/properties.
    /// </summary>
    public class SubcircuitState : IEquatable<SubcircuitState>
    {
        private readonly object[] _state;
        private readonly int _hash;

        /// <summary>
        /// Creates a new subcircuit state.
        /// </summary>
        /// <param name="states">The states.</param>
        public SubcircuitState(IEnumerable<object> states)
        {
            _state = [.. states];
            _hash = 0;
            foreach (object value in _state)
                _hash = (_hash * 1021) ^ value.GetHashCode();
        }

        /// <inheritdoc />
        public override int GetHashCode() => _hash;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is SubcircuitState subcktState && Equals(subcktState);

        /// <inheritdoc />
        public bool Equals(SubcircuitState other)
        {
            if (_state.Length != other._state.Length)
                return false;
            for (int i = 0; i < _state.Length; i++)
            {
                if (!_state[i].Equals(other._state[i]))
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override string ToString() => $"{{{string.Join(", ", _state)}}}";
    }
}
