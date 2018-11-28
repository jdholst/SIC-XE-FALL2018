using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SICXE
{
    /// <summary>
    /// Contains all attributes of a literal.
    /// </summary>
    public struct Literal
    {
        public string Name;
        public string Value;
        public int Length;
        public string Address;
    }

    /// <summary>
    /// LinkedList that contains literals containing data such as symbol name, value, length, and address.
    /// </summary>
    public class LiteralTable : IEnumerable<Literal>
    {
        private LinkedList<Literal> table;

        public int Count { get { return table.Count; } }

        /// <summary>
        /// Constructor that initializes table and addressCounter fields.
        /// </summary>
        public LiteralTable()
        {
            table = new LinkedList<Literal>();
        }

        /// <summary>
        /// Displays all literals successfully inserted into the table. Displays 20 lines at a time.
        /// </summary>
        public void View()
        {
            Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10}", "Name", "Value", "Length", "Address");

            int i = 1;

            foreach(var element in table)
            {
                Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10}", element.Name, element.Value,
                    element.Length, element.Address);

                if (i % 20 == 0)
                {
                    Console.Write("Press Enter to Continue... ");
                    Console.ReadLine();
                    Console.Clear();
                }
                i++;
            }
            Console.WriteLine();
        }

        public Literal[] Search(params string[] literals)
        {
            List<Literal> foundLiterals = new List<Literal>();

            foreach (var literalName in literals)
            {
                foreach(var literal in table)
                {
                    if(literalName == literal.Name)
                    {
                        foundLiterals.Add(literal);
                    }
                }
            }

            return foundLiterals.ToArray();
        }

        public void SetLiteralAddress(Literal literal, string address)
        {
            var foundLiteral = table.Find(literal);

            if(foundLiteral != null)
            {
                var editLiteral = foundLiteral.Value;
                editLiteral.Address = address;
                foundLiteral.Value = editLiteral;
            }
            else
            {
                throw new NullReferenceException($"The literal {literal.Name} does not exist");
            }
        }

        /// <summary>
        /// Inserts the literal onto the end of the LinkedList. Can insert two types of literals:
        /// Character and Hexidecimal (denoted as =X and =C respectively).
        /// </summary>
        /// <param name="expression"></param>
        public void Insert(string expression)
        {
            if(expression[0] == '=')
            {
                string name = expression;
                bool exist = false;

                expression = expression.Substring(1, expression.Length - 1);

                var insertLiteral = new Literal()
                {
                    Name = name,
                    Value = ExpressionHandler.GetConstValue(expression),
                    Length = ExpressionHandler.GetConstLength(expression),
                    Address = "0" // default address
                };

                foreach (var literal in table)
                {
                    if (literal.Value == insertLiteral.Value) exist = true;
                }

                if (!exist)
                {
                    table.AddLast(insertLiteral);
                }
            }
            else
            {
                throw new FormatException("Does not contain an = at the start");
            }
        }

        public IEnumerator<Literal> GetEnumerator()
        {
            return table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
