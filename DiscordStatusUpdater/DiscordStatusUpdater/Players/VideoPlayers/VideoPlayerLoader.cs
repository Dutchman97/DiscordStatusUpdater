using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public class VideoPlayerLoader : XmlLoader<Player>
    {
        const int MAJOR_VERSION = 1, MINOR_VERSION = 2;
        const string FILE_PATH = "", FILE_NAME = "Players.xml";

        public VideoPlayerLoader() : base(FILE_PATH, FILE_NAME, MAJOR_VERSION, MINOR_VERSION, "player")
        {

        }

        protected override Player LoadItem(XmlNode xmlVideoPlayer)
        {
            string titlePrefix = xmlVideoPlayer["titleprefix"].InnerText;
            string titlesuffix = xmlVideoPlayer["titlesuffix"].InnerText;
            Regex videoRegex = new Regex(@"(?<=" + titlePrefix + @").+(?=(\.\w{1,4})?" + titlesuffix + @")");

            string playerName = xmlVideoPlayer.Attributes["name"].Value;
            
            XmlNodeList xmlProcessNames = ((XmlElement)xmlVideoPlayer).GetElementsByTagName("filename");
            string[] processNames = new string[xmlProcessNames.Count];
            for (int i = 0; i < xmlProcessNames.Count; i++)
                processNames[i] = xmlProcessNames[i].InnerText;

            return new VideoPlayer(processNames, playerName, titlePrefix, titlesuffix);
        }
    }
}
