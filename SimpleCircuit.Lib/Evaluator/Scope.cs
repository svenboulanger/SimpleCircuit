using System.Collections.Generic;

namespace SimpleCircuit.Evaluator
{
    /// <summary>
    /// A scope.
    /// </summary>
    /// <param name="parent">The parent scope.</param>
    public class Scope(Scope parent = null)
    {
        private readonly Dictionary<string, object> _parameters = [];
        private readonly Scope _parentScope = parent;
        private readonly HashSet<string> _usedParameters = parent?._usedParameters ?? [];

        /// <summary>
        /// Gets the parent scope.
        /// </summary>
        public Scope ParentScope => _parentScope;

        /// <summary>
        /// Gets or sets a parameter value, but only in the current scope.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>Returns the name.</returns>
        public object this[string name]
        {
            get => _parameters[name];
            set => _parameters[name] = value;
        }

        /// <summary>
        /// Tries to get the value of a parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>Returns <c>true</c> if the parameter exists; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string name, out object value)
        {
            if (_parameters.TryGetValue(name, out value))
                return true;
            if (_parentScope is not null)
                return _parentScope.TryGetValue(name, out value);
            return false;
        }
    }
}
