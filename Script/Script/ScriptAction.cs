using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptAction : ScriptMethod
    {

        public ScriptAction(string name, Delegate function) : base(name, function)
        {
            Name = name;
            Method = function;
            Types = new ScriptTypes[] { };
            ReturnType = ScriptTypes.Void;
        }

        public ScriptAction(string name, Delegate function, ScriptTypes[] types) : base(name, function, types)
        {
            Name = name;
            Method = function;
            Types = types;
            ReturnType = ScriptTypes.Void;
        }

    }
}
