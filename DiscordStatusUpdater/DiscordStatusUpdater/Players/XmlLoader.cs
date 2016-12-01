using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace DiscordStatusUpdater.Players
{
    public abstract class XmlLoader<T>
    {
        protected XmlDocument xmlDocument;
        string filePath, fileName;
        int majorVersion, minorVersion;
        string itemName;

        public XmlLoader(string filePath, string fileName, int majorVersion, int minorVersion, string itemName)
        {
            xmlDocument = new XmlDocument();
            this.filePath = filePath;
            this.fileName = fileName;
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
            this.itemName = itemName;
        }

        public bool LoadItems(List<T> items)
        {
            // Load the xml file
            if (!LoadXmlDocument())
                return false;

            // Create the items
            XmlNodeList xmlItems = xmlDocument.GetElementsByTagName(itemName);
            for (int i = 0; i < xmlItems.Count; i++)
            {
                T item = LoadItem(xmlItems[i]);
                items.Add(item);
                Debug.WriteLine("Added item: " + item.ToString());
            }

            // Return success
            return true;
        }

        protected abstract T LoadItem(XmlNode item);

        bool LoadXmlDocument()
        {
            // Load the xml file
            if (File.Exists(filePath + fileName))
            {
                xmlDocument.Load(filePath + fileName);

                // Check the version
                string version = xmlDocument.ChildNodes[1].Attributes["version"].Value;
                if (CheckVersion(version, filePath, fileName, majorVersion, minorVersion))
                    return true;
            }

            return false;
        }

        protected bool CheckVersion(string version, string filePath, string fileName, int majorVersion, int minorVersion)
        {
            // Get the major and minor versions of the Players.xml file
            if (version == string.Empty) version = "1.0";
            string[] versionSplit = version.Split('.');
            int xmlMajorVersion = int.Parse(versionSplit[0]), xmlMinorVersion = int.Parse(versionSplit[1]);

            Debug.WriteLine(fileName + " file: " + version);
            Debug.WriteLine("Supported version: {0}.{1}", majorVersion, minorVersion);
            Debug.WriteLine(fileName + " version: {0}.{1}", xmlMajorVersion, xmlMinorVersion);

            // Compare the Player.xml version to the supported version
            if (majorVersion < xmlMajorVersion || majorVersion > xmlMajorVersion)
            {
                string s = majorVersion < xmlMajorVersion ? "newer" : "older";
                MessageBox.Show("Error: this version of the " + fileName + " file is " + s + " than the version supported.",
                    "Unsupported " + fileName + " file", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Return failure
                return false;
            }
            else if (minorVersion < xmlMinorVersion || minorVersion > xmlMinorVersion)
            {
                string s = minorVersion < xmlMinorVersion ? "newer" : "older";
                DialogResult result = MessageBox.Show("Warning: this version of the " + fileName + " file is " + s + " than the version supported. Things may not work correctly. Continue?",
                    "Unsupported " + fileName + " file", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    // Return failure
                    return false;
            }

            // If major and minor versions were the same, return success
            return true;
        }
    }
}
