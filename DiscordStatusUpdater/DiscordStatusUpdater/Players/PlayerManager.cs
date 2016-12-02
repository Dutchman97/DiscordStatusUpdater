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
            new WebBrowserLoader().LoadItems(players);
        }

        public string GetVideoTitle()
        {
            foreach (Player player in players)
                foreach (string processName in player.ProcessNames)
                {
                    //Debug.WriteLineIf(processName == "chrome", "----------");

                    Process[] processes = Process.GetProcessesByName(processName);
                    foreach (Process process in processes)
                    {
                        //if (processName == "chrome")
                        //    Debug.WriteLine("{0}: {1}", process.Id, process.MainWindowTitle);

                        string videoTitle;
                        if (player.TryGetVideoTitle(process, out videoTitle))
                            return videoTitle;
                    }
                }

            return string.Empty;
        }
    }
}
