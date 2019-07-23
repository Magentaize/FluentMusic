using System.Collections.Generic;

namespace DynamicData
{
    public static class SourceList
    {
        public static SourceList<T> CreateFromEnumerable<T>(IEnumerable<T> source)
        {
            var ret = new SourceList<T>();

            ret.Edit(a =>
            {
                a.AddRange(source);
            });

            return ret;
        }
    }
}
