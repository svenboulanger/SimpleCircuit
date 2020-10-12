using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCircuit.Algebra;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A class that can minimize a function under given constraints and minima.
    /// </summary>
    public class Minimizer
    {
        private readonly Random _rnd = new Random();

        /// <summary>
        /// If <c>true</c>, logging info is shown.
        /// </summary>
        public static bool LogInfo = false;

        private class Constraint
        {
            public Function Local { get; set; }
            public Function Original { get; set; }
        }

        private readonly Dictionary<Unknown, double> _minimum = new Dictionary<Unknown, double>();
        private readonly HashSet<Function> _constraints = new HashSet<Function>();
        private readonly UnknownMap _map = new UnknownMap();
        private readonly Dictionary<Unknown, IRowEquation> _equations = new Dictionary<Unknown, IRowEquation>();
        private readonly HashSet<Unknown> _lambdas = new HashSet<Unknown>();
        private ISparsePivotingSolver<double> _solver = null;
        private IVector<double> _solution = null, _oldSolution = null;

        /// <summary>
        /// Gets the unknowns.
        /// </summary>
        /// <value>
        /// The unknowns.
        /// </value>
        public IEnumerable<Unknown> Unknowns => _map.Select(p => p.Key);

        /// <summary>
        /// Gets or sets the function that needs to be minimized.
        /// </summary>
        /// <value>
        /// The minimize.
        /// </value>
        public Function Minimize { get; set; } = 1.0;

        /// <summary>
        /// Adds the constraint to the circuit.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        public void AddConstraint(Function function)
        {
            if (function == null)
                return;
            _constraints.Add(function);
        }

        /// <summary>
        /// Adds a function that needs to be.
        /// </summary>
        /// <param name="unknown">The unknown with a minimum value.</param>
        /// <param name="strict">The strict minimum value for the unknown.</param>
        /// <param name="initial">The loose minimum value for the unknown.</param>
        public void AddMinimum(Unknown unknown, double strict)
        {
            if (unknown == null)
                return;
            _minimum.Add(unknown, strict);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            Minimize = null;
            _constraints.Clear();
            _map.Clear();
            _minimum.Clear();
            _equations.Clear();
            _lambdas.Clear();
        }

        /// <summary>
        /// Resolves the variables.
        /// </summary>
        public void Solve()
        {
            BuildSolver();

            // Solve for real this time
            var iterations = 0;
            double error = 1.0;
            bool needsReordering = true;
            while (error > 1e-9)
            {
                // Build the matrix
                _solver.Reset();
                foreach (var eq in _map)
                    eq.Key.Value = _solution[eq.Value];
                foreach (var eq in _equations)
                {
                    eq.Value.Update();
                    eq.Value.Apply(1.0, null);
                }
                iterations++;

                // Order and factor
                int steps;
                _solver.Degeneracy = 0;
                if (needsReordering)
                {
                    steps = _solver.OrderAndFactor();
                    needsReordering = false;
                }
                else
                {
                    if (!_solver.Factor())
                    {
                        needsReordering = true;
                        continue;
                    }
                    else
                        steps = _solution.Length;
                }

                // Update the solution
                if (LogInfo)
                {
                    Console.WriteLine($"- Iteration {iterations} ({steps}/{_solver.Size} rows factored)");
                    if (steps < _solution.Length)
                    {
                        for (var i = steps + 1; i <= _solution.Length; i++)
                        {
                            var ext = _solver.InternalToExternal(new MatrixLocation(i, i));
                            Console.WriteLine($"    (Could not solve equation for {_map.Reverse(ext.Row)} or unknown {_map.Reverse(ext.Column)})");
                        }    
                    }
                }
                _solver.Degeneracy = _solver.Size - steps;
                _solution.CopyTo(_oldSolution);
                _solver.Solve(_solution);
                if (LogInfo)
                {
                    foreach (var eq in _map)
                    {
                        var msg = $"    {eq.Key} = {_solution[eq.Value]} (last was {_oldSolution[eq.Value]})";
                        if (Math.Abs(_solution[eq.Value] - _oldSolution[eq.Value]) > 1e-6)
                            Utility.Error(msg);
                        else
                            Console.WriteLine(msg);
                    }
                }
                
                // find the maximum error
                error = 0.0;
                for (var i = 1; i <= _map.Count; i++)
                {
                    // Detect infinity and NaN
                    var uk = _map.Reverse(i);
                    if (_lambdas.Contains(uk))
                    {
                        // We don't particularly care of our Lagrange multipliers, we will just make sure that they don't cause problems later on
                        // The thing is that lagrange multipliers can become undefined if a minimization and a constraint
                        // are leading to the same solution. In that case we get lamba*0=0 and the lagrange multiplier becomes undefined.
                        if (double.IsNaN(_solution[i]) || double.IsPositiveInfinity(_solution[i]) || _solution[i] > 1e6)
                            _solution[i] = 1e6; // Just a value to get through the next iteration
                        else if (double.IsNegativeInfinity(_solution[i]) || _solution[i] < -1e6)
                            _solution[i] = -1e6;
                    }
                    else
                    {
                        // For any other unknown, we do want to know how far we're off
                        double e;
                        switch (uk.Type)
                        {
                            case UnknownTypes.Length:
                                e = Math.Abs(_solution[i] - _oldSolution[i]);

                                // Add some randomization to avoid invalid "local minimum" solutions
                                if (_solution[i] < 0)
                                    _solution[i] = 1e-6 / (_rnd.NextDouble() + 1);
                                error = Math.Max(error, e);
                                break;
                            case UnknownTypes.Scale:
                                e = Math.Abs(_solution[i] - _oldSolution[i]);

                                // Add some random variation to stay away from zero to avoid invalid "local minimum" solutions
                                if (_solution[i].IsZero())
                                {
                                    var r = _rnd.NextDouble();
                                    if (r > 0.5)
                                        _solution[i] = 1e-3 / (r + 1.0);
                                    else
                                        _solution[i] = -1e-3 / (r + 1.0);
                                }
                                error = Math.Max(error, e);
                                break;
                            default:
                                e = Math.Abs(_solution[i] - _oldSolution[i]);
                                error = Math.Max(error, e);
                                break;
                        }
                    }
                }

                // Take a partial step if necessary to keep our inequalities greater than their minimum value
                double alpha = 1;
                foreach (var m in _minimum)
                {
                    if (_map.TryGet(m.Key, out var index))
                    {
                        if (_solution[index] < m.Value)
                        {
                            alpha = Math.Min(alpha, (m.Value - _oldSolution[index]) / (_solution[index] - _oldSolution[index]));
                            error = 1;
                        }
                        if (_solution[index] <= m.Value)
                            _solution[index] = m.Value + 1e-9;
                    }
                }
                if (LogInfo)
                    Console.WriteLine($"Alpha = {alpha}");
                for (var i = 1; i <= _map.Count; i++)
                    _solution[i] = _solution[i] * alpha + _oldSolution[i] * (1 - alpha);

                if (error < 1e-6)
                    break;
                if (iterations > 100)
                {
                    if (LogInfo)
                        Console.WriteLine("Could not converge...");
                    return;
                }
            }
        }

        /// <summary>
        /// Build a new solver from scratch using the current constraints and variables.
        /// </summary>
        /// <returns>The solver.</returns>
        private void BuildSolver()
        {
            _lambdas.Clear();
            _solver = new SparseRealSolver();
            _map.Clear();

            // Build our total function
            var f = Minimize ?? 1.0;
            int index = 1;
            foreach (var c in _constraints)
            {
                if (c.IsConstant)
                    continue;
                var lambda = new Unknown($"lambda{index++}", UnknownTypes.Scalar);
                f += lambda * c;
                _lambdas.Add(lambda);
            }

            // Differentiate the Lagrangian function
            if (LogInfo)
                Console.WriteLine(f);
            var diffs = new Dictionary<Unknown, Function>();
            f.Differentiate(null, diffs);

            // Handle unknowns that need to be minimized
            foreach (var m in _minimum)
            {
                if (m.Key.IsFixed)
                    continue;
                if (diffs.TryGetValue(m.Key, out var eq))
                    diffs[m.Key] = eq - 0.01 / (m.Key - m.Value);
            }

            // Setup the equations and solution
            _solution = new DenseVector<double>(_equations.Count);
            _oldSolution = new DenseVector<double>(_equations.Count);
            foreach (var eq in diffs)
            {
                index = _map.Map(eq.Key);
                var rowEquation = eq.Value.CreateEquation(index, _map, _solver);
                _equations.Add(eq.Key, rowEquation);
                switch (eq.Key.Type)
                {
                    case UnknownTypes.Scale:
                        if (eq.Key.Value.IsZero())
                        {
                            _solution[index] = 1.0;
                            _oldSolution[index] = 1.0;
                        }
                        else
                        {
                            _solution[index] = eq.Key.Value;
                            _oldSolution[index] = eq.Key.Value;
                        }
                        break;
                    case UnknownTypes.Length:
                        if (eq.Key.Value < 0)
                        {
                            _solution[index] = 0;
                            _oldSolution[index] = 0;
                        }
                        else
                        {
                            _solution[index] = eq.Key.Value;
                            _oldSolution[index] = eq.Key.Value;
                        }
                        break;
                    default:
                        _solution[index] = eq.Key.Value;
                        _oldSolution[index] = eq.Key.Value;
                        break;
                }
                if (LogInfo)
                    Console.WriteLine($"df/d{eq.Key} = {eq.Value}");
            }
            foreach (var m in _minimum)
            {
                if (m.Key.Value <= m.Value)
                {
                    if (_map.TryGet(m.Key, out index))
                    {
                        _solution[index] = m.Value + 1e-9;
                        _oldSolution[index] = m.Value + 1e-9;
                    }
                }
            }
        }
    }
}
