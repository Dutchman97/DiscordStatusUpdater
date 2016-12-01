﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;

namespace DiscordStatusUpdater.Players
{
    public class Chrome : WebBrowser
    {
        public Chrome(List<WebsiteTitleParser> websites, string titlePrefix, string titleSuffix)
            : base(new string[] { "chrome" }, "Google Chrome", websites, titlePrefix, titleSuffix)
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
                Debug.WriteLine(propertyValue);

                if (!Uri.TryCreate("http://" + propertyValue, UriKind.Absolute, out uri))
                    continue;

                return true;
            }

            return false;
        }
    }
}