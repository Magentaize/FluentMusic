using System.Linq;

namespace DynamicData
{
    public static class ExtendedListExtension
    {
        public static void Edit<T>(this IExtendedList<T> source, Change<T> change)
        {
            switch (change.Reason)
            {
                case ListChangeReason.Add:
                    source.Add(change.Item.Current);
                    break;
                case ListChangeReason.Remove:
                    source.Remove(change.Item.Current);
                    break;
                case ListChangeReason.AddRange:
                    source.AddRange(change.Range);
                    break;
                case ListChangeReason.RemoveRange:
                    source.RemoveMany(change.Range);
                    break;
                default: break;
            }
        }

        public static void Edit<T>(this IExtendedList<T> source, IChangeSet<T> changes)
        {
            foreach(var c in changes)
            {
                source.Edit(c);
            }
        }
    }
}
