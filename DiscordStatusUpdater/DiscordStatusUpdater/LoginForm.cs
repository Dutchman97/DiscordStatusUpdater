using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Discord;

namespace DiscordStatusUpdater
{
    public partial class LoginForm : Form
    {
        bool pressed = false;
        const int TIMEOUT = 10;
        const int PLAYER_MAJOR_VERSION = 1, PLAYER_MINOR_VERSION = 1;
        const int WEBSITE_MAJOR_VERSION = 1, WEBSITE_MINOR_VERSION = 0;
        const string SETTINGS_FILE_PATH = "settings";

        public LoginForm()
        {
            InitializeComponent();

            var loginInfo = LoadLogin();
            if (loginInfo != null)
            {
                textBox1.Text = loginInfo.Item1;
                textBox2.Text = loginInfo.Item2;
                checkBox1.Checked = loginInfo.Item3;
            }
        }

        void SaveLogin(string email, string password, bool remember)
        {
            byte[] emailBytes = Encoding.UTF8.GetBytes(email);
            byte[] passBytes;
            byte rememberByte = remember == true ? (byte)1 : (byte)0;

            using (Aes aes = Aes.Create())
            {
                SetAesIVAndKey(aes, emailBytes);
                
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            swEncrypt.Write(password);
                        passBytes = msEncrypt.ToArray();
                    }
                }
            }

            byte[] output = new byte[emailBytes.Length + passBytes.Length + 2];
            Array.Copy(emailBytes, 0, output, 0, emailBytes.Length);
            Array.Copy(passBytes, 0, output, emailBytes.Length, passBytes.Length);
            output[emailBytes.Length + passBytes.Length] = rememberByte;
            output[emailBytes.Length + passBytes.Length + 1] = (byte)emailBytes.Length;
            
            File.WriteAllBytes(SETTINGS_FILE_PATH, output);
        }

        Tuple<string, string, bool> LoadLogin()
        {
            if (!File.Exists(SETTINGS_FILE_PATH))
                return null;

            byte[] input = File.ReadAllBytes(SETTINGS_FILE_PATH);
            byte[] emailBytes = new byte[input[input.Length - 1]];
            byte[] passEncBytes = new byte[input.Length - emailBytes.Length - 2];

            Array.Copy(input, 0, emailBytes, 0, emailBytes.Length);
            Array.Copy(input, emailBytes.Length, passEncBytes, 0, passEncBytes.Length);

            string email = Encoding.UTF8.GetString(emailBytes);
            string password;
            bool remember = input[input.Length - 2] == 1;

            using (Aes aes = Aes.Create())
            {
                SetAesIVAndKey(aes, emailBytes);

                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (MemoryStream msDecrypt = new MemoryStream(passEncBytes))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            password = srDecrypt.ReadToEnd();
            }

            return new Tuple<string, string, bool>(email, password, remember);
        }

        void SetAesIVAndKey(Aes aes, byte[] emailBytes)
        {
            // Using the plaintext email as encryption key is stupid,
            // but it's better than storing the password as plaintext
            byte[] key = new byte[aes.KeySize / 8];
            byte[] keyTemp = emailBytes;
            while (keyTemp.Length < key.Length)
            {
                byte[] keyTemp1 = new byte[keyTemp.Length * 2];
                Array.Copy(keyTemp, 0, keyTemp1, 0, keyTemp.Length);
                Array.Copy(keyTemp, 0, keyTemp1, keyTemp.Length, keyTemp.Length);
                keyTemp = keyTemp1;
            }
            Array.Copy(keyTemp, 0, key, 0, key.Length);
            aes.Key = key;

            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(keyTemp, keyTemp.Length - iv.Length, iv, 0, iv.Length);
            aes.IV = iv;
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

                    SaveLogin(textBox1.Text, textBox2.Text, checkBox1.Checked);

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
