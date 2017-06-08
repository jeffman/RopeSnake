using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace RopeSnake.Tests
{
    public static class TestHelpers
    {
        // https://stackoverflow.com/a/844855/1188632
        public static void AssertPublicInstancePropertiesEqual<T>(T expected, T actual, params string[] ignore) where T : class
        {
            if (expected != null && actual != null)
            {
                Type type = typeof(T);
                List<string> ignoreList = new List<string>(ignore);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (!ignoreList.Contains(pi.Name))
                    {
                        object expectedValue = type.GetProperty(pi.Name).GetValue(expected, null);
                        object actualValue = type.GetProperty(pi.Name).GetValue(actual, null);

                        if (expectedValue != actualValue && (expectedValue == null || !expectedValue.Equals(actualValue)))
                        {
                            Assert.Fail($"Property equality assertion failed. Name: {pi.Name}, expected = {expectedValue}, actual = {actualValue}");
                        }
                    }
                }
            }
            else if (expected != actual)
                Assert.Fail($"Instance equality assertion failed. Expected = {expected}, actual = {actual}");
        }

        public static void AssertPublicInstancePropertiesEqualDeep(object expected, object actual)
        {
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.AreEqual(expectedJson, actualJson);
        }
    }
}
