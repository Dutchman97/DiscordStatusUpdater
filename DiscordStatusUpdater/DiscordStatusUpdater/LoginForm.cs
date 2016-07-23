using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class LoginForm : Form
    {
        bool pressed = false;
        const int TIMEOUT = 10;
        const int PLAYER_MAJOR_VERSION = 1, PLAYER_MINOR_VERSION = 1;
        const int WEBSITE_MAJOR_VERSION = 1, WEBSITE_MINOR_VERSION = 0;

        public LoginForm()
        {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Email;
            textBox2.Text = Properties.Settings.Default.Password;
            checkBox1.Checked = Properties.Settings.Default.Remember;
            Properties.Settings.Default.Save();

            if (!(LoadPlayers() && LoadWebsites()))
                Application.Exit();
        }

        private bool LoadPlayers()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Players.xml");

            // Check version of Players.xml file
            string version = xmlDocument.ChildNodes[1].Attributes["version"].Value;
            if (!CheckVersion("Players.xml", version)) return false;

            XmlNodeList xmlPlayers = xmlDocument.GetElementsByTagName("player");

            Properties.Settings.Default.Players.Clear();
            for (int i = 0; i < xmlPlayers.Count; i++)
            {
                Player player = new Player(xmlPlayers[i]);
                Properties.Settings.Default.Players.Add(player);
                Console.WriteLine("Added player " + player.ToString());
            }

            string[] extensions = new string[] { "mkv", "mp4", "avi" };
            Properties.Settings.Default.Extensions = new ArrayList(extensions);

            Properties.Settings.Default.Save();

            // Return success
            return true;
        }

        private bool LoadWebsites()
        {
            return true;

            // |------------|   | | |
            // | UNFINISHED |   | | |
            // |------------|   O O O

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Websites.xml");

            // Check version of Websites.xml file
            string version = xmlDocument.ChildNodes[1].Attributes["version"].Value;
            if (!CheckVersion("Websites.xml", version)) return false;

            XmlNodeList xmlWebsites = xmlDocument.GetElementsByTagName("website");

            Properties.Settings.Default.Websites.Clear();
            for (int i = 0; i < xmlWebsites.Count; i++)
            {
                
            }
        }

        private bool CheckVersion(string name, string version)
        {
            if (version == string.Empty) version = "1.0";
            Console.WriteLine(name + " file: " + version);
            Console.WriteLine("Supported version: {0}.{1}", PLAYER_MAJOR_VERSION, PLAYER_MINOR_VERSION);
            string[] versionSplit = version.Split('.');
            int major_version = int.Parse(versionSplit[0]), minor_version = int.Parse(versionSplit[1]);

            if (PLAYER_MAJOR_VERSION == major_version && PLAYER_MINOR_VERSION < minor_version)
            {
                DialogResult result = MessageBox.Show("Warning: this version of the " + name + " file is newer than the version supported. Things may not work correctly. Continue?",
                    "Unsupported " + name + " file", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    // Return failure
                    return false;
            }
            else if (PLAYER_MAJOR_VERSION < major_version)
            {
                MessageBox.Show("Warning: this version of the " + name + " file is newer than the version supported.",
                    "Unsupported " + name + " file", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Return failure
                return false;
            }

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DiscordClient client = new DiscordClient();

            try
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                this.Text = "Logging in...";

                client.Connect(textBox1.Text, textBox2.Text);

                if (client.State == ConnectionState.Connected || client.State == ConnectionState.Connecting)
                {
                    if (!checkBox1.Checked)
                    {
                        textBox1.Text = "";
                        textBox2.Text = "";
                    }
                    Properties.Settings.Default.Email = textBox1.Text;
                    Properties.Settings.Default.Password = textBox2.Text;
                    Properties.Settings.Default.Remember = checkBox1.Checked;
                    Properties.Settings.Default.Save();

                    // The best way to wait of course
                    DateTime start = DateTime.Now;
                    while (client.State != ConnectionState.Connected)
                    {
                        System.Threading.Thread.Sleep(10);

                        var delta = DateTime.Now - start;
                        if (delta >= TimeSpan.FromSeconds(TIMEOUT))
                            throw new TimeoutException("Login timed out. Either the email/password provided is incorrect or the program can not connect to Discord.");
                        else if (delta <= TimeSpan.Zero)
                            throw new Exception("The time on this computer was changed.");
                    }

                    MainForm main = new MainForm(client);
                    main.Owner = this;
                    main.Show();
                    this.Hide();
                }
                else
                {
                    throw new Exception("Login failed.");
                }
            }
            catch (Exception ex)
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                this.Text = "DiscordStatusUpdater";
                MessageBox.Show(ex.Message, "Failed to login", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Console.WriteLine(ex.ToString());
                client.Dispose();
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !pressed)
            {
                pressed = true;
                button1_Click(sender, (EventArgs)e);
            }
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                pressed = false;
        }
    }
}
