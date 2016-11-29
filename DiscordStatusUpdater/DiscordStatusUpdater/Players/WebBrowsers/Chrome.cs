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

        protected override string GetUrl()
        {
            Process[] procsChrome = Process.GetProcessesByName(ProcessNames[0]);

            if (procsChrome.Length == 0)
                return null;

            string url = null;

            foreach (Process proc in procsChrome)
            {
                // the chrome process must have a window 
                if (proc.MainWindowHandle == IntPtr.Zero)
                    continue;
                
                // to find the tabs we first need to locate something reliable - the 'New Tab' button 
                AutomationElement root = AutomationElement.FromHandle(proc.MainWindowHandle);
                var SearchBar = root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));
                if (SearchBar != null)
                    url = (string)SearchBar.GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
            }

            return url;
        }

        public override string GetVideoTitle(Process process)
        {
            return GetUrl();
        }

        public override bool IsVideoPlaying(Process process)
        {
            throw new NotImplementedException();
        }
    }
}
