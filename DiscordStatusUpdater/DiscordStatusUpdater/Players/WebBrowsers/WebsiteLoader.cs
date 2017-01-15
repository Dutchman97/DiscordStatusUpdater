using System.Text.RegularExpressions;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class WebsiteLoader : XmlLoader<WebsiteTitleParser>
    {
        const int MAJOR_VERSION = 1, MINOR_VERSION = 0;

        public WebsiteLoader() : base("cache/", "Websites.xml", MAJOR_VERSION, MINOR_VERSION, "website")
        {

        }

        protected override WebsiteTitleParser LoadItem(XmlNode item)
        {
            string name = item.Attributes["name"].Value;

            Regex urlRegex = new Regex(item["urlregex"].InnerText);

            Regex regex = new Regex(item["regex"].InnerText);

            int animeIdx = int.Parse(item["animeidx"].InnerText);
            int episodeIdx = int.Parse(item["episodeidx"].InnerText);

            return new WebsiteTitleParser(name, urlRegex, regex, animeIdx, episodeIdx);
        }
    }
}
