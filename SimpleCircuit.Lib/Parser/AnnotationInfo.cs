﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components;
using SimpleCircuit.Components.Annotations;
using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Information for annotations.
    /// </summary>
    public class AnnotationInfo : IDrawableInfo
    {
        private IAnnotation _annotation;

        /// <inheritdoc />
        public Token Source { get; }

        /// <inheritdoc />
        public string Fullname { get; }

        /// <inheritdoc />
        public IList<Token> Labels { get; } = new List<Token>(2);

        /// <inheritdoc />
        public IList<VariantInfo> Variants { get; } = new List<VariantInfo>();

        /// <inheritdoc />
        public IDictionary<Token, object> Properties { get; } = new Dictionary<Token, object>();

        /// <summary>
        /// Creates a new <see cref="AnnotationInfo"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="fullname">The full name.</param>
        public AnnotationInfo(Token source, string fullname)
        {
            Source = source;
            Fullname = fullname;
            _annotation = null;
        }

        /// <summary>
        /// Gets or creates the annotation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the annotation, or <c>null</c> if it failed.</returns>
        public IAnnotation GetOrCreate(ParsingContext context)
        {
            if (_annotation != null)
                return _annotation;

            if (context.Circuit.TryGetValue(Fullname, out var presence))
            {
                if (presence is IAnnotation annotation)
                    _annotation = annotation;
                else
                {
                    context.Diagnostics?.Post(ErrorCodes.AnnotationComponentAlreadyExists);
                    return null;
                }
            }
            else
            {
                _annotation = new Box(Fullname);
                context.Circuit.Add(_annotation);
            }

            // Handle the labels
            if (Labels.Count > 0 && _annotation is ILabeled labeled)
            {
                if (Labels.Count > labeled.Labels.Maximum)
                {
                    context.Diagnostics?.Post(Labels[labeled.Labels.Maximum - 1], ErrorCodes.TooManyLabels);
                    for (int i = 0; i < labeled.Labels.Maximum; i++)
                        labeled.Labels[i] = Labels[i].Content[1..^1].ToString();
                }
                else
                {
                    for (int i = 0; i < Labels.Count; i++)
                        labeled.Labels[i] = Labels[i].Content[1..^1].ToString();
                }
            }

            // Handle variants
            foreach (var variant in Variants)
            {
                if (variant.Include)
                    _annotation.Variants.Add(variant.Name);
                else
                    _annotation.Variants.Remove(variant.Name);
            }

            // Handle properties
            foreach (var property in Properties)
                _annotation.SetProperty(property.Key, property.Value, context.Diagnostics);
            return _annotation;
        }

        /// <summary>
        /// Gets an annotation, potentially using a context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the annotation; or <c>null</c> if the annotation doesn't exist.</returns>
        public IAnnotation Get(IPrepareContext context)
        {
            if (_annotation != null)
                return _annotation;

            _annotation = context.Find(Fullname) as IAnnotation;
            if (_annotation == null)
                context.Diagnostics?.Post(Source, ErrorCodes.CouldNotFindDrawable, Fullname);
            return _annotation;
        }

        /// <inheritdoc />
        public override string ToString() => Fullname;
    }
}