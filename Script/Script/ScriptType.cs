using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public static class ScriptType
    {
        public static ScriptTypes ToEnum(Type o)
        {
            var name = o.ToString();
            switch (name)
            {
                case "System.Int32":
                    return ScriptTypes.Integer;
                case "System.Double":
                    return ScriptTypes.Double;
                case "System.String":
                    return ScriptTypes.String;
                case "System.Boolean":
                    return ScriptTypes.Boolean;
                case "System.Collections.Generic.List`1[System.Int32]":
                    return ScriptTypes.ListInteger;
                case "System.Collections.Generic.List`1[System.Double]":
                    return ScriptTypes.ListDouble;
                case "System.Collections.Generic.List`1[System.String]":
                    return ScriptTypes.ListString;
                case "System.Collections.Generic.List`1[System.Boolean]":
                    return ScriptTypes.ListBoolean;
                case "System.Text.RegularExpressions.Regex":
                    return ScriptTypes.Regex;
                case "System.Object":
                    return ScriptTypes.Void;
            }
            return ScriptTypes.Null;
        }
    }
}
