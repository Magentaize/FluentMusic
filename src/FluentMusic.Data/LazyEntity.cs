using System;

namespace FluentMusic.Data
{
    public abstract class LazyEntity
    {
        protected Action<object, string> LazyLoader { get; set; }

        protected LazyEntity(Action<object, string> lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        public LazyEntity() { }
    }
}
