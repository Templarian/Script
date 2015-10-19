using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptFunction : ScriptMethod
    {

        public ScriptFunction(string name, Delegate function) : base(name, function)
        {
            Name = name;
            Method = function;
            Types = new ScriptTypes[] { };
            ReturnType = ScriptTypes.Void;
        }

        public ScriptFunction(string name, Delegate function, ScriptTypes returnType) : base(name, function, returnType)
        {
            Name = name;
            Method = function;
            Types = new ScriptTypes[] { };
            ReturnType = returnType;
        }

        public ScriptFunction(string name, Delegate function, ScriptTypes[] types) : base(name, function, types)
        {
            Name = name;
            Method = function;
            Types = types;
            ReturnType = ScriptTypes.Void;
        }

        public ScriptFunction(string name, Delegate function, ScriptTypes[] types, ScriptTypes returnType) : base(name, function, types, returnType)
        {
            Name = name;
            Method = function;
            Types = types;
            ReturnType = returnType;
        }
    }
}
