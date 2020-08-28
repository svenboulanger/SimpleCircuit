using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using SimpleCircuit.Algebra;
using SimpleCircuit.Circuits;
using SimpleCircuit.Components;
using SimpleCircuit.Constraints;
using SimpleCircuit.Contributions;

namespace SimpleCircuit
{
    public class Circuit
    {
        public double DefaultWireLength = 10;

        private class ConstraintItem
        {
            public bool Suppressed { get; set; }
            public int FirstRow { get; set; }
            public int LastRow { get; set; }
            public IConstraint Constraint { get; }
            public ConstraintItem(IConstraint constraint)
            {
                Constraint = constraint;
            }
            public override string ToString() => $"{Constraint} ({(Suppressed ? "suppressed" : $"{LastRow-FirstRow} rows")})";
        }
        private readonly Dictionary<string, IComponent> _components = new Dictionary<string, IComponent>();
        private readonly HashSet<ConstraintItem> _constraints = new HashSet<ConstraintItem>();
        private readonly UnknownSolverMap _variables = new UnknownSolverMap();

        /// <summary>
        /// Gets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        public IEnumerable<IComponent> Components => _components.Values;

        /// <summary>
        /// Gets the wires that connect pins together in the order that they are drawn.
        /// </summary>
        /// <value>
        /// The wires.
        /// </value>
        public List<Wire> Wires { get; } = new List<Wire>();

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _components.Count;

        /// <summary>
        /// Gets the <see cref="IComponent"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IComponent"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IComponent this[string name] => _components[name];

        /// <summary>
        /// Adds the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Add(IComponent component)
        {
            if (component == null)
                return;
            _components.Add(component.Name, component);
        }

        /// <summary>
        /// Adds the constraint to the circuit.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        public void AddConstraint(IConstraint constraint)
        {
            if (constraint == null)
                return;
            _constraints.Add(new ConstraintItem(constraint));
        }

        /// <summary>
        /// Removes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        public bool Remove(string name)
        {
            if (_components.TryGetValue(name, out var component))
            {
                _components.Remove(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the specified components.
        /// </summary>
        /// <param name="components">The components.</param>
        public void Add(IEnumerable<IComponent> components)
        {
            foreach (var component in components)
                Add(component);
        }

        /// <summary>
        /// Adds the specified components.
        /// </summary>
        /// <param name="components">The components.</param>
        public void Add(params IComponent[] components)
        {
            foreach (var component in components)
                Add(component);
        }

        /// <summary>
        /// Tries to get a value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string name, out IComponent value) => _components.TryGetValue(name, out value);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _components.Clear();
            _constraints.Clear();
            _variables.Clear();
            Wires.Clear();
        }

        /// <summary>
        /// Renders the circuit to an SVG image.
        /// </summary>
        /// <returns>The Xml that describes the SVG image.</returns>
        /// <exception cref="ArgumentException">Thrown if the circuit is underconstrained.</exception>
        public XmlDocument Render()
        {
            ResolveVariables();

            // Render all the components
            var drawing = new SvgDrawing();
            foreach (var component in _components)
            {
                drawing.StartGroup(component.Key, component.Value.GetType().Name.ToLower());
                component.Value.Render(drawing);
                drawing.EndGroup();
            }

            // Render all the wires
            List<Wire> drawn = new List<Wire>(Wires.Count);
            foreach (var wire in Wires)
            {
                wire.Render(drawn, drawing);
                drawn.Add(wire);
            }

            return drawing.GetDocument();
        }

        /// <summary>
        /// Resolves the variables.
        /// </summary>
        private void ResolveVariables()
        {
            var solver = BuildSolver();
            var solution = new DenseVector<double>(_variables.Count);

            // Apply a first time for testing
            solver.Reset();
            foreach (var c in _constraints.Where(c => !c.Suppressed))
            {
                c.Constraint.Update(solution);
                c.Constraint.Apply();
            }
            if (_variables.Count == 0)
                return;
            var steps = solver.OrderAndFactor();
            
            // We have an over-constrained thing here...
            // Throw away the last constraints
            if (solver.Size > _variables.Count)
            {
                Console.WriteLine($"Overconstrained problem!");
                for (var row = _variables.Count + 1; row <= solver.Size; row++)
                {
                    var loc = solver.InternalToExternal(new MatrixLocation(row, 1));
                    var constraint = _constraints.First(c => c.FirstRow <= loc.Row && c.LastRow > loc.Row);
                    constraint.Suppressed = true;
                    Console.WriteLine($"- Suppressed {constraint}");
                }
                Console.WriteLine();
                Console.WriteLine("Rebuilding...");

                // Rebuild the solver
                solver = BuildSolver();
                steps = solver.OrderAndFactor();
            }

            // See if something changed
            if (steps < _variables.Count)
            {
                Constrain(solver, steps);
                solver = BuildSolver();
            }

            // Solve for real this time
            var iterations = 0;
            double error = 1.0;
            solution = new DenseVector<double>(_variables.Count);
            var oldSolution = new DenseVector<double>(solution.Length);
            while (error > 1e-9)
            {
                solver.Reset();
                foreach (var c in _constraints.Where(c => !c.Suppressed))
                {
                    c.Constraint.Update(solution);
                    c.Constraint.Apply();
                }
                iterations++;
                Console.WriteLine($"- iteration {iterations}");
                solver.Print();

                // Order and factor
                steps = solver.OrderAndFactor();
                if (steps < solution.Length)
                {
                    Console.WriteLine("Could not solve:");
                    DumpReordered(solver);
                    return;
                }

                solution.CopyTo(oldSolution);
                solver.Solve(solution);

                // find the maximum error
                error = 0.0;
                for (var i = 1; i <= _variables.Count; i++)
                {
                    // Detect infinity and NaN
                    if (double.IsNaN(solution[i]) || double.IsInfinity(solution[i]))
                        Console.WriteLine($"Solution is not a number.");

                    double e;
                    switch (_variables[i])
                    {
                        case UnknownTypes.Angle:
                            e = Math.Abs(Utility.Difference(solution[i], oldSolution[i]));
                            if (e > Math.PI / 2)
                            {
                                if (Utility.Difference(solution[i], oldSolution[i]) > 0)
                                    solution[i] = oldSolution[i] + Math.PI / 2;
                                else
                                    solution[i] = oldSolution[i] - Math.PI / 2;
                            }
                            error = Math.Max(error, e);
                            solution[i] = Utility.Wrap(solution[i]);
                            break;
                        case UnknownTypes.Length:
                            e = Math.Abs(solution[i] - oldSolution[i]);
                            if (solution[i] < 0)
                                solution[i] = 0.0;
                            error = Math.Max(error, e);
                            break;
                        default:
                            e = Math.Abs(solution[i] - oldSolution[i]);
                            if (solution[i] > 0)
                            {
                                if (oldSolution[i] <= 0)
                                    solution[i] = Math.Min(solution[i], 100);
                                else
                                    solution[i] = Math.Min(solution[i], Math.Max(oldSolution[i] * 10, 100));
                            }
                            else
                            {
                                if (oldSolution[i] >= 0)
                                    solution[i] = Math.Max(solution[i], -100);
                                else
                                    solution[i] = Math.Max(solution[i], Math.Min(oldSolution[i] * 10, -100));
                            }
                            error = Math.Max(error, e);
                            break;
                    }

                    Console.WriteLine($"solution[{i}] = {solution[i]}");
                }

                if (error < 1e-9)
                    break;
                if (iterations > 100)
                {
                    Console.WriteLine("Could not converge...");
                    return;
                }    
            }

            // Update all contributions once more
            foreach (var c in _constraints)
                c.Constraint.Update(solution);

            // List all of them now
            for (var i = 1; i <= _variables.Count; i++)
            {
                var o = _variables.GetOwner(i);
                Console.WriteLine($"Solution of {o} = {o.Value}");
            }
        }

        private SparseRealSolver BuildSolver()
        {
            _variables.Clear();
            var solver = new SparseRealSolver();

            foreach (var component in Components)
            {
                foreach (var contributor in component.Contributors.Where(c => !c.IsFixed))
                    _variables.GetUnknown(contributor, contributor.Type);
            }

            // Allow all our items and constraints to allocate variables
            foreach (var c in _constraints)
            {
                if (c.Suppressed)
                    continue;
                c.FirstRow = solver.UsedRows + 1;
                c.Constraint.Setup(solver, c.FirstRow, _variables);
                c.LastRow = solver.UsedRows + 1;
                if (c.FirstRow == c.LastRow)
                {
                    Console.WriteLine($"Constraint {c.Constraint} doesn't solve any unknowns.");
                    c.Suppressed = true;
                }
                else
                    Console.WriteLine($"Constraint '{c.Constraint}' uses {c.LastRow - c.FirstRow} rows, starting at {c.FirstRow}.");
            }
            return solver;
        }

        private void Constrain(SparseRealSolver solver, int steps)
        {
            Console.WriteLine("Underconstrained problem:");
            DumpReordered(solver);

            // Let's first make a list of problems
            HashSet<int> looseVariables = new HashSet<int>(LooseVariables(solver, steps));
            var names = looseVariables.Select(v =>
            {
                var loc = solver.InternalToExternal(new MatrixLocation(v, v));
                return _variables.GetOwner(loc.Column);
            });
            Console.WriteLine($"- Trying to fix variables: {string.Join(", ", names)}.");

            // Start by suppressing the constraints that are giving problems
            foreach (var ci in _constraints)
            {
                if (ci.FirstRow <= steps)
                    continue;
                Console.WriteLine($"- Suppressing constraint: '{ci}'.");
                ci.Suppressed = true;
                ci.FirstRow = -1;
                ci.LastRow = -1;
            }

            // Let's try to resolve the variables that are still giving issues

            // We can use wires to our advantage!
            // Keep adding constraints using wires until our problems don't reduce
            while (looseVariables.Count > 0)
            {
                int Ext(int internalIndex)
                {
                    var loc = solver.InternalToExternal(new MatrixLocation(internalIndex, internalIndex));
                    return loc.Column;
                }
                bool hasFixed = false;

                // First try to fix some components if possible
                foreach (var w in Wires)
                    hasFixed |= FixByWire(w, solver, looseVariables);
                if (hasFixed)
                    continue;

                // Then try to just constrain them if fixing wasn't possible
                foreach (var w in Wires)
                    hasFixed |= ConstrainByWire(w, solver, looseVariables);
                if (hasFixed)
                    continue;

                // We are out of options... Just pick a loose variable and set it to a fixed value...
                // First try to set lengths
                int option = looseVariables.FirstOrDefault(index => _variables[Ext(index)] == UnknownTypes.Length);
                if (option > 0)
                {
                    var o = _variables.GetOwner(Ext(option));
                    Console.WriteLine($"- Setting the length {o} to {DefaultWireLength}.");
                    AddConstraint(new EqualsConstraint(o, new ConstantContributor(o.Type, DefaultWireLength)));
                    FixVariables(solver, looseVariables, option);
                    continue;
                }

                // Then try scaling stuff
                option = looseVariables.FirstOrDefault(index =>
                {
                    var e = _variables[Ext(index)];
                    if (e == UnknownTypes.ScaleX || e == UnknownTypes.ScaleY)
                        return true;
                    return false;
                });
                if (option > 0)
                {
                    var o = _variables.GetOwner(Ext(option));
                    Console.WriteLine($"- Setting the scale {o} to 1.");
                    AddConstraint(new EqualsConstraint(o, new ConstantContributor(o.Type, 1.0)));
                    FixVariables(solver, looseVariables, option);
                    continue;
                }

                // Orientations are next
                option = looseVariables.FirstOrDefault(index => _variables[Ext(index)] == UnknownTypes.Angle);
                if (option > 0)
                {
                    var o = _variables.GetOwner(Ext(option));
                    Console.WriteLine($"- Setting the angle {o} to 0.");
                    AddConstraint(new EqualsConstraint(o, new ConstantContributor(o.Type, 0.0)));
                    FixVariables(solver, looseVariables, option);
                    continue;
                }

                // Just whatever now...
                option = looseVariables.First();
                var c = _variables.GetOwner(Ext(option));
                Console.WriteLine($"- Setting the unknown {c} to 0.");
                AddConstraint(new EqualsConstraint(c, new ConstantContributor(c.Type, 0.0)));
                FixVariables(solver, looseVariables, option);
            }
        }

        /// <summary>
        /// Finds all variables that are fixed. The solver should be ordered and factored.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="steps">The number of steps that were succesfully eliminated.</param>
        /// <returns>All variables that are nicely fixed to a single possible value.</returns>
        private IEnumerable<int> LooseVariables(ISparsePivotingSolver<double> solver, int steps)
        {
            var areNotFixed = new HashSet<int>();
            var areFixed = new HashSet<int>();
            solver.Precondition((matrix, rhs) =>
            {
                // Get the already unset variables and potentially set variables
                for (var i = 1; i <= steps; i++)
                    areFixed.Add(i);
                for (var i = steps + 1; i <= _variables.Count; i++)
                    areNotFixed.Add(i);

                // The unfixed variables propagate, so let's find these
                var count = 0;
                while (areNotFixed.Count > count)
                {
                    count = areNotFixed.Count;
                    foreach (var index in areFixed.ToArray())
                    {
                        var elt = matrix.GetFirstInRow(index);
                        while (elt != null)
                        {
                            if (areNotFixed.Contains(elt.Column))
                            {
                                areFixed.Remove(elt.Row);
                                areNotFixed.Add(elt.Row);
                                break;
                            }
                            elt = elt.Right;
                        }
                    }
                }
            });
            return areNotFixed;
        }

        /// <summary>
        /// Fixes the variables.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="looseVariables">The loose variables.</param>
        /// <param name="toFix">The variable that just was fixed.</param>
        private void FixVariables(ISparsePivotingSolver<double> solver, HashSet<int> looseVariables, int toFix)
        {
            looseVariables.Remove(toFix);

            // Finds a row that uniquely determines a variable
            int IsFixingRow(ISparseMatrix<double> matrix, int row)
            {
                var elt = matrix.GetFirstInRow(row);
                if (elt == null)
                    return -1;
                int looseElements = 0, fixedElements = 0;
                int result = -1;
                while (elt != null)
                {
                    if (looseVariables.Contains(elt.Column))
                    {
                        looseElements++;
                        result = elt.Column;
                    }
                    else
                        fixedElements++;
                    elt = elt.Right;
                }
                if (looseElements == 1 && fixedElements > 0)
                    return result;
                return elt?.Column ?? 0;
            }

            solver.Precondition((matrix, rhs) =>
            {
                var count = looseVariables.Count + 1;
                while (looseVariables.Count < count)
                {
                    count = looseVariables.Count;
                    for (var i = 1; i < matrix.Size; i++)
                    {
                        var fixVar = IsFixingRow(matrix, i);
                        if (fixVar > 0)
                        {
                            looseVariables.Remove(fixVar);
                            var loc = solver.InternalToExternal(new MatrixLocation(fixVar, fixVar));
                            Console.WriteLine($"\t- This also fixed variable {_variables.GetOwner(loc.Column)}.");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Fixes pins by using their wires.
        /// </summary>
        /// <param name="wire">The wire.</param>
        /// <param name="solver">The solver.</param>
        /// <param name="looseVariables">The loose variables.</param>
        /// <returns><c>true</c> if the wire could be used to constrain a variable.</returns>
        private bool FixByWire(Wire wire, SparseRealSolver solver, HashSet<int> looseVariables)
        {
            if (double.IsNaN(wire.PreferredOrientation))
                return false;
            int Int(int externalIndex)
            {
                var loc = solver.ExternalToInternal(new MatrixLocation(externalIndex, externalIndex));
                return loc.Column;
            }

            // Let's see if this wire can fix some of our problems along the X-axis
            var unknownsA = wire.PinA.X.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            var unknownsB = wire.PinB.X.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            if (unknownsA.Length == 1 && !wire.PinA.X.IsFixed && wire.PinB.X.IsFixed)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsA[0])}");
                if (wire.PinA.X.Fix(wire.PinB.X.Value - DefaultWireLength * Math.Cos(wire.PreferredOrientation)))
                {
                    FixVariables(solver, looseVariables, Int(unknownsA[0]));
                    return true;
                }
            }
            else if (unknownsB.Length == 1 && !wire.PinB.X.IsFixed && wire.PinA.X.IsFixed)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsB[0])}");
                if (wire.PinB.X.Fix(wire.PinA.X.Value + DefaultWireLength * Math.Cos(wire.PreferredOrientation)))
                {
                    FixVariables(solver, looseVariables, Int(unknownsB[0]));
                    return true;
                }
            }

            // Let's see if this wire can fix some of our problems along the Y-axis
            unknownsA = wire.PinA.Y.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            unknownsB = wire.PinB.Y.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            if (unknownsA.Length == 1 && !wire.PinA.Y.IsFixed && wire.PinB.Y.IsFixed)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsA[0])}");
                if (wire.PinA.Y.Fix(wire.PinB.Y.Value - DefaultWireLength * Math.Sin(wire.PreferredOrientation)))
                {
                    FixVariables(solver, looseVariables, Int(unknownsA[0]));
                    return true;
                }
            }
            else if (unknownsB.Length == 1 && !wire.PinB.Y.IsFixed && wire.PinA.Y.IsFixed)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsB[0])}");
                if (wire.PinB.Y.Fix(wire.PinA.Y.Value + DefaultWireLength * Math.Sin(wire.PreferredOrientation)))
                {
                    FixVariables(solver, looseVariables, Int(unknownsB[0]));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Constrains pins by using their wires.
        /// </summary>
        /// <param name="wire">The wire.</param>
        /// <param name="solver">The solver.</param>
        /// <param name="looseVariables">The loose variables.</param>
        /// <returns><c>true</c> if the wire could be used to constrain a variable.</returns>
        private bool ConstrainByWire(Wire wire, SparseRealSolver solver, HashSet<int> looseVariables)
        {
            if (double.IsNaN(wire.PreferredOrientation))
                return false;

            int Int(int externalIndex)
            {
                var loc = solver.ExternalToInternal(new MatrixLocation(externalIndex, externalIndex));
                return loc.Column;
            }

            // Let's see if this wire can fix some of our problems along the X-axis
            var unknownsA = wire.PinA.X.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            var unknownsB = wire.PinB.X.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            if (unknownsA.Length == 1 && unknownsB.Length == 0)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsA[0])}");
                AddConstraint(new EqualsConstraint(new OffsetContributor(wire.PinA.X, DefaultWireLength * Math.Cos(wire.PreferredOrientation), UnknownTypes.X), wire.PinB.X));
                FixVariables(solver, looseVariables, Int(unknownsA[0]));
                return true;
            }
            else if (unknownsB.Length == 1 && unknownsA.Length == 0)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsB[0])}");
                AddConstraint(new EqualsConstraint(new OffsetContributor(wire.PinA.X, DefaultWireLength * Math.Cos(wire.PreferredOrientation), UnknownTypes.X), wire.PinB.X));
                FixVariables(solver, looseVariables, Int(unknownsB[0]));
                return true;
            }

            // Let's see if this wire can fix some of our problems along the Y-axis
            unknownsA = wire.PinA.Y.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            unknownsB = wire.PinB.Y.GetUnknowns(_variables).Where(c => looseVariables.Contains(Int(c))).ToArray();
            if (unknownsA.Length == 1 && unknownsB.Length == 0)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsA[0])}");
                AddConstraint(new EqualsConstraint(new OffsetContributor(wire.PinA.Y, DefaultWireLength * Math.Sin(wire.PreferredOrientation), UnknownTypes.Y), wire.PinB.Y));
                FixVariables(solver, looseVariables, Int(unknownsA[0]));
                return true;
            }
            else if (unknownsB.Length == 1 && unknownsA.Length == 0)
            {
                Console.WriteLine($"- Using wire {wire} to fix {_variables.GetOwner(unknownsB[0])}");
                AddConstraint(new EqualsConstraint(new OffsetContributor(wire.PinA.Y, DefaultWireLength * Math.Sin(wire.PreferredOrientation), UnknownTypes.Y), wire.PinB.Y));
                FixVariables(solver, looseVariables, Int(unknownsB[0]));
                return true;
            }
            return false;
        }

        private void DumpReordered(SparseRealSolver solver)
        {
            solver.PrintReordered();
            for (var i = 1; i <= _variables.Count; i++)
            {
                var loc = solver.InternalToExternal(new MatrixLocation(i, i));
                var constraint = _constraints.FirstOrDefault(c => loc.Row >= c.FirstRow && loc.Row < c.LastRow);
                if (constraint != null)
                    Console.WriteLine($"Row {i} = {constraint}");
                Console.WriteLine($"Column {i} = {_variables.GetOwner(loc.Column)}");
            }
        }
    }
}
