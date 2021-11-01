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
                "// These are between brackets, and first indicate along which axis you wish to align.",
                "(Y GND1 <0> GND2)"
            })),

            // Inverting amplifier
            new Demo("2. Inverting amplifier", "An inverting amplifier using an opamp.", string.Join(Environment.NewLine, new[]
            {
                "// You can control which pin a wire starts from or ends in by adding the pin name between square brackets.",
                "// Wires can be given a fixed length by adding a number after the direction.",
                "GND1 <u> V1(\"Vin\") <u r> R1(\"R1\") <r> Xfb <r> OA1 <r> Xout <u l> [n]R(\"Rfb\")[p] <l d 20> Xfb",
                "OA1[p] <l d> GND2",
                "Xout <r 5> T(\"Vout\")",
                "",
                "(Y GND1 <0> GND2)"
            })),

            // Non-inverting amplifier
            new Demo("3. Non-inverting amplifier", "The circuit on EEVblog's \"I only give negative feedback\" shirt.", string.Join(Environment.NewLine, new[]
            {
                "// Make sure you specify the pin on the right side of the component name! This should not be OA1[p].",
                "T(\"Vin\") <r> [p]OA1",
                "",
                "// Resistive voltage divider:",
                "OA1[out] <r> Xout <d l> Rfb(\"RF\") <l> Xfb <d> R1(\"RG\") <d> GND2",
                "Xfb <u 10 r> [n]OA1",
                "",
                "Xout <r 5> T(\"Vout\")",
                "",
                "// Many components have properties that can be set by starting a line with '-'.",
                "// You can find a list of properties in the list of components on the right.",
                "- OA1.SwapInputs = true",
            })),

            // Wheatstone bridge
            new Demo("4. Wheatstone bridge", "A Wheatstone bridge with the typical diamond-like (45 degree angled) resistor structure.", string.Join(Environment.NewLine, new[]
            {
                "// Other possible wire directions are n, s, e and w for north, south, east and west.",
                "// Non horizontal/vertical wires are also possible (ne, nw, se, sw). Generally, you can",
                "// specify any angle using <a #> with # the angle in degrees (e.g. <u> is the same as <a 90>).",
                "X1 <l u> V1(\"Vs\") <u r> X2",
                "X1 <ne> R1 <ne> Xright <nw> R2 <nw> X2",
                "X1 <nw> R3 <nw> Xleft <ne> R4 <ne> X2",
                "",
                "Xright <l> Tb(\"+\")",
                "Xleft <r> Ta(\"-\")",
                "",
                "// For wires (both normal or virtual), you can also add a '+' before the length to tell",
                "// that the distance is not exact, but a minimum.",
                "(X V1 <r +30> Xleft)",
                "",
                "// Extra note: Angled wires can be a bit more tricky to converge to a solution, so use with care",
            })),

            // Subcircuit demo
            new Demo("5. Subcircuits", "Demo on how to use a subcircuit definition", string.Join(Environment.NewLine, new[] {
                "// Subcircuit definitions are similar to Spice netlists. Just start with '.subckt' followed",
                "// by the pins.",
                ".subckt ABC R1[p] R2[n]",
                "R1 <r> R2",
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
                "(Y BB1[wInput2] <0> [eOutput2]BB1)",
                "(Y BB1[wInput1] <0> [eOutput1]BB1)",
                "(X BB1[wInput1] <r +60> [eOutput1]BB1)",
            })),

            // Double pole switch
            new Demo("Demo: Two-way light switch", "A circuit for switching a light using two switches.", string.Join(Environment.NewLine, new[]
            {
                "// Make the main circuit",
                "GND1 <u> V(\"AC\") <u r> SPDT1[t1] <r> X1 <r> [t1]SPDT2[p] <r d> LIGHT1 <d> GND2",
                "SPDT1[t2] <r> X2 <r> [t2]SPDT2",
                "",
                "// These switches can be put in positions 0, 1 or -1",
                "- SPDT1.Throw = 1",
                "- SPDT2.Throw = -1",
                "",
                "SPDT1[c] <u> T(\"A\")",
                "SPDT2[c] <u> T(\"B\")",
                "",
                "(Y GND1 <u 0> GND2)",
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
                "(Y D1 <d +15> D2 <d +15> D3 <d +15> D4)",
                "",
                "// Alignment of some wires",
                "(X Xlt <r 5> Xlb)",
                "",
                "// Align the terminals",
                "(X Tlt <0> Tlb)",
                "(X Trt <0> Trb)",
            })),

            // A simple push-pull CMOS inverter
            new Demo("Demo: CMOS inverter", "A CMOS push-pull inverter.", string.Join(Environment.NewLine, new[]
            {
                "// Define the push-pull branch.",
                "GND1 <u> NMOS1 <u> Xout <u> PMOS1 <u> POW",
                "",
                "// Define the gate structure",
                "PMOS1[g] <l d> Xin <d r> [g]NMOS1",
                "",
                "// Add some input signal",
                "GND2 <u> V1 <u r> Xin",
                "",
                "// Add some load",
                "Xout <r d> CL(\"CL\") <d> GND3",
                "",
                "// Some alignment",
                "(Y GND1 <0> GND2 <0> GND3)",
                "(X V1 <r +20> Xin)",
                "(X NMOS1 <r +20> CL)",
                "",
                "// Use packages symbol style",
                "- NMOS1.Packaged = true",
                "- PMOS1.Packaged = true",
            })),

            // Pixel array with 3-Transistor read-out
            new Demo("Demo: 3T Pixel array", "An array of CMOS 3T pixels. Also a demonstration of subcircuits.", string.Join(Environment.NewLine, new[] {
                "// Define a single pixel",
                ".subckt PIXEL DIRleft DIRtop DIRbottom DIRright",
                "// Reset",
                "GND <u> D <u> Xd <u> MNrst <u> POW",
                "MNrst[g] <l 0> T(\"RST\")",
                "Xd <r> [g]MNsf[d] <u> POW",
                "MNsf[s] <d r> MNsel <r> Xcol",
                "MNsel[g] <u 60> Xrow",

                "// Make column and row lines",
                "Xcol <u 70> DIRtop",
                "Xcol <d 15> DIRbottom",
                "Xrow <l 50> DIRleft",
                "Xrow <r 20> DIRright",
                ".ends",

                "// The grid of pixels",
                "PIXEL1 <r> PIXEL2",
                "PIXEL1[DIRbottom_o] <d> [DIRtop_o]PIXEL3",
                "PIXEL3 <r> PIXEL4[DIRtop_o] <u> [DIRbottom_o]PIXEL2",

                "// Add some terminals",
                "PIXEL1[DIRtop_o] <u 0> T(\"COL OUT k\")",
                "PIXEL2[DIRtop_o] <u 0> T(\"COL OUT k+1\")",
                "PIXEL2[DIRright_o] <r 0> T(\"ROW SEL i\")",
                "PIXEL4[DIRright_o] <r 0> T(\"ROW SEL i+1\")",
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
                "XOR1[o] <r 5 se r 5> Xab <r> [a]XOR2",
                "Xab <d r> [a]AND1",
                "",
                "// XOR gate and two AND gates",
                "XOR2[o] <r> Ts(\"S\")",
                "Xc <d r> [b]AND1",
                "Xa <d r> [a]AND2",
                "Xb <d r> [b]AND2",
                "",
                "// Last OR gate for carry out",
                "AND1[o] <r 5 se r> [a]OR1",
                "AND2[o] <r 5 ne r> [b]OR1",
                "OR1[o] <r> Tco(\"Cout\")",
                "",
                "// Alignment",
                "(X Ta <0> Tb <0> Tc)",
                "(X Xb <r 5> Xa)",
                "(X Xc <r 5> Xab)",
                "(X XOR2[a] <0> [a]AND1[a] <0> [a]AND2)",
                "(X Ts <0> Tco)",
                "(Y XOR2 <d 15> AND1 <d +15> AND2)",
            })),

            // Some transformers
            new Demo("Demo: Transformers", "A demo of some improvised transformers.", string.Join(Environment.NewLine, new[]
            {
                "// The transformers can already be aligned",
                "(Y Lp1 <0> Ls1 <0> Lp2 <0> Ls2)",
                "(X Lp1 <r +10> Ls1)",
                "(X Lp2 <r +10> Ls2)",
                "",
                "// Because we already took care of our alignments",
                "// we can just define with the inductors as we go",
                "Lp1 <d l +25> Xgnd <u> V1 <u r d> Lp1",
                "Xgnd <d> GND",
                "Ls1 <u r +25 d> Lp2 <d l u> Ls1",
                "Ls2 <u r +25 d> RL <d l u> Ls2",
            })),

            // A latch
            new Demo("Demo: Latch", "A latch.", string.Join(Environment.NewLine, new[]
            {
                "T <r> Xia <r> NAND1 <r> Xoa <r> T",
                "T <r> Xib <r> NAND2 <r> Xob <r> T",
                "NAND1[b] <l d a -20 d> Xob",
                "Xoa <d a -160 d r> [b]NAND2",
                "(X NAND1[o] <0> [o]NAND2)",
            })),
        };
    }
}
