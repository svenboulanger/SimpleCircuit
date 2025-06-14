﻿using SpiceSharp.Algebra;
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
        private readonly ElementSet<double> _elements;
        private readonly Parameters _parameters;
        private bool _lastState, _state;
        private readonly double _gOff, _iOn, _iOff;
        private double _g, _i;

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
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));
            _elements = new ElementSet<double>(state.Solver,
                _variables.GetMatrixLocations(state.Map),
                _variables.GetRhsIndices(state.Map));

            _state = true;
            _lastState = true;
            
            // Calculate conductances
            _g = _gOn + _iteration.Gmin;
            _gOff = _parameters.Weight + _iteration.Gmin;

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
                _g = _gOn + _iteration.Gmin;
                _i = _iOn;
            }
            else
            {
                // Get the controlled value
                _lastState = _state;
                double ctrl = _variables.Positive.Value - (_variables.Negative.Value + _parameters.Offset);
                if (ctrl < _parameters.Minimum - _threshold - _iteration.Gmin * 1e6)
                    _state = true;
                else if (ctrl > _parameters.Minimum + _threshold + _iteration.Gmin * 1e6)
                    _state = false;

                if (_state != _lastState)
                {
                    _iteration.IsConvergent = false;
                    if (_state)
                    {
                        _g = _gOn + _iteration.Gmin;
                        _i = _iOn;
                    }
                    else
                    {
                        _g = _gOff + _iteration.Gmin;
                        _i = _iOff;
                    }
                }
            }

            // Load the Y-matrix
            _elements.Add(_g, -_g, -_g, _g, _i, -_i);
        }
    }
}
