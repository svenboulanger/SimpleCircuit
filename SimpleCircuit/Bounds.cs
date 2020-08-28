using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit
{
    public class Bounds
    {
        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Top { get; private set; }
        public double Bottom { get; private set; }
        public double Width => Math.Abs(Right - Left);
        public double Height => Math.Abs(Bottom - Top);

        public void Expand(double x, double y, double extra = 1)
        {
            Left = Math.Min(x - extra, Left);
            Right = Math.Max(x + extra, Right);
            Bottom = Math.Max(y + extra, Bottom);
            Top = Math.Min(y - extra, Top);
        }
        public void Expand(Vector2 vector, double extra = 1)
            => Expand(vector.X, vector.Y, extra);
    }
}
