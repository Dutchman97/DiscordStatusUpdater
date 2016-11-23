using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public class TitleParser
    {
        Regex regex;
        int captIdx;

        public TitleParser(string prefix, string suffix)
        {
            // Regex to capture player prefix and suffix, and file extension, and video file name
            regex = new Regex(@"^(" + prefix + @")(.+)(\.\w{1,4})(" + suffix + ")$");
            captIdx = 1;
        }

        public bool CanParse(string fullTitle)
        {
            // Return whether or not the regex matches
            return regex.IsMatch(fullTitle);
        }

        public string Parse(string fullTitle)
        {
            // Get the captured video file name using the regex
            // Note: for some reason, group index starts with 1, while capture index starts with 0
            string videoName = regex.Match(fullTitle).Groups[1].Captures[captIdx].Value;

            // Remove everything between square brackets
            videoName = Regex.Replace(videoName, @"\[\w+?\]", "");

            // Remove all leading and trailing whitespace
            videoName = videoName.Trim();
            return videoName;
        }

        public override string ToString()
        {
            return regex.ToString();
        }
    }
}
