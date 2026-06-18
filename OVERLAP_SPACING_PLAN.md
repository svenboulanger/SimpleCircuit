# Plan: Automatic spacing of overlapping components

## Goal

Within a connected group, detect components whose bounding boxes overlap and push
them apart by **injecting `MinimumConstraint`s and re-solving** — reusing the existing
SpiceSharp pipeline so wires re-route correctly.

Constraints:

- All changes live in `SimpleCircuit.Lib`.
- Target framework stays `.NET Standard 2.0`.
- No large performance hit for the common (non-overlapping) case.
- Overlap detection is allowed to use only the component bounding boxes.

## Scope

**Within connected groups.** Overlap between *separate* (disconnected) blocks is already
handled by the rigid zig-zag block placement in `GraphicalCircuit.Update()`. This feature
targets components that overlap *inside* a single connected group, where positions are
solver unknowns.

## Why constraint-injection (not displacement)

Inside a connected group a component's position is a solver unknown tied to its wires.
Physically nudging one component would distort the wires the solver routed. Instead we add
a minimum-distance constraint between the two components' location nodes and let SpiceSharp
re-solve everything consistently. This is exactly what the engine already does for pin
spacing (`MinimumConstraint`), so it is idiomatic and respects the existing weighting /
trade-off machinery.

A key property that makes this safe and convergent: a `MinimumConstraint` is expressible
directly between two location nodes. For components A (left) and B (right) we want
`B.Left >= A.Right + s`. Since `A.Right = A.Xnode + aRightOff` and
`B.Left = B.Xnode + bLeftOff` (offsets measured from the solved bounds), this becomes:

```
B.Xnode - A.Xnode >= aRightOff - bLeftOff + s
```

— a plain directional minimum, which `MinimumConstraint.AddMinimum` handles natively
(representative / offset math included).

## Pipeline integration

Refactor `GraphicalCircuit.Solve()` (`SimpleCircuit.Lib/Circuits/GraphicalCircuit.cs:142`)
so the **register -> ground -> OP -> Update** block becomes a reusable private method
`SolveOnce(prepareContext, presences, extraConstraints, diagnostics)`. Then wrap it in a
resolution loop:

```
Prepare(presences)                       // once - unchanged
extra = []                               // accumulated overlap constraints
for i in 0 .. MaxOverlapIterations:
    SolveOnce(prepareContext, presences + extra, diagnostics)   // existing logic
    overlaps = DetectOverlaps(prepareContext)                   // cheap
    if overlaps.Count == 0: break
    added = BuildConstraints(overlaps, prepareContext, ref counter)
    if added.Count == 0: break           // remaining overlaps are rigid -> left as-is
    extra.AddRange(added)
Solved = true
```

The common case (no overlaps, or feature disabled) pays only **one extra detection pass**
— no additional solves.

## Detection (`DetectOverlaps`) — O(n log n)

1. Iterate only connectivity groups (`prepareContext.DrawnGroups.Groups`) with >= 2
   drawables. Single-component groups can't self-overlap; cross-group overlap is already
   handled by the existing block spacing in `Update()`.
2. For each drawable in a group, get its world-space AABB by rendering once to a
   `BoundsBuilder` (the same trick `Update()` already uses, but per-component). Expand each
   by half the desired margin.
3. Find overlapping pairs with a **sweep-line**: sort by `Left`, sweep maintaining an
   active set, emit pairs whose X-intervals overlap and then test Y-overlap. Avoids O(n^2).

## Constraint building (`BuildConstraints`)

This is the core geometric decision: **along which axis and in which direction** to add the
single minimum constraint that separates an overlapping pair (A, B).

### Coordinate setup

SimpleCircuit screen space: X grows right, Y grows down (so `Top < Bottom`). For each
component C with solved location node values `(Cx, Cy)` and world AABB, define four
translation-invariant edge offsets (constant for the solved orientation / scale):

```
offLeft   = C.Bounds.Left   - Cx
offRight  = C.Bounds.Right  - Cx
offTop    = C.Bounds.Top    - Cy
offBottom = C.Bounds.Bottom - Cy
```

These are what turn an edge relationship into a constraint on the location nodes.

### Step 1 - separate on ONE axis only

Two AABBs overlap only when they interpenetrate on *both* axes; opening a gap on *one* axis
is enough (Separating-Axis / Minimum-Translation-Vector idea). Penetration depths:

```
penX = min(A.Right, B.Right) - max(A.Left, B.Left)
penY = min(A.Bottom, B.Bottom) - max(A.Top, B.Top)
```

Pick the axis with the **smaller penetration** - least-disruptive push, and the reason a
single constraint per pair suffices.

### Step 2 - direction (ordering-first, centers only as a fallback)

`MinimumConstraint` enforces `highest_value >= lowest_value + minimum`, so `lowest` = the
node that should stay smaller (the near side). The direction is chosen **to agree with the
existing minimum constraints** (see "Avoiding conflicts" below); only when the pair is
genuinely unordered do we fall back to the current centers, which keeps us moving *with* the
layout rather than against it. The formulas below show the constraint once the order
(A is the lower/near side, B the higher/far side) has been decided.

- **X axis chosen:** if `A.Center.X <= B.Center.X`, A stays left of B (else swap). Require
  `B.Left >= A.Right + s`:

  ```
  Bx - Ax >= A.offRight - B.offLeft + s
  -> MinimumConstraint(lowest = Ax_node, highest = Bx_node,
                       minimum = A.offRight - B.offLeft + s)
  ```

- **Y axis chosen:** if `A.Center.Y <= B.Center.Y`, A stays above B (smaller Y is higher).
  Require `B.Top >= A.Bottom + s`:

  ```
  By - Ay >= A.offBottom - B.offTop + s
  -> MinimumConstraint(lowest = Ay_node, highest = By_node,
                       minimum = A.offBottom - B.offTop + s)
  ```

### Avoiding conflicts with existing minimum constraints (direction safety)

The most important correctness concern: an overlap constraint added in the *wrong* direction
contradicts an existing minimum constraint and distorts the layout.

**Why it is harmful, and why `Weight` cannot fix it.** The underlying constraint
(`Constraints/MinimumConstraints/Biasing.cs`) is a *one-sided spring*: when the gap is below
`Minimum` it switches on with a stiff conductance `gOn = 1e4`; when above it switches off
with `gOff = Weight` (weak). It only ever pushes apart -> it enforces
`highest >= lowest + minimum` in one direction only. If existing constraints require A <= B
on an axis but we add B <= A for overlap removal, we get `B >= A + m1` and `A >= B + m2`
(contradiction). When both are violated, **both** springs are on at the constant `gOn` and
fight to a distorted compromise. `Weight` only sets the *off-state* stiffness `gOff`, so
lowering an overlap constraint's weight does **not** make it yield in a conflict. The only
real protection is correct direction == never creating a contradictory cycle.

**Fix - orient to agree with the existing partial order.** A contradiction is exactly a
directed cycle in the per-axis ordering graph. Per overlapping pair on the chosen axis
(representatives `rA`, `rB`):

1. Build the ordering graph once per iteration from the just-solved circuit:
   - `MinimumConstraints.MinimumConstraint` entities have `Nodes[0]` = higher coordinate,
     `Nodes[1]` = lower -> add edge `lower -> higher`.
   - **Axis-aligned** `SlopedMinimumConstraint` entities (normal has a zero component) also
     project to one clean per-axis edge (see "Sloped constraints" below).
   - All nodes mapped through `Offsets.GetRepresentative`. This captures wire-derived
     constraints, `AlignedComponents`, `BlackBox` pins, and overlap constraints from earlier
     iterations (the loop rebuilds the circuit each pass, so they are present).
2. Decide direction by reachability:
   - `rB` reachable from `rA` (existing order A <= B) -> make A lowest, B highest (forced,
     consistent - just widens the gap).
   - `rA` reachable from `rB` -> make B lowest, A highest.
   - Neither (incomparable) -> free: choose by current centers, then verify the new edge
     does not close a cycle with edges added so far this pass; if it would, flip (the reverse
     is then guaranteed acyclic).
3. Add the chosen edge to the graph incrementally so later pairs in the same pass stay
   consistent too.

This keeps the combined constraint set a DAG per axis -> no contradictions. It is also
strictly better than centers when an existing constraint is currently *violated*: it
reinforces the intended direction instead of opposing it.

**Performance:** one O(E) pass to build the graph + a BFS (O(V+E)) per pair actually
constrained. Overlap pairs are few, so this is negligible next to the re-solve.

**Sloped constraints.** A `SlopedMinimumConstraint` enforces a half-plane
`n . (P2 - P1) >= Minimum + n.o` (one-sided spring along normal `n`; see
`SlopedMinimumConstraints/Biasing.cs`). Two regimes:

- **Axis-aligned normal** (`n.Y == 0` or `n.X == 0`, the `_zeroY`/`_zeroX` fast paths the
  biasing code already special-cases - produced whenever `AddDirectionalMinimum` is given a
  cardinal direction). These reduce *exactly* to one per-axis ordering edge and are
  **included** in v1:
  - `n.Y == 0`: `n.X . (x2 - x1) >= c` -> edge `x1 -> x2` if `n.X > 0`, else `x2 -> x1`.
  - `n.X == 0`: same on the Y nodes by `sign(n.Y)`.
  - Read `entity.Nodes` (x1, y1, x2, y2) and `entity.Parameters.Normal` via the public
    `Component` API in the same build pass.
- **Diagonal normal** (both components non-zero): couples the axes and implies no hard
  per-axis order, so it is **excluded** - and that is safe *by construction*, not a gap: a
  diagonal half-plane always leaves a free perpendicular DOF, so it cannot form the stiff,
  mutually-exclusive cycle that the graph exists to prevent (the solver absorbs an
  axis-aligned overlap push along the free direction). A conservative "project when
  `|n.X| >> |n.Y|`" heuristic could be added later but would over-constrain direction choices
  without removing any real contradiction, so it is out of scope for v1.

### Mapping to overlap "type"

| Overlap shape                         | penX vs penY    | Result                                  |
| ------------------------------------- | --------------- | --------------------------------------- |
| Mostly side-by-side (small h-bite)    | penX < penY     | push apart in X                         |
| Mostly stacked (small v-bite)         | penY < penX     | push apart in Y                         |
| Corner / diagonal                     | comparable      | smaller of the two wins                 |
| One contains the other / concentric   | ~equal, large   | smaller dimension's axis; tie-break dir |

### Degenerate cases

1. **Chosen axis is rigid** (`Offsets.GetRepresentative(A.node) == Offsets.GetRepresentative(B.node)`):
   the relative position can't move on that axis -> fall back to the other axis even if its
   penetration is larger. If both axes are rigid -> skip the pair silently (left as-is).
2. **Penetration tie (`penX == penY`)**: prefer a non-rigid axis; if both free, break
   deterministically (prefer the axis with the larger center separation, falling back to X)
   so re-runs are reproducible.
3. **Coincident centers on the chosen axis** (containment / concentric, direction
   undefined): this only arises for pairs left unordered by the ordering graph; break the tie
   with a stable key (component name) rather than the equal centers - keeps the result
   deterministic and avoids oscillation across iterations.

### Margin handling

Detect overlap on bounds **expanded by the margin** `s` (so boxes closer than `s` count as
overlapping), but compute `minimum` from the **raw** edge offsets plus `s` as shown above -
separated pairs then land at exactly the gap `s`, not `2s`.

### Registration

Create each `MinimumConstraint` with a unique name (e.g. `$"overlap.{counter++}"`) and call
its `Prepare` in `Offsets` / `Groups` mode (harmless - the pair is already in the same
connectivity group) so it registers cleanly.

**Convergence:** once a pair is separated on an axis, the persistent constraint keeps them
apart, so the same pair never re-triggers; only genuinely new pairs appear. Pairs are
finite => it terminates, and `MaxOverlapIterations` (default ~5) is a backstop.

## Configuration (on `GraphicalCircuit`)

- `bool ResolveOverlaps { get; set; }` — **recommended default `false`** (opt-in, matches
  "can be"; avoids silently changing existing diagrams). Easy to flip to `true` later.
- `int MaxOverlapIterations { get; set; } = 5`.
- Reuse `Spacing` for the separation margin, or add `Vector2 OverlapMargin` if a tighter
  gap than the inter-block spacing is wanted.

## Performance

- Disabled or no-overlap: one extra per-component bounds pass + sweep, O(n log n). No extra
  solves.
- With overlaps: at most `MaxOverlapIterations` re-solves; each re-solve is the existing OP.
  Constraint count grows by at most one per distinct overlapping pair.

## Files to touch (all in `SimpleCircuit.Lib`)

- `Circuits/GraphicalCircuit.cs` — refactor `Solve()`, add `SolveOnce`, the resolution
  loop, and config properties.
- New `Circuits/OverlapResolver.cs` (or private helpers) — `DetectOverlaps` (sweep-line) +
  `BuildConstraints`.
- Reuse existing `MinimumConstraint`, `BoundsBuilder`, `Bounds`, `PrepareContext` — no new
  framework deps, all `.NET Standard 2.0`-safe.

## Testing

- Two solver-positioned components forced to overlap -> separated, wires still connect.
- Rigidly-linked overlap -> unchanged (left as-is, no diagnostic).
- Ripple case (A pushed into C) -> resolves within a few iterations.
- Large overlap-free circuit -> assert no extra OP solve runs (perf guard).
