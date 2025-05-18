using System;
using System.Collections.Generic;

namespace esharp.solidity.compiler
{
    /// <summary>
    /// Represents an assignment statement (e.g., x = 5)
    /// </summary>
    public class AssignmentNode : AstNode
    {
        public VariableReferenceNode Variable { get; }
        public AstNode Value { get; }

        public AssignmentNode(VariableReferenceNode variable, AstNode value)
        {
            Variable = variable;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Variable} = {Value}";
        }
    }

    /// <summary>
    /// Represents a literal value (number, string, etc.)
    /// </summary>
    public class LiteralNode : AstNode
    {
        public string Value { get; }
        public string Type { get; }

        public LiteralNode(string value, string type)
        {
            Value = value;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Type} literal: {Value}";
        }
    }

    /// <summary>
    /// Represents a variable reference
    /// </summary>
    public class VariableReferenceNode : AstNode
    {
        public string Name { get; }

        public VariableReferenceNode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents a function call
    /// </summary>
    public class FunctionCallNode : AstNode
    {
        public string Name { get; }
        public List<AstNode> Arguments { get; } = new List<AstNode>();

        public FunctionCallNode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Arguments)})";
        }
    }

    /// <summary>
    /// Represents a return statement
    /// </summary>
    public class ReturnStatementNode : AstNode
    {
        public AstNode Expression { get; }

        public ReturnStatementNode(AstNode expression)
        {
            Expression = expression;
        }

        public override string ToString()
        {
            return $"return {Expression}";
        }
    }
}