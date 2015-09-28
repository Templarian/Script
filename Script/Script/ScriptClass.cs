using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class ScriptClass
    {
        internal Action<ScriptError> Error;

        public string Name { get; set; }
        public Delegate Function { get; set; }

        public ScriptClass ()
        {
            
        }

        public ScriptClass (string name)
        {
            Name = name;
        }

        internal List<ScriptProperty> Property = new List<ScriptProperty>();

        public void SetProperty<T>(string name, object value)
        {
            ScriptTypes type = ScriptType.ToEnum(typeof(T));
            var property = Property.FirstOrDefault(p => p.Name == name);
            if (property == null)
            {
                Property.Add(new ScriptProperty
                {
                    Name = name,
                    Type = type,
                    Value = value
                });
            }
            else
            {
                if (property.Type == type)
                {
                    property.Value = value;
                }
                else
                {
                    Error.DynamicInvoke(new ScriptError
                    {
                        Message = String.Format("'{0}' requires a data type of {1}", name, type),
                        LineNumber = 0,
                        Position = 0,
                        MethodName = "undefined"
                    });
                }
            }
        }

        public void GetProperty(string name)
        {
            Property.RemoveAll(property => property.Name == name);
        }

        internal List<ScriptFunction> Functions = new List<ScriptFunction>();

        /// <summary>
        /// Add a user defined function to the script engine.
        /// </summary>
        /// <param name="name">Function name in the script. Example "dialog"</param>
        /// <param name="function">A Func definition.</param>
        public void AddFunction<TResult>(string name, Func<TResult> function)
        {
            ScriptTypes[] args = { };
            Functions.Add(new ScriptFunction(name, function, args));
        }

        public void AddFunction<T1, TResult>(string name, Func<T1, TResult> function)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            ScriptTypes[] args = { t1 };
            Functions.Add(new ScriptFunction(name, function, args));
        }

        public void AddFunction<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            ScriptTypes[] args = { t1, t2 };
            Functions.Add(new ScriptFunction(name, function, args));
        }

        /// <summary>
        /// Add an action with 0 arguments. Actions do not have a return type.
        /// </summary>
        /// <param name="name">The action's name in the script.</param>
        /// <param name="action">Executed when action name is found and argument types match.</param>
        public void AddAction(string name, Action action)
        {
            Functions.Add(new ScriptFunction(name, action));
        }


        public void AddAction<T1>(string name, Action<T1> action)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            ScriptTypes[] args = { t1 };
            Functions.Add(new ScriptFunction(name, action, args));
        }


        public void AddAction<T1, T2>(string name, Action<T1, T2> action)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            ScriptTypes[] args = { t1, t2 };
            Functions.Add(new ScriptFunction(name, action, args));
        }


        public void AddAction<T1, T2, T3>(string name, Action<T1, T2, T3> action)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            var t3 = ScriptType.ToEnum(typeof(T3));
            ScriptTypes[] args = { t1, t2, t3 };
            Functions.Add(new ScriptFunction(name, action, args));
        }

        internal List<ScriptCondition> Conditions = new List<ScriptCondition>();

        /// <summary>
        /// Add a user defined function to the script engine.
        /// </summary>
        /// <param name="name">Function name in the script. Example "dialog"</param>
        /// <param name="function">A Predicate definition.</param>
        public void AddCondition<T1>(string name, Func<T1, bool> condition)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            ScriptTypes[] args = { t1 };
            Conditions.Add(new ScriptCondition(name, condition, args));
        }

        public void AddCondition<T1, T2>(string name, Func<T1, T2, bool> condition)
        {
            var t1 = ScriptType.ToEnum(typeof(T1));
            var t2 = ScriptType.ToEnum(typeof(T2));
            ScriptTypes[] args = { t1, t2 };
            Conditions.Add(new ScriptCondition(name, condition, args));
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
