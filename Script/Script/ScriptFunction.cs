using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptFunction
    {
        public string Name { get; set; }
        public Delegate Function { get; set; }
        public ScriptTypes[] Types { get; set; }

        public ScriptFunction (string name, Delegate function)
        {
            Name = name;
            Function = function;
            Types = new ScriptTypes[] { };
        }

        public ScriptFunction(string name, Delegate function, ScriptTypes[] types)
        {
            Name = name;
            Function = function;
            Types = types;
        }
    }
}
