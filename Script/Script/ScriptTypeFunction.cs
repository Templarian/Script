using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptTypeFunction : ScriptTypeMethod
    {

        public ScriptTypeFunction(ScriptTypes extendType, string name, Delegate function, ScriptTypes returnType) : base(extendType, name, function, returnType)
        {
            ExtendType = extendType;
            Name = name;
            Method = function;
            Types = new ScriptTypes[] { };
            ReturnType = returnType;
        }

        public ScriptTypeFunction(ScriptTypes extendType, string name, Delegate function, ScriptTypes[] types, ScriptTypes returnType) : base(extendType, name, function, types, returnType)
        {
            ExtendType = extendType;
            Name = name;
            Method = function;
            Types = types;
            ReturnType = returnType;
        }

    }
}
