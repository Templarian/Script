using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Text;

namespace ScriptTest
{
    [TestClass]
    public class Cast
    {
        [TestMethod]
        public void CastStringToString()
        {
            var script = new ScriptEngine();
            Assert.AreEqual("Test", script.Evaluate<string>("string('Test')"));
        }

        [TestMethod]
        public void CastIntToString()
        {
            var script = new ScriptEngine();
            Assert.AreEqual("4", script.Evaluate<string>("string(4)"));
            Assert.AreEqual("42", script.Evaluate<string>("string(42)"));
        }

        [TestMethod]
        public void CastDoubleToString()
        {
            var script = new ScriptEngine();
            Assert.AreEqual("4", script.Evaluate<string>("string(4.0)"));
            Assert.AreEqual("4.2", script.Evaluate<string>("string(4.2)"));
            Assert.AreEqual("4.2", script.Evaluate<string>("string(4.20)"));
        }

        [TestMethod]
        public void CastBoolToString()
        {
            var script = new ScriptEngine();
            Assert.AreEqual("true", script.Evaluate<string>("string(true)"));
            Assert.AreEqual("false", script.Evaluate<string>("string(false)"));
        }

        [TestMethod]
        public void CastNullToString()
        {
            var script = new ScriptEngine();
            Assert.AreEqual("null", script.Evaluate<string>("string(null)"));
        }
    }
}
