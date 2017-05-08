using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using RopeSnake.Core;
using System.Diagnostics;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void EnumerateRecursive()
        {
            (var expected, var list) = CreateNestedList(2, 2);

            var actual = new List<object>(list.EnumerateRecursive());

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestMethod]
        public void EnumerateRecursiveNull()
        {
            object value = null;
            Assert.AreEqual(null, value.EnumerateRecursive().First());
        }

        [TestMethod]
        public void EnumerateRecursiveEmpty()
        {
            var list = Enumerable.Empty<object>();
            Assert.AreEqual(list, list.EnumerateRecursive().First());
        }

        [TestMethod]
        public void EnumerateRecursiveReferenceLoop()
        {
            var a = new List<object>();
            var b = new List<object>();
            a.Add(b);
            b.Add(a);

            var actual = a.EnumerateRecursive().ToList();

            CollectionAssert.AreEquivalent(new List<object> { a, b }, actual);
            CollectionAssert.AreEquivalent(b.EnumerateRecursive().ToList(), actual);
        }

        [TestMethod]
        public void EnumerateRecursivePerformance()
        {
            (var _, var nested) = CreateNestedList(10, 5000);

            var sw = Stopwatch.StartNew();
            int totalCount = nested.EnumerateRecursive().Count();
            sw.Stop();

            double millisPerObject = sw.Elapsed.TotalMilliseconds / totalCount;

            Assert.IsTrue(millisPerObject < 0.01);
        }

        internal static (List<object> flattened, List<object> nested) CreateNestedList(int childrenCount, int depth)
            => CreateNestedList(childrenCount, depth, (i, j) => $"{i},{j}");

        internal static (List<object> flattened, List<object> nested) CreateNestedList(int childrenCount, int depth, Func<int, int, object> valueFactory)
        {
            var root = new List<object>();
            var flat = new List<object>();

            var list = root;
            flat.Add(root);

            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < childrenCount; j++)
                {
                    var value = valueFactory(i, j);
                    list.Add(value);
                    flat.Add(value);
                }

                if (i < (depth - 1))
                {
                    var nested = new List<object>();
                    list.Add(nested);
                    flat.Add(nested);
                    list = nested;
                }
            }

            return (flat, root);
        }

        [TestMethod]
        public void EnumerateRecursiveDictionary()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("a", "Alice");
            dict.Add("b", "Bob");
            dict.Add("n", null);
            dict.Add("d", new Dictionary<string, object>
            {
                ["c"] = "Chris",
                ["e"] = "Eve",
                ["f"] = new Dictionary<string, string>
                {
                    ["g"] = "George",
                    ["h"] = "Henry",
                    ["i"] = null
                }
            });

            var expected = new List<object>
            {
                dict,
                "Alice",
                "Bob",
                null,
                dict["d"],
                "Chris",
                "Eve",
                (dict["d"] as IDictionary<string, object>)["f"],
                "George",
                "Henry",
                null
            };

            var actual = dict.EnumerateRecursive().ToArray();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestMethod]
        public void EnumerateRecursiveMixed()
        {
            (var expected, var list) = CreateNestedList(2, 2);
            var dict = new Dictionary<string, object>
            {
                ["a"] = "Alice",
                ["b"] = "Bob",
                ["c"] = list
            };

            list.Add(dict);

            expected.Add(dict);
            expected.Add("Alice");
            expected.Add("Bob");

            var actual = list.EnumerateRecursive().ToArray();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        private void AlignHelper(int[] original, int[] aligned, int align)
        {
            CollectionAssert.AreEqual(original.Select(o => o.Align(align)).ToList(),
                aligned);
        }

        private void IsAlignedHelper(int[] nums, bool[] compare, int align)
        {
            CollectionAssert.AreEqual(nums.Select(o => o.IsAligned(align)).ToList(),
                compare);
        }

        [TestMethod]
        public void AlignBy1()
        {
            AlignHelper(
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 },
                1);
        }

        [TestMethod]
        public void AlignBy2()
        {
            AlignHelper(
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                new int[] { 0, 2, 2, 4, 4, 6, 6, 8 },
                2);
        }

        [TestMethod]
        public void AlignBy4()
        {
            AlignHelper(
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                new int[] { 0, 4, 4, 4, 4, 8, 8, 8, 8, 12, 12, 12, 12, 16, 16, 16 },
                4);
        }

        [TestMethod]
        public void IsAlignedBy1()
        {
            IsAlignedHelper(
                new int[] { 0, 1, 2, 3 },
                new bool[] { true, true, true, true },
                1);
        }

        [TestMethod]
        public void IsAlignedBy2()
        {
            IsAlignedHelper(
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                new bool[] { true, false, true, false, true, false, true, false },
                2);
        }

        [TestMethod]
        public void IsAlignedBy4()
        {
            IsAlignedHelper(
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                new bool[] { true, false, false, false, true, false, false, false,
                             true, false, false, false, true, false, false, false},
                4);
        }
    }
}
