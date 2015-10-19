﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptVariable
    {

        public object Value { get; set; }

        public ScriptTypes Type { get; set; }

        public string Name { get; set; }

        public ScriptVariable(object value, ScriptTypes type)
        {
            Value = value;
            Type = type;
        }

        public T Return<T>()
        {
            var returnT = ScriptType.ToEnum(typeof(T));
            switch (returnT)
            {
                case ScriptTypes.String:
                case ScriptTypes.Integer:
                case ScriptTypes.Double:
                case ScriptTypes.Boolean:
                case ScriptTypes.Regex:
                    return (T)this.Value;
                case ScriptTypes.ListString:
                    return (T)(object)((List<ScriptVariable>)this.Value).Select(x => x.Value.ToString()).ToList();
                case ScriptTypes.ListInteger:
                    return (T)(object)((List<ScriptVariable>)(this.Value)).Select(x => x).ToList();
                case ScriptTypes.ListDouble:
                    return (T)(object)((List<ScriptVariable>)(this.Value)).Select(x => x).ToList();
                case ScriptTypes.ListBoolean:
                    return (T)(object)((List<ScriptVariable>)(this.Value)).Select(x => x).ToList();
                default:
                    return default(T);
            }
        }

        public ScriptVariable Cast<ReturnT>(Lexer lexer)
        {
            var outputType = ScriptType.ToEnum(typeof(ReturnT));
            switch (outputType)
            {
                case ScriptTypes.String:
                    switch (this.Type)
                    {
                        case ScriptTypes.Integer:
                        case ScriptTypes.Double:
                            this.Value = this.Value.ToString();
                            break;
                        case ScriptTypes.Boolean:
                            this.Value = (bool)this.Value ? "true" : "false";
                            break;
                        case ScriptTypes.Null:
                            this.Value = "null";
                            break;
                    }
                    this.Type = ScriptTypes.String;
                    break;
                case ScriptTypes.Integer:
                    switch (this.Type)
                    {
                        case ScriptTypes.String:
                            int tryInt = 0;
                            if (int.TryParse(this.Value.ToString(), out tryInt))
                            {
                                this.Value = tryInt;
                            }
                            else
                            {
                                goto castError;
                            }
                            break;
                        case ScriptTypes.Double:
                            double tryDouble = 0;
                            if (double.TryParse(this.Value.ToString(), out tryDouble))
                            {
                                this.Value = tryDouble;
                            }
                            else
                            {
                                goto castError;
                            }
                            break;
                        case ScriptTypes.Boolean:
                            this.Value = (bool)this.Value ? 1 : 0;
                            break;
                    }
                    this.Type = ScriptTypes.Integer;
                    break;
                case ScriptTypes.Double:
                    switch (this.Type)
                    {
                        case ScriptTypes.String:
                        case ScriptTypes.Integer:
                            double tryDouble = 0;
                            if (double.TryParse(this.Value.ToString(), out tryDouble))
                            {
                                this.Value = tryDouble;
                            }
                            else
                            {
                                goto castError;
                            }
                            break;
                        case ScriptTypes.Boolean:
                            this.Value = (bool)this.Value ? 1.0 : 0.0;
                            break;
                    }
                    this.Type = ScriptTypes.Double;
                    break;
                case ScriptTypes.Boolean:

                    switch (this.Type)
                    {
                        case ScriptTypes.String:
                            this.Value = this.Value.ToString() == "true";
                            break;
                    }
                    this.Type = ScriptTypes.Boolean;
                    break;
                case ScriptTypes.ListString:
                case ScriptTypes.ListInteger:
                case ScriptTypes.ListDouble:
                case ScriptTypes.ListBoolean:

                    break;
                case ScriptTypes.Void:
                    this.Value = default(ReturnT);
                    break;
            }
            return this;
            castError:
            lexer.Prev();
            lexer.Prev();
            throw new ScriptException(
                message: String.Format("Unable to cast value '{0}' from '{1}' to '{2}' on Line {3} Col {4}",
                    Value.ToString(),
                    Type.ToString(),
                    outputType.ToString(),
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position,
                method: lexer.TokenContents
            );
        }

        /*public ScriptVariable Cast<ReturnT>(Lexer lexer)
        {
            var outputType = ScriptType.ToEnum(typeof(ReturnT));
            var outValue = default(ReturnT);
            if (TryCast<ReturnT>(Type, Value, out outValue))
            {
                return this;
            }
            lexer.Prev();
            lexer.Prev();
            throw new ScriptException(
                message: String.Format("Unable to cast value '{0}' from '{1}' to '{2}' on Line {3} Col {4}",
                    Value.ToString(),
                    Type.ToString(),
                    outputType.ToString(),
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position,
                method: lexer.TokenContents
            );
        }*/
    }
}
