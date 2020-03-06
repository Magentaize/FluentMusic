using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class EnumerableExtension
    {
        private static readonly Random _rng = new Random();

        public static IList<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var ret = new List<T>(source);
            var n = ret.Count;
            while (n > 1)
            {
                n--;
                var k = _rng.Next(n + 1);
                var value = ret[k];
                ret[k] = ret[n];
                ret[n] = value;
            }

            return ret;
        }

        [DebuggerHidden]
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var x in source) action(x);
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T,Task> action)
        {
            foreach (var x in source) await action(x);
        }

        public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, Task<IEnumerable<TResult>>> selector)
        {
            return (await Task.WhenAll(source.Select(selector))).SelectMany(s=>s);
        }

        public static IEnumerable<T1> Except<T1, T2, TKey>(
            this IEnumerable<T1> first, 
            IEnumerable<T2> second, 
            Func<T1, TKey> selector1, 
            Func<T2, TKey> selector2, 
            IEqualityComparer<TKey> comparer = null)
        {
            var secondKeys = second.Select(selector2).ToHashSet();
            return first.Where(x => !secondKeys.Contains(selector1(x), comparer));
        }

        public static IEnumerable<T> Except<T, TKey>(
            this IEnumerable<T> first,
            IEnumerable<TKey> second,
            Func<T, TKey> selector,
            IEqualityComparer<TKey> comparer = null)
        {
            return first.Where(x => !second.Contains(selector(x), comparer));
        }
        
        public static IEnumerable<TKey> Intersect<T1, T2, TKey>(
            this IEnumerable<T1> first,
            IEnumerable<T2> second,
            Func<T1, TKey> selector1,
            Func<T2, TKey> selector2)
        {
            var arr1 = first.Select(selector1);
            var arr2 = second.Select(selector2);
            return arr1.Intersect(arr2);
        }
    }
} 