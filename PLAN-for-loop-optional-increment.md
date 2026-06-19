# Plan: Optional increment in `.for` loops

## Goal

Allow a `.for` loop to be written with only **3 arguments** (variable, start, end).
When the 4th argument (increment) is omitted, it defaults to:

- `+1` when `end >= start` (counting up)
- `-1` when `end < start` (counting down)

Both forms stay valid:

```
.for i 1 5        ; increment defaults to +1
R
.endfor

.for i 5 1        ; increment defaults to -1
R
.endfor

.for i 1 5 2      ; explicit increment, unchanged
R
.endfor
```

## Background / current behaviour

- **Parser** — `ParseForLoop` in
  [SimpleCircuitParser.cs:1442](SimpleCircuit.Lib/Parser/SimpleCircuitParser.cs:1442)
  parses `variable`, `start`, `end`, then `increment`. The increment is currently
  **mandatory**: if `ParseValueOrExpression` yields `null` (nothing before the
  newline), it posts `ErrorCodes.ExpectedIncrementValue` and fails.
- **Node** — [ForLoopNode.cs](SimpleCircuit.Lib/Parser/Nodes/ForLoopNode.cs)
  stores `Increment` as a non-null `SyntaxNode`; the constructor throws
  `ArgumentNullException` when it is null, and `ToString()` always emits it.
- **Evaluator** — `Evaluate(ForLoopNode, …)` in
  [StatementEvaluator.cs:523](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs:523)
  reads `start`, `end`, `increment`, then **already auto-corrects the sign** of
  the increment to match the loop direction (lines 539-566: it forces the
  increment positive when `end > start` and negative otherwise). So once the
  increment defaults to `1`, the existing logic produces `-1` for a descending
  loop with no extra work.

This means the direction-defaulting requirement is **already satisfied** by the
evaluator — the only real work is making the increment *optional* and feeding a
default of `1` when it is absent.

## Changes

### 1. `ForLoopNode` — allow a null increment
File: [SimpleCircuit.Lib/Parser/Nodes/ForLoopNode.cs](SimpleCircuit.Lib/Parser/Nodes/ForLoopNode.cs)

- Update the XML doc on `Increment` to note it may be `null` (meaning "default
  step of 1 in the loop direction").
- In the constructor, drop the null-guard for `increment`
  (`Increment = increment;` instead of the `?? throw`). Keep the guards on
  `start` and `end`.
- In `ToString()`, only append `' '` + `Increment` when `Increment is not null`,
  so a 3-argument loop round-trips to its 3-argument form.

### 2. `ParseForLoop` — make the increment optional
File: [SimpleCircuitParser.cs:1442](SimpleCircuit.Lib/Parser/SimpleCircuitParser.cs:1442)

Replace the increment block (currently lines ~1471-1478):

```csharp
// Parse the increment value
if (!ParseValueOrExpression(lexer, context, out var increment))
    return false;
if (increment is null)
{
    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedIncrementValue);
    return false;
}
```

with a version that treats a missing increment as "use the default":

```csharp
// Parse the optional increment value. When omitted (next token is the
// newline), increment stays null and the evaluator uses a step of +/-1
// based on the loop direction.
if (!ParseValueOrExpression(lexer, context, out var increment))
    return false;
```

`ParseValueOrExpression` returns `true` with `result == null` when there is no
value before the newline (same convention already used elsewhere), and `false`
only on a genuine parse error — so simply removing the null check makes the
argument optional without masking real errors. The existing newline check that
follows still enforces that nothing unexpected trails the loop header.

**Also fix the wrong guard after the *end* parse** (lines ~1465-1469). It
currently re-checks `start` instead of `end`, so a missing end value is never
caught:

```csharp
// Parse the end value
if (!ParseValueOrExpression(lexer, context, out var end))
    return false;
if (start is null)                                   // <-- bug: should be `end`
{
    context.Diagnostics?.Post(lexer.Token, ErrorCodes.ExpectedEndValue);
    return false;
}
```

Change the condition to `if (end is null)` so an omitted end value posts
`ExpectedEndValue` as intended.

### 3. Evaluator — default the step to 1 when absent
File: [StatementEvaluator.cs:523](SimpleCircuit.Lib/Evaluator/StatementEvaluator.cs:523)

Change the increment read (line ~529) to tolerate a null node:

```csharp
double increment = forLoop.Increment is null
    ? 1.0
    : EvaluateAsDouble(forLoop.Increment, context, 1.0);
```

The sign-correction logic at lines 539-566 then flips `1.0` to `-1.0`
automatically for a descending loop, so no further evaluator change is needed.
The zero-increment guard (line ~532) can never trigger on the default path,
which is correct.

## Tests
File: [SimpleCircuit.Tests/Parser/SimpleCircuitParserTests.cs](SimpleCircuit.Tests/Parser/SimpleCircuitParserTests.cs)

- Update the comment in `ForLoop_ParsesToForLoopNode` (line 53) — the increment
  is no longer always required.
- Add a parser test: `.for i 1 3\nR\n.endfor` parses to a `ForLoopNode` with
  `Increment is null` and no diagnostics.
- Add a parser test for the fixed end guard: `.for i 1\nR\n.endfor` (no end
  value) now reports an error (`ExpectedEndValue`) instead of silently parsing.

Add evaluator/integration coverage (see
[PipelineTests.cs](SimpleCircuit.Tests/Integration/PipelineTests.cs) for the
existing pattern) asserting iteration counts/values:

- `.for i 1 3` runs for `i = 1, 2, 3` (3 iterations, default +1).
- `.for i 3 1` runs for `i = 3, 2, 1` (3 iterations, default -1).
- `.for i 1 3 2` still runs for `i = 1, 3` (explicit increment unchanged).

## Documentation
File: [.claude/skills/simplecircuit-scripting/SKILL.md](.claude/skills/simplecircuit-scripting/SKILL.md)

- Update the syntax row at line 254 to show the increment as optional, e.g.
  `` `.for i start end [inc]` … `.endf` `` and note it defaults to +/-1 in the
  loop direction.
- Optionally add a 3-argument example near the existing one (line 267).

## Out of scope / notes
- The pre-existing `ExpectedIncrementValue` error code stays defined; it is just
  no longer raised by the loop header. Leave it in place (may be reused).

## Verification
1. `dotnet build`
2. `dotnet test` (parser + integration suites green).
3. Manual smoke test of all three forms (`.for i 1 5`, `.for i 5 1`,
   `.for i 1 5 2`) through the CLI or Sandbox to confirm rendered iteration counts.
