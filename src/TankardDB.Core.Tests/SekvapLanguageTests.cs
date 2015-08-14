
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
                string input = "VaLuE";
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
                string input = string.Empty;
                bool result = lang.IsMatch(input);
                Assert.IsTrue(result, "The input should match the language processor");
            }

            [TestMethod]
            public void PrefixOk_ValueAndStuff()
            {
                var lang = new SekvapLanguage();
                string input = "VaLuE;Key=Val";
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
                string input = string.Empty;
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
                string input = parts[0];
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
                string input = parts[0];
                var result = lang.Parse(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("Value", result[0].Key);
                Assert.AreEqual("hello ; world", result[0].Value);
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
                string input = string.Join(string.Empty, parts);
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
                string input = string.Join(string.Empty, parts);
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

            [TestMethod]
            public void OneKeyWithEscape()
            {
                var target = new SekvapLanguage();
                string key1 = "Key1";
                string value1 = "Super;Sheep";
                string expected = ";" + key1 + "=Super;;Sheep";
                var parts = new string[]
                {
                    ";", "Key1", "=", "Super;;Sheep",
                };
                string input = string.Join(string.Empty, parts);
                var result = target.Parse(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                int i = -1;
                Assert.AreEqual("Value", result[++i].Key);
                Assert.AreEqual(string.Empty, result[i].Value);
                Assert.AreEqual("Key1", result[++i].Key);
                Assert.AreEqual("Super;Sheep", result[i].Value);
            }
        }

        [TestClass]
        public class WriteMethod
        {
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void Arg0IsNull()
            {
                var target = new SekvapLanguage();
                target.Write(null);
            }

            [TestMethod]
            public void ValueOnly()
            {
                var target = new SekvapLanguage();
                string input = "Hello World";
                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>("Value", input));
                var result = target.Write(data);
                Assert.AreEqual(input, result);
            }

            [TestMethod]
            public void ValueAndOneKey()
            {
                var target = new SekvapLanguage();
                string input = "Hello World";
                string key1 = "Key1";
                string value1 = "Super Sheep";
                string expected = input + ";" + key1 + "=" + value1;
                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>("Value", input));
                data.Add(new KeyValuePair<string, string>(key1, value1));
                var result = target.Write(data);
                Assert.AreEqual(expected, result);
            }

            [TestMethod]
            public void OneKey()
            {
                var target = new SekvapLanguage();
                string key1 = "Key1";
                string value1 = "Super Sheep";
                string expected = ";" + key1 + "=" + value1;
                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>(key1, value1));
                var result = target.Write(data);
                Assert.AreEqual(expected, result);
            }

            [TestMethod]
            public void OneKeyWithEscape()
            {
                var target = new SekvapLanguage();
                string key1 = "Key1";
                string value1 = "Super;Sheep";
                string expected = ";" + key1 + "=Super;;Sheep";
                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>(key1, value1));
                var result = target.Write(data);
                Assert.AreEqual(expected, result);
            }

            [TestMethod]
            public void ValueOnlyWithEscape()
            {
                var target = new SekvapLanguage();
                string input = "Hello;World";
                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>("Value", input));
                var result = target.Write(data);
                Assert.AreEqual("Hello;;World", result);
            }
        }
    }
}
