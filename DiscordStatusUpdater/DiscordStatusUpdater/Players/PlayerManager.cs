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
            new VideoPlayerLoader().LoadItems(players);
            WebBrowserLoader.LoadPlayers(players);
        }

        public string GetVideoTitle()
        {
            foreach (Player player in players)
                foreach (string processName in player.ProcessNames)
                {
                    Process[] processes = Process.GetProcessesByName(processName);
                    foreach (Process process in processes)
                    {
                        string videoTitle;
                        if (player.TryGetVideoTitle(process, out videoTitle))
                            return videoTitle;
                    }
                }

            return string.Empty;
        }
    }
}
