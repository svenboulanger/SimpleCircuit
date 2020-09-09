using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SimpleCircuit.Algebra;
using SimpleCircuit.Circuits;
using SimpleCircuit.Components;
using SimpleCircuit.Constraints;
using SimpleCircuit.Contributors;

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
            public override string ToString() => $"{Constraint} ({(Suppressed ? "suppressed" : $"{LastRow - FirstRow} rows")})";
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

        private void Load(SparseRealSolver solver, IVector<double> solution)
        {
            solver.Reset();
            foreach (var c in _constraints.Where(c => !c.Suppressed))
            {
                c.Constraint.Update(solution);
                c.Constraint.Apply();
            }
        }

        /// <summary>
        /// Tries to resolve as many constraints possible to reduce the number of unknowns that need to be solved by
        /// Newton-Raphson.
        /// </summary>
        private void ResolveConstraints()
        {
            bool needsMoreResolving = true;
            while (needsMoreResolving)
            {
                needsMoreResolving = false;
                foreach (var constraint in _constraints.Where(c => !c.Suppressed).ToArray())
                {
                    if (constraint.Constraint.TryResolve())
                    {
                        constraint.Suppressed = true;
                        needsMoreResolving = true;
                        Console.WriteLine($"- Could resolve {constraint.Constraint}");
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the variables.
        /// </summary>
        private void ResolveVariables()
        {
            // Add the wire constraint if necessary
            foreach (var wire in Wires)
            {
                if (!_constraints.Any(c => ReferenceEquals(c.Constraint, wire.Definition)))
                    AddConstraint(wire.Definition);
            }

            // First try to resolve all the constraints where possible
            Console.WriteLine("Resolving constraints without using the nonlinear solver:");
            ResolveConstraints();

            var solver = BuildSolver();
            if (_variables.Count == 0)
                return;
            var solution = BuildSolution();

            // Apply a first time and see how many constraints are actually working out
            Load(solver, solution);
            var steps = solver.OrderAndFactor();

            // Remove constraints if there are too many
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

            // If the solver is underconstrained, add our own constraints
            if (steps < _variables.Count)
            {
                Constrain();
                ResolveConstraints();
                solver = BuildSolver();                
            }

            // Solve for real this time
            var iterations = 0;
            double error = 1.0;
            solution = BuildSolution();
            var oldSolution = new DenseVector<double>(solution.Length);
            while (error > 1e-9)
            {
                Load(solver, solution);
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
                            {
                                solution[i] = 0.0;
                                Console.WriteLine($"The solution for {_variables.GetOwner(i)} became negative.");
                            }
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

                    Console.WriteLine($"solution of {_variables.GetOwner(i)} = {solution[i]}");
                }

                if (error < 1e-9)
                    break;
                if (iterations > 10)
                {
                    Console.WriteLine("Could not converge...");
                    return;
                }
            }

            // Update all contributions once more
            foreach (var c in _constraints.Where(c => !c.Suppressed))
                c.Constraint.Update(solution);

            // List all of them now
            for (var i = 1; i <= _variables.Count; i++)
            {
                var o = _variables.GetOwner(i);
                Console.WriteLine($"Solution of {o} = {o.Value}");
            }
        }

        /// <summary>
        /// Build a new solver from scratch using the current constraints and variables.
        /// </summary>
        /// <returns>The solver.</returns>
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
                    c.Suppressed = true;
            }

            return solver;
        }

        /// <summary>
        /// Build a new solution from scratch using the current constraints and variables.
        /// </summary>
        /// <returns>An initial solution vector.</returns>
        private IVector<double> BuildSolution()
        {
            var solution = new DenseVector<double>(_variables.Count);
            for (var i = 1; i <= _variables.Count; i++)
            {
                switch (_variables[i])
                {
                    case UnknownTypes.Length: solution[i] = DefaultWireLength; break;
                }
            }
            return solution;
        }

        /// <summary>
        /// Try constraining an underconstrained circuit.
        /// </summary>
        private void Constrain()
        {
            Console.WriteLine("------------------ Underconstrained problem ------------------");

            // Let's bring all the wire lengths to the end of the solver and restrict it from being used
            var solver = BuildSolver();
            var solution = BuildSolution();
            int count = _variables.Count;
            solver.Precondition((matrix, rhs) =>
            {
                foreach (var wire in Wires)
                {
                    if (!wire.Length.IsFixed)
                    {
                        var row = _constraints.First(c => ReferenceEquals(c.Constraint, wire.Definition)).FirstRow;
                        if (_variables.TryGetIndex(wire.Length, UnknownTypes.Length, out var col))
                        {
                            var loc = solver.ExternalToInternal(new MatrixLocation(col, col));
                            matrix.SwapRows(row, count);
                            matrix.SwapColumns(loc.Column, count);
                            count--;
                        }
                    }
                }
                solver.Degeneracy = _variables.Count - count;
                solver.PivotSearchReduction = _variables.Count - count;
            });

            // Look at the wiring of the structure
            Load(solver, solution);
            var steps = solver.OrderAndFactor();
            if (steps == count)
            {
                Console.WriteLine("Already fully constrained...");
                return;
            }

            DumpReordered(solver);

            // Constrain any constants that aren't connected to our wires
            var possibleRows = new HashSet<int>();
            for (var i = count + 1; i <= _variables.Count; i++)
                possibleRows.Add(i);
            var possibleColumns = new HashSet<int>();
            solver.Precondition((matrix, rhs) =>
            {
                for (var i = steps + 1; i <= count; i++)
                {
                    var elt = matrix.GetLastInColumn(i);
                    if (elt == null || elt.Row < count)
                    {
                        var ext = solver.InternalToExternal(new MatrixLocation(i, i));
                        var owner = _variables.GetOwner(ext.Column);
                        Contributor reference;
                        switch (owner.Type)
                        {
                            case UnknownTypes.Length: reference = new ConstantContributor(UnknownTypes.Length, DefaultWireLength); break;
                            case UnknownTypes.ScaleX:
                            case UnknownTypes.ScaleY: reference = new ConstantContributor(owner.Type, 1.0); break;
                            default: reference = new ConstantContributor(owner.Type, 0.0); break;
                        }
                        AddConstraint(new EqualsConstraint(owner, reference));
                        Console.WriteLine($"    - Fixed {owner} to {reference}.");
                    }
                    else
                        possibleColumns.Add(i);
                }
            });

            // The wire unknowns are unknowns that are defined by wire lengths, let's deal with them!
            var dependents = new DependentWires();
            while (possibleRows.Count > 0)
            {
                // Start with the first wire and extract all linked wires
                dependents.Clear();
                dependents.AddRow(solver, possibleRows.First(), possibleRows, possibleColumns);

                Wire GetWire(int row)
                {
                    var ext = solver.InternalToExternal(new MatrixLocation(row, row));
                    var constraint = _constraints.First(c => !c.Suppressed && c.FirstRow == ext.Row);
                    return Wires.First(w => ReferenceEquals(w.Definition, constraint.Constraint));
                }
                void Constrain(int[] ws)
                {
                    if (ws.Length > 0)
                    {
                        if (ws.Length == 1)
                        {
                            var wire = GetWire(ws[0]);
                            AddConstraint(new EqualsConstraint(wire.Length, new ConstantContributor(UnknownTypes.Length, DefaultWireLength)));
                            Console.WriteLine($"- Fixing {wire} length to {DefaultWireLength}.");
                        }
                        else
                        {
                            var wire = GetWire(ws[0]);
                            Contributor contributor = new SkewedContributor(DefaultWireLength - wire.Length);
                            for (var i = 1; i < ws.Length; i++)
                            {
                                wire = GetWire(ws[i]);
                                contributor += new SkewedContributor(DefaultWireLength - wire.Length);
                            }
                            AddConstraint(new EqualsConstraint(contributor, new ConstantContributor(UnknownTypes.Length, 1.0)));
                            Console.WriteLine($"- Fixing {contributor} to 1.");
                        }
                        foreach (var wire in ws)
                            dependents.ConstrainWire(wire);
                    }
                }

                // If these are loose, then fix the left-most unknown
                if (dependents.IsLoose)
                {
                    var uk = dependents.LeftMostUnknowns.First();
                    var ext = solver.InternalToExternal(new MatrixLocation(uk, uk));
                    var owner = _variables.GetOwner(ext.Column);
                    Console.WriteLine($"- Loose wire group: fixed {owner}.");
                    AddConstraint(new EqualsConstraint(owner, new ConstantContributor(owner.Type, 0)));
                    dependents.ConstrainUnknown(uk);
                }

                // These wire lengths will be fixed, so remove them from the possibilities
                foreach (var row in dependents.Rows)
                    possibleRows.Remove(row);

                // Constrain wires to fixed points
                var wires = dependents.RightOf(0).ToArray();
                Constrain(wires);

                // Let's work our way from left to right, start with the wires starting from the left-most
                while (dependents.Count > 0)
                {
                    // Remove any loose wires
                    bool redo = true;
                    while (redo)
                    {
                        redo = false;
                        foreach (var s in dependents.Starts.ToArray())
                        {
                            wires = dependents.RightOf(s).ToArray();
                            Constrain(wires);
                            redo = true;
                        }
                        foreach (var e in dependents.Ends.ToArray())
                        {
                            wires = dependents.LeftOf(e).ToArray();
                            Constrain(wires);
                            redo = true;
                        }
                    }

                    // Work from left to right
                    var unknown = dependents.LeftMostUnknowns.FirstOrDefault();
                    if (unknown > 0)
                    {
                        wires = dependents.RightOf(unknown).ToArray();
                        Constrain(wires);
                    }
                }
            }

            Console.WriteLine("------------------ End of constraining ------------------");
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
            }
            for (var i = 1; i <= _variables.Count; i++)
            {
                var loc = solver.InternalToExternal(new MatrixLocation(i, i));
                Console.WriteLine($"Column {i} = {_variables.GetOwner(loc.Column)}");
            }
        }
    }
}
