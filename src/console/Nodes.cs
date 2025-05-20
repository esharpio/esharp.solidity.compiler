using System;
using System.Collections.Generic;

namespace SolidityCompiler
{
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