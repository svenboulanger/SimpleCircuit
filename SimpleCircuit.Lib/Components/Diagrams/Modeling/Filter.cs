namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("FILT", "A filter", "Modeling")]
    public class Filter : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var result = new Instance(name);
            result.Variants.Add(ModelingDrawable.Square);
            return result;
        }

        private class Instance : ModelingDrawable, ILabeled
        {
            private const string _graph = "graph";
            private const string _lp = "lowpass";
            private const string _lp2 = "lowpass2";
            private const string _bp = "bandpass";
            private const string _hp = "highpass";
            private const string _hp2 = "highpass2";

            /// <inheritdoc />
            protected override double Size => 12;

            /// <inheritdoc />
            public override string Type => "filter";

            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <summary>
            /// Creats a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name) : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);
                switch (Variants.Select(_graph))
                {
                    case 0:
                        DrawGraphs(drawing);
                        break;

                    default:
                        DrawSquigglies(drawing);
                        break;
                }

                Labels.BoxedLabel(Variants, new(-Size * 0.5, -Size * 0.5), new(Size * 0.5, Size * 0.5), 1, -1, 1);
                Labels.Draw(drawing);
            }

            private void DrawGraphs(SvgDrawing drawing)
            {
                double s = Size * 0.25;

                switch (Variants.Select(_lp, _bp, _hp, _lp2, _hp2))
                {
                    default:
                    case 0:
                    case 3:
                        drawing.Polyline(new Vector2[] { new(-s, -s), new(-s, s), new(s, s) });
                        drawing.Polyline(new Vector2[] { new(-s, -s * 0.6), new(s * 0.1, -s * 0.6), new(s * 0.6, s) });
                        break;

                    case 1:
                        drawing.Polyline(new Vector2[] { new(-s, -s), new(-s, s), new(s, s) });
                        drawing.Polyline(new Vector2[] { new(-s, s), new(-s * 0.45, -s * 0.6), new(s * 0.15, -s * 0.6), new(s * 0.6, s) });
                        break;

                    case 2:
                    case 4:
                        drawing.Polyline(new Vector2[] { new(-s, -s), new(-s, s), new(s, s) });
                        drawing.Polyline(new Vector2[] { new(-s, s), new(-s * 0.1, -s * 0.6), new(s * 0.6, -s * 0.6) });
                        break;
                }
            }

            private void DrawSquigglies(SvgDrawing drawing)
            {
                double s = Size * 0.2;
                switch (Variants.Select(_lp, _bp, _hp, _lp2, _hp2))
                {
                    case 0:
                        drawing.AC(new(0, -s * 1.5), s);
                        drawing.AC(new(0, 0), s);
                        drawing.AC(new(0, s * 1.5), s);
                        drawing.Line(new(-s * 0.5, -s), new(s * 0.5, -s * 2));
                        drawing.Line(new(-s * 0.5, s * 0.5), new(s * 0.5, -s * 0.5));
                        break;

                    case 1:
                        drawing.AC(new(0, -s * 1.5), s);
                        drawing.AC(new(0, 0), s);
                        drawing.AC(new(0, s * 1.5), s);
                        drawing.Line(new(-s * 0.5, -s), new(s * 0.5, -s * 2));
                        drawing.Line(new(-s * 0.5, s * 2), new(s * 0.5, s));
                        break;

                    case 2:
                        drawing.AC(new(0, -s * 1.5), s);
                        drawing.AC(new(0, 0), s);
                        drawing.AC(new(0, s * 1.5), s);
                        drawing.Line(new(-s * 0.5, s * 0.5), new(s * 0.5, -s * 0.5));
                        drawing.Line(new(-s * 0.5, s * 2), new(s * 0.5, s));
                        break;

                    case 3:
                        drawing.AC(new(0, -s), s);
                        drawing.AC(new(0, s), s);
                        drawing.Line(new(-s * 0.5, -s * 0.5), new(s * 0.5, -s * 1.5));
                        break;

                    case 4:
                        drawing.AC(new(0, -s), s);
                        drawing.AC(new(0, s), s);
                        drawing.Line(new(-s * 0.5, s * 1.5), new(s * 0.5, s * 0.5));
                        break;

                    default:
                        drawing.AC(new(), s);
                        break;
                }
            }
        }
    }
}
