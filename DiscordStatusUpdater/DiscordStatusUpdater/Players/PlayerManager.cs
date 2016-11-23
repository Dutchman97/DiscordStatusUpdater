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
            // TODO: Load every WebBrowser
        }

        public string GetVideoTitle()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process proc in processes)
                if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    foreach (Player player in players)
                        foreach (string processName in player.ProcessNames)
                            if (proc.ProcessName.ToLower() == processName.ToLower())
                                if (player.IsVideoPlaying(proc))
                                    return player.GetVideoTitle(proc);
            return string.Empty;
        }
    }
}
