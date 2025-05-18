using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace esharp.solidity.compiler
{
    /// <summary>
    /// Lexer for Solidity - converts source code into tokens
    /// </summary>
    public class SolidityLexer
    {
        private readonly string _source;
        private int _position;
        private int _line;
        private int _column;

        private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            { "contract", TokenType.Contract },
            { "function", TokenType.Function },
            { "public", TokenType.Public },
            { "private", TokenType.Private },
            { "internal", TokenType.Internal },
            { "external", TokenType.External },
            { "view", TokenType.View },
            { "pure", TokenType.Pure },
            { "payable", TokenType.Payable },
            { "memory", TokenType.Memory },
            { "storage", TokenType.Storage },
            { "calldata", TokenType.Calldata },
            { "struct", TokenType.Struct },
            { "mapping", TokenType.Mapping },
            { "if", TokenType.If },
            { "else", TokenType.Else },
            { "while", TokenType.While },
            { "for", TokenType.For },
            { "return", TokenType.Return },
            { "require", TokenType.Require },
            { "uint", TokenType.Uint },
            { "int", TokenType.Int },
            { "bool", TokenType.Bool },
            { "address", TokenType.Address },
            { "string", TokenType.String },
            { "bytes", TokenType.Bytes },
            { "true", TokenType.BoolLiteral },
            { "false", TokenType.BoolLiteral }
        };

        public SolidityLexer(string source)
        {
            _source = source;
            _position = 0;
            _line = 1;
            _column = 1;
        }

        /// <summary>
        /// Returns the current character or \0 if at the end of source
        /// </summary>
        private char Current => _position < _source.Length ? _source[_position] : '\0';

        /// <summary>
        /// Advances the position and returns the character
        /// </summary>
        private char Advance()
        {
            char current = Current;
            _position++;
            _column++;

            if (current == '\n')
            {
                _line++;
                _column = 1;
            }

            return current;
        }

        /// <summary>
        /// Looks ahead at the next character without advancing
        /// </summary>
        private char Peek()
        {
            if (_position + 1 >= _source.Length)
                return '\0';
            return _source[_position + 1];
        }

        /// <summary>
        /// Checks if the current character matches expected and advances if it does
        /// </summary>
        private bool Match(char expected)
        {
            if (Current != expected)
                return false;

            Advance();
            return true;
        }

        /// <summary>
        /// Skips whitespace and comments
        /// </summary>
        private void SkipWhitespaceAndComments()
        {
            while (true)
            {
                char c = Current;

                if (char.IsWhiteSpace(c))
                {
                    Advance();
                }
                else if (c == '/' && Peek() == '/')
                {
                    // Skip single-line comment
                    Advance(); // Skip first /
                    Advance(); // Skip second /

                    while (Current != '\n' && Current != '\0')
                        Advance();
                }
                else if (c == '/' && Peek() == '*')
                {
                    // Skip multi-line comment
                    Advance(); // Skip /
                    Advance(); // Skip *

                    while (!(Current == '*' && Peek() == '/') && Current != '\0')
                    {
                        Advance();
                    }

                    if (Current != '\0')
                    {
                        Advance(); // Skip *
                        Advance(); // Skip /
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Processes an identifier or keyword
        /// </summary>
        private Token Identifier()
        {
            int startColumn = _column;
            StringBuilder sb = new StringBuilder();

            while (char.IsLetterOrDigit(Current) || Current == '_')
            {
                sb.Append(Advance());
            }

            string text = sb.ToString();

            // Check if this is a keyword
            if (_keywords.TryGetValue(text, out TokenType type))
            {
                return new Token(type, text, _line, startColumn);
            }

            return new Token(TokenType.Identifier, text, _line, startColumn);
        }

        /// <summary>
        /// Processes a number literal
        /// </summary>
        private Token Number()
        {
            int startColumn = _column;
            StringBuilder sb = new StringBuilder();

            // Check for hex literal
            if (Current == '0' && (Peek() == 'x' || Peek() == 'X'))
            {
                sb.Append(Advance()); // 0
                sb.Append(Advance()); // x

                if (!IsHexDigit(Current))
                {
                    return new Token(TokenType.Invalid, sb.ToString(), _line, startColumn);
                }

                while (IsHexDigit(Current))
                {
                    sb.Append(Advance());
                }

                return new Token(TokenType.HexLiteral, sb.ToString(), _line, startColumn);
            }

            // Regular number
            while (char.IsDigit(Current))
            {
                sb.Append(Advance());
            }

            // Look for a decimal point
            if (Current == '.' && char.IsDigit(Peek()))
            {
                sb.Append(Advance()); // .

                while (char.IsDigit(Current))
                {
                    sb.Append(Advance());
                }
            }

            return new Token(TokenType.NumberLiteral, sb.ToString(), _line, startColumn);
        }

        /// <summary>
        /// Checks if a character is a hex digit
        /// </summary>
        private bool IsHexDigit(char c)
        {
            return char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        /// <summary>
        /// Processes a string literal
        /// </summary>
        private Token String()
        {
            int startColumn = _column;
            StringBuilder sb = new StringBuilder();

            // Skip the opening quote
            Advance();

            while (Current != '"' && Current != '\0')
            {
                if (Current == '\\')
                {
                    Advance(); // Skip the backslash

                    switch (Current)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        default: sb.Append('\\').Append(Current); break;
                    }

                    Advance();
                }
                else
                {
                    sb.Append(Advance());
                }
            }

            if (Current == '\0')
            {
                return new Token(TokenType.Invalid, sb.ToString(), _line, startColumn);
            }

            // Skip the closing quote
            Advance();

            return new Token(TokenType.StringLiteral, sb.ToString(), _line, startColumn);
        }

        /// <summary>
        /// Gets the next token from the source
        /// </summary>
        public Token NextToken()
        {
            SkipWhitespaceAndComments();

            if (_position >= _source.Length)
            {
                return new Token(TokenType.Eof, "", _line, _column);
            }

            char c = Current;
            int column = _column;

            switch (c)
            {
                case '(': Advance(); return new Token(TokenType.OpenParen, "(", _line, column);
                case ')': Advance(); return new Token(TokenType.CloseParen, ")", _line, column);
                case '{': Advance(); return new Token(TokenType.OpenBrace, "{", _line, column);
                case '}': Advance(); return new Token(TokenType.CloseBrace, "}", _line, column);
                case '[': Advance(); return new Token(TokenType.OpenBracket, "[", _line, column);
                case ']': Advance(); return new Token(TokenType.CloseBracket, "]", _line, column);
                case ';': Advance(); return new Token(TokenType.Semicolon, ";", _line, column);
                case ',': Advance(); return new Token(TokenType.Comma, ",", _line, column);
                case ':': Advance(); return new Token(TokenType.Colon, ":", _line, column);
                case '.': Advance(); return new Token(TokenType.Dot, ".", _line, column);

                case '+': Advance(); return new Token(TokenType.Plus, "+", _line, column);
                case '-': Advance(); return new Token(TokenType.Minus, "-", _line, column);
                case '*': Advance(); return new Token(TokenType.Multiply, "*", _line, column);
                case '/': Advance(); return new Token(TokenType.Divide, "/", _line, column);
                case '%': Advance(); return new Token(TokenType.Modulo, "%", _line, column);

                case '=':
                    Advance();
                    if (Match('='))
                        return new Token(TokenType.Equal, "==", _line, column);
                    return new Token(TokenType.Assign, "=", _line, column);

                case '!':
                    Advance();
                    if (Match('='))
                        return new Token(TokenType.NotEqual, "!=", _line, column);
                    return new Token(TokenType.Not, "!", _line, column);

                case '>':
                    Advance();
                    if (Match('='))
                        return new Token(TokenType.GreaterEqual, ">=", _line, column);
                    return new Token(TokenType.Greater, ">", _line, column);

                case '<':
                    Advance();
                    if (Match('='))
                        return new Token(TokenType.LessEqual, "<=", _line, column);
                    return new Token(TokenType.Less, "<", _line, column);

                case '&':
                    Advance();
                    if (Match('&'))
                        return new Token(TokenType.And, "&&", _line, column);
                    return new Token(TokenType.Invalid, "&", _line, column);

                case '|':
                    Advance();
                    if (Match('|'))
                        return new Token(TokenType.Or, "||", _line, column);
                    return new Token(TokenType.Invalid, "|", _line, column);

                case '"':
                    return String();

                default:
                    if (char.IsLetter(c) || c == '_')
                        return Identifier();

                    if (char.IsDigit(c))
                        return Number();

                    Advance();
                    return new Token(TokenType.Invalid, c.ToString(), _line, column);
            }
        }

        /// <summary>
        /// Tokenizes the entire source
        /// </summary>
        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();
            Token token;

            do
            {
                token = NextToken();
                tokens.Add(token);
            } while (token.Type != TokenType.Eof);

            return tokens;
        }
    }
}