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
            var text = "text";
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
            var text = "contract";
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.ContractKeyword, token.Kind);
            Assert.Equal(text, token.Text);
        }

        [Fact]
        public void Lexer_Should_Get_Interface_Token()
        {
            var text = "interface";
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.InterfaceKeyword, token.Kind);
            Assert.Equal(text, token.Text);
        }

        [Fact]
        public void Lexer_Should_Get_Contract_Tokens()
        {
            var text = "contract Greeter {";
            var tokens = SyntaxTree.ParseTokens(text).ToList();

            Assert.Equal(6, tokens.Count());
            Assert.Equal(SyntaxKind.ContractKeyword, tokens[1].Kind);
        }
        
        [Fact]
        public void Lexer_Should_Simple_Addition_Tokens()
        {
            var text = "a + b";
            var tokens = SyntaxTree.ParseTokens(text);
            Assert.Equal(5, tokens.Count());
            // var token = Assert.Single(tokens);
            // Assert.Equal(SyntaxKind.ContractKeyword, token.Kind);
            // Assert.Equal(text, token.Text);
        }
    }
}
