using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Text;

namespace ScriptTest
{
    [TestClass]
    public class Variable
    {
        [TestMethod]
        public void VariableString()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("string foo = 'Hello World!'");
            code.AppendLine("foo");
            Assert.AreEqual("Hello World!", script.Evaluate<string>(code.ToString()));
        }

        [TestMethod]
        public void VariableInteger()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("int foo = 10");
            code.AppendLine("foo");
            Assert.AreEqual(10, script.Evaluate<int>(code.ToString()));
        }

        [TestMethod]
        public void VariableBoolTrue()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("bool foo = true");
            code.AppendLine("foo");
            Assert.AreEqual(true, script.Evaluate<bool>(code.ToString()));
        }

        [TestMethod]
        public void VariableBoolFalse()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("bool foo = false");
            code.AppendLine("foo");
            Assert.AreEqual(false, script.Evaluate<bool>(code.ToString()));
        }
    }
}
