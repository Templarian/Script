using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Script;
using System.Text;

namespace ScriptTest
{
    [TestClass]
    public class Property
    {
        [TestMethod]
        public void PropertyString()
        {
            var script = new ScriptEngine();
            var test = script.AddClass("Test");
            test.AddProperty("test", "Hello World!");
            Assert.AreEqual("Hello World!", script.Evaluate<string>("Test.test"));
        }

        [TestMethod]
        public void PropertyInteger()
        {
            var script = new ScriptEngine();
            var test = script.AddClass("Test");
            test.AddProperty("test", 10);
            Assert.AreEqual(10, script.Evaluate<int>("Test.test"));
        }

        [TestMethod]
        public void PropertyDouble()
        {
            var script = new ScriptEngine();
            var test = script.AddClass("Test");
            test.AddProperty("test", 10.0);
            Assert.AreEqual(10.0, script.Evaluate<double>("Test.test"));
        }

        [TestMethod]
        public void PropertyBoolTrue()
        {
            var script = new ScriptEngine();
            var test = script.AddClass("Test");
            test.AddProperty("test", true);
            Assert.AreEqual(true, script.Evaluate<bool>("Test.test"));
        }

        [TestMethod]
        public void PropertyBoolFalse()
        {
            var script = new ScriptEngine();
            var test = script.AddClass("Test");
            test.AddProperty("test", false);
            Assert.AreEqual(false, script.Evaluate<bool>("Test.test"));
        }
    }
}
