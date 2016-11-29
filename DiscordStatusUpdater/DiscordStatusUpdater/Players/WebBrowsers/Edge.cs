using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;

namespace DiscordStatusUpdater.Players
{
    public class Edge : WebBrowser
    {
        public Edge() : base(new string[] { "ApplicationFrameHost" }, "(Possibly) Microsoft Edge")
        {

        }

        protected override string GetUrl(Process process)
        {
            // The process must have a window 
            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement root = AutomationElement.FromHandle(process.MainWindowHandle);

            // For each element, try to find out if it contains an URL
            var descendants = root.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            for (int i = 0; i < descendants.Count; i++)
            {
                string propertyValue = (string)descendants[i].GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
                if (string.IsNullOrWhiteSpace(propertyValue))
                    continue;

                Uri uri;
                if (!Uri.TryCreate("http://" + propertyValue, UriKind.Absolute, out uri))
                    continue;

                return propertyValue;
            }

            return null;
        }

        public override string GetVideoTitle(Process process)
        {
            Debug.WriteLine(GetUrl(process));
            return "test321";//GetUrl(process);
        }

        public override bool IsVideoPlaying(Process process)
        {
            Debug.WriteLine("Checking for url...");
            return GetUrl(process) != null;
        }
    }
}
