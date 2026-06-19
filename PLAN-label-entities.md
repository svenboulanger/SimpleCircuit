# Plan: HTML-entity keyword translation in labels

## Problem

HTML named entities such as `&Omega;` do not render correctly inside SVG. SVG/XML
only recognizes five named entities (`&amp;`, `&lt;`, `&gt;`, `&apos;`, `&quot;`)
plus numeric character references. So `&Omega;` must be translated to `&#937;`.

### Current flow

`Label.Value` (raw string)
→ [`SimpleTextFormatter.Format`](SimpleCircuit.Lib/Drawing/Spans/SimpleTextFormatter.cs:18) escapes only `<`/`>`
→ [`SimpleTextLexer`](SimpleCircuit.Lib/Parser/SimpleTexts/SimpleTextLexer.cs) treats `&`, letters, `;` as plain `Character`s
→ accumulated verbatim into `TextSpan.Content`
→ measured via [`FontsTextMeasurer.Measure`](SimpleCircuit.Lib/Parser/SimpleTexts/FontsTextMeasurer.cs:94) (calls `WebUtility.HtmlDecode`, so `&Omega;` measures as `Ω`)
→ rendered via [`SvgBuilder.BuildTextSVG`](SimpleCircuit.Lib/Drawing/Builders/SvgBuilder.cs:235) with `element.InnerXml = textSpan.Content`.

### The bug

`InnerXml` parses the content as XML. `&Omega;` is an *undeclared* general entity
→ it throws / fails to render. Meanwhile measurement already decodes it fine, so
layout silently disagrees with output. A literal user-typed `&` has the same latent
problem.

## Design decision

Translate named entities to **numeric character references** (`&Omega;` → `&#937;`)
in `SimpleTextFormatter.Format`, the same single chokepoint that already escapes
`<`/`>`. Numeric refs are valid XML, so they survive `InnerXml` unchanged, and
`WebUtility.HtmlDecode("&#937;")` → `Ω` keeps measurement consistent. No lexer,
parser, or renderer changes needed — the refs flow through as ordinary characters.

Recommended over the alternatives because it's the smallest blast radius, matches the
target output (`&#937;`), and keeps a curated, predictable "common characters" set.

**Alternative considered:** decode everything to real Unicode glyphs via `HtmlDecode`
and switch the renderer off `InnerXml` to escaped text nodes — zero lookup table and
full HTML5 coverage, but it touches the render path and changes output to literal
glyphs. Keep this only if full HTML entity coverage is wanted.

## Steps

### 1. Add an entity table

New static class `SimpleCircuit.Lib/Parser/SimpleTexts/TextEntities.cs` holding
`Dictionary<string, int>` (name → Unicode codepoint). Seed with common
electronics/math symbols:

- Greek: `Omega`/`omega`, `alpha`, `beta`, `gamma`, `mu`, `pi`, `phi`, `theta`, `lambda`, `Delta`, `Sigma`, …
- Symbols: `ohm`, `micro`, `deg`, `plusmn`, `times`, `divide`, `le`, `ge`, `ne`, `approx`, `infin`, `radic`, `sum`, `prod`, `partial`, `nabla`, `middot`, `hellip`, arrows.

Codepoints must match what `HtmlDecode` produces for the same name so measurement and
render agree.

### 2. Add the translation pass

In `SimpleTextFormatter.Format`, applied to `content` **before** the `<`/`>`
escaping. A single regex `&(#x[0-9a-fA-F]+|#[0-9]+|[A-Za-z][A-Za-z0-9]*);` with a
`MatchEvaluator`:

- numeric refs (`&#nnn;`, `&#xhh;`) → passed through unchanged (already valid);
- named refs in the table → rewritten to `&#<codepoint>;`;
- named refs **not** in the table → see step 5.

### 3. Escape stray ampersands

After the entity pass, any remaining bare `&` (not part of a ref) → `&amp;`, so
literal ampersands in labels stop silently breaking output. Order is important:
translate entities → escape leftover `&` → escape `<`/`>` (so we never double-escape
the `&` we just introduced).

### 4. Tests

Add unit tests covering:

- `&Omega;` → `&#937;`;
- numeric ref pass-through;
- unknown entity handling;
- bare `&` escaping;
- an entity adjacent to subscript/markup like `R_&Omega;` and `\textb{&mu;A}`;
- a measurement-vs-render consistency check (decoded width equals rendered glyph).

Match the existing test project's style for label/formatter tests.

### 5. Decision: unrecognized `&name;` (e.g. `&Foo;`)

- (a) leave verbatim + warning (risks invalid SVG),
- (b) escape to `&amp;Foo;` so it renders literally as text,
- (c) fall back to the full `HtmlDecode` set so any standard HTML entity works and
  only truly-unknown ones are escaped.

Recommended: **(c)** — it makes "common characters" effectively "all standard
entities" for free while the curated table just documents the blessed keywords.

### 6. Documentation

Add a labels/entities subsection to
`.claude/skills/simplecircuit-scripting/SKILL.md` (near the `\overline`/`\textcolor`
markup notes) and an example in `reference/examples.md`, listing the supported
keywords.

## Edge cases handled

- Already-numeric refs untouched (idempotent).
- Entities inside sub/superscripts and `\textb{}`/`\textcolor{}` blocks (translation
  is on the raw string, ahead of the lexer, so markup is unaffected).
- Literal `&` no longer corrupts SVG.
- Measurement/render stay in sync because both resolve to the same codepoint.
