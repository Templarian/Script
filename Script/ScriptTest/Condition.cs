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
    public class Condition
    {

        [TestMethod]
        public void ConditionBoolTrue()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            var code = new StringBuilder();
            code.AppendLine("if (true)"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionBoolFalse()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", message =>
            {
                Assert.Fail("This should never be called.");
            });
            var code = new StringBuilder();
            code.AppendLine("if (false)"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }
        
        [TestMethod]
        public void ConditionFunctionBoolTrue()
        {
            var script = new ScriptEngine();
            script.AddCondition<string>("foo", message => {
                return "bar" == message;
            });
            script.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            var code = new StringBuilder();
            code.AppendLine("if (foo('bar'))"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionFunctionBoolFalse()
        {
            var script = new ScriptEngine();
            script.AddCondition<string>("foo", message =>
            {
                return "bar" == message;
            });
            script.AddAction<string>("log", message =>
            {
                Assert.Fail("This should never be called.");
            });
            var code = new StringBuilder();
            code.AppendLine("if (foo('blarg'))"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionBoolTrueAndTrue()
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
            code.AppendLine("if (foo('bar') and foo('bar'))"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionBoolTrueAndFalse()
        {
            var script = new ScriptEngine();
            script.AddCondition<string>("foo", message =>
            {
                return "bar" == message;
            });
            script.AddAction<string>("log", message =>
            {
                Assert.Fail("This should never be called.");
            });
            var code = new StringBuilder();
            code.AppendLine("if (foo('bar') and foo('blarg'))"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionBoolTrueOrFalse()
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
            code.AppendLine("if (foo('bar') or foo('blarg'))"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

        [TestMethod]
        public void ConditionBoolFalseOrFalse()
        {
            var script = new ScriptEngine();
            script.Exception(ExceptionHandler);
            script.AddCondition<string>("foo", message =>
            {
                return "bar" == message;
            });
            script.AddAction<string>("log", message =>
            {
                Assert.Fail("This should never be called.");
            });
            var code = new StringBuilder();
            code.AppendLine("if (foo('fail1') and foo('fail2'))"); // Basic if statment
            code.AppendLine("    log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

        private void ExceptionHandler(ScriptException error)
        {
            Assert.Fail();
        }
    }
}
