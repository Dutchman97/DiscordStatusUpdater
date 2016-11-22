using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace DiscordStatusUpdater
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            this.Size = new Size(300, 303);
            Reset();
            helpLabel.Location = new Point(videoPlayerTitleLabel.Location.X + videoPlayerTitleLabel.Size.Width - 3, videoPlayerTitleLabel.Location.Y + 1);
        }

        public void Reset()
        {
            playerList.Items.Clear();
            playerList.Items.AddRange(Properties.Settings.Default.Players.ToArray());
            playerList.Items.Add("<Add a new video player...>");
            playerList.SelectedIndex = 0;
        }

        private void SettingsForm_Move(object sender, EventArgs e)
        {
            if (Owner != null)
                Owner.Location = new Point(this.DesktopLocation.X - Owner.Width + 15, this.DesktopLocation.Y);
        }

        private void playerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
            Console.WriteLine(playerList.SelectedIndex + ": " + playerList.SelectedItem);

            if (playerList.SelectedItem is Player)
            {
                nameTextBox.Text = ((Player)playerList.SelectedItem).Name;
                fileNameTextBox.Text = ((Player)playerList.SelectedItem).FileName;
                titlePrefixTextBox.Text = ((Player)playerList.SelectedItem).Title.Prefix;
                titleSuffixTextBox.Text = ((Player)playerList.SelectedItem).Title.Suffix;
            }
            else
            {
                nameTextBox.Text = string.Empty;
                fileNameTextBox.Text = string.Empty;
                titlePrefixTextBox.Text = string.Empty;
                titleSuffixTextBox.Text = string.Empty;
            }
            */
        }

        private void UpdatePreviewTextBox()
        {
            previewTextBox.Text = titlePrefixTextBox.Text + "Example_video.mkv" + titleSuffixTextBox.Text;
        }

        private void titleTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdatePreviewTextBox();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            /*
            nameTextBox.Text = nameTextBox.Text.Trim();
            fileNameTextBox.Text = fileNameTextBox.Text.Trim();

            // If the video player has no name, color the relevant text box red and return
            if (nameTextBox.Text == string.Empty)
            {
                nameTextBox.BackColor = Color.Salmon;
                return;
            }
            else
                nameTextBox.BackColor = Color.White;

            // If the video player has no file name, color the relevant text box red and return
            if (fileNameTextBox.Text == string.Empty)
            {
                fileNameTextBox.BackColor = Color.Salmon;
                return;
            }
            else
                fileNameTextBox.BackColor = Color.White;

            // Remove .exe from the file name if possible
            if (fileNameTextBox.Text.Contains(".exe"))
                fileNameTextBox.Text = fileNameTextBox.Text.Replace(".exe", string.Empty);

            Player player = new Player(nameTextBox.Text, fileNameTextBox.Text, titlePrefixTextBox.Text, titleSuffixTextBox.Text, PlayerType.Player);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Players.xml");

            if (playerList.SelectedItem is Player)
            {
                // If an existing player is being modified:

                Properties.Settings.Default.Players[playerList.SelectedIndex] = player;

                XmlNodeList xmlPlayers = xmlDocument.GetElementsByTagName("player");
                XmlNode xmlPlayer = xmlPlayers[playerList.SelectedIndex];

                xmlPlayer.Attributes["name"].Value = player.Name;
                xmlPlayer["filename"].InnerText = player.FileName;
                xmlPlayer["titleprefix"].InnerText = player.Title.Prefix;
                xmlPlayer["titlesuffix"].InnerText = player.Title.Suffix;

            }
            else
            {
                // If a new player is being added:

                Properties.Settings.Default.Players.Add(player);

                XmlElement xmlPlayer = xmlDocument.CreateElement("player");

                XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("name");
                xmlAttribute.Value = player.Name;
                xmlPlayer.SetAttributeNode(xmlAttribute);

                XmlElement fileName = xmlDocument.CreateElement("filename");
                fileName.InnerText = player.FileName;
                xmlPlayer.AppendChild(fileName);

                XmlElement titlePrefix = xmlDocument.CreateElement("titleprefix");
                titlePrefix.InnerText = player.Title.Prefix;
                xmlPlayer.AppendChild(titlePrefix);

                XmlElement titleSuffix = xmlDocument.CreateElement("titlesuffix");
                titleSuffix.InnerText = player.Title.Suffix;
                xmlPlayer.AppendChild(titleSuffix);

                xmlDocument["players"].AppendChild(xmlPlayer);
            }

            xmlDocument.Save("Players.xml");
            Properties.Settings.Default.Save();

            int i = playerList.SelectedIndex;
            Reset();
            playerList.SelectedIndex = i;
            */
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Reset();
            this.Hide();
            if (!Owner.Visible)
                Owner.Show();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            /*
            if (!(playerList.SelectedItem is Player))
                return;

            DialogResult result = MessageBox.Show("Are you sure you want to delete this video player and stop tracking it?", "Delete video player",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Properties.Settings.Default.Players.Remove(playerList.SelectedItem);

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load("Players.xml");
                XmlNodeList xmlPlayers = xmlDocument.GetElementsByTagName("player");
                XmlNode xmlPlayer = xmlPlayers[playerList.SelectedIndex];
                xmlDocument.DocumentElement.RemoveChild(xmlPlayer);
                xmlDocument.Save("Players.xml");
                Reset();
            }
            */
        }
    }
}
