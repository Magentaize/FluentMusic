using System.Collections.Generic;
using System.Diagnostics;

namespace DynamicData
{
    public static class SourceList
    {
        [DebuggerHidden]
        public static SourceList<T> CreateFromEnumerable<T>(IEnumerable<T> source)
        {
            var ret = new SourceList<T>();

            ret.Edit(a =>
            {
                a.AddRange(source);
            });

            return ret;
        }

        [DebuggerHidden]
        public static void Edit<T>(this ISourceList<T> source, IChangeSet<T> changes)
        {
            source.Edit(a =>
            {
                foreach (var c in changes)
                {
                    a.Edit(c);
                }
            });
        }
    }
}
