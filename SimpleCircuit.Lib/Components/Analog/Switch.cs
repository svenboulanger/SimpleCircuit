using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A switch.
    /// </summary>
    [SimpleKey("S", "A (voltage-controlled) switch. The controlling pin is optional.", Category = "Analog")]
    public class Switch : ScaledOrientedDrawable, ILabeled
    {
        [Flags]
        public enum MyVariants
        {
            [Description("An open switch.")]
            Open = 0x00,

            [Description("A closed switch.")]
            Closed = 0x01,

            [Description("A push switch.")]
            Push = 0x02,

            [Description("Display an illuminator light.")]
            Lamp = 0x04,

            [Description("The switch is a toggling switch.")]
            Toggling = 0x08,

            [Description("A double throw switch.")]
            DoublePole = 0x10,

            [Description("Onewire representation.")]
            OneWire = 0x20,

            [Description("Windowed push button.")]
            Window = 0x40,
        }

        /// <inheritdoc/>
        [Description("The label next to the switch.")]
        public string Label { get; set; }

        [Description("The number of poles on the switch.")]
        public int Poles { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Switch"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Switch(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
            Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, -6), new(0, -1)), "c", "ctrl");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");
            if (options.ElectricalInstallation)
                Variants.Add("eic");
        }

        protected override void Draw(SvgDrawing drawing)
        {
            if (Variants.Contains("eic"))
            {
                if (Variants.Contains("push"))
                    DrawOneWirePushSwitch(drawing, Variants.Contains("lamp"), Variants.Contains("window"));
                else
                    DrawOneWireSwitch(drawing, Variants.Contains("toggle"), Variants.Contains("double"), Variants.Contains("lamp"));
            }
            else
            {
                if (Variants.Contains("push"))
                    DrawPushSwitch(drawing, Variants.Contains("closed"));
                else
                    DrawRegularSwitch(drawing, Variants.Contains("closed"));
            }
        }

        private void DrawRegularSwitch(SvgDrawing drawing, bool closed)
        {
            // Switch terminals
            drawing.Circle(new Vector2(-5, 0), 1);
            drawing.Circle(new Vector2(5, 0), 1);

            // Controlling pin (optional)
            if (Pins["c"].Connections > 0)
            {
                if (closed)
                    drawing.Line(new(), new(0, -6));
                else
                    drawing.Line(new(0, -2), new(0, -6));
            }

            if (closed)
                drawing.Line(new(-4, 0), new(4, 0));
            else
                drawing.Line(new(-4, 0), new(4, -4));

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, 6), new Vector2(0, 1));
        }
        private void DrawPushSwitch(SvgDrawing drawing, bool closed)
        {
            // Switch terminals
            drawing.Circle(new Vector2(-5, 0), 1);
            drawing.Circle(new Vector2(5, 0), 1);

            if (closed)
            {
                drawing.Line(new(0, 0), new(0, -6), new("wire"));
                drawing.Line(new(-4, 0), new(4, 0));
            }
            else
            {
                drawing.Line(new(0, -4), new(0, -6), new("wire"));
                drawing.Line(new(-5, -4), new(5, -4));
            }

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, 6), new Vector2(0, 1));
        }
        private void DrawOneWirePushSwitch(SvgDrawing drawing, bool lamp, bool window)
        {
            drawing.Circle(new(), 4);
            drawing.Circle(new(), 2);

            if (lamp)
            {
                double x = 2 / Math.Sqrt(2);
                drawing.Segments(new Vector2[]
                {
                    new(-x, -x), new(x, x),
                    new(-x, x), new(x, -x)
                }, new("lamp"));
            }

            if (window)
            {
                drawing.Polyline(new Vector2[]
                {
                    new(-6.5, 2.5), new(-4.5, 2.5), new(-4.5, 6), new(4.5, 6), new(4.5, 2.5), new(6.5, 2.5)
                }, new("window"));
            }

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -5), new Vector2(0, -1));
        }
        private void DrawOneWireSwitch(SvgDrawing drawing, bool toggling, bool doublePole, bool lamp)
        {
            double length = Math.Max(8, 5 + Math.Max(1, Poles) * 2);
            drawing.Circle(new(), 2);
            var n = Vector2.Normal(-Math.PI * 0.37);
            drawing.Line(n * 2, n * length);
            if (toggling)
                drawing.Line(-n * 2, -n * length);
            if (doublePole)
            {
                Vector2 np = new(-n.X, n.Y);
                drawing.Line(np * 2, np * length);
                if (toggling)
                    drawing.Line(-np * 2, -np * length);
            }

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -length), new Vector2(0, -1));

            // Small cross for illuminator lamps
            if (lamp)
            {
                double x = 2.0 / Math.Sqrt(2.0);
                drawing.Segments(new Vector2[]
                {
                            new(-x, -x), new(x, x),
                            new(-x, x), new(x, -x)
                });
            }

            // Draw the poles
            if (Poles > 0)
            {
                var p = n.Perpendicular;
                var pts = new List<Vector2>(Poles * 2);
                for (int i = 0; i < Poles; i++)
                {
                    // Draw from out to in
                    var r = n * length;
                    pts.Add(r);
                    pts.Add(r + p * 3);
                    length -= 2.0;
                }
                drawing.Segments(pts);
                if (toggling)
                    drawing.Segments(pts.Select(v => -v));
                if (doublePole)
                {
                    drawing.Segments(pts.Select(v => new Vector2(-v.X, v.Y)));
                    if (toggling)
                        drawing.Segments(pts.Select(v => new Vector2(v.X, -v.Y)));
                }
            }
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            if (!Variants.Contains("eic"))
            {
                ((FixedOrientedPin)Pins[0]).Offset = new(-6, 0);
                ((FixedOrientedPin)Pins[2]).Offset = new(6, 0);
            }
            else if (Variants.Contains("push"))
            {
                ((FixedOrientedPin)Pins[0]).Offset = new(-4, 0);
                ((FixedOrientedPin)Pins[2]).Offset = new(4, 0);
            }
            else
            {
                ((FixedOrientedPin)Pins[0]).Offset = new(-2, 0);
                ((FixedOrientedPin)Pins[2]).Offset = new(2, 0);
            }
            base.DiscoverNodeRelationships(context, diagnostics);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Switch {Name}";
    }
}
