using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DiscordStatusUpdater.Players
{
    public class Chrome : WebBrowser
    {
        public Chrome(List<WebsiteTitleParser> websites, string titlePrefix, string titleSuffix)
            : base(new string[] { "chrome" }, "Google Chrome", websites, titlePrefix, titleSuffix)
        {

        }
    }
}
