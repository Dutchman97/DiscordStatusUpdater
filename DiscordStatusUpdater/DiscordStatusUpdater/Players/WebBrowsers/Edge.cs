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

        /*
        protected override Window[] GetWindows(Process process)
        {
            if (!process.MainWindowTitle.Contains("Microsoft Edge"))
                return new Window[0];

            return base.GetWindows(process);
        }*/

        /*
        protected override bool TryGetUrl(Process process, out Uri uri)
        {
            uri = null;

            // The process must have a window and that window must contain "Microsoft Edge"
            if (process.MainWindowHandle == IntPtr.Zero || !process.MainWindowTitle.Contains("Microsoft Edge"))
                return false;

            AutomationElement root = AutomationElement.FromHandle(process.MainWindowHandle);

            // For each element, try to find out if it contains an URL
            var descendants = root.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            for (int i = 0; i < descendants.Count; i++)
            {
                string propertyValue = (string)descendants[i].GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
                if (string.IsNullOrWhiteSpace(propertyValue))
                    continue;
                
                if (!Uri.TryCreate("http://" + propertyValue, UriKind.Absolute, out uri))
                    continue;

                return true;
            }

            return false;
        }
        */
    }
}
