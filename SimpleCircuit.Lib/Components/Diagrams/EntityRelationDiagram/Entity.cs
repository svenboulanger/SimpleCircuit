using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
    [Drawable("ENT", "An entity-relationship diagram entity. Any label after the first will be added as an attribute.", "ERD", "box rectangle")]
    public class Entity : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : LocatedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <inheritdoc />
            public override string Type => "entity";

            /// <inheritdoc />
            protected override IEnumerable<string> GroupClasses => new[] { "diagram" };

            /// <summary>
            /// Gets or sets the width of the entity block.
            /// </summary>
            [Description("The width of the entity block.")]
            [Alias("w")]
            public double Width { get; set; } = 30;

            [Description("The height of a line for attributes.")]
            [Alias("lh")]
            public double LineHeight { get; set; } = 8;

            /// <summary>
            /// Gets the height of the entity block (only valid after <see cref="Reset(IDiagnosticHandler)"/>).
            /// </summary>
            protected double Height { get; private set; }

            /// <summary>
            /// Creates a new entity.
            /// </summary>
            /// <param name="name">The name of the entity.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                Pins.Clear();
                double w = Width * 0.5;
                if (Labels.Count <= 1)
                {
                    Height = LineHeight * 2;
                    Pins.Add(new FixedOrientedPin("left", "The left pin", this, new(-w, 0), new(-1, 0)), "l", "w", "left");
                    Pins.Add(new FixedOrientedPin("top", "The top pin", this, new(0, -LineHeight), new(0, -1)), "t", "n", "top");
                    Pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, new(0, LineHeight), new(0, 1)), "b", "s", "bottom");
                    Pins.Add(new FixedOrientedPin("right", "The right pin", this, new(w, 0), new(1, 0)), "r", "e", "right");
                }
                else
                {
                    // We have a header and attributes
                    Height = LineHeight * Labels.Count;
                    Pins.Add(new FixedOrientedPin("left", "The left pin", this, new(-w, 0), new(-1, 0)), "l", "w", "left");
                    Pins.Add(new FixedOrientedPin("top", "The top pin", this, new(0, -LineHeight * 0.5), new(0, -1)), "t", "n", "top");
                    Pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, new(0, -LineHeight * 0.5 + Height), new(0, 1)), "b", "s", "bottom");

                    // Add pins for all the attributes
                    for (int i = 1; i < Labels.Count; i++)
                    {
                        Pins.Add(new FixedOrientedPin($"attribute {i} left", $"The left pin for attribute {i}.", this, new(-w, i * LineHeight), new(-1, 0)), $"l{i}", $"w{i}", $"left{i}");
                        Pins.Add(new FixedOrientedPin($"attribute {i} right", $"The right pin of attribute {i}.", this, new(w, i * LineHeight), new(1, 0)), $"r{i}", $"e{i}", $"right{i}");
                    }
                    Pins.Add(new FixedOrientedPin("right", "The right pin", this, new(w, 0), new(1, 0)), "r", "e", "right");
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                int count = Math.Max(Labels.Count, 1);
                var anchors = new CustomLabelAnchorPoints(new LabelAnchorPoint[count]);
                anchors[0] = new LabelAnchorPoint(new(), new(), new("header"));
                if (Labels.Count <= 1)
                {
                    drawing.Rectangle(-Width * 0.5, -Height * 0.5, Width, Height, options: new("erd"));
                }
                else
                {
                    double w = Width * 0.5;
                    drawing.Rectangle(-Width * 0.5, (Height - LineHeight) * 0.5 - Height * 0.5, Width, Height);
                    drawing.Line(new(-w, LineHeight * 0.5), new(w, LineHeight * 0.5));

                    for (int i = 1; i < Labels.Count; i++)
                        anchors[i] = new LabelAnchorPoint(new(-w + 2.0, i * LineHeight), new(1, 0), new("attribute"));
                }
                anchors.Draw(drawing, this);
            }
        }
    }
}
