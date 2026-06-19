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

The design splits cleanly along the two constraints:

- **Where the shorthand variants live** → `EvaluationContext` (sections 1–2 below).
- **How a matched shorthand turns into first/last-segment markers** → entirely inside
  `StatementEvaluator.ProcessWire` (section 3 below). No other class
  (`Wire`, `CreateWire`, the parser) needs to change to place the markers.

### 1. `EvaluationContext` owns the registry of possible shorthand variants

The set of possible shorthand variants is **stored on `EvaluationContext`**, right
alongside the existing `Markers` dictionary
([EvaluationContext.cs:63](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs)). This
is the home for the data because `Markers` already lives there, the constructor
already does marker reflection there, and `ProcessWire` already receives the
`EvaluationContext`.

```csharp
// EvaluationContext.cs — next to `public Dictionary<string, Func<Marker>> Markers { get; } = [];`
/// <summary>
/// Maps a wire shorthand class name to the marker factories applied to the
/// start and end of the whole wire. Resolved against <see cref="Markers"/>.
/// </summary>
public Dictionary<string, (Func<Marker> Start, Func<Marker> End)> WireMarkerClasses { get; } = [];
```

Storing the **resolved factories** (not the raw key strings) means each shorthand
is validated once, at construction time, and `ProcessWire` can call the factories
directly without another dictionary lookup.

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

Resolve each `(Start, End)` against `Markers` at registration time (in the
constructor, right after the marker reflection loop). The table is hard-coded, so a
key that doesn't resolve is a **developer error**, not a user-script error: throw
(e.g. `InvalidOperationException` / `KeyNotFoundException`) naming the offending
shorthand and key so it surfaces immediately during development rather than
degrading silently at runtime.

```csharp
foreach (var (cls, start, end) in _defaultWireClasses)
{
    if (!Markers.TryGetValue(start, out var startFactory))
        throw new InvalidOperationException($"Wire class '{cls}' references unknown start marker '{start}'.");
    if (!Markers.TryGetValue(end, out var endFactory))
        throw new InvalidOperationException($"Wire class '{cls}' references unknown end marker '{end}'.");
    WireMarkerClasses.Add(cls, (startFactory, endFactory));
}
```

### 2. Recognize the shorthand in `ProcessWire` (in `StatementEvaluator`)

All wire-side logic stays in `StatementEvaluator.ProcessWire`
([StatementEvaluator.cs:864](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)).
That method already walks `wire.Items` and, for each bare word, looks it up in
`context.Markers` and either collects a `Marker` or pushes the node into
`propertiesAndVariants`. The shorthand recognition slots into the same branches.

The bare-word branches today are:
- `LiteralNode` → [:963–968](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)
- `UnaryNode` (Positive) → [:975–981](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)
- `IdentifierNode` → [:991–994](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)
- `BinaryNode` (Concatenate) → [:1005–1011](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)

In each, insert a check **between** the `context.Markers` lookup and the
variant/property fallback:

1. If the word is in `context.Markers` → existing behavior (positional marker,
   appended to the local `markers` list).
2. **Else if the word is in `context.WireMarkerClasses`** → invoke the resolved
   `Start`/`End` factories and collect the results into two new locals,
   `startWireMarkers` / `endWireMarkers` (see section 3), **and** still push the
   word into `propertiesAndVariants` so it remains a CSS/variant class (decision 1).
3. Else → variant/property fallback (unchanged).

To avoid repeating this in four places, factor it into a small local helper inside
`ProcessWire`, e.g. `bool TryShorthand(string name, SyntaxNode node)`, that does the
`WireMarkerClasses` lookup, fills `startWireMarkers`/`endWireMarkers`, adds the
variant node, and returns whether it matched.

### 3. Apply the collected full-wire markers — also in `ProcessWire`

`ProcessWire` already declares `List<Marker> markers = []` and attaches it to
`segments[0].StartMarkers` (first segment) or `segments[^1].EndMarkers` (last
segment) by list position
([:884–890](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs) and the trailing
block at [:1020–1024](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)). The
shorthand markers reuse exactly this first/last-segment mechanism — they are not a
new concept, just two extra accumulator lists that always target the global ends:

```csharp
// new locals next to `List<Marker> markers = [];`
List<Marker> startWireMarkers = [];   // always → segments[0].StartMarkers
List<Marker> endWireMarkers = [];     // always → segments[^1].EndMarkers
```

After the loop, alongside the existing trailing-`markers` block at
[:1020](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs) and guarded by the same
`segments.Count > 0` (decision 4), append (not overwrite) onto the first/last
segment so shorthand and manually placed markers coexist (decisions 2 & 3):

```csharp
if (segments.Count > 0)
{
    if (startWireMarkers.Count > 0)
        segments[0].StartMarkers = [.. segments[0].StartMarkers ?? [], .. startWireMarkers];
    if (endWireMarkers.Count > 0)
        segments[^1].EndMarkers = [.. segments[^1].EndMarkers ?? [], .. endWireMarkers];
}
```

Because both lists target `segments[0]`/`segments[^1]` regardless of where the
shorthand word appeared in `wire.Items`, a shorthand at the start, middle, or end of
the wire produces identical results.

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

1. **Registry on `EvaluationContext`** — add the `WireMarkerClasses` property and a
   private default table to
   [EvaluationContext.cs](SimpleCircuit.Lib/Evaluator/EvaluationContext.cs);
   populate + validate it in the constructor right after the marker reflection loop
   (resolve start/end keys against `Markers` into factory pairs, **throwing** if a
   key doesn't resolve since the table is hard-coded developer data — no
   reserved-word collision check needed, per decision 5). This is the single source
   of truth for the possible shorthand variants.
2. **Evaluator** — in
   [StatementEvaluator.ProcessWire](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs)
   (and nowhere else): add `startWireMarkers`/`endWireMarkers` locals, the
   `TryShorthand` recognition helper wired into the four bare-word branches, and the
   post-loop application that appends them to `segments[0].StartMarkers` /
   `segments[^1].EndMarkers` (guarded by `segments.Count > 0`, per decision 4). When
   a shorthand is matched, **also** push the word into `propertiesAndVariants` so it
   is added as a variant by `CreateWire`/`ApplyPropertiesAndVariants` (decision 1).
3. **Docs & skill**
   - Update the markers section of the scripting skill
     ([.claude/skills/simplecircuit-scripting/SKILL.md](.claude/skills/simplecircuit-scripting/SKILL.md))
     and `reference/components.md` with the shorthand table.
   - Update the online help / marker listing if markers are surfaced in the UI
     ([SimpleCircuitOnline/Shared/GlobalOptionList.razor.cs](SimpleCircuitOnline/Shared/GlobalOptionList.razor.cs)).
4. **Version bump** consistent with recent commits (e.g. `Bump version to …`).

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
- **Registry integrity** — constructing an `EvaluationContext` (which populates
  `WireMarkerClasses` from the default table) does not throw, i.e. every shorthand's
  start/end key resolves against `Markers`. This is the regression guard against a
  developer typo in `_defaultWireClasses`, replacing what would otherwise be a
  silent runtime failure.

---

## Out of scope

- User-defined shorthands via a control statement (could be a follow-up: a
  `.wireclass NAME start end` directive feeding the same registry).
- New marker glyphs — this feature only composes existing markers.
