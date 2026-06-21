using SimpleCircuit.Components;
using SimpleCircuit.Components.Markers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A set of markers.
    /// </summary>
    public class MarkerSet
    {
        private readonly Dictionary<string, Func<Marker>> _factories = [];

        /// <summary>
        /// Tries to create a marker.
        /// </summary>
        /// <param name="markerName">The marker name.</param>
        /// <param name="marker">The created marker.</param>
        /// <returns>Returns <c>true</c> if the marker was created successfully; otherwise, <c>false</c>.</returns>
        public bool TryCreateMarker(string markerName, out Marker marker)
        {
            if (!_factories.TryGetValue(markerName, out var factory))
            {
                marker = null;
                return false;
            }

            marker = factory();
            return true;
        }

        /// <summary>
        /// Creates a new marker set.
        /// </summary>
        /// <param name="loadAssembly">If <c>true</c>, all markers of this assembly are loaded.</param>
        public MarkerSet(bool loadAssembly = true)
        {
            // Search for markers in the assembly
            if (loadAssembly)
            {
                foreach (var t in typeof(Marker).Assembly.GetTypes())
                {
                    if (t.IsAbstract || t.IsInterface || t.IsGenericType)
                        continue;
                    var nt = t;
                    while (nt is not null)
                    {
                        nt = nt.BaseType;
                        if (nt == typeof(Marker))
                        {
                            // Version where location and orientation is given
                            var ctor = t.GetConstructor([]);
                            if (ctor is not null)
                            {
                                Marker factory() => (Marker)Activator.CreateInstance(t);
                                foreach (var attribute in t.GetCustomAttributes<DrawableAttribute>())
                                    _factories.Add(attribute.Key, factory);
                            }
                        }
                    }
                }
            }
        }
    }
}
