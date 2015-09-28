using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Text;

namespace ScriptTest
{
    [TestClass]
    public class Function
    {
        [TestMethod]
        public void FunctionString()
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
        public void FunctionVariableString()
        {
            var script = new ScriptEngine();
            script.AddFunction<string, string>("appendWorld", message =>
            {
                return message + " World!";
            });
            var code = new StringBuilder();
            code.AppendLine("string foo = 'Hello'");
            code.AppendLine("return appendWorld(foo)"); // Should run log, pass { indent: 1 }
            Assert.AreEqual("Hello World!", script.Evaluate<string>(code.ToString()));
        }
    }
}
