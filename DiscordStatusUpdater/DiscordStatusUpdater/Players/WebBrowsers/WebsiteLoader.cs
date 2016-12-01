using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class WebsiteLoader : XmlLoader<WebsiteTitleParser>
    {
        const int MAJOR_VERSION = 1, MINOR_VERSION = 0;
        const string FILE_PATH = "", FILE_NAME = "Websites.xml";

        public WebsiteLoader() : base(FILE_PATH, FILE_NAME, MAJOR_VERSION, MINOR_VERSION, "website")
        {

        }

        protected override WebsiteTitleParser LoadItem(XmlNode item)
        {
            string name = item.Attributes["name"].Value;

            string baseUrl = item["url"].InnerText;

            Regex regex = new Regex(item["regex"].InnerText);

            int animeIdx = int.Parse(item["animeidx"].InnerText);
            int episodeIdx = int.Parse(item["episodeidx"].InnerText);

            return new WebsiteTitleParser(name, baseUrl, regex, animeIdx, episodeIdx);
        }
    }
}
