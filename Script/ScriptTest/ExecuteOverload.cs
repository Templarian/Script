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
    public class ExecuteOverloads
    {
        [TestMethod]
        public void ExecuteOverload()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            script.AddAction<string, string>("log", (message1, message2) =>
            {
                Assert.AreEqual("Hello World!", message1 + " " + message2);
            });
            var code = new StringBuilder();
            code.AppendLine("log('Hello World!')"); // Should run log, pass { indent: 1 }
            code.AppendLine("log('Hello', 'World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

    }
}
