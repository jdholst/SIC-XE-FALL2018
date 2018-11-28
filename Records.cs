using System;
using System.Collections.Generic;
using System.Text;

namespace SICXE
{
    /// <summary>
    /// LinkedList containing methods  object code record 
    /// </summary>
    public abstract class Record
    {
        protected LinkedList<string> record;

        private readonly int maxEntries;
        private int size;
        private readonly string type;

        /// <summary>
        /// Current size of the record.
        /// </summary>
        public int Size {
            get
            {
                return size;
            }
            protected set
            {
                if (size >= maxEntries)
                    throw new RecordOverflowException();
                else
                    size = value;
            }
        }

        protected Record(string type, int maxEntries)
        {
            this.maxEntries = maxEntries;
            this.type = type;
            size = 0;
            record = new LinkedList<string>();
        }

        /// <summary>
        /// String form of the record. Each entry is separated by a ^.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder recordString = new StringBuilder(type);

            foreach(var entry in record)
            {
                recordString.Append("^" + entry);
            }

            return recordString.ToString();
        }

        public abstract void AddEntry(RecordArgs args);
    }

    /// <summary>
    /// First record of the object program containing the program name, starting address, and program length
    /// </summary>
    public class HeaderRecord : Record
    {
        public HeaderRecord(string programName, string startingAddress, string programLength) : base("H", 1)
        {
            record.AddLast(programName.Length > 4 ? programName.Remove(4) : programName);
            record.AddLast(startingAddress.PadLeft(6, '0'));
            record.AddLast(programLength.PadLeft(6, '0'));
            Size++;
        }

        /// <summary>
        /// Throws exception because header only contains one entry added in the contructor.
        /// </summary>
        /// <param name="args"></param>
        public override void AddEntry(RecordArgs args)
        {
            throw new RecordOverflowException();
        }
    }

    /// <summary>
    /// Last record of the program containing starting address of program.
    /// </summary>
    public class EndRecord : Record
    {
        public EndRecord(string startingAddress) : base("E", 1)
        {
            record.AddLast(startingAddress.PadLeft(6, '0'));
            Size++;
        }

        /// <summary>
        /// Throws exception because end record only contains one entry added in the contructor.
        /// </summary>
        /// <param name="args"></param>
        public override void AddEntry(RecordArgs args)
        {
            throw new RecordOverflowException();
        }
    }

    /// <summary>
    /// Begins with starting address of the record and the size of the record. 
    /// Each object code entry is added to the end with a max size of 10 entries.
    /// </summary>
    public class TextRecord : Record
    {
        private string location;
        private string size;

        public TextRecord(string location) : base("T", 10)
        {
            this.location = location;
            size = location;
            record.AddLast(location.PadLeft(6, '0'));
        }

        /// <summary>
        /// Adds an entry containing object code.
        /// </summary>
        /// <param name="args"> Contains object-code and location counter that helps keep track of the record size </param>
        public override void AddEntry(RecordArgs args)
        {
            Size++;
            record.AddLast(args.Line);
            record.Remove(size.PadLeft(2, '0'));
            size = ( int.Parse((args as TextRecordArgs).LocationCounter, System.Globalization.NumberStyles.HexNumber) -
                        int.Parse(location, System.Globalization.NumberStyles.HexNumber)
                    ).ToString("X");
            record.AddAfter(record.Find(location.PadLeft(6, '0')), size.PadLeft(2, '0'));
        }
    }

    /// <summary>
    /// Contains names of all symbols referenced through EXTREF directive.
    /// </summary>
    public class ReferenceRecord : Record
    {
        public ReferenceRecord() : base("R", int.MaxValue) { }

        /// <summary>
        /// Adds referenced symbol
        /// </summary>
        /// <param name="args"> Contains name of symbol </param>
        public override void AddEntry(RecordArgs args)
        {
            record.AddLast(args.Line);
            Size++;
        }
    }

    /// <summary>
    /// Contains names of all symbols defined through EXTDEF directive
    /// </summary>
    public class DefineRecord : Record
    {
        public DefineRecord() : base("D", int.MaxValue) { }

        /// <summary>
        /// Adds an entry containing the symbol name and its address
        /// </summary>
        /// <param name="args"></param>
        public override void AddEntry(RecordArgs args)
        {
            record.AddLast(args.Line);
            record.AddLast((args as DefineRecordArgs).Address.PadLeft(6, '0'));
            Size++;
        }
    }

    /// <summary>
    /// Used for symbols which are modified by a format 4 instruction.
    /// Contains address, size, and name of the modification
    /// </summary>
    public class ModificationRecord : Record
    {
        public ModificationRecord(string address, string size, string name) : base("M", 1)
        {
            record.AddLast(address.PadLeft(6, '0'));
            record.AddLast(size.PadLeft(2, '0'));
            record.AddLast(name);
        }

        public override void AddEntry(RecordArgs args)
        {
            throw new RecordOverflowException();
        }
    }

    public class RecordArgs
    {
        private readonly string line;

        public string Line { get { return line; } }

        public RecordArgs(string line)
        {
            this.line = line;
        }
    }

    public class TextRecordArgs : RecordArgs
    {
        private readonly string locationCounter;

        public string LocationCounter { get { return locationCounter; } }

        public TextRecordArgs(string objectCode, string locationCounter) : base(objectCode)
        {
            this.locationCounter = locationCounter;
        }
    }

    public class DefineRecordArgs : RecordArgs
    {
        private readonly string address;

        public string Address { get { return address; } }

        public DefineRecordArgs(string address, string name) : base(name)
        {
            this.address = address;
        }
    }

    public class RecordOverflowException : Exception
    {

    }
}
