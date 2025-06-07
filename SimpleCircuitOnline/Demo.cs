namespace SimpleCircuitOnline
{
    /// <summary>
    /// A demo.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="Demo"/>.
    /// </remarks>
    /// <param name="title">The title of the demo.</param>
    /// <param name="description">The description of the demo.</param>
    /// <param name="code">The code of the demo.</param>
    public class Demo(string title, string description, string category, string code)
    {
        /// <summary>
        /// Gets the title of the demo.
        /// </summary>
        /// <value>
        /// The demo title.
        /// </value>
        public string Title => title;

        /// <summary>
        /// Gets the description of the demo.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description => description;

        /// <summary>
        /// Gets the category of the demo.
        /// </summary>
        public string Category => category;

        /// <summary>
        /// Gets the actual code of the demo.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; } = code;

        /// <summary>
        /// The demos.
        /// </summary>
        public static Demo[] Demos { get; } =
        [
            // Low-pass RC filter
            new Demo(
                "1. Component chains (basic)",
                "Tutorial explaining component chains.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* A component chain is a series of components seperated by <wires>.\r\n* The type of component is defined by the first letter(s), which have to be capital letters.\r\n* Wires can be defined between '<' and '>', using their direction: u, d, l, r for up, down, left or right.\r\n* Most components also have labels, which are specified as quoted strings between parenthesis.\r\nGND1 <u> V1(\"1V\") <u r> R(\"1k\") <r d> C1(\"1uF\") <d> GND2\r\n\r\n* Virtual chains act like component chains but are not drawn.\r\n* They can be used to align components.\r\n* Virtual chains are always between brackets.\r\n(GND1 <r> GND2)\r\n"
            ),

            // Tutorial about pins
            new Demo(
                "2. Pins (basic)",
                "Tutorial explaining pins.",
                "Tutorial",
                "* Pins are specified between square brackets\r\n* It is important whether the pin is specified before or after the component\r\nX1 <r> [g]NMOS1[s] <d> GND1\r\n\r\n* The pin order of the component is important\r\n* If no pin is specified, then the first pin is used as the default for wires ending in the component.\r\n* The last pin is used for wires starting from that component.\r\nNMOS1[d] <u> R1 <u> POW\r\n* You can find this information in Help > Components.\r\n\r\n* The resistor actually has 3 pins: 'p', 'c' and 'n' (each of them have aliases too). The 'c' pin however, if not used, is hidden.\r\nR1[c] <r> T(\"hello\")\r\n"
                ),

            new Demo(
                "3. Virtual chains / alignment (basic)",
                "Tutorial explaining virtual chains more.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* We define a number of component chains\r\nGND1 <u> V1 <u r> R <r> X1\r\nX1 <d> C <d> GND2\r\nX1 <r> X2 <d> L1 <d> R <d> GND3\r\nX2 <r> X3 <d> C <d> R <d> GND4\r\nX3 <r> T(\"output\")\r\n\r\n* We can now start aligning them using virtual chains\r\n* It is possible to align components only along one axis\r\n* This is done by adding \"x\" or \"y\" at the start of the virtual chain\r\n(x X1 <r +20> X2 <r +20> X3)\r\n\r\n* It is also possible to align components using a filter on their name\r\n* For example, we would like to align all the grounds in our circuit:\r\n(y GND*)\r\n* The '*' character acts as a wildcard\r\n\r\n* We can also align all anonymous components of a certain type in one statement.\r\n* We can for example align all anonymous capacitors:\r\n(y C)\r\n\r\n* Note that if no pins are specified explicitly, then virtual wires will use the center of the component unlike regular wires\r\n(y V1 <r> L1)\r\n"
                ),

            // Inverting amplifier
            new Demo(
                "4. Variants and properties (basic)",
                "Tutorial explaining variants for changing appearances.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Variants allow changing the appearance of certain components\r\n* For example, a resistor can have the \"programmable\" variant:\r\nT1(\"in\") <r> R1(programmable) <r> T2(\"out\")\r\n\r\n* Many components also have properties that can be specified as well\r\nR1(scale=2 zigs=7)\r\n\r\n* The property syntax can also be used to specify variants\r\nT1(input)\r\nT2(output)\r\n\r\n* Variants can be removed again by adding a '-' before them\r\nT2(-output, +pad)\r\n"
                ),

            // Wheatstone bridge
            new Demo(
                "5. Wires (basic)",
                "Tutorial explaining odd angle wires, and changing their appearance.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Wires do not have to be horizontal or vertical, SimpleCircuit can solve any angle\r\n* For shorthand notation, the 4 cardinal directions can be used\r\nX1 <n e s w> X1\r\n\r\n* In fact, the 4 ordinal directions can also be used!\r\nX2 <ne se sw nw> X2\r\n\r\n* It is possible to specify any angled wires by using <a #>\r\nX3 <a 60 r a -60 a -120 l a 120> X3\r\n* One note of caution: Using fine increments of angles can lead to rather unexpected results! Uncomment the next example to see what could happen:\r\n* X4 <e n a -91> X4\r\n* In such events, consider using unconstrained wires instead\r\n\r\n* We might just use <?> or '-' to copy the wire orientation from the pin it is connected to\r\nX <ne> R <?> R - R\r\n* Sometimes this could lead to errors though, for example:\r\n* X - R\r\n\r\n* Unconstrained wires are wires that do not constrain anything\r\n* While this may be useful for very odd angles, it also means that the components will need to be constrained in other ways.\r\n* They are specified using the <??> syntax:\r\nX7 <u> R <u r> C <r d ??> X7\r\n\r\n* Wires can also have special appearances:\r\nXt1 <d arrow d arrow> X\r\nXt2 <rarrow d arrow> X\r\nXt3 <arrow d> X\r\nXt4 <d rarrow> X\r\nXt5 <d one> X\r\nXt6 <d many> X\r\nXt7 <d onemany> X\r\nXt8 <d zeroone> X\r\nXt9 <d zeromany> X\r\nXt10 <d plus> X\r\nXt11 <d minus> X\r\nXt12 <d dashed> X\r\nXt13 <d dotted> X\r\n(y Xt*)\r\n"),

            // Section demo
            new Demo(
                "6. Sections (intermediate)",
                "Tutorial explaining sections.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Defining a section is achieved with the '.section' control statement\r\n.section A\r\n    GND1 <u> V1 <u r> TL <r> Xo\r\n    GND1(signal)\r\n.endsection\r\n\r\n* Elements inside sections can be referenced using a '/'\r\n* Names are local to the section, we can reuse 'GND1' for example\r\nA/Xo <r d> C <d> GND1\r\n(y A/GND1 <r> GND1)\r\n\r\n* You can re-use previously defined sections\r\n.section B A\r\nB/Xo <r d> L <d> GND2\r\n(y B/GND1 <r> GND2)\r\n\r\n.section C A\r\nC/Xo <r d> R <d> GND3\r\n(y C/GND1 <r> GND3)\r\n\r\n* We can align things that are not in sections\r\n(x GND*)\r\n\r\n* Or we can also align instances across sections\r\n(x */V1)\r\n"),

            // Subcircuit demo
            new Demo(
                "7. Subcircuits (intermediate)",
                "Tutorial explaining subcircuits",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Subcircuits are solved separately on their own, after which they act like a component\r\n* The pins need to be specified\r\n.subckt ABC DIR1[in] DIR2[out]\r\n    DIR1 <r> X1\r\n    X1 <u r> R1 <r d> X2\r\n    X1 <d r> C1 <r u> X2\r\n    X2 <r> DIR2\r\n.ends\r\n\r\n* Now we can instantiate this subcircuit definition multiple times.\r\nABC1 <r d> ABC <d> Xe <l> ABC <l u> ABC <u> Xs <r> ABC1\r\n\r\n* They can even be angled because our pins also have a direction!\r\n* Also showing how you can refer to pins\r\nXs <a -45> [DIR1_in]ABC[DIR2_out] <a -45 0> L <a -45> Xe\r\n"),

            // Black-box demo
            new Demo(
                "8. Black boxes (advanced)",
                "Tutorial on black boxes (custom pin components).",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Black boxes are components with custom pins:\r\n* - Pins are added on the fly\r\n* - Pin order is important\r\n* - Black boxes cannot rotate\r\n* - The pin orientation decides the side\r\n* - Pin locations only have minimum spacings\r\n\r\n* You can predefine the order and orientation\r\nBB1[Input1] <l>\r\nBB1[Input2] <l>\r\nBB1[Output1] <r>\r\nBB1[Output2] <r>\r\nBB1[VDD] <u> POW\r\nBB1[VSS] <d> GND\r\n\r\n* The distance between pins can vary, but they cannot change order\r\n* Notice how the two pins are spaced further apart because of the following statement\r\nBB1[Output1] <r d> R <d l> [Output2]BB1\r\n\r\n* The black box can stretch in any direction\r\n(x BB1[Input1] <r +80> [Output1]BB1)\r\n"),

            // Queued anonymous points
            new Demo(
                "9. Queued anonymous points (advanced)",
                "Tutorial on using queued anonymous points.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Queued anonymous points are anonymous points (X components) that are specified between wire segments.\r\n* You can specify them using an x or X in a wire.\r\nT(\"a\") <r x r> R <r x r> T(\"b\")\r\n\r\n* If you then use anonymous points in the next statement, it will first try to match them to the queued anonymous points of the previous chain.\r\nX <u r x r> C <r x r d> X\r\n\r\n* This is useful if you want to make parallel branches.\r\nX <u x r> L <r x d> X\r\nX <u r> S <r d> X\r\n"),

            // Custom symbols
            new Demo(
                "10. Custom symbols (advanced)",
                "Tutorial for making custom symbols using SVG-like XML.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* A symbol is custom definition of a component with SVG-like XML for drawing any shape\r\n* The following symbol defines a seven-segment display, based on the\r\n* SVG file from Wikipedia\r\n.symbol segment\r\n    <pins>\r\n        <pin name=\"a\" x=\"0\" y=\"3.625\" nx=\"-1\" />\r\n        <pin name=\"b\" x=\"0\" y=\"10.875\" nx=\"-1\" />\r\n        <pin name=\"c\" x=\"0\" y=\"18.125\" nx=\"-1\" />\r\n        <pin name=\"d\" x=\"0\" y=\"25.375\" nx=\"-1\" />\r\n        <pin name=\"plus\" x=\"8.75\" y=\"29\" ny=\"1\" />\r\n        <pin name=\"e\" x=\"17.5\" y=\"25.375\" nx=\"1\" />\r\n        <pin name=\"f\" x=\"17.5\" y=\"18.125\" nx=\"1\" />\r\n        <pin name=\"g\" x=\"17.5\" y=\"10.875\" nx=\"1\" />\r\n        <pin name=\"h\" x=\"17.5\" y=\"3.625\" nx=\"1\" />\r\n    </pins>\r\n    <!-- Note that the full SVG specification is not supported. -->\r\n    <!-- Only basic SVG XML is supported due to it needing to interface with SimpleCircuit. -->\r\n    <drawing scale=\"0.05\">\r\n        <rect x=\"0\" y=\"0\" width=\"350\" height=\"580\" style=\"fill: black;\" />\r\n    \t<g>\r\n            <!-- The \"variant\" attribute will control when a tag is drawn -->\r\n            <!-- Middle bar -->\r\n            <polygon points=\"279,288 243,324 99,324 63,288 99,252 243,252\" style=\"fill:red; stroke: none;\" variant=\"!(zero or one or seven)\" />\r\n            <polygon points=\"279,288 243,324 99,324 63,288 99,252 243,252\" style=\"fill:#666; stroke: none;\" variant=\"zero or one or seven\" />\r\n\r\n            <!-- Bottom bar -->\r\n            <polygon points=\"279,521 243,557 99,557 63,521 99,485 243,485\" style=\"fill:red; stroke: none;\" variant=\"!(one or four or seven)\" />\r\n            <polygon points=\"279,521 243,557 99,557 63,521 99,485 243,485\" style=\"fill:#666; stroke: none;\" variant=\"one or four or seven\" />\r\n\r\n            <!-- Bottom-right bar -->\r\n            <polygon points=\"288,296 324,332 324,476 288,512 252,476 252,332\" style=\"fill:red; stroke: none;\"  variant=\"!two\" />\r\n            <polygon points=\"288,296 324,332 324,476 288,512 252,476 252,332\" style=\"fill:#666; stroke: none;\" variant=\"two\" />\r\n\r\n            <!-- Bottom-left bar -->\r\n            <polygon points=\"54,296 90,332 90,476 54,512 18,476 18,332\" style=\"fill:red; stroke: none;\" variant=\"!(one or three or four or five or seven or nine)\" />\r\n            <polygon points=\"54,296 90,332 90,476 54,512 18,476 18,332\" style=\"fill:#666; stroke: none;\" variant=\"one or three or four or five or seven or nine\" />\r\n\r\n            <!-- Top bar -->\r\n            <polygon points=\"279,55 243,91 99,91 63,55 99,19 243,19\" style=\"fill:red; stroke: none;\" variant=\"!(one or four)\" />\r\n            <polygon points=\"279,55 243,91 99,91 63,55 99,19 243,19\" style=\"fill:#666; stroke: none;\" variant=\"one or four\" />\r\n\r\n            <!-- top-right bar -->\r\n            <polygon points=\"288,64 324,100 324,244 288,280 252,244 252,100\" style=\"fill:red; stroke: none;\" variant=\"!(five or six)\" />\r\n            <polygon points=\"288,64 324,100 324,244 288,280 252,244 252,100\" style=\"fill:#666; stroke: none;\" variant=\"five or six\" />\r\n\r\n            <!-- top-left bar -->\r\n            <polygon points=\"54,64 90,100 90,244 54,280 18,244 18,100\" style=\"fill:red; stroke: none;\"  variant=\"!(one or two or three or seven)\" />\r\n            <polygon points=\"54,64 90,100 90,244 54,280 18,244 18,100\" style=\"fill:#666; stroke: none;\" variant=\"one or two or three or seven\" />\r\n        </g>\r\n        <label x=\"0\" y=\"-40\" nx=\"1\" ny=\"-1\" />\r\n    </drawing>\r\n.ends\r\n\r\n* Some terminals, just to be fancy we give it some odd angle\r\nsegment1[a] <l> T(\"a\")\r\nsegment1[b] <l> T(\"b\")\r\nsegment1[c] <l> T(\"c\")\r\nsegment1[d] <l> T(\"d\")\r\nsegment1[e] <r> T(\"e\")\r\nsegment1[f] <r> T(\"f\")\r\nsegment1[g] <r> T(\"g\")\r\nsegment1[h] <r> T(\"h\")\r\n\r\nsegment2[a] <l> T(\"a\")\r\nsegment2[b] <l> T(\"b\")\r\nsegment2[c] <l> T(\"c\")\r\nsegment2[d] <l> T(\"d\")\r\nsegment2[e] <r> T(\"e\")\r\nsegment2[f] <r> T(\"f\")\r\nsegment2[g] <r> T(\"g\")\r\nsegment2[h] <r> T(\"h\")\r\n\r\nsegment1[plus] <d r +30 x r +30 u> [plus]segment2\r\nX <d> T(\"+\")\r\n\r\n* Add a label and variants\r\nsegment1(\"label A\", four)\r\nsegment2(\"label B\", two)\r\n"),

            // Annotation boxes
            new Demo(
                "11. Annotation boxes (advanced)",
                "Tutorial on using annotation boxes.",
                "Tutorial",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Annotations are boxes that will automatically be fitted\r\n* to their contents.\r\n* To start an annotation box, use two pipe '|' characters\r\n* and start with the name of the annotation box.\r\n|annotation1|\r\nT(\"in\") <r> [s]NMOS1[d] <r> T(\"out\")\r\nNMOS1[g] <u> T(\"c\")\r\n* The end of an annotation box is two pipe characters\r\n* without a name.\r\n||\r\n\r\n* It is possible to start and end annotation boxes inside\r\n* a component chain\r\nC1 |a| <r> R2 ||\r\n\r\n* You can also give extra parameters to annotation boxes.\r\n* Check the Wiki for a list of available options.\r\n|b \"B\" poly radius=2 margin=2 |\r\nR3 <ne r se> R4\r\n* As long as the contents between pipe '|' characters is\r\n* not starting with the name of an annotation, it is\r\n* treated as the end of the last opened annotation box.\r\n|-------------------------------|\r\n"),

            // Non-inverting amplifier
            new Demo(
                "Example: Non-inverting amplifier",
                "The circuit on EEVblog's \"I only give negative feedback\" t-shirt.",
                "Example",
                "* Make sure you specify the pin on the right side of the component name! This should not be OA1[p].\r\nT(\"V_in\") <r> [p]OA1\r\n\r\n* Resistive voltage divider:\r\nOA1[out] <r> Xout <d l> R(\"R_F\") <l> Xfb <d> R(\"R_G\") <d> GND\r\nXfb <u r> [n]OA1\r\n\r\nXout <r 5> T(\"V_out\")\r\n"),

            // Full bridge rectifier
            new Demo(
                "Example: Full bridge rectifier",
                "The circuit on ElectroBOOM's \"FULL BRIDGE RECTIFIER!!\" t-shirt.",
                "Example",
                "* Top diode\r\nTlt <r> Xlt <r> D1 <r> Xrt <r> Trt(\"+\")\r\n\r\n* Bottom diode\r\nTlb <r> Xlb <r> [n]D4[p] <r> Xrb <r> Trb(\"-\")\r\n\r\n* Cross diodes\r\nXlb <u r> D2 <r u> Xrt\r\nXrb <u l> D3 <l u> Xlt\r\n\r\n* Space the diodes apart for at least 15pt\r\n(y D1 <d +15> D2 <d +15> D3 <d +15> D4)\r\n\r\n* Alignment of some wires\r\n(x Xlt <r 5> Xlb)\r\n\r\n* Align the terminals\r\n(x Tlt <d> Tlb)\r\n(x Trt <d> Trb)\r\n"),

            // Double pole switch
            new Demo(
                "Example: Two-way light switch",
                "A circuit for switching a light using two switches.",
                "Example",
                "* Make the main circuit\r\nGND <u> V(\"AC\", ac) <u r> SPDT1[t1] <r> X1 <r> [t1]SPDT2[p] <r d> LIGHT1 <d> GND\r\nSPDT1(t1)[t2] <r> X2 <r> [t2]SPDT2(t2)\r\n\r\nSPDT1[c] <u> T(\"A\")\r\nSPDT2[c] <u> T(\"B\")\r\n\r\n* Align all anonymous grounds and terminals\r\n(y GND)\r\n(y T)\r\n"),

            // A simple push-pull CMOS inverter
            new Demo(
                "Example: CMOS inverter",
                "A CMOS push-pull inverter.",
                "Example",
                "* Define the push-pull branch.\r\nGND <u> NMOS1 <u> Xout <u> PMOS1 <u> POW\r\n\r\n* Define the gate structure\r\nPMOS1[g] <l d> Xin <d r> [g]NMOS1\r\n\r\n* Add some input signal\r\nGND <u> V1 <u r> Xin\r\n\r\n* Add some load\r\nXout <r d> CL(\"CL\") <d> GND\r\n\r\n* Some alignment\r\n(y GND)\r\n(y Xin <r> Xout)\r\n(x V1 <r +20> Xin)\r\n(x NMOS1 <r +20> CL)\r\n"),

            // Pixel array with 3-Transistor read-out
            new Demo(
                "Example: 3T Pixel array",
                "An array of CMOS 3T pixels. Also a demonstration of subcircuits.",
                "Tutorial",
                "* Define a single pixel\r\n.subckt PIXEL DIRleft DIRtop DIRbottom DIRright\r\n    * Reset\r\n    GND <u> D(photodiode) <u> Xd <u> MNrst <u> POW\r\n    MNrst[g] <l> T(\"RST\")\r\n    Xd <r> [g]MNsf[d] <u> POW\r\n    MNsf[s] <d r> MNsel <r> Xcol\r\n    MNsel[g] <u 60> Xrow\r\n\r\n    * Make column and row lines (DIR is used as direction for pins)\r\n    Xcol <u 70> DIRtop\r\n    Xcol <d 15> DIRbottom\r\n    Xrow <l 60> DIRleft\r\n    Xrow <r 20> DIRright\r\n.ends\r\n\r\n* We have chosen the subcircuit names such that they are unique,\r\n* allowing us to use the short names\r\nPIXEL1 <r> PIXEL2\r\nPIXEL1[DIRbottom] <d> [DIRtop]PIXEL3\r\nPIXEL3 <r> PIXEL4[DIRtop] <u> [DIRbottom]PIXEL2\r\n\r\n* Add some terminals\r\nPIXEL1[DIRtop] <u> T(\"COL_{OUT,k}\")\r\nPIXEL2[DIRtop] <u> T(\"COL_{OUT,k+1}\")\r\nPIXEL2[DIRright] <r> T(\"ROW_{SEL,i}\")\r\nPIXEL4[DIRright] <r> T(\"ROW_{SEL,i+1}\")\r\n"),

            // Full adder logic - showing off digital logic cells
            new Demo(
                "Example: Full adder logic",
                "A demonstration of the digital logic cells.",
                "Example",
                "* Inputs\r\nTia(\"A\") <r> Xa <r> [a]XOR1\r\nTib(\"B\") <r> Xb <r> [b]XOR1\r\nTic(\"Cin\") <r> Xc <r> [b]XOR2\r\n\r\n* First XOR\r\nXOR1[o] <r se r> Xab <r> [a]XOR2\r\nXab <d r> [a]AND1\r\n\r\n* XOR gate and two AND gates\r\nXOR2[o] <r> Tos(\"S\")\r\nXc <d r> [b]AND1\r\nXa <d r> [a]AND2\r\nXb <d r> [b]AND2\r\n\r\n* Last OR gate for carry out\r\nAND1[o] <r se r> [a]OR1\r\nAND2[o] <r ne r> [b]OR1\r\nOR1[o] <r> Toc(\"Cout\")\r\n\r\n* Align terminals using the wildcard '*'\r\n(x Ti*)\r\n(x To*)\r\n\r\n* Other alignment\r\n(x Xb <r 5> Xa)\r\n(x Xc <r 5> Xab)\r\n(xy XOR2 <d +15> AND1 <d +15> AND2)\r\n"),

            // Some transformers
            new Demo(
                "Example: Transformers",
                "A demonstration of some improvised transformers.",
                "Example",
                "* Define a transformer\r\n.subckt M DIRpa[i] DIRpb[o] DIRsa[i] DIRsb[o]\r\n    DIRpa <d 0> L1(dot) <d 0> DIRpb\r\n    DIRsa <d 0> L2(dot, flip) <d 0> DIRsb\r\n    (xy L1 <r 10> L2)\r\n.ends\r\n\r\n* Primary side\r\nV1 <u r d> [DIRpa]M1[DIRpb] <d l u> V1\r\n\r\n* Secondary side to second transformer\r\nM1[DIRsa] <u r d> [DIRpa]M2[DIRpb] <d l u> [DIRsb]M1\r\n\r\n* Load\r\nM2[DIRsa] <u r d> RL <d l u> [DIRsb]M2\r\n\r\n* Alignment\r\n(x V1 <r +25> M1 <r +25> M2 <r +25> RL)\r\n"),

            // A latch
            new Demo(
                "Example: Latch",
                "A latch, showing off odd-angle wires.",
                "Example",
                "* Horizontal chains\r\nT <r> Xia <r> NAND1 <r> Xoa <r> T\r\nT <r> Xib <r> NAND2 <r> Xob <r> T\r\n\r\n* The cross-coupled wires\r\nXoa <d a -160 d r> [b]NAND2(flip)\r\nXob <u a 160 u r> [b]NAND1\r\n"),

            // A charge transimpedance amplifier
            new Demo(
                "Example: CTIA",
                "A Charge Transimpedance Amplifier.",
                "Example",
                "* Input section\r\n.section Input\r\n    X1 <l d> I(\"I_in\") <d> GND1\r\n    X1 <d> C(\"C_in\") <d> GND2\r\n    (xy GND1 <r +25> GND2)\r\n.endsection\r\n\r\n* Link to the next section\r\nInput/X1 <r> CTIA/Xin\r\n\r\n* Charge Transimpedance Amplifier\r\n.section CTIA\r\n    Xin <r> A1(\"-A\", anchor1=1) <r> Xout\r\n    Xin <u r> C1(\"C_fb\") <r d> Xout\r\n    (y A1 <u +20> C1)\r\n.endsection\r\n\r\n* Link CTIA to output circuit\r\nCTIA/Xout <r> Output/Xout\r\n\r\n* Output circuit\r\n.section Output\r\n    Xout <d> C1(\"C_L\") <d> GND1\r\n    Xout <r> Xout2 <d> R1(\"R_L\") <d> GND2\r\n    Xout2 <r> T(\"V_out\")\r\n    (xy GND1 <r +20> GND2)\r\n.endsection\r\n\r\n* We can still enforce alignment between elements\r\n* This would not be possible with subcircuits\r\n(y Input/GND1 <r> Output/GND1)\r\n"),

            // CSS styling example
            new Demo(
                "Example: CSS styling",
                "Demonstration of how you can use CSS styling to change the circuit",
                "Example",
                "* Check the style tab to see how the red lines were done\r\n.section buck\r\n    GND1 <u> V1(\"V\") <u r> S1 <r> X1\r\n    GND2 <u> D1 <u> X1\r\n    X1 <r> L1(\"L\") <r> X2\r\n    X2 <d> C1(\"C\") <d> GND3\r\n    X2 <r arrow r> X3\r\n    X3 <d> Z1 <d> GND4\r\n    (y GND*)\r\n.ends\r\n\r\n.section buck2 buck\r\nbuck2/S1(closed)\r\n\r\n.css\r\n/* Styling section \"buck\" */\r\n#buck\\/w-4 *,\r\n#buck\\/D1 *,\r\n#buck\\/w-5 *,\r\n#buck\\/w-6 *,\r\n#buck\\/L1 path,\r\n#buck\\/w-7 *,\r\n#buck\\/w-10 *,\r\n#buck\\/w-11 path,\r\n#buck\\/Z1 polygon,\r\n#buck\\/w-12 path { stroke: red; }\r\n#buck\\/w-10 polygon { fill: red; }\r\n\r\n/* Styling section \"buck2\" */\r\n#buck2\\/w-1 *,\r\n#buck2\\/V1 *,\r\n#buck2\\/w-2 *,\r\n#buck2\\/L1 path,\r\n#buck2\\/w-3 *,\r\n#buck2\\/S1 *,\r\n#buck2\\/w-6 *,\r\n#buck2\\/w-7 *,\r\n#buck2\\/w-10 *,\r\n#buck2\\/w-11 *,\r\n#buck2\\/Z1 polygon,\r\n#buck2\\/w-12 * { stroke: red; }\r\n#buck2\\/w-10 polygon { fill: red; }\r\n#buck2\\/V1 tspan { stroke: none; }\r\n.endcss\r\n"),

            // Bit vector demo
            new Demo(
                "Example: Bit vector",
                "Demonstration on using the BIT component to display bit vectors.",
                "Example",
                "BIT1(\"A_2,A_1,A_0,D_3,D_2,D_1,D_0\" separator=',')\r\n\r\n* We just want lines, remove the dots on intersections\r\n.variants X* -dot\r\n\r\n* Let's temporarily reduce the minimum wire length to avoid warnings\r\n* .option minimumwirelength = 3\r\n.property wire ml=3\r\nBIT1[b3] <d r> X1\r\nBIT1[b2] <d 3> X1\r\nBIT1[b1] <d> X2\r\nBIT1[b0] <d l> X2\r\nX1 <r> Xdata <r> X2\r\n\r\nBIT1[b6] <d r> Xaddress\r\nBIT1[b5] <d 3> Xaddress\r\nBIT1[b4] <d l> Xaddress\r\n\r\n* Restore the minimum wire length\r\n\r\n* Show output text\r\nXdata <d r ml=10 arrow> Xdo(\"Data\")\r\nXaddress <d r ml=10 arrow> Xao(\"Address\")\r\n(xy Xdo <d> Xao)\r\n"),

            // Modeling demo
            new Demo(
                "Example: Modeling",
                "Demonstration on using the modeling block components.",
                "Example",
                "* Input to adder\r\nT(in, \"input\") <r arrow plus> ADD1\r\n\r\n* Adder to gain block\r\nADD1 <r> DIR(\"e\") <r arrow> BLOCK1(\"G\")\r\n\r\n* Gain block and output\r\nBLOCK1 <r> Xout(\"y\" angle=90) <r> T(out \"output\")\r\n\r\n* Feedback branch\r\n* Note that the direction of the wire determines\r\n* on which side the symbol is connected\r\nXout <d +20 l arrow> BLOCKfb(\"&#946;\") <l u arrow minus> ADD1\r\n"),

            // Song/flowchart demo
            new Demo(
                "Example: Flowchart",
                "Demonstration of flowcharts using the song \"Total Eclipse of the Heart\" by Bonnie Tyler (Jeannr - Tumblr)",
                "Example",
                "* Give all wires and process edges a nice curve\r\n.property wire r = 2.5\r\n.property FP r=1.5\r\n.property FP* r=1.5\r\n\r\n* Turn arouuuund...\r\nFPta(\"Turn\\naround\", bg=\"rgb(200,255,200)\")\r\n\r\n* ... every now and then I ...\r\nFPta <r d arrow> FP1(\"every now\\nand then I\")\r\nFPta <d arrow> FP(\"bright eyes\") <r arrow> FP1\r\n\r\n* ... get a little bit ...\r\nFP1 <r arrow> FP2(\"get a little bit\")\r\n\r\n* Lines\r\nFP2 <d +10 r arrow> FP(\"lonely and you're never coming 'round\") <r u> Xal1 <u l d arrow> FPta\r\nFP2 <d +25 r arrow> FP(\"tired of listening to the sound of my tears\") <r u> Xal2 <u l d arrow> FPta\r\nFP2 <d +40 r arrow> FP(\"nervous that the best of all the years have gone by\") <r u> Xal3 <u l d arrow> FPta\r\nFP2 <d +55 r arrow> FP(\"terrified and then I see the look in your eyes\") <r u> Xal4 <u l d arrow> FPta\r\n(x Xal*)\r\n\r\n* ... fall apart ...\r\nFP1 <d +50 arrow> FPfa(\"fall apart\")\r\nFPfa <d arrow> FPny(\"and I\\nneed you\", bg=\"rgb(200,200,255)\")\r\n\r\n* This is a little hack to allow you to connect to different positions on the same block\r\nFPny <a 30 0 r u r +30 d arrow> FPnt(\"now, tonight\") <l arrow a 150 0> FPny\r\nFPny <d +20 r arrow> FP(\"more than\\never!!\")\r\n"),

            // Engineering flowchart demo
            new Demo(
                "Example: Engineering flowchart",
                "Demonstration of flowcharts using the engineering flowchart.",
                "Example",
                "* Start and decision 1\r\nFT(\"start\") <d arrow> FD1(\"Does it move?\")\r\n\r\n* Decision 1 to decision 2\r\nFD1 <r d> DIR(\"yes\") <d arrow> FD2b(\"Should it?\")\r\nFD1 <l d> DIR(\"no\") <d arrow> FD2a(\"Should it?\")\r\n\r\n* Decision 2 to terminals\r\nFD2a <l d> DIR(\"no\") <d arrow> FTnpa(\"No problem\", bg=\"#cfc\")\r\nFD2a <r d> DIR(\"yes\") <d arrow> FTb(\"WD-40\", bg=\"#ccf\")\r\nFD2b <l d> DIR(\"no\") <d arrow> FTc(\"Duck tape\", bg=\"#ccf\")\r\nFD2b <r d> DIR(\"yes\") <d arrow> FTnpd(\"No problem\", bg=\"#cfc\")\r\n\r\n* Some alignment\r\n(x FTnpa <r +50> FTb <r +50> FTc <r +50> FTnpd)\r\n\r\n* Round wire corners and apply some fill colors\r\n.property wire r = 3\r\n.property FD* bg=\"#fcc\""),

            // All possible symbols
            new Demo(
                "Example: All built-in symbols (with variants)",
                "A list of all implemented symbols without any variants applied to them.",
                "Example",
                "* List of all native components\r\n\r\n* A\r\nA_1(\"a\", \"b\", \"c\")\r\nA_2(diffin, \"a\", \"b\", \"c\")\r\nA_3(diffout, \"a\", \"b\", \"c\")\r\nA_4(schmitt, \"a\", \"\", \"c\")\r\nA_5(comparator, \"a\", \"\", \"c\")\r\nA_6(programmable, \"a\")\r\n(y A_*)\r\nACT_1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\nADC_1(\"a\", \"b\")\r\nADC_2(diffin, \"a\", \"b\")\r\nADC_3(diffout, \"a\", \"b\")\r\n(y ADC_*)\r\nADD_1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\nADD_2(square, \"a\")\r\n(y ADD_*)\r\nAND_1(\"a\", \"b\")\r\nAND_2(euro)\r\n(y AND_*)\r\nANT_1(\"a\", \"b\")\r\nANT_2(\"a\", \"b\", alt)\r\n(y ANT_*)\r\nAPP_1(\"a\")\r\nAPP_2(heater, \"a\")\r\nAPP_3(heater, ventilator, \"a\")\r\nAPP_4(heater, accu, \"a\")\r\nAPP_5(ventilator, \"a\")\r\nAPP_6(boiler, \"a\")\r\nAPP_7(boiler, accu, \"a\")\r\nAPP_8(microwave, \"a\")\r\nAPP_9(oven, \"a\")\r\nAPP_10(washer, \"a\")\r\nAPP_11(dryer, \"a\")\r\nAPP_12(dishwasher, \"a\")\r\nAPP_13(fridge, \"a\")\r\nAPP_14(freezer, \"a\")\r\n(y APP_*)\r\nATTR_1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\n\r\n* B\r\nBAT_1(\"a\", \"b\")\r\nBB_1(\"a\")\r\nBIT_1(\"abc\", \"def\", \"ghij\")\r\nBLOCK_1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\", \"j\", \"k\", \"l\", \"m\", \"n\", \"o\", \"p\", \"q\", \"r\", \"s\", \"t\", \"u\", scale=2)\r\nBLOCK_2(\"label\")\r\n(y BLOCK_*)\r\nBUF_1(\"a\", \"b\")\r\nBUS_1(\"a\", \"b\")\r\nBUS_2(straight, \"a\", \"b\")\r\n(y BUS_*)\r\n\r\n* C\r\nC_1(\"a\", \"b\")\r\nC_2(curved, \"a\", \"b\")\r\nC_3(curved, signs, \"a\", \"b\")\r\nC_4(signs, \"a\", \"b\")\r\nC_5(electrolytic, \"a\", \"b\")\r\nC_6(electrolytic, signs, \"a\", \"b\")\r\nC_7(programmable, \"a\", \"b\")\r\nC_8(sensor, \"a\", \"b\")\r\n(y C_*)\r\nCB_1(\"a\", \"b\")\r\nCB_2(euro, \"a\", \"b\")\r\nCB_3(arei, \"a\", \"b\")\r\n(y CB_*)\r\nCIRC_1(\"a\", \"b\", \"c\")\r\nCIRC_2(square, \"a\", \"b\", \"c\")\r\n(y CIRC_*)\r\nCONN_1(\"a\", \"b\")\r\nCONN_2(american, \"a\", \"b\")\r\nCONN_3(american, male, \"a\")\r\nCONN_4(american, female, \"a\")\r\n(y CONN_*)\r\nCUT_1(\"a\", \"b\")\r\nCUT_2(straight, \"a\", \"b\")\r\n(y CUT_*)\r\n\r\n* D\r\nD_1(\"a\", \"b\")\r\nD_2(varactor, \"a\", \"b\")\r\nD_3(zener, \"a\", \"b\")\r\nD_4(tunnel, \"a\", \"b\")\r\nD_5(schockley, \"a\", \"b\")\r\nD_6(photodiode, \"a\", \"b\")\r\nD_7(laser, \"a\", \"b\")\r\nD_8(led, \"a\", \"b\")\r\nD_9(single, \"a\", \"b\")\r\nD_10(slanted, \"a\", \"b\")\r\nD_11(tvs, \"a\", \"b\")\r\nD_12(bidirectional, \"a\", \"b\")\r\nD_13(stroke, \"a\", \"b\")\r\n(y D_*)\r\nDIFF_1(\"a\")\r\nDIFF_2(sdomain, \"a\")\r\nDIFF_3(zdomain, \"a\")\r\n(y DIFF_*)\r\nDIR_1(\"a\", \"b\")\r\n\r\n* E\r\nE_1(\"a\", \"b\")\r\nE_2(euro, \"a\", \"b\")\r\n(y E_*)\r\nENT_1(\"a\", \"b\", \"c\")\r\n \r\n* F\r\nF_1(\"a\", \"b\")\r\nF_2(euro, \"a\", \"b\")\r\n(y F_*)\r\nFD_1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\nFDOC_1(\"a\")\r\nFDOC_2(multiple, \"a\")\r\n(y FDOC_*)\r\nFF_1(\"a\")\r\nFILT_1(\"a\")\r\nFILT_2(lowpass, \"a\")\r\nFILT_3(lowpass_2, \"a\")\r\nFILT_4(highpass, \"a\")\r\nFILT_5(highpass_2, \"a\")\r\nFILT_6(bandpass, \"a\")\r\nFILT_7(graph, \"a\")\r\nFILT_8(graph, highpass, \"a\")\r\nFILT_9(graph, bandpass, \"a\")\r\n(y FILT_*)\r\nFIO_1(\"a\")\r\nFP_1(\"a\")\r\nFT_1(\"a\")\r\nFUSE_1(\"a\", \"b\")\r\nFUSE_2(alt, \"a\", \"b\")\r\nFUSE_3(euro, \"a\", \"b\")\r\n(y FUSE_*)\r\n\r\n* G\r\nG_1(\"a\", \"b\")\r\nG_2(euro, \"a\", \"b\")\r\n(y G_*)\r\nGND_1(\"a\", \"b\")\r\nGND_2(earth, \"a\", \"b\")\r\nGND_3(chassis, \"a\", \"b\")\r\nGND_4(signal, \"a\", \"b\")\r\nGND_5(noiseless, \"a\", \"b\")\r\nGND_6(protective, \"a\", \"b\")\r\n(y GND_*)\r\n\r\n* H\r\nH_1(\"a\", \"b\")\r\nH_2(euro, \"a\", \"b\")\r\n(y H_*)\r\n\r\n* I\r\nI_1(\"a\", \"b\")\r\nI_2(ac, \"a\", \"b\")\r\nI_3(euro, \"a\", \"b\")\r\nI_4(programmable, \"a\", \"b\")\r\nI_5(euro, programmable, \"a\", \"b\")\r\n(y I_*)\r\nINT_1(\"a\")\r\nINT_2(sdomain, \"a\")\r\nINT_3(zdomain, \"a\")\r\n(y INT_*)\r\nINV_1(\"a\", \"b\")\r\nINV_2(euro, \"a\")\r\n(y INV_*)\r\n\r\n* J\r\nJACK_1(\"a\")\r\n(y JACK_*)\r\n\r\n* K\r\n\r\n* L\r\nL_1(\"a\", \"b\")\r\nLATCH_1(\"a\")\r\nLIGHT_1(\"a\", \"b\")\r\nLIGHT_2(arei, \"a\", \"b\")\r\nLIGHT_3(arei, wall, \"a\", \"b\")\r\nLIGHT_4(arei, projector, \"a\", \"b\")\r\nLIGHT_5(arei, direction, \"a\", \"b\")\r\nLIGHT_6(arei, direction, diverging, \"a\", \"b\")\r\nLIGHT_7(arei, emergency, \"a\", \"b\")\r\n(y LIGHT_*)\r\n\r\n* M\r\nMIC_1(\"a\", \"b\")\r\nMIX_1(\"a\")\r\nMIX_2(square, \"a\")\r\n(y MIX_*)\r\nMN_1(\"a\")\r\nMN_2(packaged, \"a\")\r\n(y MN_*)\r\nMOTOR_1(\"a\", \"b\")\r\nMP_1(\"a\")\r\nMP_2(packaged, \"a\")\r\n(y MP_*)\r\nMUX_1(\"a\")\r\n\r\n* N\r\nNAND_1(\"a\", \"b\")\r\nNAND_2(euro, \"a\")\r\n(y NAND_*)\r\nNMOS_1(\"a\")\r\nNMOS_2(packaged, \"a\")\r\n(y NMOS_*)\r\nNOR_1(\"a\", \"b\")\r\nNOR_2(euro, \"a\", \"b\")\r\n(y NOR_*)\r\nNOT_1(\"a\", \"b\")\r\nNOT_2(euro, \"a\", \"b\")\r\n(y NOT_*)\r\nNPN_1(\"a\")\r\nNPN_2(packaged, \"a\")\r\n(y NPN_*)\r\n\r\n* O\r\nOA_1(\"a\", \"b\", \"c\")\r\nOR_1(\"a\", \"b\")\r\nOR_2(euro, \"a\", \"b\")\r\n(y OR_*)\r\nOSC_1(\"a\", \"b\")\r\nOSC_2(square, \"a\")\r\n(y OSC_*)\r\nOTA_1(\"a\", \"b\", \"c\")\r\n\r\n* P\r\nPMOS_1(\"a\")\r\nPMOS_2(packaged, \"a\")\r\n(y PMOS_*)\r\nPNP_1(\"a\")\r\nPNP_2(packaged, \"a\")\r\n(y PNP_*)\r\nPOW_1(\"a\")\r\n\r\n* Q\r\nQN_1(\"a\")\r\nQN_2(packaged)\r\n(y QN_*)\r\nQP_1(\"a\")\r\nQP_2(packaged)\r\n(y QP_*)\r\n\r\n* R\r\nR_1(\"a\", \"b\")\r\nR_2(programmable, \"a\", \"b\")\r\nR_3(photo, \"a\", \"b\")\r\nR_4(thremistor, \"a\", \"b\")\r\nR_5(euro, \"a\", \"b\")\r\nR_6(euro, x, \"a\", \"b\")\r\nR_7(euro, memristor, \"a\", \"b\")\r\n(y R_*)\r\n\r\n* S\r\nS_1(\"a\", \"b\")\r\nS_2(closed, \"a\", \"b\")\r\nS_3(invert, \"a\", \"b\")\r\nS_4(closing, \"a\", \"b\")\r\nS_5(opening, \"a\", \"b\")\r\nS_6(reed, \"a\", \"b\")\r\nS_7(knife, \"a\", \"b\")\r\nS_8(knife, closed, \"a\", \"b\")\r\nS_9(push, \"a\", \"b\")\r\nS_10(push, invert, \"a\", \"b\")\r\nS_11(arei, \"a\", \"b\")\r\nS_12(arei, lamp, \"a\", \"b\")\r\nS_13(arei, push, \"a\", \"b\")\r\nS_14(arei, push, lamp, \"a\", \"b\")\r\nS_15(arei, push, window, \"a\", \"b\")\r\n(y S_*)\r\nSEG_1(\"a\", \"b\")\r\nSEG_2(underground, \"a\", \"b\")\r\nSEG_3(air, \"a\", \"b\")\r\nSEG_4(tube, \"a\", \"b\")\r\nSEG_5(tube, multiple=2, \"a\", \"b\")\r\nSEG_6(inwall, \"a\", \"b\")\r\nSEG_7(onwall, \"a\", \"b\")\r\n(y SEG_*)\r\nSGND_1(\"a\", \"b\")\r\nSPDT_1(\"a\")\r\nSPEAKER_1(\"a\")\r\nSPEAKER_2(off, \"a\")\r\n(y SPEAKER_*)\r\nSPLIT_1(\"a\", \"b\")\r\nSPLIT_2(-square, \"a\", \"b\")\r\n(y SPLIT_*)\r\nSUB_1(\"a\")\r\nSUB_2(square, \"a\")\r\n(y SUB_*)\r\n\r\n* T\r\nT_1(\"a\")\r\nT_2(in, \"a\")\r\nT_3(out, \"a\")\r\nT_4(inout, \"a\")\r\nT_5(other, \"a\")\r\nT_6(pad, \"a\")\r\nT_7(square, \"a\")\r\nT_8(none, \"a\")\r\n(y T_*)\r\nTA_1(\"a\", \"b\", \"c\")\r\nTL_1(\"a\", \"b\", \"c\")\r\n\r\n* U\r\n\r\n* V\r\nV_1(\"a\", \"b\")\r\nV_2(ac, \"a\", \"b\")\r\nV_3(square, \"a\", \"b\")\r\nV_4(tri, \"a\", \"b\")\r\nV_5(pulse, \"a\", \"b\")\r\nV_6(step, \"a\", \"b\")\r\nV_7(programmable, \"a\", \"b\")\r\nV_8(euro, \"a\", \"b\")\r\nV_9(euro, programmable, \"a\", \"b\")\r\n(y V_*)\r\n\r\n* W\r\nWP_1(\"a\")\r\nWP_2(earth, \"a\")\r\nWP_3(sealed, \"a\")\r\nWP_4(child, \"a\")\r\nWP_5(multiple=2)\r\n(y WP_*)\r\n\r\n* X\r\nX_1(\"a\")\r\nX_2(-dot, \"a\")\r\nX_3(forced, \"a\") <r> X\r\n(y X_*)\r\nXNOR_1(\"a\", \"b\")\r\nXNOR_2(euro, \"a\", \"b\")\r\n(y XNOR_*)\r\nXOR_1(\"a\", \"b\")\r\nXOR_2(euro, \"a\", \"b\")\r\n(y XOR_*)\r\nXTAL_1(\"a\", \"b\")\r\n\r\n* Y\r\nY_1(\"a\", \"b\")\r\nY_2(programmable, \"a\", \"b\")\r\n(y Y_*)\r\n\r\n* Z\r\nZ_1(\"a\", \"b\")\r\nZ_2(programmable, \"a\", \"b\")\r\n(y Z_*)\r\n")
        ];
    }
}
