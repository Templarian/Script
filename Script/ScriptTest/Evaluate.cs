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
    public class Evaluate
    {
        [TestMethod]
        public void EvaluateString()
        {
            var script = new ScriptEngine();
            script.AddFunction<string, string>("append", message =>
            {
                return message + " World!";
            });
            var code = new StringBuilder();
            code.AppendLine("return append('Hello')"); // Should run log, pass { indent: 1 }
            Assert.AreEqual("Hello World!", script.Evaluate<string>(code.ToString()));
        }

    }
}
