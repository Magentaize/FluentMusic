using Magentaize.FluentPlayer.Data;

namespace Magentaize.FluentPlayer.Core
{
    public static class Singleton
    {
        public static FluentPlayerDbContext Db = new FluentPlayerDbContext();
    }
}