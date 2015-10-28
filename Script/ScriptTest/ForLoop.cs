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
    public class ForLoop
    {

        [TestMethod]
        public void ConditionBoolTrue()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", message =>
            {
                Assert.IsTrue(message == "test1" || message == "test2");
            });
            var code = new StringBuilder();
            code.AppendLine("for (var i in ['test1', 'test2'])"); // Basic if statment
            code.AppendLine("    log(i)"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

    }
}
