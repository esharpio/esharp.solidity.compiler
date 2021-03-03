using System;
using System.Collections.Immutable;

namespace esharp.solidity.compiler
{
    public class Lexer
    {
        private int _position;

        private readonly string _text;

        public readonly IImmutableList<String> tokens;

        private char Current => Peek(0);

        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }
        
        public Lexer(string text)
        {
            _text = text;
            _position = 0;

            // tokens = new ImmutableList<String>();
        }



        public void Next()
        {
            _position++;
        }
    }
}