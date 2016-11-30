using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DiscordStatusUpdater.Players
{
    public abstract class WebBrowser : Player
    {
        List<WebsiteTitleParser> websites;
        string titlePrefix, titleSuffix;

        public WebBrowser(string[] processNames, string browserName, List<WebsiteTitleParser> websites, string titlePrefix, string titleSuffix) : base(processNames, browserName)
        {
            this.websites = websites;
            this.titlePrefix = titlePrefix;
            this.titleSuffix = titleSuffix;
        }

        protected abstract bool TryGetUrl(Process process, out Uri uri);

        public override bool TryGetVideoTitle(Process process, out string videoTitle)
        {
            Uri uri;

            if (TryGetUrl(process, out uri))
                foreach (WebsiteTitleParser website in websites)
                    if (website.IsWebsiteUrl(uri))
                        if (website.TryParse(process.MainWindowTitle, out videoTitle))
                            return true;

            videoTitle = null;
            return false;
        }
    }
}
