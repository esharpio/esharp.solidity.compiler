using System.Collections.Generic;
using System.Linq;

namespace esharp.solidity.compiler.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();
    }
}