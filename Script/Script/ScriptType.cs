using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Script
{
    public static class ScriptType
    {
        public static ScriptTypes ToEnum(Type o)
        {
            return o == typeof(string) ? ScriptTypes.String
                : o == typeof(int) ? ScriptTypes.Integer
                : o == typeof(double) ? ScriptTypes.Double
                : o == typeof(bool) ? ScriptTypes.Boolean
                : o == typeof(Regex) ? ScriptTypes.Regex
                : o == typeof(List<string>) ? ScriptTypes.ListString
                : o == typeof(List<int>) ? ScriptTypes.ListInteger
                : o == typeof(List<double>) ? ScriptTypes.ListDouble
                : o == typeof(List<bool>) ? ScriptTypes.ListBoolean
                : ScriptTypes.Any;
        }

        public static bool IsList(ScriptTypes type)
        {
            switch(type)
            {
                case ScriptTypes.ListString:
                case ScriptTypes.ListInteger:
                case ScriptTypes.ListDouble:
                case ScriptTypes.ListBoolean:
                    return true;
            }
            return false;
        }
        
    }
}
