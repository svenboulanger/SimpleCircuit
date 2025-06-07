using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Evaluator
{
    /// <summary>
    /// A scope.
    /// </summary>
    /// <param name="parent">The parent scope.</param>
    public class Scope(Scope parent = null)
    {
        private readonly struct DefaultProperty(Token property, object value)
        {
            public Token Property { get; } = property;
            public object Value { get; } = value;
        }
        private class DefaultOptions(string filter)
        {
            public Regex Filter { get; } = new Regex(filter);
            public HashSet<string> Includes { get; } = [];
            public HashSet<string> Excludes { get; } = [];
            public List<DefaultProperty> Properties { get; } = [];
        }
        private readonly Dictionary<string, object> _parameterValues = [];
        private readonly Scope _parentScope = parent;
        private readonly HashSet<string> _usedParameters = parent?._usedParameters ?? [];
        private readonly List<DefaultOptions> _defaultOptions = [];

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
            get => _parameterValues[name];
            set => _parameterValues[name] = value;
        }

        /// <summary>
        /// Tries to get the value of a parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>Returns <c>true</c> if the parameter exists; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string name, out object value) => _parameterValues.TryGetValue(name, out value);

        /// <summary>
        /// Applies the defaults to a drawable for the current scope.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public void ApplyDefaults(string name, IDrawable drawable, IDiagnosticHandler diagnostics)
        {
            // First give the parent scope to apply its defaults
            _parentScope?.ApplyDefaults(name, drawable, diagnostics);

            // Then go through the defaults of the current scope
            foreach (var defaultOption in _defaultOptions)
            {
                if (!defaultOption.Filter.IsMatch(name))
                    continue;

                // Remove variants
                foreach (string exclude in defaultOption.Excludes)
                    drawable.Variants.Remove(exclude);

                // Add variants
                foreach (string include in defaultOption.Includes)
                    drawable.Variants.Add(include);

                // Handle default properties
                foreach (var defaultProperty in defaultOption.Properties)
                    drawable.SetProperty(defaultProperty.Property, defaultProperty.Value, diagnostics);
            }
        }

        /// <summary>
        /// Adds default options for the given filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="includes">The included variants.</param>
        /// <param name="excludes">The removed variants.</param>
        /// <param name="properties">The properties.</param>
        public void AddDefault(string filter, IEnumerable<string> includes, IEnumerable<string> excludes, IEnumerable<(Token Name, object Value)> properties)
        {
            // Modify the filter
            filter = $"(?<=^|/){filter.Replace("*", ".*")}";
            var r = new DefaultOptions(filter);

            if (includes is not null)
            {
                foreach (string include in includes)
                    r.Includes.Add(include);
            }
            
            if (excludes is not null)
            {
                foreach (string exclude in excludes)
                    r.Excludes.Add(exclude);
            }

            if (properties is not null)
            {
                foreach (var property in properties)
                    r.Properties.Add(new(property.Name, property.Value));
            }

            _defaultOptions.Add(r);
        }
    }
}
