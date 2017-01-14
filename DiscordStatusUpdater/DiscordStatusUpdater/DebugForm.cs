using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Windows.Forms;

namespace DiscordStatusUpdater
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }

        void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                outputTextBox.Text = Do(inputTextBox.Text);
            }
        }

        string Do(string input)
        {
            //string output = "File: " + input + ".exe" + Environment.NewLine;
            string output = string.Empty;

            Process[] processes;
            if (input.Contains("*"))
            {
                processes = Process.GetProcesses();
                processes = processes.Where((process) => { return Regex.IsMatch(process.ProcessName.ToLower(), "^" + input.ToLower().Replace("*", ".*") + "$"); }).ToArray();
            }
            else
                processes = Process.GetProcessesByName(input);

            output += "Found " + processes.Length + " processes matching \"" + input + "\"." + Environment.NewLine + Environment.NewLine;

            foreach (Process process in processes)
            {
                var windowHandles = GetWindows((uint)process.Id);
                if (windowHandles.Count == 0)
                    continue;

                output += process.ProcessName + " (" + process.Id + "):" + Environment.NewLine;
                output += "Found " + windowHandles.Count + " windows belonging to this process." + Environment.NewLine;

                foreach (var windowHandle in windowHandles)
                {
                    string windowTitle = GetWindowTextRaw(windowHandle);
                    output += "Window title: " + windowTitle + Environment.NewLine;

                    AutomationElement root = AutomationElement.FromHandle(windowHandle);
                    var descendants = root.FindAll(TreeScope.Descendants, new NotCondition(new PropertyCondition(ValuePatternIdentifiers.ValueProperty, string.Empty)));
                    for (int i = 0; i < descendants.Count; i++)
                    {
                        string propertyValue = (string)descendants[i].GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
                        if (string.IsNullOrWhiteSpace(propertyValue))
                            continue;

                        if (!propertyValue.StartsWith("http"))
                            propertyValue = "http://" + propertyValue;

                        Uri uri;
                        if (Uri.TryCreate(propertyValue, UriKind.Absolute, out uri))
                            output += "Found URL: " + propertyValue;
                        else
                            output += "No URL found.";
                        output += Environment.NewLine;
                    }
                }
                output += Environment.NewLine;
            }

            return output;
        }


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
    }
}
