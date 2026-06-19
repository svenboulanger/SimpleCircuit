using System.Collections.Generic;

namespace SimpleCircuit.Parser.SimpleTexts;

/// <summary>
/// A curated set of HTML named entities that are guaranteed to be understood in
/// labels. The values are Unicode code points; <see cref="Drawing.Spans.SimpleTextFormatter"/>
/// rewrites <c>&amp;name;</c> to the matching numeric character reference
/// (<c>&amp;#code;</c>) so that it is valid XML and renders consistently with how it
/// is measured.
/// </summary>
/// <remarks>
/// The code points match what <see cref="System.Net.WebUtility.HtmlDecode(string)"/>
/// produces for the same name, so the curated fast-path and the full HTML fallback
/// agree. This table is mostly documentation of the "blessed" keywords; any other
/// standard HTML entity still works through the fallback.
/// </remarks>
public static class TextEntities
{
    /// <summary>
    /// Maps an HTML entity name (without the surrounding <c>&amp;</c> and <c>;</c>)
    /// to its Unicode code point.
    /// </summary>
    public static IReadOnlyDictionary<string, int> Entities { get; } = new Dictionary<string, int>
    {
        // Greek lowercase
        ["alpha"] = 945, ["beta"] = 946, ["gamma"] = 947, ["delta"] = 948,
        ["epsilon"] = 949, ["zeta"] = 950, ["eta"] = 951, ["theta"] = 952,
        ["iota"] = 953, ["kappa"] = 954, ["lambda"] = 955, ["mu"] = 956,
        ["nu"] = 957, ["xi"] = 958, ["omicron"] = 959, ["pi"] = 960,
        ["rho"] = 961, ["sigma"] = 963, ["tau"] = 964, ["upsilon"] = 965,
        ["phi"] = 966, ["chi"] = 967, ["psi"] = 968, ["omega"] = 969,

        // Greek uppercase
        ["Alpha"] = 913, ["Beta"] = 914, ["Gamma"] = 915, ["Delta"] = 916,
        ["Epsilon"] = 917, ["Zeta"] = 918, ["Eta"] = 919, ["Theta"] = 920,
        ["Iota"] = 921, ["Kappa"] = 922, ["Lambda"] = 923, ["Mu"] = 924,
        ["Nu"] = 925, ["Xi"] = 926, ["Omicron"] = 927, ["Pi"] = 928,
        ["Rho"] = 929, ["Sigma"] = 931, ["Tau"] = 932, ["Upsilon"] = 933,
        ["Phi"] = 934, ["Chi"] = 935, ["Psi"] = 936, ["Omega"] = 937,

        // Common symbols
        ["ohm"] = 937,      // rendered as a capital Omega
        ["mho"] = 8487,     // rendered as upside down capital Omega
        ["micro"] = 181,    // µ
        ["deg"] = 176,      // °
        ["plusmn"] = 177,   // ±
        ["times"] = 215,    // ×
        ["divide"] = 247,   // ÷
        ["le"] = 8804,      // ≤
        ["ge"] = 8805,      // ≥
        ["ne"] = 8800,      // ≠
        ["approx"] = 8776,  // ≈
        ["infin"] = 8734,   // ∞
        ["radic"] = 8730,   // √
        ["sum"] = 8721,     // ∑
        ["prod"] = 8719,    // ∏
        ["partial"] = 8706, // ∂
        ["nabla"] = 8711,   // ∇
        ["middot"] = 183,   // ·
        ["hellip"] = 8230,  // …

        // Arrows
        ["larr"] = 8592,    // ←
        ["uarr"] = 8593,    // ↑
        ["rarr"] = 8594,    // →
        ["darr"] = 8595,    // ↓
        ["harr"] = 8596,    // ↔
    };
}
