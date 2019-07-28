using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DynamicData
{
    public static class SourceCacheExtension
    {
        [DebuggerHidden]
        public static ISourceCache<TObject, TKey> AddOrUpdateForEach<TObject, TKey>(this ISourceCache<TObject, TKey> source,
            IEnumerable<TObject> items)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Edit(x => items.ForEach(a => x.AddOrUpdate(a)));

            return source;
        }
    }
}
