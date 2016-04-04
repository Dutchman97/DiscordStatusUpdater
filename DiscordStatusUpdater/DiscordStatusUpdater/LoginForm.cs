using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class LoginForm : Form
    {
        bool pressed = false;
        bool loggedIn = false;
        const int TIMEOUT = 10;

        public LoginForm()
        {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Email;
            textBox2.Text = Properties.Settings.Default.Password;
            checkBox1.Checked = Properties.Settings.Default.Remember;
            Properties.Settings.Default.Save();

            LoadPlayers();
        }

        private void LoadPlayers()
        {
            //FileStream fileStream = new FileStream("Players.xml", FileMode.Open);
            //XmlReader xmlReader = XmlReader.Create(fileStream);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Players.xml");
            XmlNodeList xmlPlayers = xmlDocument.GetElementsByTagName("player");

            //Player[] players = new Player[xmlPlayers.Count];
            Properties.Settings.Default.Players.Clear();
            for (int i = 0; i < xmlPlayers.Count; i++)
            {
                Player player = new Player(xmlPlayers[i]);
                Properties.Settings.Default.Players.Add(player);
                Console.WriteLine("Added player " + player.ToString());
            }

            /*Player[] players = new Player[] {
                new Player("VLC", "vlc", "", " - VLC media player"),
                new Player("MPC-HC", "mpc-hc", "", ""),
                new Player("MPC-BE", "mpc-be", "", " - MPC-BE")
            };

            foreach (Player p in players)
                if (!Properties.Settings.Default.Players.Contains(p))
                    Properties.Settings.Default.Players.Add(p);*/

            string[] extensions = new string[] { "mkv", "mp4", "avi" };
            Properties.Settings.Default.Extensions = new ArrayList(extensions);

            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DiscordClient client = new DiscordClient();
            MainForm main;
            try
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                this.Text = "Logging in...";

                client.LoggedIn += (sender1, e1) => loggedIn = true;
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
                    while (!loggedIn)
                    {
                        System.Threading.Thread.Sleep(5);

                        var delta = DateTime.Now - start;
                        if (delta >= TimeSpan.FromSeconds(TIMEOUT))
                            throw new TimeoutException("Login timed out. Either the email/password provided is incorrect or the program can not connect to Discord.");
                        else if (delta <= TimeSpan.Zero)
                            throw new Exception("The time on this computer was changed.");
                    }

                    this.Hide();
                    main = new MainForm(client);
                    main.Owner = this;
                    main.Show();
                }
                else
                {
                    throw new Exception("Login failed.");
                }
            }
            catch (Exception ex)
            {
                client.Dispose();
                client = null;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                main = null;
                this.Text = "DiscordStatusUpdater";
                MessageBox.Show(ex.Message, "Failed to login", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
