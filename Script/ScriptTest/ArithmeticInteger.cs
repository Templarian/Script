using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Collections.Generic;
using System.Text;

namespace ScriptTest
{
    [TestClass]
    public class ArithmeticInteger
    {

        [TestMethod]
        public void ArithmeticIntegerAddString()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", value =>
            {
                Assert.AreEqual("40text", value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 + 'text')");
        }

        [TestMethod]
        public void ArithmeticIntegerMinusString()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", value =>
            {
                
            });
            script.Exception(e =>
            {
                Assert.AreEqual("Invalid operator '-' with data types String and String Line 2 Col -1", e.Message);
            });
            script.Execute("log(40 - 'text')");
        }

        [TestMethod]
        public void ArithmeticIntegerMultiplyString()
        {
            var script = new ScriptEngine();
            script.Exception(e =>
            {
                Assert.AreEqual("Invalid operator '*' with data types String and String Line 2 Col -1", e.Message);
            });
            script.Evaluate<string>("40 * 'text'");
        }

        [TestMethod]
        public void ArithmeticIntegerDivideStringString()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", value =>
            {
                
            });
            script.Exception(e =>
            {
                Assert.AreEqual("Invalid operator '/' with data types String and String Line 2 Col -1", e.Message);
            });
            script.Execute("log(40 / 'text')");
        }

        [TestMethod]
        public void ArithmeticIntegerAddInteger()
        {
            var script = new ScriptEngine();
            script.AddAction<int>("log", value =>
            {
                Assert.AreEqual(42, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 + 2)");
        }

        [TestMethod]
        public void ArithmeticIntegerMinusInteger()
        {
            var script = new ScriptEngine();
            script.AddAction<int>("log", value =>
            {
                Assert.AreEqual(38, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 - 2)");
        }

        [TestMethod]
        public void ArithmeticIntegerMultiplyInteger()
        {
            var script = new ScriptEngine();
            script.AddAction<int>("log", value =>
            {
                Assert.AreEqual(80, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 * 2)");
        }

        [TestMethod]
        public void ArithmeticIntegerDivideInterger()
        {
            var script = new ScriptEngine();
            script.AddAction<int>("log", value =>
            {
                Assert.AreEqual(20, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 / 2)");
        }

        [TestMethod]
        public void ArithmeticIntegerAddDouble()
        {
            var script = new ScriptEngine();
            script.AddAction<double>("log", value =>
            {
                Assert.AreEqual(42, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 + 2.0)");
        }

        [TestMethod]
        public void ArithmeticIntegerMinusDouble()
        {
            var script = new ScriptEngine();
            script.AddAction<double>("log", value =>
            {
                Assert.AreEqual(38, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 - 2.0)");
        }

        [TestMethod]
        public void ArithmeticIntegerMultiplyDouble()
        {
            var script = new ScriptEngine();
            script.AddAction<double>("log", value =>
            {
                Assert.AreEqual(80, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 * 2.0)");
        }

        [TestMethod]
        public void ArithmeticIntegerDivideDouble()
        {
            var script = new ScriptEngine();
            script.AddAction<double>("log", value =>
            {
                Assert.AreEqual(20, value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 / 2.0)");
        }

        [TestMethod]
        public void ArithmeticIntegerAddListString()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", value =>
            {
                Assert.AreEqual("40hello, world, !", value);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("log(40 + ['hello', 'world', '!'])");
        }

        [TestMethod]
        public void ArithmeticVariableIntegerAddInteger()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("int foo = 40");
            code.AppendLine("foo + 2");
            Assert.AreEqual(42, script.Evaluate<int>(code.ToString()));
        }

        [TestMethod]
        public void ArithmeticVariableIntegerAddString()
        {
            var script = new ScriptEngine();
            var code = new StringBuilder();
            code.AppendLine("int foo = 40");
            code.AppendLine("foo + '2'");
            Assert.AreEqual("402", script.Evaluate<string>(code.ToString()));
        }
    }
}
