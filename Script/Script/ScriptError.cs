using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Script
{
    public class ScriptError
    {
        public string Message { get; set; }

        public string MethodName { get; set; }

        public int LineNumber { get; set; }

        public int Position { get; set; }
    }
}
