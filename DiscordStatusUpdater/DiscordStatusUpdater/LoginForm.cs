using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using Gecko;
using Gecko.Events;
using Newtonsoft.Json;

namespace DiscordStatusUpdater {

    public partial class LoginForm : Form {

        bool pressed = false;
        const int TIMEOUT = 10;
        const int PLAYER_MAJOR_VERSION = 1, PLAYER_MINOR_VERSION = 1;
        const int WEBSITE_MAJOR_VERSION = 1, WEBSITE_MINOR_VERSION = 0;
        const string SETTINGS_FILE_PATH = "settings";
        string state;
        static readonly HttpClient httpClient = new HttpClient();
        const string clientId = "372387845333057538";
        string clientSecret;
        const string redirectUri = "http://localhost:5000";
        const bool implicitGrant = false;

        public LoginForm() {
            this.InitializeComponent();

            Xpcom.Initialize("Firefox");

            //ClearCookies();

            this.state = this.RandomString(19);
            string scope = "identify";
            string responseType = implicitGrant ? "token" : "code";

            clientSecret = File.ReadAllText(SETTINGS_FILE_PATH);

            string url = "https://discordapp.com/api/v6/oauth2/authorize" +
                "?response_type=" + responseType +
                "&client_id=" + clientId +
                "&scope=" + scope +
                "&state=" + this.state +
                "&redirect_uri=" + Uri.EscapeDataString(redirectUri);

            this.geckoWebBrowser1.Navigate(url);
            this.geckoWebBrowser1.Navigating += this.GeckoWebBrowser1_Navigating;
        }

        private void ClearCookies() {
            nsICookieManager cookieMan;
            cookieMan = Xpcom.GetService<nsICookieManager>("@mozilla.org/cookiemanager;1");
            cookieMan = Xpcom.QueryInterface<nsICookieManager>(cookieMan);
            cookieMan.RemoveAll();
        }

        private void button1_Click(object sender, EventArgs e) {

        }

        private void GeckoWebBrowser1_Navigating(object sender, GeckoNavigatingEventArgs e) {
            Uri uri = e.Uri;
            string uriString = uri.AbsoluteUri;

            Debug.WriteLine("Uri: " + uriString);
            Debug.WriteLine("Host: " + uri.Host);
            if (uri.Host != "localhost") {
                return;
            }

            try {
                this.geckoWebBrowser1.Stop();

                string token, tokenType;
                if (implicitGrant)
                    this.ImplicitGrant(uri, out token, out tokenType);
                else
                    this.AuthorizationCodeGrant(uri, out token, out tokenType);

                if (tokenType != "Bearer")
                    throw new Exception("Invalid token type: " + tokenType);

                DiscordSocketClient client = new DiscordSocketClient();// new DiscordSocketConfig() { LogLevel = LogSeverity.Verbose });
                //client.Log += this.Client_Log;
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

        private void ImplicitGrant(Uri uri, out string token, out string tokenType) {
            string state = null;
            token = null;
            tokenType = null;

            string[] pairs = uri.Fragment.Substring(1).Split('&');
            foreach (string pair in pairs) {
                string[] split = pair.Split('=');

                if (split.Length != 2) {
                    throw new Exception("Weird parameter in query received." + Environment.NewLine +
                        "Parameter: " + pair + Environment.NewLine +
                        "Full URI:" + uri.AbsoluteUri
                    );
                }

                string key = split[0];
                string value = split[1];
                Debug.WriteLine(key + ": " + value);

                switch (key) {
                    case "state":
                        state = value;
                        break;
                    case "access_token":
                        token = value;
                        break;
                    case "token_type":
                        tokenType = value;
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
        }

        private void AuthorizationCodeGrant(Uri uri, out string token, out string tokenType) {
            this.GetStateAndCode(uri, out string state, out string code);

            if (state != this.state) {
                throw new Exception("Invalid authorization request: incorrect state received." + Environment.NewLine +
                    "Received state: " + state + Environment.NewLine +
                    "Actual state: " + this.state
                );
            }

            Dictionary<string, string> attributes = this.ExchangeCode(code);

            token = attributes["access_token"];
            Debug.WriteLine("Token: " + token);

            tokenType = attributes["token_type"];
        }

        private void GetStateAndCode(Uri uri, out string state, out string code) {
            state = null;
            code = null;

            string[] pairs = uri.Query.Substring(1).Split('&');
            foreach (string pair in pairs) {
                string[] split = pair.Split('=');

                if (split.Length != 2) {
                    throw new Exception("Weird parameter in query received." + Environment.NewLine +
                        "Parameter: " + pair + Environment.NewLine +
                        "Full URI:" + uri.AbsoluteUri
                    );
                }

                string key = split[0];
                string value = split[1];
                Debug.WriteLine(key + ": " + value);

                switch (key) {
                    case "state":
                        state = value;
                        break;
                    case "code":
                        code = value;
                        break;
                    default:
                        break;
                }
            }
        }

        private Dictionary<string, string> ExchangeCode(string code) {
            var values = new Dictionary<string, string> {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri }
                };

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            HttpResponseMessage response = httpClient.PostAsync("https://discordapp.com/api/v6/oauth2/token", content).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Debug.WriteLine("Response:" + Environment.NewLine + responseString);

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
        }

        private Task Client_Log(LogMessage arg) {
            Debug.WriteLine("Log: " + arg.ToString());
            return new Task(() => { });
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

        private void textBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && !pressed) {
                pressed = true;
                button1_Click(sender, (EventArgs)e);
            }
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
                pressed = false;
        }
    }
}