using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public class Window
    {
        public string Title;
        public Uri Uri;
        public Process Process;
        public IntPtr Pointer;

        public override string ToString()
        {
            return string.Format("[{0}] {1} - {2}", Process.ProcessName, Title, Uri);
        }
    }
}
