using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;

namespace DiscordStatusUpdater.Players
{
    public class Edge : WebBrowser
    {
        public Edge(List<WebsiteTitleParser> websites, string titlePrefix, string titleSuffix)
            : base(new string[] { "ApplicationFrameHost" }, "(Possibly) Microsoft Edge", websites, titlePrefix, titleSuffix)
        {

        }

        public override bool TryGetVideoTitle(Process process, out string videoTitle)
        {
            if (!process.MainWindowTitle.Contains("Microsoft Edge"))
            {
                videoTitle = null;
                return false;
            }
            return base.TryGetVideoTitle(process, out videoTitle);
        }
    }
}
