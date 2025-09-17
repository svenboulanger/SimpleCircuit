using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SimpleCircuit.Components.Constraints.SlopedMinimumConstraints
{
    /// <summary>
    /// The biasing behavior for a <see cref="SlopedMinimumConstraint"/>.
    /// </summary>
    [BehaviorFor(typeof(SlopedMinimumConstraint))]
    public class Biasing : Behavior, IBiasingBehavior
    {
        private const double _thresholdHysteresis = 0.1;
        private const double _gOnFactor = 1.0e4;
        private readonly IIterationSimulationState _iteration;
        private bool _lastState, _state;
        private readonly Parameters _parameters;
        private readonly IVariable<double> _x1, _y1, _x2, _y2;
        private readonly ElementSet<double> _elements;
        private double _gnx2, _gnxny, _gny2;
        private Vector2 _i;
        private readonly double _xo;
        private readonly Vector2 _iOn, _iOff, _n;
        private readonly bool _zeroX, _zeroY;

        /// <summary>
        /// Creates a new <see cref="Biasing"/> behavior.
        /// </summary>
        /// <param name="context">The context.</param>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            _parameters = context.GetParameterSet<Parameters>();
            _iteration = context.GetState<IIterationSimulationState>();
            var biasing = context.GetState<IBiasingSimulationState>();
            var map = biasing.Map;

            // Get the mode of operation
            _zeroX = _parameters.Normal.X.IsZero();
            _zeroY = _parameters.Normal.Y.IsZero();
            if (_zeroX && _zeroY)
                throw new ArgumentException($"The normal ({_parameters.Normal}) is zero for {Name}");

            // Get the variables to solve for
            int x1 = map[_x1 = biasing.GetSharedVariable(context.Nodes[0])];
            int y1 = map[_y1 = biasing.GetSharedVariable(context.Nodes[1])];
            int x2 = map[_x2 = biasing.GetSharedVariable(context.Nodes[2])];
            int y2 = map[_y2 = biasing.GetSharedVariable(context.Nodes[3])];
            int i = map[biasing.CreatePrivateVariable(Name.Combine("i"), Units.Ampere)];

            // Create the matrix and RHS elements
            if (_zeroX)
            {
                // A lot of elements are always 0, so let's not allocate them
                _elements = new ElementSet<double>(biasing.Solver,
                    [
                        new(x1, i),
                        new(y1, y1), new(y1, y2),
                        new(y2, y1), new(y2, y2),
                        new(i, x1), new(i, x2)
                    ],
                    [y1, y2, i]);
            }
            else if (_zeroY)
            {
                // A lot of elements are always 0, so let's not allocate them
                _elements = new ElementSet<double>(biasing.Solver,
                    [
                        new(x1, x1), new(x1, x2),
                        new(y1, i),
                        new(x2, x1), new(x2, x2),
                        new(y2, i),
                        new(i, y1), new(i, y2)
                    ],
                    [x1, x2, i]);
            }
            else
            {

                _elements = new ElementSet<double>(biasing.Solver,
                    [
                        new(x1, x1), new(x1, y1), new(x1, x2), new(x1, y2), new(x1, i),
                        new(y1, x1), new(y1, y1), new(y1, x2), new(y1, y2), new(y1, i),
                        new(x2, x1), new(x2, y1), new(x2, x2), new(x2, y2), new(x2, i),
                        new(y2, x1), new(y2, y1), new(y2, x2), new(y2, y2), new(y2, i),
                        new(i, x1), new(i, y1), new(i, x2), new(i, y2), new(i, i)
                    ],
                    [x1, y1, x2, y2, i]);
            }

            // Initialize
            _state = true;
            _lastState = true;
            _n = _parameters.Normal;
            _gnx2 = _gOnFactor * _n.X * _n.X / _parameters.Weight + _iteration.Gmin;
            _gnxny = _gOnFactor * _n.X * _n.Y / _parameters.Weight + _iteration.Gmin;
            _gny2 = _gOnFactor * _n.Y * _n.Y / _parameters.Weight + _iteration.Gmin;
            _iOff = -_n * (_parameters.Minimum + _n.Dot(_parameters.Offset)) / _parameters.Weight;
            _i = _iOn = _gOnFactor * _iOff; // -_gOnFactor / _parameters.Weight * _n * (_parameters.Minimum + _n.Dot(_parameters.Offset));
            _xo = _parameters.Normal.Dot(_parameters.Offset.Perpendicular);
        }

        /// <inheritdoc />
        public void Load()
        {
            // Deal with minimum distances between coordinates
            if (_iteration.Mode == IterationModes.Fix || _iteration.Mode == IterationModes.Junction)
            {
                _state = true;
                _gnx2 = _gOnFactor * _n.X * _n.X / _parameters.Weight + _iteration.Gmin;
                _gnxny = _gOnFactor * _n.X * _n.Y / _parameters.Weight + _iteration.Gmin;
                _gny2 = _gOnFactor * _n.Y * _n.Y / _parameters.Weight + _iteration.Gmin;
            }
            else
            {
                // Find the distance between the two points, accounting for the direction
                _lastState = _state;
                Vector2 ctrl = new(_x2.Value - (_x1.Value + _parameters.Offset.X), _y2.Value - (_y1.Value + _parameters.Offset.Y));
                double dot = ctrl.Dot(_n);
                if (dot < _parameters.Minimum - _thresholdHysteresis - _iteration.Gmin * 1e6)
                    _state = true;
                else if (dot > _parameters.Minimum + _thresholdHysteresis + _iteration.Gmin * 1e6)
                    _state = false;
            
                // Change the parameters if necessary
                if (_state != _lastState)
                {
                    _iteration.IsConvergent = false;
                    if (_state)
                    {
                        _gnx2 = _gOnFactor * _n.X * _n.X / _parameters.Weight + _iteration.Gmin;
                        _gnxny = _gOnFactor * _n.X * _n.Y / _parameters.Weight + _iteration.Gmin;
                        _gny2 = _gOnFactor * _n.Y * _n.Y / _parameters.Weight + _iteration.Gmin;
                        _i = _iOn;
                    }
                    else
                    {
                        _gnx2 = _n.X * _n.X / _parameters.Weight + _iteration.Gmin;
                        _gnxny = _n.X * _n.Y / _parameters.Weight + _iteration.Gmin;
                        _gny2 = _n.Y * _n.Y / _parameters.Weight + _iteration.Gmin;
                        _i = _iOff;
                    }
                }
            }

            // Load the Y-matrix
            if (_zeroX)
            {
                _elements.Add(
                    -_n.Y,
                    _gny2, -_gny2,
                    _n.Y,
                    -_n.Y, _n.Y,
                    _i.Y, -_i.Y, _xo);
            }
            else if (_zeroY)
            {
                _elements.Add(
                    _gnx2, -_gnx2,
                    _n.X,
                    -_gnx2, _gnx2,
                    -_n.X,
                    _n.X, -_n.X,
                    _i.X, -_i.X, _xo);
            }
            else
            {
                _elements.Add(
                     _gnx2, _gnxny, -_gnx2, -_gnxny, -_n.Y,
                     _gnxny, _gny2, -_gnxny, -_gny2, _n.X,
                    -_gnx2, -_gnxny, _gnx2, _gnxny, _n.Y,
                    -_gnxny, -_gny2, _gnxny, _gny2, -_n.X,
                    -_n.Y, _n.X, _n.Y, -_n.X, 1e-9,

                    _i.X, _i.Y, -_i.X, -_i.Y, _xo);
            }
        }
    }
}
