using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class PlayerManager
    {
        List<Player> players;

        public PlayerManager()
        {
            players = new List<Player>();
            VideoPlayerLoader.LoadPlayers(players);
            WebBrowserLoader.LoadPlayers(players);
            // TODO: Load every WebBrowser
        }

        public string GetVideoTitle()
        {
            Process[] processes = Process.GetProcesses();
            /*
            foreach (Process proc in processes)
                if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    foreach (Player player in players)
                        foreach (string processName in player.ProcessNames)
                            if (proc.ProcessName.ToLower() == processName.ToLower())
                                if (player.IsVideoPlaying(proc))
                                    return player.GetVideoTitle(proc);
                                    */

            //foreach (Process p in Process.GetProcesses())
            //    if (!string.IsNullOrEmpty(p.MainWindowTitle))
            //        Debug.WriteLine("{0}: {1}", p.ProcessName, p.MainWindowTitle);

            foreach (Player player in players)
                foreach (string processName in player.ProcessNames)
                    foreach (Process process in Process.GetProcessesByName(processName))
                        if (player.IsVideoPlaying(process))
                            return player.GetVideoTitle(process);

            return string.Empty;
        }
    }
}
