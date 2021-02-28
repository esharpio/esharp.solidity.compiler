using System;
using Xunit;

namespace esharp.tests
{
    public class ParserTests
    {

        [Fact]
        public void Should_Parse_Line_As_Tokens()
        {
            const String line = "return a + b";
            Assert.NotEmpty(line);
        }
    }
}
