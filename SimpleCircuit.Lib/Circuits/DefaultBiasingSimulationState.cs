using SimpleCircuit.Circuits.Contexts;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits
{
    /// <summary>
    /// An <see cref="IBiasingSimulationState"/> that creates an empty solution.
    /// </summary>
    public class DefaultBiasingSimulationState : IBiasingSimulationState
    {
        private readonly NodeOffsetFinder _offsets;
        private readonly Dictionary<string, Variable> _variables;

        private class Variable(string name, IVector<double> vector, int index) : IVariable<double>
        {
            private readonly IVector<double> _vector = vector;
            private readonly int _index = index;
            public double Value => _vector[_index];
            public string Name { get; } = name;
            public IUnit Unit { get; } = Units.Volt;
        }

        /// <inheritdoc />
        public IVariable<double> this[string key]
        {
            get
            {
                if (_variables.TryGetValue(key, out var variable))
                    return variable;
                return null;
            }
        }

        /// <inheritdoc />
        public IVector<double> OldSolution => null;

        /// <inheritdoc />
        public ISparsePivotingSolver<double> Solver => null;

        /// <inheritdoc />
        public IVector<double> Solution { get; }

        /// <inheritdoc />
        public IVariableMap Map { get; }

        /// <inheritdoc />
        public IEqualityComparer<string> Comparer => StringComparer.Ordinal;

        /// <inheritdoc />
        public IEnumerable<string> Keys => _variables.Keys;

        /// <inheritdoc />
        public IEnumerable<IVariable<double>> Values => _variables.Values;

        /// <inheritdoc />
        public int Count => _variables.Count;

        /// <summary>
        /// Creates a new default biasing simulation state.
        /// </summary>
        /// <param name="offsets">The offsets.</param>
        public DefaultBiasingSimulationState(NodeOffsetFinder offsets)
        {
            _offsets = offsets ?? throw new ArgumentNullException(nameof(offsets));
            _variables = [];
            Solution = new DenseVector<double>(offsets.Count);
            var map = new VariableMap(new Variable("0", Solution, 0));
            Map = map;
            int index = 1;
            foreach (string rep in offsets.Representatives)
            {
                var variable = new Variable(rep, Solution, index);
                map.Add(variable, index++);
                _variables.Add(rep, variable);
            }
        }

        /// <inheritdoc />
        public void Add(string id, IVariable<double> variable) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool ContainsKey(string key) => _variables.ContainsKey(key);

        /// <inheritdoc />
        public IVariable<double> CreatePrivateVariable(string name, IUnit unit) => throw new NotImplementedException();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IVariable<double>>> GetEnumerator()
        {
            foreach (var pair in _variables)
                yield return new KeyValuePair<string, IVariable<double>>(pair.Key, pair.Value);
        }

        /// <inheritdoc />
        public IVariable<double> GetSharedVariable(string name) => _variables[name];

        /// <inheritdoc />
        public bool TryGetValue(string key, out IVariable<double> value)
        {
            if (_variables.TryGetValue(key, out var variable))
            {
                value = variable;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
