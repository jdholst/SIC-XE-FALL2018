/*******************************************************************************
Name:        Jacob Holst
Assignment:  SymbolTable
Due Date:    9/11/18
Course:      CSc 354 Systems Programming
Instructor:  Gamradt

Description: This class creates a Symbol Tree that implements the Binary Search 
             Tree abstract ADT that functions to construct, insert, search, 
             view. This class utilizes recursion to traverse down the BST. Each
             symbol is checked and displays errors (which don't stop the program)
             if check fails. Contains a view function which calls an
             implementation of the abstract BST function inview and formats it
             into a table. Can either directly insert/search or use a search and
             symbol file.
*******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SICXE
{
    /// <summary>
    /// Contains all attributes of a symbol.
    /// </summary>
    public struct Symbol
    {
        public string Name;
        public int Value;
        public bool RFlag, IFlag, MFlag;

        /// <summary>
        /// Returns a formatted display of the symbol data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10}", Name, Value.ToString("X"),
                    RFlag ? 1 : 0, IFlag ? 1 : 0, MFlag ? 1 : 0);
        }
        #region +- operators

        private enum OperatorType { Add, Subtract }

        /// <summary>
        /// Add two symbols. Adjusts the RFlag according to the evaluation rule table.
        /// </summary>
        /// <param name="left"> IN </param>
        /// <param name="right"> IN </param>
        /// <returns>Symbol</returns>
        public static Symbol operator+(Symbol left, Symbol right)
        {
            return new Symbol()
            {
                Name = left.Name + "+" + right.Name,
                IFlag = true,
                MFlag = false,
                RFlag = EvaluateR(left.RFlag, right.RFlag, OperatorType.Add),
                Value = left.Value + right.Value
            };
        }

        /// <summary>
        /// Add one symbol value (left) with one integer value (right). 
        /// Adjusts the RFlag according to the evaluation rule table.
        /// </summary>
        /// <param name="left"> IN </param>
        /// <param name="right"> IN </param>
        /// <returns>Symbol</returns>
        public static Symbol operator+(Symbol left, int right)
        {
            return new Symbol()
            {
                Name = left.Name + "+" + right,
                IFlag = true,
                MFlag = false,
                RFlag = EvaluateR(left.RFlag, false, OperatorType.Add),
                Value = left.Value + right
            };
        }

        /// <summary>
        /// Add one symbol value (right) with one integer value (left). 
        /// Adjusts the RFlag according to the evaluation rule table.
        /// </summary>
        /// <param name="left"> IN </param>
        /// <param name="right"> IN </param>
        /// <returns>Symbol</returns>
        public static Symbol operator+(int left, Symbol right)
        {
            return new Symbol()
            {
                Name = left + "+" + right.Name,
                IFlag = true,
                MFlag = false,
                RFlag = EvaluateR(false, right.RFlag, OperatorType.Add),
                Value = left + right.Value
            };
        }

        /// <summary>
        /// Subtracts two symbols. Adjusts the RFlag according to the evaluation rule table.
        /// </summary>
        /// <param name="left"> IN </param>
        /// <param name="right"> IN </param>
        /// <returns>Symbol</returns>
        public static Symbol operator-(Symbol left, Symbol right)
        {
            return new Symbol()
            {
                Name = left.Name + "-" + right.Name,
                IFlag = true,
                MFlag = true,
                RFlag = EvaluateR(left.RFlag, right.RFlag, OperatorType.Subtract),
                Value = left.Value - right.Value
            };
        }

        /// <summary>
        /// Add one symbol value (left) with one integer value (right). 
        /// Adjusts the RFlag according to the evaluation rule table.
        /// </summary>
        /// <param name="left"> IN </param>
        /// <param name="right"> IN </param>
        /// <returns>Symbol</returns>
        public static Symbol operator-(Symbol left, int right)
        {
            return new Symbol()
            {
                Name = left.Name + "-" + right,
                IFlag = true,
                MFlag = true,
                RFlag = EvaluateR(left.RFlag, false, OperatorType.Subtract),
                Value = left.Value - right
            };
        }

        /// <summary>
        /// Add one symbol value (right) with one integer value (left). 
        /// Adjusts the RFlag according to the evaluation rule table.
        /// </summary>
        /// <param name="left"> IN </param>
        /// <param name="right"> IN </param>
        /// <returns>Symbol</returns>
        public static Symbol operator-(int left, Symbol right)
        {
            return new Symbol()
            {
                Name = left + "-" + right.Name,
                IFlag = true,
                MFlag = true,
                RFlag = EvaluateR(false, right.RFlag, OperatorType.Subtract),
                Value = left - right.Value
            };
        }

        private static bool EvaluateR(bool left, bool right, OperatorType op)
        {
            int result = -1;
            if (op == OperatorType.Add)
                result = (left ? 1 : 0) + (right ? 1 : 0);
            else if (op == OperatorType.Subtract)
                result = (left ? 1 : 0) - (right ? 1 : 0);

            if (result < 0)
                throw new InvalidOperationException("ABSOLUTE - RELATIVE");
            else if (result > 1)
                throw new InvalidOperationException("RELATIVE + RELATIVE");

            return result > 0 ? true : false; // can be 0 or 1
        }

        #endregion
    }

    /// <summary>
    /// Symbol Tree that contains symbols containing data such as symbol name, value, and flags.
    /// </summary>
    public class SymbolTable : IEnumerable<Symbol>
    {
        private BinarySearchTree<Symbol> symbolTree = new BinarySearchTree<Symbol>(SymbolComparer.Create());

        public int Count { get { return symbolTree.Count; } }

        /// <summary>
        /// Displays all symbols successfully inserted in a table.
        /// </summary>
        public void View()
        {
            Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10}", "Symbol", "Value", "RFlag", "IFlag", "MFlag");
            symbolTree.InView();
            Console.WriteLine();
        }

        /// <summary>
        /// Checks each symbol and adds it in a list of found symbols.
        /// </summary>
        /// <param name="symbols"> IN </param>
        /// <returns> Symbol[] (foundSymbols) </returns>
        public Symbol[] Search(params string[] symbols)
        {
            List<Symbol> foundSymbols = new List<Symbol>();

            foreach (var symbol in symbols)
            {
                var trim = symbol.Trim(' ', '\t').ToUpper();

                if (CheckSymbol(trim))
                {
                    if(symbolTree.Search(new Symbol() { Name = trim }, out Symbol foundSymbol))
                        foundSymbols.Add(foundSymbol);
                }
                // dump error trace but do not display errors
                ErrorTrace.Dump();
            }

            return foundSymbols.ToArray();
        }

        /// <summary>
        /// Overloaded to search symbosls directly.
        /// </summary>
        /// <param name="symbols"> IN </param>
        /// <returns> Symbol[] </returns>
        public Symbol[] Search(params Symbol[] symbols)
        {
            var symbolNames = new List<string>();

            foreach(var symbol in symbols)
                symbolNames.Add(symbol.Name);

            return Search(symbolNames.ToArray());
        }

        /// <summary>
        /// Breaks down each line passed in, checks each attribute, and inserts if no errors.
        /// Takes only the first 4 characters of the symbol name.
        /// </summary>
        /// <param name="lines"> IN </param>
        public void Insert(params string[] lines)
        {
            foreach (var line in lines)
            {
                string symbol, value, rFlag, iFlag, mFlag;
                List<string> lineSplit = new List<string>(line.Replace('\t', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // covers any unfilled commands
                if (lineSplit.Count < 5)
                {
                    lineSplit.Add(" ");
                    lineSplit.Add(" ");
                    lineSplit.Add(" ");
                    lineSplit.Add(" ");
                    lineSplit.Add(" ");
                }

                symbol = lineSplit[0].ToUpper();
                value = lineSplit[1];
                rFlag = lineSplit[2];
                iFlag = lineSplit[3];
                mFlag = lineSplit[4];

                if (CheckSymbol(symbol) && CheckValue(value, out int intValue) && CheckFlag(rFlag, out bool boolRFlag) && CheckDuplicate(symbol))
                {
                    if (!CheckFlag(iFlag, out bool boolIFlag))
                        boolIFlag = !boolIFlag; //overrides default from false to true if check fails

                    CheckFlag(mFlag, out bool boolMFlag); // uses default of false if check fails

                    ErrorTrace.Dump(); // ignore errors for mflag and iflag

                    Symbol newSymbol = new Symbol()
                    {
                        Value = intValue,
                        RFlag = boolRFlag,
                        MFlag = boolMFlag,
                        IFlag = boolIFlag
                    };

                    // Cut off all characters after index 4
                    newSymbol.Name = symbol.Length > 4 ? symbol.Remove(4) : symbol;

                    symbolTree.Insert(newSymbol);
                }
                else
                {
                    throw new InvalidSymbolException($"SYMBOL ERROR {symbol}: {string.Join(' ', ErrorTrace.Dump())}");
                }
            }
        }

        /// <summary>
        /// Overloaded to insert symbols directly.
        /// </summary>
        /// <param name="symbols"> IN </param>
        public void Insert(params Symbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                if (CheckSymbol(symbol.Name) && CheckDuplicate(symbol.Name))
                {
                    var newSymbol = symbol;

                    // Cut off all characters after index 4
                    newSymbol.Name = symbol.Name.Length > 4 ? symbol.Name.Remove(4) : symbol.Name;

                    symbolTree.Insert(newSymbol);
                }
                else
                {
                    throw new InvalidSymbolException($"SYMBOL ERROR {symbol}: {string.Join(' ', ErrorTrace.Dump())}");
                }
            }
        }

        /// <summary>
        /// Checks if first character is a letter. Checks if following characters
        /// are letters, digits, or underscores. Checks if symbol is longer than 10
        /// characters
        /// </summary>
        /// <param name="sym"> IN </param>
        /// <returns> bool </returns>
        private bool CheckSymbol(string sym)
        {
            if (sym.Length > 10)
            {
                ErrorTrace.Write("Symbol is longer than 10 characters.");
                return false;
            }

            if (!char.IsLetter(sym[0]))
            {
                ErrorTrace.Write("Symbol does not start with a letter.");
                return false;
            }

            foreach (char c in sym.Substring(1))
            {
                if (!char.IsLetterOrDigit(c) && !char.Equals('_', c))
                {
                    ErrorTrace.Write("Symbol does not contain only letters, digits, or underscores ('_').");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if value is of type Int32. Converts string value into int value.
        /// </summary>
        /// <param name="stringVal"> IN </param>
        /// <param name="intVal"> OUT </param>
        /// <returns> bool </returns>
        private bool CheckValue(string stringVal, out int intVal)
        {
            if (int.TryParse(stringVal, out intVal))
            {
                return true;
            }
            else
            {
                ErrorTrace.Write($"Value ({stringVal}) is not of type Int32.");
                return false;
            }
        }

        /// <summary>
        /// Checks if the flag is of a valid true or false value (1, t, true, 0, f, false).
        /// Converts the string flag into a bool flag.
        /// </summary>
        /// <param name="flag"> IN </param>
        /// <param name="flagActual"> OUT </param>
        /// <returns> bool </returns>
        private bool CheckFlag(string flag, out bool flagActual)
        {
            List<string> trueStrings = new List<string>() { "1", "t", "true" },
                falseStrings = new List<string>() { "0", "f", "false" };

            if (trueStrings.Contains(flag.ToLower()))
            {
                flagActual = true;
                return true;
            }

            if (falseStrings.Contains(flag.ToLower()))
            {
                flagActual = false;
                return true;
            }

            ErrorTrace.Write($"RFlag ({flag}) must be: {string.Join(',', trueStrings) + ',' + string.Join(',', falseStrings)}.");
            flagActual = false;
            return false;
        }

        /// <summary>
        /// Checks if the symbol already exists using search function.
        /// Modifies MFlag if duplicate is found.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private bool CheckDuplicate(string symbol)
        {
            var exists = symbolTree.Search(new Symbol() { Name = symbol.ToUpper() }, out Symbol searchSymbol);

            if (exists)
            {
                //set MFlag
                var newSymbol = searchSymbol;
                newSymbol.MFlag = true;
                symbolTree.Replace(searchSymbol, newSymbol);

                ErrorTrace.Write("Symbol previously defined.");
                return false;
            }
            return true;
        }

        public IEnumerator<Symbol> GetEnumerator()
        {
            return symbolTree.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SymbolComparer : Comparer<Symbol>
    {
        /// <summary>
        /// Creates a new comparer
        /// </summary>
        /// <returns></returns>
        public static SymbolComparer Create()
        {
            return new SymbolComparer();
        }

        /// <summary>
        /// Compares two symbols based on the first four characters of the name.
        /// </summary>
        /// <param name="x"> IN </param>
        /// <param name="y"> IN </param>
        /// <returns></returns>
        public override int Compare(Symbol x, Symbol y)
        {
            x.Name = x.Name.Length > 4 ? x.Name.Remove(4).ToUpper() : x.Name.ToUpper();
            y.Name = y.Name.Length > 4 ? y.Name.Remove(4).ToUpper() : y.Name.ToUpper();

            return string.Compare(x.Name, y.Name);
        }
    }
}
