﻿using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A parameter definition.
    /// </summary>
    public record ParameterDefinitionNode : SyntaxNode
    {
        /// <summary>
        /// The token of the parameter name.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name => Token.Content.ToString();

        /// <summary>
        /// Gets the value.
        /// </summary>
        public SyntaxNode Value { get; }

        /// <summary>
        /// Creates a new <see cref="ParameterDefinitionNode"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
        public ParameterDefinitionNode(Token token, SyntaxNode value)
            : base(token.Location)
        {
            Token = token;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Name} = {Value}";
    }
}
