using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public class PlayerList
    {
        List<IPlayer> players;

        public PlayerList()
        {
            players = new List<IPlayer>();

            // TODO: Load XML nodes of video players from XML files
            // TODO: Load every WebBrowser
        }

        public string GetVideoTitle()
        {
            // TODO: Check through each IPlayer if it's active and if a video is playing
            throw new NotImplementedException();
        }
    }
}
