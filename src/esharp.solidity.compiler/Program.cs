using System;

namespace esharp.solidity.compiler
{
    class Program
    {
        static void Main(String[] args)
        {
            while (true)
            {
                Console.Write("> ");
                String line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                // var syntaxTree = SyntaxTree.Parse(line);
            }
        }
    }
}
