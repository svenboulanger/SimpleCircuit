using SimpleCircuit.Components;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A factory for components.
    /// </summary>
    public class ComponentFactory
    {
        private readonly KeySearch<Func<string, Options, IDrawable>> _search = new();

        /// <summary>
        /// Registers the types that can be used as a component in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void RegisterAssembly(Assembly assembly)
        {
            foreach (var t in assembly.GetTypes())
            {
                var attributes = t.GetCustomAttributes<SimpleKeyAttribute>(false).ToArray();
                if (attributes != null && attributes.Length > 0)
                {
                    // Create the constructor
                    var ctors = t.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                    var p = Expression.Parameter(typeof(string), "name");
                    var o = Expression.Parameter(typeof(Options), "options");
                    NewExpression ne = null;
                    foreach (var ctor in ctors)
                    {
                        var ps = ctor.GetParameters();
                        if (ps == null || ps.Length == 0 || ps.Length > 2)
                            continue;
                        if (ps[0].ParameterType != typeof(string))
                            continue;
                        if (ps.Length > 1)
                        {
                            if (ps[1].ParameterType != typeof(Options))
                                continue;
                            ne = Expression.New(ctor, p, o);
                            break;
                        }
                        else
                        {
                            ne = Expression.New(ctor, p);
                        }
                    }
                    if (ne == null)
                        return;
                    var factory = Expression.Lambda<Func<string, Options, IDrawable>>(ne, p, o).Compile();
                    foreach (var attribute in attributes)
                        _search.Add(attribute.Key, factory);
                }
            }
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IDrawable Create(string name, Options options)
        {
            if (_search.Count == 0)
                RegisterAssembly(GetType().Assembly);
            _search.Search(name, out var factory);
            if (factory != null)
                return factory(name, options);
            return null;
        }

        /// <summary>
        /// Determines whether the specified name is an exact identifier.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified name is exact; otherwise, <c>false</c>.
        /// </returns>
        public bool IsExact(string name)
        {

            if (_search.Count == 0)
                RegisterAssembly(GetType().Assembly);
            return _search.Search(name, out _);
        }
    }
}
