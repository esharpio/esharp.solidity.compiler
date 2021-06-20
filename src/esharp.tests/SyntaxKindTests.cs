using System;
using System.Collections.Generic;
using System.Linq;
using esharp.solidity.compiler.Syntax;
using Xunit;

namespace esharp.tests
{
    public class SyntaxKindTests
    {
        [Fact]
        public void Lexer_Should_Get_Keyword_Contract_Kind()
        {
            String text = "contract";
            SyntaxKind kind = SyntaxFacts.GetKeywordKind(text);

            Assert.Equal(SyntaxKind.ContractKeyword, kind);
        }

        // // OpenBraceToken
        // [Fact]
        // public void Lexer_Should_Get_Keyword_OpenBrace_Kind()
        // {
        //     String text = "{";
        //     SyntaxKind kind = SyntaxFacts.GetKeywordKind(text);

        //     Assert.Equal(SyntaxKind.OpenBraceToken, kind);
        // }

        [Fact]
        public void Lexer_Should_Get_Keyword_Pragma_Kind()
        {
            String text = "pragma";
            SyntaxKind kind = SyntaxFacts.GetKeywordKind(text);

            Assert.Equal(SyntaxKind.PragmaKeyword, kind);
        }

        [Fact]
        public void Lexer_Should_Get_Keyword_Pure_Kind()
        {
            String text = "pure";
            SyntaxKind kind = SyntaxFacts.GetKeywordKind(text);

            Assert.Equal(SyntaxKind.PureKeyword, kind);
        }

        [Fact]
        public void Lexer_Should_Get_Keyword_View_Kind()
        {
            String text = "view";
            SyntaxKind kind = SyntaxFacts.GetKeywordKind(text);

            Assert.Equal(SyntaxKind.ViewKeyword, kind);
        }
    }
}
