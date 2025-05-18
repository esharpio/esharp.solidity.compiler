using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace esharp.solidity.compiler
{
    /// <summary>
    /// Token types for Solidity language
    /// </summary>
    public enum TokenType
    {
        // Keywords
        Contract,
        Function,
        Public,
        Private,
        Internal,
        External,
        View,
        Pure,
        Payable,
        Memory,
        Storage,
        Calldata,
        Struct,
        Mapping,
        If,
        Else,
        While,
        For,
        Return,
        Require,
        Uint,
        Int,
        Bool,
        Address,
        String,
        Bytes,

        // Identifiers and literals
        Identifier,
        StringLiteral,
        NumberLiteral,
        HexLiteral,
        BoolLiteral,

        // Operators
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,
        Assign,
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterEqual,
        LessEqual,
        And,
        Or,
        Not,

        // Delimiters
        OpenParen,
        CloseParen,
        OpenBrace,
        CloseBrace,
        OpenBracket,
        CloseBracket,
        Semicolon,
        Comma,
        Colon,
        Dot,

        // Special
        Eof,
        Invalid,
        Try,
        Catch,
    }

    /// <summary>
    /// Represents a token in the Solidity source code
    /// </summary>
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"{Type}: '{Value}' at {Line}:{Column}";
        }
    }

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

    /// <summary>
    /// Base class for all AST nodes
    /// </summary>
    public abstract class AstNode
    {
        // This will be used as a base class for all AST nodes
    }

    /// <summary>
    /// Represents a Solidity contract
    /// </summary>
    public class ContractNode : AstNode
    {
        public string Name { get; }
        public List<AstNode> Members { get; } = new List<AstNode>();

        public ContractNode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"Contract {Name} with {Members.Count} members";
        }
    }

    /// <summary>
    /// Represents a function declaration
    /// </summary>
    public class FunctionNode : AstNode
    {
        public string Name { get; }
        public List<ParameterNode> Parameters { get; } = new List<ParameterNode>();
        public List<ParameterNode> ReturnParameters { get; } = new List<ParameterNode>();
        public List<string> Modifiers { get; } = new List<string>();
        public BlockNode Body { get; set; }
        public bool IsView { get; set; }
        public bool IsPure { get; set; }
        public bool IsPayable { get; set; }
        public string Visibility { get; set; } = "internal"; // Default in Solidity

        public FunctionNode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"Function {Name}({string.Join(", ", Parameters)})";
        }
    }

    /// <summary>
    /// Represents a function parameter
    /// </summary>
    public class ParameterNode : AstNode
    {
        public string Type { get; }
        public string Name { get; }
        public string DataLocation { get; set; } // memory, storage, calldata

        public ParameterNode(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            string location = !string.IsNullOrEmpty(DataLocation) ? $" {DataLocation}" : "";
            return $"{Type}{location} {Name}";
        }
    }

    /// <summary>
    /// Represents a block of statements
    /// </summary>
    public class BlockNode : AstNode
    {
        public List<AstNode> Statements { get; } = new List<AstNode>();

        public override string ToString()
        {
            return $"Block with {Statements.Count} statements";
        }
    }

    /// <summary>
    /// Represents a variable declaration
    /// </summary>
    public class VariableDeclarationNode : AstNode
    {
        public string Type { get; }
        public string Name { get; }
        public AstNode Initializer { get; set; }
        public string Visibility { get; set; } = "internal"; // Default in Solidity
        public string DataLocation { get; set; } // memory, storage, calldata

        public VariableDeclarationNode(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            string location = !string.IsNullOrEmpty(DataLocation) ? $" {DataLocation}" : "";
            return $"{Type}{location} {Name}";
        }
    }

    /// <summary>
    /// Represents a binary expression (e.g., a + b)
    /// </summary>
    public class BinaryExpressionNode : AstNode
    {
        public AstNode Left { get; }
        public string Operator { get; }
        public AstNode Right { get; }

        public BinaryExpressionNode(AstNode left, string op, AstNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }
    }

    // /// <summary>
    // /// Represents a literal value (number, string, etc.)
    // /// </summary>
    // public class LiteralNode : AstNode
    // {
    //     public string Value { get; }
    //     public string Type { get; }

    //     public LiteralNode(string value, string type)
    //     {
    //         Value = value;
    //         Type = type;
    //     }

    //     public override string ToString()
    //     {
    //         return $"{Type} literal: {Value}";
    //     }
    // }

    // /// <summary>
    // /// Represents a variable reference
    // /// </summary>
    // public class VariableReferenceNode : AstNode
    // {
    //     public string Name { get; }

    //     public VariableReferenceNode(string name)
    //     {
    //         Name = name;
    //     }

    //     public override string ToString()
    //     {
    //         return Name;
    //     }
    // }

    // /// <summary>
    // /// Represents a function call
    // /// </summary>
    // public class FunctionCallNode : AstNode
    // {
    //     public string Name { get; }
    //     public List<AstNode> Arguments { get; } = new List<AstNode>();

    //     public FunctionCallNode(string name)
    //     {
    //         Name = name;
    //     }

    //     public override string ToString()
    //     {
    //         return $"{Name}({string.Join(", ", Arguments)})";
    //     }
    // }

    // /// <summary>
    // /// Represents a return statement
    // /// </summary>
    // public class ReturnStatementNode : AstNode
    // {
    //     public AstNode Expression { get; }

    //     public ReturnStatementNode(AstNode expression)
    //     {
    //         Expression = expression;
    //     }

    //     public override string ToString()
    //     {
    //         return $"return {Expression}";
    //     }
    // }

    /// <summary>
    /// Represents an if statement
    /// </summary>
    public class IfStatementNode : AstNode
    {
        public AstNode Condition { get; }
        public AstNode ThenBranch { get; }
        public AstNode ElseBranch { get; }

        public IfStatementNode(AstNode condition, AstNode thenBranch, AstNode elseBranch = null)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override string ToString()
        {
            if (ElseBranch != null)
                return $"if ({Condition}) {ThenBranch} else {ElseBranch}";
            
            return $"if ({Condition}) {ThenBranch}";
        }
    }

    /// <summary>
    /// Represents a while loop
    /// </summary>
    public class WhileStatementNode : AstNode
    {
        public AstNode Condition { get; }
        public AstNode Body { get; }

        public WhileStatementNode(AstNode condition, AstNode body)
        {
            Condition = condition;
            Body = body;
        }

        public override string ToString()
        {
            return $"while ({Condition}) {Body}";
        }
    }

    /// <summary>
    /// Represents a for loop
    /// </summary>
    public class ForStatementNode : AstNode
    {
        public AstNode Initializer { get; }
        public AstNode Condition { get; }
        public AstNode Increment { get; }
        public AstNode Body { get; }

        public ForStatementNode(AstNode initializer, AstNode condition, AstNode increment, AstNode body)
        {
            Initializer = initializer;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public override string ToString()
        {
            return $"for ({Initializer}; {Condition}; {Increment}) {Body}";
        }
    }

    /// <summary>
    /// Represents a try/catch statement
    /// </summary>
    public class TryCatchNode : AstNode
    {
        public AstNode TryExpression { get; }
        public AstNode TryBlock { get; }
        public List<CatchClauseNode> CatchClauses { get; } = new List<CatchClauseNode>();

        public TryCatchNode(AstNode tryExpression, AstNode tryBlock)
        {
            TryExpression = tryExpression;
            TryBlock = tryBlock;
        }

        public override string ToString()
        {
            return $"try {TryExpression} {TryBlock} with {CatchClauses.Count} catch clauses";
        }
    }

    /// <summary>
    /// Represents a catch clause
    /// </summary>
    public class CatchClauseNode : AstNode
    {
        public string ErrorType { get; }
        public string ErrorName { get; }
        public AstNode Body { get; }

        public CatchClauseNode(string errorType, string errorName, AstNode body)
        {
            ErrorType = errorType;
            ErrorName = errorName;
            Body = body;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ErrorType))
                return $"catch {ErrorType} {ErrorName} {Body}";
            
            return $"catch {Body}";
        }
    }

    /// <summary>
    /// Parser for Solidity - converts tokens into an AST
    /// </summary>
    public class SolidityParser
    {
        private readonly List<Token> _tokens;
        private int _current;

        public SolidityParser(List<Token> tokens)
        {
            _tokens = tokens;
            _current = 0;
        }

        /// <summary>
        /// Returns the current token
        /// </summary>
        private Token Current => _current < _tokens.Count ? _tokens[_current] : _tokens[_tokens.Count - 1];

        /// <summary>
        /// Advances to the next token and returns the previous one
        /// </summary>
        private Token Advance()
        {
            Token current = Current;
            if (_current < _tokens.Count)
                _current++;
            return current;
        }

        /// <summary>
        /// Checks if the current token is of the given type
        /// </summary>
        private bool Check(TokenType type)
        {
            if (IsAtEnd())
                return false;
            return Current.Type == type;
        }

        /// <summary>
        /// Checks if we've reached the end of tokens
        /// </summary>
        private bool IsAtEnd()
        {
            return Current.Type == TokenType.Eof;
        }

        /// <summary>
        /// Consumes the current token if it's of the expected type, otherwise throws an error
        /// </summary>
        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
                return Advance();
            
            throw new SyntaxError($"{message} at line {Current.Line}, column {Current.Column}");
        }

        /// <summary>
        /// Parses the entire source into an AST
        /// </summary>
        public List<AstNode> Parse()
        {
            List<AstNode> nodes = new List<AstNode>();
            
            while (!IsAtEnd())
            {
                try
                {
                    nodes.Add(ParseTopLevel());
                }
                catch (SyntaxError error)
                {
                    Console.WriteLine(error.Message);
                    Synchronize();
                }
            }
            
            return nodes;
        }

        /// <summary>
        /// Synchronizes the parser after an error
        /// </summary>
        private void Synchronize()
        {
            Advance();
            
            while (!IsAtEnd())
            {
                if (Current.Type == TokenType.Semicolon)
                {
                    Advance();
                    return;
                }
                
                switch (Current.Type)
                {
                    case TokenType.Contract:
                    case TokenType.Function:
                    case TokenType.Struct:
                        return;
                }
                
                Advance();
            }
        }

        /// <summary>
        /// Parses a top-level declaration (contract, interface, library)
        /// </summary>
        private AstNode ParseTopLevel()
        {
            if (Match(TokenType.Contract))
                return ParseContract();
            
            // Add support for other top-level declarations (interface, library, etc.)
            
            throw new SyntaxError($"Expected top-level declaration at line {Current.Line}, column {Current.Column}");
        }

        /// <summary>
        /// Checks if the current token is of the given type and advances if it is
        /// </summary>
        private bool Match(TokenType type)
        {
            if (!Check(type))
                return false;
            
            Advance();
            return true;
        }

        /// <summary>
        /// Parses a contract declaration
        /// </summary>
        private ContractNode ParseContract()
        {
            Token name = Consume(TokenType.Identifier, "Expected contract name");
            ContractNode contract = new ContractNode(name.Value);
            
            Consume(TokenType.OpenBrace, "Expected '{' after contract name");
            
            while (!Check(TokenType.CloseBrace) && !IsAtEnd())
            {
                contract.Members.Add(ParseContractMember());
            }
            
            Consume(TokenType.CloseBrace, "Expected '}' after contract body");
            
            return contract;
        }

        /// <summary>
        /// Parses a contract member (function, variable, struct, etc.)
        /// </summary>
        private AstNode ParseContractMember()
        {
            if (Match(TokenType.Function))
                return ParseFunction();
            
            // By default, try to parse as a state variable
            return ParseStateVariable();
        }

        /// <summary>
        /// Parses a function declaration
        /// </summary>
        private FunctionNode ParseFunction()
        {
            // Function name might be missing for the fallback function
            string name = "";
            if (Check(TokenType.Identifier))
            {
                name = Advance().Value;
            }
            
            FunctionNode function = new FunctionNode(name);
            
            // Parse parameters
            Consume(TokenType.OpenParen, "Expected '(' after function name");
            
            if (!Check(TokenType.CloseParen))
            {
                do
                {
                    // Parse parameter type and name
                    Token typeToken = Consume(TokenType.Identifier, "Expected parameter type");
                    string typeName = typeToken.Value;
                    
                    // Some types have multiple parts (e.g., "uint256")
                    // For simplicity, we'll just check for digits after the type
                    if (Check(TokenType.NumberLiteral))
                    {
                        typeName += Advance().Value;
                    }
                    
                    ParameterNode parameter;
                    
                    if (Check(TokenType.Identifier))
                    {
                        // Named parameter
                        Token nameToken = Advance();
                        parameter = new ParameterNode(typeName, nameToken.Value);
                    }
                    else
                    {
                        // Anonymous parameter
                        parameter = new ParameterNode(typeName, "");
                    }
                    
                    // Check for data location (memory, storage, calldata)
                    if (Match(TokenType.Memory))
                    {
                        parameter.DataLocation = "memory";
                    }
                    else if (Match(TokenType.Storage))
                    {
                        parameter.DataLocation = "storage";
                    }
                    else if (Match(TokenType.Calldata))
                    {
                        parameter.DataLocation = "calldata";
                    }
                    
                    function.Parameters.Add(parameter);
                    
                } while (Match(TokenType.Comma));
            }
            
            Consume(TokenType.CloseParen, "Expected ')' after parameters");
            
            // Parse modifiers and function specifiers
            while (true)
            {
                if (Match(TokenType.Public))
                {
                    function.Visibility = "public";
                }
                else if (Match(TokenType.Private))
                {
                    function.Visibility = "private";
                }
                else if (Match(TokenType.Internal))
                {
                    function.Visibility = "internal";
                }
                else if (Match(TokenType.External))
                {
                    function.Visibility = "external";
                }
                else if (Match(TokenType.View))
                {
                    function.IsView = true;
                }
                else if (Match(TokenType.Pure))
                {
                    function.IsPure = true;
                }
                else if (Match(TokenType.Payable))
                {
                    function.IsPayable = true;
                }
                else if (Check(TokenType.Identifier))
                {
                    // Custom modifier
                    function.Modifiers.Add(Advance().Value);
                    
                    // Check for modifier arguments
                    if (Match(TokenType.OpenParen))
                    {
                        // Skip over the modifier arguments for now
                        int parenCount = 1;
                        while (parenCount > 0 && !IsAtEnd())
                        {
                            if (Match(TokenType.OpenParen))
                                parenCount++;
                            else if (Match(TokenType.CloseParen))
                                parenCount--;
                            else
                                Advance();
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            
            // Parse return parameters
            if (Match(TokenType.Return))
            {
                Consume(TokenType.OpenParen, "Expected '(' after 'returns'");
                
                if (!Check(TokenType.CloseParen))
                {
                    do
                    {
                        // Parse return parameter type and name
                        Token typeToken = Consume(TokenType.Identifier, "Expected return parameter type");
                        string typeName = typeToken.Value;
                        
                        ParameterNode parameter;
                        
                        if (Check(TokenType.Identifier))
                        {
                            // Named return parameter
                            Token nameToken = Advance();
                            parameter = new ParameterNode(typeName, nameToken.Value);
                        }
                        else
                        {
                            // Anonymous return parameter
                            parameter = new ParameterNode(typeName, "");
                        }
                        
                        // Check for data location (memory, storage, calldata)
                        if (Match(TokenType.Memory))
                        {
                            parameter.DataLocation = "memory";
                        }
                        else if (Match(TokenType.Storage))
                        {
                            parameter.DataLocation = "storage";
                        }
                        else if (Match(TokenType.Calldata))
                        {
                            parameter.DataLocation = "calldata";
                        }
                        
                        function.ReturnParameters.Add(parameter);
                        
                    } while (Match(TokenType.Comma));
                }
                
                Consume(TokenType.CloseParen, "Expected ')' after return parameters");
            }
            
            // Parse function body or just a semicolon for interface functions
            if (Match(TokenType.Semicolon))
            {
                // Interface function, no body
                return function;
            }
            
            function.Body = ParseBlock();
            
            return function;
        }

        /// <summary>
        /// Parses a block of statements
        /// </summary>
        private BlockNode ParseBlock()
        {
            BlockNode block = new BlockNode();
            
            Consume(TokenType.OpenBrace, "Expected '{' before block");
            
            while (!Check(TokenType.CloseBrace) && !IsAtEnd())
            {
                block.Statements.Add(ParseStatement());
            }
            
            Consume(TokenType.CloseBrace, "Expected '}' after block");
            
            return block;
        }

        /// <summary>
        /// Parses a statement
        /// </summary>
        private AstNode ParseStatement()
        {
            if (Match(TokenType.OpenBrace))
            {
                // We've already consumed the opening brace
                BlockNode block = new BlockNode();
                
                while (!Check(TokenType.CloseBrace) && !IsAtEnd())
                {
                    block.Statements.Add(ParseStatement());
                }
                
                Consume(TokenType.CloseBrace, "Expected '}' after block");
                
                return block;
            }
            else if (Match(TokenType.If))
            {
                return ParseIfStatement();
            }
            else if (Match(TokenType.While))
            {
                return ParseWhileStatement();
            }
            else if (Match(TokenType.For))
            {
                return ParseForStatement();
            }
            else if (Match(TokenType.Try))
            {
                return ParseTryCatchStatement();
            }
            else if (Match(TokenType.Return))
            {
                // Parse a return statement
                AstNode expression = null;
                if (!Check(TokenType.Semicolon))
                {
                    expression = ParseExpression();
                }
                
                Consume(TokenType.Semicolon, "Expected ';' after return statement");
                
                return new ReturnStatementNode(expression);
            }
            else if (Check(TokenType.Identifier))
            {
                // This could be a variable declaration or an assignment
                // We'll peek ahead to determine which
                Token identifier = Current;
                Advance();
                
                if (Match(TokenType.Assign))
                {
                    // This is an assignment
                    VariableReferenceNode varRef = new VariableReferenceNode(identifier.Value);
                    AstNode value = ParseExpression();
                    
                    Consume(TokenType.Semicolon, "Expected ';' after assignment");
                    
                    return new AssignmentNode(varRef, value);
                }
                else if (Check(TokenType.Identifier))
                {
                    // This is a variable declaration
                    string typeName = identifier.Value;
                    string name = Advance().Value;
                    
                    VariableDeclarationNode varDecl = new VariableDeclarationNode(typeName, name);
                    
                    // Check for memory/storage keyword
                    if (Match(TokenType.Memory))
                    {
                        varDecl.DataLocation = "memory";
                    }
                    else if (Match(TokenType.Storage))
                    {
                        varDecl.DataLocation = "storage";
                    }
                    else if (Match(TokenType.Calldata))
                    {
                        varDecl.DataLocation = "calldata";
                    }
                    
                    // Check for initializer
                    if (Match(TokenType.Assign))
                    {
                        varDecl.Initializer = ParseExpression();
                    }
                    
                    Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
                    
                    return varDecl;
                }
                else if (Match(TokenType.OpenParen))
                {
                    // This is a function call
                    FunctionCallNode funcCall = new FunctionCallNode(identifier.Value);
                    
                    if (!Check(TokenType.CloseParen))
                    {
                        do
                        {
                            funcCall.Arguments.Add(ParseExpression());
                        } while (Match(TokenType.Comma));
                    }
                    
                    Consume(TokenType.CloseParen, "Expected ')' after function arguments");
                    Consume(TokenType.Semicolon, "Expected ';' after function call");
                    
                    return funcCall;
                }
                
                // If we get here, it's an error
                throw new SyntaxError($"Unexpected token after identifier at line {Current.Line}, column {Current.Column}");
            }
            
            // If we get here, it's a parse error
            throw new SyntaxError($"Expected statement at line {Current.Line}, column {Current.Column}");
        }

        /// <summary>
        /// Parses an if statement
        /// </summary>
        private IfStatementNode ParseIfStatement()
        {
            Consume(TokenType.OpenParen, "Expected '(' after 'if'");
            AstNode condition = ParseExpression();
            Consume(TokenType.CloseParen, "Expected ')' after if condition");
            
            AstNode thenBranch = ParseStatement();
            AstNode elseBranch = null;
            
            if (Match(TokenType.Else))
            {
                elseBranch = ParseStatement();
            }
            
            return new IfStatementNode(condition, thenBranch, elseBranch);
        }

        /// <summary>
        /// Parses a while statement
        /// </summary>
        private WhileStatementNode ParseWhileStatement()
        {
            Consume(TokenType.OpenParen, "Expected '(' after 'while'");
            AstNode condition = ParseExpression();
            Consume(TokenType.CloseParen, "Expected ')' after while condition");
            
            AstNode body = ParseStatement();
            
            return new WhileStatementNode(condition, body);
        }

        /// <summary>
        /// Parses a for statement
        /// </summary>
        private ForStatementNode ParseForStatement()
        {
            Consume(TokenType.OpenParen, "Expected '(' after 'for'");
            
            // Initializer
            AstNode initializer;
            if (Match(TokenType.Semicolon))
            {
                initializer = null;
            }
            else
            {
                // This could be a variable declaration or an expression
                // if (Check(TokenType.Identifier) && Peek().Type == TokenType.Identifier)
                if (Check(TokenType.Identifier))
                {
                    // Variable declaration
                    string typeName = Advance().Value;
                    string name = Advance().Value;
                    
                    VariableDeclarationNode varDecl = new VariableDeclarationNode(typeName, name);
                    
                    // Check for initializer
                    if (Match(TokenType.Assign))
                    {
                        varDecl.Initializer = ParseExpression();
                    }
                    
                    initializer = varDecl;
                }
                else
                {
                    // Expression
                    initializer = ParseExpression();
                }
                
                Consume(TokenType.Semicolon, "Expected ';' after for initializer");
            }
            
            // Condition
            AstNode condition = null;
            if (!Check(TokenType.Semicolon))
            {
                condition = ParseExpression();
            }
            
            Consume(TokenType.Semicolon, "Expected ';' after for condition");
            
            // Increment
            AstNode increment = null;
            if (!Check(TokenType.CloseParen))
            {
                increment = ParseExpression();
            }
            
            Consume(TokenType.CloseParen, "Expected ')' after for clauses");
            
            AstNode body = ParseStatement();
            
            return new ForStatementNode(initializer, condition, increment, body);
        }

        /// <summary>
        /// Parses a try/catch statement
        /// </summary>
        private TryCatchNode ParseTryCatchStatement()
        {
            // Parse the try expression (function call)
            AstNode tryExpression = ParsePrimary();
            
            // Parse the returns clause if present
            if (Match(TokenType.Return))
            {
                // Skip the returns clause for now
                Consume(TokenType.OpenParen, "Expected '(' after 'returns'");
                
                while (!Check(TokenType.CloseParen) && !IsAtEnd())
                {
                    Advance();
                }
                
                Consume(TokenType.CloseParen, "Expected ')' after returns parameters");
            }
            
            // Parse the try block
            AstNode tryBlock = ParseStatement();
            
            TryCatchNode tryNode = new TryCatchNode(tryExpression, tryBlock);
            
            // Parse catch clauses
            while (Match(TokenType.Identifier) && Current.Value == "catch")
            {
                string errorType = "";
                string errorName = "";
                
                // Check if this is a typed catch clause
                if (Check(TokenType.Identifier) && Current.Value != "{")
                {
                    errorType = Advance().Value;
                    
                    if (Check(TokenType.Identifier))
                    {
                        errorName = Advance().Value;
                    }
                }
                
                AstNode catchBody = ParseStatement();
                
                tryNode.CatchClauses.Add(new CatchClauseNode(errorType, errorName, catchBody));
            }
            
            return tryNode;
        }

        /// <summary>
        /// Parses an expression
        /// </summary>
        private AstNode ParseExpression()
        {
            return ParseAssignment();
        }

        /// <summary>
        /// Parses an assignment expression
        /// </summary>
        private AstNode ParseAssignment()
        {
            AstNode expr = ParseLogicalOr();
            
            if (Match(TokenType.Assign))
            {
                AstNode value = ParseAssignment();
                
                if (expr is VariableReferenceNode varRef)
                {
                    return new AssignmentNode(varRef, value);
                }
                
                throw new SyntaxError($"Invalid assignment target at line {Current.Line}, column {Current.Column}");
            }
            
            return expr;
        }

        /// <summary>
        /// Parses a logical OR expression
        /// </summary>
        private AstNode ParseLogicalOr()
        {
            AstNode expr = ParseLogicalAnd();
            
            while (Match(TokenType.Or))
            {
                AstNode right = ParseLogicalAnd();
                expr = new BinaryExpressionNode(expr, "||", right);
            }
            
            return expr;
        }

        /// <summary>
        /// Parses a logical AND expression
        /// </summary>
        private AstNode ParseLogicalAnd()
        {
            AstNode expr = ParseEquality();
            
            while (Match(TokenType.And))
            {
                AstNode right = ParseEquality();
                expr = new BinaryExpressionNode(expr, "&&", right);
            }
            
            return expr;
        }

        /// <summary>
        /// Parses an equality expression
        /// </summary>
        private AstNode ParseEquality()
        {
            AstNode expr = ParseComparison();
            
            while (Match(TokenType.Equal) || Match(TokenType.NotEqual))
            {
                string op = Current.Type == TokenType.Equal ? "==" : "!=";
                AstNode right = ParseComparison();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        /// <summary>
        /// Parses a comparison expression
        /// </summary>
        private AstNode ParseComparison()
        {
            AstNode expr = ParseTerm();
            
            while (Match(TokenType.Less) || Match(TokenType.Greater) || 
                   Match(TokenType.LessEqual) || Match(TokenType.GreaterEqual))
            {
                string op;
                if (Current.Type == TokenType.Less)
                    op = "<";
                else if (Current.Type == TokenType.Greater)
                    op = ">";
                else if (Current.Type == TokenType.LessEqual)
                    op = "<=";
                else
                    op = ">=";
                
                AstNode right = ParseTerm();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        /// <summary>
        /// Parses a term
        /// </summary>
        private AstNode ParseTerm()
        {
            AstNode expr = ParseFactor();
            
            while (Match(TokenType.Plus) || Match(TokenType.Minus))
            {
                string op = Current.Type == TokenType.Plus ? "+" : "-";
                AstNode right = ParseFactor();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        /// <summary>
        /// Parses a factor
        /// </summary>
        private AstNode ParseFactor()
        {
            AstNode expr = ParseUnary();
            
            while (Match(TokenType.Multiply) || Match(TokenType.Divide) || Match(TokenType.Modulo))
            {
                string op;
                if (Current.Type == TokenType.Multiply)
                    op = "*";
                else if (Current.Type == TokenType.Divide)
                    op = "/";
                else
                    op = "%";
                
                AstNode right = ParseUnary();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        /// <summary>
        /// Parses a unary expression
        /// </summary>
        private AstNode ParseUnary()
        {
            if (Match(TokenType.Not) || Match(TokenType.Minus))
            {
                string op = Current.Type == TokenType.Not ? "!" : "-";
                AstNode right = ParseUnary();
                return new BinaryExpressionNode(new LiteralNode("0", "number"), op, right);
            }
            
            return ParsePrimary();
        }

        /// <summary>
        /// Parses a primary expression
        /// </summary>
        private AstNode ParsePrimary()
        {
            if (Match(TokenType.NumberLiteral))
            {
                return new LiteralNode(Current.Value, "number");
            }
            else if (Match(TokenType.StringLiteral))
            {
                return new LiteralNode(Current.Value, "string");
            }
            else if (Match(TokenType.BoolLiteral))
            {
                return new LiteralNode(Current.Value, "bool");
            }
            else if (Match(TokenType.HexLiteral))
            {
                return new LiteralNode(Current.Value, "hex");
            }
            else if (Match(TokenType.Identifier))
            {
                string name = Current.Value;
                
                if (Match(TokenType.OpenParen))
                {
                    // Function call
                    FunctionCallNode funcCall = new FunctionCallNode(name);
                    
                    if (!Check(TokenType.CloseParen))
                    {
                        do
                        {
                            funcCall.Arguments.Add(ParseExpression());
                        } while (Match(TokenType.Comma));
                    }
                    
                    Consume(TokenType.CloseParen, "Expected ')' after function arguments");
                    
                    return funcCall;
                }
                
                // Variable reference
                return new VariableReferenceNode(name);
            }
            else if (Match(TokenType.OpenParen))
            {
                AstNode expr = ParseExpression();
                Consume(TokenType.CloseParen, "Expected ')' after expression");
                return expr;
            }
            
            throw new SyntaxError($"Expected expression at line {Current.Line}, column {Current.Column}");
        }

        /// <summary>
        /// Parses a state variable declaration
        /// </summary>
        private AstNode ParseStateVariable()
        {
            // For simplicity, just consume until semicolon
            // This would be expanded in a full implementation
            
            string typeName = Current.Value;
            Advance();
            
            if (Check(TokenType.Identifier))
            {
                string name = Current.Value;
                Advance();
                
                // Skip until semicolon
                while (!Check(TokenType.Semicolon) && !IsAtEnd())
                {
                    Advance();
                }
                
                Consume(TokenType.Semicolon, "Expected ';' after state variable declaration");
                
                return new VariableDeclarationNode(typeName, name);
            }
            
            throw new SyntaxError($"Expected identifier after type at line {Current.Line}, column {Current.Column}");
        }
    }

    /// <summary>
    /// Custom exception for syntax errors
    /// </summary>
    public class SyntaxError : Exception
    {
        public SyntaxError(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Represents Nethermind EVM opcodes
    /// </summary>
    public enum Opcode
    {
        // 0x0 range - arithmetic ops
        STOP = 0x00,
        ADD = 0x01,
        MUL = 0x02,
        SUB = 0x03,
        DIV = 0x04,
        SDIV = 0x05,
        MOD = 0x06,
        SMOD = 0x07,
        ADDMOD = 0x08,
        MULMOD = 0x09,
        EXP = 0x0a,
        SIGNEXTEND = 0x0b,

        // 0x10 range - bit ops
        LT = 0x10,
        GT = 0x11,
        SLT = 0x12,
        SGT = 0x13,
        EQ = 0x14,
        ISZERO = 0x15,
        AND = 0x16,
        OR = 0x17,
        XOR = 0x18,
        NOT = 0x19,
        BYTE = 0x1a,
        SHL = 0x1b, // Shift Left
        SHR = 0x1c, // Logical Shift Right
        SAR = 0x1d, // Arithmetic Shift Right

        // 0x20 range - crypto
        SHA3 = 0x20,

        // 0x30 range - closure state
        ADDRESS = 0x30,
        BALANCE = 0x31,
        ORIGIN = 0x32,
        CALLER = 0x33,
        CALLVALUE = 0x34,
        CALLDATALOAD = 0x35,
        CALLDATASIZE = 0x36,
        CALLDATACOPY = 0x37,
        CODESIZE = 0x38,
        CODECOPY = 0x39,
        GASPRICE = 0x3a,
        EXTCODESIZE = 0x3b,
        EXTCODECOPY = 0x3c,
        RETURNDATASIZE = 0x3d,
        RETURNDATACOPY = 0x3e,
        EXTCODEHASH = 0x3f,

        // 0x40 range - block operations
        BLOCKHASH = 0x40,
        COINBASE = 0x41,
        TIMESTAMP = 0x42,
        NUMBER = 0x43,
        DIFFICULTY = 0x44,
        GASLIMIT = 0x45,
        CHAINID = 0x46,
        SELFBALANCE = 0x47,
        BASEFEE = 0x48,

        // 0x50 range - 'storage' and execution
        POP = 0x50,
        MLOAD = 0x51,
        MSTORE = 0x52,
        MSTORE8 = 0x53,
        SLOAD = 0x54,
        SSTORE = 0x55,
        JUMP = 0x56,
        JUMPI = 0x57,
        PC = 0x58,
        MSIZE = 0x59,
        GAS = 0x5a,
        JUMPDEST = 0x5b,
        PUSH0 = 0x5f,

        // 0x60 range - pushes
        PUSH1 = 0x60,
        PUSH2 = 0x61,
        PUSH3 = 0x62,
        PUSH4 = 0x63,
        PUSH5 = 0x64,
        PUSH6 = 0x65,
        PUSH7 = 0x66,
        PUSH8 = 0x67,
        PUSH9 = 0x68,
        PUSH10 = 0x69,
        PUSH11 = 0x6a,
        PUSH12 = 0x6b,
        PUSH13 = 0x6c,
        PUSH14 = 0x6d,
        PUSH15 = 0x6e,
        PUSH16 = 0x6f,
        PUSH17 = 0x70,
        PUSH18 = 0x71,
        PUSH19 = 0x72,
        PUSH20 = 0x73,
        PUSH21 = 0x74,
        PUSH22 = 0x75,
        PUSH23 = 0x76,
        PUSH24 = 0x77,
        PUSH25 = 0x78,
        PUSH26 = 0x79,
        PUSH27 = 0x7a,
        PUSH28 = 0x7b,
        PUSH29 = 0x7c,
        PUSH30 = 0x7d,
        PUSH31 = 0x7e,
        PUSH32 = 0x7f,

        // 0x80 range - dups
        DUP1 = 0x80,
        DUP2 = 0x81,
        DUP3 = 0x82,
        DUP4 = 0x83,
        DUP5 = 0x84,
        DUP6 = 0x85,
        DUP7 = 0x86,
        DUP8 = 0x87,
        DUP9 = 0x88,
        DUP10 = 0x89,
        DUP11 = 0x8a,
        DUP12 = 0x8b,
        DUP13 = 0x8c,
        DUP14 = 0x8d,
        DUP15 = 0x8e,
        DUP16 = 0x8f,

        // 0x90 range - swaps
        SWAP1 = 0x90,
        SWAP2 = 0x91,
        SWAP3 = 0x92,
        SWAP4 = 0x93,
        SWAP5 = 0x94,
        SWAP6 = 0x95,
        SWAP7 = 0x96,
        SWAP8 = 0x97,
        SWAP9 = 0x98,
        SWAP10 = 0x99,
        SWAP11 = 0x9a,
        SWAP12 = 0x9b,
        SWAP13 = 0x9c,
        SWAP14 = 0x9d,
        SWAP15 = 0x9e,
        SWAP16 = 0x9f,

        // 0xa0 range - logging ops
        LOG0 = 0xa0,
        LOG1 = 0xa1,
        LOG2 = 0xa2,
        LOG3 = 0xa3,
        LOG4 = 0xa4,

        // 0xf0 range - closures
        CREATE = 0xf0,
        CALL = 0xf1,
        CALLCODE = 0xf2,
        RETURN = 0xf3,
        DELEGATECALL = 0xf4,
        CREATE2 = 0xf5,
        STATICCALL = 0xfa,
        REVERT = 0xfd,
        INVALID = 0xfe,
        SELFDESTRUCT = 0xff
    }

    /// <summary>
    /// Represents a bytecode instruction
    /// </summary>
    public class Instruction
    {
        public Opcode Opcode { get; }
        public byte[] Operand { get; }

        public Instruction(Opcode opcode)
        {
            Opcode = opcode;
            Operand = new byte[0];
        }

        public Instruction(Opcode opcode, byte[] operand)
        {
            Opcode = opcode;
            Operand = operand;
        }

        public Instruction(Opcode opcode, int operand)
        {
            Opcode = opcode;
            Operand = BitConverter.GetBytes(operand);
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte> { (byte)Opcode };
            bytes.AddRange(Operand);
            return bytes.ToArray();
        }

        public override string ToString()
        {
            if (Operand.Length == 0)
                return Opcode.ToString();
            
            return $"{Opcode} 0x{BitConverter.ToString(Operand).Replace("-", "")}";
        }
    }

    /// <summary>
    /// Simple symbol table for variables and functions
    /// </summary>
    public class SymbolTable
    {
        private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _functions = new Dictionary<string, int>();
        private int _nextVariableSlot = 0;

        public void DeclareVariable(string name)
        {
            _variables[name] = _nextVariableSlot++;
        }

        public int GetVariableSlot(string name)
        {
            if (_variables.TryGetValue(name, out int slot))
                return slot;
            
            throw new Exception($"Undefined variable: {name}");
        }

        public void DeclareFunction(string name, int offset)
        {
            _functions[name] = offset;
        }

        public int GetFunctionOffset(string name)
        {
            if (_functions.TryGetValue(name, out int offset))
                return offset;
            
            throw new Exception($"Undefined function: {name}");
        }
        
        public bool IsFunctionDeclared(string name)
        {
            return _functions.ContainsKey(name);
        }
    }

    /// <summary>
    /// Code generator that transforms AST nodes into EVM bytecode
    /// </summary>
    public class CodeGenerator
    {
        private readonly List<Instruction> _instructions = new List<Instruction>();
        private readonly SymbolTable _symbols = new SymbolTable();
        private readonly Dictionary<string, int> _jumpDestinations = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _pendingJumps = new Dictionary<string, List<int>>();

        /// <summary>
        /// Generate bytecode from an AST
        /// </summary>
        public byte[] Generate(List<AstNode> ast)
        {
            // Reset state for a new generation
            _instructions.Clear();
            _symbols.DeclareFunction("main", 0);
            
            // First pass: collect function declarations
            foreach (AstNode node in ast)
            {
                if (node is ContractNode contract)
                {
                    foreach (AstNode member in contract.Members)
                    {
                        if (member is FunctionNode function)
                        {
                            // We'll fill in the actual offsets later
                            _symbols.DeclareFunction($"{contract.Name}.{function.Name}", 0);
                        }
                    }
                }
            }
            
            // Second pass: generate code
            foreach (AstNode node in ast)
            {
                GenerateNode(node);
            }
            
            // Resolve pending jumps
            ResolvePendingJumps();
            
            // Convert instructions to bytes
            List<byte> bytecode = new List<byte>();
            foreach (Instruction instruction in _instructions)
            {
                bytecode.AddRange(instruction.ToBytes());
            }
            
            return bytecode.ToArray();
        }

        /// <summary>
        /// Generate code for an AST node
        /// </summary>
        private void GenerateNode(AstNode node)
        {
            if (node is ContractNode contract)
            {
                GenerateContract(contract);
            }
            else if (node is FunctionNode function)
            {
                GenerateFunction(function);
            }
            else if (node is BlockNode block)
            {
                GenerateBlock(block);
            }
            else if (node is VariableDeclarationNode varDecl)
            {
                GenerateVariableDeclaration(varDecl);
            }
            else if (node is BinaryExpressionNode binExpr)
            {
                GenerateBinaryExpression(binExpr);
            }
            else if (node is LiteralNode literal)
            {
                GenerateLiteral(literal);
            }
            else if (node is VariableReferenceNode varRef)
            {
                GenerateVariableReference(varRef);
            }
            else if (node is FunctionCallNode funcCall)
            {
                GenerateFunctionCall(funcCall);
            }
            else if (node is ReturnStatementNode returnStmt)
            {
                GenerateReturnStatement(returnStmt);
            }
            else if (node is AssignmentNode assignment)
            {
                GenerateAssignment(assignment);
            }
            else if (node is IfStatementNode ifStmt)
            {
                GenerateIfStatement(ifStmt);
            }
            else if (node is WhileStatementNode whileStmt)
            {
                GenerateWhileStatement(whileStmt);
            }
            else if (node is ForStatementNode forStmt)
            {
                GenerateForStatement(forStmt);
            }
            else if (node is TryCatchNode tryCatch)
            {
                GenerateTryCatchStatement(tryCatch);
            }
            else
            {
                throw new NotImplementedException($"Code generation not implemented for {node.GetType().Name}");
            }
        }

        /// <summary>
        /// Generate code for an if statement
        /// </summary>
        private void GenerateIfStatement(IfStatementNode ifStmt)
        {
            // Generate a unique label for the end of the if statement
            string endLabel = $"if_end_{_labelCounter++}";
            string elseLabel = null;
            
            // Generate code for the condition
            GenerateNode(ifStmt.Condition);
            
            if (ifStmt.ElseBranch != null)
            {
                // If there's an else branch, we need a label for it
                elseLabel = $"if_else_{_labelCounter++}";
                
                // If condition is false, jump to the else branch
                _instructions.Add(new Instruction(Opcode.ISZERO));
                EmitPush(0); // Placeholder, will be filled in later
                int jumpToElsePos = _instructions.Count;
                _instructions.Add(new Instruction(Opcode.JUMPI));
                
                // Add the jump destination to pending jumps
                AddPendingJump(elseLabel, jumpToElsePos - 1);
            }
            else
            {
                // If condition is false, jump to the end
                _instructions.Add(new Instruction(Opcode.ISZERO));
                EmitPush(0); // Placeholder, will be filled in later
                int jumpToEndPos = _instructions.Count;
                _instructions.Add(new Instruction(Opcode.JUMPI));
                
                // Add the jump destination to pending jumps
                AddPendingJump(endLabel, jumpToEndPos - 1);
            }
            
            // Generate code for the then branch
            GenerateNode(ifStmt.ThenBranch);
            
            if (ifStmt.ElseBranch != null)
            {
                // Jump to the end after the then branch
                EmitPush(0); // Placeholder, will be filled in later
                int jumpToEndPos = _instructions.Count;
                _instructions.Add(new Instruction(Opcode.JUMP));
                
                // Add the jump destination to pending jumps
                AddPendingJump(endLabel, jumpToEndPos - 1);
                
                // Add a jump destination for the else branch
                int elsePos = _instructions.Count;
                _jumpDestinations[elseLabel] = elsePos;
                _instructions.Add(new Instruction(Opcode.JUMPDEST));
                
                // Generate code for the else branch
                GenerateNode(ifStmt.ElseBranch);
            }
            
            // Add a jump destination for the end
            int endPos = _instructions.Count;
            _jumpDestinations[endLabel] = endPos;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
        }

        /// <summary>
        /// Generate code for a while statement
        /// </summary>
        private void GenerateWhileStatement(WhileStatementNode whileStmt)
        {
            // Generate unique labels for the loop
            string loopStart = $"while_start_{_labelCounter++}";
            string loopEnd = $"while_end_{_labelCounter++}";
            
            // Add a jump destination for the loop start
            int startPos = _instructions.Count;
            _jumpDestinations[loopStart] = startPos;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
            
            // Generate code for the condition
            GenerateNode(whileStmt.Condition);
            
            // If condition is false, jump to the end
            _instructions.Add(new Instruction(Opcode.ISZERO));
            EmitPush(0); // Placeholder, will be filled in later
            int jumpToEndPos = _instructions.Count;
            _instructions.Add(new Instruction(Opcode.JUMPI));
            
            // Add the jump destination to pending jumps
            AddPendingJump(loopEnd, jumpToEndPos - 1);
            
            // Generate code for the loop body
            GenerateNode(whileStmt.Body);
            
            // Jump back to the start
            EmitPush(0); // Placeholder, will be filled in later
            int jumpToStartPos = _instructions.Count;
            _instructions.Add(new Instruction(Opcode.JUMP));
            
            // Add the jump destination to pending jumps
            AddPendingJump(loopStart, jumpToStartPos - 1);
            
            // Add a jump destination for the end
            int endPos = _instructions.Count;
            _jumpDestinations[loopEnd] = endPos;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
        }

        /// <summary>
        /// Generate code for a for statement
        /// </summary>
        private void GenerateForStatement(ForStatementNode forStmt)
        {
            // Generate unique labels for the loop
            string loopStart = $"for_start_{_labelCounter++}";
            string loopIncrement = $"for_increment_{_labelCounter++}";
            string loopEnd = $"for_end_{_labelCounter++}";
            
            // Generate code for the initializer
            if (forStmt.Initializer != null)
            {
                GenerateNode(forStmt.Initializer);
            }
            
            // Add a jump destination for the loop start
            int startPos = _instructions.Count;
            _jumpDestinations[loopStart] = startPos;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
            
            // Generate code for the condition
            if (forStmt.Condition != null)
            {
                GenerateNode(forStmt.Condition);
                
                // If condition is false, jump to the end
                _instructions.Add(new Instruction(Opcode.ISZERO));
                EmitPush(0); // Placeholder, will be filled in later
                int jumpToEndPos = _instructions.Count;
                _instructions.Add(new Instruction(Opcode.JUMPI));
                
                // Add the jump destination to pending jumps
                AddPendingJump(loopEnd, jumpToEndPos - 1);
            }
            
            // Generate code for the loop body
            GenerateNode(forStmt.Body);
            
            // Add a jump destination for the increment
            int incrementPos = _instructions.Count;
            _jumpDestinations[loopIncrement] = incrementPos;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
            
            // Generate code for the increment
            if (forStmt.Increment != null)
            {
                GenerateNode(forStmt.Increment);
            }
            
            // Jump back to the start
            EmitPush(0); // Placeholder, will be filled in later
            int jumpToStartPos = _instructions.Count;
            _instructions.Add(new Instruction(Opcode.JUMP));
            
            // Add the jump destination to pending jumps
            AddPendingJump(loopStart, jumpToStartPos - 1);
            
            // Add a jump destination for the end
            int endPos = _instructions.Count;
            _jumpDestinations[loopEnd] = endPos;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
        }

        /// <summary>
        /// Generate code for a try/catch statement
        /// </summary>
        private void GenerateTryCatchStatement(TryCatchNode tryCatch)
        {
            // For try/catch, we need to set up a simple exception handling mechanism
            // We'll use a special memory slot to track if an exception occurred
            int exceptionSlot = 0xFFFF; // A special slot for the exception flag
            
            // Generate unique labels
            string tryEnd = $"try_end_{_labelCounter++}";
            string[] catchStarts = new string[tryCatch.CatchClauses.Count];
            for (int i = 0; i < tryCatch.CatchClauses.Count; i++)
            {
                catchStarts[i] = $"catch_start_{_labelCounter++}";
            }
            string tryFinalEnd = $"try_final_end_{_labelCounter++}";
            
            // Initialize exception flag to 0 (no exception)
            EmitPush(0);
            EmitPush(exceptionSlot);
            _instructions.Add(new Instruction(Opcode.SSTORE));
            
            // Generate code for the try expression (usually a function call)
            // In a real Solidity compiler, this would involve STATICCALL or similar
            GenerateNode(tryCatch.TryExpression);
            
            // Check if an exception occurred and jump to the appropriate catch block
            EmitPush(exceptionSlot);
            _instructions.Add(new Instruction(Opcode.SLOAD));
            
            for (int i = 0; i < tryCatch.CatchClauses.Count; i++)
            {
                // If exception type matches, jump to the catch block
                // For simplicity, we'll just check if any exception occurred
                _instructions.Add(new Instruction(Opcode.DUP1));
                EmitPush(0);
                _instructions.Add(new Instruction(Opcode.GT));
                
                EmitPush(0); // Placeholder, will be filled in later
                int jumpToCatchPos = _instructions.Count;
                _instructions.Add(new Instruction(Opcode.JUMPI));
                
                // Add the jump destination to pending jumps
                AddPendingJump(catchStarts[i], jumpToCatchPos - 1);
            }
            
            // If no exception, execute the try block
            GenerateNode(tryCatch.TryBlock);
            
            // Jump to the end
            EmitPush(0); // Placeholder, will be filled in later
            int jumpToEndPos = _instructions.Count;
            _instructions.Add(new Instruction(Opcode.JUMP));
            
            // Add the jump destination to pending jumps
            AddPendingJump(tryFinalEnd, jumpToEndPos - 1);
            
            // Generate code for each catch clause
            for (int i = 0; i < tryCatch.CatchClauses.Count; i++)
            {
                int catchPos = _instructions.Count;
                _jumpDestinations[catchStarts[i]] = catchPos;
                _instructions.Add(new Instruction(Opcode.JUMPDEST));
                
                // Generate code for the catch body
                GenerateNode(tryCatch.CatchClauses[i].Body);
                
                // Jump to the end
                EmitPush(0); // Placeholder, will be filled in later
                int jumpToFinalEndPos = _instructions.Count;
                _instructions.Add(new Instruction(Opcode.JUMP));
                
                // Add the jump destination to pending jumps
                AddPendingJump(tryFinalEnd, jumpToFinalEndPos - 1);
            }
            
            // Add a jump destination for the final end
            int finalEndPos = _instructions.Count;
            _jumpDestinations[tryFinalEnd] = finalEndPos;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
        }

        // Counter for generating unique labels
        private int _labelCounter = 0;

        /// <summary>
        /// Generate code for a binary expression
        /// </summary>
        private void GenerateBinaryExpression(BinaryExpressionNode binExpr)
        {
            // Generate code for the left and right operands
            GenerateNode(binExpr.Left);
            GenerateNode(binExpr.Right);
            
            // Generate the operation
            switch (binExpr.Operator)
            {
                case "+":
                    _instructions.Add(new Instruction(Opcode.ADD));
                    break;
                case "-":
                    _instructions.Add(new Instruction(Opcode.SUB));
                    break;
                case "*":
                    _instructions.Add(new Instruction(Opcode.MUL));
                    break;
                case "/":
                    _instructions.Add(new Instruction(Opcode.DIV));
                    break;
                case "%":
                    _instructions.Add(new Instruction(Opcode.MOD));
                    break;
                case "==":
                    _instructions.Add(new Instruction(Opcode.EQ));
                    break;
                case "!=":
                    _instructions.Add(new Instruction(Opcode.EQ));
                    _instructions.Add(new Instruction(Opcode.ISZERO));
                    break;
                case "<":
                    _instructions.Add(new Instruction(Opcode.LT));
                    break;
                case ">":
                    _instructions.Add(new Instruction(Opcode.GT));
                    break;
                case "<=":
                    _instructions.Add(new Instruction(Opcode.GT));
                    _instructions.Add(new Instruction(Opcode.ISZERO));
                    break;
                case ">=":
                    _instructions.Add(new Instruction(Opcode.LT));
                    _instructions.Add(new Instruction(Opcode.ISZERO));
                    break;
                case "&&":
                    _instructions.Add(new Instruction(Opcode.AND));
                    break;
                case "||":
                    _instructions.Add(new Instruction(Opcode.OR));
                    break;
                default:
                    throw new NotImplementedException($"Operator not implemented: {binExpr.Operator}");
            }
        }

        /// <summary>
        /// Generate code for a literal value
        /// </summary>
        private void GenerateLiteral(LiteralNode literal)
        {
            switch (literal.Type)
            {
                case "number":
                    // For simplicity, we'll just convert the string to an int
                    // In a real compiler, you'd need more type checking
                    if (int.TryParse(literal.Value, out int intValue))
                    {
                        EmitPush(intValue);
                    }
                    else if (decimal.TryParse(literal.Value, out decimal decimalValue))
                    {
                        // This is a simplification; Solidity uses fixed-point for decimals
                        EmitPush((int)decimalValue);
                    }
                    else
                    {
                        throw new Exception($"Invalid number literal: {literal.Value}");
                    }
                    break;
                
                case "string":
                    // For strings, we'll use a simple encoding scheme
                    // In practice, you'd need a more complex approach
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(literal.Value);
                    
                    // Push the length of the string
                    EmitPush(bytes.Length);
                    
                    // Push the string content
                    // This is a simplification; in reality, you'd store the string in memory
                    EmitPush(bytes);
                    break;
                
                case "bool":
                    EmitPush(literal.Value.ToLower() == "true" ? 1 : 0);
                    break;
                
                case "hex":
                    // Remove the "0x" prefix and convert to bytes
                    string hex = literal.Value.StartsWith("0x") ? literal.Value.Substring(2) : literal.Value;
                    byte[] hexBytes = new byte[hex.Length / 2];
                    for (int i = 0; i < hex.Length; i += 2)
                    {
                        hexBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                    }
                    EmitPush(hexBytes);
                    break;
                
                default:
                    throw new NotImplementedException($"Literal type not implemented: {literal.Type}");
            }
        }

        /// <summary>
        /// Generate code for a variable reference
        /// </summary>
        private void GenerateVariableReference(VariableReferenceNode varRef)
        {
            // Get the variable's storage slot
            int slot = _symbols.GetVariableSlot(varRef.Name);
            
            // Push the slot onto the stack
            EmitPush(slot);
            
            // Load the value from storage
            _instructions.Add(new Instruction(Opcode.SLOAD));
        }

        /// <summary>
        /// Generate code for an assignment statement
        /// </summary>
        private void GenerateAssignment(AssignmentNode assignment)
        {
            // Generate code to evaluate the right-hand side
            GenerateNode(assignment.Value);
            
            // Get the variable's storage slot
            int slot = _symbols.GetVariableSlot(assignment.Variable.Name);
            
            // Push the slot onto the stack
            EmitPush(slot);
            
            // Store the value
            _instructions.Add(new Instruction(Opcode.SSTORE));
        }

        /// <summary>
        /// Generate code for a function call
        /// </summary>
        private void GenerateFunctionCall(FunctionCallNode funcCall)
        {
            // Generate code for each argument
            // Push them onto the stack in reverse order (to match EVM calling convention)
            for (int i = funcCall.Arguments.Count - 1; i >= 0; i--)
            {
                GenerateNode(funcCall.Arguments[i]);
            }
            
            // For internal functions
            if (_symbols.IsFunctionDeclared(funcCall.Name))
            {
                // Get the function's offset
                int offset = _symbols.GetFunctionOffset(funcCall.Name);
                
                // Push the return address
                int returnAddr = _instructions.Count + 3; // 3 instructions from here
                EmitPush(returnAddr);
                
                // Push the function address
                EmitPush(offset);
                
                // Jump to the function
                _instructions.Add(new Instruction(Opcode.JUMP));
                
                // Add a JUMPDEST for the return address
                _instructions.Add(new Instruction(Opcode.JUMPDEST));
            }
            else
            {
                // For external functions or built-in functions
                switch (funcCall.Name)
                {
                    case "require":
                        // Push the invalid address (for revert)
                        EmitPush(0);
                        
                        // Push the length of the revert data (0)
                        EmitPush(0);
                        
                        // Check the condition and revert if false
                        _instructions.Add(new Instruction(Opcode.JUMPI));
                        _instructions.Add(new Instruction(Opcode.REVERT));
                        break;
                    
                    // More built-in functions can be added here
                    
                    default:
                        throw new Exception($"Unknown function: {funcCall.Name}");
                }
            }
        }

        /// <summary>
        /// Generate code for a return statement
        /// </summary>
        private void GenerateReturnStatement(ReturnStatementNode returnStmt)
        {
            if (returnStmt.Expression != null)
            {
                // Generate code to evaluate the expression
                GenerateNode(returnStmt.Expression);
                
                // Save the value to memory
                // In a real compiler, you'd need a more sophisticated memory manager
                _instructions.Add(new Instruction(Opcode.PUSH1, new byte[] { 0 })); // Memory offset
                _instructions.Add(new Instruction(Opcode.MSTORE));
                
                // Return the value from memory
                _instructions.Add(new Instruction(Opcode.PUSH1, new byte[] { 0 })); // Memory offset
                _instructions.Add(new Instruction(Opcode.PUSH1, new byte[] { 32 })); // Size (32 bytes)
                _instructions.Add(new Instruction(Opcode.RETURN));
            }
            else
            {
                // Return with no value
                _instructions.Add(new Instruction(Opcode.PUSH1, new byte[] { 0 })); // Memory offset
                _instructions.Add(new Instruction(Opcode.PUSH1, new byte[] { 0 })); // Size (0 bytes)
                _instructions.Add(new Instruction(Opcode.RETURN));
            }
        }

        /// <summary>
        /// Generate code for a contract
        /// </summary>
        private void GenerateContract(ContractNode contract)
        {
            // For each contract member
            foreach (AstNode member in contract.Members)
            {
                GenerateNode(member);
            }
        }

        /// <summary>
        /// Generate code for a function
        /// </summary>
        private void GenerateFunction(FunctionNode function)
        {
            // Create a label for the function
            string label = function.Name;
            
            // Add a JUMPDEST instruction at the function entry point
            int offset = _instructions.Count;
            _jumpDestinations[label] = offset;
            _instructions.Add(new Instruction(Opcode.JUMPDEST));
            
            // Generate code for each parameter
            foreach (ParameterNode parameter in function.Parameters)
            {
                // For now, just declare the parameter in the symbol table
                _symbols.DeclareVariable(parameter.Name);
            }
            
            // Generate code for the function body
            if (function.Body != null)
            {
                GenerateNode(function.Body);
            }
            
            // Add a RETURN instruction at the end
            _instructions.Add(new Instruction(Opcode.RETURN));
        }

        /// <summary>
        /// Generate code for a block
        /// </summary>
        private void GenerateBlock(BlockNode block)
        {
            // Generate code for each statement in the block
            foreach (AstNode statement in block.Statements)
            {
                GenerateNode(statement);
            }
        }

        /// <summary>
        /// Generate code for a variable declaration
        /// </summary>
        private void GenerateVariableDeclaration(VariableDeclarationNode varDecl)
        {
            // Declare the variable in the symbol table
            _symbols.DeclareVariable(varDecl.Name);
            
            // If there's an initializer, generate code for it
            if (varDecl.Initializer != null)
            {
                // Generate code to push the value onto the stack
                GenerateNode(varDecl.Initializer);
                
                // Store the value in the variable's slot
                int slot = _symbols.GetVariableSlot(varDecl.Name);
                _instructions.Add(new Instruction(Opcode.PUSH1, new byte[] { (byte)slot }));
                _instructions.Add(new Instruction(Opcode.SSTORE));
            }
        }

        /// <summary>
        /// Emits a PUSH instruction based on the size of the value
        /// </summary>
        private void EmitPush(byte[] value)
        {
            if (value.Length == 0)
            {
                _instructions.Add(new Instruction(Opcode.PUSH0));
            }
            else if (value.Length == 1)
            {
                _instructions.Add(new Instruction(Opcode.PUSH1, value));
            }
            else if (value.Length <= 32)
            {
                // Select the appropriate PUSH opcode based on the value length
                Opcode pushOpcode = (Opcode)((int)Opcode.PUSH1 + value.Length - 1);
                _instructions.Add(new Instruction(pushOpcode, value));
            }
            else
            {
                throw new Exception("Value too large for PUSH instruction");
            }
        }

        /// <summary>
        /// Emits a PUSH instruction for an integer value
        /// </summary>
        private void EmitPush(int value)
        {
            // Convert the int to the minimum number of bytes required
            byte[] bytes;
            if (value == 0)
            {
                _instructions.Add(new Instruction(Opcode.PUSH0));
                return;
            }
            else if (value > 0 && value < 256)
            {
                bytes = new byte[] { (byte)value };
            }
            else
            {
                // This is a simplification; in practice, you'd want to use the minimum
                // number of bytes required to represent the value
                bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes); // EVM is big-endian
            }
            
            EmitPush(bytes);
        }

        /// <summary>
        /// Adds a pending jump to be resolved later
        /// </summary>
        private void AddPendingJump(string label, int instructionIndex)
        {
            if (!_pendingJumps.TryGetValue(label, out List<int> indices))
            {
                indices = new List<int>();
                _pendingJumps[label] = indices;
            }
            
            indices.Add(instructionIndex);
        }

        /// <summary>
        /// Resolves all pending jumps
        /// </summary>
        private void ResolvePendingJumps()
        {
            foreach (var pair in _pendingJumps)
            {
                string label = pair.Key;
                List<int> indices = pair.Value;
                
                if (!_jumpDestinations.TryGetValue(label, out int destination))
                {
                    throw new Exception($"Undefined jump label: {label}");
                }
                
                foreach (int index in indices)
                {
                    Instruction instruction = _instructions[index];
                    // Replace the operand with the actual destination
                    byte[] bytes = BitConverter.GetBytes(destination);
                    Array.Reverse(bytes); // EVM is big-endian
                    _instructions[index] = new Instruction(instruction.Opcode, bytes);
                }
            }
        }
    }

    /// <summary>
    /// Main entry point for the compiler
    /// </summary>
    public class SolidityCompiler
    {
        public static byte[] CompileString(string source)
        {
            // 1. Tokenize
            SolidityLexer lexer = new SolidityLexer(source);
            List<Token> tokens = lexer.Tokenize();
            
            // Print tokens for debugging
            Console.WriteLine("Tokens:");
            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
            
            // 2. Parse
            SolidityParser parser = new SolidityParser(tokens);
            List<AstNode> ast = parser.Parse();
            
            // Print AST for debugging
            Console.WriteLine("\nAST:");
            foreach (AstNode node in ast)
            {
                Console.WriteLine(node);
            }
            
            // 3. Generate code
            CodeGenerator generator = new CodeGenerator();
            byte[] bytecode = generator.Generate(ast);
            
            // Print bytecode for debugging
            Console.WriteLine("\nBytecode:");
            Console.WriteLine(BitConverter.ToString(bytecode).Replace("-", ""));
            
            return bytecode;
        }
        
        // public static void Main(string[] args)
        // {
        //     if (args.Length == 0)
        //     {
        //         Console.WriteLine("Usage: SolidityCompiler <filename>");
        //         return;
        //     }
            
        //     string source = System.IO.File.ReadAllText(args[0]);
        //     byte[] bytecode = CompileString(source);
            
        //     // Write bytecode to a file
        //     string outputPath = Path.ChangeExtension(args[0], ".bin");
        //     File.WriteAllBytes(outputPath, bytecode);
        //     Console.WriteLine($"Compiled bytecode written to {outputPath}");
        // }
    }
}