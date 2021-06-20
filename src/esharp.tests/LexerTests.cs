using System;
using System.Collections.Generic;
using System.Linq;
using esharp.solidity.compiler.Syntax;
using Xunit;

namespace esharp.tests
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Lexes_UnterminatedString()
        {
            var text = "\"text";
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringToken, token.Kind);
            Assert.Equal(text, token.Text);

            // var diagnostic = Assert.Single(diagnostics);
            // Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
            // Assert.Equal("Unterminated string literal.", diagnostic.Message);
        }

        [Fact]
        public void Lexer_Should_Get_Contract_Token()
        {
            var text = "Contract";
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringToken, token.Kind);
            Assert.Equal(text, token.Text);

            // Assert.Equal(4, tokens.Count());
            // Assert.Equal(SyntaxKind.StringToken, token.Kind);
            // Assert.Equal(text, token.Text);

            // var diagnostic = Assert.Single(diagnostics);
            // Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
            // Assert.Equal("Unterminated string literal.", diagnostic.Message);
        }
    }
}
