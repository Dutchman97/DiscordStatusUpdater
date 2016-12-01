using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public static class WebBrowserLoader
    {
        public static void LoadPlayers(List<Player> players)
        {
            List<WebsiteTitleParser> websites = new List<WebsiteTitleParser>();
            new WebsiteLoader().LoadItems(websites);

            players.Add(new Chrome(websites, "", " - Google Chrome"));
            players.Add(new Edge(websites, "", " - Microsoft Edge"));
        }
    }
}
