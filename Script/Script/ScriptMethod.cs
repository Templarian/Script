using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptMethod
    {
        public string Name { get; set; }
        public Delegate Method { get; set; }
        public ScriptTypes[] Types { get; set; }
        public ScriptTypes ReturnType { get; set; }

        public ScriptMethod(string name, Delegate method)
        {
            Name = name;
            Method = method;
            Types = new ScriptTypes[] { };
            ReturnType = ScriptTypes.Void;
        }

        public ScriptMethod(string name, Delegate method, ScriptTypes returnType)
        {
            Name = name;
            Method = method;
            Types = new ScriptTypes[] { };
            ReturnType = returnType;
        }

        public ScriptMethod(string name, Delegate method, ScriptTypes[] types)
        {
            Name = name;
            Method = method;
            Types = types;
            ReturnType = ScriptTypes.Void;
        }

        public ScriptMethod(string name, Delegate method, ScriptTypes[] types, ScriptTypes returnType)
        {
            Name = name;
            Method = method;
            Types = types;
            ReturnType = returnType;
        }
    }
}
