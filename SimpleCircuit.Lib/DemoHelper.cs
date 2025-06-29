using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components;
using SimpleCircuit.Drawing.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit
{
    /// <summary>
    /// Helper class for demonstrating certain aspects of SimpleCircuit.
    /// </summary>
    public static class DemoHelper
    {
        private class VariantCombination : IEquatable<VariantCombination>
        {
            public HashSet<string> Set { get; }

            public VariantCombination(IEnumerable<string> items)
            {
                Set = [.. items];
            }

            public override int GetHashCode()
            {
                int hash = 0;
                foreach (string item in Set)
                    hash ^= item.GetHashCode();
                return hash;
            }

            public override bool Equals(object obj) => obj is VariantCombination vc && Equals(vc);

            public bool Equals(VariantCombination other)
            {
                if (ReferenceEquals(this, other))
                    return true;
                if (Set.Count != other.Set.Count)
                    return false;
                return Set.SetEquals(other.Set);
            }
        }

        /// <summary>
        /// Creates a demo circuit for a given key and drawable factory.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="labels">The labels, or <c>null</c> if the method should try to create labels from metadata.</param>
        /// <returns>The script of the demo page.</returns>
        public static string CreateDemo(string key, IDrawableFactory factory, string[] labels = null)
        {
            var options = new Options();
            var representative = factory.Create(key, key, options, null, null);

            // Remove all variants first (some elements add variants at creation)
            foreach (string v in representative.Variants.ToList())
                representative.Variants.Add(v);

            // Specify labels
            if (labels is null)
            {
                // Try to add labels based on the metadata
                var info = factory.GetType().GetCustomAttributes(false).FirstOrDefault(attr => attr is DrawableAttribute da && da.Key == key) as DrawableAttribute;
                if (info is not null)
                    labels = [.. Enumerable.Range(0, info.LabelCount).Select(n => $"\"label {n + 1}\"")];
                else
                    labels = ["\"label 1\""];
            }
            else
            {
                for (int i = 0; i < labels.Length; i++)
                    labels[i] = $"\"{labels[i]}\"";
            }

            // Each column will represent a styling thing
            List<string> styles = [
                "",
                "color=\"red\"",
                "background=\"red\"",
                "dashed",
                "dotted",
                "color=\"green\", background=\"blue\", dashed, thickness=1",
                "fontfamily=\"Times New Roman\""
                ];

            // Each row represents possible variants
            var ckt = new GraphicalCircuit();
            var builder = new BoundsBuilder(ckt.TextFormatter, ckt.Style, null);
            var context = new PrepareContext(ckt, null)
            {
                Mode = PreparationMode.Reset
            };
            representative.Prepare(context);
            representative.Render(builder);
            var variantSet = new HashSet<VariantCombination>();
            ExploreVariants(representative, context, builder, [], variantSet);
            var variants = variantSet.ToList();

            var sb = new StringBuilder();
            for (int row = 0; row < variants.Count; row++)
            {
                for (int col = 0; col < styles.Count; col++)
                {
                    var style = styles[col];
                    sb.AppendLine($"{key}_{col}_{row}({style} {string.Join(", ", variants[row].Set)} {string.Join(", ", labels)})");
                }

                // Also do a different orientation
                sb.AppendLine($"X <sw 5 color=\"red\"> {key}_{styles.Count}_{row}({string.Join(", ", variants[row].Set)} {string.Join(", ", labels)})");
                sb.AppendLine($"X <nw 5 color=\"red\"> {key}_{styles.Count + 1}_{row}({string.Join(", ", variants[row].Set)} {string.Join(", ", labels)})");
                sb.AppendLine($"X <se 5 color=\"red\"> {key}_{styles.Count + 2}_{row}({string.Join(", ", variants[row].Set)} {string.Join(", ", labels)})");
                sb.AppendLine($"X <ne 5 color=\"red\"> {key}_{styles.Count + 3}_{row}({string.Join(", ", variants[row].Set)} {string.Join(", ", labels)})");
                sb.AppendLine($"(y {key}_*_{row})");
            }
            for (int col = 0; col < styles.Count + 2; col++)
                sb.AppendLine($"(x {key}_{col}_*)");
            return sb.ToString();
        }

        private static void ExploreVariants(IDrawable drawable, IPrepareContext context, IGraphicsBuilder builder, LinkedList<string> variantPath, HashSet<VariantCombination> collected)
        {
            // Just these variables are OK
            collected.Add(new(variantPath));

            foreach (string variant in drawable.Variants.Branches.ToList())
            {
                switch (variant)
                {
                    case Drawable.Dashed:
                    case Drawable.Dotted:
                    case "flip":
                        continue;
                }

                // Start a new item in the path
                drawable.Variants.Add(variant);
                drawable.Variants.Reset();
                foreach (string v in variantPath)
                    drawable.Variants.Add(v);

                drawable.Prepare(context);
                drawable.Render(builder);
                variantPath.AddLast(variant);

                ExploreVariants(drawable, context, builder, variantPath, collected);

                // Remove the item from the path again
                variantPath.RemoveLast();
                drawable.Variants.Remove(variant);
            }
        }
    }
}
