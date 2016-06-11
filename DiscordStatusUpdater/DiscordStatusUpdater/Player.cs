using System;
using System.Xml;

namespace DiscordStatusUpdater
{
    [Serializable]
    public class Player
    {
        public string Name { get; private set; }
        public string FileName { get; private set; }
        public Title Title { get; private set; }
        public bool Website { get; private set; }

        public Player(string name, string fileName, string titlePrefix, string titleSuffix)
        {

            Name = name; FileName = fileName;
            Title = new Title(titlePrefix, titleSuffix);
        }

        public Player(XmlNode player)
        {
            Name = player.Attributes["name"].Value;
            FileName = player["filename"].InnerText;
            Title = new Title(player["titleprefix"].InnerText, player["titlesuffix"].InnerText);
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}.exe]", new string[] { Name, FileName });
        }
    }
}
