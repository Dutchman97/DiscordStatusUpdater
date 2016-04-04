using System;
using System.Xml;

namespace DiscordStatusUpdater
{
    [Serializable]
    public class Player
    {
        public string Name { get; private set; }
        public string FileName { get; private set; }
        public string TitlePrefix { get; private set; }
        public string TitleSuffix { get; private set; }

        public Player(string name, string fileName, string titlePrefix, string titleSuffix)
        {
            Name = name; FileName = fileName;
            TitlePrefix = titlePrefix; TitleSuffix = titleSuffix;
        }

        public Player(XmlNode player)
        {
            Name = player.Attributes["name"].Value;
            FileName = player["filename"].InnerText;
            TitlePrefix = player["titleprefix"].InnerText;
            TitleSuffix = player["titlesuffix"].InnerText;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]: \"{2}abcd.mkv{3}\"", new string[] { Name, FileName, TitlePrefix, TitleSuffix });
        }
    }
}
