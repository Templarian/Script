using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ScriptTest
{
    [TestClass]
    public class RegexBasic
    {
        [TestMethod]
        public void RegexParameter()
        {
            var script = new ScriptEngine();
            script.TypeFunction<string, Regex, List<string>>("match", (value, regex) =>
            {
                Match m = regex.Match(value);
                if (m.Success)
                {
                    return new List<string>() { m.Value };
                }
                else
                {
                    return new List<string>();
                }
            });
            var val = script.Evaluate<List<string>>("'footestfoo'.match(/test/)");
            Assert.AreEqual(1, val.Count);
            Assert.AreEqual("test", val[0]);
        }

        [TestMethod]
        public void RegexParameterIgnoreCase()
        {
            var script = new ScriptEngine();
            script.TypeFunction<string, Regex, List<string>>("match", (value, regex) =>
            {
                Match m = regex.Match(value);
                if (m.Success)
                {
                    return new List<string>() { m.Value };
                }
                else
                {
                    return new List<string>();
                }
            });
            var val = script.Evaluate<List<string>>("'fooTestfoo'.match(/test/i)");
            Assert.AreEqual(1, val.Count);
            Assert.AreEqual("Test", val[0]);
        }
    }
}
