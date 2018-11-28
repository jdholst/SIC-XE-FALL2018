using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SICXE
{
    public abstract class Pass
    {
        protected readonly OpcodeTable opcodes;
        protected bool programExists;
        protected int lineNumber;

        protected string programName;
        protected string programLength;
        protected string startingAddress;
        protected string generatedFile;
        protected StringBuilder generatedProgram;
        protected string inputFile;
        protected StringBuilder inputProgram;
        protected SymbolTable symbols;
        protected LiteralTable literals;

        public string ProgramName { get { return programName; } }
        public string ProgramLength { get { return programLength; } }
        public string StartingAddress { get { return startingAddress; } }
        public string GeneratedProgram { get { return generatedProgram.ToString(); } }
        public string GeneratedFile { get { return generatedFile; } }
        public string InputProgram { get { return inputProgram.ToString(); } }
        public string InputFile { get { return inputFile; } }
        public SymbolTable Symbols { get { return symbols; } }
        public LiteralTable Literals { get { return literals; } }

        /// <summary>
        /// Contructor initializes program
        /// </summary>
        /// <param name="opcodes"> IN - SICXE instruction set </param>
        /// <param name="sourceProgram"> IN - Every line from the source program </param>
        protected Pass(OpcodeTable opcodes, string targetDirectory, string programName, string programExtension)
        {
            this.opcodes = opcodes;
            generatedFile = Path.Combine(targetDirectory, programName + programExtension);
        }

        /// <summary>
        /// Runs pass with parsing and error checking
        /// </summary>
        /// <param name="program"> IN - File path of the source program </param>
        public virtual void RunPass(string inputFilePath)
        {
            inputFile = inputFilePath;
            string[][] parsedLines = ParseLines(File.ReadAllLines(inputFilePath));

            try
            {
                CreateProgram(parsedLines);
            }
            catch (SyntaxException ex)
            {
                ViewError(parsedLines, ex);
            }

            File.WriteAllText(generatedFile, generatedProgram.ToString());
        }

        /// <summary>
        /// Used by the RunPass function to parse each line from the input program
        /// </summary>
        /// <param name="programLines"></param>
        /// <returns></returns>
        protected abstract string[][] ParseLines(string[] programLines);
        protected abstract void CreateProgram(string[][] parsedLines);
        protected abstract void ViewError(string[][] parsedInput, SyntaxException ex);

        /// <summary>
        /// Initializes Intermediate Program data.
        /// </summary>
        protected virtual void BeginNewProgram(params string[] headerArgs)
        {
            symbols = new SymbolTable();
            literals = new LiteralTable();

            generatedProgram = new StringBuilder();
            inputProgram = new StringBuilder();

            lineNumber = 0;

            // did not call AddGeneratedLine because it is virtual
            foreach (var arg in headerArgs)
            {
                generatedProgram.Append(string.Format("{0, -15}", arg));
            }
            generatedProgram.AppendLine();

            
        }

        /// <summary>
        /// Adds a line to the generated program.
        /// </summary>
        protected virtual void AddGeneratedLine(params string[] headerArgs)
        {
            lineNumber++;
            foreach (var arg in headerArgs)
            {
                generatedProgram.Append(string.Format("{0, -15}", arg)); 
            }
            generatedProgram.AppendLine();
        }

        /// <summary>
        /// Adds a line to the input program. Generally used for error checking.
        /// </summary>
        protected virtual void AddInputLine(params string[] headerArgs)
        {
            foreach (var arg in headerArgs)
            {
                inputProgram.Append(string.Format("{0, -1}", arg));
            }
            inputProgram.AppendLine();
        }
    }
}
