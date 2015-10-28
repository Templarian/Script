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
    public class Trigger
    {
        [TestMethod]
        public void TriggerBasic()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log1", message =>
            {
                Assert.AreEqual("Hello World 1!", message);
            });
            script.AddAction<string>("log2", message =>
            {
                Assert.AreEqual("Hello World 2!", message);
            });
            script.AddAction<string>("log", message =>
            {
                Assert.Fail();
            });
            var code = new StringBuilder();
            code.AppendLine("event action");
            code.AppendLine("    log1('Hello World 1!')");
            code.AppendLine("    log2('Hello World 2!')");
            code.AppendLine("event foo");
            code.AppendLine("    log('Goodbye World!')");
            script.Process(code.ToString());
            script.Trigger("action");
        }

        [TestMethod]
        public void TriggerFunction()
        {
            var script = new ScriptEngine();
            script.AddAction<int>("log1", message =>
            {
                Assert.AreEqual(10, message);
            });
            script.AddAction<string>("log2", message =>
            {
                Assert.Fail();
            });
            var code = new StringBuilder();
            code.AppendLine("event drop(item)");
            code.AppendLine("    log1(item)");
            code.AppendLine("event foo");
            code.AppendLine("    log2('Goodbye World!')");
            script.Process(code.ToString());
            script.Trigger("drop", 10);
        }

        [TestMethod]
        public void TriggerUndefined()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log1", message =>
            {
                Assert.AreEqual("Hello World 1!", message);
            });
            script.AddAction<string>("log2", message =>
            {
                Assert.AreEqual("Hello World 2!", message);
            });
            script.AddAction<string>("log", message =>
            {
                Assert.Fail();
            });
            var code = new StringBuilder();
            code.AppendLine("event action");
            code.AppendLine("    log1('Hello World 1!')");
            code.AppendLine("    log2('Hello World 2!')");
            code.AppendLine("event foo");
            code.AppendLine("    log('Goodbye World!')");
            script.Process(code.ToString());
            script.Trigger("test");
        }

    }
}
