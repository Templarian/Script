﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptClass
    {
        
        public string Name { get; set; }
        public Delegate Function { get; set; }

        internal Action<ScriptException> Error;
        internal List<ScriptVariable> Variables = new List<ScriptVariable>();
        internal List<ScriptVariable> Properties = new List<ScriptVariable>();
        internal List<ScriptMethod> Methods = new List<ScriptMethod>();

        public ScriptClass()
        {

        }

        public ScriptClass(string name)
        {
            Name = name;
        }
        
        public void SetVariable(string name, ScriptVariable value)
        {
            var property = Variables.FirstOrDefault(p => p.Name == name);
            if (property == null)
            {
                value.Name = name;
                Variables.Add(value);
            }
            else
            {
                if (property.Type == value.Type)
                {
                    property.Value = value;
                }
                else
                {
                    throw new ScriptException(
                        message: String.Format("'{0}' requires a data type of {1}",
                            name,
                            value.Type),
                        row: 0,
                        column: 0
                    );
                }
            }
        }

        public ScriptVariable GetProperty(string name)
        {
            return Variables.Where(v => v.Name == name).FirstOrDefault();
        }

        public void DeleteProperty(string name)
        {
            Variables.RemoveAll(property => property.Name == name);
        }

        public void AddProperty(string name, string value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.String));
        }

        public void AddProperty(string name, int value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Integer));
        }

        public void AddProperty(string name, double value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Double));
        }

        public void AddProperty(string name, bool value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Double));
        }

        public void AddProperty(string name, Regex value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Double));
        }

        public void AddProperty(string name, List<string> value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Double));
        }

        public void AddProperty(string name, List<int> value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Double));
        }

        public void AddProperty(string name, List<double> value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Double));
        }

        public void AddProperty(string name, List<bool> value)
        {
            Properties.Add(new ScriptVariable(name, value, ScriptTypes.Double));
        }

        private void AddProperty(string name, object value)
        {
            Properties.Add(new ScriptVariable(name, value));
        }

        /// <summary>
        /// Add a user defined function to the script engine.
        /// </summary>
        /// <param name="name">Function name in the script. Example "dialog"</param>
        /// <param name="function">A Func definition.</param>
        public void AddFunction<TResult>(string name, Func<TResult> function)
        {
            ScriptTypes[] args = { };
            var tr = ScriptType.ToEnum(typeof(TResult));
            Methods.Add(new ScriptFunction(name, function, args, tr));
        }

        public void AddFunction<T1, TResult>(string name, Func<T1, TResult> function)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var tr = ScriptType.ToEnum(typeof(TResult));
            ScriptTypes[] args = { t1 };
            Methods.Add(new ScriptFunction(name, function, args, tr));
        }

        public void AddFunction<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            var tr = ScriptType.ToEnum(typeof(TResult));
            ScriptTypes[] args = { t1, t2 };
            Methods.Add(new ScriptFunction(name, function, args, tr));
        }

        /// <summary>
        /// Add an action with 0 arguments. Actions do not have a return type.
        /// </summary>
        /// <param name="name">The action's name in the script.</param>
        /// <param name="action">Executed when action name is found and argument types match.</param>
        public void AddAction(string name, Action action)
        {
            Methods.Add(new ScriptFunction(name, action));
        }


        public void AddAction<T1>(string name, Action<T1> action)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            ScriptTypes[] args = { t1 };
            Methods.Add(new ScriptFunction(name, action, args));
        }


        public void AddAction<T1, T2>(string name, Action<T1, T2> action)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            ScriptTypes[] args = { t1, t2 };
            Methods.Add(new ScriptFunction(name, action, args));
        }


        public void AddAction<T1, T2, T3>(string name, Action<T1, T2, T3> action)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            var t3 = ScriptType.ToEnum(typeof(T3));
            ScriptTypes[] args = { t1, t2, t3 };
            Methods.Add(new ScriptFunction(name, action, args));
        }

        /// <summary>
        /// Add a user defined function to the script engine.
        /// </summary>
        /// <param name="name">Function name in the script. Example "dialog"</param>
        /// <param name="function">A Predicate definition.</param>
        public void AddCondition<T1>(string name, Func<T1, bool> condition)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            ScriptTypes[] args = { t1 };
            Methods.Add(new ScriptCondition(name, condition, args));
        }

        public void AddCondition<T1, T2>(string name, Func<T1, T2, bool> condition)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            ScriptTypes[] args = { t1, t2 };
            Methods.Add(new ScriptCondition(name, condition, args));
        }

        internal List<ScriptClass> Classes = new List<ScriptClass>();

        public ScriptClass AddClass(string name)
        {
            var newClass = new ScriptClass(name);
            Classes.Add(newClass);
            return newClass;
        }

        public ScriptClass AddClass(string name, Delegate function)
        {
            var newClass = new ScriptClass(name);
            Classes.Add(newClass);
            return newClass;
        }
    }
}
