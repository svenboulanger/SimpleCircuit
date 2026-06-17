# SimpleCircuit worked examples

These scripts are drawn from the project's built-in demos
(`SimpleCircuitOnline/Demo.cs`). They are the canonical, known-good way to use each
feature. Copy and adapt.

## Minimal main example

```
* A component chain is a sequence of components and wires.
GND <u> V1("V_1") <u r> R1("R") <r> Xout <d> C1("C") <d> GND
Xout <r> T(output, "V_out")

* You can align components using virtual chains
(y GND)
```

## Components: named, anonymous, backtracking

```
* Named: leading letters are the type, the rest names it
V1
Rrest_of_name
V1               // reusing the name refers to the same component

* Anonymous: just the type letters, a new component each time
C
L

* Backtrack to a previous anonymous component with '~'
C~1
L~1
```

## Wires

```
* Directions l/u/r/d, default minimum length 10
GND <u> V <u r> R <r> T

* '+' sets a minimum length, a bare number sets a fixed length
GND <r +20> V
GND <u 5> V <u 20 r 10> R <r 0> T

* Odd angles with 'a' and degrees
GND <a 45> V <a 45 a -45> R <a -45 +20> T
```

## Pins

```
* Pins go in [ ]: before the name for the incoming wire, after for outgoing
NMOS1[g] <r> T
GND <u> [s]NMOS1[d] <u> POW

* Some pins exist only in certain configurations (AND with 3 inputs)
T <r> [a]AND1
T <r> [b]AND1
T <r> [c]AND1
AND1(inputs = 3)
```

## Virtual chains (alignment)

```
GND1 <u> R <u> R <u r +30 d> R <d> GND2

* Align only along y; 'x'/'y'/'xy' (default xy)
(y GND1 <r> GND2)

* '++' guarantees spacing from bounding boxes
(x C~2 <r ++5> C~1)

* Wildcards
GNDa <u +10> Rload1 <u +30> POWvdd1("VDD")
GNDb <u +20> Cload2 <u +20> POWvdd2("VDD")
(y *load*)
```

## Styling and markers

```
R(color="red")
R(alpha=0.5, t=1, fontsize=2, "Text")
AND(dashed)
XOR(fg="--primary")

* Markers on wires (start / middle / end positions)
Xs1 <d arrow> X
Xs3 <d dot> X
Xs15 <arrow d dot r slash> X
(y Xs*)
```

## Labels

```
R("label 1", "label 2")
R(label1="label A", label2="label B\nNew line")

* Limited LaTeX-style markup
V("\overline{v^2_n}")
C("a \textb{\textcolor{red}{b_i}}")

* Choose label anchor and per-label tweaks
ADC("top label", anchor1=1)
T("ABC", font1="Courier", bold1=false, offset1={-1, 3})
```

## Annotation boxes

```
.box
    R1
.endb

R1 <r> C1 <r> L1

.box "Danger zone" r=2 fg="--danger" anchor1="s"
    L1
    S1 <r> Z2
.endb

(x L1 <d> S1)
```

## Parameters, expressions, if/else, for-loops

```
.param myLabel = "Hello World"
R(label1={myLabel})

.param case = 1
.if {case == 0}
    C("Case = 0")
.else
    C("Case != 0")
.endif

.for i 0 5 1
    X{i} <r +5> R(label1={"R_" + i}) <r +5> X{i + 1}
.endf
```

## Sections

```
.section SEC fg="--primary"
    .property *|wire fg={fg}
    GND <u> Vin("Input 1") <u r> R <r> S <r d> C <d> GND
    (y GND)
.ends

* Copy a section with overridden parameters
.section SEC2 SEC(fg="--secondary")

* Access items inside a section with '/'
SEC2/Vin("Input 2")
(y SEC/Vin <r> SEC2/Vin)
```

## Subcircuits

```
.subckt LPF DIRin[in] DIRout[b] r="1k&#937;" c="1uF" fg="--foreground"
    .property *|wire fg = {fg}
    DIRin <r> R(label1={r}) <r x r 5> DIRout
    X <d> C(label1={c}) <d> GND
.ends

T(in, "V_in") <r> LPF1 <r> X1
X1 <ne> LPF(r="2k&#937;", fg="--primary") <ne> T("V_out1", out)
X1 <se> LPF(c="2uF", fg="--secondary") <se> T("V_out2", out)
```

## Custom symbols (SVG-like XML)

```
.symbol DIRTEST
    <pins>
        <pin x="-5" y="0" nx="-1" ny="0" name="in" />
        <pin x="0" y="5" nx="0" ny="1" name="out" />
    </pins>
    <drawing>
        <path d="M-5 0 0 0 0 5" style="stroke: --success; stroke-width: 1;" />
    </drawing>
.ends

DIRTEST <r> DIRTEST <d> DIRTEST
```

Inside `<drawing>` you may use `<circle>`, `<rect>`, `<polygon>`, `<path>`,
`<label>`, `<text>`. The `variant="..."` attribute (with `!`, `and`, `or`)
conditionally draws an element; `marker-start`/`marker-end` add built-in markers.

## Black boxes (dynamic pins)

```
* Pins created on demand; side decided by the connecting wire
BB1[a] <r d> R <d l> [b]BB1

* '_' splits an alias (before) from displayed text (after)
BB1[_shown] <d r> L <r u> [hidden_]BB1
BB1[special_{"\overline{v^2}"}] <l> T(in)
```

## Full example — non-inverting amplifier

```
T("V_in") <r> [p]OA1
OA1 <r> Xout <r> T("V_out")
OA1[n] <l d> Xfb
Xfb <d> R("R_G") <d> GND
Xfb <r> R("R_F", flip) <r u> Xout
```

## Full example — full bridge rectifier

```
Tlt <r> Xlt <r> D1 <r> Xrt <r> Trt("+")
Tlb <r> Xlb <r> [n]D4[p] <r> Xrb <r> Trb("-")
Xlb <u r> D2 <r u> Xrt
Xrb <u l> D3 <l u> Xlt
(y D1 <d +15> D2 <d +15> D3 <d +15> D4)
(x Xlt <r 5> Xlb)
(x Tlt <d> Tlb)
(x Trt <d> Trb)
```

## Full example — parameterised pixel array (for-loops + subcircuit)

```
.param rows = 3
.param columns = 3

.subckt PIXEL DIRleft DIRtop DIRbottom DIRright fg="--foreground"
    .property *|wire fg={fg}
    GND <u> D(photodiode, flip) <u> Xd <u> MNrst <u> POW
    Xd <r> [g]MNsf[d] <u> POW
    MNrst[g] <l> T("RST")
    MNsf[s] <d r> MNsel <r> Xcol
    MNsel[g] <u 60> Xrow
    Xcol <u 70> DIRtop
    Xcol <d 15> DIRbottom
    Xrow <l 60> DIRleft
    Xrow <r 20> DIRright
.ends

.for r 1 {rows} 1
    .for c 1 {columns} 1
        Xh_{r}_{c} <r> PIXEL_{r}_{c} <r> Xh_{r}_{c+1}
        Xv_{r}_{c} <d> [DIRtop]PIXEL_{r}_{c}[DIRbottom] <d> Xv_{r+1}_{c}
    .endf
.endf

PIXEL_{round((rows + 1) / 2)}_{round((columns + 1) / 2)}(fg="--primary")
```
