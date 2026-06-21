# Plan: Resolve wire markers at draw time so shorthand classes work through the variant system

## Goal

Make shorthand wire classes (e.g. `dbl-arrow`, `fwd-arrow`, `one-to-many`, …) behave like
regular variants: they can be **added and removed** through the normal variant
mechanism (`+class` / `-class`, `.variant`, property toggles, etc.).

**There is no separate "shorthand class" concept.** A shorthand is just a `Marker` like
any other — `dbl-arrow` is a `Marker` that draws an arrow at both wire ends, `one-to-many`
is a `Marker` that draws the ERD shapes at each end. Every marker, simple or composite,
derives from the single `Marker` abstract base, is discovered the same way (reflection over
`[Drawable]`), and is resolved the same way (variant name → `Marker` instance). The plan
therefore **removes** the `_defaultWireClasses` / `WireMarkerClasses` name → (start-key,
end-key) table entirely.

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
  `EvaluationContext` ([SimpleCircuit.Lib/Evaluator/EvaluationContext.cs:63](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs) and `:69`).
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
Two parse-time decisions are baked in and never revisited:
1. The class → markers expansion (`_defaultWireClasses`) happens once, at parse time,
   producing fixed `Marker[]` on the segments.
2. Direct markers are likewise instantiated at parse time onto the segments.

The variant set is the only thing that survives as "live" state, but nothing re-derives
markers from it. Removing the variant later leaves the baked markers in place; adding the
variant later creates no markers. On top of that, shorthand classes are a **second,
parallel** marker mechanism (a name→key-pair table) that exists only because a single
marker could not previously place shapes at *both* ends of a wire — a limitation the
geometry-aware draw (below) removes, letting us delete the table.

---

## Target architecture

Resolve markers **during preparation, from the wire's current variants** (plus the
positionally-placed direct markers), using a marker registry passed down on the prepare
context. The wire builds its `Marker` instances in `Prepare(PreparationMode.Find)` and
stores them; `Draw` only positions and renders them. Everything that draws on a wire is a
`Marker`.

### 1. One marker registry, passed down on `IPrepareContext`
Introduce a single `MarkerRegistry` (in `Components/Markers`) that:
- lazily reflection-scans the assembly once for `[Drawable]` marker types →
  `name → Func<Marker>` (the same scan `EvaluationContext` does today), and
- exposes `TryCreateMarker(string name, out Marker)`.

That is the **whole** registry. There is no shorthand-class table and no
`TryGetWireClass` — shorthands are ordinary entries in the same `name → Func<Marker>`
map because they are ordinary `Marker` types (see §3).

**Delivery to preparation:** expose the registry on `IPrepareContext`
([IPrepareContext.cs](SimpleCircuit.Lib/Circuits/Contexts/IPrepareContext.cs)) — e.g. a
`MarkerRegistry Markers { get; }` property — and surface it from `PrepareContext`
([PrepareContext.cs](SimpleCircuit.Lib/Circuits/Contexts/PrepareContext.cs)). The wire's
marker list is built in `Prepare(PreparationMode.Find)` (the mode already used "for finding
links and references … such as finding pins"), so the registry must be reachable there —
**not** on `IGraphicsBuilder`. Building markers in `Find` (rather than at draw time) means
the resolved markers exist before `Orientation`/`Sizes`/`Offsets`, so they can participate
in later preparation passes if needed, and `Draw` becomes pure rendering. (The registry is
effectively static data — there is no script-level way to define new markers today — so
`PrepareContext` can simply surface a shared singleton; source it from the
`GraphicalCircuit` it already holds.)

`EvaluationContext` is refactored to source `Markers` from the same registry, and its
`WireMarkerClasses` / `_defaultWireClasses` members are **deleted**, so parsing and
preparation agree on one set of names.

**About `BaseGraphicsBuilder.AddMarker`** ([BaseGraphicsBuilder.cs:847](SimpleCircuit.Lib/Drawing/Builders/BaseGraphicsBuilder.cs)):
this is a temporary hardcoded switch added only to support markers inside XML-described
custom components. We do **not** need to unify it for this feature. Preferred: leave it
as-is (the marker changes below do not break it — markers still construct and still draw
via `Location`/`Orientation` + `Draw(builder, style)`). If it gets in the way (e.g. compile
friction from the new geometry-aware draw), simply **remove it and the XML marker hook**
for now; XML-symbol marker support will be re-added on top of `MarkerRegistry` afterwards.
This keeps the current change focused on wires.

### 2. `Marker` receives the full wire geometry when drawing
Add a geometry-aware draw entry point to `Marker`
([Marker.cs](SimpleCircuit.Lib/Components/Markers/Marker.cs)) that receives the **per-segment
start and end points** plus their normals, so a marker can place itself anywhere —
including at *both* ends of the wire, which is what lets composite shorthands be a single
marker. A segment's start and end points are distinct from the neighbouring segment's
because of **rounded corners**: with `Radius > 0` the segment ends *before* the corner arc
and the next segment begins *after* it, so a single shared "corner point" does not exist.
The arrays are therefore per-segment, not per-corner:

```csharp
public virtual void Draw(
    IReadOnlyList<Vector2> startPoints,   // startPoints[i] = start of segment i (after the previous corner arc)
    IReadOnlyList<Vector2> endPoints,     // endPoints[i]   = end of segment i (before the next corner arc)
    IReadOnlyList<Vector2> startNormals,  // tangent leaving startPoints[i]
    IReadOnlyList<Vector2> endNormals,    // tangent arriving at endPoints[i]
    IGraphicsBuilder builder,
    IStyle style)
{
    // Default placement: the segment end indicated by Segment/AtEnd (see below).
    if (AtEnd) { Location = endPoints[Segment];   Orientation = endNormals[Segment]; }
    else       { Location = startPoints[Segment]; Orientation = -startNormals[Segment]; }
    Draw(builder, style); // existing transform-based draw
}
```

- Keep the existing `Marker.Draw(builder, style)` + `Location`/`Orientation` intact —
  it is still used by `BaseGraphicsBuilder.DrawMarkers` for XML/path-symbol markers, and
  composite markers call it once per sub-shape.
- All four arrays have `segments.Count` entries (one per segment, **excluding**
  jump-over points). For a straight wire (`Radius == 0`) `endPoints[i] == startPoints[i+1]`,
  but for rounded corners they differ — `endPoints[i]` is just before the arc and
  `startPoints[i+1]` is just after it; the normals likewise differ across the arc. These
  are captured the same way `Wire.Draw` reads `builder.Start`/`End`/`StartNormal`/`EndNormal`
  per sub-segment today.
- Coordinates stay in the wire's local builder space (wire transform is identity).

### 3. Everything is a `Marker` — no `SegmentMarker`, no class table
`Marker` is the **single** abstract base. Fold the segment-relative placement state onto
it directly:

```csharp
public abstract class Marker(Vector2 location = new(), Vector2 orientation = new())
{
    public Vector2 Location { get; set; } = location;
    public Vector2 Orientation { get; set; } = orientation;

    /// <summary>Segment index this marker is placed on (default whole-wire start = 0).</summary>
    public int Segment { get; set; }

    /// <summary>Whether the marker sits at the end of its segment vs its start.</summary>
    public bool AtEnd { get; set; }

    // §2 geometry-aware Draw (default placement uses Segment/AtEnd) ...
    // existing Draw(builder, style) + abstract DrawMarker(builder, style) ...
}
```

There is **no** `SegmentMarker` subclass. Simple appearance markers (`Arrow`, `Dot`,
`ReverseArrow`, `ERD*`, `Plus`, `PlusB`, `Minus`, `MinusB`, `Slash`) derive directly from
`Marker`, are unchanged apart from inheriting the new members, and rely on the default
geometry-aware `Draw` (place at `Segment`/`AtEnd`, then draw the shape at the origin via
the transform).

**Composite shorthand markers become real `Marker` types.** Each entry that used to live
in `_defaultWireClasses` becomes a marker class with its own `[Drawable]` key that draws
its constituent shapes at the appropriate wire ends by overriding the geometry-aware
`Draw`. They reuse the existing shape markers rather than re-implementing geometry. A tiny
abstract helper keeps them one-liners while still being plain `Marker`s:

```csharp
// Still just a Marker; only exists to share the "draw a start shape and/or an end shape
// at the two ends of the whole wire" logic. Discovered? No — it's abstract, so the
// reflection scan skips it, exactly like Marker itself.
public abstract class WirePairMarker(Marker start, Marker end) : Marker
{
    public override void Draw(startPoints, endPoints, startNormals, endNormals, builder, style)
    {
        if (start is not null) { start.Segment = 0; start.AtEnd = false;
                                 start.Draw(startPoints, endPoints, startNormals, endNormals, builder, style); }
        if (end   is not null) { end.Segment = endPoints.Count - 1; end.AtEnd = true;
                                 end.Draw(startPoints, endPoints, startNormals, endNormals, builder, style); }
    }
}

[Drawable("dbl-arrow", ...)] public sealed class DoubleArrow(Vector2 l = new(), Vector2 o = new())
    : WirePairMarker(new Arrow(), new Arrow()) { protected override void DrawMarker(...) { } }

[Drawable("fwd-arrow", ...)] public sealed class ForwardArrow(...) : WirePairMarker(null, new Arrow()) { ... }
[Drawable("bck-arrow", ...)] public sealed class BackwardArrow(...) : WirePairMarker(new Arrow(), null) { ... }
[Drawable("one-to-many", ...)] public sealed class OneToMany(...) : WirePairMarker(new ERDOnlyOne(), new ERDZeroMany()) { ... }
// … the remaining ERD relationship pairs, one class each …
```

Notes:
- `WirePairMarker` is **not** a parallel hierarchy or a wrapper — it *is* a `Marker`, and
  its constituents are `Marker`s drawn through the same geometry-aware entry point. It only
  exists so the ~19 shorthands don't each repeat the two-ended placement. If preferred, a
  shorthand can skip it and override `Draw` directly; the registry and the wire treat it
  identically either way.
- `DrawMarker(builder, style)` on a composite is a no-op (the composite never draws "at the
  origin"; it delegates to its constituents' geometry-aware draw). Alternatively give
  `Marker.DrawMarker` an empty virtual default so composites need not override it.
- Each composite still needs the `(Vector2, Vector2)` constructor so the reflection scan
  (which looks for that ctor and a `Marker` somewhere in the base chain — see
  [EvaluationContext.cs:157-176](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs)) registers
  its `[Drawable]` key.

Result: `arrow`, `dot`, `dbl-arrow`, `one-to-many`, … all sit in one `name → Func<Marker>`
map and are resolved by one code path.

### 4. Track variant → segment index on the `Wire`
A marker resolved from a variant needs to know which segment it lands on.
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
- **Composite/whole-wire shorthands ignore the tracked segment** — they place themselves
  at both wire ends regardless. The tracked segment matters only for single-shape markers
  resolved from a per-segment variant.

### 5. Wire builds its markers in `Prepare(PreparationMode.Find)`
The wire's marker list is built during preparation, **not** at draw time, in the
`PreparationMode.Find` branch of `Wire.Prepare`
([Wire.cs:110](SimpleCircuit.Lib/Components/Wires/Wire.cs)) — the same pass that already
resolves the wire's pins. Build it as **one uniform path, no shorthand special-case**:

1. Clear any previously-built marker list (preparation can re-run; mirror the
   `PreparationMode.Reset` clearing of `_localPoints`).
2. Start from the positional direct markers (per segment, start/end) → create the `Marker`
   instance via `context.Markers.TryCreateMarker(name, …)` and set its `Segment` index +
   `AtEnd`.
3. For each variant currently in `Variants`:
   - If `context.Markers.TryCreateMarker(variant, out var m)` succeeds → it's a marker
     (simple *or* composite). Set `m.Segment`/`m.AtEnd` from the tracked association
     (whole-wire composites ignore it and self-place). Add it.
   - Else leave it as a plain CSS/SVG class variant (current behavior).
4. Store the resulting `List<Marker>` as wire state (e.g. `_markers`).

Because `Find` runs **after** all parse-time variant/property application (and re-runs when
preparation is re-triggered), reading `Variants` here picks up `+dbl-arrow` / `-dbl-arrow`
and `.variant` toggles — they add/remove markers correctly, and `dbl-arrow` flows through
exactly the same code as `arrow`.

Then `Wire.Draw` only **positions and renders** the pre-built list:
1. Draw the path (unchanged), capturing per-segment `startPoints`, `endPoints`,
   `startNormals`, `endNormals`.
2. Call `marker.Draw(startPoints, endPoints, startNormals, endNormals, builder, style)` for
   each marker in `_markers`. (Markers carry their `Segment`/`AtEnd`; geometry is supplied
   now, when the solved path is known.)

The `Segment`/`AtEnd` assignment happens in `Find`; the actual `Location`/`Orientation`
is computed in `Draw` from the solved geometry — positions aren't known during `Find`.

---

## Detailed change list

| # | File | Change |
|---|------|--------|
| 1 | `Components/Markers/Marker.cs` | Add `Segment`/`AtEnd` and the geometry-aware `Draw(startPoints, endPoints, startNormals, endNormals, builder, style)` virtual (default placement from `Segment`/`AtEnd`); keep existing `Draw(builder, style)`; consider making `DrawMarker` an empty virtual so composites needn't override it. |
| 2 | `Components/Markers/WirePairMarker.cs` (new) | Abstract `WirePairMarker : Marker` holding a start/end constituent `Marker` and drawing each at the whole-wire ends. (Optional helper — composites may override `Draw` directly instead.) |
| 3 | `Components/Markers/*` (new composites) | One `Marker` subclass per former `_defaultWireClasses` row (`DoubleArrow`=`dbl-arrow`, `ForwardArrow`=`fwd-arrow`, `BackwardArrow`=`bck-arrow`, and the ERD relationship pairs `one-to-many`, `many-to-one`, …), each with its `[Drawable]` key and `(Vector2,Vector2)` ctor. |
| 4 | `Components/Markers/MarkerRegistry.cs` (new) | Single registry: `name → Func<Marker>` (reflection scan, identical to today's). `TryCreateMarker`. **No** shorthand-class table. |
| 5 | `Circuits/Contexts/IPrepareContext.cs` + `PrepareContext.cs` | Expose `MarkerRegistry Markers { get; }` (shared singleton, sourced from the `GraphicalCircuit`). This is how the registry reaches `Wire.Prepare`. `BaseGraphicsBuilder.AddMarker` is untouched — leave it, or remove it + the XML marker hook temporarily if it blocks the build. |
| 6 | `Components/Wires/WireSegmentInfo.cs` | Replace baked `Marker[]` with marker **names** (`string[]`) so resolution is deferred to `Prepare(Find)`. |
| 7 | `Components/Wires/Wire.cs` | Add variant→segment tracking storage + API; build the marker list in `Prepare(PreparationMode.Find)` from variants + positional markers (one uniform path), store it as wire state, and clear it on `Reset`; in `Draw` capture per-segment `startPoints`/`endPoints`/`startNormals`/`endNormals` and call the new marker draw signature on the pre-built list. Remove per-marker `Location`/`Orientation` assignment from the path walk. |
| 8 | `Evaluator/StatementEvaluator.cs` (`ProcessWire`/`CreateWire`) | Stop creating marker instances; **delete `TryShorthand`** and the class-expansion path. Recognize direct marker names → store as positional names on the segment; everything else → variant. Track current segment index; pass associations to the `Wire`. |
| 9 | `Evaluator/EvaluationContext.cs` | Source `Markers` from `MarkerRegistry`; **delete `WireMarkerClasses` and `_defaultWireClasses`**. Parser uses the registry only to classify tokens (marker name vs variant), not to instantiate. |

---

## Edge cases & details to handle

- **Jump-over points** (`JumpOver`): excluded from the per-segment arrays passed to
  markers; marker placement keys off segment start/end points only (as today).
- **Rounded corners** (`Radius > 0`): the whole reason the geometry is per-segment —
  `endPoints[i]` (just before the arc) ≠ `startPoints[i+1]` (just after it), and the
  normals differ across the arc too. Capture all four from the path builder as the
  existing code already does per sub-segment.
- **Unconstrained / NaN-orientation segments**: skipped for constraints today; ensure the
  geometry arrays stay index-aligned with `_segments`.
- **Composite constituents share the same geometry**: a composite passes the *same*
  `startPoints`/`endPoints`/normals to its start and end constituents and sets their
  `Segment`/`AtEnd`; no cloning of geometry needed.
- **Coexistence of manual + class markers**: today shorthand markers are appended after
  manual ones (StatementEvaluator.cs:1048-1056). Preserve ordering/coexistence — both are
  now just markers in the same list, so ordering is whatever order they're added in step 5.
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

1. Build the shared `MarkerRegistry` (`name → Func<Marker>` only); point
   `EvaluationContext` at it and expose it on `IPrepareContext` / `PrepareContext`. *(No
   behavior change yet; leave `AddMarker` alone. `_defaultWireClasses` still exists for
   now.)*
2. Add `Segment`/`AtEnd` + the geometry-aware `Marker.Draw` overload to the base `Marker`.
   Refactor `Wire.Draw` to capture per-segment
   `startPoints`/`endPoints`/`startNormals`/`endNormals` and draw the **existing** baked
   markers through the new path. *(Still parse-time resolution; pure refactor.)* If
   `AddMarker` blocks the build, remove it + the XML marker hook here and note it.
3. Convert the `_defaultWireClasses` rows into composite `Marker` types (`WirePairMarker`
   + one class per shorthand), register them, and delete `WireMarkerClasses` /
   `_defaultWireClasses` / `TryShorthand`. *(Shorthands now resolve as plain markers.)*
4. Add variant→segment tracking on `Wire`; move marker resolution from `ProcessWire` into
   the `Prepare(PreparationMode.Find)` branch of `Wire.Prepare` (store the list as wire
   state, clear it on `Reset`); switch segments to store names; drive every marker (simple
   and composite) from `Variants` through the one uniform path. `Wire.Draw` becomes pure
   positioning + rendering of that list. *(Behavior change: classes now toggle.)*
5. Tests + visual regression pass; clean up the now-unused parse-time expansion code.
