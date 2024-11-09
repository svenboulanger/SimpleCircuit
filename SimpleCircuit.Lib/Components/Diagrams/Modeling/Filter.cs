using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("FILT", "A filter", "Modeling", "lowpass highpass bandpass low high band")]
    public class Filter : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var result = new Instance(name);
            result.Variants.Add(ModelingDrawable.Square);
            return result;
        }

        /// <summary>
        /// Creats a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 12.0)
        {
            private const string _graph = "graph";
            private const string _lp = "lowpass";
            private const string _lp2 = "lowpass2";
            private const string _bp = "bandpass";
            private const string _hp = "highpass";
            private const string _hp2 = "highpass2";

            /// <inheritdoc />
            public override string Type => "filter";

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                base.Draw(builder);
                switch (Variants.Select(_graph))
                {
                    case 0:
                        DrawGraphs(builder);
                        break;

                    default:
                        DrawSquigglies(builder);
                        break;
                }
                DrawLabels(builder);
            }

            private void DrawGraphs(IGraphicsBuilder builder)
            {
                double s = Size * 0.25;

                switch (Variants.Select(_lp, _bp, _hp, _lp2, _hp2))
                {
                    default:
                    case 0:
                    case 3:
                        builder.Polyline([
                            new(-s, -s),
                            new(-s, s),
                            new(s, s)
                        ]);
                        builder.Polyline([
                            new(-s, -s * 0.6),
                            new(s * 0.1, -s * 0.6),
                            new(s * 0.6, s)
                        ]);
                        break;

                    case 1:
                        builder.Polyline([
                            new(-s, -s),
                            new(-s, s),
                            new(s, s)
                        ]);
                        builder.Polyline([
                            new(-s, s),
                            new(-s * 0.45, -s * 0.6),
                            new(s * 0.15, -s * 0.6),
                            new(s * 0.6, s)
                        ]);
                        break;

                    case 2:
                    case 4:
                        builder.Polyline([
                            new(-s, -s), 
                            new(-s, s), 
                            new(s, s)
                        ]);
                        builder.Polyline([
                            new(-s, s), 
                            new(-s * 0.1, -s * 0.6), 
                            new(s * 0.6, -s * 0.6)
                        ]);
                        break;
                }
            }

            private void DrawSquigglies(IGraphicsBuilder builder)
            {
                double s = Size * 0.2;
                switch (Variants.Select(_lp, _bp, _hp, _lp2, _hp2))
                {
                    case 0:
                        builder.AC(new(0, -s * 1.5), s);
                        builder.AC(new(0, 0), s);
                        builder.AC(new(0, s * 1.5), s);
                        builder.Line(new(-s * 0.5, -s), new(s * 0.5, -s * 2));
                        builder.Line(new(-s * 0.5, s * 0.5), new(s * 0.5, -s * 0.5));
                        break;

                    case 1:
                        builder.AC(new(0, -s * 1.5), s);
                        builder.AC(new(0, 0), s);
                        builder.AC(new(0, s * 1.5), s);
                        builder.Line(new(-s * 0.5, -s), new(s * 0.5, -s * 2));
                        builder.Line(new(-s * 0.5, s * 2), new(s * 0.5, s));
                        break;

                    case 2:
                        builder.AC(new(0, -s * 1.5), s);
                        builder.AC(new(0, 0), s);
                        builder.AC(new(0, s * 1.5), s);
                        builder.Line(new(-s * 0.5, s * 0.5), new(s * 0.5, -s * 0.5));
                        builder.Line(new(-s * 0.5, s * 2), new(s * 0.5, s));
                        break;

                    case 3:
                        builder.AC(new(0, -s), s);
                        builder.AC(new(0, s), s);
                        builder.Line(new(-s * 0.5, -s * 0.5), new(s * 0.5, -s * 1.5));
                        break;

                    case 4:
                        builder.AC(new(0, -s), s);
                        builder.AC(new(0, s), s);
                        builder.Line(new(-s * 0.5, s * 1.5), new(s * 0.5, s * 0.5));
                        break;

                    default:
                        builder.AC(new(), s);
                        break;
                }
            }
        }
    }
}
