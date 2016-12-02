using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiscordStatusUpdater.Players
{
    public class WebsiteTitleParser : TitleParser
    {
        Regex urlRegex;
        int animeIdx, episodeIdx;

        public WebsiteTitleParser(string name, Regex urlRegex, Regex regex, int animeIdx, int episodeIdx)
        {
            this.urlRegex = urlRegex;
            this.regex = regex;
            this.animeIdx = animeIdx;
            this.episodeIdx = episodeIdx;
            Name = name;
        }

        public bool IsWebsiteUrl(Uri uri)
        {
            return urlRegex.IsMatch(uri.AbsoluteUri);
        }

        public override bool TryParse(string fullTitle, out string result)
        {
            result = null;

            if (!regex.IsMatch(fullTitle))
                return false;

            Match match = regex.Match(fullTitle);
            string anime = match.Groups[animeIdx].Captures[0].Value;

            string episode;
            if (episodeIdx == 0)
                episode = "";
            else
                episode = " - Episode " + match.Groups[episodeIdx].Captures[0].Value;

            result = anime + episode;
            return true;
        }

        public string Name { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
