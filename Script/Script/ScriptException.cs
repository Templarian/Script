using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptException : Exception
    {
        public ScriptException(string message, int column = 0, int row = 0, string method = null) : base(message)
        {
            Row = row;
            Column = column;
            Method = method;
        }

        public string Method { get; set; }

        public int Column { get; set; }

        public int Row { get; set; }
    }
}
