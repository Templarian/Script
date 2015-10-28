using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Collections.Generic;
using System.Text;

namespace ScriptTest
{
    [TestClass]
    public class Debug
    {

        [TestMethod]
        public void DebugLogString()
        {
            var script = new ScriptEngine();
            var debugClass = script.AddClass("debug");
            debugClass.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            script.Execute("debug.log('Hello World!')");
        }

        [TestMethod]
        public void DebugLogInteger()
        {
            var script = new ScriptEngine();
            var debugClass = script.AddClass("debug");
            debugClass.AddAction<int>("log", message =>
            {
                Assert.AreEqual(42, message);
            });
            script.Execute("debug.log(42)");
        }

        [TestMethod]
        public void DebugLogDouble()
        {
            var script = new ScriptEngine();
            var debugClass = script.AddClass("debug");
            debugClass.AddAction<double>("log", message =>
            {
                Assert.AreEqual(4.2, message);
            });
            script.Execute("debug.log(4.2)");
        }

        [TestMethod]
        public void DebugLogBoolean()
        {
            var script = new ScriptEngine();
            var debugClass = script.AddClass("debug");
            debugClass.AddAction<bool>("log", message =>
            {
                Assert.AreEqual(true, message);
            });
            script.Execute("debug.log(true)");
        }

        [TestMethod]
        public void DebugLogArray()
        {
            var script = new ScriptEngine();
            var debugClass = script.AddClass("debug");
            debugClass.AddAction<List<string>>("log", message =>
            {
                Assert.AreEqual("Hello World!", message[0]);
            });
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            script.Execute("debug.log(['Hello World!'])");
        }

        [TestMethod]
        public void DebugLogSetList()
        {
            var script = new ScriptEngine();
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            var code = new StringBuilder();
            code.AppendLine("string[] foo = ['Hello', 'World!']");
            code.AppendLine("foo");
            var value = script.Evaluate<List<string>>(code.ToString());
            Assert.AreEqual(value[0], "Hello");
        }

        [TestMethod]
        public void DebugLogSetListNull()
        {
            var script = new ScriptEngine();
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            var code = new StringBuilder();
            code.AppendLine("string[] foo = []");
            code.AppendLine("foo");
            var value = script.Evaluate<List<string>>(code.ToString());
            Assert.AreEqual(value.Count, 0);
        }

        [TestMethod]
        public void ListStringAndRead()
        {
            var script = new ScriptEngine();
            script.Exception(e =>
            {
                Assert.Fail(e.Message);
            });
            var code = new StringBuilder();
            code.AppendLine("string[] foo = ['Hello', 'World!']");
            code.AppendLine("foo[0]");
            var value = script.Evaluate<string>(code.ToString());
            Assert.AreEqual(value, "Hello");
        }

        [TestMethod]
        public void ListStringNestedAndRead()
        {
            // Nested arrays are not supported.
            var script = new ScriptEngine();
            script.Exception(e =>
            {
                Assert.AreEqual("Invalid data type 'ListString' in 'String' list near Line 1 Col 36", e.Message);
            });
            var code = new StringBuilder();
            code.AppendLine("string[] foo = [['Hello', 'Again'], 'World!']");
            code.AppendLine("foo[0][0]");
            var value = script.Evaluate<string>(code.ToString());
            Assert.AreNotEqual(value, "Hello");
        }

    }
}
