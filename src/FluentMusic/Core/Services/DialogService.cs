using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace FluentMusic.Core.Services
{
    public sealed class DialogService
    {
        public static async Task OkAsync(string msg)
        {
            var dlg = new MessageDialog(msg);
            await dlg.ShowAsync();
        }

        public static async Task<bool> ConfirmOrNotAsync(string msg)
        {
            var dlg = new MessageDialog(msg);
            var confirm = new UICommand("Confirm");
            var cancel = new UICommand("Cancel");
            dlg.Commands.Add(confirm);
            dlg.Commands.Add(cancel);
            var result = await dlg.ShowAsync();

            return result == confirm;
        }
    }
}
