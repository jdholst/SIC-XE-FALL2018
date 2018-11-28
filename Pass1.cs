/*******************************************************************************
Name:        Jacob Holst
Assignment:  Assignment 3 (Pass 1)
Due Date:    10/24/18
Course:      CSc 354 Systems Programming
Instructor:  Gamradt

Description: This module contains functions to read source programs line-by-line
             by using an algorithm that reads the operation of an assembly
             directive, sets the location counter, and inserts symbols and
             literals based on the directive. The location counter is set by
             an increment on the format of an instruction, the size of a res
             keyword, or the value of a symbol define. Symbols and Literals are
             inserted into tables whenever encounters. At the end of the program,
             program length is calculated and the LiteralTable is dumped into
             the program.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SICXE
{
    /// <summary>
    /// Creates an instance containing functionality and data for the pass 1 portion
    /// of SICXE assembly compilation.
    /// </summary>
    public class Pass1 : Pass
    {
        // pass 1 data for internal use
        private string locationCounter;

        /// <summary>
        /// Contructor initializes program
        /// </summary>
        /// <param name="opcodes"> IN - SICXE instruction set </param>
        /// <param name="sourceProgram"> IN - Every line from the source program </param>
        public Pass1(OpcodeTable opcodes, string targetDirectory, string programName)
            : base(opcodes, targetDirectory, programName, ".tmp")
        {
            BeginNewProgram("Line#", "LC", "LABEL", "OPERATION", "OPERAND");
        }

        /// <summary>
        /// Displays error in source program.
        /// </summary>
        /// <param name="parsedSource"> IN - source program </param>
        /// <param name="ex"> IN - The error </param>
        protected override void ViewError(string[][] parsedSource, SyntaxException ex)
        {
            string label = string.Empty, operation = string.Empty, operand = string.Empty;
            string arrows = string.Empty;

            for (int i = 0; i < lineNumber + 1 && i < parsedSource.Length; i++)
            {
                label = parsedSource[i][0];
                operation = parsedSource[i][1];
                operand = parsedSource[i][2];
                Console.WriteLine("{0, -10} {1, -10} {2, -10}", label, operation, operand);
            }

            if (ex is IllegalInstructionException)
            {
                Array.ForEach(operation.ToCharArray(), x => arrows += "^");
                Console.WriteLine(arrows.PadLeft(11 + operation.Length));
                Console.WriteLine("Error (Line {0}): IllegalInstructionError: {1}", lineNumber + 1, ex.Message);
                Console.WriteLine();
            }
            else if(ex is InvalidSymbolException)
            {
                if ((ex as InvalidSymbolException).IsLabel)
                {
                    Array.ForEach(label.ToCharArray(), x => arrows += "^");
                    Console.WriteLine(arrows);
                }
                else
                {
                    Array.ForEach(operand.ToCharArray(), x => arrows += "^");
                    Console.WriteLine(arrows.PadLeft(22 + operand.Length));
                }

                Console.WriteLine("Error (Line {0}): InvalidSymbolError: {1}", lineNumber + 1, ex.Message);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Error (Line {0}): Error: {1}", lineNumber + 1, ex.Message);
            }

            BeginNewProgram("Line#", "LC", "LABEL", "OPERATION", "OPERAND");
        }

        /// <summary>
        /// Runs an algorithm that parses each line from an SICXE assembly program and sets their
        /// line number and location counter. Symbols and literals are insterted into their respective
        /// tables when encountered.
        /// </summary>
        /// <param name="program"> IN - All lines from the source program </param>
        protected override void CreateProgram(string[][] parsedLines)
        {
            if (programExists)
            {
                Console.WriteLine("An intermediate program already exists for current instance.\n"
                                  + "Type \"confirm\" to overwrite existing program. Otherwise press enter.");
                if(Console.ReadLine().ToUpper() == "CONFIRM")
                {
                    BeginNewProgram("Line#", "LC", "LABEL", "OPERATION", "OPERAND");
                }
                else
                {
                    return;
                }
            }

            int counter, totalLiteralLength = 0;
            string label = parsedLines[lineNumber][0],
                   operation = parsedLines[lineNumber][1],
                   operand = parsedLines[lineNumber][2];
            bool checkedFormat1 = false;

            if (operation == "START")
            {
                programName = label;
                startingAddress = operand;
                locationCounter = startingAddress;

                try
                {
                    symbols.Insert(new Symbol
                    {
                        Name = label,
                        Value = int.Parse(locationCounter, System.Globalization.NumberStyles.HexNumber),
                        RFlag = true,
                        IFlag = true,
                        MFlag = false
                    });
                }
                catch (InvalidSymbolException ex)
                {
                    throw new InvalidSymbolException(ex.Message, true);
                }
                AddGeneratedLine(locationCounter.PadLeft(5, '0'), label, operation, operand);

                label = parsedLines[lineNumber][0];
                operation = parsedLines[lineNumber][1];
                operand = parsedLines[lineNumber][2];
            }

            while (operation != "END")
            {
                counter = 0;

                if (label != string.Empty && operation != "EQU") // symbol exists and isn't symbol defining
                {
                    if (symbols.Search(label).Length != 0) // found
                    {
                        throw new InvalidSymbolException("Duplicate symbols", true);
                    }
                    else
                    {
                        try
                        {
                            symbols.Insert(new Symbol
                            {
                                Name = label,
                                Value = int.Parse(locationCounter, System.Globalization.NumberStyles.HexNumber),
                                RFlag = true,
                                IFlag = true,
                                MFlag = false
                            });
                        }
                        catch (InvalidSymbolException ex)
                        {
                            throw new InvalidSymbolException(ex.Message, true);
                        }
                    }
                }

                if (opcodes.Search(operation, out Opcode opcode))
                {
                    counter = opcode.Format;
                }
                else if (operation.ToUpper() == "BASE")
                {
                    AddGeneratedLine(locationCounter.PadLeft(5, '0'), label, operation, operand);
                    label = parsedLines[lineNumber][0];
                    operation = parsedLines[lineNumber][1];
                    operand = parsedLines[lineNumber][2];
                    continue;
                }
                else if (operation.ToUpper() == "WORD")
                {
                    counter = 3;
                }
                else if (operation.ToUpper() == "RESW")
                {
                    counter = int.Parse(operand) * 3;
                }
                else if (operation.ToUpper() == "RESB")
                {
                    counter = int.Parse(operand);
                }
                else if (operation.ToUpper() == "BYTE")
                {
                    counter = ExpressionHandler.GetConstLength(operand);
                }
                else if (operation.ToUpper() == "EQU")
                {
                    DefineSymbol(label, operand);

                    label = parsedLines[lineNumber][0];
                    operation = parsedLines[lineNumber][1];
                    operand = parsedLines[lineNumber][2];
                    continue;
                }
                else if (operation.ToUpper() == "EXTDEF" || operation.ToUpper() == "EXTREF")
                {
                    AddGeneratedLine(locationCounter.PadLeft(5, '0'), label, operation, operand);
                    label = parsedLines[lineNumber][0];
                    operation = parsedLines[lineNumber][1];
                    operand = parsedLines[lineNumber][2];
                    continue;
                }
                else
                {
                    if (checkedFormat1)
                    {
                        // shift back and throw error
                        operand = operation;
                        operation = label;
                        label = string.Empty;
                        throw new IllegalInstructionException($"Operation {operation} does not exist");
                    }
                    else
                    {
                        // shift left and run line again (checks for format 1 instruction)
                        label = operation;
                        operation = operand;
                        operand = string.Empty;
                        checkedFormat1 = true;
                        continue;
                    }
                }

                try
                {
                    if (checkedFormat1 ? false : operand.Length > 0 ? operand[0] == '=' : false)
                    {
                        literals.Insert(operand);
                    }
                }
                catch (FormatException ex)
                {
                    throw new InvalidSymbolException(ex.Message, false);
                }
                finally
                {
                    checkedFormat1 = false;
                }

                AddGeneratedLine(locationCounter.PadLeft(5, '0'), label, operation, operand);

                locationCounter = (int.Parse(locationCounter, System.Globalization.NumberStyles.HexNumber) + counter).ToString("X");

                label = parsedLines[lineNumber][0];
                operation = parsedLines[lineNumber][1];
                operand = parsedLines[lineNumber][2];
            }

            AddGeneratedLine(locationCounter.PadLeft(5, '0'), label, operation, operand);

            foreach(var literal in literals)
            {
                totalLiteralLength += literal.Length;
            }

            programLength = ((int.Parse(locationCounter, System.Globalization.NumberStyles.HexNumber)
                          - int.Parse(startingAddress, System.Globalization.NumberStyles.HexNumber)) + totalLiteralLength).ToString("X");
            DumpLiterals();

            generatedProgram.AppendLine($"Program Length: {programLength}");
            programExists = true;
        }

        /// <summary>
        /// Parses each line containing a label (may be blank), operation, and operand.
        /// Ignores lines with full-line comments (marked by '$' symbol).
        /// </summary>
        /// <param name="lines"> program lines </param>
        /// <returns> [program lines][label, operation, operand] </returns>
        protected override string[][] ParseLines(string[] lines)
        {
            List<string[]> parsedLines = new List<string[]>();
            foreach (var line in lines)
            {
                var noComments = line.Contains('$') ? line.Remove(line.IndexOf('$')).Replace('\t', ' ') : line.Replace('\t', ' ');

                if (noComments == string.Empty) continue;

                var splitLine = noComments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] parsedLine = new string[3];

                if (splitLine.Length == 3)
                {
                    parsedLine[0] = splitLine[0];
                    parsedLine[1] = splitLine[1];
                    parsedLine[2] = splitLine[2];
                }
                else if (splitLine.Length <= 2 && splitLine.Length > 0) // does not contain symbol so make label empty
                {
                    parsedLine[0] = string.Empty;
                    parsedLine[1] = splitLine[0];
                    parsedLine[2] = splitLine.Length == 2 ? splitLine[1] : string.Empty;
                }
                else
                {
                    throw new IllegalInstructionException("Invalid Assembly Line");
                }

                AddInputLine(parsedLine[0], parsedLine[1], parsedLine[2]);
                parsedLines.Add(parsedLine);
            }

            return parsedLines.ToArray();
        }

        /// <summary>
        /// Adds a line to the program with its respective line number.
        /// </summary>
        /// <param name="args"></param>
        protected override void AddGeneratedLine(params string[] args)
        {
            lineNumber++;
            var argsWithLNum = new LinkedList<string>(args);
            argsWithLNum.AddFirst(lineNumber.ToString().PadLeft(2, '0'));
            args = new List<string>(argsWithLNum).ToArray();
            lineNumber--;

            base.AddGeneratedLine(args);
        }

        /// <summary>
        /// EQU operation containing symbol defining functionality.
        /// </summary>
        /// <param name="name"> IN - label -> symbol name </param>
        /// <param name="value"> IN - operand -> symbol value </param>
        private void DefineSymbol(string name, string value)
        {
            bool rFlag;
            Symbol symbol;
            if(int.TryParse(value, out int intVal))
            {
                rFlag = false;
            }
            else if(value == "*")
            {
                rFlag = true;
                intVal = int.Parse(locationCounter, System.Globalization.NumberStyles.HexNumber);
            }
            else if(value.Contains("+") || value.Contains("-"))
            {
                symbol = ExpressionHandler.ArithmeticEvaluation(value, symbols);

                rFlag = symbol.RFlag;
                intVal = symbol.Value;
            }
            else
            {
                throw new FormatException();
            }

            try
            {
                symbols.Insert(new Symbol
                {
                    Name = name,
                    Value = intVal,
                    RFlag = rFlag,
                    IFlag = true,
                    MFlag = false
                });
            }
            catch (InvalidSymbolException ex)
            {
                throw new InvalidSymbolException(ex.Message, true);
            }

            AddGeneratedLine(intVal.ToString("X").PadLeft(5, '0'), name, "EQU", value);
        }

        /// <summary>
        /// "Dumps" literals into the intermediate program. Denoted by '*' in the label.
        /// </summary>
        private void DumpLiterals()
        {
            foreach (var literal in literals)
            {
                AddGeneratedLine(locationCounter.PadLeft(5, '0'), "*", literal.Name, string.Empty);
                literals.SetLiteralAddress(literal, locationCounter); // literal address becomes location counter

                locationCounter = (int.Parse(locationCounter, System.Globalization.NumberStyles.HexNumber) + literal.Length).ToString("X");
            }
        }
    }
}
