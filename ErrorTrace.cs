using System;
using System.Collections.Generic;
using System.Text;

namespace SICXE
{
    /// <summary>
    /// Traces errors throughout a program. Used to buffer multiple errors
    /// to return later for display.
    /// </summary>
    public static class ErrorTrace
    {
        private static StringBuilder errorBuffer = new StringBuilder();

        /// <summary>
        /// Writes to the error buffer.
        /// </summary>
        /// <param name="error"> IN </param>
        public static void Write(string error)
        {
            errorBuffer.AppendJoin(',', errorBuffer, error);
        }

        /// <summary>
        /// Returns and clears the buffer.
        /// </summary>
        /// <returns> string[] </returns>
        public static string[] Dump()
        {
            try
            {
                return errorBuffer.ToString().Split(',');
            }
            finally
            {
                errorBuffer.Clear();
            }
        }
    }
}
