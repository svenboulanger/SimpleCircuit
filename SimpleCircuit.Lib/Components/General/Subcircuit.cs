using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
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
        private readonly Dictionary<SubcircuitState, (GraphicalCircuit, List<Action<PinCollection, ILocatedDrawable>>)> _versions = [];
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
        public IDrawable Create(string key, string name, Options options, Scope scope, IDiagnosticHandler diagnostics)
        {
            var result = new Instance(key, name, this);
            return result;
        }

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
                // Note: the top-level statements are usually defined in their own scope, so we'll use
                // the sub-global scope instead of the global scope.
                var globalScope = context.CurrentScope;
                while (globalScope.ParentScope?.ParentScope != null)
                    globalScope = globalScope.ParentScope;

                // Create a new evaluation context
                var evalContext = new EvaluationContext(context.Factory, context.Circuit, context.Options, globalScope)
                {
                    Diagnostics = context.Diagnostics
                };
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
                            var circuit = new GraphicalCircuit(context.Style.ModifyDashedDotted(this), context.TextFormatter);
                            var evalContext = new EvaluationContext(_parentFactory._factories, circuit, _parentFactory._options)
                            {
                                Diagnostics = context.Diagnostics
                            };

                            // Apply the parameters
                            for (int i = 0; i < _properties.Length; i++)
                                evalContext.CurrentScope[_parentFactory._definitionNode.Statements.References[i]] = _properties[i];
                            StatementEvaluator.Evaluate(_parentFactory._definitionNode.Statements, evalContext);

                            // Solve the graphical circuit
                            if (!circuit.Solve(context.Diagnostics))
                                return PresenceResult.GiveUp;

                            HashSet<string> usedPins = [];
                            List<Action<PinCollection, ILocatedDrawable>> pinFactories = [];
                            foreach (var pinInfo in _parentFactory._definitionNode.Pins)
                            {
                                var r = RegisterPin(circuit, pinInfo, evalContext, pinFactories, usedPins, context.Diagnostics);
                                if (r == PresenceResult.GiveUp)
                                    return PresenceResult.GiveUp;
                            }
                            version = (circuit, pinFactories);
                            _parentFactory._versions.Add(state, version);
                        }

                        // Calculate the positions of the pins
                        Pins.Clear();
                        foreach (var pf in version.Item2)
                            pf(Pins, this);
                        _circuit = version.Item1;
                        break;
                }
                return result;
            }

            private PresenceResult RegisterPin(GraphicalCircuit circuit, SyntaxNode node, EvaluationContext context, List<Action<PinCollection, ILocatedDrawable>> pinFactories, HashSet<string> usedPins, IDiagnosticHandler diagnostics)
            {
                switch (node)
                {
                    case PinNamePinNode pnp:
                        {
                            // Let's try to find the drawable
                            string name = StatementEvaluator.EvaluateName(pnp.Name, context);
                            if (!circuit.TryGetValue(name, out var presence) || presence is not IDrawable drawable)
                            {
                                diagnostics?.Post(new SourceDiagnosticMessage(pnp.Name.Location, SeverityLevel.Error, "ERR", $"Could not find component '{name}' in subcircuit '{_parentFactory._key}'"));
                                return PresenceResult.GiveUp;
                            }

                            // Deal with the pins
                            if (pnp.PinLeft is not null)
                            {
                                // Find the pin on the drawable
                                string pinName = StatementEvaluator.EvaluateName(pnp.PinLeft, context);
                                if (pinName is null)
                                    return PresenceResult.GiveUp;
                                if (drawable.Pins.TryGetValue(pinName, out var pin))
                                    pinFactories.Add(CreatePinFactory(name, pinName, pin, usedPins, context.Diagnostics));
                                else
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(pnp.PinLeft.Location, SeverityLevel.Error, "ERR", $"Could not find pin '{pinName}' on '{name}'"));
                                    return PresenceResult.GiveUp;
                                }
                            }
                            if (pnp.PinRight is not null)
                            {
                                string pinName = StatementEvaluator.EvaluateName(pnp.PinRight, context);
                                if (pinName is null)
                                    return PresenceResult.GiveUp;
                                if (drawable.Pins.TryGetValue(pinName, out var pin))
                                    pinFactories.Add(CreatePinFactory(name, pinName, pin, usedPins, context.Diagnostics));
                                else
                                {
                                    context.Diagnostics?.Post(new SourceDiagnosticMessage(pnp.PinRight.Location, SeverityLevel.Error, "ERR", $"Could not find pin '{pinName}' on '{name}'"));
                                    return PresenceResult.GiveUp;
                                }
                            }
                        }
                        break;

                    case LiteralNode literal:
                        {
                            // Let's try to find the drawable
                            string name = literal.Value.ToString();
                            if (!circuit.TryGetValue(name, out var presence) || presence is not IDrawable drawable)
                            {
                                diagnostics?.Post(new SourceDiagnosticMessage(literal.Location, SeverityLevel.Error, "ERR", $"Could not find component '{name}' in subcircuit '{_parentFactory._key}'"));
                                return PresenceResult.GiveUp;
                            }

                            // Determine the name of the pin
                            if (drawable.Pins.Count == 0)
                            {
                                context.Diagnostics?.Post(new SourceDiagnosticMessage(literal.Location, SeverityLevel.Error, "ERR", $"The component '{name}' does not have pins"));
                                return PresenceResult.GiveUp;
                            }
                            var pin = drawable.Pins[^1];
                            pinFactories.Add(CreatePinFactory(name, pin.Name, pin, usedPins, context.Diagnostics));
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
                return PresenceResult.Success;
            }

            private static Action<PinCollection, ILocatedDrawable> CreatePinFactory(string drawableName, string pinName, IPin pin, HashSet<string> usedPins, IDiagnosticHandler diagnostics)
            {
                List<string> names = [];

                // The full name with pin
                string namePin = $"{drawableName}_{pinName}";
                if (usedPins.Add(namePin))
                    names.Add(namePin);
                else
                    diagnostics?.Post(new SourcesDiagnosticMessage(pin.Sources, SeverityLevel.Warning, "WARNING", $"The pin '{pinName}' on '{drawableName}' is used multiple times and will not be accessible as a port anymore."));

                // Also try just the drawable name
                if (usedPins.Add(drawableName))
                    names.Add(drawableName);

                // Then also try to add the local name
                string localName = drawableName;
                int index = drawableName.LastIndexOf(DrawableFactoryDictionary.Separator);
                if (index >= 0)
                {
                    localName = drawableName[index..];

                    // The local name with pin
                    string localNamePin = $"{localName}_{pinName}";
                    if (usedPins.Add(localNamePin))
                        names.Add(localNamePin);

                    // The local name
                    if (usedPins.Add(localName))
                        names.Add(localName);
                }

                // Also make a shorter version for DIR components
                // These can be quite handy then
                if (localName.StartsWith("DIR"))
                {
                    string shortPinName = localName[3..];
                    if (usedPins.Add(shortPinName))
                        names.Add(shortPinName);
                }

                switch (pin)
                {
                    case FixedOrientedPin oriented:
                        {
                            var offset = oriented.Location;
                            var orientation = oriented.Orientation;
                            return (pc, d) => pc.Add(new FixedOrientedPin(namePin, "Pin", d, offset, orientation), names);
                        }

                    case FixedPin @fixed:
                        {
                            var offset = @fixed.Location;
                            return (pc, d) => pc.Add(new FixedPin(namePin, "Pin", d, offset), names);
                        }

                    default:
                        throw new NotImplementedException();
                }
            }

            /// <inheritdoc/>
            protected override void Draw(IGraphicsBuilder builder)
            {
                _circuit.Render(builder);
            }
        }
    }
}