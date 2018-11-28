using System;
using System.IO;

namespace SICXE
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
            string fileName;

            if (args.Length != 0)
            {
                fileName = args[0];
            }
            else
            {
                fileName = string.Empty;
            }

            while (!File.Exists(Path.Combine(currentDirectory, fileName)))
            {
                Console.WriteLine("File does not exist. Please enter valid file: ");
                fileName = Console.ReadLine();
                Console.Clear();
            }

            OpcodeTable opcodes = new OpcodeTable(File.ReadAllLines(Path.Combine(currentDirectory, "OPCODES.dat")));

            var pass = new Pass1(opcodes, currentDirectory, fileName.Remove(fileName.IndexOf('.')));

            pass.RunPass(Path.Combine(currentDirectory, fileName));

            var pass2 = new Pass2(pass);

            pass2.RunPass(Path.Combine(currentDirectory, fileName.Replace(".txt", ".tmp").Replace(".asm", ".tmp")));

            Console.WriteLine("Listing File: ");
            Console.WriteLine(pass2.GeneratedProgram);

            Console.WriteLine("Object Program: ");
            Console.WriteLine(pass2.ObjectProgram);

            Console.ReadKey();
        }
    }
}
