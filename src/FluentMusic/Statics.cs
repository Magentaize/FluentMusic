using Windows.Foundation.Collections;
using Windows.Storage;

namespace FluentMusic
{
    public static class Statics
    {
        public static IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;

        public static readonly string[] AudioFileTypes = { ".flac", ".wav", ".m4a", ".aac", ".mp3", ".wma", ".ogg", ".oga", ".opus" };
    }
}