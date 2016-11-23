using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace DiscordStatusUpdater.Players
{
    public static class VideoPlayerLoader
    {
        const int PLAYER_MAJOR_VERSION = 1, PLAYER_MINOR_VERSION = 2;
        const string PLAYERS_FILE_PATH = "", PLAYERS_FILE_NAME = "Players.xml";

        public static bool LoadPlayers(List<Player> players)
        {
            XmlDocument xmlDocument = new XmlDocument();

            // Load the Players.xml file
            if (File.Exists(PLAYERS_FILE_PATH + PLAYERS_FILE_NAME))
                xmlDocument.Load(PLAYERS_FILE_PATH + PLAYERS_FILE_NAME);
            else
                return false;

            // Check version of Players.xml file
            string version = xmlDocument.ChildNodes[1].Attributes["version"].Value;
            if (!CheckVersion(version)) return false;
            
            // Create the video players
            XmlNodeList xmlPlayers = xmlDocument.GetElementsByTagName("player");

            //Properties.Settings.Default.Players.Clear();
            for (int i = 0; i < xmlPlayers.Count; i++)
            {
                VideoPlayer videoPlayer = LoadVideoPlayer(xmlPlayers[i]);
                players.Add(videoPlayer);
                Debug.WriteLine("Added video player: " + videoPlayer.ToString());
            }

            // Return success
            return true;
        }

        static VideoPlayer LoadVideoPlayer(XmlNode xmlVideoPlayer)
        {
            string titlePrefix = xmlVideoPlayer["titleprefix"].InnerText;
            string titlesuffix = xmlVideoPlayer["titlesuffix"].InnerText;
            Regex videoRegex = new Regex(@"(?<=" + titlePrefix + @").+(?=(\.\w{1,4})?" + titlesuffix + @")");

            string playerName = xmlVideoPlayer.Attributes["name"].Value;
            
            XmlNodeList xmlProcessNames = ((XmlElement)xmlVideoPlayer).GetElementsByTagName("filename");
            //XmlNodeList xmlProcessNames = xmlVideoPlayer.SelectNodes("/filename");
            string[] processNames = new string[xmlProcessNames.Count];
            for (int i = 0; i < xmlProcessNames.Count; i++)
                processNames[i] = xmlProcessNames[i].InnerText;

            return new VideoPlayer(processNames, playerName, new TitleParser(titlePrefix, titlesuffix));
        }

        static bool CheckVersion(string version)
        {
            // Get the major and minor versions of the Players.xml file
            if (version == string.Empty) version = "1.0";
            string[] versionSplit = version.Split('.');
            int major_version = int.Parse(versionSplit[0]), minor_version = int.Parse(versionSplit[1]);

            Debug.WriteLine(PLAYERS_FILE_NAME + " file: " + version);
            Debug.WriteLine("Supported version: {0}.{1}", PLAYER_MAJOR_VERSION, PLAYER_MINOR_VERSION);
            Debug.WriteLine(PLAYERS_FILE_NAME + " version: {0}.{1}", major_version, minor_version);

            // Compare the Player.xml version to the supported version
            if (PLAYER_MAJOR_VERSION < major_version || PLAYER_MAJOR_VERSION > major_version)
            {
                string s = PLAYER_MAJOR_VERSION < major_version ? "newer" : "older";
                MessageBox.Show("Error: this version of the " + PLAYERS_FILE_NAME + " file is " + s + " than the version supported.",
                    "Unsupported " + PLAYERS_FILE_NAME + " file", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Return failure
                return false;
            }
            else if (PLAYER_MINOR_VERSION < minor_version || PLAYER_MINOR_VERSION > minor_version)
            {
                string s = PLAYER_MINOR_VERSION < minor_version ? "newer" : "older";
                DialogResult result = MessageBox.Show("Warning: this version of the " + PLAYERS_FILE_NAME + " file is " + s + " than the version supported. Things may not work correctly. Continue?",
                    "Unsupported " + PLAYERS_FILE_NAME + " file", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    // Return failure
                    return false;
            }

            // If major and minor versions were the same, return success
            return true;
        }
    }
}
