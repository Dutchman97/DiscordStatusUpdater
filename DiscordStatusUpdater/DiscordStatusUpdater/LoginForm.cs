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
        }

        bool button1Clicked = false;
        object o = new object();
        private async void button1_Click(object sender, EventArgs e)
        {
            lock (o)
            {
                if (button1Clicked) return;
                else button1Clicked = true;
            }

            DiscordClient client = new DiscordClient();

            try
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                this.Text = "Logging in...";

                await client.Connect(textBox1.Text, textBox2.Text);

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

                    /*
                    // The best way to wait of course
                    DateTime start = DateTime.Now;
                    while (client.State != ConnectionState.Connected)
                    {
                        System.Threading.Thread.Sleep(10);

                        var delta = DateTime.Now - start;
                        if (delta >= TimeSpan.FromSeconds(TIMEOUT))
                            throw new TimeoutException("Login timed out. Either the email/password provided is incorrect or the program can not connect to Discord.");
                        else if (delta < TimeSpan.Zero)
                            throw new Exception("The time on this computer was changed.");
                    }
                    */

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
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace, "Failed to login", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Console.WriteLine(ex.ToString() + Environment.NewLine + ex.StackTrace);
                client.Dispose();
            }

            lock (o)
            {
                button1Clicked = false;
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
