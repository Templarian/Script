using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptTypeMethod
    {
        public ScriptTypes ExtendType { get; set; }
        public string Name { get; set; }
        public Delegate Method { get; set; }
        public ScriptTypes[] Types { get; set; }
        public ScriptTypes ReturnType { get; set; }

        public ScriptTypeMethod(ScriptTypes extendType, string name, Delegate method)
        {
            ExtendType = extendType;
            Name = name;
            Method = method;
            Types = new ScriptTypes[] { };
            ReturnType = ScriptTypes.Void;
        }

        public ScriptTypeMethod(ScriptTypes extendType, string name, Delegate method, ScriptTypes returnType)
        {
            ExtendType = extendType;
            Name = name;
            Method = method;
            Types = new ScriptTypes[] { };
            ReturnType = returnType;
        }

        public ScriptTypeMethod(ScriptTypes extendType, string name, Delegate method, ScriptTypes[] types)
        {
            ExtendType = extendType;
            Name = name;
            Method = method;
            Types = types;
            ReturnType = ScriptTypes.Void;
        }

        public ScriptTypeMethod(ScriptTypes extendType, string name, Delegate method, ScriptTypes[] types, ScriptTypes returnType)
        {
            ExtendType = extendType;
            Name = name;
            Method = method;
            Types = types;
            ReturnType = returnType;
        }
    }
}
