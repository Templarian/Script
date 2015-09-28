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
            Parse(code);
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

        private bool DepthValidCondition ()
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

        public void Exception(Action<ScriptError> error)
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
                new TokenDefinition(@"([""'])(?:\\\1|.)*?\1", "STRING"),
                new TokenDefinition(@"[-+]?\d*\.\d+", "DOUBLE"),
                new TokenDefinition(@"[-+]?\d+", "INTEGER"),
                new TokenDefinition(@"(true|false)", "BOOLEAN"),
                new TokenDefinition(@"if\s?\(", "IF"),
                new TokenDefinition(@"else if\s?\(", "ELSEIF"),
                new TokenDefinition(@"else", "ELSE"),
                new TokenDefinition(@"(\+|-|\*|\/)", "ARITHMETIC"),
                new TokenDefinition(@"(int|double|string|bool)(\[\]|)", "TYPE"),
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
            return Return<T>(Step(lexer, this));
        }

        private T Return<T>(object value)
        {
            if (typeof(T).GenericTypeArguments.Count() == 0)
            {
                return (T)value;
            }
            else
            {
                if (typeof(T).GenericTypeArguments[0].Name == "String")
                {
                    return (T)(object)((List<object>)(value)).Select(x => (string)x).ToList();
                }
                else
                {
                    return default(T);
                }
            }
        }

        private void LexerException(int lineNumber, int position, string lineRemaining)
        {
            Error.DynamicInvoke(new ScriptError
            {
                Message = String.Format("Invalid tokens at Line {0} Col {1}",
                    lineNumber,
                    position),
                LineNumber = lineNumber,
                Position = position,
                MethodName = "undefined"
            });
        }

        private object Step(Lexer lexer, ScriptClass classScope)
        {
            object result = null;
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
                    // 2. Function
                    var functionInstances = classScope.Functions.Where(x => x.Name == lexer.TokenContents).ToList();
                    if (functionInstances.Count() > 0)
                    {
                        Debug.WriteLine("Step Function: {0}", lexer.TokenContents);
                        if ((Indent - indentOffset) > Depths.Length)
                        {
                            var functionInstance = functionInstances.First();
                            Error.DynamicInvoke(new ScriptError
                            {
                                Message = String.Format("Indented too far \"{0}\" Line {1} Col {2}",
                                    functionInstance.Name,
                                    lexer.LineNumber,
                                    lexer.Position),
                                LineNumber = lexer.LineNumber,
                                Position = lexer.Position,
                                MethodName = functionInstance.Name
                            });
                        }
                        else if (Indent == 0 || Depths[Indent - 1 - indentOffset])
                        {
                            result = StepFunction(lexer, functionInstances); // Not capturing just calling
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
                    var type = ScriptTypes.Null;
                    lexer.Prev();
                    return StepValue(lexer, out type);
                    
                    Error.DynamicInvoke(new ScriptError
                    {
                        Message = String.Format("Invalid class or function or property \"{0}\" Line {1} Col {2}",
                            lexer.TokenContents,
                            lexer.LineNumber,
                            lexer.Position),
                        LineNumber = lexer.LineNumber,
                        Position = lexer.Position,
                        MethodName = lexer.TokenContents
                    });
                }
                else if (lexer.Token == "TYPE")
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
                        default:
                            Error.DynamicInvoke(new ScriptError
                            {
                                Message = String.Format("Invalid type \"{0}\" Line {1} Col {2}",
                                    lexer.TokenContents,
                                    lexer.LineNumber,
                                    lexer.Position),
                                LineNumber = lexer.LineNumber,
                                Position = lexer.Position,
                                MethodName = lexer.TokenContents
                            });
                            break;
                    }
                }
                else if (lexer.Token == "IF")
                {
                    var check = DepthCondition(StepCondition(lexer, classScope));
                    if (check == null)
                    {
                        Debug.WriteLine("If: ERROR");
                        return null;
                    }
                    Debug.WriteLine("If: {0}", Depths[Indent]);
                }
                else if (lexer.Token == "ELSEIF")
                {
                    if (DepthValidCondition()) // Previous has to be false
                    {
                        var check = DepthCondition(StepCondition(lexer, classScope));
                        if (check == null)
                        {
                            Debug.WriteLine("Else If: ERROR");
                            return null;
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
                else if (lexer.Token == "INTEGER")
                {
                    var type = ScriptTypes.Null;
                    lexer.Prev();
                    return StepValue(lexer, out type);
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
        }

        private void StepProperty<T>(Lexer lexer, ScriptClass classScope)
        {
            var name = "";
            var type = ScriptTypes.Null;
            while (lexer.Next())
            {
                if (lexer.Token == "SPACE") { }
                else if (lexer.Token == "SYMBOL")
                {
                    name = lexer.TokenContents;
                }
                else if (name != "" && lexer.Token == "ASSIGNMENT")
                {
                    SetProperty<T>(name, StepValue(lexer, out type));
                    return;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Step for complex value
        /// </summary>
        /// <param name="lexer">Lexer</param>
        /// <returns>Valid value or null</returns>
        private object StepValue(Lexer lexer, out ScriptTypes type)
        {
            type = ScriptTypes.Null;
            object value = null;
            while (lexer.Next())
            {
                if (lexer.Token == "SPACE") { }
                else if (lexer.Token == "BLOCK")
                {
                    return value;
                }
                else if (lexer.Token == "SYMBOL")
                {
                    var matches = Regex.Matches(lexer.TokenContents, @"^([^\[\]]+?)(\[(\d+)\])?$");
                    if (matches[0].Groups[3].Success) // Is as List
                    {
                        var property = Property.FirstOrDefault(p => p.Name == matches[0].Groups[1].Value);
                        if (property != null)
                        {
                            var index = int.Parse(matches[0].Groups[3].Value);
                            var list = ((List<object>)(property.Value));
                            if (index >= list.Count())
                            {
                                Error.DynamicInvoke(new ScriptError
                                {
                                    Message = String.Format("Property '{0}' contains {1} items (not {2}) on Line {3} Col {4}",
                                        property.Name,
                                        list.Count(),
                                        index + 1,
                                        lexer.LineNumber,
                                        lexer.Position),
                                    LineNumber = lexer.LineNumber,
                                    Position = lexer.Position,
                                    MethodName = lexer.TokenContents
                                });
                            }
                            switch (property.Type)
                            {
                                case ScriptTypes.ListString:
                                    return list.Select(i => i.ToString()).ToList()[index];
                                case ScriptTypes.ListInteger:
                                    return ((List<int>)property.Value)[index];
                                case ScriptTypes.ListDouble:
                                    return ((List<double>)property.Value)[index];
                                case ScriptTypes.ListBoolean:
                                    return ((List<bool>)property.Value)[index];
                                default:
                                    // error
                                    break;
                            }
                            return null;
                        }
                    }
                    else
                    {
                        var property = Property.FirstOrDefault(p => p.Name == matches[0].Groups[1].Value);
                        if (property != null)
                        {
                            type = property.Type;
                            value = property.Value;
                        }
                    }
                }
                else if (lexer.Token == "RIGHT")
                {
                    // No pemdas support, fix this later on!
                    return value;
                }
                else if (lexer.Token == "ARRAYLEFT")
                {
                    type = ScriptTypes.Null;
                    value = StepList(lexer, out type);
                }
                else if (lexer.Token == "STRING")
                {
                    type = ScriptTypes.String;
                    value = lexer.TokenContents.Substring(1, lexer.TokenContents.Length - 2);
                }
                else if (lexer.Token == "INTEGER")
                {
                    type = ScriptTypes.Integer;
                    value = int.Parse(lexer.TokenContents);
                }
                else if (lexer.Token == "DOUBLE")
                {
                    type = ScriptTypes.Double;
                    value = double.Parse(lexer.TokenContents);
                }
                else if (lexer.Token == "BOOLEAN")
                {
                    type = ScriptTypes.Boolean;
                    value = lexer.TokenContents == "true";
                }
                else if (lexer.Token == "ARITHMETIC")
                {
                    switch (lexer.TokenContents)
                    {
                        case "+":
                            switch(type)
                            {
                                case ScriptTypes.String:

                                    break;
                                case ScriptTypes.Integer:
                                    var nextValue = StepValue(lexer, out type);
                                    switch(type)
                                    {
                                        case ScriptTypes.String:
                                            type = ScriptTypes.String;
                                            value = value.ToString() + nextValue.ToString();
                                            break;
                                        case ScriptTypes.Integer:
                                            type = ScriptTypes.Integer;
                                            value = (int)value + (int)nextValue;
                                            break;
                                        case ScriptTypes.Double:
                                            type = ScriptTypes.Double;
                                            value = (int)value + (double)nextValue;
                                            break;
                                        case ScriptTypes.Boolean:
                                            type = ScriptTypes.String;
                                            value = value.ToString() + ((bool)nextValue ? "true" : "false");
                                            break;
                                        case ScriptTypes.ListString:
                                            type = ScriptTypes.String;
                                            var listString = ((List<object>)nextValue).Select(s => s.ToString()).ToList();
                                            value = value.ToString() + string.Join(", ", listString);
                                            break;
                                        case ScriptTypes.ListInteger:
                                            type = ScriptTypes.String;
                                            var listInteger = (List<int>)(nextValue);
                                            value = value.ToString() + string.Join(", ", listInteger);
                                            break;
                                        case ScriptTypes.ListDouble:
                                            type = ScriptTypes.String;
                                            var listDouble = (List<double>)(nextValue);
                                            value = value.ToString() + string.Join(", ", listDouble);
                                            break;
                                        case ScriptTypes.ListBoolean:
                                            type = ScriptTypes.String;
                                            var listBoolean = (List<bool>)(nextValue);
                                            value = value.ToString() + string.Join(", ", listBoolean.Select(b => b ? "true" : "false"));
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "-":
                            if (type == ScriptTypes.Integer)
                            {
                                var nextValue = StepValue(lexer, out type);
                                if (type == ScriptTypes.String)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'-\' with data types {0} and String Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                                else if (type == ScriptTypes.Integer)
                                {
                                    type = ScriptTypes.Integer;
                                    value = (int)value - (int)nextValue;
                                }
                                else if (type == ScriptTypes.Double)
                                {
                                    type = ScriptTypes.Double;
                                    value = (int)value - (double)nextValue;
                                }
                                else if (type == ScriptTypes.Boolean)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'-\' with data types {0} and Boolean Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                                else if (type == ScriptTypes.ListString)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'-\' with data types {0} and ListString Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                            }
                            break;
                        case "*":
                            if (type == ScriptTypes.Integer)
                            {
                                var nextValue = StepValue(lexer, out type);
                                if (type == ScriptTypes.String)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'*\' with data types {0} and String Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                                else if (type == ScriptTypes.Integer)
                                {
                                    type = ScriptTypes.Integer;
                                    value = (int)value * (int)nextValue;
                                }
                                else if (type == ScriptTypes.Double)
                                {
                                    type = ScriptTypes.Double;
                                    value = (int)value * (double)nextValue;
                                }
                                else if (type == ScriptTypes.Boolean)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'*\' with data types {0} and Boolean Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                                else if (type == ScriptTypes.ListString)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'*\' with data types {0} and ListString Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                            }
                            break;
                        case "/":
                            if (type == ScriptTypes.Integer)
                            {
                                var nextValue = StepValue(lexer, out type);
                                if (type == ScriptTypes.String)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'/\' with data types {0} and String Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                                else if (type == ScriptTypes.Integer)
                                {
                                    type = ScriptTypes.Integer;
                                    value = (int)value / (int)nextValue;
                                }
                                else if (type == ScriptTypes.Double)
                                {
                                    type = ScriptTypes.Double;
                                    value = (int)value / (double)nextValue;
                                }
                                else if (type == ScriptTypes.Boolean)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'/\' with data types {0} and Boolean Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                                else if (type == ScriptTypes.ListString)
                                {
                                    Error.DynamicInvoke(new ScriptError
                                    {
                                        Message = String.Format("Invalid operator \'/\' with data types {0} and ListString Line {1} Col {2}",
                                            type.ToString(),
                                            lexer.LineNumber,
                                            lexer.Position),
                                        LineNumber = lexer.LineNumber,
                                        Position = lexer.Position,
                                        MethodName = lexer.TokenContents
                                    });
                                }
                            }
                            break;
                    }
                }
            }
            // Syntax error thrown here
            return value;
        }
        
        private object StepList(Lexer lexer, out ScriptTypes type)
        {
            type = ScriptTypes.Null;
            var items = new List<object>();
            while (lexer.Next())
            {
                if (lexer.Token == "SPACE") { }
                else if (lexer.Token == "ARRAYRIGHT")
                {
                    return items;
                }
                else if (lexer.Token == "STRING")
                {
                    if (!(type == ScriptTypes.Null || type == ScriptTypes.ListString))
                    {
                        Error.DynamicInvoke(new ScriptError
                        {
                            Message = String.Format("Invalid STRING found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            LineNumber = lexer.LineNumber,
                            Position = lexer.Position,
                            MethodName = "undefined"
                        });
                        return items;
                    }
                    items.Add(lexer.TokenContents.Substring(1, lexer.TokenContents.Length - 2));
                    type = ScriptTypes.ListString;
                }
                else if (lexer.Token == "DOUBLE")
                {
                    if (!(type == ScriptTypes.Null || type != ScriptTypes.ListDouble))
                    {
                        Error.DynamicInvoke(new ScriptError
                        {
                            Message = String.Format("Invalid DOUBLE found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            LineNumber = lexer.LineNumber,
                            Position = lexer.Position,
                            MethodName = "undefined"
                        });
                        return items;
                    }
                    items.Add(double.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == "INTEGER")
                {
                    if (!(type == ScriptTypes.Null || type != ScriptTypes.ListInteger))
                    {
                        Error.DynamicInvoke(new ScriptError
                        {
                            Message = String.Format("Invalid INTEGER found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            LineNumber = lexer.LineNumber,
                            Position = lexer.Position,
                            MethodName = "undefined"
                        });
                        return items;
                    }
                    items.Add(int.Parse(lexer.TokenContents));
                }
                else if (lexer.Token == "BOOLEAN")
                {
                    if (!(type == ScriptTypes.Null || type != ScriptTypes.ListBoolean))
                    {
                        Error.DynamicInvoke(new ScriptError
                        {
                            Message = String.Format("Invalid BOOLEAN found in {0} at Line {1} Col {2}",
                                type.ToString(),
                                lexer.LineNumber,
                                lexer.Position),
                            LineNumber = lexer.LineNumber,
                            Position = lexer.Position,
                            MethodName = "undefined"
                        });
                        return items;
                    }
                    items.Add(lexer.TokenContents == "true");
                }
            }
            Error.DynamicInvoke(new ScriptError
            {
                Message = String.Format("Missing \"]\" near Line {0} Col {1}",
                    lexer.LineNumber,
                    lexer.Position),
                LineNumber = lexer.LineNumber,
                Position = lexer.Position,
                MethodName = "undefined"
            });
            return items;
        }
        /// <summary>
        /// THIS NEEDS AN ENTIRE REWRITE TO USE THE UNIVERSAL STEP VALUE METHOD!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="functionInstances"></param>
        /// <returns></returns>
        private object StepFunction(Lexer lexer, List<ScriptFunction> functionInstances)
        {
            var openParentheses = false;
            var type = ScriptTypes.Null;
            List<KeyValuePair<ScriptTypes, object>> args = new List<KeyValuePair<ScriptTypes, object>>();
            string MethodName = lexer.TokenContents;
            while (lexer.Next())
            {
                if (lexer.Token == "LEFT")
                {
                    openParentheses = true;
                    do
                    {
                        if (lexer.Token == "COMMA")
                        {

                        }
                        else
                        {
                            var value = StepValue(lexer, out type);
                            args.Add(new KeyValuePair<ScriptTypes, object>(type, value));
                        }
                        if (lexer.Token == "RIGHT")
                        {
                            break;
                        }
                    } while (lexer.Next());
                    if (lexer.Token == "RIGHT")
                    {
                        break;
                    }
                }
                else if (lexer.Token == "RIGHT")
                {
                    return null;
                }
            }
            foreach (var functionInstance in functionInstances)
            {
                if (args.Select(pair => pair.Key).SequenceEqual(functionInstance.Types))
                {
                    return functionInstance.Function.DynamicInvoke(args.Select(pair => {
                        if (pair.Key == ScriptTypes.ListString)
                        {
                            return this.Return<List<string>>(pair.Value);
                        }
                        return pair.Value;
                    }).ToArray());
                }
            }
            Error.DynamicInvoke(new ScriptError
            {
                Message = String.Format("Invalid arguments in \"{0}\" Line {1} Col {2}",
                    MethodName,
                    lexer.LineNumber,
                    lexer.Position),
                LineNumber = lexer.LineNumber,
                Position = lexer.Position,
                MethodName = MethodName
            });
            return null;
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
            Error.DynamicInvoke(new ScriptError
            {
                Message = String.Format("Invalid list syntax Line {0} Col {1}",
                    lexer.LineNumber,
                    lexer.Position),
                LineNumber = lexer.LineNumber,
                Position = lexer.Position,
                MethodName = "array"
            });
            list = null;
            type = ScriptTypes.Null;
        }

        private Nullable<bool> StepCondition(Lexer lexer, ScriptClass classInstance)
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
        }

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
    }
}
