namespace FluentMusic.Data.Extensions
{
    public static class ObjectExtension
    {
        public static T Cast<T>(this object obj)
        {
            switch (obj)
            {
                case null: return default(T);
                default: return (T) obj;
            }
        }
    }
}