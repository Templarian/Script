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
    public class Execute
    {
        [TestMethod]
        public void ExecuteBasic()
        {
            var script = new ScriptEngine();
            script.AddAction<string>("log", message =>
            {
                Assert.AreEqual("Hello World!", message);
            });
            var code = new StringBuilder();
            code.AppendLine("log('Hello World!')"); // Should run log, pass { indent: 1 }
            script.Execute(code.ToString());
        }

    }
}
