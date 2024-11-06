using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;

namespace SimpleCircuit.Components.Constraints.MinimumConstraints
{
    /// <summary>
    /// Describes the biasing behavior for a minimum constraint.
    /// </summary>
    [BehaviorFor(typeof(MinimumConstraint))]
    public class Biasing : Behavior, IBiasingBehavior
    {
        private const double _threshold = 0.01;
        private const double _gOn = 1.0e4;
        private readonly IIterationSimulationState _iteration;
        private readonly OnePort<double> _variables;
        private readonly ElementSet<double> _elements, _rhs;
        private readonly Parameters _parameters;
        private bool _lastState, _state;
        private readonly double _gOff, _iOn, _iOff;
        private double _g, _i;
        private IBiasingSimulationState _biasingState;

        /// <summary>
        /// Creates a new <see cref="Biasing"/> behavior.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            _parameters = context.GetParameterSet<Parameters>();
            _iteration = context.GetState<IIterationSimulationState>();

            // Get the nodes
            _biasingState = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(
                _biasingState.GetSharedVariable(context.Nodes[0]),
                _biasingState.GetSharedVariable(context.Nodes[1]));
            _elements = new ElementSet<double>(_biasingState.Solver,
                _variables.GetMatrixLocations(_biasingState.Map),
                _variables.GetRhsIndices(_biasingState.Map));

            _state = true;
            _lastState = true;
            
            // Calculate conductances
            _g = _gOn;
            _gOff = 1.0 / _parameters.Weight;

            // Calculate currents
            _iOn = (_parameters.Offset + _parameters.Minimum) * _gOn;
            _iOff = _parameters.Offset * _gOff;
            _i = _iOn;
        }

        /// <inheritdoc />
        public void Load()
        {
            if (_iteration.Mode == IterationModes.Fix || _iteration.Mode == IterationModes.Junction)
            {
                _state = true;
                _g = _gOn;
                _i = _iOn;
            }
            else
            {
                // Get the controlled value
                _lastState = _state;
                double ctrl = _variables.Positive.Value - (_variables.Negative.Value + _parameters.Offset);
                if (ctrl < _parameters.Minimum - _threshold)
                    _state = true;
                else if (ctrl > _parameters.Minimum + _threshold)
                    _state = false;

                if (_state != _lastState)
                {
                    _iteration.IsConvergent = false;
                    if (_state)
                    {
                        _g = _gOn;
                        _i = _iOn;
                    }
                    else
                    {
                        _g = _gOff;
                        _i = _iOff;
                    }
                }
            }

            // Load the Y-matrix
            _elements.Add(_g, -_g, -_g, _g, _i, -_i);
        }
    }
}
