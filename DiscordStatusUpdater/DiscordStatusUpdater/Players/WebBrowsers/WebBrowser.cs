using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public abstract class WebBrowser : Player
    {
        public WebBrowser(string[] processNames, string browserName) : base(processNames, browserName)
        {

        }
        // TODO: Think more about how to implement web browsers in this

        protected abstract string GetUrl();
    }
}
