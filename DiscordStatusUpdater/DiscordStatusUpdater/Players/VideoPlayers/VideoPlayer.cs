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
        VideoPlayerTitleParser titleParser;

        public VideoPlayer(string[] processNames, string playerName, string titlePrefix, string titleSuffix) : base(processNames, playerName)
        {
            titleParser = new VideoPlayerTitleParser(titlePrefix, titleSuffix);
        }

        public override bool TryGetVideoTitle(Process process, out string videoTitle)
        {
            string windowTitle = process.MainWindowTitle;

            string result;
            if (titleParser.TryParse(windowTitle, out result))
            {
                videoTitle = result;
                return true;
            }
            else
            {
                videoTitle = null;
                return false;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
