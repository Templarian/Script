using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Script
{
    public class ScriptEngine : ScriptClass
    {
        private string code = "";
        private int indentOffset = 0;

        public ScriptEngine()
        {

        }

        public void Execute(string script)
        {
            code = script;
            try
            {
                Parse(code);
            }
            catch (ScriptException e)
            {
                if (Error == null)
                {
                    throw e;
                }
                else
                {
                    Error.DynamicInvoke(e);
                }
            }
        }

        public T Evaluate<T>(string script)
        {
            code = script;
            return Parse<T>(code);
        }

        public void Process(string script)
        {
            code = script;
        }

        public void Trigger(string eventName)
        {
            Trigger(eventName, false);
        }

        public void Trigger(string eventName, bool suppressException)
        {
            var defs = new TokenDefinition[]
            {
                new TokenDefinition(@"event\s+" + eventName, "MATCH"),
                new TokenDefinition(@"event\s+([*<>\?\-+/A-Za-z->!]+)", "EVENT"),
                new TokenDefinition(@"function\s+([*<>\?\-+/A-Za-z->!]+)", "FUNCTION"),
                new TokenDefinition(@".*", "CODE")
            };
            TextReader reader = new StringReader(code);
            Lexer lexer = new Lexer(reader, defs, new Action<int, int, string>(LexerException));
            TriggerEvent(lexer, this);
        }

        private int Indent { get; set; }

        // We need to track the boolean values
        // be realistic... 20+ depths = terrible script
        private bool[] Depths = new bool[20];

        private bool DepthCondition()
        {
            return Depths[Indent];
        }

        private Nullable<bool> DepthCondition(Nullable<bool> result)
        {
            if (result != null)
            {
                Depths[Indent] = (bool)result;
            }
            return result; // Makes code look cleaner if it passes through
        }

        private bool DepthValidCondition()
        {
            if (Depths[Indent]) // if previous block true
            {
                return (Depths[Indent] = false); // set block to false return false
            }
            else
            {
                return true;
            }
        }

        private void DepthInverseCondition()
        {
            Depths[Indent] = !Depths[Indent];
        }

        public void Exception(Action<ScriptException> error)
        {
            Error = error;
        }

        private void Parse(string code)
        {
            Parse<object>(code, 0);
        }

        private T Parse<T>(string code)
        {
            return Parse<T>(code, 0);
        }

        private T Parse<T>(string code, int indentOffset)
        {
            this.indentOffset = indentOffset;
            this.Indent = indentOffset;

            var defs = new TokenDefinition[]
            {
                new TokenDefinition(@"#.*", "COMMENT"),
                new TokenDefinition(@"/[^/]+/[ims]*", "REGEX"),
                new TokenDefinition(@"([""'])(?:\\\1|.)*?\1", "STRING"),
                new TokenDefinition(@"[-+]?\d*\.\d+", "DOUBLE"),
                new TokenDefinition(@"[-+]?\d+", "INTEGER"),
                new TokenDefinition(@"(true|false)", "BOOLEAN"),
                new TokenDefinition(@"null", "NULL"),
                new TokenDefinition(@"if\s?\(", "IF"),
                new TokenDefinition(@"else if\s?\(", "ELSEIF"),
                new TokenDefinition(@"else", "ELSE"),
                new TokenDefinition(@"(\+|-|\*|\/)", "ARITHMETIC"),
                new TokenDefinition(@"(int|double|string|bool)\[\]", "LISTTYPE"),
                new TokenDefinition(@"(int|double|string|bool)", "BASETYPE"),
                new TokenDefinition(@"(name|extends|event|function|condition|string|integer|double|boolean|array|return)", "KEYWORD"),
                new TokenDefinition(@"[*<>\?\-+/A-Za-z->!][*<>\?\-+/A-Za-z0-9->!]*(\[\d+\]|)", "SYMBOL"),
                new TokenDefinition(@"\[", "ARRAYLEFT"),
                new TokenDefinition(@"\]", "ARRAYRIGHT"),
                new TokenDefinition(@"\.", "DOT"),
                new TokenDefinition(@",", "COMMA"),
                new TokenDefinition(@"\(", "LEFT"),
                new TokenDefinition(@"\)", "RIGHT"),
                new TokenDefinition(@"(=|\+=|-=|\*=|\\=)", "ASSIGNMENT"),
                new TokenDefinition(@" (and|or) ", "OPERATOR"),
                new TokenDefinition(@"[ ]{4}", "TAB"),
                new TokenDefinition(@"\s", "SPACE"), // I don't care about this I think
                new TokenDefinition(@"\t", "TAB")
            };

            TextReader reader = new StringReader(code);
            Lexer lexer = new Lexer(reader, defs, new Action<int, int, string>(LexerException));
            ScriptVariable value = Step(lexer, this);
            return value.Return<T>();
        }

        /*private T Return<T>(object value)
        {
            var returnT = ScriptType.ToEnum(typeof(T));
            switch (ScriptType.ToEnum(typeof(T)))
            {
                case ScriptTypes.String:
                case ScriptTypes.Integer:
                case ScriptTypes.Double:
                case ScriptTypes.Boolean:
                case ScriptTypes.Regex:
                    return (T)value;
                case ScriptTypes.ListString:
                    return (T)(object)((List<object>)(value)).Select(x => x.ToString()).ToList();
                case ScriptTypes.ListInteger:
                    return (T)(object)((List<object>)(value)).Select(x => (string)x).ToList();
                case ScriptTypes.ListDouble:
                    return (T)(object)((List<object>)(value)).Select(x => (string)x).ToList();
                case ScriptTypes.ListBoolean:
                    return (T)(object)((List<object>)(value)).Select(x => (string)x).ToList();
                default:
                    return default(T);
            }
        }*/

        private void LexerException(int lineNumber, int position, string lineRemaining)
        {
            throw new ScriptException(
                message: String.Format("Invalid tokens at Line {0} Col {1}",
                    lineNumber,
                    position),
                row: lineNumber,
                column: position,
                method: "undefined"
            );
        }

        private ScriptVariable Step(Lexer lexer, ScriptClass classScope)
        {
            ScriptVariable result = new ScriptVariable(null, ScriptTypes.Null);
            while (lexer.Next())
            {
                if (lexer.Token == "BLOCK")
                {
                    Indent = indentOffset;
                    Debug.WriteLine("Block Statement");
                }
                else if (lexer.Token == "TAB")
                {
                    Indent++;
                    Debug.WriteLine("Tab: {0}", Indent);
                }
                else if (lexer.Token == "SYMBOL")
                {
                    // 1. Class
                    var classInstance = classScope.Classes.FirstOrDefault(x => x.Name == lexer.TokenContents);
                    if (classInstance != null)
                    {
                        Debug.WriteLine("Step Class: {0}", classInstance.Name);
                        result = Step(lexer, classInstance);
                        if (classScope.Name == null)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    // 2. Method
                    var functionInstances = classScope.Methods.Where(x => x.Name == lexer.TokenContents).ToList();
                    if (functionInstances.Count() > 0)
                    {
                        Debug.WriteLine("Step Function: {0}", lexer.TokenContents);
                        if ((Indent - indentOffset) > Depths.Length)
                        {
                            var functionInstance = functionInstances.First();
                            throw new ScriptException(
                                message: String.Format("Indented too far \"{0}\" Line {1} Col {2}",
                                    functionInstance.Name,
                                    lexer.LineNumber,
                                    lexer.Position),
                                row: lexer.LineNumber,
                                column: lexer.Position,
                                method: functionInstance.Name
                            );
                        }
                        else if (Indent == 0 || Depths[Indent - 1 - indentOffset])
                        {
                            result = StepMethod(lexer, functionInstances); // Not capturing just calling
                            if (classScope.Name != null)
                            {
                                break;
                            }
                        }
                        else
                        {
                            lexer.SkipBlock();
                            break;
                        }
                        continue;
                    }
                    // 3. Property
                    lexer.Prev();
                    return StepValue(lexer);
                    /*Error.DynamicInvoke(new ScriptError
                    {
                        Message = String.Format("Invalid class or function or property \"{0}\" Line {1} Col {2}",
                            lexer.TokenContents,
                            lexer.LineNumber,
                            lexer.Position),
                        LineNumber = lexer.LineNumber,
                        Position = lexer.Position,
                        MethodName = lexer.TokenContents
                    });*/
                }
                else if (lexer.Token == "LISTTYPE")
                {
                    switch (lexer.TokenContents)
                    {
                        case "string[]":
                            StepProperty<List<string>>(lexer, classScope);
                            break;
                        case "int[]":
                            StepProperty<List<int>>(lexer, classScope);
                            break;
                        case "double[]":
                            StepProperty<List<double>>(lexer, classScope);
                            break;
                        case "bool[]":
                            StepProperty<List<bool>>(lexer, classScope);
                            break;
                    }
                }
                else if (lexer.Token == "BASETYPE")
                {
                    var tempType = lexer.TokenContents;
                    lexer.Next();
                    if (lexer.Token == "LEFT")
                    {
                        lexer.Prev(); // LEFT
                        lexer.Prev(); // BASETYPE
                        return StepValue(lexer);
                    }
                    else if (lexer.Token == "SPACE")
                    {
                        switch (tempType)
                        {
                            case "string":
                                StepProperty<string>(lexer, classScope);
                                break;
                            case "int":
                                StepProperty<int>(lexer, classScope);
                                break;
                            case "double":
                                StepProperty<double>(lexer, classScope);
                                break;
                            case "bool":
                                StepProperty<bool>(lexer, classScope);
                                break;
                        }
                    }
                }
                else if (lexer.Token == "IF")
                {
                    var val = StepValue(lexer);
                    var check = DepthCondition(val.Return<bool>());
                    if (check == null)
                    {
                        throw new ScriptException("error in if");
                    }
                    Debug.WriteLine("If: {0}", Depths[Indent]);
                }
                else if (lexer.Token == "ELSEIF")
                {
                    if (DepthValidCondition()) // Previous has to be false
                    {
                        var check = DepthCondition(StepValue(lexer).Return<bool>());
                        if (check == null)
                        {
                            Debug.WriteLine("Else If: ERROR");
                            throw new ScriptException("error in if");
                        }
                        Debug.WriteLine("Else If: {0}", Depths[Indent]);
                    }
                    else // loop through, ignore everything!
                    {
                        while (lexer.Next())
                        {
                            if (lexer.Token == "BLOCK")
                            {
                                break;
                            }
                        }
                        // invalid end of script error
                    }
                }
                else if (lexer.Token == "ELSE")
                {
                    DepthInverseCondition();
                    Debug.WriteLine("Else: {0}", Depths[Indent]);
                }
                else if (lexer.Token == "STRING")
                {
                    lexer.Prev();
                    return StepValue(lexer);
                }
                else if (lexer.Token == "INTEGER")
                {
                    lexer.Prev();
                    return StepValue(lexer);
                }
                else if (lexer.Token == "DOUBLE")
                {
                    lexer.Prev();
                    return StepValue(lexer);
                }
                else if (lexer.Token == "BOOLEAN")
                {
                    lexer.Prev();
                    return StepValue(lexer);
                }
                else if (lexer.Token == "NULL")
                {
                    lexer.Prev();
                    return StepValue(lexer);
                }
                else if (lexer.Token == "KEYWORD")
                {
                    if (lexer.TokenContents == "return")
                    {
                        return Step(lexer, classScope);
                    }
                }
                //Debug.WriteLine("Token: {0} Contents: {1}", lexer.Token, lexer.TokenContents);
            }
            return result;
            throw new ScriptException(
                message: String.Format("Invalid lexer step on Line {1} Col {2}",
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position
            );
        }

        private void StepProperty<T>(Lexer lexer, ScriptClass classScope)
        {
            var name = "";
            while (lexer.Next())
            {
                if (lexer.Token == "SPACE") { }
                else if (lexer.Token == "SYMBOL")
                {
                    name = lexer.TokenContents;
                }
                else if (name != "" && lexer.Token == "ASSIGNMENT")
                {
                    SetVariable<T>(name, StepValue(lexer));
                    return;
                }
                else
                {
                    break;
                }
            }
        }

        /*private bool TryCast<ReturnT>(ScriptTypes inputType, object value, out ReturnT outValue)
        {
            var outputType = ScriptType.ToEnum(typeof(ReturnT));
            switch (outputType)
            {
                case ScriptTypes.String:
                    switch (inputType)
                    {
                        case ScriptTypes.String:
                            outValue = Return<ReturnT>(value);
                            return true;
                        case ScriptTypes.Integer:
                        case ScriptTypes.Double:
                            outValue = Return<ReturnT>(value.ToString());
                            return true;
                        case ScriptTypes.Boolean:
                            outValue = Return<ReturnT>(Return<bool>(value) ? "true" : "false");
                            return true;
                        case ScriptTypes.Null:
                            outValue = Return<ReturnT>("null");
                            return true;
                    }
                    break;
                case ScriptTypes.Integer:
                    switch (inputType)
                    {
                        case ScriptTypes.String:
                            outValue = Return<ReturnT>(value);
                            return true;
                        case ScriptTypes.Integer:
                            int tryInt = 0;
                            if (int.TryParse(value.ToString(), out tryInt))
                            {
                                outValue = Return<ReturnT>(tryInt);
                                return true;
                            }
                            else
                            {
                                outValue = Return<ReturnT>(0);
                                return false;
                            }
                        case ScriptTypes.Double:
                            double tryDouble = 0;
                            if (double.TryParse(value.ToString(), out tryDouble))
                            {
                                outValue = Return<ReturnT>(tryDouble);
                                return true;
                            }
                            else
                            {
                                outValue = Return<ReturnT>(0);
                                return false;
                            }
                        case ScriptTypes.Boolean:
                            outValue = Return<ReturnT>(Return<bool>(value) ? "true" : "false");
                            return true;
                    }
                    break;
                case ScriptTypes.Double:
                    switch (inputType)
                    {

                        case ScriptTypes.Double:
                            outValue = Return<ReturnT>(value);
                            return true;
                        case ScriptTypes.String:
                        case ScriptTypes.Integer:
                            double tryDouble = 0;
                            if (double.TryParse(value.ToString(), out tryDouble))
                            {
                                outValue = Return<ReturnT>(tryDouble);
                                return true;
                            }
                            else
                            {
                                outValue = Return<ReturnT>(0.0);
                                return false;
                            }
                    }
                    break;
                case ScriptTypes.Boolean:
                    outValue = Return<ReturnT>(value);
                    return true;
                case ScriptTypes.ListString:
                case ScriptTypes.ListInteger:
                case ScriptTypes.ListDouble:
                case ScriptTypes.ListBoolean:
                    outValue = Return<ReturnT>(value);
                    return true;
                case ScriptTypes.Void:
                    outValue = default(ReturnT);
                    return true;
            }
            outValue = Return<ReturnT>(null);
            return false;
        }

        private ReturnT Cast<ReturnT>(Lexer lexer, ScriptTypes inputType, object value)
        {
            var outputType = ScriptType.ToEnum(typeof(ReturnT));
            if (TryCast<ReturnT>(inputType, value, out outValue))
            {
                return outValue;
            }
            lexer.Prev();
            lexer.Prev();
            Error.DynamicInvoke(new ScriptException(
                message: String.Format("Unable to cast value '{0}' from '{1}' to '{2}' on Line {3} Col {4}",
                    value.ToString(),
                    inputType.ToString(),
                    outputType.ToString(),
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position,
                method: lexer.TokenContents
            );
        }*/

        private ReturnT StepValue<ReturnT>(Lexer lexer)
        {
            var value = StepValue(lexer);
            return value.Cast<ReturnT>(lexer).Return<ReturnT>();
        }

        /// <summary>
        /// Step for complex value
        /// </summary>
        /// <param name="lexer">Lexer</param>
        /// <returns>Valid value or null</returns>
        private ScriptVariable StepValue(Lexer lexer)
        {
            var current = new ScriptVariable(null, ScriptTypes.Undefined);
            while (lexer.Next())
            {
                if (lexer.Token == "SPACE") { continue; }
                else if (lexer.Token == "BLOCK")
                {
                    return current;
                }
                else if (lexer.Token == "NULL")
                {
                    current.Type = ScriptTypes.Null;
                    current.Value = null;
                }
                else if (lexer.Token == "BASETYPE")
                {
                    var castType = lexer.TokenContents;
                    lexer.Next();
                    if (lexer.Token == "LEFT")
                    {
                        var fromValue = StepValue(lexer);
                        switch (castType)
                        {
                            case "string":
                                return fromValue.Cast<string>(lexer);
                            case "int":
                                return fromValue.Cast<int>(lexer);
                            case "double":
                                return fromValue.Cast<double>(lexer);
                            case "bool":
                                return fromValue.Cast<bool>(lexer);
                        }

                    }
                    // Error should be impossible
                }
                else if (lexer.Token == "COMMA" || lexer.Token == "ARRAYRIGHT")
                {
                    lexer.Prev();
                    return current;
                }
                else if (lexer.Token == "DOT")
                {
                    lexer.Next();
                    if (lexer.Token == "SYMBOL")
                    {
                        var typeFunctionInstances = TypeFunctions.Where(x => x.ExtendType == current.Type && x.Name == lexer.TokenContents).Select(x => (ScriptTypeMethod)x).ToList();
                        if (typeFunctionInstances.Count() > 0)
                        {
                            current = StepTypeFunction(lexer, typeFunctionInstances, current);
                            continue;
                        }
                        throw new ScriptException(
                            message: String.Format("Type '{0}' has no method named '{1}' on Line {2} Col {3}",
                                        current.Type.ToString(),
                                        lexer.TokenContents,
                                        lexer.LineNumber,
                                        lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position,
                            method: lexer.TokenContents
                        );
                    }
                }
                else if (lexer.Token == "SYMBOL")
                {
                    var matches = Regex.Matches(lexer.TokenContents, @"^([^\[\]]+?)(\[(\d+)\])?$");
                    if (matches[0].Groups[3].Success) // Is as List
                    {
                        var property = Variables.FirstOrDefault(p => p.Name == matches[0].Groups[1].Value);
                        if (property != null)
                        {
                            var index = int.Parse(matches[0].Groups[3].Value);
                            var list = ((List<ScriptVariable>)(property.Value));
                            if (index >= list.Count())
                            {
                                throw new ScriptException(
                                    message: String.Format("Property '{0}' contains {1} items (not {2}) on Line {3} Col {4}",
                                        property.Name,
                                        list.Count(),
                                        index + 1,
                                        lexer.LineNumber,
                                        lexer.Position),
                                    row: lexer.LineNumber,
                                    column: lexer.Position,
                                    method: lexer.TokenContents
                                );
                            }
                            switch (property.Type)
                            {
                                case ScriptTypes.ListString:
                                case ScriptTypes.ListInteger:
                                case ScriptTypes.ListDouble:
                                case ScriptTypes.ListBoolean:
                                    return list[index];
                            }
                            throw new ScriptException(
                                message: String.Format("Type '{0}' has no method named '{1}' on Line {2} Col {3}",
                                            current.Type.ToString(),
                                            lexer.TokenContents,
                                            lexer.LineNumber,
                                            lexer.Position),
                                row: lexer.LineNumber,
                                column: lexer.Position,
                                method: lexer.TokenContents
                            );
                        }
                    }
                    else
                    {
                        var property = Variables.FirstOrDefault(p => p.Name == matches[0].Groups[1].Value);
                        if (property != null)
                        {
                            current.Type = property.Type;
                            current.Value = property.Value;
                        }
                    }
                    // Methods
                    var methodInstances = Methods.Where(x => x.Name == lexer.TokenContents).ToList();
                    if (methodInstances.Count() > 0)
                    {
                        current = StepMethod(lexer, methodInstances);
                    }
                }
                else if (lexer.Token == "RIGHT")
                {
                    // No pemdas support, fix this later on!
                    return current;
                }
                else if (lexer.Token == "ARRAYLEFT")
                {
                    current = StepList(lexer);
                }
                else if (lexer.Token == "STRING")
                {
                    if (current.Type != ScriptTypes.Undefined) {
                        lexer.Prev();
                        throw new ScriptException(
                            message: String.Format("Unexpected string on Line {0} Col {1}",
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                    current.Type = ScriptTypes.String;
                    current.Value = lexer.TokenContents.Substring(1, lexer.TokenContents.Length - 2);
                }
                else if (lexer.Token == "INTEGER")
                {
                    current.Type = ScriptTypes.Integer;
                    current.Value = int.Parse(lexer.TokenContents);
                }
                else if (lexer.Token == "DOUBLE")
                {
                    current.Type = ScriptTypes.Double;
                    current.Value = double.Parse(lexer.TokenContents);
                }
                else if (lexer.Token == "BOOLEAN")
                {
                    current.Type = ScriptTypes.Boolean;
                    current.Value = lexer.TokenContents == "true";
                }
                else if (lexer.Token == "REGEX")
                {
                    if (current.Type != ScriptTypes.Undefined) { return current; }
                    current.Type = ScriptTypes.Regex;
                    var optionsChars = lexer.TokenContents.Substring(lexer.TokenContents.LastIndexOf('/') + 1).ToCharArray();
                    var options = RegexOptions.ECMAScript;
                    if (optionsChars.Contains('i'))
                    {
                        options |= RegexOptions.IgnoreCase;
                    }
                    if (optionsChars.Contains('m'))
                    {
                        options |= RegexOptions.Multiline;
                    }
                    if (optionsChars.Contains('s'))
                    {
                        options |= RegexOptions.Singleline;
                    }
                    current.Value = new Regex(lexer.TokenContents.Substring(1, lexer.TokenContents.LastIndexOf('/') - 1), options);
                }
                else if (lexer.Token == "OPERATOR")
                {
                    if (lexer.TokenContents == "and")
                    {
                        current.Value = current.Cast<bool>(lexer).Return<bool>() && StepValue<bool>(lexer);
                    }
                    else if (lexer.TokenContents == "or")
                    {
                        current.Value = current.Cast<bool>(lexer).Return<bool>() || StepValue<bool>(lexer);
                    }
                }
                else if (lexer.Token == "ARITHMETIC")
                {
                    var symbol = lexer.TokenContents;
                    var next = StepValue(lexer);
                    switch (symbol)
                    {
                        case "+":
                            if (current.Type == ScriptTypes.Integer && next.Type == ScriptTypes.Integer)
                            {
                                current.Value = (int)current.Value + (int)next.Value;
                                current.Type = ScriptTypes.Integer;
                            }
                            else if (current.Type == ScriptTypes.Integer && next.Type == ScriptTypes.ListString)
                            {
                                current.Value = current.Value.ToString() + string.Join(",", next.Return<List<string>>());
                                current.Type = ScriptTypes.String;
                            }
                            else if (current.Type == ScriptTypes.Integer && next.Type == ScriptTypes.Double)
                            {
                                current.Value = current.Cast<double>(lexer).Return<double>() + next.Cast<double>(lexer).Return<double>();
                                current.Type = ScriptTypes.Double;
                            }
                            else
                            {
                                current.Value = current.Value.ToString() + next.Value.ToString();
                                current.Type = ScriptTypes.String;
                            }
                            break;
                        case "-":
                            if (current.Type == ScriptTypes.Integer && next.Type == ScriptTypes.Integer)
                            {
                                current.Value = (int)current.Value - (int)next.Value;
                                current.Type = ScriptTypes.Integer;
                            }
                            else
                            {
                                current.Value = current.Cast<double>(lexer).Return<double>() - next.Cast<double>(lexer).Return<double>();
                                current.Type = ScriptTypes.Double;
                            }
                            break;
                        case "*":
                            if (current.Type == ScriptTypes.Integer && next.Type == ScriptTypes.Integer)
                            {
                                current.Value = (int)current.Value * (int)next.Value;
                                current.Type = ScriptTypes.Integer;
                            }
                            else
                            {
                                current.Value = current.Cast<double>(lexer).Return<double>() * next.Cast<double>(lexer).Return<double>();
                                current.Type = ScriptTypes.Double;
                            }
                            break;
                        case "/":
                            if (current.Type == ScriptTypes.Integer)
                            {
                                if (current.Type == ScriptTypes.Integer && next.Type == ScriptTypes.Integer)
                                {
                                    current.Value = current.Cast<double>(lexer).Return<double>() / next.Cast<double>(lexer).Return<double>();
                                    current.Type = ScriptTypes.Double;
                                }
                                else
                                {
                                    current.Value = current.Cast<double>(lexer).Return<double>() / next.Cast<double>(lexer).Return<double>();
                                    current.Type = ScriptTypes.Double;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    return current;
                }
            }
            // Syntax error thrown here
            return current;
        }

        private ScriptVariable StepList(Lexer lexer)
        {
            var type = ScriptTypes.Undefined;
            var items = new List<ScriptVariable>();
            var hasComma = true;
            while (lexer.Next())
            {
                if (lexer.Token == "SPACE") { }
                else if (lexer.Token == "ARRAYRIGHT")
                {
                    type = type == ScriptTypes.String ? ScriptTypes.ListString
                        : type == ScriptTypes.Integer ? ScriptTypes.ListInteger
                        : type == ScriptTypes.Double ? ScriptTypes.ListDouble
                        : type == ScriptTypes.Boolean ? ScriptTypes.ListBoolean
                        : ScriptTypes.Undefined;
                    return new ScriptVariable(items, type);
                }
                else if (lexer.Token == "COMMA")
                {
                    hasComma = true;
                }
                else if (hasComma)
                {
                    lexer.Prev();
                    var variable = StepValue(lexer);
                    if (type == ScriptTypes.Undefined)
                    {
                        type = variable.Type;
                    }
                    else if (type != variable.Type)
                    {
                        throw new ScriptException(
                            message: String.Format("Invalid data type '{0}' in '{1}' list near Line {2} Col {3}",
                                type.ToString(),
                                "unknown",
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                    items.Add(variable);
                    hasComma = false;
                }
                else
                {
                    throw new ScriptException(
                        message: String.Format("Missing \",\" near Line {0} Col {1}",
                            lexer.LineNumber,
                            lexer.Position),
                        row: lexer.LineNumber,
                        column: lexer.Position
                    );
                }
                /*if (lexer.Token == "STRING")
                {
                    if (!(type == ScriptTypes.Null || type == ScriptTypes.ListString))
                    {
                        throw new ScriptException(
                            message: String.Format("Invalid STRING found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                    items.Add(lexer.TokenContents.Substring(1, lexer.TokenContents.Length - 2));
                    type = ScriptTypes.ListString;
                }
                else if (lexer.Token == "DOUBLE")
                {
                    if (!(type == ScriptTypes.Null || type != ScriptTypes.ListDouble))
                    {
                        throw new ScriptException(
                            message: String.Format("Invalid DOUBLE found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                    items.Add(double.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == "INTEGER")
                {
                    if (!(type == ScriptTypes.Null || type != ScriptTypes.ListInteger))
                    {
                        throw new ScriptException(
                            message: String.Format("Invalid INTEGER found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                    items.Add(int.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == "BOOLEAN")
                {
                    if (!(type == ScriptTypes.Null || type != ScriptTypes.ListBoolean))
                    {
                        throw new ScriptException(
                            message: String.Format("Invalid BOOLEAN found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                    items.Add(lexer.TokenContents == "true");
                }*/
            }
            throw new ScriptException(
                message: String.Format("Missing \"]\" near Line {0} Col {1}",
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position
            );
        }

        /// <summary>
        /// Simply call the StepFunction
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="functionInstances"></param>
        /// <returns></returns>
        private ScriptVariable StepTypeFunction(Lexer lexer, List<ScriptTypeMethod> functionTypeInstances, ScriptVariable parentValue)
        {
            return StepTypeMethod(lexer, functionTypeInstances, parentValue);
        }

        private ScriptVariable StepMethod(Lexer lexer, List<ScriptMethod> functionInstances)
        {
            return StepMethod(lexer, functionInstances, new ScriptVariable(null, ScriptTypes.Undefined));
        }

        private List<ScriptVariable> StepArguments(Lexer lexer)
        {
            List<ScriptVariable> args = new List<ScriptVariable>();
            string MethodName = lexer.TokenContents;

            while (lexer.Next())
            {
                if (lexer.Token == "LEFT")
                {
                    var usedComma = true;
                    while (lexer.Next())
                    {
                        if (lexer.Token == "SPACE") { }
                        else if (lexer.Token == "COMMA")
                        {
                            usedComma = true;
                        }
                        else if (lexer.Token == "RIGHT")
                        {
                            goto DoneArgs;
                        }
                        else
                        {
                            if (usedComma)
                            {
                                lexer.Prev();
                                args.Add(StepValue(lexer));
                                usedComma = false;
                                if (lexer.Token == "RIGHT")
                                {
                                    goto DoneArgs;
                                }
                                else
                                {
                                    //lexer.Prev();
                                }
                            }
                            else
                            {
                                throw new ScriptException(
                                    message: String.Format("Missing comma in function call \"{0}\" Line {1} Col {2}",
                                        MethodName,
                                        lexer.LineNumber,
                                        lexer.Position),
                                    row: lexer.LineNumber,
                                    column: lexer.Position,
                                    method: MethodName
                                );
                            }
                        }
                    }
                }
            }
            DoneArgs:
            return args;
        }

        private ScriptVariable StepMethod(Lexer lexer, List<ScriptMethod> functionInstances, ScriptVariable parentValue)
        {
            var args = StepArguments(lexer);
            if (args == null)
            {
                throw new ScriptException(
                    message: String.Format("Syntax error in arguments for \"{0}\" Line {1} Col {2}",
                        "test",
                        lexer.LineNumber,
                        lexer.Position),
                    row: lexer.LineNumber,
                    column: lexer.Position,
                    method: "test"
                );
            }
            foreach (var functionInstance in functionInstances)
            {
                if (args.Select(pair => pair.Type).SequenceEqual(functionInstance.Types))
                {
                    if (parentValue.Type != ScriptTypes.Undefined)
                    {
                        args.Insert(0, parentValue);
                    }
                    return new ScriptVariable(functionInstance.Method.DynamicInvoke(args.Select(pair =>
                    {
                        switch (pair.Type)
                        {
                            case ScriptTypes.Regex:
                                return pair.Return<Regex>();
                            case ScriptTypes.ListString:
                                return pair.Return<List<string>>();
                            default:
                                return pair.Value;
                        }
                    }).ToArray()), functionInstance.ReturnType);
                }
            }
            throw new ScriptException(
                message: String.Format("Invalid arguments in \"{0}\" Line {1} Col {2}",
                    "test",
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position,
                method: "test"
            );
        }

        private ScriptVariable StepTypeMethod(Lexer lexer, List<ScriptTypeMethod> functionInstances, ScriptVariable parentValue)
        {
            var args = StepArguments(lexer);
            foreach (var functionInstance in functionInstances)
            {
                if (args.Select(pair => pair.Type).SequenceEqual(functionInstance.Types))
                {
                    if (parentValue.Type != ScriptTypes.Undefined)
                    {
                        args.Insert(0, parentValue);
                    }
                    var returnVariable = functionInstance.Method.DynamicInvoke(args.Select(pair =>
                    {
                        switch (pair.Type)
                        {
                            case ScriptTypes.Regex:
                                return pair.Return<Regex>();
                            case ScriptTypes.ListString:
                                return pair.Return<List<string>>();
                            default:
                                return pair.Value;
                        }
                    }).ToArray());
                    switch(functionInstance.ReturnType)
                    {
                        case ScriptTypes.ListString:
                        case ScriptTypes.ListInteger:
                        case ScriptTypes.ListDouble:
                        case ScriptTypes.ListBoolean:
                            return new ScriptVariable(((List<string>)returnVariable).Select(x => new ScriptVariable(x, ScriptTypes.String)).ToList(), functionInstance.ReturnType);
                        default:
                            return new ScriptVariable(returnVariable, functionInstance.ReturnType);
                    }
                }
            }
            throw new ScriptException(
                message: String.Format("Invalid arguments in \"{0}\" Line {1} Col {2}",
                    "test",
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position
            );
        }

        private void StepArray(Lexer lexer, out ScriptTypes type, out object list)
        {
            List<object> array = new List<object>();
            while (lexer.Next())
            {
                if (lexer.Token == "STRING")
                {
                    type = ScriptTypes.ListString;
                    array.Add(lexer.TokenContents.Substring(1, lexer.TokenContents.Length - 2));
                }
                else if (lexer.Token == "DOUBLE")
                {
                    type = ScriptTypes.ListDouble;
                    array.Add(double.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == "INTEGER")
                {
                    type = ScriptTypes.ListInteger;
                    array.Add(int.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == "BOOLEAN")
                {
                    type = ScriptTypes.ListBoolean;
                    array.Add(lexer.TokenContents == "true");
                }
                else if (lexer.Token == "ARRAYRIGHT")
                {
                    list = array.ToList();
                }
            }
            throw new ScriptException(
                message: String.Format("Invalid list syntax Line {0} Col {1}",
                    lexer.LineNumber,
                    lexer.Position),
                row: lexer.LineNumber,
                column: lexer.Position,
                method: "array"
            );
        }

        /*private Nullable<bool> StepCondition(Lexer lexer, ScriptClass classInstance)
        {
            var result = false;
            while (lexer.Next())
            {
                if (lexer.Token == "SYMBOL")
                {
                    var conditionInstances = classInstance.Conditions.Where(x => x.Name == lexer.TokenContents).ToList<ScriptFunction>();
                    if (conditionInstances != null)
                    {
                        Debug.WriteLine("Step Condition: {0}", lexer.TokenContents);
                        var check = StepFunction(lexer, conditionInstances);
                        if (check == null)
                        {
                            return null;
                        }
                        else
                        {
                            result = (bool)check;
                        }
                    }
                }
                else if (lexer.Token == "OPERATOR")
                {
                    if (lexer.TokenContents == "and")
                    {
                        var check = StepCondition(lexer, classInstance);
                        if (check == null)
                        {
                            return null;
                        }
                        else
                        {
                            result = result && (bool)check;
                        }
                    }
                    else if (lexer.TokenContents == "or")
                    {
                        var check = StepCondition(lexer, classInstance);
                        if (check == null)
                        {
                            return null;
                        }
                        else
                        {
                            result = result && (bool)check;
                        }
                    }
                }
                else if (lexer.Token == "RIGHT")
                {
                    return result;
                }
                else if (lexer.Token == "BLOCK")
                {
                    Error.DynamicInvoke(new ScriptError
                    {
                        Message = String.Format("Invalid conditional syntax Line {1} Col {2}",
                            "if",
                            lexer.LineNumber,
                            lexer.Position),
                        LineNumber = lexer.LineNumber,
                        Position = lexer.Position,
                        MethodName = "if"
                    });
                }
            }
            return false; /// needs to throw error
        }*/

        private void TriggerEvent(Lexer lexer, ScriptClass classInstance)
        {
            while (lexer.Next())
            {
                if (lexer.Token == "MATCH")
                {
                    var script = new StringBuilder();
                    while (lexer.Next())
                    {
                        if (lexer.Token == "CODE")
                        {
                            script.AppendLine(lexer.TokenContents);
                        }
                        else if (lexer.Token != "BLOCK")
                        {
                            break;
                        }
                    }
                    Debug.WriteLine("Run: {0}", script.ToString());
                    Depths[0] = true;
                    Parse<object>(script.ToString(), 1);
                }
                Debug.WriteLine("Token: {0} Contents: {1}", lexer.Token, lexer.TokenContents);
            }
        }

        private List<ScriptTypeFunction> TypeFunctions = new List<ScriptTypeFunction>();

        public void TypeFunction<InputT, ReturnT>(string methodName, Func<InputT, ReturnT> method)
        {
            var inputT = ScriptType.ToEnum(typeof(InputT));
            var returnT = ScriptType.ToEnum(typeof(ReturnT));
            TypeFunctions.Add(new ScriptTypeFunction(inputT, methodName, method, returnT));
        }

        public void TypeFunction<InputT, T1, ReturnT>(string methodName, Func<InputT, T1, ReturnT> method)
        {
            var inputT = ScriptType.ToEnum(typeof(InputT));
            var t1 = ScriptType.ToEnum(typeof(T1));
            var returnT = ScriptType.ToEnum(typeof(ReturnT));
            ScriptTypes[] args = { t1 };
            TypeFunctions.Add(new ScriptTypeFunction(inputT, methodName, method, args, returnT));
        }

        public void TypeFunction<InputT, T1, T2, ReturnT>(string methodName, Func<InputT, T1, T2, ReturnT> method)
        {
            var inputT = ScriptType.ToEnum(typeof(InputT));
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            var returnT = ScriptType.ToEnum(typeof(ReturnT));
            ScriptTypes[] args = { t1, t2 };
            TypeFunctions.Add(new ScriptTypeFunction(inputT, methodName, method, args, returnT));
        }

        private List<ScriptTypeCondition> TypeConditions = new List<ScriptTypeCondition>();
        public void TypeCondition<InputT>(string methodName, Func<InputT> method)
        {
            var inputT = ScriptType.ToEnum(typeof(InputT));
            TypeConditions.Add(new ScriptTypeCondition(inputT, methodName, method));
        }

        public void TypeCondition<InputT, T1, T2>(string methodName, Func<InputT, T1> method)
        {
            var inputT = ScriptType.ToEnum(typeof(InputT));
            var t1 = ScriptType.ToEnum(typeof(T1));
            ScriptTypes[] args = { t1 };
            TypeConditions.Add(new ScriptTypeCondition(inputT, methodName, method, args));
        }
        public void TypeCondition<InputT, T1, T2>(string methodName, Func<InputT, T1, T2> method)
        {
            var inputT = ScriptType.ToEnum(typeof(InputT));
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            ScriptTypes[] args = { t1, t2 };
            TypeConditions.Add(new ScriptTypeCondition(inputT, methodName, method, args));
        }

        private List<ScriptTypeProperty> TypeProperties = new List<ScriptTypeProperty>();

        public void TypeProperty<InputT, ReturnT>(string propertyName, Func<InputT, ReturnT> method)
        {
            var inputT = ScriptType.ToEnum(typeof(InputT));
            TypeProperties.Add(new ScriptTypeProperty(inputT, propertyName, method));
        }
    }
}
