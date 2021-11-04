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
        private readonly KeySearch<Func<string, IDrawable>> _search = new KeySearch<Func<string, IDrawable>>();

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
                    var ctor = ctors?.FirstOrDefault(ct =>
                    {
                        var ps = ct.GetParameters();
                        if (ps == null || ps.Length != 1)
                            return false;
                        if (ps[0].ParameterType != typeof(string))
                            return false;
                        return true;
                    });
                    if (ctor == null)
                        continue;
                    var p = Expression.Parameter(typeof(string), "name");
                    var factory = Expression.Lambda<Func<string, IDrawable>>(Expression.New(ctor, p), p).Compile();
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
        public IDrawable Create(string name)
        {
            if (_search.Count == 0)
                RegisterAssembly(GetType().Assembly);
            _search.Search(name, out var factory);
            if (factory != null)
                return factory(name);
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
