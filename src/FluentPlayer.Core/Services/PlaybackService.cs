using System.Threading.Tasks;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class PlaybackService
    {
        internal PlaybackService() { }

        internal static async Task<PlaybackService> CreateAsync()
        {
            var ins = new PlaybackService();
            return await Task.FromResult(ins);
        }
    }
}