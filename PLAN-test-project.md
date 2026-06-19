# Plan: xUnit test project for SimpleCircuit

## Goal
Add a unit test project (`SimpleCircuit.Tests`) based on xUnit that covers the
`SimpleCircuit.Lib` core. Tests range from cheap, deterministic unit tests on the
pure-math/helper types up to end-to-end pipeline tests that take a SimpleCircuit
script string and assert on the resulting circuit / SVG.

## 1. Project setup

Create `SimpleCircuit.Tests/SimpleCircuit.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <IsPackable>false</IsPackable>
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SimpleCircuit.Lib\SimpleCircuit.Lib.csproj" />
  </ItemGroup>
</Project>
```

Notes / decisions:
- **Target `net9.0`** (both .NET 9 and 10 SDKs are installed). The library is
  `netstandard2.0`, so it is referenced fine from a modern test runner TFM.
- **`InvariantGlobalization`** mirrors what `Program.Main` sets at runtime
  (`CultureInfo.InvariantCulture`). SI-suffix / number parsing depends on `.`
  being the decimal separator, so the test process must run invariant too.
- Add the project to `SimpleCircuit.sln` and wire up Debug/Release configs.
- Keep dependencies minimal (no FluentAssertions/Moq for now) — the API is mostly
  static methods and value types, so xUnit's `Assert` is enough.

### Shared test helper
Add `Helpers/TestDiagnosticHandler.cs` — an `IDiagnosticHandler` that records all
posted messages so tests can assert "no errors" or "expected error code X":

```csharp
public sealed class TestDiagnosticHandler : IDiagnosticHandler
{
    public List<IDiagnosticMessage> Messages { get; } = [];
    public void Post(IDiagnosticMessage message) => Messages.Add(message);
    public bool HasErrors => Messages.Any(m => m.Severity == SeverityLevel.Error);
}
```

And a `Helpers/Pipeline.cs` convenience to run a script string through
lexer → parser → evaluator and return `(EvaluationContext, TestDiagnosticHandler)`,
modeled on `Job.Compute()`:

```csharp
var lexer = SimpleCircuitLexer.FromString(script, "test");
var diag = new TestDiagnosticHandler();
var pc = new ParsingContext { Diagnostics = diag };
SimpleCircuitParser.Parse(lexer, pc, out var stmts);
var ctx = new EvaluationContext(true, new Style(), null);
ctx.Diagnostics = diag;          // verify the real wiring while writing the helper
StatementEvaluator.Evaluate(stmts, ctx);
```

## 2. Test areas and proposed cases

Organized roughly cheapest/most-deterministic first. Each bullet is a proposed
`[Fact]` or `[Theory]`.

### 2.1 `Vector2` — `Drawing/Vector2Tests.cs`
Pure value type, fully deterministic — highest ROI.
- Constructor sets `X`/`Y`; `Zero`, `UX`, `UY` constants.
- `Length`: `(3,4)` → `5`.
- `Perpendicular` of `(1,0)` → `(0,1)`; perpendicular is 90° rotation.
- `Dot` / `operator *`(vec,vec): orthogonal vectors → 0; `(2,3)·(4,5)` → 23.
- `Rotate(π/2)` of `(1,0)` ≈ `(0,1)` (use `Assert.True(expected.Equals(actual))`
  because `Equals` uses the type's own 1e-9 tolerance).
- `Scale(sx,sy)`.
- Operators `+ - *(scalar) /(scalar)` and unary `-`; commutativity of `f * v`.
- `Normal(angle)` → unit vector at angle.
- `AtX`: point on a line at a given x; **vertical line (Δx≈0) returns `NaN`**.
- `Equals` tolerance: vectors within 1e-9 are equal, outside are not.
- `GetHashCode` equal for equal vectors (rounding to 9 digits).
- `Order` reorders refs into the first quadrant and returns the abs-direction.
- `ToString` formats `"(x, y)"` (assert under invariant culture).

### 2.2 `Matrix2` — `Drawing/Matrix2Tests.cs`
- `Identity`; `Determinant` (e.g. `[[1,2],[3,4]]` → -2).
- `Inverse` and `Inverse * M == Identity`; `TryInvert` returns false for a
  singular matrix (det 0) and true otherwise.
- `Transposed`.
- `IsOrthonormal`: true for a rotation matrix, false for a scale≠1.
- `Rotate(angle)` matches `Vector2.Rotate` when applied to a vector.
- `Scale(s)` and `Scale(sx,sy)`.
- Operators: `M*M` (associativity vs known product), `M*v`, `M*scalar`,
  `M/scalar`, `+ - ` unary `-`.
- `Equals` / `GetHashCode` tolerance, same pattern as Vector2.

### 2.3 `Utility` — `UtilityTests.cs`
- `IsZero(double)`: true below 1e-9, false above; `IsZero(Vector2)`.
- `IsNaN(double)` / `IsNaN(Vector2)`.
- `ToSVG(double)`: trailing-zero trimming — `1.0`→`"1"`, `1.5`→`"1.5"`,
  `1.234`→`"1.23"` (rounds to 2 decimals), `0`→`"0"`, negative values.
- `ToSVG(Vector2)`: `"x,y"`.
- (XML `ParseScalar`/`ParseCoordinate` helpers can be deferred — they need
  `XmlAttribute` fixtures; lower priority.)

### 2.4 Expression parsing + evaluation — `Evaluator/ExpressionTests.cs`
This is the richest pure-logic surface. Build a small helper that lexes an
expression, calls `ExpressionParser.ParseExpression`, then
`ExpressionEvaluator.Evaluate` against a minimal `EvaluationContext`, returning the
`object` result. Then `[Theory]` over input → expected:

- **Arithmetic**: `1+2`→3, `2*3+4`→10 (precedence), `2+3*4`→14,
  `(2+3)*4`→20 (brackets), `7%3`, `10/4`.
- **Unary**: `-5`, `+5`, `!true`→false, `!false`→true.
- **SI suffixes** (`ParseNumber`): `1k`→1000, `2.2k`→2200, `1m`→1e-3,
  `1meg`→1e6, `1u`→1e-6, `1n`, `1p`, `1f`, `1G`, `1T`, case-insensitivity
  (`1K`==`1k`). Exponent form `1e3`→1000, `1.5e-3`.
- **Booleans / comparisons**: `1 < 2`→true, `2 <= 2`→true, `3 == 3`,
  `3 != 4`, `2 > 5`→false, logical `true && false`, `true || false`.
- **Bitwise / shift**: `6 & 3`, `4 | 1`, `5 ^ 1`, `1 << 3`→8, `16 >> 2`→4.
- **Ternary**: `1 < 2 ? 10 : 20`→10; nested ternary; numeric condition
  (`0 ? a : b` picks b, nonzero picks a).
- **String concat via `+`**: `"a" + "b"`, `"x" + 1` (number→string), `1 + "x"`.
- **Vectors**: `1, 2` parses to a `VectorNode` and evaluates to `Vector2(1,2)`;
  vector `+`/`-`, `vector * scalar`, `scalar * vector`, `vector / scalar`.
- **`round()` function**: `round(2.4)`→2, `round(2.5)`→3, `round(2.345, 2)`,
  wrong arg count posts `ExpectedBetweenArgumentsFor`.
- **Error cases** (assert a diagnostic is posted, result null/false):
  unknown function `foo(1)` → `UnrecognizedFunction`; type mismatch
  `"a" - 1` → `CouldNotOperateForArgumenttype`; unknown variable → 
  `CouldNotFindVariable`; circular variable reference.

### 2.5 Lexer — `Parser/SimpleCircuitLexerTests.cs`
- `FromString` then iterate tokens; assert `TokenType` sequence and `Content`
  for representative inputs: a component chain `GND <u> R <r> C`, a number with
  suffix, a quoted string, a control statement `.param x = 1`.
- Token locations / line tracking (`FromString(..., line)` offset).
- Newline / comment handling (`//` and `/* */` if supported — verify against
  the lexer's actual rules first).

### 2.6 Parser / AST — `Parser/SimpleCircuitParserTests.cs`
- `Parse` of a simple chain returns `true`, no diagnostics, and a
  `ScopedStatementsNode` whose children are the expected node types
  (`ComponentChain` with `ComponentNode` / `WireNode`).
- A virtual chain `( ... )` parses to a `VirtualChainNode`.
- A `.param`/`.for`/`.if` control statement parses to the right node
  (`ParameterDefinitionNode`, `ForLoopNode`, `IfElseNode`).
- Malformed input (e.g. dangling wire `R <r>`) posts a diagnostic and `Parse`
  reports failure — pin down exact expected behavior against the parser first.

### 2.7 End-to-end pipeline — `Integration/PipelineTests.cs`
Use the `Pipeline` helper (§1). These are the highest-value regression tests.
- **Smoke**: `"GND <u> R <r> C"` evaluates with **no error diagnostics** and the
  resulting `ctx.Circuit.Count > 0`, containing the expected named presences
  (e.g. a `GND`, an `R`, a `C` and the wires).
- **Anonymous naming**: two resistors `R R` get distinct full names
  (`R:1`, `R:2` per `GetFullname`).
- **`.param` substitution**: a script using a parameter in a label/value
  resolves to the evaluated value.
- **`.for` loop**: a loop emits the expected number of components.
- **`.if`/`.else`**: only the taken branch's components appear.
- **Solve + render**: call `ctx.Circuit.Solve(diag)` then
  `ctx.Circuit.Render(diag)` and assert a non-null `XmlDocument` whose root is
  `<svg>` with `width`/`height` attributes, for a minimal circuit. Keeps the
  geometry solver honest without asserting exact coordinates.
- **Error surfacing**: an unknown component or bad syntax produces an error
  diagnostic and does not throw.

> For render tests, assert on **structure** (`svg` root exists, has viewBox/size,
> contains N `<path>`/`<g>` elements) rather than exact path data — coordinate
> output from the solver is brittle to pin byte-for-byte.

## Library bugs surfaced by the tests (now fixed)
Writing the tests turned up two genuine defects in `SimpleCircuit.Lib`. Both were
crashes (thrown exceptions). Both have since been **fixed**, and the tests that
captured them are now live regression tests (no longer skipped).

1. **Scientific-notation number parsing threw** — *fixed*
   `ExpressionParser.ParseNumber` (`Parser/ExpressionParser.cs`) handled an exponent
   with `i += 2` and an inner `while`, but the outer `for` loop then ran its own
   `i++`, overshooting the string length. A pure scientific-notation literal such as
   `1e3` or `1.5e-3` threw `ArgumentOutOfRangeException` on the final
   `lexer.Content.ToString()[..i]` slice.
   Fix: the exponent branch now advances past `e`/sign/digits internally and ends
   with an explicit `break`, so the `for`'s `i++` no longer overshoots.
   Regression test: `ExpressionTests.ScientificNotation`.

2. **Binary type-mismatch diagnostic threw** — *fixed*
   `ExpressionEvaluator.Evaluate(BinaryNode)` (`Evaluator/ExpressionEvaluator.cs:299`)
   posted `ErrorCodes.CouldNotOperateForArgumenttype` with two arguments (the two type
   names), but that message string has three placeholders
   (`"Could not {0} for arguments of type '{1}' and '{2}'"`), so `string.Format`
   threw `FormatException`. An expression like `"a" - 1` crashed instead of
   reporting a clean diagnostic.
   Fix: the operation `name` is now passed as the first argument at the call site.
   Regression test: `ExpressionTests.TypeMismatch_PostsDiagnostic`.

## Language notes discovered while writing tests
- The `%` (modulo) and `^` (xor) operators exist in the expression parser/evaluator
  but are **not emitted by the lexer**, so they cannot be used from a script.
- `.for` requires four values: `.for <var> <start> <end> <increment>`.
- `round()` uses `Math.Round`'s default banker's rounding (round-half-to-even), so
  `round(2.5) == 2`.
- Unknown component names do not error: the factory matches the leading key letter,
  so e.g. `Tfoo` resolves to a `T` component named `foo`.

## 3. Out of scope / deferred
- `Index.cs`, `Range.cs` — these are BCL polyfills for netstandard2.0, not our
  logic; skip.
- Font measurement (`FontsTextMeasurer`) and pixel-exact SVG geometry — too
  brittle / environment-dependent for first pass.
- SVG path-data and styles sub-lexers — add later if those areas churn.

## 4. Suggested order of implementation
1. Project + sln wiring + `TestDiagnosticHandler` (verify it builds & runs an
   empty `[Fact]`).
2. `Vector2`, `Matrix2`, `Utility` (pure, no dependencies) — proves the harness.
3. Expression parse+evaluate theories (largest bang for the buck).
4. Lexer + parser AST tests.
5. End-to-end pipeline tests.

## 5. Resolved decisions
- **TFM: `net10.0`** — same as the CLI (`SimpleCircuit/SimpleCircuit.csproj`).
- **No extra libraries** — xUnit + the built-in `Assert` only. No
  FluentAssertions / Moq.
- **No golden-file SVG comparison** — the solver can produce small coordinate
  differences, so render/integration tests assert on structure only
  (`<svg>` root exists, expected child element counts/types), never exact bytes.
