using System.Collections.Generic;
using System.Diagnostics;

namespace DiscordStatusUpdater.Players
{
    public class PlayerManager
    {
        List<Player> players;

        public PlayerManager()
        {
            players = new List<Player>();
            new VideoPlayerLoader().LoadItems(players);
            new WebBrowserLoader().LoadItems(players);
        }

        public void Reload()
        {
            players.Clear();
            new VideoPlayerLoader().LoadItems(players);
            new WebBrowserLoader().LoadItems(players);
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
