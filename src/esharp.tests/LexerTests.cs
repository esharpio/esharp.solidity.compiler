using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using esharp.solidity.compiler.Syntax;
using Newtonsoft.Json.Linq;
using Xunit;

namespace esharp.tests
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Lexes_UnterminatedString()
        {
            var text = "text";
            // var tokens = SyntaxTree.ParseTokens(text);

            // var token = Assert.Single(tokens);
            // Assert.Equal(SyntaxKind.StringToken, token.Kind);
            // Assert.Equal(text, token.Text);

            // var diagnostic = Assert.Single(diagnostics);
            // Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
            // Assert.Equal("Unterminated string literal.", diagnostic.Message);
        }

        [Fact]
        public void Lexer_Should_Get_Contract_Token()
        {
            var text = "contract";
            // var tokens = SyntaxTree.ParseTokens(text);

            // var token = Assert.Single(tokens);
            // Assert.Equal(SyntaxKind.ContractKeyword, token.Kind);
            // Assert.Equal(text, token.Text);
        }

        [Fact]
        public void Lexer_Should_Get_Interface_Token()
        {
            var text = "interface";
            // var tokens = SyntaxTree.ParseTokens(text);

            // var token = Assert.Single(tokens);
            // Assert.Equal(SyntaxKind.InterfaceKeyword, token.Kind);
            // Assert.Equal(text, token.Text);
        }

        [Fact]
        public void Lexer_Should_Get_Contract_Tokens()
        {
            var text = "contract Greeter {";
            // var tokens = SyntaxTree.ParseTokens(text).ToList();

            // Assert.Equal(5, tokens.Count());
            // Assert.Equal(SyntaxKind.ContractKeyword, tokens[0].Kind);
        }
        
        [Fact]
        public void Lexer_Should_Simple_Addition_Tokens()
        {
            var text = "a + b";
            // var tokens = SyntaxTree.ParseTokens(text);
            // Assert.Equal(5, tokens.Count());
            // var token = Assert.Single(tokens);
            // Assert.Equal(SyntaxKind.ContractKeyword, token.Kind);
            // Assert.Equal(text, token.Text);
        }
        
        [Fact]
        public void Lexer_Should_Sol_File()
        {
            foreach (string line in System.IO.File.ReadLines(@"./Contracts/4_Adder.sol"))
            {  
                System.Console.WriteLine(line);  
                // var tokens = SyntaxTree.ParseTokens(line);
                // Assert.Equal(5, tokens.Count());
                // var token = Assert.Single(tokens);
                // Assert.Equal(SyntaxKind.ContractKeyword, token.Kind);
                // Assert.Equal(text, token.Text);
                
                // Console.WriteLine(tokens);
            }
        }
        
        [Fact]
        public void Lexer_Should_Get_Contract_Tokens_From_File()
        {
            var contract = File.ReadAllText("Contracts/4_Adder_ByteCode.json");
            var myJObject = JObject.Parse(contract);
            Console.WriteLine(myJObject.SelectToken("object").Value<string>());

            string op_codes = myJObject.SelectToken("object").Value<string>();
            // var tokens = SyntaxTree.ParseTokens(op_codes).ToList();

            // Assert.Equal(6, tokens.Count());
            // Assert.Equal(SyntaxKind.ContractKeyword, tokens[1].Kind);
        }
    }
}
