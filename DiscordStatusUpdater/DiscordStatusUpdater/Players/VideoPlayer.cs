using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class VideoPlayer : IPlayer
    {
        public VideoPlayer(XmlNode playerNode)
        {
            // TODO: Read XML node for player's properties
            videoRegex = null;
            ProcessName = null;
            PlayerName = null;
        }

        public override string GetVideoTitle()
        {
            throw new NotImplementedException();
        }

        public override bool IsVideoPlaying()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
