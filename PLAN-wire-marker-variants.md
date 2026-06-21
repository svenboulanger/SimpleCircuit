# Plan: Resolve wire markers at draw time so shorthand classes work through the variant system

## Goal

Make shorthand wire classes (e.g. `dbl-arrow`, `fwd-arrow`, `one-to-many`, …) behave like
regular variants: they can be **added and removed** through the normal variant
mechanism (`+class` / `-class`, `.variant`, property toggles, etc.).

> **Naming note:** the arrow shorthands were renamed `f-arrow`→`fwd-arrow`,
> `b-arrow`→`bck-arrow`, `d-arrow`→`dbl-arrow` (commit "Rename shorthand arrows to avoid
> conflict with direction"). The old `d-arrow` collided with the down-direction keyword
> `d` (the parser read `d` + `-arrow`). The new names begin with non-direction prefixes,
> so the class names parse cleanly as variants and **no lexer/parser change is required**.

Today these classes are expanded into concrete `Marker` instances **during parsing**
and baked onto the wire's segments. Because the marker instances are fixed at parse
time, toggling the class as a variant afterwards has no effect on what is drawn.

The fix is to stop resolving markers during parsing and instead let the `Wire`
resolve them **at draw time** from its current variant set, using a marker registry
that is passed down to the drawing stage.

---

## Current architecture

### Parsing
- `StatementEvaluator.ProcessWire` ([SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs:866](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs))
  walks the wire's syntax items:
  - **Direct markers** (`arrow`, `dot`, …) are looked up in `context.Markers` and a
    `Marker` instance is created immediately and stored on the segment's
    `StartMarkers` / `EndMarkers`.
  - **Shorthand classes** are resolved by `TryShorthand` against
    `context.WireMarkerClasses`, which creates start/end `Marker` instances appended
    to `segments[0].StartMarkers` / `segments[^1].EndMarkers`, **and** keeps the class
    name as a variant (added to `propertiesAndVariants`).
  - Everything else becomes a variant/property.
- `context.Markers` (reflection over `[Drawable]` marker types) and
  `context.WireMarkerClasses` (from the static `_defaultWireClasses` table) live in
  `EvaluationContext` ([SimpleCircuit.Lib/Evaluator/EvaluationContext.cs:63](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs) and `:75`).
- `CreateWire` builds the `Wire` and applies variants/properties via
  `ApplyPropertiesAndVariants`.

### Storage
- `WireSegmentInfo` ([SimpleCircuit.Lib/Components/Wires/WireSegmentInfo.cs](SimpleCircuit.Lib/Components/Wires/WireSegmentInfo.cs))
  carries `Marker[] StartMarkers` and `Marker[] EndMarkers` — concrete instances.

### Drawing
- `Wire.Draw` ([SimpleCircuit.Lib/Components/Wires/Wire.cs:396](SimpleCircuit.Lib/Components/Wires/Wire.cs))
  builds the path and, while walking it, assigns each pre-created marker's
  `Location`/`Orientation` from the path builder's `Start`/`End`/`StartNormal`/`EndNormal`
  (see [IPathBuilder.cs:12-30](SimpleCircuit.Lib/Drawing/Builders/IPathBuilder.cs)), collects
  them, and draws them after the path.
- `Marker.Draw(builder, style)` ([SimpleCircuit.Lib/Components/Markers/Marker.cs:31](SimpleCircuit.Lib/Components/Markers/Marker.cs))
  applies a transform from `Location`/`Orientation` and calls the abstract
  `DrawMarker`. Concrete markers (`Arrow`, `Dot`, `ERD*`, `Plus/Minus`, `Slash`) only
  know how to draw themselves at the origin.
- A **second** marker resolver exists at draw time:
  `BaseGraphicsBuilder.AddMarker` ([SimpleCircuit.Lib/Drawing/Builders/BaseGraphicsBuilder.cs:847](SimpleCircuit.Lib/Drawing/Builders/BaseGraphicsBuilder.cs)),
  a hardcoded `switch` used for markers declared in XML/path symbol styles. **Note:** it
  disagrees with the reflection registry (e.g. it uses `rarrow`, the attribute key is
  `reverse-arrow`; it has no `dot`). This duplication must be reconciled.

### Variants
- `VariantSet` ([SimpleCircuit.Lib/Components/Variants/VariantSet.cs](SimpleCircuit.Lib/Components/Variants/VariantSet.cs))
  supports `Add`/`Remove`/`Contains` and fires a `Changed` event. `Wire.Draw` already
  reads `Variants.Contains(...)` live (e.g. `Hidden`, `JumpOver`), so anything resolved
  from variants at draw time automatically tracks add/remove.

### Why the current design blocks the goal
The class → markers expansion happens once, at parse time, producing fixed `Marker[]`
on the segments. The variant set is the only thing that survives as "live" state, but
nothing re-derives markers from it. Removing the variant later leaves the baked
markers in place; adding the variant later creates no markers.

---

## Target architecture

Resolve markers **lazily, at draw time, from the wire's current variants** (plus the
positionally-placed direct markers), using a marker registry passed down to drawing.

### 1. Pass the marker registry down to drawing (not parsing)
Provide a draw-time resolver that maps:
- a **marker name** (`arrow`, `dot`, `erd-one`, …) → a new `Marker` instance, and
- a **shorthand class name** (`dbl-arrow`, `one-to-many`, …) → its start/end marker keys.

Introduce a single `MarkerRegistry` (in `Components/Markers`) that:
- lazily reflection-scans the assembly once for `[Drawable]` marker types →
  `name → Func<Marker>` (the same scan `EvaluationContext` does today), and
- holds the shorthand class table → `name → (string Start, string End)` (move
  `_defaultWireClasses` here).
- exposes `TryCreateMarker(string name, out Marker)` and
  `TryGetWireClass(string name, out string start, out string end)`.

**Delivery to draw time:** expose the registry on `IGraphicsBuilder` (e.g. a
`MarkerRegistry Markers { get; }` property that returns the shared registry). `Wire.Draw`
already receives the builder, so this is the "list of valid markers passed down while
drawing" the task calls for. (The registry is effectively static data — there is no
script-level way to define new markers/classes today — so the builder can simply surface
a shared singleton.)

`EvaluationContext` is refactored to source `Markers` / `WireMarkerClasses` from the same
registry, so parsing and drawing agree on names.

**About `BaseGraphicsBuilder.AddMarker`** ([BaseGraphicsBuilder.cs:847](SimpleCircuit.Lib/Drawing/Builders/BaseGraphicsBuilder.cs)):
this is a temporary hardcoded switch added only to support markers inside XML-described
custom components. We do **not** need to unify it for this feature. Preferred: leave it
as-is (reparenting markers onto `SegmentMarker` does not break it — markers still
construct and still draw via `Location`/`Orientation` + `Draw(builder, style)`). If it
gets in the way (e.g. compile friction from the new geometry-aware draw), simply **remove
it and the XML marker hook** for now; XML-symbol marker support will be re-added on top of
`MarkerRegistry` afterwards. This keeps the current change focused on wires.

### 2. `Marker` receives the full wire geometry when drawing
Add a geometry-aware draw entry point to `Marker`
([Marker.cs](SimpleCircuit.Lib/Components/Markers/Marker.cs)) that receives **all** wire
point locations, start normals and end normals, so a marker can place itself anywhere:

```csharp
public virtual void Draw(
    IReadOnlyList<Vector2> points,
    IReadOnlyList<Vector2> startNormals,
    IReadOnlyList<Vector2> endNormals,
    IGraphicsBuilder builder,
    IStyle style)
{
    // Default placement: whole-wire start.
    Location = points[0];
    Orientation = -startNormals[0];
    Draw(builder, style); // existing transform-based draw
}
```

- Keep the existing `Marker.Draw(builder, style)` + `Location`/`Orientation` intact —
  it is still used by `BaseGraphicsBuilder.DrawMarkers` for XML/path-symbol markers.
- `points` has `segments.Count + 1` entries (segment corner points, **excluding**
  jump-over points). `startNormals[i]` / `endNormals[i]` are the path tangents leaving
  point `i` and arriving at point `i+1` (these differ when `Radius > 0`), captured the
  same way `Wire.Draw` reads `builder.StartNormal`/`EndNormal` today.
- Coordinates stay in the wire's local builder space (wire transform is identity).

### 3. `SegmentMarker` abstraction (via inheritance)
Add an abstract `SegmentMarker : Marker` that owns the segment-relative placement, and
**reparent the concrete appearance markers onto it**. The appearance markers keep doing
nothing but `DrawMarker` (their shape at the origin); `SegmentMarker` supplies the
geometry-aware placement:

```csharp
public abstract class SegmentMarker(Vector2 location = new(), Vector2 orientation = new())
    : Marker(location, orientation)
{
    /// <summary>Segment index this marker applies to. Set by the wire on resolve.</summary>
    public int Segment { get; set; }

    /// <summary>Whether the marker sits at the end of the segment vs its start.</summary>
    public bool AtEnd { get; set; }

    public override void Draw(points, startNormals, endNormals, builder, style)
    {
        if (AtEnd) { Location = points[Segment + 1]; Orientation = endNormals[Segment]; }
        else       { Location = points[Segment];     Orientation = -startNormals[Segment]; }
        Draw(builder, style); // existing transform-based draw
    }
}
```

- A start-of-segment marker → `Segment = i, AtEnd = false` → `points[i]`, `-startNormals[i]`.
- An end-of-segment marker → `Segment = i, AtEnd = true` → `points[i+1]`, `endNormals[i]`.
- Whole-wire start/end (shorthand class markers) are just
  `Segment = 0, AtEnd = false` and `Segment = lastSegment, AtEnd = true`.

**Reparenting:** `Arrow`, `Dot`, `ReverseArrow`, `ERD*`, `Plus`, `PlusB`, `Minus`,
`MinusB`, `Slash` change their base class from `Marker` to `SegmentMarker`. Their
`(Vector2, Vector2)` constructors still chain through, so the reflection registry (which
scans for a `(Vector2, Vector2)` ctor and a `Marker` somewhere in the base chain — see
[EvaluationContext.cs:157-176](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs)) keeps
discovering them unchanged. `SegmentMarker` itself is abstract, so it is skipped by the
scan.

The base `Marker` keeps the geometry-aware `Draw(points, …)` virtual (step 2) so the
"draw anywhere on the wire" capability stays on `Marker`; `SegmentMarker` is the common
specialization the wire uses, and the wire's `marker is SegmentMarker` check assigns
`Segment`/`AtEnd`.

### 4. Track variant → segment index on the `Wire`
When a variant is applied during parsing, record the **current segment index** so a
marker resolved from that variant lands on the right segment.
- In `ProcessWire`, maintain a running `currentSegmentIndex` as direction/`-` segments
  are appended.
- Store on the `Wire` a list of `(string Variant, int Segment)` associations (in
  addition to the variant being added to `Variants` as usual).
- Add an API on `Wire` to receive these, e.g.
  `void TrackVariantSegment(string variant, int segment)`, populated from `ProcessWire`
  /`CreateWire`.
- Positionally-placed **direct** markers (`A <r arrow r> B`) keep their segment+position
  too — either retained on `WireSegmentInfo` (as names instead of instances) or folded
  into the same association list.

### 5. Wire builds its markers during draw-time setup
At the start of `Wire.Draw` (after points are known), build the marker list:
1. Start from the positional direct markers (per segment, start/end) → create the
   `SegmentMarker` instance via the registry and set its `Segment` index + `AtEnd`.
2. For each variant currently in `Variants`:
   - If it resolves to a **shorthand class** via `builder.Markers` → create the start/end
     appearance markers (each is a `SegmentMarker`). Whole-wire classes use segment
     `0`/last; if a per-segment association was tracked (step 4), use that.
   - Else if it resolves to a **marker name** → create one `SegmentMarker` at the tracked
     segment.
   - Else leave it as a plain CSS/SVG class variant (current behavior).
   - For each created marker, `if (marker is SegmentMarker sm) { sm.Segment = …; sm.AtEnd = …; }`.
3. Draw the path (unchanged), capturing `points`, `startNormals`, `endNormals`.
4. Call `marker.Draw(points, startNormals, endNormals, builder, style)` for each marker.

Because this reads `Variants` live, `+dbl-arrow` / `-dbl-arrow` and `.variant` toggles now
add/remove markers correctly.

---

## Detailed change list

| # | File | Change |
|---|------|--------|
| 1 | `Components/Markers/Marker.cs` | Add geometry-aware `Draw(points, startNormals, endNormals, builder, style)` virtual; keep existing `Draw(builder, style)`. |
| 2 | `Components/Markers/SegmentMarker.cs` (new) | Abstract `SegmentMarker : Marker` with `Segment`, `AtEnd`, geometry override. |
| 2b | `Components/Markers/{Arrow,Dot,ReverseArrow,ERD*,Plus,PlusB,Minus,MinusB,Slash}.cs` | Reparent: change base from `Marker` to `SegmentMarker` (ctors unchanged). |
| 3 | `Components/Markers/MarkerRegistry.cs` (new) | Single registry: name → `Func<Marker>` (reflection) and shorthand class → `(startKey, endKey)` (move `_defaultWireClasses` here). `TryCreateMarker` / `TryGetWireClass`. |
| 4 | `Drawing/Builders/IGraphicsBuilder.cs` + `BaseGraphicsBuilder.cs` | Expose `MarkerRegistry Markers { get; }` (shared singleton). **Do not** rework `AddMarker`; leave it, or remove it + the XML marker hook temporarily if it blocks the build. |
| 5 | `Components/Wires/WireSegmentInfo.cs` | Replace baked `Marker[]` with marker **names** (`string[]`) so resolution is deferred to draw. |
| 6 | `Components/Wires/Wire.cs` | Add variant→segment tracking storage + API; build the `SegmentMarker` list at draw-time setup from variants + positional markers; capture `points`/`startNormals`/`endNormals`; call new marker draw signature. Remove per-marker `Location`/`Orientation` assignment from the path walk. |
| 7 | `Evaluator/StatementEvaluator.cs` (`ProcessWire`/`CreateWire`) | Stop creating marker instances and stop calling `TryShorthand` expansion. Recognize direct marker names → store as positional names on the segment; everything else → variant. Track current segment index; pass associations to the `Wire`. |
| 8 | `Evaluator/EvaluationContext.cs` | Source `Markers` / `WireMarkerClasses` from `MarkerRegistry`; parser uses it only to classify tokens (direct marker name vs variant), not to instantiate. |

---

## Edge cases & details to handle

- **Jump-over points** (`JumpOver`): excluded from the `points` array passed to markers;
  marker placement keys off segment corner points only (as today).
- **Rounded corners** (`Radius > 0`): `startNormals[i]` ≠ `endNormals[i]`; capture both
  from the path builder as the existing code already does per sub-segment.
- **Unconstrained / NaN-orientation segments**: skipped for constraints today; ensure the
  geometry arrays stay index-aligned with `_segments`.
- **Coexistence of manual + class markers**: today shorthand markers are appended after
  manual ones (StatementEvaluator.cs:1048-1056). Preserve ordering/coexistence.
- **`Plus`/`Minus` opposite-side**: `PlusB`/`MinusB` are their own marker types with
  distinct `[Drawable]` keys, so the reflection registry already produces them — no
  special `OppositeSide` handling needed.
- **`AddMarker` removal (if chosen)**: dropping it temporarily disables markers declared
  in XML symbol style strings (`ParseStyleModifier`). Note it in the commit; re-add on
  `MarkerRegistry` afterwards.
- **Removing direct markers**: direct positional markers historically cannot be removed
  via variants ("Can't remove markers" comment). Keep that limitation — they are stored
  as positional names on the segment, not as variants.
- **Repeated direct markers**: storing direct markers as positional per-segment names
  (not in the variant *set*) preserves the same marker appearing on multiple segments.

---

## Testing

- Unit/script tests under the existing test project for:
  - `wire +dbl-arrow` produces arrows at both ends; `-dbl-arrow` removes them.
  - Toggling a class via `.variant` after wire creation updates the drawing.
  - Per-segment marker placement (`A <r arrow r dbl-arrow d> B`) lands on the correct
    segment, including with `Radius > 0` (rounded corners).
  - ERD relationship classes (`one-to-many`, etc.) still render identically to before.
  - Manual markers and shorthand-class markers coexist (no regressions).
  - `hidden` wires draw no markers.
- Visual regression: render representative `.sc` samples before/after and diff SVG
  (markers should be byte-identical for the unchanged cases).

---

## Suggested implementation order

1. Build the shared `MarkerRegistry` (names + shorthand classes); point
   `EvaluationContext` at it and expose it on `IGraphicsBuilder`. *(No behavior change
   yet; leave `AddMarker` alone.)*
2. Add the geometry-aware `Marker.Draw` overload + abstract `SegmentMarker`, and reparent
   the concrete marker types onto it. Refactor `Wire.Draw` to capture
   `points`/`startNormals`/`endNormals` and draw the **existing** baked markers through
   the new path. *(Still parse-time resolution; pure refactor.)* If `AddMarker` blocks the
   build, remove it + the XML marker hook here and note it.
3. Add variant→segment tracking on `Wire` and populate it from `ProcessWire`.
4. Move marker resolution from `ProcessWire` to `Wire` draw-time setup; switch segments
   to store names; drive shorthand classes from `Variants`. *(Behavior change: classes
   now toggle.)*
5. Tests + visual regression pass; clean up the now-unused parse-time expansion code.
