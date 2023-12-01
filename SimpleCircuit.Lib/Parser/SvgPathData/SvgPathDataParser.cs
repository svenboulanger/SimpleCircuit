using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System.Collections.Generic;

namespace SimpleCircuit.Parser.SvgPathData
{
    /// <summary>
    /// A parser for SVG path data.
    /// </summary>
    public static class SvgPathDataParser
    {
        public readonly struct MarkerLocation(Vector2 location, Vector2 normal)
        {
            public Vector2 Location { get; } = location;
            public Vector2 Normal { get; } = normal;
        }

        /// <summary>
        /// Parses a series of points.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <returns>The list of points.</returns>
        public static List<Vector2> ParsePoints(SvgPathDataLexer lexer, IDiagnosticHandler diagnostics)
        {
            var points = new List<Vector2>();
            while (lexer.Type != TokenType.EndOfContent)
            {
                // Keep parsing vectors
                lexer.ParseVector(diagnostics, out var p);
                points.Add(p);
            }
            return points;
        }

        /// <summary>
        /// Parse Svg path.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="b">The path builder.</param>
        /// <param name="diagnostics">The diagnostics message handler.</param>
        public static MarkerLocation Parse(SvgPathDataLexer lexer, PathBuilder b, IDiagnosticHandler diagnostics)
        {
            bool isFirstDrawn = true;
            Vector2 startLocation = new(), startNormal = new(1, 0);
            void StoreStart()
            {
                if (isFirstDrawn)
                {
                    isFirstDrawn = false;
                    startLocation = b.Start;
                    startNormal = -b.StartNormal;
                }
            }
            while (lexer.Type != TokenType.EndOfContent)
            {
                Vector2 h1, h2, p;
                double d;
                bool result = true;
                if (!lexer.Branch(TokenType.Command, out var cmd))
                {
                    diagnostics?.Post(lexer.Token, ErrorCodes.CouldNotRecognizePathCommand, lexer.Token.Content.ToString());
                    break;
                }
                switch (cmd.Content.Span[0])
                {
                    case 'A':
                        if (!lexer.ParseCoordinate(diagnostics, out double rx) ||
                            !lexer.ParseCoordinate(diagnostics, out double ry) ||
                            !lexer.ParseCoordinate(diagnostics, out double angle) ||
                            !lexer.ParseCoordinate(diagnostics, out double largeArc) ||
                            !lexer.ParseCoordinate(diagnostics, out double sweepFlag) ||
                            !lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.ArcTo(rx, ry, angle, largeArc != 0, sweepFlag != 0, p);
                        StoreStart();
                        break;

                    case 'a':
                        if (!lexer.ParseCoordinate(diagnostics, out rx) ||
                            !lexer.ParseCoordinate(diagnostics, out ry) ||
                            !lexer.ParseCoordinate(diagnostics, out angle) ||
                            !lexer.ParseCoordinate(diagnostics, out largeArc) ||
                            !lexer.ParseCoordinate(diagnostics, out sweepFlag) ||
                            !lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Arc(rx, ry, angle, largeArc != 0, sweepFlag != 0, p);
                        StoreStart();
                        break;

                    case 'M':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.MoveTo(p);
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                        {
                            b.LineTo(p);
                            StoreStart();
                        }
                        break;
                    case 'm':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Move(p);
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                        {
                            b.Line(p);
                            StoreStart();
                        }
                        break;
                    case 'L':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.LineTo(p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                            b.LineTo(p);
                        break;
                    case 'l':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Line(p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                            b.Line(p);
                        break;
                    case 'H':
                        if (!lexer.ParseCoordinate(diagnostics, out d))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.HorizontalTo(d);
                        StoreStart();
                        while (lexer.TryParseCoordinate(diagnostics, 0.0, out d))
                            b.HorizontalTo(d);
                        break;
                    case 'h':
                        if (!lexer.ParseCoordinate(diagnostics, out d))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Horizontal(d);
                        StoreStart();
                        while (lexer.TryParseCoordinate(diagnostics, 0.0, out d))
                            b.Horizontal(d);
                        break;
                    case 'V':
                        if (!lexer.ParseCoordinate(diagnostics, out d))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.VerticalTo(d);
                        StoreStart();
                        while (lexer.TryParseCoordinate(diagnostics, 0.0, out d))
                            b.VerticalTo(d);
                        break;
                    case 'v':
                        if (!lexer.ParseCoordinate(diagnostics, out d))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Vertical(d);
                        StoreStart();
                        while (lexer.TryParseCoordinate(diagnostics, 0.0, out d))
                            b.Vertical(d);
                        break;
                    case 'C':
                        result &= lexer.ParseVector(diagnostics, out h1);
                        result &= lexer.ParseVector(diagnostics, out h2);
                        result &= lexer.ParseVector(diagnostics, out p);
                        if (!result)
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.CurveTo(h1, h2, p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out h1) &&
                            lexer.ParseVector(diagnostics, out h2) &&
                            lexer.ParseVector(diagnostics, out p))
                            b.CurveTo(h1, h2, p);
                        break;
                    case 'c':
                        result &= lexer.ParseVector(diagnostics, out h1);
                        result &= lexer.ParseVector(diagnostics, out h2);
                        result &= lexer.ParseVector(diagnostics, out p);
                        if (!result)
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Curve(h1, h2, p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out h1) &&
                            lexer.ParseVector(diagnostics, out h2) &&
                            lexer.ParseVector(diagnostics, out p))
                            b.Curve(h1, h2, p);
                        break;
                    case 'S':
                        result &= lexer.ParseVector(diagnostics, out h2);
                        result &= lexer.ParseVector(diagnostics, out p);
                        if (!result)
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.SmoothTo(h2, p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out h2) &&
                            lexer.ParseVector(diagnostics, out p))
                            b.SmoothTo(h2, p);
                        break;
                    case 's':
                        result &= lexer.ParseVector(diagnostics, out h2);
                        result &= lexer.ParseVector(diagnostics, out p);
                        if (!result)
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Smooth(h2, p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out h2) &&
                            lexer.ParseVector(diagnostics, out p))
                            b.Smooth(h2, p);
                        break;
                    case 'Q':
                        result &= lexer.ParseVector(diagnostics, out h1);
                        result &= lexer.ParseVector(diagnostics, out p);
                        if (!result)
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.QuadCurveTo(h1, p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out h1) &&
                            lexer.ParseVector(diagnostics, out p))
                            b.QuadCurveTo(h1, p);
                        break;
                    case 'q':
                        result &= lexer.ParseVector(diagnostics, out h1);
                        result &= lexer.ParseVector(diagnostics, out p);
                        if (!result)
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.QuadCurve(h1, p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out h1) &&
                            lexer.ParseVector(diagnostics, out p))
                            b.QuadCurve(h1, p);
                        break;
                    case 'T':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.SmoothQuadTo(p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                            b.SmoothQuadTo(p);
                        break;
                    case 't':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.SmoothQuad(p);
                        StoreStart();
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                            b.SmoothQuad(p);
                        break;
                    case 'z':
                    case 'Z':
                        b.Close();
                        break;

                    default:
                        diagnostics?.Post(cmd, ErrorCodes.CouldNotRecognizePathCommand, cmd.Content.ToString());
                        break;
                }
            }
            return new MarkerLocation(startLocation, startNormal);
        }
    }
}
