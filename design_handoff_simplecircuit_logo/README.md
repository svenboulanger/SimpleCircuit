# Handoff: SimpleCircuit logo & favicon (proposal 2b — Indigo "Route S")

## Overview
A new identity mark and favicon for **SimpleCircuit** (Blazor WASM app hosted at
`svenboulanger.github.io/SimpleCircuit/`, repo `SimpleCircuitOnline` + `SimpleCircuit.Lib`).

The mark is an **orthogonal wire route with a node at each terminal**. The route reads as the
letter **S** (for *Simple*) while being literally a schematic wire: two terminals joined by a
right-angled path with filleted corners — the same routing language the tool itself produces.
Chosen variant: **2b, Indigo `#3b4ec9`** — a brand colour that sits calmly next to the app's
existing Bootstrap blue `#007bff`, which stays the UI/interaction colour (buttons, links).

## About the Design Files
The files in this bundle are **design references created in HTML/SVG** — they show intended look
and proportion, they are not production code to paste in. The task is to drop the exported
**assets** into the app and docs, and to recreate the header lockup using the codebase's own
markup (Razor components + the existing plain-CSS approach in `wwwroot/css/app.css` and the
per-component `*.razor.css` files). No new CSS framework, no icon library.

## Fidelity
**High fidelity.** Colours, geometry, stroke weights and optical sizes below are final. The
provided SVG/PNG/ICO files are production-ready and should be used as-is rather than redrawn.

## Geometry (single source of truth)
All artwork lives on a **100 × 100** viewBox, `fill="none"`, `stroke-linecap="round"`,
`stroke-linejoin="round"`.

Standard route path (used at ≥ 24 px):

```
M86 24 H38 A12 12 0 0 0 38 48 H62 A12 12 0 0 1 62 72 H14
```

* stroke-width `7` for the standalone mark, `10` when knocked out of a solid tile
* terminal nodes: `<circle cx="86" cy="24" r="7">` and `<circle cx="14" cy="72" r="7">`

Small-size route path (used at ≤ 24 px — wider corner radii, heavier stroke, **no nodes**):

```
M82 26 H40 A14 14 0 0 0 40 50 H60 A14 14 0 0 1 60 74 H18
```

* stroke-width `13`

App-tile background: `<rect width="100" height="100" rx="22" fill="#3b4ec9"/>` (22 % corner radius).

Clear space: keep at least **12 units** (12 % of the mark's height) free on all sides.
Minimum sizes: mark 16 px, lockup 96 px wide.

## Assets (in `assets/`)
| File | Use |
| --- | --- |
| `logo-mark.svg` | Mark on light backgrounds — indigo wire, ink nodes |
| `logo-mark-dark.svg` | Mark on dark backgrounds — `#8b97f5` wire, `#f2f4f3` nodes |
| `logo-lockup.svg` | Horizontal lockup, light (420 × 100) |
| `logo-lockup-dark.svg` | Horizontal lockup, dark |
| `favicon.svg` | Vector favicon — white route knocked out of the indigo tile |
| `favicon.ico` | Multi-resolution ICO (16 / 32 / 48, PNG-encoded) |
| `favicon-16/32/48/64.png` | Raster favicons |
| `favicon-180.png` | `apple-touch-icon` |
| `favicon-512.png` | PWA / social / README header |

The 16 px raster uses the small-size path; 32 px and up use the standard path.

## Where the assets go in this codebase
1. Replace `SimpleCircuitOnline/wwwroot/favicon.ico` and `docs/favicon.ico` with `assets/favicon.ico`.
2. Copy `favicon.svg`, `favicon-32.png`, `favicon-180.png` into `SimpleCircuitOnline/wwwroot/`
   (and mirror into `docs/` on publish).
3. In `SimpleCircuitOnline/wwwroot/index.html`, replace the single
   `<link rel="shortcut icon" type="image/x-icon" href="favicon.ico">` with:

```html
<link rel="icon" href="favicon.ico" sizes="any">
<link rel="icon" type="image/svg+xml" href="favicon.svg">
<link rel="apple-touch-icon" href="favicon-180.png">
```

4. Update `<title>SimpleCircuitOnline</title>` → `<title>SimpleCircuit</title>`.
5. README header (repo root `README.md`): use `logo-lockup.svg` with the
   `#gh-light-mode-only` / `#gh-dark-mode-only` pattern already used for the sample circuits,
   pointing the dark fragment at `logo-lockup-dark.svg`.

## Lockup specification
Horizontal lockup, baseline-aligned:

* Mark height **= cap height × 1.55** (at 44 px type → 62 px mark).
* Gap between mark and wordmark **= 16 px at 44 px type** (0.36 × type size).
* Wordmark: `Helvetica Neue, Helvetica, Arial, sans-serif` — matches `app.css` `html, body`.
  * `Simple` — weight **400**, colour `#1b1d1c` (light) / `#f2f4f3` (dark)
  * `Circuit` — weight **700**, colour `#3b4ec9` (light) / `#8b97f5` (dark)
  * letter-spacing `-0.02em`, single word, no space.
* Never re-colour, rotate, outline, or add effects to the mark. Mirroring is not allowed —
  the S direction is the identity.

## Design tokens
| Token | Value | Notes |
| --- | --- | --- |
| `--brand-indigo` | `#3b4ec9` | Logo wire, `Circuit` in the wordmark, favicon tile |
| `--brand-indigo-light` | `#8b97f5` | Dark-background variant of the above |
| `--ink` | `#1b1d1c` | Terminal nodes, `Simple` in the wordmark |
| `--paper` | `#f2f4f3` | Nodes / wordmark on dark |
| `--surface-dark` | `#141716` | Dark-mode backdrop used in the proposal sheet |
| existing `#007bff` | unchanged | Bootstrap blue stays the interaction colour (`Button.razor.css`, `a` in `app.css`) |
| radius (tile) | `22%` | Favicon / app tile |
| radius (UI) | `0.25rem` | Existing app value — unchanged |

The brand indigo is **not** a replacement for `#007bff` in the UI. If a header bar is ever added,
use indigo for the logo only and leave buttons and links on the existing blue.

## Interactions & behaviour
None — this is a static identity. The only behavioural note: the logo lockup in a future app
header should be a link to `/` and use the existing `a` colour rules (no underline, hover
`#0062cc`), with the mark itself unaffected by hover.

## State management
None.

## Files in this bundle
* `assets/…` — production SVG / PNG / ICO exports listed above.
* `SimpleCircuit Logo Proposals.dc.html` — the full proposal sheet (turn 2 = the six colour
  studies including the chosen 2b; turn 1 = the five original mark concepts, kept for context).
  Reference only.

## Note on the alternatives
Five mark concepts (resistor, ground, route, node, loop-C) and six colourways were reviewed.
The route mark in indigo was selected. The runner-up colour was signal orange `#e8590c` if a
louder tab presence is ever wanted; the geometry would not change.
