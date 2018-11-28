using System;
using System.Collections.Generic;
using System.Text;

namespace SICXE
{
    /// <summary>
    /// Contains all attributes of an opcode.
    /// </summary>
    public struct Opcode
    {
        public string Mnemonic;
        public string Op;
        public int Format;
    }

    /// <summary>
    /// Opcode array containing an instruction set for assembly code
    /// </summary>
    public class OpcodeTable
    {
        private Opcode[] opcodes;
        private int mnemonicIndex;
        private int opcodeIndex;
        private int formatIndex;

        /// <summary>
        /// Default constructor initializing array and index defaults
        /// </summary>
        public OpcodeTable()
        {
            opcodes = Array.Empty<Opcode>();

            mnemonicIndex = 0;
            opcodeIndex = 1;
            formatIndex = 2;
        }
        
        /// <summary>
        /// Contructor to initialize indices and instruction set
        /// </summary>
        /// <param name="codes"> IN </param>
        public OpcodeTable(params string[] codes) : this()
        {
            if (codes[0].ToUpper().Contains("MNEMONIC") &&
                codes[0].ToUpper().Contains("OPCODE") &&
                codes[0].ToUpper().Contains("FORMAT"))
            {
                SetIndices(codes[0]);
                codes = new List<string>(codes).GetRange(1, codes.Length - 1).ToArray(); // remove first line
            }

            Insert(codes);
        }

        /// <summary>
        /// Searches the instruction set by mnemonic
        /// </summary>
        /// <param name="key"> IN - search key </param>
        /// <param name="opcode"> OUT - empty opcode if not found </param>
        /// <returns> success of the search </returns>
        public bool Search(string key, out Opcode opcode)
        {
            bool format4 = key[0] == '+';
            int index = Array.BinarySearch(opcodes, new Opcode { Mnemonic = format4 ? key.Substring(1, key.Length - 1) : key }, OpcodeComparer.Create());

            if(index >= 0)
            {
                opcode = opcodes[index];

                if (format4)
                {
                    if(opcode.Format == 3)
                    {
                        opcode.Format++;
                    }
                }
                return true;
            }
            else // not found
            {
                opcode = new Opcode(); // set to a blank opcode
                return false;
            }
        }

        /// <summary>
        /// Inserts instructions into the table. Sorts the table at the end.
        /// </summary>
        /// <param name="codes"> IN - instructions </param>
        public void Insert(params string[] codes)
        {
            List<Opcode> opcodes = new List<Opcode>(this.opcodes);
            foreach(var code in codes)
            {
                var splitLine = code.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                opcodes.Add(new Opcode()
                {
                    Mnemonic = splitLine[mnemonicIndex],
                    Op = splitLine[opcodeIndex],
                    Format = int.Parse(splitLine[formatIndex])
                });
            }

            opcodes.Sort(OpcodeComparer.Create());

            this.opcodes = opcodes.ToArray();
        }

        /// <summary>
        /// Sets the indices based on the location of each keyword.
        /// </summary>
        /// <param name="firstLine"> IN - line to be analyzed </param>
        private void SetIndices(string firstLine)
        {
            
            var firstSplit = firstLine.ToUpper().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < 3; i++)
            {
                if (firstSplit[i].ToUpper().Contains("MNEMONIC"))
                {
                    mnemonicIndex = i;
                }
                else if (firstSplit[i].ToUpper().Contains("OPCODE"))
                {
                    opcodeIndex = i;
                }
                else if (firstSplit[i].ToUpper().Contains("FORMAT"))
                {
                    formatIndex = i;
                }
            }
        }
    }
    
    /// <summary>
    /// Comparer used for binary search.
    /// </summary>
    public class OpcodeComparer : Comparer<Opcode>
    {
        /// <summary>
        /// Creates a new comparer
        /// </summary>
        /// <returns> new OpcodeComparer </returns>
        public static OpcodeComparer Create()
        {
            return new OpcodeComparer();
        }

        /// <summary>
        /// Compares two opcodes based on the mnemonics.
        /// </summary>
        /// <param name="x"> IN </param>
        /// <param name="y"> IN </param>
        /// <returns></returns>
        public override int Compare(Opcode x, Opcode y)
        {
            return string.Compare(x.Mnemonic.ToUpper(), y.Mnemonic.ToUpper());
        }
    }
}
