using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Modeling
{
    [Drawable("FILT", "A filter", "Modeling")]
    public class Filter : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
        {
            var result = new Instance(name);
            result.Variants.Add(ModelingDrawable.Square);
            return result;
        }

        private class Instance : ModelingDrawable
        {
            private const string _lp = "lowpass";
            private const string _lp2 = "lowpass2";
            private const string _bp = "bandpass";
            private const string _hp = "highpass";
            private const string _hp2 = "highpass2";

            protected override double Size => 12;

            /// <summary>
            /// Creats a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name) : base(name)
            {
            }

            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);

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
                }
            }
        }
    }
}
