﻿using System;
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
        /// <summary>
        /// Gets or sets the separator for paths.
        /// </summary>
        public static string Separator { get; set; } = "/";

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
        /// Gets the name from a full name (which can have a path).
        /// </summary>
        /// <param name="fullname">The full name.</param>
        /// <returns>The name without path.</returns>
        public static string GetName(string fullname)
        {
            int index = fullname.LastIndexOf(Separator);
            if (index < 0)
                return fullname;
            return fullname.Substring(index + Separator.Length);
        }

        /// <summary>
        /// Gets the path from a full name (without the local name).
        /// </summary>
        /// <param name="fullname">The full name.</param>
        /// <returns>The path with trailing separator.</returns>
        public static string GetPath(string fullname)
        {
            int index = fullname.LastIndexOf(Separator);
            if (index < 0)
                return "";
            return fullname.Substring(0, index + Separator.Length);
        }

        /// <summary>
        /// Creates a new drawable for the specified name.
        /// </summary>
        /// <param name="fullname">The full name of the component.</param>
        /// <param name="options">The options.</param>
        /// <returns>The created variable.</returns>
        public IDrawable Create(string fullname, Options options)
        {
            var elt = _root;
            var factory = _root.Factory;
            bool isAnonymous = false;
            int end = 0;
            string name = GetName(fullname);
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

            // We didn't find a factory for this...
            if (factory == null)
                return null;

            if (isAnonymous)
            {
                var args = new AnonymousFoundEventArgs(name);
                fullname = args.NewName != null ? GetPath(fullname) + args.NewName : $"{fullname}:{++_anonymousIndex}";
                return factory.Create(name, fullname, options);
            }
            else
            {
                return factory.Create(name[..(end + 1)], fullname, options);
            }
        }

        /// <summary>
        /// Called when an anonymous name has been found.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnAnonymousFound(AnonymousFoundEventArgs args) => AnonymousFound?.Invoke(this, args);
    }
}