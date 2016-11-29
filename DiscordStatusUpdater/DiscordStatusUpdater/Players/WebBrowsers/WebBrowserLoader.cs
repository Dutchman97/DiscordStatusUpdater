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
            //players.Add(new Chrome());
            players.Add(new Edge());
        }
    }
}
