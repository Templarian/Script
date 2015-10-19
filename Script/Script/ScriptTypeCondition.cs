using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptTypeCondition : ScriptTypeMethod
    {

        public ScriptTypeCondition(ScriptTypes extendType, string name, Delegate condition) : base (extendType, name, condition)
        {
            ExtendType = extendType;
            Name = name;
            Method = condition;
            Types = new ScriptTypes[] { };
            ReturnType = ScriptTypes.Boolean;
        }

        public ScriptTypeCondition(ScriptTypes extendType, string name, Delegate condition, ScriptTypes[] types) : base(extendType, name, condition, types)
        {
            ExtendType = extendType;
            Name = name;
            Method = condition;
            Types = types;
            ReturnType = ScriptTypes.Boolean;
        }
    }
}
