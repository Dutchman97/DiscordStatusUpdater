using System.Text.RegularExpressions;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class VideoPlayerLoader : XmlLoader<Player>
    {
        const int MAJOR_VERSION = 1, MINOR_VERSION = 2;

        public VideoPlayerLoader() : base("cache/", "Players.xml", MAJOR_VERSION, MINOR_VERSION, "player")
        {

        }

        protected override Player LoadItem(XmlNode xmlVideoPlayer)
        {
            string titlePrefix = xmlVideoPlayer["titleprefix"].InnerText;
            string titleSuffix = xmlVideoPlayer["titlesuffix"].InnerText;
            Regex videoRegex = new Regex(@"(?<=" + titlePrefix + @").+(?=(\.\w{1,4})?" + titleSuffix + @")");

            string playerName = xmlVideoPlayer.Attributes["name"].Value;
            
            XmlNodeList xmlProcessNames = ((XmlElement)xmlVideoPlayer).GetElementsByTagName("filename");
            string[] processNames = new string[xmlProcessNames.Count];
            for (int i = 0; i < xmlProcessNames.Count; i++)
                processNames[i] = xmlProcessNames[i].InnerText;

            return new VideoPlayer(processNames, playerName, titlePrefix, titleSuffix);
        }
    }
}
