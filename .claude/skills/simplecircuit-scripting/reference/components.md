# SimpleCircuit component catalogue

Every drawable is registered with a `[Drawable(key, description, category, variants, labelCount)]`
attribute in `SimpleCircuit.Lib/Components/`. This file lists all of them. The
authoritative source is the `[Drawable(...)]` attributes; regenerate by grepping
for `\[Drawable\(` if the library changes.

Usage recap:
- Reference a component by its **key** (leading letters). Extra characters name it.
- Variants are bare words in `( )`; prefix `-` to remove. Properties are `key=value`.
  Bare strings are labels. `labelCount` is how many label anchors the symbol has.

## Analog

| Key(s) | Description | Variants |
|---|---|---|
| `R` | Resistor (potentially programmable), 2 labels | `programmable` `photo` `photoresistor` `thermistor` `memristor` |
| `C` | Capacitor, 2 labels | `electrolytic` `programmable` `sensor` |
| `L` | Inductor, 2 labels | `choke` `programmable` |
| `D` | Diode, 2 labels | `varactor` `zener` `tunnel` `schottky` `schockley` `photodiode` `led` `laser` `tvs` |
| `Z` | Impedance | `programmable` |
| `Y` | Admittance | `programmable` |
| `XTAL` | Crystal | |
| `TL` | Transmission line | |
| `S` | Switch (controlling pin optional), 2 labels | `push` `lamp` `window` `toggle` `knife` `reed` `arei` |
| `SPDT` | Single-pole double-throw switch (controlling pin optional) | |
| `A` | Generic amplifier, 3 labels | `schmitt` `trigger` `comparator` `programmable` |
| `OA` | Operational amplifier, 3 labels | `schmitt` `trigger` `comparator` `programmable` |
| `TA` / `OTA` | Transconductance amplifier, 3 labels | `programmable` |
| `QN` / `NPN` | NPN bipolar transistor | `packaged` |
| `QP` / `PNP` | PNP bipolar transistor | `packaged` |
| `MN` / `NMOS` | N-type MOSFET (bulk optional) | `packaged` `depletion` |
| `MP` / `PMOS` | P-type MOSFET (bulk optional) | `packaged` `depletion` |

## Sources

| Key(s) | Description |
|---|---|
| `V` | Voltage source, 2 labels |
| `I` | Current source, 2 labels |
| `E` / `H` | Controlled voltage source, 2 labels |
| `G` / `F` | Controlled current source, 2 labels |
| `BAT` | Battery (variants `cell` `cells`), 2 labels |

## Digital

| Key(s) | Description |
|---|---|
| `AND` / `NAND` | AND / NAND gate, 3 labels (set input count with `inputs=N`) |
| `OR` / `NOR` | OR / NOR gate, 3 labels |
| `XOR` / `XNOR` | XOR / XNOR gate, 3 labels |
| `BUF` | Buffer, 3 labels |
| `INV` / `NOT` | Inverter, 3 labels |
| `MUX` | Multiplexer |
| `LATCH` | General latch (variants `level` `trigger`), 2 labels |
| `FF` | Flip-flop (variants `edge` `trigger`), 2 labels |
| `ADC` | Analog-to-digital converter, 3 labels |
| `BIT` | Bit vector (variants `bits` `literal` `binary` `bin`); props like `separator`, `msbfirst` |

## General

| Key(s) | Description |
|---|---|
| `GND` | Common ground (variants `earth` `chassis` `vss` `vee`), 2 labels |
| `SGND` | Signal ground (same variants), 2 labels |
| `POW` | Power plane (variants `vdd` `vcc` `vss` `vee`) |
| `T` | Terminal (variants `in` `input` `out` `output` `other` `pad` `square`) |
| `X` (Point) | A point that connects multiple wires; also the catch-all for queued points |
| `DIR` | Directional point — used for subcircuit ports |
| `BB` | Black box — pins created on the fly; first char `n`/`s`/`e`/`w` picks the side; `_` splits alias from displayed pin text |

## Wires

| Key(s) | Description |
|---|---|
| `BUS` | A bus / wire segment |
| `SEG` | A wire segment (variants `underground` `air` `tube` `inwall` `onwall` `arei`) |
| `CUT` | A wire cut (variants `break` `arei`), 2 labels |
| `CB` | Circuit breaker (variants `automatic` `arei`), 2 labels |
| `FUSE` | A fuse, 2 labels |

## Inputs

| Key(s) | Description |
|---|---|
| `MIC` | Microphone, 2 labels |
| `JACK` | (Phone) jack |
| `CONN` | Connector / fastener (variants `male` `female`), 2 labels |
| `ANT` | Antenna |

## Outputs

| Key(s) | Description |
|---|---|
| `LIGHT` | Light point (variants `direction` `directional` `diverging` `projector` `emergency` `wall` `arei`), 2 labels |
| `APP` | Fixed household appliance (variants `ventilator` `heater` `boiler` `cooking` `microwave` `overn` `washer` `dryer` `dishwasher` `refrigerator`/`fridge` `freezer` `accu` `arei`), 2 labels |
| `MOTOR` | Motor, 2 labels |
| `SPEAKER` | Speaker (variants `sound` `music` `audio`) |
| `WP` | Wall plug (variants `earth` `child` `proof` `sealed`), 2 labels |

## Modeling (block diagrams)

| Key(s) | Description |
|---|---|
| `BLOCK` | Generic block with text (variants `box` `rectangle`) |
| `SPLIT` | Block with a split line, 2 labels |
| `ADD` | Addition (variants `plus` `sum`) |
| `SUB` | Subtraction (variants `minus` `difference`) |
| `INT` | Integrator (variant `sum`) |
| `DIFF` | Differentiator (variant `derivative`) |
| `MIX` | Mixer (variant `x`) |
| `FILT` | Filter (variants `lowpass`/`low` `highpass`/`high` `bandpass`/`band`) |
| `CIRC` | Circulator (variant `rotate`) |
| `OSC` | Oscillator (variants `source` `generator`) |

## Flowchart

| Key(s) | Description |
|---|---|
| `FT` | Terminator (variant `pill`) |
| `FP` | Process (variants `box` `rectangle`) |
| `FD` | Decision (variant `diamond`) |
| `FIO` | Input / Output (variant `parallelogram`) |
| `FDOC` | Document |

## Entity-Relationship Diagram (ERD)

| Key(s) | Description |
|---|---|
| `ENT` | Entity — labels after the first become attributes (variants `box` `rectangle`), 3 labels |
| `ATTR` | Attribute (variant `ellipse`) |
| `ACT` | Action (variant `diamond`) |

## Markers (used inside wires, not as components)

`dot`, `arrow`, `reverse-arrow`, `slash`, `plus` / `plusa` / `plusb`,
`minus` / `minusa` / `minusb`, `erd-one`, `erd-many`, `erd-one-many`,
`erd-only-one`, `erd-zero-one`, `erd-zero-many`.

Example: `Xa <d arrow> Xb`, `ENTgame <erd-one-many r u r erd-only-one> ENTplayers`.

## Common style variant pairs (most components)

- `dashed`, `dotted` — line styles.
- Color/region presets: `american`, `euro`, `arei` (regional symbol standards).
