using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptTypeProperty
    {
        public ScriptTypes ExtendType { get; set; }
        public string Name { get; set; }
        public Delegate Function { get; set; }

        public ScriptTypeProperty (ScriptTypes extendType, string name, Delegate function)
        {
            ExtendType = extendType;
            Name = name;
            Function = function;
        }
    }
}
