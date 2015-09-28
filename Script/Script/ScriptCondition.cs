using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptCondition : ScriptFunction
    {
        public ScriptCondition(string name, Delegate condition)
            : base(name, condition)
        {
            Name = name;
            Function = condition;
            Types = new ScriptTypes[] { };
        }

        public ScriptCondition(string name, Delegate condition, ScriptTypes[] types)
            : base(name, condition, types)
        {
            Name = name;
            Function = condition;
            Types = types;
        }
    }
}
