using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptTypeFunction
    {
        public ScriptTypes ExtendType { get; set; }
        public string Name { get; set; }
        public Delegate Function { get; set; }
        public ScriptTypes[] Types { get; set; }

        public ScriptTypeFunction (ScriptTypes extendType, string name, Delegate function)
        {
            ExtendType = extendType;
            Name = name;
            Function = function;
            Types = new ScriptTypes[] { };
        }

        public ScriptTypeFunction(ScriptTypes extendType, string name, Delegate function, ScriptTypes[] types)
        {
            ExtendType = extendType;
            Name = name;
            Function = function;
            Types = types;
        }
    }
}
