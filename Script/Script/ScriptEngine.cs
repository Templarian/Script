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
            try
            {
                return Parse<T>(code);
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
            return (T)(object)"";
        }

        public void Process(string script)
        {
            code = script;
        }

        public void Trigger(string eventName)
        {
            Trigger(eventName, new List<ScriptVariable> { });
        }

        public void Trigger(string eventName, object arg1)
        {
            var args = new List<ScriptVariable> {
                new ScriptVariable(arg1)
            };
            try
            {
                Trigger(eventName, args);
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

        private void Trigger(string eventName, List<ScriptVariable> arguments)
        {
            TextReader reader = new StringReader(code);
            var defs = this.tokenDefinitions.First(x => x.Name == "event");
            defs.TokenDefinitions[0].Matcher = new RegexMatcher(@"event[ ]+" + eventName + @"[ ]*\(");
            defs.TokenDefinitions[1].Matcher = new RegexMatcher(@"event[ ]+" + eventName);
            Lexer lexer = new Lexer(reader, this.tokenDefinitions, new Action<int, int, string>(LexerException));
            lexer.Step("event");
            while (lexer.Next())
            {
                if (lexer.Token == Tokens.Block)
                {
                    if (!lexer.Next(true))
                    {
                        break;
                    }
                    //this.indentOffset = -1;
                    Depths[0] = true;
                    if (lexer.Token == Tokens.EventName)
                    {
                        Step(lexer, this);
                        return;
                    }
                    else if (lexer.Token == Tokens.EventNameParameters)
                    {
                        var args = StepParameters(lexer);
                        if (args.Count() != arguments.Count())
                        {
                            throw new ScriptException(
                                message: String.Format("'{0}' requires {1} properties at Line {2} Col {3}",
                                    eventName,
                                    arguments.Count(),
                                    lexer.LineNumber,
                                    lexer.Position),
                                row: lexer.LineNumber,
                                column: lexer.Position
                            );
                        }
                        for (int i = 0; i < arguments.Count(); i++)
                        {
                            SetVariable(args[i], arguments[i]);
                        }
                        Step(lexer, this);
                        return;
                    }
                }
                else
                {
                    throw new ScriptException(
                        message: String.Format("Invalid syntax at Line {0} Col {1}",
                            lexer.LineNumber,
                            lexer.Position),
                        row: lexer.LineNumber,
                        column: lexer.Position
                    );
                }
            }
            //TriggerEvent(lexer, arguments, this);
        }

        private int Indent { get; set; }

        // We need to track the boolean values
        // be realistic... 20+ depths = terrible script
        private Nullable<bool>[] Depths = new Nullable<bool>[20];

        private bool DepthCondition()
        {
            return (bool)Depths[Indent];

        }

        private Nullable<bool> DepthCondition(Nullable<bool> result)
        {
            if (result != null)
            {
                Depths[Indent] = result;
            }
            return result; // Makes code look cleaner if it passes through
        }

        private bool DepthValidCondition()
        {
            if ((bool)Depths[Indent]) // if previous block true
            {
                Depths[Indent] = false;
                return false; // set block to false return false
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

        private List<TokenDefinitionGroup> tokenDefinitions = new List<TokenDefinitionGroup> {
            new TokenDefinitionGroup("root", new TokenDefinition[]
            {
                TokenDefinitions.Comment,
                TokenDefinitions.Regex,
                TokenDefinitions.String,
                TokenDefinitions.Double,
                TokenDefinitions.Integer,
                TokenDefinitions.Boolean,
                TokenDefinitions.Null,
                TokenDefinitions.If,
                TokenDefinitions.ElseIf,
                TokenDefinitions.Else,
                TokenDefinitions.For,
                TokenDefinitions.Arithmetic,
                TokenDefinitions.ListType,
                TokenDefinitions.BaseType,
                TokenDefinitions.Keyword,
                TokenDefinitions.Symbol,
                TokenDefinitions.ArrayLeft,
                TokenDefinitions.ArrayRight,
                TokenDefinitions.Dot,
                TokenDefinitions.Comma,
                TokenDefinitions.Left,
                TokenDefinitions.Right,
                TokenDefinitions.Assignment,
                TokenDefinitions.Operator,
                TokenDefinitions.Tab,
                TokenDefinitions.Space
            }),
            new TokenDefinitionGroup("eventargs", new TokenDefinition[]
            {
                TokenDefinitions.Symbol,
                TokenDefinitions.Comma,
                TokenDefinitions.Space,
                TokenDefinitions.Right
            }),
            new TokenDefinitionGroup("parameters", new TokenDefinition[]
            {
                TokenDefinitions.BaseType,
                TokenDefinitions.ListType,
                TokenDefinitions.Symbol,
                TokenDefinitions.Comma,
                TokenDefinitions.Space,
                TokenDefinitions.Right
            }),
            new TokenDefinitionGroup("event", new TokenDefinition[]
            {
                new TokenDefinition(@"event[ ]+error[ ]*\(", Tokens.EventNameParameters),
                new TokenDefinition(@"event[ ]+error", Tokens.EventName)
            })
        };

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

            TextReader reader = new StringReader(code);
            Lexer lexer = new Lexer(reader, this.tokenDefinitions, new Action<int, int, string>(LexerException));
            ScriptVariable value = Step(lexer, this);
            return value.Return<T>();
        }

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
            lexer.Step("root");
            ScriptVariable result = new ScriptVariable();
            while (lexer.Next())
            {
                switch (lexer.Token)
                {
                    case Tokens.Block:
                        Indent = indentOffset;
                        Debug.WriteLine("Block Statement");
                        break;
                    case Tokens.Tab:
                        Indent++;
                        Debug.WriteLine("Tab: {0}", Indent);
                        if (Depths[Indent - 1] == true)
                        {
                            continue;
                        }
                        else if (Depths[Indent - 1] == false)
                        {
                            lexer.SkipBlock();
                            continue;
                        }
                        else if (Depths[Indent - 1] == null)
                        {
                            throw new ScriptException(
                                message: String.Format("Syntax error on Line {0} Col {1}",
                                    lexer.LineNumber,
                                    lexer.Position),
                                row: lexer.LineNumber,
                                column: lexer.Position
                            );
                        }
                        break;
                    case Tokens.Symbol:
                        /*var functionInstances = classScope.Methods.Where(x => x.Name == lexer.TokenContents).ToList();
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
                        var classInstance = classScope.Classes.FirstOrDefault(x => x.Name == lexer.TokenContents);
                        if (classInstance != null)
                        {
                            result = Step(lexer, classInstance);
                            if (classScope.Name == null)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }*/
                        lexer.Prev();
                        return StepValue(lexer);
                    case Tokens.ListType:
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
                        break;
                    case Tokens.BaseType:
                        var tempType = lexer.TokenContents;
                        lexer.Next();
                        if (lexer.Token == Tokens.Left)
                        {
                            lexer.Prev(); // LEFT
                            lexer.Prev(); // BASETYPE
                            return StepValue(lexer, this);
                        }
                        else if (lexer.Token == Tokens.Space)
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
                                case "var":
                                    StepProperty<object>(lexer, classScope);
                                    break;
                            }
                        }
                        break;
                    case Tokens.If:
                        var val = StepValue(lexer);
                        var check = DepthCondition(val.Return<bool>());
                        if (check == null)
                        {
                            throw new ScriptException("error in if");
                        }
                        Debug.WriteLine("If: {0}", Depths[Indent]);
                        break;
                    case Tokens.ElseIf:
                        if (DepthValidCondition()) // Previous has to be false
                        {
                            var check2 = DepthCondition(StepValue(lexer).Return<bool>());
                            if (check2 == null)
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
                                if (lexer.Token == Tokens.Block)
                                {
                                    break;
                                }
                            }
                            // invalid end of script error
                        }
                        break;
                    case Tokens.Else:
                        DepthInverseCondition();
                        Debug.WriteLine("Else: {0}", Depths[Indent]);
                        break;
                    case Tokens.For:
                        break;
                    case Tokens.String:
                        lexer.Prev();
                        return StepValue(lexer);
                    case Tokens.Integer:
                        lexer.Prev();
                        return StepValue(lexer);
                    case Tokens.Double:
                        lexer.Prev();
                        return StepValue(lexer);
                    case Tokens.Boolean:
                        lexer.Prev();
                        return StepValue(lexer);
                    case Tokens.Null:
                        lexer.Prev();
                        return StepValue(lexer);
                    case Tokens.Keyword:
                        switch (lexer.TokenContents)
                        {
                            case "return":
                                return StepValue(lexer, this);
                            case "event":
                                return result; // Stop script
                        }
                        break;
                    default:
                        throw new ScriptException(
                            message: String.Format("Unknown keyword {0} on Line {1} Col {2}",
                                lexer.TokenContents,
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
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
                if (lexer.Token == Tokens.Space) { }
                else if (lexer.Token == Tokens.Symbol)
                {
                    name = lexer.TokenContents;
                }
                else if (name != "" && lexer.Token == Tokens.Assignment)
                {
                    ScriptTypes type = ScriptType.ToEnum(typeof(T));
                    var variable = StepValue(lexer);
                    if (variable.Type == ScriptTypes.Undefined && ScriptType.IsList(type))
                    {
                        variable.Type = type;
                    }
                    if (type == ScriptTypes.Any || type == variable.Type)
                    {
                        SetVariable(name, variable);
                    }
                    else
                    {
                        lexer.Prev();
                        lexer.Prev();
                        throw new ScriptException(
                            message: String.Format("{0} '{1}' set to {2} on Line {3} Col {4}",
                                type.ToString(),
                                name,
                                variable.Type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                    return;
                }
                else
                {
                    break;
                }
            }
        }

        private ReturnT StepValue<ReturnT>(Lexer lexer)
        {
            var value = StepValue(lexer, this);
            return value.Cast<ReturnT>(lexer).Return<ReturnT>();
        }

        private ScriptVariable StepValue(Lexer lexer)
        {
            return StepValue(lexer, this);
        }

        private ReturnT StepValue<ReturnT>(Lexer lexer, ScriptClass classScope)
        {
            var value = StepValue(lexer, classScope);
            return value.Cast<ReturnT>(lexer).Return<ReturnT>();
        }

        /// <summary>
        /// Step for complex value
        /// </summary>
        /// <param name="lexer">Lexer</param>
        /// <returns>Valid value or null</returns>
        private ScriptVariable StepValue(Lexer lexer, ScriptClass classScope)
        {
            var current = new ScriptVariable();
            while (lexer.Next())
            {
                switch (lexer.Token)
                {
                    case Tokens.Space:
                        break;
                    case Tokens.Block:
                        return current;
                    case Tokens.Null:
                        current.Type = ScriptTypes.Null;
                        current.Value = null;
                        break;
                    case Tokens.BaseType:
                        var castType = lexer.TokenContents;
                        lexer.Next();
                        if (lexer.Token == Tokens.Left)
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
                        break;
                    case Tokens.Comma:
                        lexer.Prev();
                        return current;
                    case Tokens.ArrayRight:
                        lexer.Prev();
                        return current;
                    case Tokens.Dot:
                        lexer.Next();
                        if (lexer.Token == Tokens.Symbol && current.Type == ScriptTypes.Undefined)
                        {
                            // Classes
                            var classInstance2 = classScope.Classes.FirstOrDefault(x => x.Name == lexer.TokenContents);
                            if (classInstance2 != null)
                            {
                                var result = StepValue(lexer, classInstance2);
                                continue;
                            }
                            // Methods
                            var methodInstances = classScope.Methods.Where(x => x.Name == lexer.TokenContents).ToList();
                            if (methodInstances.Count() > 0)
                            {
                                return StepMethod(lexer, methodInstances);
                            }
                            // Properties
                            var propertyInstances = classScope.Properties.FirstOrDefault(x => x.Name == lexer.TokenContents);
                            if (propertyInstances != null)
                            {
                                return propertyInstances;
                            }
                        }
                        else if (lexer.Token == Tokens.Symbol)
                        {
                            // Typed Functions
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
                        lexer.Prev();
                        throw new ScriptException(
                            message: String.Format("Syntax error on Line {0} Col {1}",
                                        lexer.LineNumber,
                                        lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    case Tokens.Symbol:
                        // Classes
                        var classInstance = Classes.FirstOrDefault(x => x.Name == lexer.TokenContents);
                        if (classInstance != null)
                        {
                            return StepValue(lexer, classInstance);
                        }
                        // Variables
                        var variable = Variables.FirstOrDefault(p => p.Name == lexer.TokenContents);
                        if (variable != null)
                        {
                            //if (ScriptType.IsList(variable.Type))
                            //{
                            //    var index = StepValue<int>(lexer);
                            //    var list = ((List<ScriptVariable>)(variable.Value));
                            //    if (index >= list.Count())
                            //    {
                            //        throw new ScriptException(
                            //            message: String.Format("Property '{0}' contains {1} items (not {2}) on Line {3} Col {4}",
                            //                variable.Name,
                            //                list.Count(),
                            //                index + 1,
                            //                lexer.LineNumber,
                            //                lexer.Position),
                            //            row: lexer.LineNumber,
                            //            column: lexer.Position,
                            //            method: lexer.TokenContents
                            //        );
                            //    }
                            //    return list[index];
                            //}
                            //else
                            //{
                            current.Type = variable.Type;
                            current.Value = variable.Value;
                            continue;
                            //}
                        }
                        // Functions
                        var functionInstances = Methods.Where(x => x.Name == lexer.TokenContents).ToList();
                        if (functionInstances.Count() > 0)
                        {
                            current = StepMethod(lexer, functionInstances);
                            break;
                        }
                        lexer.Prev();
                        throw new ScriptException(
                            message: String.Format("Undefined '{0}' on Line {1} Col {2}",
                                        lexer.TokenContents,
                                        lexer.LineNumber,
                                        lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position,
                            method: lexer.TokenContents
                        );
                    case Tokens.Right:
                        // No pemdas support, fix this later on!
                        return current;
                    case Tokens.ArrayLeft:
                        if (current.Type == ScriptTypes.Undefined)
                        {
                            current = StepList(lexer);
                        }
                        else
                        {
                            if (ScriptType.IsList(current.Type))
                            {
                                var index = StepValue<int>(lexer);
                                var list = ((List<ScriptVariable>)(current.Value));
                                if (index >= list.Count())
                                {
                                    throw new ScriptException(
                                        message: String.Format("Property '{0}' contains {1} items (not {2}) on Line {3} Col {4}",
                                            current.Name,
                                            list.Count(),
                                            index + 1,
                                            lexer.LineNumber,
                                            lexer.Position),
                                        row: lexer.LineNumber,
                                        column: lexer.Position,
                                        method: lexer.TokenContents
                                    );
                                }
                                return list[index];
                            }
                            else
                            {

                             }
                        }
                        break;
                    case Tokens.String:
                        if (current.Type != ScriptTypes.Undefined)
                        {
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
                        break;
                    case Tokens.Integer:
                        current.Type = ScriptTypes.Integer;
                        current.Value = int.Parse(lexer.TokenContents);
                        break;
                    case Tokens.Double:
                        current.Type = ScriptTypes.Double;
                        current.Value = double.Parse(lexer.TokenContents);
                        break;
                    case Tokens.Boolean:
                        current.Type = ScriptTypes.Boolean;
                        current.Value = lexer.TokenContents == "true";
                        break;
                    case Tokens.Regex:
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
                        break;
                    case Tokens.Operator:
                        if (lexer.TokenContents == "and")
                        {
                            current.Value = current.Cast<bool>(lexer).Return<bool>() && StepValue<bool>(lexer);
                        }
                        else if (lexer.TokenContents == "or")
                        {
                            current.Value = current.Cast<bool>(lexer).Return<bool>() || StepValue<bool>(lexer);
                        }
                        break;
                    case Tokens.Arithmetic:
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
                        break;
                    default:
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
                if (lexer.Token == Tokens.Space) { }
                else if (lexer.Token == Tokens.ArrayRight)
                {
                    type = type == ScriptTypes.String ? ScriptTypes.ListString
                        : type == ScriptTypes.Integer ? ScriptTypes.ListInteger
                        : type == ScriptTypes.Double ? ScriptTypes.ListDouble
                        : type == ScriptTypes.Boolean ? ScriptTypes.ListBoolean
                        : ScriptTypes.Undefined;
                    return new ScriptVariable(items, type);
                }
                else if (lexer.Token == Tokens.Comma)
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
                        lexer.Prev();
                        throw new ScriptException(
                            message: String.Format("Invalid data type '{0}' in '{1}' list near Line {2} Col {3}",
                                type.ToString(),
                                variable.Type.ToString(),
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
            return StepMethod(lexer, functionInstances, new ScriptVariable());
        }

        private List<string> StepParameters(Lexer lexer)
        {
            lexer.Step("eventargs");
            var args = new List<string>();
            var usedComma = true;
            while (lexer.Next())
            {
                if (lexer.Token == Tokens.Space) { }
                else if (lexer.Token == Tokens.Comma)
                {
                    usedComma = true;
                }
                else if (lexer.Token == Tokens.Right)
                {
                    goto DoneArgs;
                }
                else if (lexer.Token == Tokens.Symbol)
                {
                    if (usedComma)
                    {
                        args.Add(lexer.TokenContents);
                        usedComma = false;
                    }
                    else
                    {
                        throw new ScriptException(
                            message: String.Format("Missing comma in argument list on Line {0} Col {1}",
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
                }
            }
            DoneArgs:
            return args;
        }

        private List<ScriptVariable> StepArguments(Lexer lexer)
        {
            var args = new List<ScriptVariable>();
            string MethodName = lexer.TokenContents;

            while (lexer.Next())
            {
                if (lexer.Token == Tokens.Left)
                {
                    var usedComma = true;
                    while (lexer.Next())
                    {
                        if (lexer.Token == Tokens.Space) { }
                        else if (lexer.Token == Tokens.Comma)
                        {
                            usedComma = true;
                        }
                        else if (lexer.Token == Tokens.Right)
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
                                if (lexer.Token == Tokens.Right)
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
                    switch (functionInstance.ReturnType)
                    {
                        case ScriptTypes.ListString:
                        case ScriptTypes.ListInteger:
                        case ScriptTypes.ListDouble:
                        case ScriptTypes.ListBoolean:
                            return new ScriptVariable(((List<string>)returnVariable).Select(x => new ScriptVariable((object)x, ScriptTypes.String)).ToList(), functionInstance.ReturnType);
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
                if (lexer.Token == Tokens.String)
                {
                    type = ScriptTypes.ListString;
                    array.Add(lexer.TokenContents.Substring(1, lexer.TokenContents.Length - 2));
                }
                else if (lexer.Token == Tokens.Double)
                {
                    type = ScriptTypes.ListDouble;
                    array.Add(double.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == Tokens.Integer)
                {
                    type = ScriptTypes.ListInteger;
                    array.Add(int.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == Tokens.Boolean)
                {
                    type = ScriptTypes.ListBoolean;
                    array.Add(lexer.TokenContents == "true");
                }
                else if (lexer.Token == Tokens.ArrayRight)
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

        private void TriggerEvent(Lexer lexer, List<ScriptVariable> arguments, ScriptClass classInstance)
        {
            while (lexer.Next())
            {
                if (lexer.Token == Tokens.EventName)
                {
                    lexer.Next();
                    if (lexer.Token == Tokens.Arguments)
                    {
                        var args = lexer.TokenContents.Substring(1, lexer.TokenContents.Length - 2).Replace(" ", "");

                        Debug.WriteLine("Args: {0}", lexer.TokenContents);
                        lexer.Next();
                    }
                    if (lexer.Token == Tokens.Block)
                    {
                        var script = new StringBuilder();
                        while (lexer.Next())
                        {
                            if (lexer.Token == Tokens.Block) { continue; }
                            if (lexer.Token == Tokens.Code)
                            {
                                script.AppendLine(lexer.TokenContents);
                            }
                            else
                            {
                                break;
                            }
                        }
                        Debug.WriteLine("Run: {0}", script.ToString());
                        Depths[0] = true;
                        Parse<object>(script.ToString(), 1);
                    }
                    else
                    {
                        throw new ScriptException(
                            message: String.Format("Invalid syntax Line {0} Col {1}",
                                lexer.LineNumber,
                                lexer.Position),
                            row: lexer.LineNumber,
                            column: lexer.Position
                        );
                    }
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
