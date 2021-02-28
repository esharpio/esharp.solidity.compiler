using System;

namespace esharp.solidity.compiler
{
    public class Lexer
    {
        private int _position;

        private readonly string _text;

        public Lexer(string text)
        {
            _text = text;
            _position = 0;
        }

        public void Next()
        {
            _position++;
        }
    }
}