using System;

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
    }
}
