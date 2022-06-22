using System;

namespace SimpleCircuitOnline
{
    /// <summary>
    /// A demo.
    /// </summary>
    public class Demo
    {
        /// <summary>
        /// Gets the title of the demo.
        /// </summary>
        /// <value>
        /// The demo title.
        /// </value>
        public string Title { get; }

        /// <summary>
        /// Gets the description of the demo.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Gets the actual code of the demo.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; }

        /// <summary>
        /// Creates a new <see cref="Demo"/>.
        /// </summary>
        /// <param name="title">The title of the demo.</param>
        /// <param name="description">The description of the demo.</param>
        /// <param name="code">The code of the demo.</param>
        public Demo(string title, string description, string code)
        {
            Title = title;
            Description = description;
            Code = code;
        }

        /// <summary>
        /// The demos.
        /// </summary>
        public static Demo[] Demos { get; } = new[]
        {
            // Low-pass RC filter
            new Demo("1. Low-pass filter", "A simple passive RC low-pass filter.", string.Join(Environment.NewLine, new[]
            {
                "// A component chain is a series of components seperated by <wires>.",
                "// The type of component is defined by the first letter(s), which have to be capital letters.",
                "// Wires can be defined between '<' and '>', using their direction: u, d, l, r for up, down, left or right.",
                "GND1 <u> V1(\"1V\") <u r> R(\"1k\") <r d> C1(\"1uF\") <d> GND2",
                "",
                "// In a lot of cases, we wish to align pins or components. This can be done using virtual chains.",
                "// These are between brackets, and the \"y\" tells along which axis alignment is needed.",
                "(y GND1 <r> GND2)"
            })),

            // Inverting amplifier
            new Demo("2. Inverting amplifier", "An inverting amplifier using an opamp.", string.Join(Environment.NewLine, new[]
            {
                "// Display signal grounds",
                ".options smallsignal = true",
                "",
                "// You can control which pin a wire starts from or ends in by adding the pin name between square brackets.",
                "// Wires can be given a fixed length by adding a number after the direction.",
                "GND <u> V(\"V_in\") <u r> R(\"R_1\") <r> Xfb <r> [n]OA1(flip) <r> Xout <u l> [n]R(\"R_fb\")[p] <l d 20> Xfb",
                "OA1[p] <l d> GND",
                "Xout <r 5> T(\"V_out\")",
                "",
                "// Align all anonymous grounds",
                "(y GND)"
            })),

            // Non-inverting amplifier
            new Demo("3. Non-inverting amplifier", "The circuit on EEVblog's \"I only give negative feedback\" shirt.", string.Join(Environment.NewLine, new[]
            {
                "// Make sure you specify the pin on the right side of the component name! This should not be OA1[p].",
                "T(\"V_in\") <r> [p]OA1",
                "",
                "// Resistive voltage divider:",
                "OA1[out] <r> Xout <d l> R(\"R_F\") <l> Xfb <d> R(\"R_G\") <d> GND",
                "Xfb <u 10 r> [n]OA1",
                "",
                "Xout <r 5> T(\"V_out\")"
            })),

            // Wheatstone bridge
            new Demo("4. Wheatstone bridge", "A Wheatstone bridge with the typical diamond-like (45 degree angled) resistor structure.", string.Join(Environment.NewLine, new[]
            {
                "// Other possible wire directions are n, s, e and w for north, south, east and west.",
                "// Non horizontal/vertical wires are also possible (ne, nw, se, sw). Generally, you can",
                "// specify any angle using <a #> with # the angle in degrees (e.g. <u> is the same as <a 90>).",
                "X1 <l u> V1(\"V_s\") <u r> X2",
                "X1 <ne> R <ne> Xright <nw> R <nw> X2",
                "X1 <nw> R <nw> Xleft <ne> R <ne> X2",
                "",
                "Xright <l> T(\"+\")",
                "Xleft <r> T(\"-\")",
                "",
                "// For wires (both normal or virtual), you can also add a '+' before the length to tell",
                "// that the distance is not exact, but a minimum.",
                "(x V1 <r +30> Xleft)"
            })),

            // Section demo
            new Demo("5. Sections", "An example using sections", string.Join(Environment.NewLine, new[]
            {
                "// Define a section",
                ".section A",
                "    // These components are named, but only local to this section",
                "    GND1 <u> V <u r> Xa",
                "    - GND1.signal = true",
                ".endsection",
                "",
                "// We can reuse names, like GND1, without referring to the ground in the section",
                "// We can use a forward slash ('/') to refer to components inside a section",
                "A/Xa <r> L <r d> C <d> GND1",
                "",
                "// Align both grounds",
                "(y A/GND1 <r> GND1)"
            })),

            // Subcircuit demo
            new Demo("5. Subcircuits", "Demo on how to use a subcircuit definition", string.Join(Environment.NewLine, new[] {
                "// Subcircuit definitions are similar to Spice netlists. Just start with '.subckt' followed",
                "// by the pins.",
                ".subckt ABC R1[p] R2[n]",
                "    R1 <r> R2",
                ".ends",
                "",
                "// Now we can instantiate this subcircuit definition multiple times.",
                "ABC1 <r d> ABC <d> Xe <l> ABC <l u> ABC <u> Xs <r> ABC1",
                "",
                "// They can even be angled because our pins also have a direction!",
                "Xs <a -45> ABC <a -45> Xe" })),

            // Black-box demo
            new Demo("6. Black box demo", "Demo on how to use a black box", string.Join(Environment.NewLine, new[] {
                "// Black boxes are boxes that can have custom pins.",
                "// When accessing pins on a black box, the order is important, as they will appear",
                "// from top to bottom or from left to right.",
                "// Additionally, the first letter of the pin indicates n(orth), s(outh), e(ast) or w(est).",
                "// These statements only serve to instantiate the pins in the correct order:",
                "BB1[nVDD]",
                "BB1[wInput1]",
                "BB1[wInput2]",
                "BB1[sVSS]",
                "BB1[eOutput1]",
                "BB1[eOutput2]",
                "",
                "// The order of the pins is fixed now, so we can connect whatever we want.",
                "BB1[nVDD] <u> POW",
                "BB1[sVSS] <d> GND",
                "",
                "// The pins are ordered, but their spacing can still depend on other elements",
                "BB1[eOutput1] <r d> R <d l> [eOutput2]BB1",
                "",
                "// We can also align the pins and resize the black box using them",
                "(y BB1[wInput2] <0> [eOutput2]BB1)",
                "(y BB1[wInput1] <0> [eOutput1]BB1)",
                "(x BB1[wInput1] <r +60> [eOutput1]BB1)",
            })),

            // Double pole switch
            new Demo("Demo: Two-way light switch", "A circuit for switching a light using two switches.", string.Join(Environment.NewLine, new[]
            {
                "// Make the main circuit",
                "GND <u> V(\"AC\", ac) <u r> SPDT1[t1] <r> X1 <r> [t1]SPDT2[p] <r d> LIGHT1 <d> GND",
                "SPDT1(t1)[t2] <r> X2 <r> [t2]SPDT2(t2)",
                "",
                "SPDT1[c] <u> T(\"A\")",
                "SPDT2[c] <u> T(\"B\")",
                "",
                "// Align all anonymous grounds and terminals",
                "(y GND)",
                "(y T)"
            })),

            // Full bridge rectifier
            new Demo("Demo: Full bridge rectifier", "The circuit on a certain shocking YouTuber's shirt.", string.Join(Environment.NewLine, new[]
            {
                "Tlt <r> Xlt <r> D1 <r> Xrt <r> Trt(\"+\")",
                "Tlb <r> Xlb <r> [n]D4[p] <r> Xrb <r> Trb(\"-\")",
                "Xlb <u r> D2 <r u> Xrt",
                "Xrb <u l> D3 <l u> Xlt",
                "",
                "// Space the diodes apart for at least 15pt",
                "(y D1 <d +15> D2 <d +15> D3 <d +15> D4)",
                "",
                "// Alignment of some wires",
                "(x Xlt <r 5> Xlb)",
                "",
                "// Align the terminals",
                "(x Tlt <0> Tlb)",
                "(x Trt <0> Trb)",
            })),

            // A simple push-pull CMOS inverter
            new Demo("Demo: CMOS inverter", "A CMOS push-pull inverter.", string.Join(Environment.NewLine, new[]
            {
                "// Define the push-pull branch.",
                "GND <u> NMOS1 <u> Xout <u> PMOS1 <u> POW",
                "",
                "// Define the gate structure",
                "PMOS1[g] <l d> Xin <d r> [g]NMOS1",
                "",
                "// Add some input signal",
                "GND <u> V1 <u r> Xin",
                "",
                "// Add some load",
                "Xout <r d> CL(\"CL\") <d> GND",
                "",
                "// Some alignment",
                "(y GND)",
                "(y Xin <r> Xout)",
                "(x V1 <r +20> Xin)",
                "(x NMOS1 <r +20> CL)",
            })),

            // Pixel array with 3-Transistor read-out
            new Demo("Demo: 3T Pixel array", "An array of CMOS 3T pixels. Also a demonstration of subcircuits.", string.Join(Environment.NewLine, new[] {
                "// Define a single pixel",
                ".subckt PIXEL DIRleft DIRtop DIRbottom DIRright",
                "    // Reset",
                "    GND <u> D(photodiode) <u> Xd <u> MNrst <u> POW",
                "    MNrst[g] <l 0> T(\"RST\")",
                "    Xd <r> [g]MNsf[d] <u> POW",
                "    MNsf[s] <d r> MNsel <r> Xcol",
                "    MNsel[g] <u 60> Xrow",
                "",
                "    // Make column and row lines (DIR is used as direction for pins)",
                "    Xcol <u 70> DIRtop",
                "    Xcol <d 15> DIRbottom",
                "    Xrow <l 60> DIRleft",
                "    Xrow <r 20> DIRright",
                ".ends",
                "",
                "// We have chosen the subcircuit names such that they are unique,",
                "// allowing us to use the short names",
                "PIXEL1 <r> PIXEL2",
                "PIXEL1[DIRbottom] <d> [DIRtop]PIXEL3",
                "PIXEL3 <r> PIXEL4[DIRtop] <u> [DIRbottom]PIXEL2",
                "",
                "// Add some terminals",
                "PIXEL1[DIRtop] <u 0> T(\"COL_{OUT,k}\")",
                "PIXEL2[DIRtop] <u 0> T(\"COL_{OUT,k+1}\")",
                "PIXEL2[DIRright] <r 0> T(\"ROW_{SEL,i}\")",
                "PIXEL4[DIRright] <r 0> T(\"ROW_{SEL,i+1}\")",
            })),

            // Full adder logic - showing off digital logic cells
            new Demo("Demo: Full adder logic", "A demo of the digital logic cells.", string.Join(Environment.NewLine, new[]
            {
                "// Inputs",
                "Ta(\"A\") <r> Xa <r> [a]XOR1",
                "Tb(\"B\") <r> Xb <r> [b]XOR1",
                "Tc(\"Cin\") <r> Xc <r> [b]XOR2",
                "",
                "// First XOR",
                "XOR1[o] <r se r> Xab <r> [a]XOR2",
                "Xab <d r> [a]AND1",
                "",
                "// XOR gate and two AND gates",
                "XOR2[o] <r> Ts(\"S\")",
                "Xc <d r> [b]AND1",
                "Xa <d r> [a]AND2",
                "Xb <d r> [b]AND2",
                "",
                "// Last OR gate for carry out",
                "AND1[o] <r se r> [a]OR1",
                "AND2[o] <r ne r> [b]OR1",
                "OR1[o] <r> Tco(\"Cout\")",
                "",
                "// Alignment",
                "(x Ta <0> Tb <0> Tc)",
                "(x Xb <r +5> Xa)",
                "(x Xc <r +5> Xab)",
                "(x Ts <0> Tco)",
                "(xy XOR2[o] <d +15> [o]AND1[o] <d +15> [o]AND2)",
            })),

            // Some transformers
            new Demo("Demo: Transformers", "A demo of some improvised transformers.", string.Join(Environment.NewLine, new[]
            {
                ".subckt M DIRpa[i] DIRpb[o] DIRsa[i] DIRsb[o]",
                "    DIRpa <d 0> L1(dot) <d 0> DIRpb",
                "    DIRsa <d 0> L2(dot) <d 0> DIRsb",
                "    (xy L1 <r 10> L2)",
                "    - L2.Flipped = true",
                ".ends",
                "",
                "// Primary side",
                "V1 <u r d> [DIRpa]M1[DIRpb] <d l u> V1",
                "",
                "// Secondary side to second transformer",
                "M1[DIRsa] <u r d> [DIRpa]M2[DIRpb] <d l u> [DIRsb]M1",
                "",
                "// Load",
                "M2[DIRsa] <u r d> RL <d l u> [DIRsb]M2",
                "",
                "// Alignment",
                "(x V1 <r +25> M1 <r +25> M2 <r +25> RL)",
            })),

            // A latch
            new Demo("Demo: Latch", "A latch.", string.Join(Environment.NewLine, new[]
            {
                "// Horizontal chains",
                "T <r> Xia <r> NAND1 <r> Xoa <r> T",
                "T <r> Xib <r> NAND2 <r> Xob <r> T",
                "",
                "// The cross-coupled wires",
                "NAND1[b] <l d a -20 d> Xob",
                "Xoa <d a -160 d r> [b]NAND2",
                "(x NAND1[o] <0> [o]NAND2)",
                "",
                "// Finally flip the bottom one",
                "- NAND2.Flipped = true",
            })),

            // A charge transimpedance amplifier
            new Demo("Demo: CTIA", "A Charge Transimpedance Amplifier also using sections.", string.Join(Environment.NewLine, new[]
            {
                "// Input section",
                ".section Input",
                "    X1 <l d> I(\"I_in\") <d> GND1",
                "    X1 <d> C(\"C_in\") <d> GND2",
                "    (xy GND1 <r +25> GND2)",
                ".endsection",
                "",
                "// Link to the next section",
                "Input/X1 <r> CTIA/Xin",
                "",
                "// Charge Transimpedance Amplifier",
                ".section CTIA",
                "    Xin <r> A1 <r> Xout",
                "    Xin <u r> C1(\"C_fb\") <r d> Xout",
                "    - A1.Gain = \"-A\"",
                "    (y A1 <u +20> C1)",
                ".endsection",
                "",
                "// Link CTIA to output circuit",
                "CTIA/Xout <r> Output/Xout",
                "",
                "// Output circuit",
                ".section Output",
                "    Xout <d> C1(\"C_L\") <d> GND1",
                "    Xout <r> Xout2 <d> R1(\"R_L\") <d> GND2",
                "    Xout2 <r> T(\"V_out\")",
                "    (xy GND1 <r +20> GND2)",
                ".endsection",
                "",
                "// We can still enforce alignment between elements",
                "// This would not be possible with subcircuits",
                "(y Input/GND1 <0> Output/GND1)"
            }))
        };
    }
}
