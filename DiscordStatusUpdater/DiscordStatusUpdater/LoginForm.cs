using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class LoginForm : Form
    {
        bool pressed = false;

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

        async private void button1_Click(object sender, EventArgs e)
        {
            bool success;

            DiscordClient client = new DiscordClient();
            try
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                await client.Connect(textBox1.Text, textBox2.Text);
                success = true;
            }
            catch (Exception)
            {
                success = false;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
            }

            if (success && (client.State == ConnectionState.Connected || client.State == ConnectionState.Connecting))
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

                textBox2.Text = "";
                this.Hide();
                MainForm main = new MainForm(client);
                main.Owner = this;
                main.Show();
            }
            else
            {
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
