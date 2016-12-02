using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

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

        #region GetWindows()-related code
        /*
        protected virtual Window[] GetWindows(Process process)
        {
            // The process must have a window 
            if (process.MainWindowHandle == IntPtr.Zero)
                return new Window[0];

            List<Window> windowList = new List<Window>();

            var windowHandles = GetWindows((uint)process.Id);
            foreach (var windowHandle in windowHandles)
            {
                Debug.WriteLine("=====");

                AutomationElement root = AutomationElement.FromHandle(windowHandle);
                var descendants = root.FindAll(TreeScope.Descendants, Condition.TrueCondition);
                for (int i = 0; i < descendants.Count; i++)
                {
                    string propertyValue = (string)descendants[i].GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
                    if (string.IsNullOrWhiteSpace(propertyValue))
                        continue;
                    Debug.WriteLine(propertyValue);

                    Uri uri;
                    if (!Uri.TryCreate("http://" + propertyValue, UriKind.Absolute, out uri))
                        continue;

                    Window window = new Window() { Pointer = windowHandle, Process = process, Title = GetWindowTextRaw(windowHandle), Uri = uri };
                    Debug.WriteLine(window);
                    windowList.Add(window);
                    break;
                }
            }

            return windowList.ToArray();
        }
        */

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hwnd, int lParam);

        static List<IntPtr> GetWindows(uint pid)
        {
            List<IntPtr> result = new List<IntPtr>();

            EnumWindows(new EnumWindowsProc((hwnd, lParam) =>
            {
                uint windowPid;
                GetWindowThreadProcessId(hwnd, out windowPid);

                if (windowPid == pid)
                    result.Add(hwnd);

                return true;
            }), IntPtr.Zero);

            return result;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);

        public static string GetWindowTextRaw(IntPtr hwnd)
        {
            // Allocate correct string length first
            int length = (int)SendMessage(hwnd, 0x000E, IntPtr.Zero, null);
            StringBuilder sb = new StringBuilder(length + 1);

            SendMessage(hwnd, 0x000D, (IntPtr)sb.Capacity, sb);
            return sb.ToString();
        }
        #endregion

        public override bool TryGetVideoTitle(Process process, out string videoTitle)
        {
            videoTitle = null;

            // The process must have a (main) window 
            if (process.MainWindowHandle == IntPtr.Zero)
                return false;

            Stopwatch stopwatch = Stopwatch.StartNew();
            // For each window of the specified process...
            var windowHandles = GetWindows((uint)process.Id);
            foreach (var windowHandle in windowHandles)
            {
                //Debug.WriteLine("=====");

                string windowTitle = GetWindowTextRaw(windowHandle);

                if (string.IsNullOrWhiteSpace(windowTitle))
                    continue;

                windowTitle = RemovePrefixAndSuffix(windowTitle);

                // For each automation element...
                AutomationElement root = AutomationElement.FromHandle(windowHandle);
                var descendants = root.FindAll(TreeScope.Descendants, new NotCondition(new PropertyCondition(ValuePatternIdentifiers.ValueProperty, string.Empty)));
                for (int i = 0; i < descendants.Count; i++)
                {
                    string propertyValue = (string)descendants[i].GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
                    if (string.IsNullOrWhiteSpace(propertyValue))
                        continue;

                    if (!propertyValue.StartsWith("http"))
                        propertyValue = "http://" + propertyValue;

                    //Debug.WriteLine(propertyValue);

                    Uri uri;
                    if (!Uri.TryCreate(propertyValue, UriKind.Absolute, out uri))
                        continue;

                    foreach (WebsiteTitleParser website in websites)
                        if (website.IsWebsiteUrl(uri))
                            if (website.TryParse(windowTitle, out videoTitle))
                            {
                                stopwatch.Stop();
                                Debug.WriteLine("Succeeded in " + stopwatch.Elapsed);
                                return true;
                            }
                }
            }

            stopwatch.Stop();
            Debug.WriteLine("Failed in " + stopwatch.Elapsed);
            return false;
        }

        string RemovePrefixAndSuffix(string windowTitle)
        {
            string result = windowTitle;

            if (titlePrefix != string.Empty && windowTitle.StartsWith(titlePrefix))
                result = result.Substring(titlePrefix.Length);
            
            if (titleSuffix != string.Empty && windowTitle.EndsWith(titleSuffix))
                result = result.Remove(windowTitle.Length - titleSuffix.Length);

            return result;
        }
    }
}
