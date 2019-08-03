namespace FluentMusic.Core.IO
{
    public static class ApplicationPaths
    {
        public static string BuiltinLanguagesFolder = "Languages";
        public static string ExecutionFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}