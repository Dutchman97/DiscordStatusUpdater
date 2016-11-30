using System;
using System.Diagnostics;

namespace DiscordStatusUpdater.Players
{
    public abstract class WebBrowser : Player
    {
        public WebBrowser(string[] processNames, string browserName) : base(processNames, browserName)
        {

        }
        // TODO: Think more about how to implement web browsers in this

        protected abstract bool TryGetUrl(Process process, out Uri uri);
    }
}
