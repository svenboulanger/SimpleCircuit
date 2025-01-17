﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Annotations;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Information for annotations.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="AnnotationInfo"/>.
    /// </remarks>
    /// <param name="source">The source.</param>
    /// <param name="fullname">The full name.</param>
    public class AnnotationInfo(Token source, string fullname) : IDrawableInfo
    {
        private IAnnotation _annotation = null;

        /// <summary>
        /// The key used for referring to annotations
        /// </summary>
        public const string Key = "annotation";

        /// <inheritdoc />
        public Token Source { get; } = source;

        /// <inheritdoc />
        public string Fullname { get; } = fullname;

        /// <inheritdoc />
        public IList<Token> Labels { get; } = new List<Token>(2);

        /// <inheritdoc />
        public IList<VariantInfo> Variants { get; } = new List<VariantInfo>();

        /// <inheritdoc />
        public IDictionary<Token, object> Properties { get; } = new Dictionary<Token, object>();

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
                context.Options.Apply(Key, _annotation, context.Diagnostics);
                context.Circuit.Add(_annotation);
            }

            // Handle the labels
            if (Labels.Count > 0)
            {
                for (int i = 0; i < Labels.Count; i++)
                    _annotation.Labels[i].Value = Labels[i].Content[1..^1].ToString();
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
