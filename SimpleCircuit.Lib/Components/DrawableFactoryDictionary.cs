using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A dictionary of factor for drawables.
    /// </summary>
    public class DrawableFactoryDictionary
    {
        private class KeyNode
        {
            public IDrawableFactory Factory { get; set; }
            public Dictionary<char, KeyNode> Continuations { get; } = new();
        }
        private readonly KeyNode _root = new();
        private int _anonymousIndex = 0;

        /// <summary>
        /// Occurs when an anonymous name has been found.
        /// </summary>
        public event EventHandler<AnonymousFoundEventArgs> AnonymousFound;

        /// <summary>
        /// Gets all factories.
        /// </summary>
        public IEnumerable<IDrawableFactory> Factories
        {
            get
            {
                var set = new HashSet<IDrawableFactory>();
                GetFactories(_root, set);
                return set;
            }
        }

        private void GetFactories(KeyNode node, HashSet<IDrawableFactory> factories)
        {
            foreach (var child in node.Continuations.Values)
                GetFactories(child, factories);
            if (node.Factory != null)
                factories.Add(node.Factory);
        }

        /// <summary>
        /// Registers a factory for drawables.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public void Register(IDrawableFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            foreach (string key in factory.Metadata.SelectMany(metadata => metadata.Keys))
            {
                var elt = _root;
                for (int i = 0; i < key.Length; i++)
                {
                    if (!elt.Continuations.TryGetValue(key[i], out var nelt))
                    {
                        nelt = new();
                        elt.Continuations.Add(key[i], nelt);
                    }
                    elt = nelt;
                }
                elt.Factory = factory;
            }
        }

        /// <summary>
        /// Register all factories in a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void RegisterAssembly(Assembly assembly)
        {
            foreach (var t in assembly.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface || t.IsGenericType)
                    continue;
                if (t.GetInterfaces().Contains(typeof(IDrawableFactory)))
                {
                    var ctor = t.GetConstructor(new Type[] { });
                    if (ctor != null)
                    {
                        var factory = (IDrawableFactory)Activator.CreateInstance(t);
                        Register(factory);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new drawable for the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <returns>The created variable.</returns>
        public IDrawable Create(string name, Options options)
        {
            var elt = _root;
            var factory = _root.Factory;
            bool isAnonymous = false;
            int end = 0;
            for (int i = 0; i < name.Length; i++)
            {
                if (!elt.Continuations.TryGetValue(name[i], out var nelt))
                {
                    isAnonymous = false;
                    break;
                }
                elt = nelt;

                // Track what factory to use and whether the name is anonymous
                if (elt.Factory != null)
                {
                    factory = elt.Factory;
                    end = i;
                    isAnonymous = true;
                }
                else
                    isAnonymous = false;
            }

            if (isAnonymous)
            {
                var args = new AnonymousFoundEventArgs(name);
                name = args.NewName ?? $"{name}:{++_anonymousIndex}";
                return factory.Create(name.Substring(0, end + 1), name, options);
            }
            else
                return factory.Create(name.Substring(0, end + 1), name, options);
        }

        /// <summary>
        /// Called when an anonymous name has been found.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnAnonymousFound(AnonymousFoundEventArgs args) => AnonymousFound?.Invoke(this, args);
    }
}
