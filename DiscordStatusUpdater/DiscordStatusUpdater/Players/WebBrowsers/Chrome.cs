using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;

namespace DiscordStatusUpdater.Players
{
    public class Chrome : WebBrowser
    {
        public Chrome() : base(new string[] { "chrome" }, "Google Chrome")
        {
            
        }

        protected override bool TryGetUrl(Process process, out Uri uri)
        {
            uri = null;

            // The process must have a window 
            if (process.MainWindowHandle == IntPtr.Zero)
                return false;
                
            AutomationElement root = AutomationElement.FromHandle(process.MainWindowHandle);

            // For each element, try to find out if it contains an URL
            var descendants = root.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            for (int i = 0; i < descendants.Count; i++)
            {
                string propertyValue = (string)descendants[i].GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
                if (string.IsNullOrWhiteSpace(propertyValue))
                    continue;
                
                if (!Uri.TryCreate(propertyValue, UriKind.Absolute, out uri))
                    continue;

                return true;
            }

            return false;
        }

        public override bool TryGetVideoTitle(Process process, out string videoTitle)
        {
            Uri uri;
            if (TryGetUrl(process, out uri))
            {
                //videoTitle = UseUriToGetVideo(uri);
                videoTitle = "test123";
                return true;
            }
            else
            {
                videoTitle = null;
                return false;
            }
        }
    }
}
