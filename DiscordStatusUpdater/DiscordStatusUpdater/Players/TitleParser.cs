using System.Text.RegularExpressions;

namespace DiscordStatusUpdater.Players
{
    public abstract class TitleParser
    {
        protected Regex regex;

        public abstract bool TryParse(string fullTitle, out string result);
    }
}
