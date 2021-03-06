using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SICXE
{
    /// <summary>
    /// Base class for creating a derived pass class that runs through an assembly input program
    /// and outputs a generated assembly program.
    /// </summary>
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

        /// <summary>
        /// Opcodes used by the pass.
        /// </summary>
        public OpcodeTable Opcodes {  get { return opcodes; } }

        /// <summary>
        /// Name of the program set using START directive.
        /// </summary>
        public string ProgramName { get { return programName; } }

        /// <summary>
        /// Length of program.
        /// </summary>
        public string ProgramLength { get { return programLength; } }

        /// <summary>
        /// Starting address of the program set using START directive.
        /// </summary>
        public string StartingAddress { get { return startingAddress; } }

        /// <summary>
        /// Full program generated by the pass in string form.
        /// </summary>
        public string GeneratedProgram { get { return generatedProgram.ToString(); } }

        /// <summary>
        /// Path of generated program.
        /// </summary>
        public string GeneratedFile { get { return generatedFile; } }

        /// <summary>
        /// Input program in string form.
        /// </summary>
        public string InputProgram { get { return inputProgram.ToString(); } }

        /// <summary>
        /// Path of input file.
        /// </summary>
        public string InputFile { get { return inputFile; } }

        /// <summary>
        /// Table containing symbols defined or referenced in the program.
        /// </summary>
        public SymbolTable Symbols { get { return symbols; } }

        /// <summary>
        /// Table containing all literals defined in the program
        /// </summary>
        public LiteralTable Literals { get { return literals; } }

        /// <summary>
        /// Contructor initializes opcode instructions and the path of the generated program.
        /// </summary>
        /// <param name="opcodes"> IN - SICXE instruction set </param>
        /// <param name="sourceProgram"> IN - Every line from the source program </param>
        protected Pass(OpcodeTable opcodes, string targetDirectory, string programName, string programExtension)
        {
            this.opcodes = opcodes;
            generatedFile = Path.Combine(targetDirectory, programName + programExtension);
        }

        /// <summary>
        /// Runs pass with parsing and error checking.
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
        /// Parses each line from the input program.
        /// </summary>
        /// <param name="programLines"> Each line of the program </param>
        /// <returns> Each token of every line </returns>
        protected abstract string[][] ParseLines(string[] programLines);

        /// <summary>
        /// Generates the output program.
        /// </summary>
        /// <param name="parsedLines"> 2D array containing each line and each token of the input program </param>
        protected abstract void CreateProgram(string[][] parsedLines);

        /// <summary>
        /// Displays error information to the console.
        /// </summary>
        /// <param name="parsedInput"> 2D array containing each line and each token of the input program </param>
        /// <param name="ex"> Contains error information </param>
        protected abstract void ViewError(string[][] parsedInput, SyntaxException ex);

        /// <summary>
        /// Initializes Intermediate Program data.
        /// </summary>
        /// <param name="headerArgs"> To set the header of the program file </param>
        protected virtual void BeginNewProgram(params string[] headerArgs)
        {
            symbols = new SymbolTable();
            literals = new LiteralTable();

            generatedProgram = new StringBuilder();
            inputProgram = new StringBuilder();

            lineNumber = 0;

            foreach (var arg in headerArgs)
            {
                generatedProgram.Append(string.Format("{0, -15}", arg));
            }
            generatedProgram.AppendLine();

            
        }

        /// <summary>
        /// Adds a line to the generated program.
        /// </summary>
        /// <param name="args"> Each item to be printed on the line </param>
        protected virtual void AddGeneratedLine(params string[] args)
        {
            lineNumber++;
            foreach (var arg in args)
            {
                generatedProgram.Append(string.Format("{0, -15}", arg)); 
            }
            generatedProgram.AppendLine();
        }

        /// <summary>
        /// Adds a line to the input program. Generally used for error checking.
        /// </summary>
        /// <param name="args"> Each item to be printed on the line </param>
        protected virtual void AddInputLine(params string[] args)
        {
            foreach (var arg in args)
            {
                inputProgram.Append(string.Format("{0, -1}", arg));
            }
            inputProgram.AppendLine();
        }
    }
}
