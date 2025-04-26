using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A subcircuit definition.
    /// </summary>
    public class Subcircuit : IDrawableFactory
    {
        private readonly string _key;
        private readonly SubcircuitDefinitionNode _definitionNode;
        private readonly Dictionary<SubcircuitState, (GraphicalCircuit, List<Func<ILocatedDrawable, IPin>>)> _versions = [];
        private readonly DrawableFactoryDictionary _factories;
        private readonly Options _options;

        /// <inheritdoc />
        public IEnumerable<string> Keys => [_key];

        /// <summary>
        /// Creates a new subcircuit factory.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="definitionNode">The node that describes the circuit definition.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <exception cref="ArgumentNullException">Thrown if an argument is <c>null</c>.</exception>
        public Subcircuit(string key, SubcircuitDefinitionNode definitionNode, DrawableFactoryDictionary factories, Options options, IDiagnosticHandler diagnostics)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            _key = key;
            _definitionNode = definitionNode ?? throw new ArgumentNullException(nameof(definitionNode));
            _factories = factories ?? throw new ArgumentNullException(nameof(factories));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public DrawableMetadata GetMetadata(string key)
        {
            if (key == _key)
                return new DrawableMetadata(_key, $"A subcircuit of type '{_key}'.", "Subcircuit");
            return null;
        }

        /// <inheritdoc />
        public IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
            => new Instance(key, name, this);

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="type">The key of the instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent subcircuit.</param>
        public class Instance(string type, string name, Subcircuit parent) : ScaledOrientedDrawable(name)
        {
            private readonly Subcircuit _parentFactory = parent;
            private readonly object[] _properties = new object[parent._definitionNode.Statements.References.Length];
            private GraphicalCircuit _circuit;

            /// <inheritdoc />
            public override string Type { get; } = type.ToLower();

            /// <inheritdoc />
            public override bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
            {
                string name = propertyToken.Content.ToString();
                int index = Array.IndexOf(_parentFactory._definitionNode.Statements.References, name);
                if (index < 0)
                    return base.SetProperty(propertyToken, value, diagnostics);
                else
                {
                    _properties[index] = value;
                    return true;
                }
            }

            /// <summary>
            /// Applies the default properties. This should be called when the subcircuit instance is first created.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>Returns <c>true</c> if the default properties could be applied; otherwise, <c>false</c>.</returns>
            /// <exception cref="NotImplementedException">Thrown if the properties aren't valid.</exception>
            public bool ApplyDefaultProperties(EvaluationContext context)
            {
                // Find the global scope
                var globalScope = context.CurrentScope;
                while (globalScope.ParentScope?.ParentScope != null)
                    globalScope = globalScope.ParentScope;

                // Create a new evaluation context
                var evalContext = new EvaluationContext(context.Factory, context.Circuit, context.Options, context.Diagnostics, globalScope);
                var direct = new Dictionary<string, object>();
                foreach (var node in _parentFactory._definitionNode.Properties)
                {
                    switch (node)
                    {
                        case BinaryNode binary:
                            if (binary.Type == BinaryOperatorTypes.Assignment)
                            {
                                if (binary.Left is not IdentifierNode id)
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(binary.Left.Location, SeverityLevel.Error, "ERR", "Expected property name"));
                                    return false;
                                }
                                object value = StatementEvaluator.EvaluateExpression(binary.Right, evalContext);
                                direct[id.Name] = value;
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                // Fill up the default properties
                for (int i = 0; i < _properties.Length; i++)
                {
                    string name = _parentFactory._definitionNode.Statements.References[i];
                    if (!direct.TryGetValue(name, out object value))
                    {
                        if (!globalScope.TryGetValue(name, out value))
                        {
                            // Perhaps point to the references in the error message instead here
                            context.Diagnostics?.Post(new SourcesDiagnosticMessage(Sources, SeverityLevel.Error, "ERR", $"Could not find a parameter with the name '{name}'"));
                            return false;
                        }
                    }
                    _properties[i] = value;
                }
                return true;
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:

                        // First get the graphical circuit for the current state
                        if (_parentFactory._definitionNode.Statements.References.Any(item => item is null))
                        {
                            context.Diagnostics?.Post(new SourcesDiagnosticMessage(Sources, SeverityLevel.Error, "ERR", "Missing property"));
                            return PresenceResult.GiveUp;
                        }
                        var state = new SubcircuitState(_properties);
                        if (!_parentFactory._versions.TryGetValue(state, out var version))
                        {
                            var circuit = new GraphicalCircuit(context.TextFormatter);
                            var evalContext = new EvaluationContext(_parentFactory._factories, circuit, _parentFactory._options, context.Diagnostics);

                            // Apply the parameters
                            for (int i = 0; i < _properties.Length; i++)
                                evalContext.CurrentScope[_parentFactory._definitionNode.Statements.References[i]] = _properties[i];
                            StatementEvaluator.Evaluate(_parentFactory._definitionNode.Statements, evalContext);

                            // Solve the graphical circuit
                            if (!circuit.Solve(context.Diagnostics))
                                return PresenceResult.GiveUp;

                            HashSet<string> takenNames = new(StringComparer.OrdinalIgnoreCase);
                            List<string> pinNames = [];
                            List<Func<ILocatedDrawable, IPin>> pinFactories = [];
                            foreach (var pinInfo in _parentFactory._definitionNode.Pins)
                            {
                                pinNames.Clear();
                                string name = null;
                                switch (pinInfo)
                                {
                                    case PinNamePinNode pnp:
                                        // Try to find the component in the graphical circuit
                                        name = StatementEvaluator.EvaluateName(pnp.Name, evalContext);
                                        break;

                                    case LiteralNode literal:
                                        name = literal.Value.ToString();
                                        break;

                                    case IdentifierNode id:
                                        name = id.Name;
                                        break;

                                    default:
                                        context.Diagnostics?.Post(new SourceDiagnosticMessage(pinInfo.Location, SeverityLevel.Error, "ERR", "Cannot recognize pin"));
                                        return PresenceResult.GiveUp;
                                }

                                // Find the pin
                                if (!circuit.TryGetValue(name, out var presence) || presence is not IDrawable drawable)
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(pinInfo.Location, SeverityLevel.Error, "ERR", $"Could not find component '{name}' in subcircuit '{_parentFactory._key}'"));
                                    return PresenceResult.GiveUp;
                                }
                                var pinReference = new PinReference(drawable, null, pinInfo.Location);
                                var pin = pinReference.GetOrCreate(context.Diagnostics, 0);
                                var location = pin.Location;
                                switch (pin)
                                {
                                    case IOrientedPin orientedPin:
                                        var orientation = orientedPin.Orientation;
                                        pinFactories.Add(d => new FixedOrientedPin(name, "pin", d, location, orientation));
                                        break;

                                    default:
                                        pinFactories.Add(d => new FixedPin(name, "pin", d, location));
                                        break;
                                }
                            }
                            version = (circuit, pinFactories);
                        }


                        // Calculate the positions of the pins
                        Pins.Clear();
                        foreach (var pf in version.Item2)
                            Pins.Add(pf(this));
                        _circuit = version.Item1;
                        break;
                }
                return result;
            }

            /// <inheritdoc/>
            protected override void Draw(IGraphicsBuilder builder)
            {
                _circuit.Render(builder);
            }
        }
    }
}