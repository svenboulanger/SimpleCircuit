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
    public class Demo(string title, string description, string code)
    {
        /// <summary>
        /// Gets the title of the demo.
        /// </summary>
        /// <value>
        /// The demo title.
        /// </value>
        public string Title { get; } = title;

        /// <summary>
        /// Gets the description of the demo.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; } = description;

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
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* A component chain is a series of components seperated by <wires>.\r\n* The type of component is defined by the first letter(s), which have to be capital letters.\r\n* Wires can be defined between '<' and '>', using their direction: u, d, l, r for up, down, left or right.\r\n* Most components also have labels, which are specified as quoted strings between parenthesis.\r\nGND1 <u> V1(\"1V\") <u r> R(\"1k\") <r d> C1(\"1uF\") <d> GND2\r\n\r\n* Virtual chains act like component chains but are not drawn.\r\n* They can be used to align components.\r\n* Virtual chains are always between brackets.\r\n(GND1 <r> GND2)\r\n"
            ),

            // Tutorial about pins
            new Demo(
                "2. Pins (basic)",
                "Tutorial explaining pins.",
                "* Pins are specified between square brackets\r\n* It is important whether the pin is specified before or after the component\r\nX1 <r> [g]NMOS1[s] <d> GND1\r\n\r\n* The pin order of the component is important\r\n* If no pin is specified, then the first pin is used as the default for wires ending in the component.\r\n* The last pin is used for wires starting from that component.\r\nNMOS1[d] <u> R1 <u> POW\r\n* You can find this information in Help > Components.\r\n\r\n* The resistor actually has 3 pins: 'p', 'c' and 'n' (each of them have aliases too). The 'c' pin however, if not used, is hidden.\r\nR1[c] <r> T(\"hello\")\r\n"
                ),

            new Demo(
                "3. Virtual chains / alignment (basic)",
                "Tutorial explaining virtual chains more.",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* We define a number of component chains\r\nGND1 <u> V1 <u r> R <r> X1\r\nX1 <d> C <d> GND2\r\nX1 <r> X2 <d> L1 <d> R <d> GND3\r\nX2 <r> X3 <d> C <d> R <d> GND4\r\nX3 <r> T(\"output\")\r\n\r\n* We can now start aligning them using virtual chains\r\n* It is possible to align components only along one axis\r\n* This is done by adding \"x\" or \"y\" at the start of the virtual chain\r\n(x X1 <r +20> X2 <r +20> X3)\r\n\r\n* It is also possible to align components using a filter on their name\r\n* For example, we would like to align all the grounds in our circuit:\r\n(y GND*)\r\n* The '*' character acts as a wildcard\r\n\r\n* We can also align all anonymous components of a certain type in one statement.\r\n* We can for example align all anonymous capacitors:\r\n(y C)\r\n\r\n* Note that if no pins are specified explicitly, then virtual wires will use the center of the component unlike regular wires\r\n(y V1 <r> L1)\r\n"
                ),

            // Inverting amplifier
            new Demo(
                "4. Variants and properties (basic)",
                "Tutorial explaining variants for changing appearances.",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Variants allow changing the appearance of certain components\r\n* For example, a resistor can have the \"programmable\" variant:\r\nT1(\"in\") <r> R1(programmable) <r> T2(\"out\")\r\n\r\n* Many components also have properties that can be specified as well\r\nR1(scale=2 zigs=7)\r\n\r\n* The property syntax can also be used to specify variants\r\nT1(input)\r\nT2(output)\r\n\r\n* Variants can be removed again by adding a '-' before them\r\nT2(-output, +pad)\r\n"
                ),

            // Wheatstone bridge
            new Demo(
                "5. Wires (basic)",
                "Tutorial explaining odd angle wires, and changing their appearance.",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Wires do not have to be horizontal or vertical, SimpleCircuit can solve any angle\r\n* For shorthand notation, the 4 cardinal directions can be used\r\nX1 <n e s w> X1\r\n\r\n* In fact, the 4 ordinal directions can also be used!\r\nX2 <ne se sw nw> X2\r\n\r\n* It is possible to specify any angled wires by using <a #> as a wire segment with # the angle of the wire (counter-clockwise)\r\nX3 <a 60 r a -60 a -120 l a 120> X3\r\n* One note of caution: Using fine increments of angles can lead to rather unexpected results! Uncomment the next example to see what could happen:\r\n* X4 <e n a -91> X4\r\n* In such events, consider using unconstrained wires instead\r\n\r\n* We might just use <?> or '-' to copy the wire orientation from the pin it is connected to\r\nX5 <ne> R <?> R - R\r\n* Sometimes this could lead to errors though, for example:\r\n* X6 - R\r\n\r\n* Unconstrained wires are wires that do not constrain anything\r\n* While this may be useful for very odd angles, it also means that the components will need to be constrained in other ways.\r\n* They are specified using the <??> syntax:\r\nX7 <u> R <u r> C <r d ??> X7\r\n\r\n* Wires can also have special appearances:\r\nX8 <r arrow r arrow> X\r\nX9 <rarrow r arrow> X\r\nX10 <arrow r> X\r\nX11 <r rarrow> X\r\nX12 <r dashed> X\r\nX13 <r dotted> X\r\n"),

            // Section demo
            new Demo(
                "6. Sections (intermediate)",
                "Tutorial explaining sections.",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Defining a section is achieved with the '.section' control statement\r\n.section A\r\n    GND1 <u> V1 <u r> TL <r> Xo\r\n    GND1(signal)\r\n.endsection\r\n\r\n* Elements inside sections can be referenced using a '/'\r\n* Names are local to the section, we can reuse 'GND1' for example\r\nA/Xo <r d> C <d> GND1\r\n(y A/GND1 <r> GND1)\r\n\r\n* You can re-use previously defined sections\r\n.section B A\r\nB/Xo <r d> L <d> GND2\r\n(y B/GND1 <r> GND2)\r\n\r\n.section C A\r\nC/Xo <r d> R <d> GND3\r\n(y C/GND1 <r> GND3)\r\n\r\n* We can align things that are not in sections\r\n(x GND*)\r\n\r\n* Or we can also align instances across sections\r\n(x */V1)\r\n"),

            // Subcircuit demo
            new Demo(
                "7. Subcircuits (intermediate)",
                "Tutorial explaining subcircuits",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Subcircuits are solved separately on their own, after which they act like a component\r\n* The pins need to be specified\r\n.subckt ABC DIR1[in] DIR2[out]\r\n    DIR1 <r> X1\r\n    X1 <u r> R1 <r d> X2\r\n    X1 <d r> C1 <r u> X2\r\n    X2 <r> DIR2\r\n.ends\r\n\r\n* Now we can instantiate this subcircuit definition multiple times.\r\nABC1 <r d> ABC <d> Xe <l> ABC <l u> ABC <u> Xs <r> ABC1\r\n\r\n* They can even be angled because our pins also have a direction!\r\n* Also showing how you can refer to pins\r\nXs <a -45> [DIR1_in]ABC[DIR2_out] <a -45 0> L <a -45> Xe\r\n"),

            // Black-box demo
            new Demo(
                "8. Black boxes (advanced)",
                "Tutorial on black boxes (custom pin components).",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Black boxes are components with custom pins:\r\n* - Pins are added on the fly\r\n* - Pin order is important\r\n* - Black boxes cannot rotate\r\n* - The pin orientation decides the side\r\n* - Pin locations only have minimum spacings\r\n\r\n* You can predefine the order and orientation\r\nBB1[Input1] <l>\r\nBB1[Input2] <l>\r\nBB1[Output1] <r>\r\nBB1[Output2] <r>\r\nBB1[VDD] <u> POW\r\nBB1[VSS] <d> GND\r\n\r\n* The distance between pins can vary, but they cannot change order\r\n* Notice how the two pins are spaced further apart because of the following statement\r\nBB1[Output1] <r d> R <d l> [Output2]BB1\r\n\r\n* The black box can stretch in any direction\r\n(x BB1[Input1] <r +80> [Output1]BB1)\r\n"),

            // Queued anonymous points
            new Demo(
                "9. Queued anonymous points (advanced)",
                "Tutorial on using queued anonymous points.",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Queued anonymous points are anonymous points (X components) that are specified between wire segments.\r\n* You can specify them using an x or X in a wire.\r\nT(\"a\") <r x r> R <r x r> T(\"b\")\r\n\r\n* If you then use anonymous points in the next statement, it will first try to match them to the queued anonymous points of the previous chain.\r\nX <u r x r> C <r x r d> X\r\n\r\n* This is useful if you want to make parallel branches.\r\nX <u x r> L <r x d> X\r\nX <u r> S <r d> X\r\n"),

            // Custom symbols
            new Demo(
                "10. Custom symbols (advanced)",
                "Tutorial for making custom symbols using SVG-like XML.",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* A symbol is custom definition of a component with SVG-like XML for drawing any shape\r\n* The following symbol defines a seven-segment display, based on the\r\n* SVG file from Wikipedia\r\n.symbol segment\r\n    <pins>\r\n        <pin name=\"a\" x=\"0\" y=\"3.625\" nx=\"-1\" />\r\n        <pin name=\"b\" x=\"0\" y=\"10.875\" nx=\"-1\" />\r\n        <pin name=\"c\" x=\"0\" y=\"18.125\" nx=\"-1\" />\r\n        <pin name=\"d\" x=\"0\" y=\"25.375\" nx=\"-1\" />\r\n        <pin name=\"plus\" x=\"8.75\" y=\"29\" ny=\"1\" />\r\n        <pin name=\"e\" x=\"17.5\" y=\"25.375\" nx=\"1\" />\r\n        <pin name=\"f\" x=\"17.5\" y=\"18.125\" nx=\"1\" />\r\n        <pin name=\"g\" x=\"17.5\" y=\"10.875\" nx=\"1\" />\r\n        <pin name=\"h\" x=\"17.5\" y=\"3.625\" nx=\"1\" />\r\n    </pins>\r\n    <!-- Note that the full SVG specification is not supported. -->\r\n    <!-- Only basic SVG XML is supported due to it needing to interface with SimpleCircuit. -->\r\n    <drawing scale=\"0.05\">\r\n        <rect x=\"0\" y=\"0\" width=\"350\" height=\"580\" style=\"fill: black;\" />\r\n    \t<g>\r\n            <!-- The \"variant\" attribute will control when a tag is drawn -->\r\n            <!-- Middle bar -->\r\n            <polygon points=\"279,288 243,324 99,324 63,288 99,252 243,252\" style=\"fill:red; stroke: none;\" variant=\"!(zero or one or seven)\" />\r\n            <polygon points=\"279,288 243,324 99,324 63,288 99,252 243,252\" style=\"fill:#666; stroke: none;\" variant=\"zero or one or seven\" />\r\n\r\n            <!-- Bottom bar -->\r\n            <polygon points=\"279,521 243,557 99,557 63,521 99,485 243,485\" style=\"fill:red; stroke: none;\" variant=\"!(one or four or seven)\" />\r\n            <polygon points=\"279,521 243,557 99,557 63,521 99,485 243,485\" style=\"fill:#666; stroke: none;\" variant=\"one or four or seven\" />\r\n\r\n            <!-- Bottom-right bar -->\r\n            <polygon points=\"288,296 324,332 324,476 288,512 252,476 252,332\" style=\"fill:red; stroke: none;\"  variant=\"!two\" />\r\n            <polygon points=\"288,296 324,332 324,476 288,512 252,476 252,332\" style=\"fill:#666; stroke: none;\" variant=\"two\" />\r\n\r\n            <!-- Bottom-left bar -->\r\n            <polygon points=\"54,296 90,332 90,476 54,512 18,476 18,332\" style=\"fill:red; stroke: none;\" variant=\"!(one or three or four or five or seven or nine)\" />\r\n            <polygon points=\"54,296 90,332 90,476 54,512 18,476 18,332\" style=\"fill:#666; stroke: none;\" variant=\"one or three or four or five or seven or nine\" />\r\n\r\n            <!-- Top bar -->\r\n            <polygon points=\"279,55 243,91 99,91 63,55 99,19 243,19\" style=\"fill:red; stroke: none;\" variant=\"!(one or four)\" />\r\n            <polygon points=\"279,55 243,91 99,91 63,55 99,19 243,19\" style=\"fill:#666; stroke: none;\" variant=\"one or four\" />\r\n\r\n            <!-- top-right bar -->\r\n            <polygon points=\"288,64 324,100 324,244 288,280 252,244 252,100\" style=\"fill:red; stroke: none;\" variant=\"!(five or six)\" />\r\n            <polygon points=\"288,64 324,100 324,244 288,280 252,244 252,100\" style=\"fill:#666; stroke: none;\" variant=\"five or six\" />\r\n\r\n            <!-- top-left bar -->\r\n            <polygon points=\"54,64 90,100 90,244 54,280 18,244 18,100\" style=\"fill:red; stroke: none;\"  variant=\"!(one or two or three or seven)\" />\r\n            <polygon points=\"54,64 90,100 90,244 54,280 18,244 18,100\" style=\"fill:#666; stroke: none;\" variant=\"one or two or three or seven\" />\r\n        </g>\r\n        <label x=\"0\" y=\"-40\" nx=\"1\" ny=\"-1\" />\r\n    </drawing>\r\n.ends\r\n\r\n* Some terminals, just to be fancy we give it some odd angle\r\nsegment1[a] <l> T(\"a\")\r\nsegment1[b] <l> T(\"b\")\r\nsegment1[c] <l> T(\"c\")\r\nsegment1[d] <l> T(\"d\")\r\nsegment1[e] <r> T(\"e\")\r\nsegment1[f] <r> T(\"f\")\r\nsegment1[g] <r> T(\"g\")\r\nsegment1[h] <r> T(\"h\")\r\n\r\nsegment2[a] <l> T(\"a\")\r\nsegment2[b] <l> T(\"b\")\r\nsegment2[c] <l> T(\"c\")\r\nsegment2[d] <l> T(\"d\")\r\nsegment2[e] <r> T(\"e\")\r\nsegment2[f] <r> T(\"f\")\r\nsegment2[g] <r> T(\"g\")\r\nsegment2[h] <r> T(\"h\")\r\n\r\nsegment1[plus] <d r +30 x r +30 u> [plus]segment2\r\nX <d> T(\"+\")\r\n\r\n* Add a label and variants\r\nsegment1(\"label A\", four)\r\nsegment2(\"label B\", two)\r\n"),

            // Annotation boxes
            new Demo(
                "11. Annotation boxes (advanced)",
                "Tutorial on using annotation boxes.",
                "* For more tutorials, go to Help > Demo's.\r\n\r\n* Annotations are boxes that will automatically be fitted\r\n* to their contents.\r\n* To start an annotation box, use two pipe '|' characters\r\n* and start with the name of the annotation box.\r\n|annotation1|\r\nT(\"in\") <r> [s]NMOS1[d] <r> T(\"out\")\r\nNMOS1[g] <u> T(\"c\")\r\n* The end of an annotation box is two pipe characters\r\n* without a name.\r\n||\r\n\r\n* It is possible to start and end annotation boxes inside\r\n* a component chain\r\nC1 |a| <r> R2 ||\r\n\r\n* You can also give extra parameters to annotation boxes.\r\n* Check the Wiki for a list of available options.\r\n|b \"B\" poly radius=2 margin=2 |\r\nR3 <ne r se> R4\r\n* As long as the contents between pipe '|' characters is\r\n* not starting with the name of an annotation, it is\r\n* treated as the end of the last opened annotation box.\r\n|-------------------------------|\r\n"),

            // Non-inverting amplifier
            new Demo(
                "Example: Non-inverting amplifier",
                "The circuit on EEVblog's \"I only give negative feedback\" t-shirt.",
                "* Make sure you specify the pin on the right side of the component name! This should not be OA1[p].\r\nT(\"V_in\") <r> [p]OA1\r\n\r\n* Resistive voltage divider:\r\nOA1[out] <r> Xout <d l> R(\"R_F\") <l> Xfb <d> R(\"R_G\") <d> GND\r\nXfb <u r> [n]OA1\r\n\r\nXout <r 5> T(\"V_out\")\r\n"),

            // Full bridge rectifier
            new Demo(
                "Example: Full bridge rectifier",
                "The circuit on ElectroBOOM's \"FULL BRIDGE RECTIFIER!!\" t-shirt.",
                "* Top diode\r\nTlt <r> Xlt <r> D1 <r> Xrt <r> Trt(\"+\")\r\n\r\n* Bottom diode\r\nTlb <r> Xlb <r> [n]D4[p] <r> Xrb <r> Trb(\"-\")\r\n\r\n* Cross diodes\r\nXlb <u r> D2 <r u> Xrt\r\nXrb <u l> D3 <l u> Xlt\r\n\r\n* Space the diodes apart for at least 15pt\r\n(y D1 <d +15> D2 <d +15> D3 <d +15> D4)\r\n\r\n* Alignment of some wires\r\n(x Xlt <r 5> Xlb)\r\n\r\n* Align the terminals\r\n(x Tlt <d> Tlb)\r\n(x Trt <d> Trb)\r\n"),

            // Double pole switch
            new Demo(
                "Example: Two-way light switch",
                "A circuit for switching a light using two switches.",
                "* Make the main circuit\r\nGND <u> V(\"AC\", ac) <u r> SPDT1[t1] <r> X1 <r> [t1]SPDT2[p] <r d> LIGHT1 <d> GND\r\nSPDT1(t1)[t2] <r> X2 <r> [t2]SPDT2(t2)\r\n\r\nSPDT1[c] <u> T(\"A\")\r\nSPDT2[c] <u> T(\"B\")\r\n\r\n* Align all anonymous grounds and terminals\r\n(y GND)\r\n(y T)\r\n"),

            // A simple push-pull CMOS inverter
            new Demo(
                "Example: CMOS inverter",
                "A CMOS push-pull inverter.",
                "* Define the push-pull branch.\r\nGND <u> NMOS1 <u> Xout <u> PMOS1 <u> POW\r\n\r\n* Define the gate structure\r\nPMOS1[g] <l d> Xin <d r> [g]NMOS1\r\n\r\n* Add some input signal\r\nGND <u> V1 <u r> Xin\r\n\r\n* Add some load\r\nXout <r d> CL(\"CL\") <d> GND\r\n\r\n* Some alignment\r\n(y GND)\r\n(y Xin <r> Xout)\r\n(x V1 <r +20> Xin)\r\n(x NMOS1 <r +20> CL)\r\n"),

            // Pixel array with 3-Transistor read-out
            new Demo(
                "Example: 3T Pixel array",
                "An array of CMOS 3T pixels. Also a demonstration of subcircuits.",
                "* Define a single pixel\r\n.subckt PIXEL DIRleft DIRtop DIRbottom DIRright\r\n    * Reset\r\n    GND <u> D(photodiode) <u> Xd <u> MNrst <u> POW\r\n    MNrst[g] <l> T(\"RST\")\r\n    Xd <r> [g]MNsf[d] <u> POW\r\n    MNsf[s] <d r> MNsel <r> Xcol\r\n    MNsel[g] <u 60> Xrow\r\n\r\n    * Make column and row lines (DIR is used as direction for pins)\r\n    Xcol <u 70> DIRtop\r\n    Xcol <d 15> DIRbottom\r\n    Xrow <l 60> DIRleft\r\n    Xrow <r 20> DIRright\r\n.ends\r\n\r\n* We have chosen the subcircuit names such that they are unique,\r\n* allowing us to use the short names\r\nPIXEL1 <r> PIXEL2\r\nPIXEL1[DIRbottom] <d> [DIRtop]PIXEL3\r\nPIXEL3 <r> PIXEL4[DIRtop] <u> [DIRbottom]PIXEL2\r\n\r\n* Add some terminals\r\nPIXEL1[DIRtop] <u> T(\"COL_{OUT,k}\")\r\nPIXEL2[DIRtop] <u> T(\"COL_{OUT,k+1}\")\r\nPIXEL2[DIRright] <r> T(\"ROW_{SEL,i}\")\r\nPIXEL4[DIRright] <r> T(\"ROW_{SEL,i+1}\")\r\n"),

            // Full adder logic - showing off digital logic cells
            new Demo(
                "Example: Full adder logic",
                "A demonstration of the digital logic cells.",
                "* Inputs\r\nTia(\"A\") <r> Xa <r> [a]XOR1\r\nTib(\"B\") <r> Xb <r> [b]XOR1\r\nTic(\"Cin\") <r> Xc <r> [b]XOR2\r\n\r\n* First XOR\r\nXOR1[o] <r se r> Xab <r> [a]XOR2\r\nXab <d r> [a]AND1\r\n\r\n* XOR gate and two AND gates\r\nXOR2[o] <r> Tos(\"S\")\r\nXc <d r> [b]AND1\r\nXa <d r> [a]AND2\r\nXb <d r> [b]AND2\r\n\r\n* Last OR gate for carry out\r\nAND1[o] <r se r> [a]OR1\r\nAND2[o] <r ne r> [b]OR1\r\nOR1[o] <r> Toc(\"Cout\")\r\n\r\n* Align terminals using the wildcard '*'\r\n(x Ti*)\r\n(x To*)\r\n\r\n* Other alignment\r\n(x Xb <r 5> Xa)\r\n(x Xc <r 5> Xab)\r\n(xy XOR2 <d +15> AND1 <d +15> AND2)\r\n"),

            // Some transformers
            new Demo(
                "Example: Transformers",
                "A demonstration of some improvised transformers.",
                "* Define a transformer\r\n.subckt M DIRpa[i] DIRpb[o] DIRsa[i] DIRsb[o]\r\n    DIRpa <d 0> L1(dot) <d 0> DIRpb\r\n    DIRsa <d 0> L2(dot, flip) <d 0> DIRsb\r\n    (xy L1 <r 10> L2)\r\n.ends\r\n\r\n* Primary side\r\nV1 <u r d> [DIRpa]M1[DIRpb] <d l u> V1\r\n\r\n* Secondary side to second transformer\r\nM1[DIRsa] <u r d> [DIRpa]M2[DIRpb] <d l u> [DIRsb]M1\r\n\r\n* Load\r\nM2[DIRsa] <u r d> RL <d l u> [DIRsb]M2\r\n\r\n* Alignment\r\n(x V1 <r +25> M1 <r +25> M2 <r +25> RL)\r\n"),

            // A latch
            new Demo(
                "Example: Latch",
                "A latch, showing off odd-angle wires.",
                "* Horizontal chains\r\nT <r> Xia <r> NAND1 <r> Xoa <r> T\r\nT <r> Xib <r> NAND2 <r> Xob <r> T\r\n\r\n* The cross-coupled wires\r\nNAND1[b] <l d ?? d> Xob\r\nXoa <d ?? d r> [b]NAND2(flip)\r\n\r\n* Align the NAND gates\r\n(NAND1 <d +30> NAND2)\r\n"),

            // A charge transimpedance amplifier
            new Demo(
                "Example: CTIA",
                "A Charge Transimpedance Amplifier.",
                "* Input section\r\n.section Input\r\n    X1 <l d> I(\"I_in\") <d> GND1\r\n    X1 <d> C(\"C_in\") <d> GND2\r\n    (xy GND1 <r +25> GND2)\r\n.endsection\r\n\r\n* Link to the next section\r\nInput/X1 <r> CTIA/Xin\r\n\r\n* Charge Transimpedance Amplifier\r\n.section CTIA\r\n    Xin <r> A1(\"-A\", anchor1=1) <r> Xout\r\n    Xin <u r> C1(\"C_fb\") <r d> Xout\r\n    (y A1 <u +20> C1)\r\n.endsection\r\n\r\n* Link CTIA to output circuit\r\nCTIA/Xout <r> Output/Xout\r\n\r\n* Output circuit\r\n.section Output\r\n    Xout <d> C1(\"C_L\") <d> GND1\r\n    Xout <r> Xout2 <d> R1(\"R_L\") <d> GND2\r\n    Xout2 <r> T(\"V_out\")\r\n    (xy GND1 <r +20> GND2)\r\n.endsection\r\n\r\n* We can still enforce alignment between elements\r\n* This would not be possible with subcircuits\r\n(y Input/GND1 <r> Output/GND1)\r\n"),

            // CSS styling example
            new Demo(
                "Example: CSS styling",
                "Demonstration of how you can use CSS styling to change the circuit",
                "* Check the style tab to see how the red lines were done\r\n.section buck\r\n    GND1 <u> V1(\"V\") <u r> S1 <r> X1\r\n    GND2 <u> D1 <u> X1\r\n    X1 <r> L1(\"L\") <r> X2\r\n    X2 <d> C1(\"C\") <d> GND3\r\n    X2 <r arrow r> X3\r\n    X3 <d> Z1 <d> GND4\r\n    (y GND*)\r\n.ends\r\n\r\n.section buck2 buck\r\nbuck2/S1(closed)\r\n\r\n.css\r\n/* Styling section \"buck\" */\r\n#buck\\/w-4 *,\r\n#buck\\/D1 *,\r\n#buck\\/w-5 *,\r\n#buck\\/w-6 *,\r\n#buck\\/L1 path,\r\n#buck\\/w-7 *,\r\n#buck\\/w-10 *,\r\n#buck\\/w-11 path,\r\n#buck\\/Z1 polygon,\r\n#buck\\/w-12 path { stroke: red; }\r\n#buck\\/w-10 polygon { fill: red; }\r\n\r\n/* Styling section \"buck2\" */\r\n#buck2\\/w-1 *,\r\n#buck2\\/V1 *,\r\n#buck2\\/w-2 *,\r\n#buck2\\/L1 path,\r\n#buck2\\/w-3 *,\r\n#buck2\\/S1 *,\r\n#buck2\\/w-6 *,\r\n#buck2\\/w-7 *,\r\n#buck2\\/w-10 *,\r\n#buck2\\/w-11 *,\r\n#buck2\\/Z1 polygon,\r\n#buck2\\/w-12 * { stroke: red; }\r\n#buck2\\/w-10 polygon { fill: red; }\r\n#buck2\\/V1 tspan { stroke: none; }\r\n.endcss\r\n"),

            // Bit vector demo
            new Demo(
                "Example: Bit vector",
                "Demonstration on using the BIT component to display bit vectors.",
                "BIT1(\"A_2,A_1,A_0,D_3,D_2,D_1,D_0\" separator=',')\r\n\r\n* We just want lines, remove the dots on intersections\r\n.variants X -dot\r\n\r\n* Let's temporarily reduce the minimum wire length to avoid warnings\r\n.option minimumwirelength = 3\r\nBIT1[b3] <d r> X1\r\nBIT1[b2] <d 3> X1\r\nBIT1[b1] <d> X2\r\nBIT1[b0] <d l> X2\r\nX1 <r> Xdata <r> X2\r\n\r\nBIT1[b6] <d r> Xaddress\r\nBIT1[b5] <d 3> Xaddress\r\nBIT1[b4] <d l> Xaddress\r\n\r\n* Restore the minimum wire length\r\n.option minimumwirelength = 10\r\n\r\n* Show output text\r\nXdata <d r arrow> Xdo(\"Data\")\r\nXaddress <d r arrow> Xao(\"Address\")\r\n(Xdo <d> Xao)\r\n"),

            // Modeling demo
            new Demo(
                "Example: Modeling",
                "Demonstration on using the modeling block components.",
                "* Input to adder\r\nT(in, \"input\") <r arrow plus> ADD1\r\n\r\n* Adder to gain block\r\nADD1 <r> DIR(\"e\") <r arrow> BLOCK1(\"G\")\r\n\r\n* Gain block and output\r\nBLOCK1 <r> Xout(\"y\" angle=90) <r> T(out \"output\")\r\n\r\n* Feedback branch\r\n* Note that the direction of the wire determines\r\n* on which side the symbol is connected\r\nXout <d +20 l arrow> BLOCKfb(\"&#946;\") <l u arrow minus> ADD1\r\n"),

            // Song/flowchart demo
            new Demo(
                "Example: Flowchart",
                "Demonstration of flowcharts using the song \"Total Eclipse of the Heart\" by Bonnie Tyler (Jeannr - Tumblr)",
                "* Give all wires a nice curve\r\n.property wire r = 2.5\r\n\r\n* Turn arouuuund...\r\nFPta(\"Turn\\naround\")\r\n\r\n* ... every now and then I ...\r\nFPta <r d arrow> FP1(\"every now\\nand then I\")\r\nFPta <d arrow> FP(\"bright eyes\") <r a 80 +30 arrow> FP1\r\n\r\n* ... get a little bit ...\r\nFP1 <r arrow> FP2(\"get a little bit\" width=50 height=10)\r\n\r\n* Lines\r\n.section line1\r\n    FP1(\"lonely and you're never coming 'round\" width=140 height=10)\r\n.ends\r\n.section line2 line1\r\nline2/FP1(\"tired of listening to the sound of my tears\")\r\n.section line3 line1\r\nline3/FP1(\"nervous that the best of all the years have gone by\")\r\n.section line4 line1\r\nline4/FP1(\"terrified and then I see the look in your eyes\")\r\nFP2 <d +10 r arrow> line1/FP1 <r u l d arrow> FPta\r\nFP2 <d +25 r arrow> line2/FP1 <r u l d arrow> FPta\r\nFP2 <d +40 r arrow> line3/FP1 <r u l d arrow> FPta\r\nFP2 <d +55 r arrow> line4/FP1 <r u l d arrow> FPta\r\n\r\n* ... fall apart ...\r\nFP1 <d +50 arrow> FPfa(\"fall apart\")\r\nFPfa <d> FPny(\"and I\\nneed you\")\r\n\r\n* This is a little hack to allow you to connect to different positions\r\nFPny <a 30 0 r> FPnt(\"now,\\ntonight\") <d +0 l arrow a 150 0> FPny\r\nFPny <d r arrow> FP(\"more than\\never!!\")\r\n\r\n.css\r\n#FPta polygon { fill: rgb(200, 255, 200); }\r\n#FPny polygon { fill: rgb(200, 200, 255); }\r\n.endcss\r\n"),

            // Engineering flowchart demo
            new Demo(
                "Example: Engineering flowchart",
                "Demonstration of flowcharts using the engineering flowchart.",
                "* Give some nice rounding to the flowchart\r\n.property FD rx = 1 ry = 3\r\n.property wire r = 3\r\n\r\n* Start and decision 1\r\nFT(\"start\") <d> FD1(\"Does it move?\", w=50)\r\n\r\n* Decision 1 to decision 2\r\nFD1 <r d> DIR(\"yes\") <d arrow> FD2b(\"Should it?\")\r\nFD1 <l d> DIR(\"no\") <d arrow> FD2a(\"Should it?\")\r\n\r\n* Decision 2 to terminals\r\nFD2a <l d> DIR(\"no\") <d arrow> FTa(\"No problem\", np)\r\nFD2a <r d> DIR(\"yes\") <d arrow> FTb(\"WD-40\")\r\nFD2b <l d> DIR(\"no\") <d arrow> FTc(\"Duck tape\")\r\nFD2b <r d> DIR(\"yes\") <d arrow> FTd(\"No problem\", np)\r\n\r\n* Some alignment\r\n(x FTa <r +50> FTb <r +50> FTc <r +50> FTd)\r\n\r\n.css\r\n.decision path { fill: #fcc; }\r\n.np path { fill: #cfc; }\r\n.endcss\r\n"),

            // All possible symbols
            new Demo(
                "Example: All built-in symbols (with variants)",
                "A list of all implemented symbols without any variants applied to them.",
                "* List of all native components\r\n.options minimumwirelength = 30\r\n\r\n* A\r\nA1(\"a\", \"b\", \"c\")\r\nA2(diffin, \"a\", \"b\", \"c\")\r\nA3(diffout, \"a\", \"b\", \"c\")\r\nA4(schmitt, \"a\", \"\", \"c\")\r\nA5(comparator, \"a\", \"\", \"c\")\r\nA6(programmable, \"a\")\r\n(xy A1 <r> A2 <r> A3 <r> A4 <r> A5 <r> A6)\r\nACT1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\nADC1(\"a\", \"b\")\r\nADC2(diffin, \"a\", \"b\")\r\nADC3(diffout, \"a\", \"b\")\r\n(xy ADC1 <r> ADC2 <r> ADC3)\r\nADD1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\nADD2(square, \"a\")\r\n(xy ADD1 <r> ADD2)\r\nAND1(\"a\", \"b\")\r\nAND2(euro)\r\n(xy AND1 <r> AND2)\r\nANT1(\"a\", \"b\")\r\nAPP1(\"a\")\r\nAPP2(heater, \"a\")\r\nAPP3(heater, ventilator, \"a\")\r\nAPP4(heater, accu, \"a\")\r\nAPP5(ventilator, \"a\")\r\nAPP6(boiler, \"a\")\r\nAPP7(boiler, accu, \"a\")\r\nAPP8(microwave, \"a\")\r\nAPP9(oven, \"a\")\r\nAPP10(washer, \"a\")\r\nAPP11(dryer, \"a\")\r\nAPP12(dishwasher, \"a\")\r\nAPP13(fridge, \"a\")\r\nAPP14(freezer, \"a\")\r\n(xy APP1 <r> APP2 <r> APP3 <r> APP4 <r> APP5 <r> APP6 <r> APP7 <r> APP8 <r> APP9 <r> APP10 <r> APP11 <r> APP12 <r> APP13 <r> APP14)\r\nATTR1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\n(xy A1 <d> ACT1 <d> ADC1 <d> ADD1 <d> AND1 <d> ANT1 <d> APP1 <d> ATTR1 <d> BAT1)\r\n\r\n* B\r\nBAT1(\"a\", \"b\")\r\nBB1(\"a\")\r\nBIT1(\"abc\", \"def\", \"ghij\")\r\nBLOCK1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\", \"j\", \"k\", \"l\", \"m\", \"n\", \"o\", \"p\", \"q\", \"r\", \"s\", \"t\", \"u\", scale=2)\r\nBLOCK2(\"label\")\r\n(xy BLOCK1 <r> BLOCK2)\r\nBUF1(\"a\", \"b\")\r\nBUS1(\"a\", \"b\")\r\nBUS2(straight, \"a\", \"b\")\r\n(xy BUS1 <r> BUS2)\r\n(xy BAT1 <d> BB1 <d> BIT1 <d +50> BLOCK1 <d> BUF1 <d> BUS1 <d> C1)\r\n\r\n* C\r\nC1(\"a\", \"b\")\r\nC2(curved, \"a\", \"b\")\r\nC3(curved, signs, \"a\", \"b\")\r\nC4(signs, \"a\", \"b\")\r\nC5(electrolytic, \"a\", \"b\")\r\nC6(electrolytic, signs, \"a\", \"b\")\r\nC7(programmable, \"a\", \"b\")\r\nC8(sensor, \"a\", \"b\")\r\n(xy C1 <r> C2 <r> C3 <r> C4 <r> C5 <r> C6 <r> C7 <r> C8)\r\nCB1(\"a\", \"b\")\r\nCB2(euro, \"a\", \"b\")\r\nCB3(arei, \"a\", \"b\")\r\n(xy CB1 <r> CB2 <r> CB3)\r\nCIRC1(\"a\", \"b\", \"c\")\r\nCIRC2(square, \"a\", \"b\", \"c\")\r\n(xy CIRC1 <r> CIRC2)\r\nCONN1(\"a\", \"b\")\r\nCONN2(american, \"a\", \"b\")\r\nCONN3(american, male, \"a\")\r\nCONN4(american, female, \"a\")\r\n(xy CONN1 <r> CONN2 <r> CONN3 <r> CONN4)\r\nCUT1(\"a\", \"b\")\r\nCUT2(straight, \"a\", \"b\")\r\n(xy CUT1 <r> CUT2)\r\n(xy C1 <d> CB1 <d> CIRC1 <d> CONN1 <d> CUT1 <d> D1)\r\n\r\n* D\r\nD1(\"a\", \"b\")\r\nD2(varactor, \"a\", \"b\")\r\nD3(zener, \"a\", \"b\")\r\nD4(tunnel, \"a\", \"b\")\r\nD5(schockley, \"a\", \"b\")\r\nD6(photodiode, \"a\", \"b\")\r\nD7(laser, \"a\", \"b\")\r\nD8(led, \"a\", \"b\")\r\nD9(single, \"a\", \"b\")\r\nD10(slanted, \"a\", \"b\")\r\nD11(tvs, \"a\", \"b\")\r\nD12(bidirectional, \"a\", \"b\")\r\nD13(stroke, \"a\", \"b\")\r\n(xy D1 <r> D2 <r> D3 <r> D4 <r> D5 <r> D6 <r> D7 <r> D8 <r> D9 <r> D10 <r> D11 <r> D12 <r> D13)\r\nDIFF1(\"a\")\r\nDIFF2(sdomain, \"a\")\r\nDIFF3(zdomain, \"a\")\r\n(xy DIFF1 <r> DIFF2 <r> DIFF3)\r\nDIR1(\"a\", \"b\")\r\n(xy D1 <d> DIFF1 <d> DIR1 <d> E1)\r\n\r\n* E\r\nE1(\"a\", \"b\")\r\nE2(euro, \"a\", \"b\")\r\n(xy E1 <r> E2)\r\nENT1(\"a\", \"b\", \"c\")\r\n(xy E1 <d> ENT1 <d +40> F1)\r\n\r\n* F\r\nF1(\"a\", \"b\")\r\nF2(euro, \"a\", \"b\")\r\n(xy F1 <r> F2)\r\nFD1(\"a\", \"b\", \"c\", \"d\", \"e\", \"f\", \"g\", \"h\", \"i\")\r\nFDOC1(\"a\")\r\nFDOC2(multiple, \"a\")\r\n(xy FDOC1 <r +40> FDOC2)\r\nFF1(\"a\")\r\nFILT1(\"a\")\r\nFILT2(lowpass, \"a\")\r\nFILT3(lowpass2, \"a\")\r\nFILT4(highpass, \"a\")\r\nFILT5(highpass2, \"a\")\r\nFILT6(bandpass, \"a\")\r\nFILT7(graph, \"a\")\r\nFILT8(graph, highpass, \"a\")\r\nFILT9(graph, bandpass, \"a\")\r\n(xy FILT1 <r> FILT2 <r> FILT3 <r> FILT4 <r> FILT5 <r> FILT6 <r> FILT7 <r> FILT8 <r> FILT9)\r\nFIO1(\"a\")\r\nFP1(\"a\")\r\nFT1(\"a\")\r\nFUSE1(\"a\", \"b\")\r\nFUSE2(alt, \"a\", \"b\")\r\nFUSE3(euro, \"a\", \"b\")\r\n(xy FUSE1 <r> FUSE2 <r> FUSE3)\r\n(xy F1 <d> FD1 <d> FDOC1 <d> FF1 <d> FILT1 <d> FIO1 <d> FP1 <d> FT1 <d> FUSE1 <d> G1)\r\n\r\n* G\r\nG1(\"a\", \"b\")\r\nG2(euro, \"a\", \"b\")\r\n(xy G1 <r> G2)\r\nGND1(\"a\", \"b\")\r\nGND2(earth, \"a\", \"b\")\r\nGND3(chassis, \"a\", \"b\")\r\nGND4(signal, \"a\", \"b\")\r\nGND5(noiseless, \"a\", \"b\")\r\nGND6(protective, \"a\", \"b\")\r\n(xy GND1 <r> GND2 <r> GND3 <r> GND4 <r> GND5 <r> GND6)\r\n(xy G1 <d> GND1 <d> H1)\r\n\r\n* H\r\nH1(\"a\", \"b\")\r\nH2(euro, \"a\", \"b\")\r\n(xy H1 <r> H2)\r\n(xy H1 <d> I1)\r\n\r\n* I\r\nI1(\"a\", \"b\")\r\nI2(ac, \"a\", \"b\")\r\nI3(euro, \"a\", \"b\")\r\nI4(programmable, \"a\", \"b\")\r\nI5(euro, programmable, \"a\", \"b\")\r\n(xy I1 <r> I2 <r> I3 <r> I4 <r> I5)\r\nINT1(\"a\")\r\nINT2(sdomain, \"a\")\r\nINT3(zdomain, \"a\")\r\n(xy INT1 <r> INT2 <r> INT3)\r\nINV1(\"a\", \"b\")\r\nINV2(euro, \"a\")\r\n(xy INV1 <r> INV2)\r\n(xy I1 <d> INT1 <d> INV1 <d> JACK1)\r\n\r\n* J\r\nJACK1(\"a\")\r\n(xy JACK1 <d> L1)\r\n\r\n* K\r\n\r\n* L\r\nL1(\"a\", \"b\")\r\nLATCH1(\"a\")\r\nLIGHT1(\"a\", \"b\")\r\nLIGHT2(arei, \"a\", \"b\")\r\nLIGHT3(arei, wall, \"a\", \"b\")\r\nLIGHT4(arei, projector, \"a\", \"b\")\r\nLIGHT5(arei, direction, \"a\", \"b\")\r\nLIGHT6(arei, direction, diverging, \"a\", \"b\")\r\nLIGHT7(arei, emergency, \"a\", \"b\")\r\n(xy LIGHT1 <r> LIGHT2 <r> LIGHT3 <r> LIGHT4 <r> LIGHT5 <r> LIGHT6 <r> LIGHT7)\r\n(xy L1 <d> LATCH1 <d> LIGHT1 <d> MIC1)\r\n\r\n* M\r\nMIC1(\"a\", \"b\")\r\nMIX1(\"a\")\r\nMIX2(square, \"a\")\r\n(xy MIX1 <r> MIX2)\r\nMN1(\"a\")\r\nMN2(packaged, \"a\")\r\n(xy MN1 <r> MN2)\r\nMOTOR1(\"a\", \"b\")\r\nMP1(\"a\")\r\nMP2(packaged, \"a\")\r\n(xy MP1 <r> MP2)\r\nMUX1(\"a\")\r\n(xy MIC1 <d> MIX1 <d> MN1 <d> MOTOR1 <d> MP1 <d> MUX1 <d> NAND1)\r\n\r\n* N\r\nNAND1(\"a\", \"b\")\r\nNAND2(euro, \"a\")\r\n(xy NAND1 <r> NAND2)\r\nNMOS1(\"a\")\r\nNMOS2(packaged, \"a\")\r\n(xy NMOS1 <r> NMOS2)\r\nNOR1(\"a\", \"b\")\r\nNOR2(euro, \"a\", \"b\")\r\n(xy NOR1 <r> NOR2)\r\nNOT1(\"a\", \"b\")\r\nNOT2(euro, \"a\", \"b\")\r\n(xy NOT1 <r> NOT2)\r\nNPN1(\"a\")\r\nNPN2(packaged, \"a\")\r\n(xy NPN1 <r> NPN2)\r\n(xy NAND1 <d> NMOS1 <d> NOR1 <d> NOT1 <d> NPN1 <d> OA1)\r\n\r\n* O\r\nOA1(\"a\", \"b\", \"c\")\r\nOR1(\"a\", \"b\")\r\nOR2(euro, \"a\", \"b\")\r\n(xy OR1 <r> OR2)\r\nOSC1(\"a\", \"b\")\r\nOSC2(square, \"a\")\r\n(xy OSC1 <r> OSC2)\r\nOTA1(\"a\", \"b\", \"c\")\r\n(xy OA1 <d> OR1 <d> OSC1 <d> OTA1 <d> PMOS1)\r\n\r\n* P\r\nPMOS1(\"a\")\r\nPMOS2(packaged, \"a\")\r\n(xy PMOS1 <r> PMOS2)\r\nPNP1(\"a\")\r\nPNP2(packaged, \"a\")\r\n(xy PNP1 <r> PNP2)\r\nPOW1(\"a\")\r\n(xy PMOS1 <d> PNP1 <d> POW1 <d> QN1)\r\n\r\n* Q\r\nQN1(\"a\")\r\nQN2(packaged)\r\n(xy QN1 <r> QN2)\r\nQP1(\"a\")\r\nQP2(packaged)\r\n(xy QP1 <r> QP2)\r\n(xy QN1 <d> QP1 <d> R1)\r\n\r\n* R\r\nR1(\"a\", \"b\")\r\nR2(programmable, \"a\", \"b\")\r\nR3(photo, \"a\", \"b\")\r\nR4(thremistor, \"a\", \"b\")\r\nR5(euro, \"a\", \"b\")\r\nR6(euro, x, \"a\", \"b\")\r\nR7(euro, memristor, \"a\", \"b\")\r\n(xy R1 <r> R2 <r> R3 <r> R4 <r> R5 <r> R6 <r> R7)\r\n(xy R1 <d> S1)\r\n\r\n* S\r\nS1(\"a\", \"b\")\r\nS2(closed, \"a\", \"b\")\r\nS3(invert, \"a\", \"b\")\r\nS4(closing, \"a\", \"b\")\r\nS5(opening, \"a\", \"b\")\r\nS6(reed, \"a\", \"b\")\r\nS7(knife, \"a\", \"b\")\r\nS8(knife, closed, \"a\", \"b\")\r\nS9(push, \"a\", \"b\")\r\nS10(push, invert, \"a\", \"b\")\r\nS11(arei, \"a\", \"b\")\r\nS12(arei, lamp, \"a\", \"b\")\r\nS13(arei, push, \"a\", \"b\")\r\nS14(arei, push, lamp, \"a\", \"b\")\r\nS15(arei, push, window, \"a\", \"b\")\r\n(xy S1 <r> S2 <r> S3 <r> S4 <r> S5 <r> S6 <r> S7 <r> S8 <r> S9 <r> S10 <r> S11 <r> S12 <r> S13 <r> S14 <r> S15)\r\nSEG1(\"a\", \"b\")\r\nSEG2(underground, \"a\", \"b\")\r\nSEG3(air, \"a\", \"b\")\r\nSEG4(tube, \"a\", \"b\")\r\nSEG5(tube, multiple=2, \"a\", \"b\")\r\nSEG6(inwall, \"a\", \"b\")\r\nSEG7(onwall, \"a\", \"b\")\r\n(xy SEG1 <r> SEG2 <r> SEG3 <r> SEG4 <r> SEG5 <r> SEG6 <r> SEG7)\r\nSGND1(\"a\", \"b\")\r\nSPDT1(\"a\")\r\nSPEAKER1(\"a\")\r\nSPEAKER2(off, \"a\")\r\n(xy SPEAKER1 <r> SPEAKER2)\r\nSPLIT1(\"a\", \"b\")\r\nSPLIT2(-square, \"a\", \"b\")\r\n(xy SPLIT1 <r> SPLIT2)\r\nSUB1(\"a\")\r\nSUB2(square, \"a\")\r\n(xy SUB1 <r> SUB2)\r\n(xy S1 <d> SEG1 <d> SGND1 <d> SPDT1 <d> SPEAKER1 <d> SPLIT1 <d> SUB1 <d> T1)\r\n\r\n* T\r\nT1(\"a\")\r\nT2(in, \"a\")\r\nT3(out, \"a\")\r\nT4(inout, \"a\")\r\nT5(other, \"a\")\r\nT6(pad, \"a\")\r\nT7(square, \"a\")\r\nT8(none, \"a\")\r\n(xy T1 <r> T2 <r> T3 <r> T4 <r> T5 <r> T6 <r> T7 <r> T8)\r\nTA1(\"a\", \"b\", \"c\")\r\nTL1(\"a\", \"b\", \"c\")\r\n(xy T1 <d> TA1 <d> TL1 <d> V1)\r\n\r\n* U\r\n\r\n* V\r\nV1(\"a\", \"b\")\r\nV2(ac, \"a\", \"b\")\r\nV3(square, \"a\", \"b\")\r\nV4(tri, \"a\", \"b\")\r\nV5(pulse, \"a\", \"b\")\r\nV6(step, \"a\", \"b\")\r\nV7(programmable, \"a\", \"b\")\r\nV8(euro, \"a\", \"b\")\r\nV9(euro, programmable, \"a\", \"b\")\r\n(xy V1 <r> V2 <r> V3 <r> V4 <r> V5 <r> V6 <r> V7 <r> V8 <r> V9)\r\n(xy V1 <d> WP1)\r\n\r\n* W\r\nWP1(\"a\")\r\nWP2(earth, \"a\")\r\nWP3(sealed, \"a\")\r\nWP4(child, \"a\")\r\nWP5(multiple=2)\r\n(xy WP1 <r> WP2 <r> WP3 <r> WP4 <r> WP5)\r\n(xy WP1 <d> X1)\r\n\r\n* X\r\nX1(\"a\")\r\nX2(-dot, \"a\")\r\nX3(forced, \"a\") <r> X\r\n(xy X1 <r> X2 <r> X3)\r\nXNOR1(\"a\", \"b\")\r\nXNOR2(euro, \"a\", \"b\")\r\n(xy XNOR1 <r> XNOR2)\r\nXOR1(\"a\", \"b\")\r\nXOR2(euro, \"a\", \"b\")\r\n(xy XOR1 <r> XOR2)\r\nXTAL1(\"a\", \"b\")\r\n(X1 <d> XNOR1 <d> XOR1 <d> XTAL1 <d> Y1)\r\n\r\n* Y\r\nY1(\"a\", \"b\")\r\nY2(programmable, \"a\", \"b\")\r\n(xy Y1 <r> Y2)\r\n(xy Y1 <d> Z1)\r\n\r\n* Z\r\nZ1(\"a\", \"b\")\r\nZ2(programmable, \"a\", \"b\")\r\n(xy Z1 <r> Z2)\r\n")
        ];
    }
}
