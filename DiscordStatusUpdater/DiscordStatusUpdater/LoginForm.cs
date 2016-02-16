using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Email;
            textBox2.Text = Properties.Settings.Default.Password;

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
            string[] loginInfo = File.ReadAllLines(@"C:\Users\Kevin\Desktop\login.txt");
            bool success;

            DiscordClient client = new DiscordClient();
            try
            {
                await client.Connect(textBox1.Text, textBox2.Text);
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }

            if (success && (client.State == ConnectionState.Connected || client.State == ConnectionState.Connecting))
            {
                Properties.Settings.Default.Email = textBox1.Text;
                Properties.Settings.Default.Password = textBox2.Text;
                Properties.Settings.Default.Save();

                textBox2.Text = "";
                this.Hide();
                new Form1(client).ShowDialog();
            }
            else
            {
                MessageBox.Show("Your email and/or password is incorrect", "Failed to login", MessageBoxButtons.OK);
            }
        }
    }
}
