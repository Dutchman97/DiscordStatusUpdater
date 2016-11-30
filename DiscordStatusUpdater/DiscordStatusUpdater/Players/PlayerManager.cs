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
            // TODO: Load every website
        }

        public string GetVideoTitle()
        {
            Process[] processes = Process.GetProcesses();

            foreach (Player player in players)
                foreach (string processName in player.ProcessNames)
                    foreach (Process process in Process.GetProcessesByName(processName))
                    {
                        string videoTitle;
                        if (player.TryGetVideoTitle(process, out videoTitle))
                            return videoTitle;
                    }

            return string.Empty;
        }
    }
}
