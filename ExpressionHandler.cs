/*******************************************************************************
Name:        Jacob Holst
Assignment:  Assignment 2 (Expression Processing)
Due Date:    10/03/18
Course:      CSc 354 Systems Programming
Instructor:  Gamradt

Description: This static class contains functions to read expressions line-by-
             line, setting bits or inserting into the LiteralTable as it reads.
             Literals include characters and hexidecimals containing the value,
             length, and adress. Each process of the expression sets the n-bit
             (@), ibit (#), and x-bit (, X). The relocatable is set based on the 
             r-flag of the symbol. Relocatable is adjusted when symbols/values
             are added or subtracted together. This adjustment is dictated by a
             table showing all combinations of relocatable values.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace SICXE
{
    /// <summary>
    /// Static handler class containing methods to process lines of expressions and insert literals.
    /// </summary>
    public static class ExpressionHandler
    {

        /// <summary>
        /// Checks the n, i, and x bits and what kind of operation is used.
        /// </summary>
        /// <param name="expression"> IN </param>
        /// <param name="nBit"> IN/OUT </param>
        /// <param name="iBit"> IN/OUT </param>
        /// <param name="xBit"> IN/OUT </param>
        /// <param name="operation"> IN/OUT </param>
        public static byte[] CheckAdressMode(string expression)
        {
            bool isNumber = int.TryParse(expression, out int i);
            byte[] newByte = { 0, 0b0011, 0b1111, 0 };

            newByte[1] = expression.Contains('#') || isNumber ? (byte)(newByte[1] & 0b0001) : (byte)(newByte[1] | 0);
            newByte[1] = expression.Contains('@') ? (byte)(newByte[1] & 0b0010) : (byte)(newByte[1] | 0);
            newByte[2] = expression.Contains (",X") ? (byte)(newByte[2] & 0b1000) : (byte)(newByte[2] & 0);

            return newByte;
        }

        /// <summary>
        /// Evaluates both sides of an expression. Adds the values and adjusts the Relocatable.
        /// </summary>
        /// <param name="operation"> IN </param>
        /// <param name="values"> IN </param>
        /// <param name="symbols"> IN </param>
        /// <returns> Symbol </returns>
        public static Symbol ArithmeticEvaluation(string expression, SymbolTable symbolTable)
        {
            Symbol adjustedSymbol = new Symbol()
            {
                // default values
                IFlag = true,
                MFlag = false,
            };
            Symbol[] symbols = Array.Empty<Symbol>();
            int rightValue = 0, leftValue = 0;
            var values = ParseSymbolNames(expression);
            char operation = ' ';

            if (expression.Contains('-'))
                operation = '-';
            else if (expression.Contains('+'))
                operation = '+';

            bool oneValue = values.Length == 1;

            bool leftDigit = int.TryParse(values[0], out leftValue),
                 rightDigit = !oneValue ? int.TryParse(values[1], out rightValue) : false;
            string errorMessage;

            if (oneValue)
            {
                errorMessage = $"Symbol in {values[0]} not found.";
                if (!leftDigit)
                {
                    symbols = symbolTable.Search(values[0]);
                    if (symbols.Length != 1) throw new InvalidSymbolException(errorMessage);
                }
            }
            else
            {
                errorMessage = $"Symbol in {values[0]}{operation}{values[1]} not found.";

                if (!leftDigit && rightDigit)
                {
                    symbols = symbolTable.Search(values[0]);
                    if (symbols.Length != 1) throw new InvalidSymbolException(errorMessage);
                }
                else if (leftDigit && !rightDigit)
                {
                    symbols = symbolTable.Search(values[1]);
                    if (symbols.Length != 1) throw new InvalidSymbolException(errorMessage);
                }
                else if (!leftDigit && !rightDigit)
                {
                    symbols = symbolTable.Search(values);

                    if (symbols.Length != 2) throw new InvalidSymbolException(errorMessage);
                } 
            }

            bool noSymbols = symbols.Length == 0;

            try
            {
                // use overloaded operators
                if (operation == '+')
                {
                    if (leftDigit && !noSymbols)
                        adjustedSymbol = leftValue + symbols[0];
                    else if (rightDigit && !noSymbols)
                        adjustedSymbol = symbols[0] + rightValue;
                    else if (!noSymbols)
                        adjustedSymbol = symbols[0] + symbols[1];
                    else if (leftDigit && rightDigit)
                        adjustedSymbol = new Symbol()
                        {
                            Name = values[0] + "+" + values[1],
                            RFlag = false,
                            Value = leftValue + rightValue
                        };
                }
                else if (operation == '-')
                {
                    if (leftDigit && !noSymbols)
                        adjustedSymbol = leftValue - symbols[0];
                    else if (rightDigit && !noSymbols)
                        adjustedSymbol = symbols[0] - rightValue;
                    else if (!noSymbols)
                        adjustedSymbol = symbols[0] - symbols[1];
                    else if (leftDigit && rightDigit)
                        adjustedSymbol = new Symbol()
                        {
                            Name = values[0] + "-" + values[1],
                            RFlag = false,
                            Value = leftValue - rightValue
                        };
                }
                else
                {
                    if (leftDigit)
                        adjustedSymbol = new Symbol()
                        {
                            Name = values[0],
                            RFlag = false,
                            Value = leftValue
                        };
                    else if (symbols.Length != 0)
                        adjustedSymbol = symbols[0];
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }

            return adjustedSymbol;
        }

        /// <summary>
        /// Extracts each symbol name or number out of an expression
        /// </summary>
        /// <param name="expression"> IN </param>
        /// <returns> string[] </returns>
        public static string[] ParseSymbolNames(string expression)
        {
            var symbols = new List<string>();
            if (expression != string.Empty)
            {
                if (expression[0] == '@' || expression[0] == '#')
                {
                    expression = expression.Substring(1, expression.Length - 1);
                }
                else if (expression.Contains(",X"))
                {
                    expression = expression.Remove(expression.IndexOf(','));
                }

                if (expression.Contains('+'))
                {
                    symbols = new List<string>(expression.Split('+'));
                }
                else if (expression.Contains('-'))
                {
                    symbols = new List<string>(expression.Split('-'));
                }
                else
                {
                    symbols.Add(expression);
                } 
            }

            return symbols.ToArray();
        }

        public static string GetConstValue(string expression)
        {
            string value = ParseLiteralValue(expression), returnValue = string.Empty;

            if (char.ToUpper(expression[0]) == 'C')
            {
                foreach (char c in value)
                {
                    returnValue += ((int)c).ToString("X");
                }
            }
            else if (char.ToUpper(expression[0]) == 'X')
            {
                try
                {
                    foreach (char c in value)
                    {
                        returnValue += int.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber).ToString("X");
                    }
                }
                catch (FormatException)
                {
                    throw new FormatException("Does not contain valid hex value.");
                }
            }
            else
            {
                throw new FormatException($"Unrecognized literal type ({expression[0]}).");
            }

            return returnValue;
        }

        public static int GetConstLength(string expression)
        {
            if (char.ToUpper(expression[0]) == 'C')
            {
                return ParseLiteralValue(expression).Length;
            }
            else if (char.ToUpper(expression[0]) == 'X')
            {
                var literalLength = ParseLiteralValue(expression).Length;

                if (literalLength % 2 != 0)
                {
                    throw new FormatException("Does not contain an even number of hex digits.");
                }
                return literalLength / 2;
            }
            else
            {
                throw new FormatException($"Unrecognized literal type ({expression[0]}).");
            }
        }

        private static string ParseLiteralValue(string expression)
        {
            List<char> literalChars = new List<char>();
            try
            {
                if (expression[1] != '\'')
                {
                    throw new IndexOutOfRangeException(); //to be caught
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException("Does not begin with a quote");
            }

            try
            {
                for (int i = 2; expression[i] != '\''; i++)
                {
                    literalChars.Add(expression[i]);
                }
            }
            catch (IndexOutOfRangeException) // will throw if no ' at end
            {
                throw new FormatException("Does not end with a quote.");
            }

            return new string(literalChars.ToArray());
        }
    }
}
