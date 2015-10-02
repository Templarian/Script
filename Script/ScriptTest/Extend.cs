using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Text;
using System.Collections.Generic;

namespace ScriptTest
{
    [TestClass]
    public class Extend
    {
        [TestMethod]
        public void TypeFunctionStringArrayJoin()
        {
            var script = new ScriptEngine();
            script.TypeFunction<List<string>, string, string>("join", (list, separator) =>
            {
                return string.Join(separator, list.ToArray());
            });
            var code = new StringBuilder();
            code.AppendLine("string[] foo = ['foo', 'bar']");
            code.AppendLine("foo.join('-')");
            Assert.AreEqual("foo-bar", script.Evaluate<string>(code.ToString()));
        }

        [TestMethod]
        public void TypeFunctionStringArrayJoinOverload()
        {
            var script = new ScriptEngine();
            script.TypeFunction<List<string>, string>("join", (list) =>
            {
                return string.Join(",", list.ToArray());
            });
            script.TypeFunction<List<string>, string, string>("join", (list, separator) =>
            {
                return string.Join(separator, list.ToArray());
            });
            var code = new StringBuilder();
            code.AppendLine("string[] foo = ['foo', 'bar']");
            code.AppendLine("foo.join('-')");
            Assert.AreEqual("foo-bar", script.Evaluate<string>(code.ToString()));
        }

    }
}
