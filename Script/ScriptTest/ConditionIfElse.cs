using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptTest
{
    [TestClass]
    public class ConditionIfElse
    {
        [TestMethod]
        public void ConditionIfElseBoolTrue()
        {
            var script = new ScriptEngine();
            script.AddCondition<string>("foo", message =>
            {
                return "bar" == message;
            });
            script.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            var code = new StringBuilder();
            code.AppendLine("if (foo('bar'))"); // Basic if statment
            code.AppendLine("    log('Hello World!')");
            code.AppendLine("else if (foo('blarg'))");
            code.AppendLine("    log('Goodbye World!')");
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionIfElseBoolFalse()
        {
            var script = new ScriptEngine();
            script.AddCondition<string>("foo", message =>
            {
                return "bar" == message;
            });
            script.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            var code = new StringBuilder();
            code.AppendLine("if (foo('blarg'))"); // Basic if statment
            code.AppendLine("    log('Goodbye World!')");
            code.AppendLine("else if (foo('bar'))");
            code.AppendLine("    log('Hello World!')");
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionIfElseElseBoolFalse()
        {
            var script = new ScriptEngine();
            script.AddCondition<string>("foo", message =>
            {
                return "bar" == message;
            });
            script.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            var code = new StringBuilder();
            code.AppendLine("if (foo('blarg'))"); // Basic if statment
            code.AppendLine("    log('Goodbye World!')");
            code.AppendLine("else if (foo('blarg'))");
            code.AppendLine("    log('Goodbye World!')");
            code.AppendLine("else");
            code.AppendLine("    log('Hello World!')");
            script.Execute(code.ToString());
        }
    }
}
