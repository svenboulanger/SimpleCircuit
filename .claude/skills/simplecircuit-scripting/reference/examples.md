# SimpleCircuit worked examples

These scripts are the project's built-in demos, transcribed verbatim from
`SimpleCircuitOnline/Demo.cs` (Help &gt; Demo's in the online editor). They are the
canonical, known-good way to use each feature. Copy and adapt. When the demos change,
regenerate this file from `Demo.cs`.

## Main example

```
* Please go to Help > Demo's for more examples and tutorials.

* A component chain is a sequence of components and wires.
GND <u> V1("V_1") <u r> R1("R") <r> Xout <d> C1("C") <d> GND
Xout <r> T(output, "V_out")

* You can align components using virtual chains
(y GND)
```

# Tutorials — Basics

## A1. Components

Named and anonymous components.

```
* --- Components ---

* Named components:
*  - Just like SPICE, the first letters identify the type.
*  - Any following letters make it a named component.
V1
Rrest_of_name

* Reusing the name simply refers to the previous components
V1

* Anonymous components:
*  - Using just the identifying letters is also possible.
*  - Anonymous components are like quick and temporary.
C
L

* The following components create 2 additional components:
C
L

* If you really want to access anonymous components, you
* can backtrack with the '~' operator
* The following components reuse the last created anonymous
* capacitor and inductor
C~1
L~1
```

## A2. Properties and variants

```
* --- Properties and variants ---

* Variants are like tags that you can attach to a component
R
R(programmable)
R(thermistor)
R(euro, memristor)

* Properties are key-value pairs you can pass to a component
R(zigs=5)

* If you want to have all components matching a filter to 
* have certain properties of variants, you can use the
* '.properties' or '.variants' control statement
* These control statements only act in the current scope,
* so if you try moving the '.properties' statement out
* of the scope block, all of the components will turn green
.scope
    .properties * color="green"
    .variants L programmable
    R L C
.ends

* For a list of variants and properties, go to
*   Help > Components
* to have a list of available components with properties/variants.
```

## A3. Wires

```
* --- Wires ---

* Wires are defined between '<' and '>'
* You will mostly be using the directions 'l', 'u', 'r' and 'd'
* for the left, up, right and down direction respectively
GND <u> V <u r> R <r> T

* By default, a wire will have a minimum length of 10
* You can change the minimum length by adding '+' and a
* number after the direction
GND <r +20> V

* You can specify a fixed wire length by adding a number
* after the wire direction without the '+'
GND <u 5> V <u 20 r 10> R <r 0> T

* Odd-angle wires are also possible using the 'a'
* followed by the angle in degrees
GND <a 45> V <a 45 a -45> R <a -45 +20> T

* Components are spaced further apart automatically
* if they would otherwise overlap
NMOS[g] <u r d> [g]NMOS
```

## A4. Pins

```
* --- Pins ---

* For components that have more than 2 pins, you need the
* ability to specify which pin a wire should use to connect.
* This is done using '[' and ']' brackets.

NMOS1[g] <r> T

GND <u> [s]NMOS1[d] <u> POW

* Some pins are only available in specific cases
* For example, a logic AND gate can be configured to have
* multiple inputs
T <r> [a]AND1
T <r> [b]AND1
T <r> [c]AND1

* Commenting out the next line will create errors
* because then there would only be 2 inputs instead of 3.
AND1(inputs = 3)
```

## A5. Virtual chains

```
* --- Virtual chains ---

* Virtual chains are like component chains, but they
* appear between '(' and ')'.

GND1 <u> R <u> R <u r +30 d> R <d> GND2

* Starting a virtual chain with 'x' or 'y' will only align
* items along the x or y axis.
* If no 'x' or 'y' is specified, 'xy' is used which will
* align along both axes.
(y GND1 <r> GND2)

* Anonymous components in virtual chains are a catch-all
* for all anonymous components
GND <u> C <u> C(flip, "This is some pretty long text...") <u r d> C <d> GND
(y GND)

* A '++' operator can be used to guarantee
* distances from bounding boxes
(x C~2 <r ++20> C~1)

* Virtual chains also allow wildcard characters '*'
GNDa <u +10> Rload1 <u +30> POWvdd1("VDD")
GNDb <u +20> Cload2 <u +20> POWvdd2("VDD")
GNDc <u +30> Lload3 <u +10> POWvcc3("VCC")
(y *load*)
```

## A6. Styling

```
* --- Styling ---

* Components can be styled with foreground/background colors
R(color="red")

* Most components have the following properties:
*  - transparency, opacity or alpha: a number from 0 to 1 that specifies the foreground and background opacity (default is 1).
*  - foregroundopacity, fgo: a number from 0 to 1 that overrides only the foreground opacity (default is 1).
*  - backgroundopacity, bgo: a number from 0 to 1 that overrides only the background opacity (default is 1).
*  - thickness, t: a number that determines the line thickness (default is 0.5).
*  - fontsize: a number that determines labels and text size (default is 4).
*  - color, fg, foreground: a string that represents the foreground color (default is '--foreground').
*  - bg, background: a string that represents the background color (default is '--background').
*  - font, fontfamily: the font family name (default is 'Arial').
*  - bold: a boolean that if true turns all labels and text into bold text.
R(alpha=0.5, t=1, fontsize=2, "Text")

* Many components also have the following variants:
*  - dashed: a dashed line style.
*  - dotted: a dotted line style.
AND(dashed)
AND(dotted)

* Rather than specifying colors directly, you can also
* use one of the following color variables.
* These colors follow Bootstrap 5 color definitions.
*  - "--foreground": light mode "#212529", dark mode "#dee2e6"
*  - "--background": "none" by default
*  - "--primary": "#007bff" by default (blueish)
*  - "--secondary": "#6c757d" by default (greyish)
*  - "--success": "#28a745" by default (greenish)
*  - "--warning": "#ffc107" by default (yellowish)
*  - "--danger": "#dc3545" by default (reddish)
*  - "--light": "#f8f9fa" by default
*  - "--dark": "#343a40" by default
XOR(fg="--primary")

* Wires can also be styled with markers
* The following markers are available:
Xs1 <d arrow> X
Xs2 <d reverse-arrow> X
Xs3 <d dot> X
Xs4 <d erd-many> X
Xs5 <d erd-one> X
Xs6 <d erd-one-many> X
Xs7 <d erd-only-one> X
Xs8 <d erd-zero-many> X
Xs9 <d erd-zero-one> X
Xs10 <d plus> X
Xs11 <d plusb> X
Xs12 <d minus> X
Xs13 <d minusb> X
Xs14 <d slash> X

* Markers can also appear in the middle or at the start
Xs15 <arrow d dot r slash> X
(y Xs*)
```

## A7. Labeling

```
* --- Labeling ---

* Most components support labels
* There are two ways to specify labels: through properties,
* or just as strings. There is no limit to the number of labels.
R("label 1", "label 2")
R(label1="label A", label2="label B\nNew line")

* Labels also support a limited set of LaTeX style
* notation for:
*  - \underline{...} for underlined text.
*  - \overline{...} for overlined text.
*  - \textb{...} for bold text.
*  - \textcolor{...}{...} for colored text.
V("\overline{v^2_n}")
C("a \textb{\textcolor{red}{b_i}}")
D("a \textcolor{#007bff}{b^(k)}")

* Labels also accept HTML named entities and numeric character
* references, which are handy for Greek letters and math symbols:
*  - &Omega; &mu; &deg; &plusmn; &le; &ge; &ne; &approx; &infin; etc.
*  - any standard HTML entity name works, as does &#937; or &#x3A9;.
*  - a literal & that is not a valid entity is shown as-is.
R("R = 10 &Omega;")
C("C_load = 4.7 &mu;F")
T("&theta; &le; 90&deg;")

* Labels are placed at anchor positions
* E.g., for an ADC, the first label is normally placed
* inside the symbol, but we can change this by specifying
* the anchor index:
ADC("top label", anchor1=1)

* Per label, you can also specify:
*  - offset#: an offset to tweak label position.
*  - size# or fontsize#: the label font size.
*  - opacity#: a number from 0 to 1 for the transparency (default is 1).
*  - justify# or justification#: if 1, left-aligned, 0 for centered, -1 for right-aligned (default is 1).
*  - ls# or linespacing#: The line spacing for multiline labels (default 1.5).
*  - font# or fontfamily#: the font family name.
*  - bold#: a boolean that can make the label bold.
T("ABC", font1="Courier", bold1=false, offset1={-1, 3})

(y *)
```

## A8. Annotation boxes

```
* --- Annotation Boxes ---

* Annotation boxes are '.box' statements that will draw
* a box around everything inside
.box
    R1
.endb

R1 <r> C1 <r> L1

* Annotation boxes can be styled and labeled
.box "Danger zone" r=2 fg="--danger" anchor1="s"
    L1
    S1 <r> Z2
.endb

(x L1 <d> S1)
```

# Tutorials — Advanced

## B1. Parameters and expressions

```
* --- Variables and Expressions ---

* Like in SPICE, '.param' statements can define parameters
* These can in turn be used in expressions that are placed
* between '{' and '}'
* Also like SPICE, .param statements don't have to appear
* before being used. They are simply defined for the
* current scope
R(label1={myLabel})
.param myLabel = "Hello World"

* Parameters can also be used in component names
.param componentName = "DCmyName"
A{componentName} <r> T(out)
ADCmyName("Label")

* Parameters can also be used in if-else statements
.param case = 1
.if {case == 0}
    C("Case = 0")
.else
    C("Case != 0")
.endif

* Parameters can also be defined in a scope by a for-loop
.for i 0 5 1
    X{i} <r +5> R(label1={"R_" + i}) <r +5> X{i + 1}
.endf
```

## B2. Sections

```
* --- Sections ---

* A section is a group of wires and components that are
* grouped together and are less easily accessed from the
* outside. Sections can have parameters.
.section SEC fg="--primary"
    .property *|wire fg={fg}

    * Any named components are relative to the section
    * Other sections can reuse the name 'Vin' for example
    GND <u> Vin("Input 1") <u r> R <r> S <r d> C <d> GND

    * The virtual chain will only treat anonymous components
    * inside the same section
    (y GND)
.ends

* A previous section can be copied using the format:
.section SEC2 SEC(fg="--secondary")

* It is possible to access elements inside sections using '/'
SEC2/Vin("Input 2")

* Alignment across sections is also possible this way
(y SEC/Vin <r> SEC2/Vin)
```

## B3. Subcircuits

```
* --- Subcircuits ---

* Subcircuits are solved as a separate mini-circuit and
* will then act as a component on their own.
* They should have ports, and can be followed by parameters.
* For example:
.subckt LPF DIRin[in] DIRout[b] r="1k&#937;" c="1uF" fg="--foreground"
    .property *|wire fg = {fg}
    DIRin <r> R(label1={r}) <r x r 5> DIRout
    X <d> C(label1={c}) <d> GND
.ends

* The subcircuit name (LPF) can be used as a component key
* The parameters can be passed like properties
T(in, "V_in") <r> LPF1 <r> X1
X1 <ne> LPF(r="2k&#937;", fg="--primary") <ne> T("V_out1", out)
X1 <se> LPF(c="2uF", fg="--secondary") <se> T("V_out2", out)
```

## B4. Queued anonymous points and queued margins

```
* --- Queued Anonymous Points and Queued Margins ---

* Queued Anonymous Points and Queued Margins are
* completely optional. They are meant to speed up
* describing a circuit, but you can avoid them at
* any point.

.options SpacingY = 20

.box "Queued Anonymous Points" r=3 anchor1="ene" m=5
    * Queued Anonymous Points are quick points that are
    * added in a wire definition using an 'x'. This
    * point is then queued, and can be reused later.
    * This avoids having to invent a lot of names for
    * points.

    * For example, let's say we have the following:
    T(in) <r> X1 <r> C <r> X2 <r> T(out)
    X1 <u r> S <r d> X2

    * We can also used Queued Anonymous Poins instead.
    * When then using anonymous points using 'X', the
    * Queued Anonymous Points will be used first, in
    * the same order as they were created!
    T(in) <r x r> C <r x r> T(out)
    X <u r> S <r d> X
.endb


.box "Queued Margins" r=3 anchor1="ene" m=5
    * Queued Margins are a way of describing a virtual
    * wire together with a normal wire. For example,
    * to avoid overlap between the following resistors,
    * we may add a '++5' virtual wire that uses the
    * bounds of the resistors to space them apart.
    R1("R_1") <r u l> R2("R_2")
    (y R1 <u ++5> R2)

    * The same circuit can be described using a
    * Queued Margin as follows:
    R1b("R_1") <r u ++5 l> R2b("R_2")

    * Every time a component that is eligible is
    * reference, SimpleCircuit will trace back
    * until the last eligible component and automatically
    * adds a virtual wire with any queued margins in
    * between.
.endb
```

## B5. Black boxes

```
* --- Black Boxes ---

* Black Boxes ('BB') are components that we don't know the pins
* of beforehand. Pins of a 'BB' are created dynamically.

* The following chain creates 2 pins with the name 'a' and 'b'
* The order is important! The first pin will appear top or left of
* the component. The wire that connects to the pin determines
* on which side the pin should be added.
BB1[a] <r d> R <d l> [b]BB1

* The first underscore character '_' has special meaning
* Everything before the first '_' will be used as an alias for the pin
* Everything after the first '_' will be displayed as the pin name.
* The whole pin name can always be used as a reference, and
* without '_', the whole name is used as the pin
BB1[_shown] <d r> L <r u> [hidden_]BB1

* Using an expression, you can pretty make anything act as
* the pin text.
BB1[special_{"\overline{v^2}"}] <l> T(in)
```

## B6. Backtracking anonymous components

```
* --- Backtracking Anonymous Components ---

* It is sometimes tedious to invent names over and over
* again if you need to work with components that, for
* example, have multiple pins, or require multiple
* references
* In such cases, anonymous components can be backtracked
* by using the tilde '~' operator followed by how much
* needs to be backtracked
R <r> R
R~2("Hello")
R~1("World")

* Just remember that backtracking is local to the current
* section or current subcircuit definition
.section SEC
    * Backtracking cannot find components outside the section
    * Uncommenting the following line leads to an error
    * R~1("lbl")

    * Backtracking to components inside the section is OK
    R("Dummy")
    R~1(zigs=2)
.ends

* This backtracked component cannot access the 
* anonymous R inside section 'SEC'.
* It instead points to the R before the section.
R~1(programmable)

* It can sometimes be useful when combined with control
* statements, such as for-loops or if-else statements.
.param programmable = true
.if {programmable}
    R(programmable)
.else
    R
.endif
R~1("Outside")
```

## B7. Custom symbols

```
* --- Custom symbols ---

* Custom symbols allow you to make your own graphic
* using the SimpleCircuit system using SVG-like XML.
.symbol TEST
    <pins></pins>
    <drawing>
        <circle x="0" y="0" r="2" />
    </drawing>
.ends

* Minimal example, the symbol does not have any pins...
TEST

* You can give pins to the symbol with an optional
* direction
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

* Labels and text also work a bit differently
* to fit in the SimpleCircuit system.
.symbol LBL
    <pins>
        <pin x="0" y="0" name="in" />
    </pins>
    <drawing fg="--primary">
        <!-- marker-start and marker-end allow add built-in markers -->
        <!-- variant allows you to only draw something if the variant is defined -->
        <path d="M3,-3 c3 0 10 -1 10 -5"
            marker-start="arrow"
            fg="--primary"
            variant="arrow" />
        <path d="M3,-3 c3 0 10 -1 10 -5"
            fg="--primary"
            variant="!arrow" />
        <!-- ex,ey is the vector to which quadrant the text can expand -->
        <!-- x, y is the vector of the location -->
        <!-- nx, ny is the orientation of the text -->
        <label
            x="13" y="-9"
            ex="1" ey="-1" />
        <!-- anchor is a description the location should coincide with -->
        <text x="10" y="-3"
            nx="2" ny="1"
            anchor="top-left"
            value="(arrow)"
            variant="arrow"
            style="font-size:3pt;fg:#ccc;" />
    </drawing>
.ends

R <d> LBL("This is with arrow", arrow) <d> R <d> LBL("This is without arrow") <d> R
```

# Examples

## Custom 7-segment display symbols

Custom symbols making a 7-segment display whose displayed numbers change with variants.

```
.symbol SEGMENT
    <pins>
        <pin name="a" x="0" y="3.625" nx="-1" />
        <pin name="b" x="0" y="10.875" nx="-1" />
        <pin name="c" x="0" y="18.125" nx="-1" />
        <pin name="d" x="0" y="25.375" nx="-1" />
        <pin name="plus" x="8.75" y="29" ny="1" />
        <pin name="e" x="17.5" y="25.375" nx="1" />
        <pin name="f" x="17.5" y="18.125" nx="1" />
        <pin name="g" x="17.5" y="10.875" nx="1" />
        <pin name="h" x="17.5" y="3.625" nx="1" />
    </pins>
    <!-- Note that the full SVG specification is not supported. -->
    <!-- Only basic SVG XML is supported due to it needing to interface with SimpleCircuit. -->
    <drawing scale="0.05">
        <rect x="0" y="0" width="350" height="580" style="fill:black;stroke:#666;" />
        <g>
            <!-- The "variant" attribute will control when a tag is drawn -->
            <!-- Middle bar -->
            <polygon points="279,288 243,324 99,324 63,288 99,252 243,252" style="fill:red; stroke: none;" variant="!(zero or one or seven)" />
            <polygon points="279,288 243,324 99,324 63,288 99,252 243,252" style="fill:#666; stroke: none;" variant="zero or one or seven" />

            <!-- Bottom bar -->
            <polygon points="279,521 243,557 99,557 63,521 99,485 243,485" style="fill:red; stroke: none;" variant="!(one or four or seven)" />
            <polygon points="279,521 243,557 99,557 63,521 99,485 243,485" style="fill:#666; stroke: none;" variant="one or four or seven" />

            <!-- Bottom-right bar -->
            <polygon points="288,296 324,332 324,476 288,512 252,476 252,332" style="fill:red; stroke: none;"  variant="!two" />
            <polygon points="288,296 324,332 324,476 288,512 252,476 252,332" style="fill:#666; stroke: none;" variant="two" />

            <!-- Bottom-left bar -->
            <polygon points="54,296 90,332 90,476 54,512 18,476 18,332" style="fill:red; stroke: none;" variant="!(one or three or four or five or seven or nine)" />
            <polygon points="54,296 90,332 90,476 54,512 18,476 18,332" style="fill:#666; stroke: none;" variant="one or three or four or five or seven or nine" />

            <!-- Top bar -->
            <polygon points="279,55 243,91 99,91 63,55 99,19 243,19" style="fill:red; stroke: none;" variant="!(one or four)" />
            <polygon points="279,55 243,91 99,91 63,55 99,19 243,19" style="fill:#666; stroke: none;" variant="one or four" />

            <!-- top-right bar -->
            <polygon points="288,64 324,100 324,244 288,280 252,244 252,100" style="fill:red; stroke: none;" variant="!(five or six)" />
            <polygon points="288,64 324,100 324,244 288,280 252,244 252,100" style="fill:#666; stroke: none;" variant="five or six" />

            <!-- top-left bar -->
            <polygon points="54,64 90,100 90,244 54,280 18,244 18,100" style="fill:red; stroke: none;"  variant="!(one or two or three or seven)" />
            <polygon points="54,64 90,100 90,244 54,280 18,244 18,100" style="fill:#666; stroke: none;" variant="one or two or three or seven" />
        </g>
        <label x="0" y="-40" />
    </drawing>
.ends

* Some terminals, just to be fancy we give it some odd angle
SEGMENT1[a] <l 5> T("a")
SEGMENT1[b] <l 5> T("b")
SEGMENT1[c] <l 5> T("c")
SEGMENT1[d] <l 5> T("d")
SEGMENT1[e] <r 5> T("e")
SEGMENT1[f] <r 5> T("f")
SEGMENT1[g] <r 5> T("g")
SEGMENT1[h] <r 5> T("h")

SEGMENT2[a] <l 5> T("a")
SEGMENT2[b] <l 5> T("b")
SEGMENT2[c] <l 5> T("c")
SEGMENT2[d] <l 5> T("d")
SEGMENT2[e] <r 5> T("e")
SEGMENT2[f] <r 5> T("f")
SEGMENT2[g] <r 5> T("g")
SEGMENT2[h] <r 5> T("h")

SEGMENT1[plus] <d r +30 x r +30 u> [plus]SEGMENT2
X <d> T("+")

* Add a label and variants
SEGMENT1("label A", four)
SEGMENT2("label B", two)
```

## Non-inverting amplifier

The circuit on EEVblog's "I only give negative feedback" t-shirt.

```
* Input port
T("V_in") <r> [p]OA1

* Output port
OA1 <r> Xout <r> T("V_out")

* Connect the resistors
OA1[n] <l d> Xfb
Xfb <d> R("R_G") <d> GND
Xfb <r> R("R_F", flip) <r u> Xout
```

## Full bridge rectifier

The circuit on ElectroBOOM's "FULL BRIDGE RECTIFIER!!" t-shirt.

```
* Top diode
Tlt <r> Xlt <r> D1 <r> Xrt <r> Trt("+")

* Bottom diode
Tlb <r> Xlb <r> [n]D4[p] <r> Xrb <r> Trb("-")

* Cross diodes
Xlb <u r> D2 <r u> Xrt
Xrb <u l> D3 <l u> Xlt

* Horizontally align all diodes
(x D*)

* Alignment of some wires
(x Xlt <r 5> Xlb)

* Align the terminals
(x Tlt <d> Tlb)
(x Trt <d> Trb)
```

## Two-way light switch

A circuit for switching the same light with two switches.

```
* Make the main circuit
GND <u> V("AC", ac) <u r> SPDT1

* The switches
SPDT1(t1)[t1] <r +30> [t1]SPDT2(t2)
SPDT1[t2] <r> [t2]SPDT2(t2)

* To ground again
SPDT2[p] <r d> LIGHT <d> GND

* Control switches
SPDT1[c] <u> T("A", in)
SPDT2[c] <u> T("B", in)

* Align all anonymous grounds and terminals
(y GND)
(y T)
```

## CMOS inverter

A CMOS push-pull inverter.

```
* Main branch
GND <u> NMOS <u x u> PMOS <u> POW

* Output
X <r +20 x r> T(out, "V_out")
X <d> C("C_L") <d> GND

* Input
PMOS~1[g] <l d x d r> [g]NMOS~1
GND <u> V <u r> X

* Alignment
(y GND)
(y X~2 <r> X~1)
```

## 3T pixel array

A pixel array of 3T pixels. The array size can be changed using parameters. The center pixel has a different color.

```
* You can change the array size with these parameters
.param rows = 3
.param columns = 3

* Define a single pixel
.subckt PIXEL DIRleft DIRtop DIRbottom DIRright fg="--foreground"
    .property *|wire fg={fg}

    * Main branch
    GND <u> D(photodiode, flip) <u> Xd <u> MNrst <u> POW
    Xd <r> [g]MNsf[d] <u> POW

    * Inputs
    MNrst[g] <l> T("RST")
    MNsf[s] <d r> MNsel <r> Xcol
    MNsel[g] <u 60> Xrow

    * Make column and row lines (DIR is used as direction for pins)
    Xcol <u 70> DIRtop
    Xcol <d 15> DIRbottom
    Xrow <l 60> DIRleft
    Xrow <r 20> DIRright
.ends

* Now we will use for-loops to make an array of the pixel
.for r 1 {rows} 1
    .for c 1 {columns} 1
        Xh_{r}_{c} <r> PIXEL_{r}_{c} <r> Xh_{r}_{c+1}
        Xv_{r}_{c} <d> [DIRtop]PIXEL_{r}_{c}[DIRbottom] <d> Xv_{r+1}_{c}
    .endf
.endf

* Show the row driver
.for r 1 {rows} 1
    .param index = {r - round((rows + 1) / 2)}
    T(label1={"ROWSEL_in,y" + (index > 0 ? "+" + index : index == 0 ? "" : index)}, in) <r> Xh_{r}_1
.endf

* Show the column output
.for c 1 {columns} 1
    .param index = {c - round((columns + 1) / 2)}
    T(label1={"COL_out,x" + (index > 0 ? "+" + index : index == 0 ? "" : index)}, out) <u> Xv_{rows+1}_{c}
.endf

* Let's make the center pixel in the primary color
PIXEL_{round((rows + 1) / 2)}_{round((columns + 1) / 2)}(fg="--primary")

* Let's make the top-left pixel in danger color
PIXEL_1_1(fg="--danger")
```

## Full adder

The digital logic for a full adder.

```
.property Ti* in
.property To* out

* Inputs
Tia("A") <r> Xa <r> [a]XOR1
Tib("B") <r> Xb <r> [b]XOR1
Tic("Cin") <r> Xc <r> [b]XOR2

* First XOR
XOR1[o] <r se r> Xab <r> [a]XOR2
Xab <d r> [a]AND1

* XOR gate and two AND gates
XOR2[o] <r> Tos("S")
Xc <d r> [b]AND1
Xa <d r> [a]AND2
Xb <d r> [b]AND2

* Last OR gate for carry out
AND1[o] <r se r> [a]OR1
AND2[o] <r ne r> [b]OR1
OR1[o] <r> Toc("Cout")

* Align terminals using the wildcard '*'
(x Ti*)
(x To*)

* Other alignment
(x Xb <r 5> Xa)
(x Xc <r 5> Xab)
(xy XOR2 <d +15> AND1 <d +15> AND2)
```

## Transformers

A demonstration of some improvised transformers.

```
* Define a transformer
.subckt M DIRpa[i] DIRpb[o] DIRsa[i] DIRsb[o]
    DIRpa <d 0> L1(dot) <d 0> DIRpb
    DIRsa <d 0> L2(dot, flip) <d 0> DIRsb
    (xy L1 <r 10> L2)
.ends

* Primary side
V1 <u r d> [DIRpa]M1[DIRpb] <d l u> V1

* Secondary side to second transformer
M1[DIRsa] <u r d> [DIRpa]M2[DIRpb] <d l u> [DIRsb]M1

* Load
M2[DIRsa] <u r d> RL <d l u> [DIRsb]M2
```

## Latch

A latch, showing off some odd-angle wires.

```
* Horizontal chains
T(in) <r> Xia <r> NAND1 <r x r> T(out)
T(in) <r> Xib <r> [b]NAND2 <r x r> T(out)

* Cross-coupled wires
NAND2[a] <l u a 20 u> X
NAND1[b] <l d a -20 d> X
```

## CTIA / CFA / CSA

A Charge Transimpedance Amplifier (also known as Capacitive Feedback Amplifier or Charge Sense Amplifier).

```
* Input section
.section Input
    X1 <l d> I("I_in") <d> GND1
    X1 <d> C("C_in") <d> GND2
    (GND1 <r +20> GND2)
.endsection

* Link to the next section
Input/X1 <r> CTIA/Xin

* Charge Transimpedance Amplifier
.section CTIA
    Xin <r> A1("-A", anchor1=1) <r> Xout
    Xin <u r> C1("C_fb") <r d> Xout
.endsection

* Link CTIA to output circuit
CTIA/Xout <r> Output/Xout

* Output circuit
.section Output
    Xout <d> C1("C_L") <d> GND1
    Xout <r> Xout2 <d> R1("R_L") <d> GND2
    Xout2 <r> T("V_out")
    (GND1 <r +20> GND2)
.endsection

* We can still enforce alignment between elements
* This would not be possible with subcircuits
(y Input/GND1 <r> Output/GND1)
```

## Buck converter

The two states between which a buck converter toggles during operation.

```
* Check the style tab to see how the red lines were done
.section buck path=0
    .scope
        * On for path=0
        .property * fg={path == 0 ? "--primary" : "--foreground"}
        .property wire fg={path == 0 ? "--primary" : "--foreground"}
        GND1 <u> V1("V") <u r> S1(closed={path == 0}) <r> X1(fg="--primary")
    .ends

    .scope
        * On for path=1
        .property * fg={path == 1 ? "--primary" : "--foreground"}
        .property wire fg={path == 1 ? "--primary" : "--foreground"}
        GND2 <u> D1 <u> X1
    .ends

    .scope
        * Always on
        .property * fg="--primary"
        .property wire fg="--primary"
        X1 <r> L1("L", fg="--primary") <r> X2
        X2 <r arrow r d> Z1 <d> GND4
    .ends

    X2 <d> C1("C") <d> GND3
    (y GND*)
.ends

.section buck2 buck(path=1)
```

## Bit vector

A demonstration of how to use a bit vector component.

```
BIT1("A_2,A_1,A_0,D_3,D_2,D_1,D_0", separator=",", msbfirst)

.variants X -dot
.property wire ml=3

BIT1[b0] <d 5 r x r x r x r u> [b3]BIT1
BIT1[b1] <d> X
X <d r arrow> Xd("Data")
BIT1[b2] <d> X

BIT1[b4] <d 5 r x r u> [b6]BIT1
X <u> [b5]BIT1
X~1 <d r +20  arrow> Xa("Address")

(Xa <d> Xd)
```

## Modeling diagram — negative feedback

A simple modeling diagram for negative feedback.

```
T(in, "input") <r plus> ADD1
ADD1 <r> DIR("e") <r arrow> BLOCK("G") <r x r> T(out, "output")
X~1("y", angle=90) <d l arrow> BLOCK("&#946;") <l u arrow minus> ADD1
```

## Flowchart — Total Eclipse of the Heart

Demonstration of flowcharts using the song "Total Eclipse of the Heart" by Bonnie Tyler (Jeannr - Tumblr).

```
* Give all wires a nice curve
.property wire r = 2.5
.property X -dot

* Turn arouuuund...
FPta("Turn\naround")

* ... every now and then I ...
FPta <r d arrow> FP1("every now\nand then I")
FPta <d arrow> FP("bright eyes") <r a 80 arrow> FP1

* ... get a little bit ...
FP1 <r arrow> FP2("get a little bit" width=50 height=10)

* Lines
FP2 <d r arrow> FP("lonely and you're never coming 'round") <r u> X <u l d arrow> FPta
FP2 <d r arrow> FP("tired of listening to the sound of my tears") <r u> X~1
FP2 <d r arrow> FP("nervous that the best of all the years have gone by") <r u> X~1
FP2 <d r arrow> FP("terrified and then I see the look in your eyes") <r u> X~1

* ... fall apart ...
FP1 <d +50 arrow> FPfa("fall apart")
FPfa <d> FPny("and I\nneed you")

* This is a little hack to allow you to connect to different positions
FPny <a 30 0 r> FPnt("now,\ntonight") <d +0 l arrow a 150 0> FPny
FPny <d r arrow> FP("more than\never!!")

.property FP justify=0 r=1
.property FP* justify=0 r=1
.property FPta bg="rgb(200, 255, 200)"
.property FPny bg="rgb(200, 255, 255)"
```

## Flowchart — Engineering

Demonstration of flowcharts using the engineering flowchart.

```
* Overall styling
.property FT minheight=20
.property FT* minheight=20
.property FD rx=1 ry=3 minheight=20 bg="#fcc"
.property FTok* bg="#cfc"
.property wire r=3

* First terminal
FT("start") <d> FD("Does it move?")

* Top branch
FD~1 <l d> DIR("yes") <d arrow> FD
FD~2 <r d> DIR("no") <d arrow> FD

* Sub branch left
FD~2("Should it?")
FD~2 <l d> DIR("no") <d arrow> FTok1("No problem")
FD~2 <r d> DIR("yes") <d arrow> FTnok1("WD-40")

* Sub branch right
FD~1("Should it?")
FD~1 <l d> DIR("no") <d arrow> FTnok2("Duck tape")
FD~1 <r d> DIR("yes") <d arrow> FTok2("No problem")

(y FT*)
```

## Entity-relationship diagram

Demonstration of an entity-relationship diagram (ERD) for a simple sports system.

```
* Example for ERD diagrams

* General styling
.variant ENT*|ENT r=2
+ header-bg="--primary" header-fg="white"
+ odd-bg="#eeeeee" odd-fg="black" odd-fontsize=3
+ even-fontsize=3

* Define the tables
ENTplayers("Players",
+ "Player Id &#128273;",
+ "First name",
+ "Last name")

ENTgame("Games",
+ "Game Id &#128273;", 
+ "Player 1 Id &#8674;",
+ "Player 2 Id &#8674;",
+ "Score 1",
+ "Score 2",
+ "Score 3",
+ "Date")

ENTranking("Ranking",
+ "Ranking Id &#128273;",
+ "Player Id &#8674;",
+ "Date",
+ "Rank")

ENTtournament("Tournament",
+ "Tournament Id &#128273;"
+ "Name",
+ "Date")
ENTcompetition("Competition",
+ "Competition Id &#128273;"
+ "Name",
+ "StartDate")
ENTmeeting("Competition Meeting",
+ "Meeting Id &#128273;"
+ "Competition Id &#8674;",
+ "Date")

* Display the links
ENTgame <many-to-one r u r> ENTplayers
ENTplayers <one-to-many d r> ENTranking

ENTmeeting <many-to-many r d> ENTplayers
ENTcompetition <many-to-many r d> ENTplayers
ENTtournament <many-to-many l d> ENTplayers

ENTcompetition <one-to-many d +20> ENTmeeting

* There are a number of shorthand relationship cardinality:
* one-to-one = erd-only-one + erd-only-one
* one-to-zeroone = erd-only-one + erd-zero-one
* zeroone-to-one = erd-zero-one + erd-only-one
* one-to-onemany = erd-only-one + erd-one-many
* onemany-to-one = erd-one-many + erd-only-one
* one-to-many = erd-only-one + erd-zero-many
* many-to-one = erd-zero-many + erd-only-one
* zeroone-to-zeroone = erd-zero-one + erd-zero-one
* zeroone-to-onemany = erd-zero-one + erd-one-many
* onemany-to-zeroone = erd-one-many + erd-zero-one
* zeroone-to-many = erd-zero-one + erd-zero-many
* many-to-zeroone = erd-zero-many + erd-zero-one
* onemany-to-onemany = erd-one-many + erd-one-many
* onemany-to-many = erd-one-many + erd-zero-many
* many-to-onemany = erd-zero-many + erd-one-many
* many-to-many = erd-zero-many + erd-zero-many
```
