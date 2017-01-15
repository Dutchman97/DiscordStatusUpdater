using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordStatusUpdater
{
    public class XmlDownloader
    {
        const string URL_BASE = "http://pastebin.com/raw/";
        const string KEY_VIDEOPLAYERS = "rbBk05Us";
        const string KEY_WEBBROWSERS = "hv0zB1qm";
        const string KEY_WEBSITES = "bHGcvm9i";

        public static bool DownloadFiles()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL_BASE);

            string[] keys = new string[3] { KEY_VIDEOPLAYERS, KEY_WEBBROWSERS, KEY_WEBSITES };
            string[] files = new string[3] { "Players.xml", "WebBrowsers.xml", "Websites.xml" };
            string[] responseStrings = new string[3];

            for (int i = 0; i < 3; i++)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, keys[i]);
                HttpResponseMessage response = client.SendAsync(request).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // NOTE: Do something with this result
                    DialogResult result = MessageBox.Show(response.StatusCode + Environment.NewLine + "Could not download latest " + files[i] + " file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                byte[] byteArray = response.Content.ReadAsByteArrayAsync().Result;
                string responseString = Encoding.UTF8.GetString(byteArray);

                if (string.IsNullOrEmpty(responseString))
                    return false;

                responseStrings[i] = responseString;
            }

            if (!Directory.Exists("cache"))
                Directory.CreateDirectory("cache");

            for (int i = 0; i < 3; i++)
                File.WriteAllText("cache/" + files[i], responseStrings[i]);

            return true;
        }
    }
}
