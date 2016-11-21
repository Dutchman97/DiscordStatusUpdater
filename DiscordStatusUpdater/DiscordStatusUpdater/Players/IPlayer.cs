using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public abstract class IPlayer
    {
        protected Regex videoRegex;

        public abstract string GetVideoTitle();
        public abstract bool IsVideoPlaying();

        public string ProcessName { get; protected set; }
        public string PlayerName { get; protected set; }

        public override string ToString()
        {
            return string.Format("[{0}]: {1}", ProcessName, PlayerName);
        }
    }
}
