---
name: simplecircuit-scripting
description: >-
  Write, read, or debug SimpleCircuit scripts — the text language used by this
  project to sketch electronic circuits, block diagrams, flowcharts and ERDs that
  render to SVG. Use whenever working with SimpleCircuit script (component chains
  like `GND <u> R <r> C`, wires between `<` and `>`, control statements starting
  with `.` such as `.subckt`/`.param`/`.for`/`.if`/`.section`/`.symbol`, virtual
  chains in `( )`, or `.sc` circuit files), or when authoring demos, tutorials,
  unit-test scripts, or documentation for the language. Also covers the
  `SimpleCircuit` command-line tool that renders script files to SVG.
---

# SimpleCircuit scripting language

SimpleCircuit is a scripting language that lets you quickly sketch electronic
circuits (and block diagrams, flowcharts, ERDs). You describe **components** and
the **wires** between them; the engine solves positions and orientations and
renders an SVG. The core idea: *say as little as possible and let the engine fill
in the gaps.*

This skill describes the language implemented in this repository
(`SimpleCircuit.Lib`: `Parser/`, `Evaluator/`, `Components/`). It is **not** about
the online editor UI.

## Mental model

- A script is a sequence of **statements**, one per line. Blank lines are ignored.
- The two workhorse statements are **component chains** and **virtual chains**.
- **Control statements** start with `.` (e.g. `.param`, `.subckt`, `.for`).
- The engine is a constraint solver: a wire `<r>` says "go right at least 10
  units", not "go right exactly here". You add constraints until the drawing is
  pinned down enough to look right.

## Lexical rules

| Element | Syntax |
|---|---|
| Line comment | `* comment` (only when `*` starts a line) **or** `// comment` (anywhere) |
| Line continuation | start the next line with `+` (leading whitespace allowed) |
| String / label | `"text"` or `'text'` |
| Expression | `{ ... }` (evaluated; may be numeric, boolean or string) |
| Statement separator | newline |

```
* This is a comment
ENTplayers("Players", "Player Id",   // labels can wrap
+ "First name", "Last name")          // continued with a leading +
```

## Component chains

A component chain alternates **components** and **wires**:

```
GND <u> R <r> C <d> GND
```

### Components

A component reference is `[pin]Key+name(properties)[pin]` — most parts optional:

- **Key** — the leading letters select the type, exactly like SPICE. `R` is a
  resistor, `C` a capacitor, `GND` a ground. See `reference/components.md` for the
  full catalogue (98 keys).
- **Named vs anonymous** — adding characters after the key *names* the component
  (`R1`, `Rload`, `Cj`). The same name later refers to the **same** component.
  A bare key (`R`, `C`) is **anonymous**: a fresh component every time, not
  reusable by name.
- **Reuse** — naming is how you connect the same node from several lines:
  ```
  T("in") <r> R("1k") <r> Xminus <r> OA1 <r> Xout
  Xminus <u 20 r> R("1k") <r d> Xout    // Xminus and Xout reused here
  ```

### Properties and variants

Inside `( ... )` after a component, comma (or space) separated:

- **Variant** — a bare word toggles a tag: `R(programmable)`, `D(zener)`,
  `C(electrolytic)`. Prefix `-` removes a variant: `X(-dot)`. Prefix `+` is
  explicit add.
- **Property** — `key=value`: `R(zigs=5)`, `R(color="red")`, `S(closed={path==0})`.
- **Label** — a bare string is a label: `R("1k")`, `V("\overline{v^2_n}")`.
  Multiple labels allowed: `R("label 1", "label 2")`.

Common styling properties (most components):
`color`/`fg`/`foreground`, `bg`/`background`, `thickness`/`t`, `fontsize`,
`font`/`fontfamily`, `bold`, `opacity`/`alpha`, `fgo`, `bgo`, and the variants
`dashed`, `dotted`. Colors may be literal (`"red"`, `"#007bff"`) or Bootstrap-style
variables (`"--foreground"`, `"--primary"`, `"--danger"`, …).

### Pins

Components with more than two connection points need an explicit pin in `[ ]`,
*before* the name for the incoming wire and *after* for the outgoing wire:

```
NMOS1[g] <r> T
GND <u> [s]NMOS1[d] <u> POW
OA1[p] <l d> GND          // 'p' = non-inverting input pin
```

A pin must sit on the **side of the component that the wire actually attaches to** —
i.e. between the component and the wire. The pin trailing a component binds the wire
that *leaves* it; the pin leading a component binds the wire that *arrives* at it.
So for two components joined by one wire, the pins face each other across the wire:

```
M1[g] <r> [g]M2          // correct — both pins touch the wire
M1[g] <r> M2[g]          // likely wrong — M2's pin is on its far (right) side
```

Pin names per component are listed in the app's *Help > Components*; common ones:
opamp `p`/`n`/`out`, mosfet `g`/`s`/`d`/`b`, gates `a`/`b`/`c`/`o`.

## Wires

A wire lives between `<` and `>` and is a sequence of **direction segments** and
optional **markers/properties**:

| Segment | Meaning |
|---|---|
| `u` `d` `l` `r` | up, down, left, right (also `n` `s` `w` `e`, and arrows `↑↓←→↖↗↘↙`) |
| `ne` `nw` `se` `sw` | diagonals |
| `a <angle>` | arbitrary angle in degrees, e.g. `a 45`, `a -135` |
| `<dir> <number>` | **fixed** length, e.g. `r 20`, `u 5`, `r 0` |
| `<dir> +<number>` | **minimum** length, e.g. `r +20` (default minimum is 10) |
| `<dir> ++<number>` | minimum length measured between **bounding boxes** |
| `?` / `??` | unknown direction (let the solver decide) |
| `-` | shorthand for `<?>` (a wire of unknown direction) |

Multiple segments chain inside one wire: `<u 20 r 10>`, `<l d a -20 d>`.

```
GND <u> V <u r> R <r> T          // minimum-10 segments
GND <u 5> V <u 20 r 10> R <r 0> T // fixed lengths
GND <a 45> V <a 45 a -45> R       // odd angles
```

Prefer **bare direction** wires (`<r>`, `<u d>`) — they already carry the default
minimum, and the engine now pushes overlapping components apart automatically
(`ResolveOverlaps` is on by default; tune the gap with the `OverlapMarginX` /
`OverlapMarginY` options). Reach for an explicit minimum (`+n`) or fixed (`n`) length
only when you actually need to pin spacing or a position; over-specifying lengths
fights both the solver and the auto-spacing.

### Markers

Place a marker word inside the wire to draw a symbol on it; position depends on
where it sits in the segment list (start, middle, end):

```
Xs1 <d arrow> X
Xs15 <arrow d dot r slash> X     // start, middle, end markers
X2 <r d> Z1                      // no marker
```

Markers: `arrow`, `reverse-arrow`, `dot`, `slash`, `plus`/`plusa`/`plusb`,
`minus`/`minusa`/`minusb`, and ERD cardinalities `erd-one`, `erd-many`,
`erd-one-many`, `erd-only-one`, `erd-zero-one`, `erd-zero-many`.

**ERD cardinality shorthands** — a single word placed *anywhere* in a wire expands
into an ERD marker at the wire's global start and end (and stays a CSS class on the
wire). E.g. `ENTgame <one-to-many r u r> ENTplayers` puts `erd-only-one` at the
start and `erd-zero-many` at the end. Names follow the pattern `<start>-to-<end>`
where each side is `one`/`zeroone`/`onemany`/`many`; see `reference/components.md`
for the full table.

## Virtual chains — alignment

Wrap a chain in `( ... )` to constrain positions **without drawing** anything. Used
to align components. Start with `x`, `y`, or `xy` (default) to limit which axis is
constrained:

```
(y GND)                  // align all anonymous grounds on the y axis (same height)
(x Ti*)                  // align everything matching wildcard Ti* on the x axis
(y R1 <u ++5> R2)        // keep R2 at least 5 above R1's bounding box
(xy XOR2 <d +15> AND1 <d +15> AND2)
```

Virtual chains accept the same direction/length segments as real wires, support
the wildcard `*` (`*load*`, `GND*`), and an anonymous key (`GND`, `T`) acts as a
**catch-all** for all anonymous components of that type.

## Backtracking anonymous components — `~`

To reuse an anonymous component without naming it, backtrack with `~n` (n-th most
recent of that type):

```
R <r> R
R~2("Hello")     // the first R
R~1("World")     // the second R
```

Backtracking is local to the current section / subcircuit definition.

## Queued anonymous points & margins

- **Queued anonymous point** — an `x` (or `.`) inside a wire drops a point that is
  picked up later by an `X` reference, in creation order:
  ```
  T(in) <r x r> C <r x r> T(out)
  X <u r> S <r d> X            // these X's reuse the queued points
  ```
- **Queued margin** — a `++n` virtual segment embedded in a real wire auto-inserts
  spacing between bounding boxes:
  ```
  R1("R_1") <r u ++5 l> R2("R_2")
  ```

## Expressions and parameters

Expressions go in `{ }` and may produce numbers, booleans (`true`/`false`) or
strings. Define parameters with `.param` (order-independent within a scope):

```
.param myLabel = "Hello World"
R(label1={myLabel})

.param rows = 3
X{i} <r +5> R(label1={"R_" + i})    // string concat with +
```

Operators, highest to lowest precedence: unary `+ - ! ++ --`; `* / %`; `+ -`;
`<< >>`; `< > <= >=`; `== !=`; `&`; `^`; `|`; `&&`; `||`; ternary `cond ? a : b`.
Built-in functions: `round(x)` and `round(x, digits)`. Strings concatenate with `+`.

Component names and pin names may embed expressions: `A{componentName}`,
`BB1[special_{"\overline{v^2}"}]`.

## Control statements

All start with `.` immediately followed by the keyword. Block forms end with a
matching `.end…` (most accept `.ends` / `.end` as well).

| Statement | Purpose |
|---|---|
| `.param name = value` | define a parameter in the current scope |
| `.option(s) Key=Value` | global options (e.g. `SpacingX`, `SpacingY`, `KeepUpright`, `ResolveOverlaps`, `OverlapMarginX`, `OverlapMarginY`) |
| `.variant(s) <filter> v1 v2` | force variants on all components matching a filter |
| `.property / .properties <filter> k=v` | force properties on matching components/`wire` |
| `.if {cond}` … `.elif {cond}` … `.else` … `.endif` | conditional blocks |
| `.for i start end inc` … `.endf` | loop; `i` is available as a parameter inside |
| `.scope` … `.ends` | a nested scope for properties/variants/params |
| `.section NAME [props]` … `.ends` | named, isolated group; reuse with `.section B A(props)`; access inner items with `/` (`SEC/Vin`) |
| `.subckt NAME ports [params]` … `.ends` | define a reusable subcircuit; ports are usually `DIR…` points; then use `NAME` as a component key |
| `.box [props]` … `.endb` | draw an annotation box around the enclosed statements |
| `.symbol KEY` … `.ends` | define a custom symbol from SVG-like XML (`<pins>`, `<drawing>`) |
| `.theme NAME k=v` | define a color theme |
| `.include` / `.inc "file"` | include another script |

Filters accept wildcards and `|` alternation: `.property *|wire fg={fg}`,
`.variant ENT*|ENT r=2`. The `wire` keyword targets wires.

```
.for i 0 5 1
    X{i} <r +5> R(label1={"R_" + i}) <r +5> X{i + 1}
.endf

.subckt LPF DIRin[in] DIRout[b] r="1k" c="1uF"
    DIRin <r> R(label1={r}) <r x r 5> DIRout
    X <d> C(label1={c}) <d> GND
.ends
T(in) <r> LPF1 <r> X1            // LPF used as a component
```

## Command-line interface

The `SimpleCircuit` console project (`SimpleCircuit/`, an `Exe` targeting .NET 10,
referencing `SimpleCircuit.Lib`) is a batch converter: it reads script files and
renders each to an image. Run it with `dotnet run --project SimpleCircuit -- <args>`
or invoke the built `SimpleCircuit` executable directly.

### Getting the tool

If `SimpleCircuit` isn't available, a self-contained release binary
(no .NET install required) — offer to download the asset matching 
the user's platform from the project's latest GitHub release:

| Platform | Download URL |
|---|---|
| Windows x64 | `https://github.com/svenboulanger/SimpleCircuit/releases/latest/download/SimpleCircuit-win-x64.zip` |
| Linux x64 | `https://github.com/svenboulanger/SimpleCircuit/releases/latest/download/SimpleCircuit-linux-x64.tar.gz` |
| macOS x64 (Intel) | `https://github.com/svenboulanger/SimpleCircuit/releases/latest/download/SimpleCircuit-osx-x64.tar.gz` |
| macOS arm64 (Apple Silicon) | `https://github.com/svenboulanger/SimpleCircuit/releases/latest/download/SimpleCircuit-osx-arm64.tar.gz` |

Each archive contains a single executable (`SimpleCircuit` / `SimpleCircuit.exe`).
Extract it, and on Linux/macOS make it executable (`chmod +x SimpleCircuit`). Text
measurement uses a font embedded in the tool, so no system fonts or native
libraries are required. Put it on the `PATH` or invoke it by path.

If the user prefers to build from source instead, clone the repository and build
the CLI with `dotnet run --project SimpleCircuit -- <args>` (or publish it), which
requires the .NET 10 SDK. Either way, only proceed once the user confirms.

### Arguments

Arguments are positional and processed left to right. Any token that isn't a
recognised flag is treated as an **input file** (a SimpleCircuit script); each input
file becomes its own job. Relative paths resolve against the current directory.

| Flag | Meaning |
|---|---|
| `<file>` | Add an input script file as a job. The script's extension does not matter — its contents are read as SimpleCircuit script. |
| `-o` / `-out <file>` | Set the output path for the **immediately preceding** input file. |
| `-ef` / `--embed-fonts` | Embed the DejaVu Sans font face directly into the generated SVG(s) via an `@font-face` base64 data-URI, producing fully self-contained SVGs. Applies to every job regardless of position. Off by default; adds ~1 MB per file (~2 MB if bold text is used). |
| `-v` / `--version` | Print the SimpleCircuit library version and continue. |
| `-cli` | After processing any file jobs, drop into interactive mode. |

With no arguments at all, the tool prints `No files specified` and exits.

### Output format

The tool only produces **SVG**. Raster output (PNG/JPG) is no longer supported;
requesting a `.png`/`.jpg`/`.jpeg` output emits a warning and writes an SVG with
the same base name instead.

If `-o` is omitted, the output defaults to the input path with its extension
replaced by `.svg` (same folder, same base name).

```bash
# Render diagram.sc → diagram.svg (next to the input)
SimpleCircuit diagram.sc

# Explicit output path
SimpleCircuit diagram.sc -o out/diagram.svg

# Several files in one run; each -o binds to the file before it
SimpleCircuit a.sc -o a.svg b.sc -o b.svg c.sc

# Print version
SimpleCircuit --version
```

### Interactive mode (`-cli`)

`-cli` starts a read-eval loop after any batch jobs finish. At the `> ` prompt,
type the same file/`-o` arguments you would pass on the command line (quotes group
tokens with spaces). An empty line, `quit`, or `exit` leaves the loop.

```
> diagram.sc -o diagram.svg
> "my circuit.sc" -o "my circuit.svg"
> quit
```

Prefer interactive mode when converting **many files** or rendering the **same file
repeatedly** (e.g. while iterating on a script). The process stays loaded between
prompts, so you pay the startup cost (process launch, JIT warm-up, component-factory
setup) once instead of on every invocation — successive conversions are noticeably
faster than launching the tool afresh per file.

### Diagnostics

Parse and evaluation messages are written to the console. A job that produces
errors is reported but not rendered; a script with no circuit elements reports
`No circuit elements`.

## Reference files

- `reference/components.md` — full catalogue of component keys, categories,
  variants and markers.
- `reference/examples.md` — complete worked scripts (amplifiers, rectifiers,
  flowcharts, arrays, ERDs) copied from the project's demos.

## Authoring tips

- Build the skeleton with named nodes (`Xout`, `Xfb`, `Xminus`) first, then add
  `( … )` virtual chains to align and tidy.
- Prefer bare direction wires (`<r>`, `<u d>`) and let the engine auto-space
  overlapping components (`ResolveOverlaps`, on by default); add an explicit minimum
  (`+n`) or fixed length (`n`) only when you must pin spacing or position exactly.
- Use `(y GND)` / `(x T)` early — aligning grounds and terminals fixes most
  "drifting" layouts.
- When a circuit repeats, reach for `.for`, `.subckt`, or `.section` instead of
  copy-paste.
