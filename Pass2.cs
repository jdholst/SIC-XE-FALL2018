using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SICXE
{
    public enum Register{ A = 0, X = 1, L = 2, B = 3, S = 4, T = 5, F = 6, PC = 8, SW = 9 }
    public class Pass2 : Pass
    {
        private Pass1 pass1Data;

        private StringBuilder objectProgram;
        private readonly string objectFile;

        public Pass2(OpcodeTable opcodes, Pass1 pass1) 
            : base(opcodes, 
                  Path.Combine(pass1.GeneratedFile, ".."), 
                  Path.GetFileNameWithoutExtension(pass1.GeneratedFile), 
                  ".txt")
            // pass directory and name to base constructor purely through pass1 data
        {
            objectFile = Path.Combine(
                                Path.Combine(pass1.GeneratedFile, ".."),
                                Path.GetFileNameWithoutExtension(pass1.GeneratedFile) + ".o"
                             );
            pass1Data = pass1;
            BeginNewProgram("Line#", "LC", "LABEL", "OPERATION", "OPERAND", "OBJCODE");
        }

        public override void RunPass(string inputFilePath)
        {
            base.RunPass(inputFilePath);
            File.WriteAllText(objectFile, objectProgram.ToString());
        }

        protected override void BeginNewProgram(params string[] headerArgs)
        {
            base.BeginNewProgram(headerArgs);

            objectProgram = new StringBuilder();

            symbols = pass1Data.Symbols;
            literals = pass1Data.Literals;
            inputFile = pass1Data.InputFile;
            inputProgram = new StringBuilder(pass1Data.GeneratedFile);
            startingAddress = pass1Data.StartingAddress;
            programLength = pass1Data.ProgramLength;
            programName = pass1Data.ProgramName;
        }

        protected override void CreateProgram(string[][] parsedLines)
        {
            string currentLocation = parsedLines[lineNumber][0],
                   label = parsedLines[lineNumber][1],
                   operation = parsedLines[lineNumber][2],
                   operand = parsedLines[lineNumber][3];

            string opcodeAddress = string.Empty, 
                   objectCode = string.Empty,
                   locationCounter = currentLocation,
                   baseRegister = "0";
            byte[] objectSegment = new byte[4];
            bool hasObjectCode;
            int counter = 0;
            SymbolTable externalDefSymbols = new SymbolTable();
            List<ModificationRecord> modRecords = new List<ModificationRecord>();
            List<Record> records = new List<Record>();
            ReferenceRecord rRecord = null;
            DefineRecord dRecord = null;
            TextRecord textRecord;
            
            if (operation == "START")
            {
                AddGeneratedLine((lineNumber + 1).ToString(), currentLocation.PadLeft(5, '0'), label, operation, operand);
                records.Add(new HeaderRecord(label.ToUpper(), startingAddress, programLength));

                locationCounter = parsedLines[lineNumber + 1]?[0];
                currentLocation = parsedLines[lineNumber][0];
                label = parsedLines[lineNumber][1];
                operation = parsedLines[lineNumber][2];
                operand = parsedLines[lineNumber][3];
            }
            textRecord = new TextRecord(currentLocation);

            while(operation != "END")
            {
                counter = 0;
                hasObjectCode = false;
                if (opcodes.Search(operation, out Opcode opcode))
                {
                    counter = opcode.Format;
                    if(opcode.Format == 3 || opcode.Format == 4)
                    {
                        Symbol[] searchSymbols = symbols.Search(ExpressionHandler.ParseSymbolNames(operand));
                        Literal[] searchLiterals = literals.Search(operand);
                        if (searchSymbols.Length != 0)
                        {
                            opcodeAddress = ExpressionHandler.ArithmeticEvaluation(operand, symbols).Value.ToString("X");

                            if (opcode.Format == 4)
                            {
                                int i = 0;
                                char[] flags = GetFlags(operand);
                                string offsetLocation = (int.Parse(currentLocation, System.Globalization.NumberStyles.HexNumber) + 1).ToString("X");

                                foreach (var searchSymbol in searchSymbols)
                                {
                                    GetModification(ref modRecords, offsetLocation, "5", flags[i], searchSymbol);
                                    i++;
                                }
                            }
                        }
                        else if (searchLiterals.Length != 0)
                        {
                            opcodeAddress = searchLiterals[0].Address;
                        }
                        else
                        {
                            opcodeAddress = "0";
                        }
                    }
                    else
                    {
                        opcodeAddress = "0";
                    }

                    objectCode = AssembleObjectCode(opcode, operand, opcodeAddress, locationCounter, baseRegister);
                    hasObjectCode = true;
                }
                else if(operation.ToUpper() == "BASE")
                {
                    baseRegister = opcodeAddress;
                    hasObjectCode = false;
                }
                else if(operation.ToUpper() == "BYTE")
                {
                    objectCode = ExpressionHandler.GetConstValue(operand);
                    counter = ExpressionHandler.GetConstLength(operand);
                    hasObjectCode = true;
                }
                else if(operation.ToUpper() == "WORD")
                {
                    Symbol[] searchSymbols = symbols.Search(ExpressionHandler.ParseSymbolNames(operand));
                    objectCode = ExpressionHandler.ArithmeticEvaluation(operand, symbols).Value.ToString("X").PadLeft(6, '0');
                    counter = 3;
                    hasObjectCode = true;
                    
                    if (searchSymbols.Length != 0)
                    {
                        int i = 0;
                        char[] flags = GetFlags(operand);
                        foreach (var searchSymbol in searchSymbols)
                        {
                            GetModification(ref modRecords, currentLocation, "6", flags[i], searchSymbol);
                            i++;
                        }
                    }
                }
                else if(operation.ToUpper() == "EXTDEF")
                {
                    Symbol[] searchSymbols = symbols.Search(operand.Split(','));
                    externalDefSymbols.Insert(searchSymbols);

                    foreach (var define in searchSymbols)
                    {
                        try
                        {
                            dRecord = dRecord ?? new DefineRecord();
                            dRecord.AddEntry(new DefineRecordArgs(define.Value.ToString("X"), define.Name));
                        }
                        catch (RecordOverflowException)
                        {
                            records.Add(dRecord);
                            dRecord = new DefineRecord();
                            dRecord.AddEntry(new DefineRecordArgs(define.Value.ToString("X"), define.Name));
                        }
                    }

                    records.Add(dRecord);
                    dRecord = null;
                    hasObjectCode = false;
                }
                else if(operation.ToUpper() == "EXTREF")
                {
                    string[] references = operand.Split(',');
                    List<Symbol> symbols = new List<Symbol>();

                    foreach (var refer in references)
                    {
                        var reference = refer.Length > 4 ? refer.Remove(4).ToUpper() : refer.ToUpper();
                        symbols.Add(new Symbol
                        {
                            Name = reference,
                            Value = 0,
                            RFlag = false
                        });

                        try
                        {
                            rRecord = rRecord ?? new ReferenceRecord();
                            rRecord.AddEntry(new RecordArgs(reference));
                        }
                        catch (RecordOverflowException)
                        {
                            records.Add(rRecord);
                            rRecord = new ReferenceRecord();
                            rRecord.AddEntry(new RecordArgs(reference));
                        }
                    }

                    this.symbols.Insert(symbols.ToArray());

                    records.Add(rRecord);
                    rRecord = null;
                    hasObjectCode = false;
                }

                if (hasObjectCode)
                {
                    AddGeneratedLine((lineNumber + 1).ToString(), currentLocation.PadLeft(5, '0'), label, operation, operand, objectCode);

                    try
                    {
                        textRecord = textRecord ?? new TextRecord(currentLocation);
                        textRecord.AddEntry(new TextRecordArgs(objectCode, locationCounter));
                    }
                    catch (RecordOverflowException)
                    {
                        records.Add(textRecord);
                        textRecord = new TextRecord(currentLocation);
                        textRecord.AddEntry(new TextRecordArgs(objectCode, locationCounter));
                    } 
                }
                else
                {
                    AddGeneratedLine((lineNumber + 1).ToString(), currentLocation.PadLeft(5, '0'), label, operation, operand);
                    if (textRecord != null && textRecord.Size != 0)
                        records.Add(textRecord);

                    textRecord = null;
                }

                // need to move down location counter column since values actual LC is not displayed with EQU instruction
                int lcOffset = 1; // LC is always the next line down
                while (parsedLines.Length > lineNumber + lcOffset ? parsedLines[lineNumber + lcOffset][2] == "EQU" : false)
                    lcOffset++;

                currentLocation = parsedLines[lineNumber][0];
                if(parsedLines.Length > lineNumber + lcOffset) locationCounter = parsedLines[lineNumber + lcOffset][0];
                label = parsedLines[lineNumber][1];
                operation = parsedLines[lineNumber][2];
                operand = parsedLines[lineNumber][3];
            }

            AddGeneratedLine((lineNumber + 1).ToString(), currentLocation.PadLeft(5, '0'), label, operation, operand);
            if (textRecord != null && textRecord.Size != 0)
            {
                records.Add(textRecord);
                textRecord = new TextRecord(currentLocation);
            }

            foreach (var literal in literals)
            {
                objectCode = literal.Value;
                locationCounter = (int.Parse(locationCounter, System.Globalization.NumberStyles.HexNumber) + literal.Length).ToString("X");

                try
                {
                    textRecord = textRecord ?? new TextRecord(currentLocation);
                    textRecord.AddEntry(new TextRecordArgs(objectCode, locationCounter));
                }
                catch (RecordOverflowException)
                {
                    records.Add(textRecord);
                    textRecord = new TextRecord(currentLocation);
                    textRecord.AddEntry(new TextRecordArgs(objectCode, locationCounter));
                }

                currentLocation = parsedLines[lineNumber][0];
                label = parsedLines[lineNumber][1];
                operation = parsedLines[lineNumber][2];
                operand = parsedLines[lineNumber][3];
            }

            if (textRecord != null && textRecord.Size != 0)
            {
                records.Add(textRecord);
            }

            // dump modification records
            modRecords.ForEach(x => records.Add(x));

            records.Add(new EndRecord(startingAddress));

            // write object program
            records.ForEach(x => objectProgram.AppendLine(x.ToString()));

            FinalizeListing();
        }

        protected override string[][] ParseLines(string[] programLines)
        {
            List<string[]> parsedLines = new List<string[]>();
            string[] parsedLine = new string[4];

            int locIndex = programLines[0].IndexOf("LC"),
                labelIndex = programLines[0].IndexOf("LABEL"),
                operationIndex = programLines[0].IndexOf("OPERATION"),
                operandIndex = programLines[0].IndexOf("OPERAND");

            for (int i = 1; i < programLines.Length - 1; i++)
            {
                if (programLines.Length != 0)
                {
                    parsedLine = new string[4]; // allocate new space in memory for every entry
                    parsedLine[0] = programLines[i].Substring(locIndex).Split()[0];
                    parsedLine[1] = programLines[i].Substring(labelIndex).Split()[0];
                    parsedLine[2] = programLines[i].Substring(operationIndex).Split()[0];
                    parsedLine[3] = programLines[i].Substring(operandIndex).Split()[0];
                    parsedLines.Add(parsedLine); 
                }
            }

            return parsedLines.ToArray();
        }

        protected override void ViewError(string[][] parsedInput, SyntaxException ex)
        {
            throw new NotImplementedException();
        }

        private string AssembleObjectCode(Opcode opcode, string operand, string address, string locationCounter, string baseRegister)
        {
            byte[] objectSegment = ExpressionHandler.CheckAdressMode(operand);
            string objectCode = string.Empty;

            objectSegment[0] |= byte.Parse(opcode.Op[0].ToString(), System.Globalization.NumberStyles.HexNumber);
            objectSegment[1] |= byte.Parse(opcode.Op[1].ToString(), System.Globalization.NumberStyles.HexNumber);

            if(opcode.Format == 4)
            {
                objectSegment[2] |= 0b0001; // turn on E bit
                address = address.PadLeft(5, '0');
            }
            else if (opcode.Format == 3)
            {
                var symbolNames = ExpressionHandler.ParseSymbolNames(operand);
                if (int.TryParse(symbolNames.Length != 0 ? symbolNames[0] : "0", out int number))
                    // short-circuits if address comes in as 0 protecting against any exceptions being thrown due to invalid operand field
                {
                    address = number.ToString("X").PadLeft(3, '0');
                }
                else
                {
                    string register = "0";
                    int addressInt = int.Parse(address, System.Globalization.NumberStyles.HexNumber);

                    if (addressInt < 2047) // 0 - 07FF hex range
                    {
                        register = locationCounter;
                        objectSegment[2] |= 0b0010; // turn on P bit
                    }
                    else if (addressInt < 4096) // 0 - 1000 hex range
                    {
                        register = baseRegister;
                        objectSegment[2] |= 0b0100; // turn on B bit
                    }

                    address = (addressInt - int.Parse(register, System.Globalization.NumberStyles.HexNumber)
                                ).ToString("X").PadLeft(3, '0');
                    address = address.Remove(0, address.Length - 3);
                }
            }

            if (opcode.Format == 3 || opcode.Format == 4)
            {
                for (int i = 0; i < 3; i++)
                {
                    objectCode += ((int)objectSegment[i]).ToString("X");
                }
                objectCode = objectCode + address;
            }
            else if (opcode.Format == 2)
            {
                string[] registerNames = operand.Split(",", StringSplitOptions.RemoveEmptyEntries);
                byte[] registers = { 0, 0 };
                int i = 0;

                objectCode = opcode.Op;

                foreach (var registerName in registerNames)
                {
                    registers[i++] |= (byte)((Register)Enum.Parse(typeof(Register), registerName));
                }

                foreach (var reg in registers)
                {
                    objectCode += reg.ToString("X");
                }
            }
            else if(opcode.Format == 1)
            {
                objectCode = opcode.Op;
            }

            return objectCode;
        }

        private void GetModification(ref List<ModificationRecord> modRecords, string location, string size, char flag, Symbol symbol)
        {
            string name = string.Empty;
            bool hasRecord = false;
            
            if (symbol.IFlag == false)
            {
                name = symbol.Name;
                hasRecord = true;
            }
            else if (symbols.Search(symbol).Length != 0)
            {
                name = programName.Length > 4 ? programName.Remove(4) : programName;
                hasRecord = true;
            }

            if (hasRecord)
                modRecords.Add(new ModificationRecord(location, size, flag + name));

            hasRecord = false;
        }

        private char[] GetFlags(string operand)
        {
            List<char> signs = new List<char>();

            for (int i = 0; i < operand.Length; i++)
            {
                bool isNum = i + 1 < operand.Length ? int.TryParse(operand[i + 1].ToString(), out int num) : false;
                if (operand[i] == '+' && !isNum)
                    signs.Add('+');
                else if (operand[i] == '-' && !isNum)
                    signs.Add('-');
                else if(i == 0 && !isNum)
                    signs.Add('+');
            }

            return signs.ToArray();
        }

        private void FinalizeListing()
        {

            AddGeneratedLine();
            AddGeneratedLine($"Program Length = {programLength}");
            
            if(symbols.Count > 0)
            {
                AddGeneratedLine();
                AddGeneratedLine("Symbol Table");
                AddGeneratedLine("LABEL", "VALUE", "RFLAG", "IFLAG", "MFLAG");

                foreach (var symbol in symbols)
                {
                    AddGeneratedLine(symbol.Name, symbol.Value.ToString("X"),
                        symbol.RFlag ? "1" : "0" , symbol.IFlag ? "1" : "0", symbol.MFlag ? "1" : "0");
                }
            }

            if (literals.Count > 0)
            {
                AddGeneratedLine();
                AddGeneratedLine("Literal Table");
                AddGeneratedLine("LITERAL", "VALUE", "LENGTH", "ADDRESS");

                foreach (var literal in literals)
                {
                    AddGeneratedLine(literal.Name, literal.Value.ToString(), literal.Length.ToString(), literal.Address);
                }
            }
        }
    }
}
