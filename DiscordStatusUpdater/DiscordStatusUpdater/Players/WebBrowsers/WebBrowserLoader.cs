using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class WebBrowserLoader : XmlLoader<Player>
    {
        const int MAJOR_VERSION = 1, MINOR_VERSION = 0;

        List<WebsiteTitleParser> websites;

        public WebBrowserLoader() : base("cache/", "WebBrowsers.xml", MAJOR_VERSION, MINOR_VERSION, "webbrowser")
        {
            websites = new List<WebsiteTitleParser>();
            new WebsiteLoader().LoadItems(websites);
        }

        protected override Player LoadItem(XmlNode xmlWebBrowser)
        {
            string titlePrefix = xmlWebBrowser["titleprefix"].InnerText;
            string titleSuffix = xmlWebBrowser["titlesuffix"].InnerText;
            Regex videoRegex = new Regex(@"(?<=" + titlePrefix + @").+(?=(\.\w{1,4})?" + titleSuffix + @")");

            string playerName = xmlWebBrowser.Attributes["name"].Value;

            XmlNodeList xmlProcessNames = ((XmlElement)xmlWebBrowser).GetElementsByTagName("filename");
            string[] processNames = new string[xmlProcessNames.Count];
            for (int i = 0; i < xmlProcessNames.Count; i++)
                processNames[i] = xmlProcessNames[i].InnerText;

            return new WebBrowser(processNames, playerName, websites, titlePrefix, titleSuffix);
        }
    }
}
