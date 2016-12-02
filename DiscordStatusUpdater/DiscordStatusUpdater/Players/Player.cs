using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public abstract class Player
    {
        public Player(string[] processNames, string playerName)
        {
            ProcessNames = processNames;
            PlayerName = playerName;
        }

        public abstract bool TryGetVideoTitle(Process process, out string videoTitle);

        public string[] ProcessNames { get; protected set; }
        public string PlayerName { get; protected set; }

        public override string ToString()
        {
            return string.Format("[{0}]: {1}", string.Join(".exe; ", ProcessNames) + ".exe", PlayerName);
        }
    }
}
