using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class VideoPlayer : Player
    {
        TitleParser titleParser;

        public VideoPlayer(string[] processNames, string playerName, TitleParser titleParser) : base(processNames, playerName)
        {
            this.titleParser = titleParser;
        }

        public override string GetVideoTitle(Process process)
        {
            string windowTitle = process.MainWindowTitle;
            return titleParser.Parse(windowTitle);
        }

        public override bool IsVideoPlaying(Process process)
        {
            string windowTitle = process.MainWindowTitle;
            return titleParser.CanParse(windowTitle);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
