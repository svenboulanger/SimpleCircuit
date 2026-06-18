# Plan: wire "class" shorthands that expand to start + end markers

## Goal

Allow a single **shorthand class word** placed anywhere on a wire to expand into
a **start marker at the very start of the full wire** and an **end marker at the
very end of the full wire**.

Example of the intended ergonomics (final keywords TBD — see the table):

```
* Today
Entity1 <r erd-one ... erd-many> Entity2   // markers must be hand-placed per segment

* With this feature
Entity1 <one-many r> Entity2               // 'one-many' => start=erd-one, end=erd-many on the whole wire
A <r flow d> B                             // 'flow' => start=dot, end=arrow, regardless of where it sits
```

The shorthand is a convenience layer over the existing marker mechanism: it never
introduces new glyphs, it only maps a name to a pair of **existing** marker keys
(`arrow`, `dot`, `slash`, `erd-one`, `erd-many`, …).

---

## Shorthand table (to be filled in)

The first column is the word typed on the wire. The second and third columns are
the **existing marker keys** (as registered via `[Drawable("…", …)]` on the marker
classes) placed at the wire's global start and global end respectively. Leave a
cell blank/`—` to place no marker on that end.

| Shorthand class    | Start marker (wire start) | End marker (wire end) |
| ------------------ | ------------------------- | --------------------- |
| one-to-one         | erd-only-one              | erd-only-one          |
| one-to-zeroone     | erd-only-one              | erd-zero-one          |
| zeroone-to-one     | erd-zero-one              | erd-only-one          |
| one-to-onemany     | erd-only-one              | erd-one-many          |
| onemany-to-one     | erd-one-many              | erd-only-one          |
| one-to-many        | erd-only-one              | erd-zero-many         |
| many-to-one        | erd-zero-many             | erd-only-one          |
| zeroone-to-zeroone | erd-zero-one              | erd-zero-one          |
| zeroone-to-onemany | erd-zero-one              | erd-one-many          |
| onemany-to-zeroone | erd-one-many              | erd-zero-one          |
| zeroone-to-many    | erd-zero-one              | erd-zero-many         |
| many-to-zeroone    | erd-zero-many             | erd-zero-one          |
| onemany-to-onemany | erd-one-many              | erd-one-many          |
| onemany-to-many    | erd-one-many              | erd-zero-many         |
| many-to-onemany    | erd-zero-many             | erd-one-many          |
| many-to-many       | erd-zero-many             | erd-zero-many         |

> Available marker keys to choose from (from `SimpleCircuit.Lib/Components/Markers`):
> `arrow`, `reverse-arrow`, `dot`, `slash`, `plus`/`plusa`/`plusb`,
> `minus`/`minusa`/`minusb`, `erd-one`, `erd-many`, `erd-one-many`,
> `erd-only-one`, `erd-zero-one`, `erd-zero-many`.

---

## How wires + markers work today (background)

- A wire is parsed into a `WireNode` whose `Items` are direction segments, markers,
  variants, properties and labels — see
  [SimpleCircuitParser.cs](SimpleCircuit.Lib/Parser/SimpleCircuitParser.cs)
  (`ParseWireItem`, `ParseWireDirection`).
- `StatementEvaluator.ProcessWire`
  ([StatementEvaluator.cs:864](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs))
  walks `Items` and builds a `List<WireSegmentInfo>`. A bare word is looked up in
  `context.Markers` (a `Dictionary<string, Func<Marker>>`); if found it is collected
  into a pending `markers` list, otherwise it is treated as a variant/property.
- **Marker positioning is by list order**, not by an explicit "start/end of wire"
  concept:
  - markers pending *before the first segment* → `segments[0].StartMarkers`,
  - markers pending *between segments* → previous segment's `EndMarkers`,
  - markers pending *after the last segment* → `segments[^1].EndMarkers`.
- `Wire.Draw` ([Wire.cs:396](SimpleCircuit.Lib/Components/Wires/Wire.cs)) reads
  `_segments[0].StartMarkers` and each segment's `EndMarkers` to place glyphs.
- Markers are discovered by reflection in
  [EvaluationContext.cs:124](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs) from
  `[Drawable]` attributes on `Marker` subclasses.

Crucially, **"start of the full wire" = `segments[0].StartMarkers`** and
**"end of the full wire" = `segments[^1].EndMarkers`** already exist as concepts —
they are just only reachable today by careful manual placement. The shorthand
targets exactly these two slots.

---

## Proposed design

### 1. A registry mapping class name → (start marker key, end marker key)

Add a lookup that mirrors the existing `Markers` dictionary. Recommended location:
`EvaluationContext` so it lives alongside `Markers` and is easy to extend later.

```csharp
// EvaluationContext.cs
/// <summary>Maps a wire shorthand class to its (start, end) marker keys.</summary>
public Dictionary<string, (string Start, string End)> WireMarkerClasses { get; } = [];
```

Populate it from a single hard-coded table that corresponds 1:1 with the table
above (this is the one place to edit when the table is finalized):

```csharp
private static readonly (string Class, string Start, string End)[] _defaultWireClasses =
[
    ("one-to-one",         "erd-only-one",  "erd-only-one"),
    ("one-to-zeroone",     "erd-only-one",  "erd-zero-one"),
    ("zeroone-to-one",     "erd-zero-one",  "erd-only-one"),
    ("one-to-onemany",     "erd-only-one",  "erd-one-many"),
    ("onemany-to-one",     "erd-one-many",  "erd-only-one"),
    ("one-to-many",        "erd-only-one",  "erd-zero-many"),
    ("many-to-one",        "erd-zero-many", "erd-only-one"),
    ("zeroone-to-zeroone", "erd-zero-one",  "erd-zero-one"),
    ("zeroone-to-onemany", "erd-zero-one",  "erd-one-many"),
    ("onemany-to-zeroone", "erd-one-many",  "erd-zero-one"),
    ("zeroone-to-many",    "erd-zero-one",  "erd-zero-many"),
    ("many-to-zeroone",    "erd-zero-many", "erd-zero-one"),
    ("onemany-to-onemany", "erd-one-many",  "erd-one-many"),
    ("onemany-to-many",    "erd-one-many",  "erd-zero-many"),
    ("many-to-onemany",    "erd-zero-many", "erd-one-many"),
    ("many-to-many",       "erd-zero-many", "erd-zero-many"),
];
```

Resolve each `(Start, End)` against `Markers` at registration time so that an
unknown marker key is reported once, not per wire.

### 2. Recognize the shorthand in `ProcessWire`

In the item loop of `ProcessWire`, in every branch that currently falls through to
"treat as variant/property" for a bare word (`LiteralNode`, `IdentifierNode`,
`UnaryNode Positive`, concatenation), add a check **before** the variant fallback:

1. If the word is in `context.Markers` → existing behavior (positional marker).
2. **Else if the word is in `context.WireMarkerClasses`** → record the resolved
   start-marker factory and end-marker factory into two new locals
   (`fullWireStartMarkers`, `fullWireEndMarkers`) and (decision below) optionally
   still add the word as a variant.
3. Else → variant/property fallback (unchanged).

### 3. Apply the collected full-wire markers after the loop

After the segment list is built (just before/after the existing trailing-marker
block at [StatementEvaluator.cs:1020](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)):

```csharp
if (segments.Count > 0)
{
    if (fullWireStartMarkers.Count > 0)
        segments[0].StartMarkers = Combine(segments[0].StartMarkers, fullWireStartMarkers);
    if (fullWireEndMarkers.Count > 0)
        segments[^1].EndMarkers = Combine(segments[^1].EndMarkers, fullWireEndMarkers);
}
```

`Combine` appends to whatever markers were already placed explicitly so the
shorthand and manual markers coexist rather than overwrite.

---

## Decisions (confirmed)

1. **The shorthand word also remains a CSS/variant class on the wire.** In addition
   to expanding to markers, the word is pushed into `propertiesAndVariants` so the
   wire carries it as a variant/SVG class for styling (`.property wire …`, themes).
   The marker expansion is purely additive.

2. **Shorthand markers combine with explicitly placed start/end markers** — they are
   appended to any manual markers on the same end, exactly like any other variant;
   nothing is overwritten.

3. **Multiple shorthands on one wire combine** — each contributes its start/end
   markers and they all accumulate, consistent with decision 2 and with normal
   variant behavior.

4. **Empty/zero-segment wires are not drawn, so their markers are skipped.** The
   `segments.Count > 0` guard already covers this: no segments → nothing to attach
   markers to → no-op, no error.

5. **Name collisions don't need active guarding.** The parser only reaches the
   variant/shorthand path for tokens it does not consume as direction letters,
   `x`/`?`, etc., so a shorthand named after a reserved word is simply never
   reachable (it is shadowed, not a conflict). No registration-time collision check
   is required; an unknown *marker key* referenced by a shorthand is still validated.

---

## Implementation steps

1. **Registry** — add `WireMarkerClasses` to
   [EvaluationContext.cs](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs) and a
   private default table; populate + validate it in the constructor right after the
   marker reflection loop (resolve start/end keys against `Markers`, post a
   diagnostic for an unknown marker key — no reserved-word collision check needed,
   per decision 5).
2. **New error code** — add to
   [ErrorCodes.cs](SimpleCircuit.Lib/Diagnostics/ErrorCodes.cs) for "unknown marker
   key referenced by wire class".
3. **Evaluator** — in
   [StatementEvaluator.ProcessWire](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs):
   add `fullWireStartMarkers`/`fullWireEndMarkers` locals, the recognition checks in
   the four bare-word branches, the `Combine` helper, and the post-loop application
   (guarded by `segments.Count > 0`, per decision 4). When a shorthand is matched,
   **also** push the word into `propertiesAndVariants` so it is added as a variant by
   `CreateWire`/`ApplyPropertiesAndVariants` (decision 1).
4. **Docs & skill**
   - Update the markers section of the scripting skill
     ([.claude/skills/simplecircuit-scripting/SKILL.md](.claude/skills/simplecircuit-scripting/SKILL.md))
     and `reference/components.md` with the shorthand table.
   - Update the online help / marker listing if markers are surfaced in the UI
     ([SimpleCircuitOnline/Shared/GlobalOptionList.razor.cs](SimpleCircuitOnline/Shared/GlobalOptionList.razor.cs)).
5. **Version bump** consistent with recent commits (e.g. `Bump version to …`).

---

## Tests

Add evaluator/render tests (mirroring existing wire/marker tests) covering:

- A shorthand at the start, middle, and end of a multi-segment wire all produce the
  **same** start+end markers on the full wire.
- Shorthand on a single-segment wire.
- Shorthand combined with a manually placed marker — both are drawn (decision 2).
- Multiple shorthands on one wire — all of their markers accumulate (decision 3).
- Unknown shorthand falls through to variant (no regression).
- Zero-segment wire (e.g. `<>`) with a shorthand is a no-op — not drawn, no markers
  (decision 4).
- The wire's SVG group carries the shorthand as a class (decision 1).

---

## Out of scope

- User-defined shorthands via a control statement (could be a follow-up: a
  `.wireclass NAME start end` directive feeding the same registry).
- New marker glyphs — this feature only composes existing markers.
