using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public static class Extensions
    {
        public static IEnumerable<object> EnumerateRecursive(this object value)
            => value.EnumerateRecursive(EnumerateChildren);

        internal static IEnumerable<object> EnumerateRecursive(this object value, Func<object, IEnumerable> childrenEnumerator)
        {
            var visited = new HashSet<object>();
            var queue = new Queue<object>();

            queue.Enqueue(value);

            while (queue.Count > 0)
            {
                var next = queue.Dequeue();

                if (!visited.Contains(next))
                {
                    yield return next;

                    if (next != null)
                    {
                        visited.Add(next);

                        foreach (var child in childrenEnumerator(next))
                            queue.Enqueue(child);
                    }
                }
            }
        }

        internal static IEnumerable EnumerateChildren(this object value)
        {
            if (value is IDictionary dictionary)
            {
                foreach (var child in dictionary.Values)
                    yield return child;
            }

            // We do not want to bother enumerating the chars of a string
            else if (value is IEnumerable enumerable && value.GetType() != typeof(string))
            {
                foreach (var child in enumerable)
                    yield return child;
            }

            yield break;
        }

        public static bool IsDefined(this Enum value)
        {
            return Enum.IsDefined(value.GetType(), value);
        }

        /// <summary>
        /// Aligns a value to the specified alignment.
        /// </summary>
        /// <param name="value">value to align. Must be non-negative.</param>
        /// <param name="alignment">desired alignment. Must be positive.</param>
        /// <returns>aligned value</returns>
        public static int Align(this int value, int alignment)
        {
            if (alignment < 1)
                throw new InvalidOperationException("Alignment must be positive");

            if (value < 0)
                throw new InvalidOperationException("Value must be non-negative");

            if (alignment == 1)
                return value;

            int mask = -alignment;
            value += alignment - 1;
            value &= mask;
            return value;
        }

        /// <summary>
        /// Checks whether a value is aligned to the specified alignment.
        /// </summary>
        /// <param name="value">value to check. Must be non-negative.</param>
        /// <param name="alignment">alignment to check. Must be positive.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is aligned to <paramref name="alignment"/>, <c>false</c> otherwise</returns>
        public static bool IsAligned(this int value, int alignment)
        {
            if (alignment < 1)
                throw new InvalidOperationException("Alignment must be positive");

            if (value < 0)
                throw new InvalidOperationException("Value must be non-negative");

            if (alignment == 1)
                return true;

            int mask = -alignment;
            return value == (value & mask);
        }

        /// <summary>
        /// Enumerates the nodes of a <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <param name="list">the linked list whose nodes to enumerate</param>
        public static IEnumerable<LinkedListNode<T>> EnumerateNodes<T>(this LinkedList<T> list)
        {
            var current = list.First;

            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        public static void AddRange<T>(this ISet<T> set, IEnumerable<T> values)
        {
            foreach (T value in values)
                set.Add(value);
        }
    }
}
