﻿namespace SimpleCircuitOnline
{
    /// <summary>
    /// A category for demo's.
    /// </summary>
    /// <param name="title">The category title.</param>
    public class DemoCategory(string title)
    {
        /// <summary>
        /// Gets the title of the category.
        /// </summary>
        public string Title => title;

        /// <summary>
        /// Gets the demo categories.
        /// </summary>
        public DemoCategory[] Categories { get; init; }

        /// <summary>
        /// Gets the demo.
        /// </summary>
        public Demo[] Items { get; init; }

        /// <summary>
        /// Gets the main demo that can be used.
        /// </summary>
        public static Demo MainDemo { get; } = new(
            "Main example",
            "The main example that is shown when nothing is entered yet.",
            "* Please go to Help > Demo's for more examples and tutorials.\r\n\r\n* A component chain is a sequence of components and wires.\r\nGND <u> V1(\"V_1\") <u r> R1(\"R\") <r> Xout <d> C1(\"C\") <d> GND\r\nXout <r> T(output, \"V_out\")\r\n\r\n* You can align components using virtual chains\r\n(y GND)");

        /// <summary>
        /// Gets the set of demo's.
        /// </summary>
        public static DemoCategory[] Demos { get; } = [
            new("Tutorials") {
                Categories = [
                    new("Basics") {
                        Items = [
                            new Demo(
                                "A1. Components",
                                "Tutorial explaining the basics of named and anonymous components.",
                                "* --- Components ---\r\n\r\n* Named components:\r\n*  - Just like SPICE, the first letters identify the type.\r\n*  - Any following letters make it a named component.\r\nV1\r\nRrest_of_name\r\n\r\n* Reusing the name simply refers to the previous components\r\nV1\r\n\r\n* Anonymous components:\r\n*  - Using just the identifying letters is also possible.\r\n*  - Anonymous components are like quick and temporary.\r\nC\r\nL\r\n\r\n* Reusing anonymous components is not possible\r\n* The following components create 2 additional components:\r\nC\r\nL"),
                            new Demo(
                                "A2. Properties and variants",
                                "Tutorial explaning the basics of component properties and variants.",
                                "* --- Properties and variants ---\r\n\r\n* Variants are like tags that you can attach to a component\r\nR\r\nR(programmable)\r\nR(thermistor)\r\nR(euro, memristor)\r\n\r\n* Properties are key-value pairs you can pass to a component\r\nR(zigs=5)\r\n\r\n* If you want to have all components matching a filter to \r\n* have certain properties of variants, you can use the\r\n* '.properties' or '.variants' control statement\r\n* These control statements only act in the current scope,\r\n* so if you try moving the '.properties' statement out\r\n* of the scope block, all of the components will turn green\r\n.scope\r\n    .properties * color=\"green\"\r\n    .variants L programmable\r\n    R L C\r\n.ends\r\n\r\n* For a list of variants and properties, go to\r\n*   Help > Components\r\n* to have a list of available components with properties/variants.\r\n"),
                            new Demo(
                                "A3. Wires",
                                "Tutorial explaining the basics of wires.",
                                "* --- Wires ---\r\n\r\n* Wires are defined between '<' and '>'\r\n* You will mostly be using the directions 'l', 'u', 'r' and 'd'\r\n* for the left, up, right and down direction respectively\r\nGND <u> V <u r> R <r> T\r\n\r\n* By default, a wire will have a minimum length of 10\r\n* You can change the minimum length by adding '+' and a\r\n* number after the direction\r\nGND <r +20> V\r\n\r\n* You can specify a fixed wire length by adding a number\r\n* after the wire direction without the '+'\r\nGND <u 5> V <u 20 r 10> R <r 0> T\r\n\r\n* Odd-angle wires are also possible using the 'a'\r\n* followed by the angle in degrees\r\nGND <a 45> V <a 45 a -45> R <a -45> T"),
                            new Demo(
                                "A4. Pins",
                                "Tutorial explaining the basics of pins.",
                                "* --- Pins ---\r\n\r\n* For components that have more than 2 pins, you need the\r\n* ability to specify which pin a wire should use to connect.\r\n* This is done using '[' and ']' brackets.\r\n\r\nNMOS1[g] <r> T\r\n\r\nGND <u> [s]NMOS1[d] <u> POW\r\n\r\n* Some pins are only available in specific cases\r\n* For example, a logic AND gate can be configured to have\r\n* multiple inputs\r\nT <r> [a]AND1\r\nT <r> [b]AND1\r\nT <r> [c]AND1\r\n\r\n* Commenting out the next line will create errors\r\n* because then there would only be 2 inputs instead of 3.\r\nAND1(inputs = 3)"),
                            new Demo(
                                "A5. Virtual chains",
                                "Tutorial explaning how to align components",
                                "* --- Virtual chains ---\r\n\r\n* Virtual chains are like component chains, but they\r\n* appear between '(' and ')'.\r\n\r\nGND1 <u> R <u> R <u r +30 d> R <d> GND2\r\n\r\n* Starting a virtual chain with 'x' or 'y' will only align\r\n* items along the x or y axis.\r\n* If no 'x' or 'y' is specified, 'xy' is used which will\r\n* align along both axes.\r\n(y GND1 <r> GND2)\r\n\r\n* Anonymous components in virtual chains are a catch-all\r\n* for all anonymous components\r\nGND <u> C <u> C <u r +30 d> C <d> GND\r\n(y GND)\r\n\r\n* Virtual chains also allow wildcard characters '*'\r\nGNDa <u +10> Rload1 <u +30> POWvdd1(\"VDD\")\r\nGNDb <u +20> Cload2 <u +20> POWvdd2(\"VDD\")\r\nGNDc <u +30> Lload3 <u +10> POWvcc3(\"VCC\")\r\n(y *load*)"),
                            new Demo(
                                "A6. Styling",
                                "Tutorial explaining how you can style components.",
                                "* --- Styling ---\r\n\r\n* Components can be styled with foreground/background colors\r\nR(color=\"red\")\r\n\r\n* Most components have the following properties:\r\n*  - transparency, opacity or alpha: a number from 0 to 1 that specifies the foreground and background opacity (default is 1).\r\n*  - foregroundopacity, fgo: a number from 0 to 1 that overrides only the foreground opacity (default is 1).\r\n*  - backgroundopacity, bgo: a number from 0 to 1 that overrides only the background opacity (default is 1).\r\n*  - thickness, t: a number that determines the line thickness (default is 0.5).\r\n*  - fontsize: a number that determines labels and text size (default is 4).\r\n*  - color, fg, foreground: a string that represents the foreground color (default is '--foreground').\r\n*  - bg, background: a string that represents the background color (default is '--background').\r\n*  - font, fontfamily: the font family name (default is 'Arial').\r\n*  - bold: a boolean that if true turns all labels and text into bold text.\r\nR(alpha=0.5, t=1, fontsize=2, \"Text\")\r\n\r\n* Many components also have the following variants:\r\n*  - dashed: a dashed line style.\r\n*  - dotted: a dotted line style.\r\nAND(dashed)\r\nAND(dotted)\r\n\r\n* Rather than specifying colors directly, you can also\r\n* use one of the following color variables.\r\n* These colors follow Bootstrap 5 color definitions.\r\n*  - \"--foreground\": light mode \"#212529\", dark mode \"#dee2e6\"\r\n*  - \"--background\": \"none\" by default\r\n*  - \"--primary\": \"#007bff\" by default (blueish)\r\n*  - \"--secondary\": \"#6c757d\" by default (greyish)\r\n*  - \"--success\": \"#28a745\" by default (greenish)\r\n*  - \"--warning\": \"#ffc107\" by default (yellowish)\r\n*  - \"--danger\": \"#dc3545\" by default (reddish)\r\n*  - \"--light\": \"#f8f9fa\" by default\r\n*  - \"--dark\": \"#343a40\" by default\r\nXOR(fg=\"--primary\")\r\n"),
                            new Demo(
                                "A7. Labeling",
                                "Tutorial explaning how to add text labels.",
                                "* --- Labeling ---\r\n\r\n* Most components support labels\r\n* There are two ways to specify labels: through properties,\r\n* or just as strings. There is no limit to the number of labels.\r\nR(\"label 1\", \"label 2\")\r\nR(label1=\"label A\", label2=\"label B\\nNew line\")\r\n\r\n* Labels also support a limited set of LaTeX style\r\n* notation for:\r\n*  - \\underline{...} for underlined text.\r\n*  - \\overline{...} for overlined text.\r\n*  - \\textb{...} for bold text.\r\n*  - \\textcolor{...}{...} for colored text.\r\nV(\"\\overline{v^2_n}\")\r\nC(\"a \\textb{\\textcolor{red}{b_i}}\")\r\nD(\"a \\textcolor{#007bff}{b^(k)}\")\r\n\r\n* Labels are placed at anchor positions\r\n* E.g., for an ADC, the first label is normally placed\r\n* inside the symbol, but we can change this by specifying\r\n* the anchor index:\r\nADC(\"top label\", anchor1=1)\r\n\r\n* Per label, you can also specify:\r\n*  - offset#: an offset to tweak label position.\r\n*  - size# or fontsize#: the label font size.\r\n*  - opacity#: a number from 0 to 1 for the transparency (default is 1).\r\n*  - justify# or justification#: if 1, left-aligned, 0 for centered, -1 for right-aligned (default is 1).\r\n*  - ls# or linespacing#: The line spacing for multiline labels (default 1.5).\r\n*  - font# or fontfamily#: the font family name.\r\n*  - bold#: a boolean that can make the label bold.\r\nT(\"ABC\", font1=\"Courier\", bold1=false, offset1={-1, 3})\r\n\r\n(y *)\r\n"),
                        ]
                    },
                    new("Advanced") {
                        Items = [
                            new Demo(
                                "B1. Parameters and Expression",
                                "Tutorial on parameters, expressions, if-else statements and for-loops.",
                                "* --- Variables and Expressions ---\r\n\r\n* Like in SPICE, '.param' statements can define parameters\r\n* These can in turn be used in expressions that are placed\r\n* between '{' and '}'\r\n* Also like SPICE, .param statements don't have to appear\r\n* before being used. They are simply defined for the\r\n* current scope\r\nR(label1={myLabel})\r\n.param myLabel = \"Hello World\"\r\n\r\n* Parameters can also be used in component names\r\n.param componentName = \"DCmyName\"\r\nA{componentName} <r> T(out)\r\nADCmyName(\"Label\")\r\n\r\n* Parameters can also be used in if-else statements\r\n.param case = 1\r\n.if {case == 0}\r\n    C(\"Case = 0\")\r\n.else\r\n    C(\"Case != 0\")\r\n.endif\r\n\r\n* Parameters can also be defined in a scope by a for-loop\r\n.for i 0 5 1\r\n    X{i} <r +5> R(label1={\"R_\" + i}) <r +5> X{i + 1}\r\n.endf"),
                            new Demo(
                                "B2. Sections",
                                "Tutorial explaining how sections work",
                                "* --- Sections ---\r\n\r\n* A section is a group of wires and components that are\r\n* grouped together and are less easily accessed from the\r\n* outside. Sections can have parameters.\r\n.section SEC fg=\"--primary\"\r\n    .property * fg={fg}\r\n    .property wire fg={fg}\r\n\r\n    * Any named components are relative to the section\r\n    * Other sections can reuse the name 'Vin' for example\r\n    GND <u> Vin(\"Input 1\") <u r> R <r> S <r d> C <d> GND\r\n\r\n    * The virtual chain will only treat anonymous components\r\n    * inside the same section\r\n    (y GND)\r\n.ends\r\n\r\n* A previous section can be copied using the format:\r\n.section SEC2 SEC(fg=\"--secondary\")\r\n\r\n* It is possible to access elements inside sections using '/'\r\nSEC2/Vin(\"Input 2\")\r\n\r\n* Alignment across sections is also possible this way\r\n(y SEC/Vin <r> SEC2/Vin)\r\n"),
                            new Demo(
                                "B3 - Subcircuits",
                                "Tutorial explaining how subcircuits work",
                                "* --- Subcircuits ---\r\n\r\n* Subcircuits are solved as a separate mini-circuit and\r\n* will then act as a component on their own.\r\n* They should have ports, and can be followed by parameters.\r\n* For example:\r\n.subckt LPF DIRin[in] DIRout[b] r=\"1k&#937;\" c=\"1uF\" fg=\"--foreground\"\r\n    .property * fg = {fg}\r\n    DIRin <r> R(label1={r}) <r x r 5> DIRout\r\n    X <d> C(label1={c}) <d> GND\r\n.ends\r\n\r\n* The subcircuit name (LPF) can be used as a component key\r\n* The parameters can be passed like properties\r\nT(in, \"V_in\") <r> LPF1 <r> X1\r\nX1 <ne> LPF(r=\"2k&#937;\", fg=\"--primary\") <ne> T(\"V_out1\", out)\r\nX1 <se> LPF(c=\"2uF\", fg=\"--secondary\") <se> T(\"V_out2\", out)\r\n"),
                            new Demo(
                                "B4 - Queued Anonymous Points",
                                "Tutorial explaining how to use queued anonymous points",
                                "* --- Queued Anonymous Points ---\r\n\r\n* In many cases we would like to mark down a quick point\r\n* that we can use later\r\n* Inside a wire definition, the letter 'x' will queue\r\n* an anonymous point, that can be picked up again when\r\n* an anonymous point is created with 'X'\r\n\r\nT(in) <r x r> C <r x r> T(out)\r\n\r\n* We have queued up two points with the previous chain\r\n* We will now dequeue these two points in the following\r\n* statement. The order of the X'es are important!\r\nX <u r> S <r d> X"),
                            new Demo(
                                "B5 - Black Boxes",
                                "Tutorial explaning how to use black boxes with dynamic pin names.",
                                "* --- Black Boxes ---\r\n\r\n* Black Boxes ('BB') are components that we don't know the pins\r\n* of beforehand. Pins of a 'BB' are created dynamically.\r\n\r\n* The following chain creates 2 pins with the name 'a' and 'b'\r\n* The order is important! The first pin will appear top or left of\r\n* the component. The wire that connects to the pin determines\r\n* on which side the pin should be added.\r\nBB1[a] <r d> R <d l> [b]BB1\r\n\r\n* The first underscore character '_' has special meaning\r\n* Everything before the first '_' will be used as an alias for the pin\r\n* Everything after the first '_' will be displayed as the pin name.\r\n* The whole pin name can always be used as a reference, and\r\n* without '_', the whole name is used as the pin\r\nBB1[_shown] <d r> L <r u> [hidden_]BB1\r\n\r\n* Using an expression, you can pretty make anything act as\r\n* the pin text.\r\nBB1[special_{\"\\overline{v^2}\"}] <l> T(in)\r\n"),
                            new Demo(
                                "B6 - Backtracking Anonymous Components",
                                "Tutorial explaining how anonymous components can still be accessed after their creation using backtracking.",
                                "* --- Backtracking Anonymous Components ---\r\n\r\n* It is sometimes tedious to invent names over and over\r\n* again if you need to work with components that, for\r\n* example, have multiple pins, or require multiple\r\n* references\r\n* In such cases, anonymous components can be backtracked\r\n* by using the tilde '~' operator followed by how much\r\n* needs to be backtracked\r\nR <r> R\r\nR~2(\"Hello\")\r\nR~1(\"World\")\r\n\r\n* Just remember that backtracking is local to the current\r\n* section or current subcircuit definition\r\n.section SEC\r\n    * Backtracking cannot find components outside the section\r\n    * Uncommenting the following line leads to an error\r\n    * R~1(\"lbl\")\r\n\r\n    * Backtracking to components inside the section is OK\r\n    R(\"Dummy\")\r\n    R~1(zigs=2)\r\n.ends\r\n\r\n* This backtracked component cannot access the \r\n* anonymous R inside section 'SEC'.\r\n* It instead points to the R before the section.\r\nR~1(programmable)\r\n\r\n* It can sometimes be useful when combined with control\r\n* statements, such as for-loops or if-else statements.\r\n.param programmable = true\r\n.if {programmable}\r\n    R(programmable)\r\n.else\r\n    R\r\n.endif\r\nR~1(\"Outside\")\r\n"),
                        ]
                    }
                ]
            },
            new("Examples") {
            }
        ];
    }

    /// <summary>
    /// A demo.
    /// </summary>
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
        public string Title => title;

        /// <summary>
        /// Gets the description of the demo.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description => description;

        /// <summary>
        /// Gets the actual code of the demo.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code => code;
    }
}
