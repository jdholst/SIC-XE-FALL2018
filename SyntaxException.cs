using System;
using System.Collections.Generic;
using System.Text;

namespace SICXE
{
    public class SyntaxException : Exception
    {
        public SyntaxException() : base()
        {

        }

        public SyntaxException(string message) : base(message) { }
    }

    public sealed class InvalidSymbolException : SyntaxException
    {
        public bool IsLabel;
        public InvalidSymbolException(string message) : base(message) { }
        public InvalidSymbolException(string message, bool isLabel) : base(message)
        {
            IsLabel = isLabel;
        }
    }

    public sealed class IllegalInstructionException : SyntaxException
    {
        public IllegalInstructionException(string message) : base(message) { }
    }
}
