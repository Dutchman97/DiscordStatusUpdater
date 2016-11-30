using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiscordStatusUpdater.Players
{
    public class WebsiteTitleParser : TitleParser
    {
        string baseUrl;
        int animeIdx, episodeIdx;

        public WebsiteTitleParser(string name, string baseUrl, Regex regex, int animeIdx, int episodeIdx)
        {
            this.regex = regex;
            this.animeIdx = animeIdx;
            this.episodeIdx = episodeIdx;
            Name = name;
        }

        public bool IsWebsiteUrl(Uri uri)
        {
            return uri.Host.Contains(baseUrl);
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
                episode = " - " + match.Groups[episodeIdx].Captures[0].Value;

            result = anime + episode;
            return true;
        }

        public string Name { get; private set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
