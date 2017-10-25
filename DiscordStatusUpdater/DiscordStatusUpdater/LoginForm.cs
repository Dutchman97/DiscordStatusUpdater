using System;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using System.Linq;
using Gecko;
using Gecko.Events;
//using CefSharp;
//using CefSharp.WinForms;

namespace DiscordStatusUpdater
{
    public partial class LoginForm : Form
    {
        bool pressed = false;
        const int TIMEOUT = 10;
        const int PLAYER_MAJOR_VERSION = 1, PLAYER_MINOR_VERSION = 1;
        const int WEBSITE_MAJOR_VERSION = 1, WEBSITE_MINOR_VERSION = 0;
        //const string SETTINGS_FILE_PATH = "settings";
        string state;

        public LoginForm()
        {
            this.InitializeComponent();

            Xpcom.Initialize("Firefox");

            string clientId = "372387845333057538";
            this.state = this.RandomString(12);
            string url = "https://discordapp.com/api/oauth2/authorize?response_type=code&client_id=" + clientId +
                "&scope=identify&state=" + this.state +
                "&redirect_uri=http%3A%2F%2Flocalhost%3A5000";

            this.geckoWebBrowser1.Navigate(url);
            this.geckoWebBrowser1.Navigating += this.GeckoWebBrowser1_Navigating;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void GeckoWebBrowser1_Navigating(object sender, GeckoNavigatingEventArgs e) {
            Uri url = e.Uri;
            string urlString = url.AbsoluteUri;

            Debug.WriteLine("Uri: " + urlString);
            Debug.WriteLine("Host: " + url.Host);
            if (url.Host != "localhost") {
                return;
            }

            Debug.WriteLine("Fragment: " + url.Fragment);
            Debug.WriteLine("Query: " + url.Query);

            try {
                geckoWebBrowser1.Stop();
                string[] queryParams = url.Query.Substring(1).Split('&');
                string state = "", token = "";
                foreach (string param in queryParams) {
                    string[] split = param.Split('=');

                    if (split.Length != 2) {
                        throw new Exception("Weird parameter in query received." + Environment.NewLine +
                            "Parameter: " + param + Environment.NewLine +
                            "Full URI:" + urlString
                        );
                    }

                    string attr = split[0];
                    string val = split[1];
                    Debug.WriteLine(attr + ": " + val);

                    switch (attr) {
                        case "state":
                            state = val;
                            break;
                        case "code":
                            token = val;
                            break;
                        default:
                            break;
                    }
                }

                if (state != this.state) {
                    throw new Exception("Invalid authorization request: incorrect state received." + Environment.NewLine +
                        "Received state: " + state + Environment.NewLine +
                        "Actual state: " + this.state
                    );
                }

                DiscordSocketClient client = new DiscordSocketClient();
                client.LoginAsync(TokenType.Bearer, token).Wait();
                client.StartAsync().Wait();

                if (client.ConnectionState == ConnectionState.Connected || client.ConnectionState == ConnectionState.Connecting) {
                    MainForm main = new MainForm(client);
                    this.Hide();
                    main.Show(this);
                }
                else {
                    throw new Exception("Login failed.");
                }
            }
            catch (Discord.Net.HttpException) {
                MessageBox.Show("Could not connect to Discord.", "Failed to login", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            catch (Exception ex) {
                MessageBox.Show(
                    ex.Message + Environment.NewLine + Environment.NewLine +
                    "Stacktrace (show to a developer): " + Environment.NewLine +
                    ex.StackTrace,
                    "Failed to login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1
                );

                Debug.WriteLine(ex.ToString());
            }
        }

        private string RandomString(int length) {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            random.GetBytes(bytes);
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToLower();

            string result = "";
            for (int i = 0; i < length; i++) {
                int charIndex = (int)((float)bytes[i] / 255.0f * (float)length);
                result += chars[charIndex];
            }
            return result;
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e) {
            //Cef.Shutdown();
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
