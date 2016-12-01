﻿using System;
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
            // Try to remove the prefix...
            string windowTitle = process.MainWindowTitle;
            if (titlePrefix != string.Empty && windowTitle.StartsWith(titlePrefix))
                windowTitle = windowTitle.Substring(titlePrefix.Length);

            // ...and the suffix from the window title
            if (titleSuffix != string.Empty && windowTitle.EndsWith(titleSuffix))
                windowTitle = windowTitle.Remove(windowTitle.Length - titleSuffix.Length);

            Uri uri;
            if (TryGetUrl(process, out uri))
                foreach (WebsiteTitleParser website in websites)
                    if (website.IsWebsiteUrl(uri))
                        if (website.TryParse(windowTitle, out videoTitle))
                            return true;

            videoTitle = null;
            return false;
        }
    }
}
