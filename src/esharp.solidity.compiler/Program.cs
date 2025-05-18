using System;
using System.IO;

namespace esharp.solidity.compiler
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: SolidityCompiler <filename>");
                return;
            }
            
            string source = System.IO.File.ReadAllText(args[0]);
            byte[] bytecode = SolidityCompiler.CompileString(source);
            
            // Write bytecode to a file
            string outputPath = Path.ChangeExtension(args[0], ".bin");
            File.WriteAllBytes(outputPath, bytecode);
            Console.WriteLine($"Compiled bytecode written to {outputPath}");
        }
        
        // static void Main(String[] args)
        // {
        //     while (true)
        //     {
        //         Console.Write("> ");
        //         String line = Console.ReadLine();
        //         if (string.IsNullOrWhiteSpace(line))
        //             return;

        //         // var syntaxTree = SyntaxTree.Parse(line);
        //     }
        // }
    }
}
