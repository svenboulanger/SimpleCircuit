using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Circuits
{
    /// <summary>
    /// Represents a group of columns and rows that are dependent.
    /// </summary>
    public class DependentWires
    {
        private class Wire
        {
            public int Row { get; }
            public int Start { get; set; }
            public int End { get; set; }
            public Wire(int row)
            {
                Row = row;
            }
        }
        private class Unknown
        {
            public List<Wire> Start { get; } = new List<Wire>();
            public List<Wire> End { get; } = new List<Wire>();
        }
        private readonly Dictionary<int, Wire> _wires = new Dictionary<int, Wire>();
        private readonly Dictionary<int, Unknown> _unknowns = new Dictionary<int, Unknown>();

        /// <summary>
        /// Gets a value indicating whether this instance is loose.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loose; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoose { get; private set; }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <value>
        /// The rows.
        /// </value>
        public IEnumerable<int> Rows => _wires.Keys;

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public IEnumerable<int> Columns => _unknowns.Keys;

        /// <summary>
        /// Gets the left most unknowns.
        /// </summary>
        /// <value>
        /// The left most unknowns.
        /// </value>
        public IEnumerable<int> LeftMostUnknowns => _unknowns.Where(p => p.Value.End.Count == 0).Select(p => p.Key);

        /// <summary>
        /// Gets the right most unknowns.
        /// </summary>
        /// <value>
        /// The right most unknowns.
        /// </value>
        public IEnumerable<int> RightMostUnknowns => _unknowns.Where(p => p.Value.Start.Count == 0).Select(p => p.Key);

        /// <summary>
        /// Gets the simple continuations of wires.
        /// </summary>
        /// <value>
        /// The simple continuations.
        /// </value>
        public IEnumerable<int> SimpleContinuations => _unknowns.Where(p => p.Value.Start.Count == 1 && p.Value.End.Count == 1).Select(p => p.Key);

        /// <summary>
        /// Gets all unknowns that are simply the end of a wire.
        /// </summary>
        /// <value>
        /// The ends.
        /// </value>
        public IEnumerable<int> Ends => _unknowns.Where(p => p.Value.Start.Count == 0 && p.Value.End.Count == 1).Select(p => p.Key);

        /// <summary>
        /// Gets all unknowns that are simply the start of a wire.
        /// </summary>
        /// <value>
        /// The starts.
        /// </value>
        public IEnumerable<int> Starts => _unknowns.Where(p => p.Value.Start.Count == 1 && p.Value.Start.Count == 0).Select(p => p.Key);

        /// <summary>
        /// Gets the unconstrained wire count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _wires.Count;

        /// <summary>
        /// Clears dependencies.
        /// </summary>
        public void Clear()
        {
            _wires.Clear();
            _unknowns.Clear();
            IsLoose = true;
        }

        /// <summary>
        /// Adds dependent rows and columns starting from the specified unordered row.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The row.</param>
        /// <param name="possibleRows">The possible rows.</param>
        /// <param name="possibleColumns">The possible columns.</param>
        public void AddRow(ISparsePivotingSolver<double> solver, int row, HashSet<int> possibleRows, HashSet<int> possibleColumns)
        {
            var set = new HashSet<int>();
            var todo = new HashSet<int>();
            todo.Add(row);
            if (_wires.ContainsKey(row))
                return;

            solver.Precondition((matrix, rhs) =>
            {
                while (todo.Count > 0)
                {
                    // Take the row, and find other linked rows
                    var cRow = todo.First();
                    todo.Remove(cRow);
                    var wire = new Wire(cRow);
                    _wires.Add(cRow, wire);
                    var rowElt = matrix.GetFirstInRow(cRow);
                    while (rowElt != null)
                    {
                        // Only consider possible columns
                        if (possibleColumns.Contains(rowElt.Column) && !rowElt.Value.IsZero())
                        {
                            if (rowElt.Value > 0)
                            {
                                if (wire.End != 0)
                                    throw new ArgumentException("Could not find out end");
                                wire.End = rowElt.Column;
                            }
                            else
                            {
                                if (wire.Start != 0)
                                    throw new ArgumentException("Could not find out start");
                                wire.Start = rowElt.Column;
                            }

                            // Find any other linked rows that we will need to schedule
                            var colElt = matrix.GetFirstInColumn(rowElt.Column);
                            while (colElt != null)
                            {
                                if (possibleRows.Contains(colElt.Row) &&
                                    !_wires.ContainsKey(colElt.Row) &&
                                    !colElt.Value.IsZero())
                                {
                                    todo.Add(colElt.Row);
                                }
                                colElt = colElt.Below;
                            }
                        }
                        rowElt = rowElt.Right;
                    }

                    // Deal with unknowns
                    if (!_unknowns.TryGetValue(wire.Start, out var uk))
                    {
                        uk = new Unknown();
                        _unknowns.Add(wire.Start, uk);
                    }
                    uk.Start.Add(wire);
                    if (!_unknowns.TryGetValue(wire.End, out uk))
                    {
                        uk = new Unknown();
                        _unknowns.Add(wire.End, uk);
                    }
                    uk.End.Add(wire);
                }
            });
        }

        /// <summary>
        /// Constrains a wire length (removes it).
        /// </summary>
        /// <param name="row">The row.</param>
        public void ConstrainWire(int row)
        {
            // This wire length has been defined, so let's remove anything that has to do with this row
            if (!_wires.TryGetValue(row, out var wire))
                return;
            _wires.Remove(row);

            // Update the start
            var uk = _unknowns[wire.Start];
            uk.Start.Remove(wire);
            if (uk.Start.Count == 0 && uk.End.Count == 0)
                _unknowns.Remove(wire.Start);

            // Update the end
            uk = _unknowns[wire.End];
            uk.End.Remove(wire);
            if (uk.Start.Count == 0 && uk.End.Count == 0)
                _unknowns.Remove(wire.End);
        }

        /// <summary>
        /// Constrains the specified unknown.
        /// </summary>
        /// <param name="unknown">The unknown.</param>
        public void ConstrainUnknown(int unknown)
        {
            if (!_unknowns.TryGetValue(unknown, out var uk))
                return;
            _unknowns.Remove(unknown);

            // This means that any wires connected to this unknown will be fixed on one side
            IsLoose = false;
            foreach (var wire in uk.Start)
            {
                wire.Start = 0;
                if (wire.Start == wire.End)
                    _wires.Remove(wire.Row);
                else
                {
                    if (!_unknowns.TryGetValue(0, out var f))
                    {
                        f = new Unknown();
                        _unknowns.Add(0, f);
                    }
                    f.Start.Add(wire);
                }
            }
            foreach (var wire in uk.End)
            {
                wire.End = 0;
                if (wire.Start == wire.End)
                    _wires.Remove(wire.Row);
                else
                {
                    if (!_unknowns.TryGetValue(0, out var f))
                    {
                        f = new Unknown();
                        _unknowns.Add(0, f);
                    }
                    f.End.Add(wire);
                }
            }
        }

        /// <summary>
        /// Gets the wires right of the specified unknown.
        /// </summary>
        /// <param name="unknown">The unknown.</param>
        /// <returns>The unknowns on the right.</returns>
        public IEnumerable<int> RightOf(int unknown)
        {
            if (_unknowns.TryGetValue(unknown, out var uk))
                return uk.Start.Select(w => w.Row);
            return Enumerable.Empty<int>();
        }

        /// <summary>
        /// Gets the wires left of the specified unknown.
        /// </summary>
        /// <param name="unknown">The unknown.</param>
        /// <returns>The unknowns on the left.</returns>
        public IEnumerable<int> LeftOf(int unknown)
        {
            if (_unknowns.TryGetValue(unknown, out var uk))
                return uk.End.Select(w => w.Row);
            return Enumerable.Empty<int>();
        }
    }
}
