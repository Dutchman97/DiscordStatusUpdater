using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusUpdater
{
    public struct Title
    {
        public string Prefix { get; private set; }
        public string Suffix { get; private set; }

        public Title(string prefix, string suffix)
        {
            Prefix = prefix;
            Suffix = suffix;
        }

        public static string operator -(string fullTitle, Title title)
        {
            if (title.Suffix != string.Empty)
            {
                int suffixIndex = fullTitle.ToLower().LastIndexOf(title.Suffix.ToLower());
                fullTitle = fullTitle.Remove(suffixIndex);
                //Console.WriteLine("Suffix: " + title.Suffix + ", at: " + suffixIndex.ToString());
            }

            if (title.Prefix != string.Empty)
            {
                int prefixIndex = fullTitle.ToLower().IndexOf(title.Prefix.ToLower());
                fullTitle = fullTitle.Substring(prefixIndex + title.Prefix.Length);
                //Console.WriteLine("Prefix: " + title.Prefix + ", at: " + prefixIndex.ToString());
            }

            return fullTitle;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\", \"{1}\"", Prefix, Suffix);
        }

        public string ToString(string video)
        {
            return Prefix + video + Suffix;
        }
    }
}
