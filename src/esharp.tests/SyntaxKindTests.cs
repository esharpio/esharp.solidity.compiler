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
        public void Lexer_Should_Get_Keyword_Kinds()
        {
            var text = "contract";
            var kind = SyntaxFacts.GetKeywordKind(text);

            Assert.Equal(SyntaxKind.ContractKeyword, kind);
        }
    }
}
