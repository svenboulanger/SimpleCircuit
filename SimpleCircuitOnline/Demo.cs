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
            new Demo("Low-pass filter", "A simple passive RC low-pass filter.", string.Join(Environment.NewLine, new[]
            {
                "// This chain starts with a ground and continues up to V1, up-right to R, right-down to C and down to another ground",
                "GND1 <u> V1(\"1V\") <u r> R(\"1k\") <r d> C1(\"1uF\") <d> GND2",
                "",
                "// Here we align the y-coordinates to make it look a little bit nicer",
                "- GND1.y = GND2.y"
            })),

            // Inverting amplifier
            new Demo("Inverting amplifier", "An inverting amplifier using an opamp.", string.Join(Environment.NewLine, new[]
            {
                "// We start from the input and move to the output",
                "GND1 <u> V1(\"Vin\") <u r> R1(\"R1\") <r> Xfb <r> OA1 <r> Xout <u l> [n]R(\"Rfb\")[p] <l d 20> Xfb",
                "OA1[p] <l d> GND2",
                "Xout <r 5> T(\"Vout\")",
                "",
                "- GND1.y = GND2.y"
            })),

            // Non-inverting amplifier
            new Demo("Non-inverting amplifier", "The circuit on EEVblog's \"I only give negative feedback\" shirt.", string.Join(Environment.NewLine, new[]
            {
                "// Let's make our input",
                "T(\"Vin\") <r> [p]OA1",
                "",
                "// Do the resistive voltage divider",
                "OA1[out] <r> Xout <d l> Rfb(\"RF\") <l> Xfb <d> R1(\"RG\") <d> GND2",
                "Xfb <u 10 r> [n]OA1",
                "",
                "Xout <r 5> T(\"Vout\")",
                "",
                "// Mirror the opamp to have the negative input on the bottom",
                "- OA1.s = -1",
            })),

            // Double pole switch
            new Demo("Two-way light switch", "A circuit for switching a light using two switches.", string.Join(Environment.NewLine, new[]
            {
                "// Make the main circuit",
                "GND1 <u> V(\"AC\") <u r> SPDT1[t1] <r> X1 <r> [t1]SPDT2[p] <r d> LIGHT1 <d> GND2",
                "SPDT1[t2] <r> X2 <r> [t2]SPDT2",
                "",
                "// Set the switch position to -1, 0 or 1 (default is 1)",
                "- SPDT1.Throw = 1",
                "- SPDT2.Throw = -1",
                "",
                "- GND1.y = GND2.y"
            })),

            // Wheatstone bridge
            new Demo("Wheatstone bridge", "A Wheatstone bridge with the typical diamond-like (45 degree angled) resistor structure.", string.Join(Environment.NewLine, new[]
            {
                "// Build the schematic",
                "X1 <l 40 u> V1(\"Vs\") <u r 40> X2",
                "X1 <?> R1 <?> Xright <?> R2 <?> X2",
                "X1 <?> R3 <?> Xleft <?> R4 <?> X2",
                "",
                "Xright <l> Tb(\"+\")",
                "Xleft <r> Ta(\"-\")",
                "",
                "// Specify the angles",
                "// The function wrap() keeps an angle between -180 to 180",
                "- R1.Angle = -45",
                "- R2.Angle = wrap(R1.Angle - 90)",
                "- R3.Angle = -180+45",
                "- R4.Angle = wrap(R3.Angle + 90)",
                "",
                "// We want enough space between the terminals",
                "- Tb.X - Ta.X = 30"
            })),

            // Full bridge rectifier
            new Demo("Full bridge rectifier", "The circuit on that YouTuber ElectroBOOM's shirt.", string.Join(Environment.NewLine, new[]
            {
                "Tlt <r> Xlt <r> D1 <r> Xrt <r> Trt(\"+\")",
                "Tlb <r> Xlb <r> [n]D4[p] <r> Xrb <r> Trb(\"-\")",
                "Xlb <u r> D2 <r u> Xrt",
                "Xrb <u l> D3 <l u> Xlt",
                "",
                "// Align the diodes to avoid overlap",
                "- D1.y + 15 = D2.y",
                "- D2.y + 15 = D3.y",
                "- D3.y + 15 = D4.y",
                "- Xlt.x + 5 = Xlb.x",
                "",
                "// Align the terminals (for niceness)",
                "- Tlt.x = Tlb.x",
                "- Trt.x = Trb.x"
            })),

            // Pixel array with 3-Transistor read-out
            new Demo("3T Pixel array", "An array of CMOS 3T pixels. Also a demonstration of subcircuits.", string.Join(Environment.NewLine, new[] {
                "// Define a single pixel",
                ".subckt pixel dirleft dirtop dirbottom dirright",
                "// Reset",
                "gnd <u> D <u> Xd <u> Mnrst <u> pow",
                "Mnrst[g] <l 0> T(\"RST\")",
                "Xd <r> [g]Mnsf[d] <u> pow",
                "Mnsf[s] <d r> Mnsel <r> Xcol",
                "Mnsel[g] <u 60> Xrow",
                "",
                "// Make column and row lines",
                "Xcol <u 70> dirtop",
                "Xcol <d 15> dirbottom",
                "Xrow <l 50> dirleft",
                "Xrow <r 20> dirright",
                ".ends",
                "",
                "// The grid of pixels",
                "pixel1 <r> pixel2",
                "pixel1[dirbottom_out] <d> [dirtop_out]pixel3",
                "pixel3 <r> pixel4[dirtop_out] <u> [dirbottom_out]pixel2",
                "",
                "// Add some terminals",
                "pixel1[dirtop_out] <u 0> T(\"COL OUT k\")",
                "pixel2[dirtop_out] <u 0> T(\"COL OUT k+1\")",
                "pixel2[dirright_out] <r 0> T(\"ROW SEL i\")",
                "pixel4[dirright_out] <r 0> T(\"ROW SEL i+1\")"
            }))
        };
    }
}
