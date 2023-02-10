using SimpleCircuit.Components.Digital;
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
            new Demo(
                "1. Component chains",
                "Tutorial explaining component chains.",
                "// For more tutorials, go to Help > Demo's.\r\n\r\n// A component chain is a series of components seperated by <wires>.\r\n// The type of component is defined by the first letter(s), which have to be capital letters.\r\n// Wires can be defined between '<' and '>', using their direction: u, d, l, r for up, down, left or right.\r\n// Most components also have labels, which are specified as quoted strings between parenthesis.\r\nGND1 <u> V1(\"1V\") <u r> R(\"1k\") <r d> C1(\"1uF\") <d> GND2\r\n\r\n// Virtual chains act like component chains but are not drawn.\r\n// They can be used to align components.\r\n// Virtual chains are always between brackets.\r\n(GND1 <r> GND2)\r\n"
            ),

            // Tutorial about pins
            new Demo(
                "2. Pins",
                "Tutorial explaining pins.", 
                "// Pins are specified between square brackets\r\n// It is important whether the pin is specified before or after the component\r\nX1 <r> [g]NMOS1[s] <d> GND1\r\n\r\n// The pin order of the component is important\r\n// If no pin is specified, then the first pin is used as the default for wires ending in the component.\r\n// The last pin is used for wires starting from that component.\r\nNMOS1[d] <u> R1 <u> POW\r\n// You can find this information in Help > Components.\r\n\r\n// The resistor actually has 3 pins: 'p', 'c' and 'n' (each of them have aliases too). The 'c' pin however, if not used, is hidden.\r\nR1[c] <r> T(\"hello\")"
                ),

            new Demo(
                "3. Virtual chains / alignment",
                "Tutorial explaining virtual chains more.",
                "// For more tutorials, go to Help > Demo's.\r\n\r\n// We define a number of component chains\r\nGND1 <u> V <u r> R <r> X1\r\nX1 <d> C <d> GND2\r\nX1 <r> X2 <d> L <d> R <d> GND3\r\nX2 <r> X3 <d> C <d> R <d> GND4\r\nX3 <r> T(\"output\")\r\n\r\n// We can now start aligning them using virtual chains\r\n// It is possible to align components only along one axis\r\n// This is done by adding \"x\" or \"y\" at the start of the virtual chain\r\n(x X1 <r +20> X2 <r +20> X3)\r\n\r\n// It is also possible to align components using a filter on their name\r\n// For example, we would like to align all the grounds in our circuit:\r\n(y GND*)\r\n// The '*' character acts as a wildcard\r\n\r\n// We can also align all anonymous components of a certain type in one statement.\r\n// We can for example align all anonymous capacitors:\r\n(y C)\r\n"
                ),

            // Inverting amplifier
            new Demo(
                "4. Variants and properties",
                "Tutorial explaining variants for changing appearances.",
                "// For more tutorials, go to Help > Demo's.\r\n\r\n// Variants allow changing the appearance of certain components\r\n// For example, a resistor can have the \"programmable\" variant:\r\nT1(\"in\") <r> R1(programmable) <r> T2(\"out\")\r\n\r\n// Many components also have properties that can be specified as well\r\n- R1.scale = 2\r\n\r\n// The property syntax can also be used to specify variants\r\n- T1.input = true\r\n- T2.output = true\r\n\r\n// Variants can be removed again by adding a '-' before them\r\nT2(-output, +pad)\r\n"
                ),

            // Wheatstone bridge
            new Demo(
                "5. Wires",
                "Tutorial explaining odd angle wires, and changing their appearance.",
                "// For more tutorials, go to Help > Demo's.\r\n\r\n// Wires do not have to be horizontal or vertical, SimpleCircuit can solve any angle\r\n// For shorthand notation, the 4 cardinal directions can be used\r\nX1 <n e s w> X1\r\n\r\n// In fact, the 4 ordinal directions can also be used!\r\nX2 <ne se sw nw> X2\r\n\r\n// It is possible to specify any angled wires by using <a #> as a wire segment with # the angle of the wire (counter-clockwise)\r\nX3 <a 60 r a -60 a -120 l a 120> X3\r\n// One note of caution: Using fine increments of angles can lead to rather unexpected results! Uncomment the next example to see what could happen:\r\n// X4 <e n a -91> X4\r\n// In such events, consider using unconstrained wires instead\r\n\r\n// We might just use <?> or '-' to copy the wire orientation from the pin it is connected to\r\nX5 <ne> R <?> R - R\r\n// Sometimes this could lead to errors though, for example:\r\n// X6 - R\r\n\r\n// Unconstrained wires are wires that do not constrain anything\r\n// While this may be useful for very odd angles, it also means that the components will need to be constrained in other ways.\r\n// They are specified using the <??> syntax:\r\nX7 <u> R <u r> C <r d ??> X7\r\n\r\n// Wires can also have special appearances:\r\nX8 <r arrow> X\r\nX9 <rarrow r> X\r\nX10 <arrow r> X\r\nX11 <r rarrow> X\r\nX12 <r dashed> X\r\nX13 <r dotted> X\r\n"),

            // Section demo
            new Demo("5. Sections", "An example using sections.", string.Join(Environment.NewLine, new[]
            {
                "// Define a section",
                ".section A",
                "    // These components are named, but only local to this section",
                "    GND1 <u> V1 <u r> Xa",
                "    - GND1.signal = true",
                ".endsection",
                "",
                "// We can reuse names, like GND1, without referring to the ground in the section",
                "// We can use a forward slash ('/') to refer to components inside a section",
                "A/Xa <r> L <r d> C <d> GND1",
                "(y A/GND1 <r> GND1)",
                "",
                "// You can re-use previously defined section",
                ".section B A",
                "B/Xa <r> C <r d> C <d> GND2",
                "(y B/GND1 <r> GND2)",
                "",
                ".section C A",
                "C/Xa <r> R <r d> C <d> GND3",
                "(y C/GND1 <r> GND3)",
                "",
                "(x GND*)",
                "",
                "// We can also aligh instances inside sections",
                "(x */V1)",
            })),

            // Subcircuit demo
            new Demo("6. Subcircuits", "An example using subcircuits", string.Join(Environment.NewLine, new[] {
                "// Subcircuits are solved on their own, and they can then be used as any other component.",
                "// The pins need to be specified",
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
            new Demo("7. Black box demo", "An example using a block box component.", string.Join(Environment.NewLine, new[] {
                "// Black boxes are boxes that can have custom pins.",
                "// It is possible to give a black box some pins just by mentioning them somewhere.",
                "// They are added left to right or top to bottom, and their orientation determines on which",
                "// side they are added.",
                "BB1[Input1] <l>",
                "BB1[Input2] <l>",
                "BB1[Output1] <r>",
                "BB1[Output2] <r>",
                "BB1[VDD] <u> POW",
                "BB1[VSS] <d> GND",
                "",
                "// The distance between pins can vary, but they cannot change order",
                "BB1[Output1] <r d> R <d l> [Output2]BB1",
                "",
                "// The size of the black box is only a minimum, we can stretch them out",
                "(x BB1[Input1] <r +80> [Output1]BB1)",
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
                "    MNrst[g] <l> T(\"RST\")",
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
                "PIXEL1[DIRtop] <u> T(\"COL_{OUT,k}\")",
                "PIXEL2[DIRtop] <u> T(\"COL_{OUT,k+1}\")",
                "PIXEL2[DIRright] <r> T(\"ROW_{SEL,i}\")",
                "PIXEL4[DIRright] <r> T(\"ROW_{SEL,i+1}\")",
            })),

            // Full adder logic - showing off digital logic cells
            new Demo("Demo: Full adder logic", "A demo of the digital logic cells.", string.Join(Environment.NewLine, new[]
            {
                "// Inputs",
                "Tia(\"A\") <r> Xa <r> [a]XOR1",
                "Tib(\"B\") <r> Xb <r> [b]XOR1",
                "Tic(\"Cin\") <r> Xc <r> [b]XOR2",
                "",
                "// First XOR",
                "XOR1[o] <r se r> Xab <r> [a]XOR2",
                "Xab <d r> [a]AND1",
                "",
                "// XOR gate and two AND gates",
                "XOR2[o] <r> Tos(\"S\")",
                "Xc <d r> [b]AND1",
                "Xa <d r> [a]AND2",
                "Xb <d r> [b]AND2",
                "",
                "// Last OR gate for carry out",
                "AND1[o] <r se r> [a]OR1",
                "AND2[o] <r ne r> [b]OR1",
                "OR1[o] <r> Toc(\"Cout\")",
                "",
                "// Align terminals using the wildcard '*'",
                "(x Ti*)",
                "(x To*)",
                "",
                "// Other alignment",
                "(x Xb <r 5> Xa)",
                "(x Xc <r 5> Xab)",
                "(xy XOR2 <d +15> AND1 <d +15> AND2)",
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
                "(x NAND1 <d> NAND2)",
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
                "(y Input/GND1 <r> Output/GND1)"
            })),

            // Shorthand notation characters
            new Demo("Demo: Shorthand notation", "Demonstration of shorthand notation for wires.", string.Join(Environment.NewLine, new[]
            {
                "// Arrow unicode characters can be used as shorthand notation for simple wires",
                "GND ↑ V ↑ Xin",
                "",
                "// A dash '-' character is shorthand for <?>",
                "Xin - R →↘<d +20> C - GND",
                "",
                "(y GND)",
                "",
                "// By pressing CTRL + '.' you can enter a mode that replaces certain characters with arrows",
                "// In this mode, there should be arrows in the bottom-left corner",
                "// In the \"arrow mode\":",
                "// 'u', 'n' or numpad 8 turns into ↑ (<u>)",
                "// 'l', 'w' or numpad 4 turns into ← (<l>)",
                "// 'd', 's' or numpad 2 turns into ↓ (<d>)",
                "// 'r', 'e' or numpad 6 turns into → (<r>)",
                "// numpad 7 turns into ↖ (<nw>)",
                "// numpad 9 turns into ↗ (<ne>)",
                "// numpad 1 turns into ↙ (<sw>)",
                "// numpad 3 turns into ↘ (<se>)",
                "// Any other key will exit the mode.",
            }))
        };
    }
}
