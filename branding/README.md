# SimpleCircuit branding

Source vector artwork for the SimpleCircuit identity. Kept here for reuse regardless of where
it is currently deployed. The mark is an orthogonal wire route with a node at each terminal that
reads as an **S** (for *Simple*) — the same routing language the tool itself produces.

## Files
| File | Use |
| --- | --- |
| `logo-mark.svg` | Mark on light backgrounds — indigo wire, ink nodes |
| `logo-mark-dark.svg` | Mark on dark backgrounds — `#8b97f5` wire, `#f2f4f3` nodes |
| `logo-lockup.svg` | Horizontal lockup (mark + wordmark), light — 420 × 100 |
| `logo-lockup-dark.svg` | Horizontal lockup, dark |
| `favicon.svg` | Vector favicon — white route knocked out of the indigo tile |

The shipped favicons (`.ico` + raster PNGs) live in `SimpleCircuitOnline/wwwroot/`; the CLI
application icon lives at `SimpleCircuit/icon.ico`. This folder holds the vector masters.

## Colour tokens
| Token | Value | Notes |
| --- | --- | --- |
| brand indigo | `#3b4ec9` | Logo wire, `Circuit` in the wordmark, favicon tile |
| brand indigo (light) | `#8b97f5` | Dark-background variant |
| ink | `#1b1d1c` | Terminal nodes, `Simple` in the wordmark |
| paper | `#f2f4f3` | Nodes / wordmark on dark |

Bootstrap blue `#007bff` remains the UI interaction colour (buttons, links) — the brand indigo
is for the logo only.

## Rules
Never re-colour, rotate, outline, mirror, or add effects to the mark — the S direction is part
of the identity. All artwork is on a 100 × 100 viewBox with `stroke-linecap`/`-linejoin` round.

Full design rationale and the original proposal studies are in
`design_handoff_simplecircuit_logo/`.
