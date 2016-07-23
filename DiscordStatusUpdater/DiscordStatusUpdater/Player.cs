using System;
using System.Xml;

namespace DiscordStatusUpdater
{
    public enum PlayerType { Player, Browser }

    [Serializable]
    public class Player
    {
        public string Name { get; private set; }
        public string FileName { get; private set; }
        public Title Title { get; private set; }
        PlayerType playerType;// { get; private set; }
        
        public Player(string name, string fileName, string titlePrefix, string titleSuffix, PlayerType playerType)
        {
            Name = name; FileName = fileName;
            Title = new Title(titlePrefix, titleSuffix);
            this.playerType = playerType;
        }

        public Player(XmlNode player)
        {
            Name = player.Attributes["name"].Value;
            FileName = player["filename"].InnerText;
            Title = new Title(player["titleprefix"].InnerText, player["titlesuffix"].InnerText);

            XmlAttribute playerTypeAttr = player.Attributes["type"];
            if (playerTypeAttr == null)
                // Player is a PlayerType.Player (video player) by default
                this.playerType = PlayerType.Player;
            else
                // POTENTIAL ERROR: "type" attribute does not necessarily have to have a corresponding PlayerType (typos, backwards compatibility, etc.)
                this.playerType = (PlayerType)Enum.Parse(typeof(PlayerType), playerTypeAttr.Value, true);
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}.exe]", new string[] { Name, FileName });
        }
    }
}
