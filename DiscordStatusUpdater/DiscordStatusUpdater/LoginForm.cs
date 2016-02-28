using System;
using System.Collections;
using System.Windows.Forms;
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

            Player[] players = new Player[] {
                new Player("VLC", "vlc", "", " - VLC media player"),
                new Player("MPC-HC", "mpc-hc", "", ""),
                new Player("MPC-BE", "mpc-be", "", " - MPC-BE")
            };

            foreach (Player p in players)
                if (!Properties.Settings.Default.Players.Contains(p))
                    Properties.Settings.Default.Players.Add(p);

            string[] extensions = new string[] { "mkv", "mp4", "avi" };
            Properties.Settings.Default.Extensions = new ArrayList(extensions);

            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DiscordClient client = new DiscordClient();
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
                            throw new TimeoutException("Login timed out");
                        else if (delta <= TimeSpan.Zero)
                            throw new Exception("Time changed error");
                    }

                    this.Hide();
                    MainForm main = new MainForm(client);
                    main.Owner = this;
                    main.Show();
                }
                else
                {
                    MessageBox.Show("Login failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    throw new Exception("Login failed");
                }
            }
            catch (Exception)
            {
                client.Dispose();
                client = null;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                this.Text = "DiscordStatusUpdater";
                MessageBox.Show("Your email and/or password is incorrect", "Failed to login", MessageBoxButtons.OK);
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
