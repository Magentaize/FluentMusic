using DynamicData;
using FluentMusic.Core.Extensions.Internal;
using System;
using System.Collections.Generic;

namespace FluentMusic.Core.Extensions
{
    public static class ChangeSetExtension
    {
        internal static IEnumerable<UnifiedChange<T>> Unified<T>(this IChangeSet<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new UnifiedChangeEnumerator<T>(source);
        }
    }
}
