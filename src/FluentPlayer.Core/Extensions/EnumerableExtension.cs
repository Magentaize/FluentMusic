using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Magentaize.FluentPlayer.Core.Extensions
{
    public static class EnumerableExtension
    {
        public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, Task<IEnumerable<TResult>>> selector)
        {
            return (await Task.WhenAll(source.Select(selector))).SelectMany(s=>s);
        }
    }
} 