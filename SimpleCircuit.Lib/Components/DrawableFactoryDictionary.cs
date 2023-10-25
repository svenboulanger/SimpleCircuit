using SimpleCircuit.Components.General;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

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

        /// <summary>
        /// Gets or sets the separator for the anonymous index.
        /// </summary>
        public static string AnonymousSeparator { get; set; } = "-";

        private class KeyNode
        {
            public IDrawableFactory Factory { get; set; }
            public string Key { get; set; }
            public Dictionary<char, KeyNode> Continuations { get; } = new();
        }
        private readonly KeyNode _root = new();
        private int _anonymousIndex = 0;

        /// <summary>
        /// Gets all factories.
        /// </summary>
        public IEnumerable<KeyValuePair<string, IDrawableFactory>> Factories
        {
            get
            {
                var result = new Dictionary<string, IDrawableFactory>();
                GetFactories(_root, result);
                return result;
            }
        }

        private void GetFactories(KeyNode node, Dictionary<string, IDrawableFactory> factories)
        {
            foreach (var child in node.Continuations.Values)
                GetFactories(child, factories);
            if (node.Factory != null)
                factories[node.Key] = node.Factory;
        }

        /// <summary>
        /// Registers a factory for the specified key.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public void Register(IDrawableFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            foreach (string key in factory.Keys)
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
                elt.Key = key;
            }
        }

        /// <summary>
        /// Load XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public void Load(XmlDocument xml, IDiagnosticHandler diagnostics)
        {
            // Select all symbol tags from the root element
            bool missingKeys = false;
            foreach (XmlNode symbol in xml.DocumentElement.SelectNodes("symbol"))
            {
                // Get the key of the symbol
                string key = symbol.Attributes["key"]?.Value;
                if (string.IsNullOrEmpty(key))
                {
                    missingKeys = true;
                    continue;
                }

                // Check whether the key is a valid one
                bool isValid = char.IsLetter(key[0]);
                if (isValid)
                {
                    for (int i = 1; i < key.Length; i++)
                    {
                        if (!char.IsLetterOrDigit(key[i]))
                        {
                            isValid = false;
                            break;
                        }
                    }
                }
                if (isValid)
                {
                    // Create an Xml Drawable from the XML node
                    var drawable = new XmlDrawable(key, symbol, diagnostics);
                    Register(drawable);
                }
                else
                {
                    diagnostics?.Post(ErrorCodes.InvalidSymbolKey, key);
                }
            }
            if (missingKeys)
            {
                diagnostics?.Post(ErrorCodes.MissingSymbolKey);
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
            return fullname[(index + Separator.Length)..];
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
            return fullname[..(index + Separator.Length)];
        }

        private bool Extract(string fullname, out string key, out IDrawableFactory factory)
        {
            var elt = _root;
            bool isAnonymous = false;
            string name = GetName(fullname);
            int end = 0;
            factory = null;
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
                key = name;
                return true;
            }
            else
            {
                key = name[..(end + 1)];
                return false;
            }
        }

        /// <summary>
        /// Determines whether the full name represents an anonymous component.
        /// </summary>
        /// <param name="fullname">The full name.</param>
        /// <returns>Returns <c>true</c> if the name represents an anonymous component; otherwise, <c>false</c>.</returns>
        public bool IsAnonymous(string fullname) => Extract(fullname, out _, out _);

        /// <summary>
        /// Determines whether the given key is present in the factory.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns <c>true</c> if the key is present; otherwise, <c>false</c>.</returns>
        public bool IsKey(string key) => Extract(key, out string realKey, out _) && key == realKey;

        /// <summary>
        /// Creates a new drawable for the specified name.
        /// </summary>
        /// <param name="fullname">The full name of the component.</param>
        /// <param name="options">The options.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <returns>The created drawable, or <c>null</c> if the drawable could not be created.</returns>
        public IDrawable Create(string fullname, Options options, IDiagnosticHandler diagnostics)
        {
            bool isAnonymous = Extract(fullname, out var key, out var factory);
            if (isAnonymous)
                return factory?.Create(key, $"{fullname}{AnonymousSeparator}{++_anonymousIndex}", options, diagnostics);
            return factory?.Create(key, fullname, options, diagnostics);
        }
    }
}
