using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class PredicateEqualityComparer<T> : IEqualityComparer<T>
    {
        public Func<T, T, bool> Predicate { get; }
        public Func<T, int> HashCodeFunc { get; }

        public PredicateEqualityComparer(Func<T, T, bool> predicate) : this(predicate, o => o.GetHashCode()) { }

        public PredicateEqualityComparer(Func<T, T, bool> predicate, Func<T, int> hashCodeFunc)
        {
            Predicate = predicate;
            HashCodeFunc = hashCodeFunc;
        }

        public bool Equals(T x, T y)
        {
            return Predicate(x, y);
        }

        public int GetHashCode(T obj)
        {
            return HashCodeFunc(obj);
        }
    }
}
