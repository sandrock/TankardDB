
namespace TankardDB.Core.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TankardDB.Core.Internals;

    [TestClass]
    public class SekvapLanguageTests
    {
        [TestClass]
        public class Ctor0
        {
            [TestMethod]
            public void Works()
            {
                new SekvapLanguage();
            }
        }

        [TestClass]
        public class IsMatchMethod
        {
            [TestMethod]
            public void PrefixOk_Value()
            {
                var lang = new SekvapLanguage();
                string input = SekvapLanguage.Prefix + "VaLuE";
                bool result = lang.IsMatch(input);
                Assert.IsTrue(result, "The input should match the language processor");
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullInput()
            {
                var lang = new SekvapLanguage();
                string input = null;
                lang.IsMatch(input);
            }

            [TestMethod]
            public void EmptyInput()
            {
                var lang = new SekvapLanguage();
                string input = string.Empty;
                bool result = lang.IsMatch(input);
                Assert.IsFalse(result, "The input should not match the language processor");
            }

            [TestMethod]
            public void PrefixOk_NoValue()
            {
                var lang = new SekvapLanguage();
                string input = SekvapLanguage.Prefix;
                bool result = lang.IsMatch(input);
                Assert.IsTrue(result, "The input should match the language processor");
            }

            [TestMethod]
            public void PrefixOk_ValueAndStuff()
            {
                var lang = new SekvapLanguage();
                string input = SekvapLanguage.Prefix + "VaLuE;Key=Val";
                bool result = lang.IsMatch(input);
                Assert.IsTrue(result, "The input should match the language processor");
            }
        }

        [TestClass]
        public class ParseMethod
        {
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullInput()
            {
                var lang = new SekvapLanguage();
                string input = null;
                lang.Parse(input);
            }

            [TestMethod]
            public void EmptyInput()
            {
                var lang = new SekvapLanguage();
                string input = string.Empty;
                var result = lang.Parse(input);
                Assert.IsNull(result);
            }

            [TestMethod]
            public void PrefixOk_NoValue()
            {
                var lang = new SekvapLanguage();
                string input = SekvapLanguage.Prefix;
                var result = lang.Parse(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("Value", result[0].Key);
                Assert.AreEqual(string.Empty, result[0].Value);
            }

            [TestMethod]
            public void PrefixOk_SimpleValue()
            {
                var lang = new SekvapLanguage();
                var parts = new string[]
                {
                    "hello world",
                };
                string input = SekvapLanguage.Prefix + parts[0];
                var result = lang.Parse(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("Value", result[0].Key);
                Assert.AreEqual(parts[0], result[0].Value);
            }

            [TestMethod]
            public void PrefixOk_SimpleValueWithEscapedSeparator()
            {
                var lang = new SekvapLanguage();
                var parts = new string[]
                {
                    "hello ;; world",
                };
                string input = SekvapLanguage.Prefix + parts[0];
                var result = lang.Parse(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("Value", result[0].Key);
                Assert.AreEqual(parts[0], result[0].Value);
            }

            [TestMethod]
            public void PrefixOk_SimpleValue_PlusOneKevap()
            {
                var lang = new SekvapLanguage();
                var parts = new string[]
                {
                    "hello world",
                    ";", "Name", "=", "John Smith",
                };
                string input = SekvapLanguage.Prefix + string.Join(string.Empty, parts);
                var result = lang.Parse(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                int i = -1;
                Assert.AreEqual("Value", result[++i].Key);
                Assert.AreEqual(parts[0], result[i].Value);
                Assert.AreEqual(parts[2], result[++i].Key);
                Assert.AreEqual(parts[4], result[i].Value);
            }

            [TestMethod]
            public void PrefixOk_SimpleValue_PlusTwoKevap()
            {
                var lang = new SekvapLanguage();
                var parts = new string[]
                {
                    "hello world",
                    ";", "Name", "=", "John Smith",
                    ";", "Foo", "=", "Bar",
                };
                string input = SekvapLanguage.Prefix + string.Join(string.Empty, parts);
                var result = lang.Parse(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.Count);
                int i = -1;
                Assert.AreEqual("Value", result[++i].Key);
                Assert.AreEqual(parts[0], result[i].Value);
                Assert.AreEqual(parts[2], result[++i].Key);
                Assert.AreEqual(parts[4], result[i].Value);
                Assert.AreEqual(parts[6], result[++i].Key);
                Assert.AreEqual(parts[8], result[i].Value);
            }
        }
    }
}
