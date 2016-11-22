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
        public VideoPlayer(string[] processNames, string playerName, Regex videoRegex) : base(processNames, playerName)
        {
            this.videoRegex = videoRegex;
        }

        public override string GetVideoTitle(Process process)
        {
            string windowTitle = process.MainWindowTitle;
            string videoName = videoRegex.Match(windowTitle).Value;
            return videoName;
        }

        public override bool IsVideoPlaying(Process process)
        {
            string windowTitle = process.MainWindowTitle;
            return videoRegex.IsMatch(windowTitle);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
