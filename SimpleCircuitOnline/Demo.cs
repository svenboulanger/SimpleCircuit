namespace SimpleCircuitOnline
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
                                "* --- Properties and variants ---\r\n\r\n* Variants are like tags that you can attach to a component\r\nR\r\nR(programmable)\r\nR(thermistor)\r\nR(euro, memristor)\r\n\r\n* Properties are key-value pairs you can pass to a component\r\nR(zigs=5)\r\n\r\n* For a list of variants and properties, go to\r\n*   Help > Components\r\n* to have a list of available components with properties/variants."),
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
                        ]
                    },
                    new("Advanced") {
                        Items = [
                            new Demo(
                                "B1. Sections",
                                "Tutorial explaining how sections work",
                                "* --- Sections ---\r\n\r\n* A section is like a folder\r\n.section SEC\r\n    * Any named components are relative to the section\r\n    * Other sections can reuse the name 'Vin' for example\r\n    GND <u> Vin(\"Input 1\") <u r> R <r> S <r d> C <d> GND\r\n\r\n    * The virtual chain will only treat anonymous components\r\n    * inside the same section\r\n    (y GND)\r\n.ends\r\n\r\n* A section can be duplicated using the format:\r\n.section SEC2 SEC\r\n\r\n* It is possible to access elements inside sections using '/'\r\nSEC2/Vin(\"Input 2\")\r\n\r\n* Alignment across sections is also possible this way\r\n(y SEC/Vin <r> SEC2/Vin)"),
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
