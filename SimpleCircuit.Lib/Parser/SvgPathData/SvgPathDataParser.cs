﻿using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Parser.SvgPathData
{
    public static class SvgPathDataParser
    {
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
        public static void Parse(SvgPathDataLexer lexer, PathBuilder b, IDiagnosticHandler diagnostics)
        {
            while (lexer.Type != TokenType.EndOfContent)
            {
                Vector2 h1, h2, p;
                double d;
                bool result = true;
                if (!lexer.Branch(TokenType.Command, out var cmd))
                {
                    diagnostics?.Post(new TokenDiagnosticMessage(cmd, SeverityLevel.Error, "DW001",
                        $"Could not recognize the SVG path command '{cmd.Content}'"));
                    break;
                }
                switch (cmd.Content.Span[0])
                {
                    case 'M':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.MoveTo(p);
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                            b.LineTo(p);
                        break;
                    case 'm':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.Move(p);
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                            b.Line(p);
                        break;
                    case 'L':
                        if (!lexer.ParseVector(diagnostics, out p))
                        {
                            lexer.Skip(~TokenType.Command);
                            continue;
                        }
                        b.LineTo(p);
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
                        while (lexer.TryParseVector(diagnostics, new(), out p))
                            b.SmoothQuad(p);
                        break;
                    case 'z':
                    case 'Z':
                        b.Close();
                        break;

                    default:
                        diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "DRAW001",
                            $"Could not recognize path command '{cmd}'."));
                        break;
                }
            }
        }
    }
}