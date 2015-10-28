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
        public void VariableDouble()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("double foo = 2.0");
            code.AppendLine("foo");
            Assert.AreEqual(2.0, script.Evaluate<double>(code.ToString()));
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

        [TestMethod]
        public void VariableVarString()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("var foo = 'Hello World!'");
            code.AppendLine("foo");
            Assert.AreEqual("Hello World!", script.Evaluate<string>(code.ToString()));
        }

        [TestMethod]
        public void VariableVarInteger()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("var foo = 10");
            code.AppendLine("foo");
            Assert.AreEqual(10, script.Evaluate<int>(code.ToString()));
        }

        [TestMethod]
        public void VariableVarDouble()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("var foo = 2.0");
            code.AppendLine("foo");
            Assert.AreEqual(2.0, script.Evaluate<double>(code.ToString()));
        }

        [TestMethod]
        public void VariableVarBoolTrue()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("var foo = true");
            code.AppendLine("foo");
            Assert.AreEqual(true, script.Evaluate<bool>(code.ToString()));
        }

        [TestMethod]
        public void VariableVarBoolFalse()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("var foo = false");
            code.AppendLine("foo");
            Assert.AreEqual(false, script.Evaluate<bool>(code.ToString()));
        }

        [TestMethod]
        public void VariableStringToInt()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            script.Exception(error =>
            {
                Assert.AreEqual("String 'foo' set to Integer on Line 1 Col 13", error.Message);
            });
            code.AppendLine("string foo = 4");
            code.AppendLine("foo");
            script.Execute(code.ToString());
        }

    }
}
