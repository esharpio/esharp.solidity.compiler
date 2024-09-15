using System;

namespace esharp.solidity.compiler.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }        
    }
}