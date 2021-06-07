using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using Json.Net;
using System.Diagnostics;
using System.IO;
namespace DiscordLookup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region FormMoveable
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
        #endregion
        private void button1_Click(object sender, EventArgs e) => GetDiscordUser(textBox1.Text);
        private void pictureBox1_Click(object sender, EventArgs e) => GetDiscordUser(Clipboard.GetText());
        private void button2_Click(object sender, EventArgs e) => Process.GetCurrentProcess().Kill();
        private void button3_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;
        private void GetDiscordUser(string UserID)
        {
            //TryParse is just a shorter way of a try catch
            if (ulong.TryParse(UserID, out _))
            {
                textBox1.Text = UserID;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add(HttpRequestHeader.Authorization, "Bot " + GetToken());
                    string data = web.DownloadString($"https://discord.com/api/v8/users/{textBox1.Text}");
                    usr = JsonNet.Deserialize<DiscordUser>(data);
                    usr.avatar = $"https://cdn.discordapp.com/avatars/{usr.id}/{usr.avatar}";
                    usr.ConvertedFlags = CalculateUserFlags(usr.public_flags);
                    pictureBox2.ImageLocation = usr.avatar;
                    label2.Text = $"ID: {usr.id}\nUsername: {usr.username}#{usr.discriminator}\nUser Flags: {usr.ConvertedFlags}";
                }
            }
            else
            {
                textBox1.Clear();
                MessageBox.Show("That isn't a valid Discord User ID!", "DiscordLookup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } 
        }
        DiscordUser usr = new DiscordUser();
        public class DiscordUser
        {
            public string id { get; set; }
            public string username { get; set; }
            public string avatar { get; set; }
            public string discriminator { get; set; }
            public int public_flags { get; set; }
            public string ConvertedFlags { get; set; }//Self defined, not defined by discord
        }

        private string CalculateUserFlags(int number)
        {
            //https://discord.com/developers/docs/resources/user#user-object-user-flags
            switch (number)
            {
                //case 0: return "None";     //Default case will handle this
                case 1 << 0: return "Discord Employee";
                case 1 << 1: return "Partnered Server Owner";
                case 1 << 2: return "HypeSquad Events";   
                case 1 << 3: return  "Bug Hunter Level 1"; 
                case 1 << 6: return  "House Bravery"; 
                case 1 << 7: return  "House Brilliance";
                case 1 << 8: return  "House Balance";  
                case 1 << 9: return  "Early Supporter";
                case 1 << 10: return  "Team User";
                case 1 << 14: return  "Bug Hunter Level 2";
                case 1 << 16: return  "Verified Bot"; 
                case 1 << 17: return  "Early Verified Bot Developer";  
                case 1 << 18: return  "Discord Certified Moderator";
                default: return "None";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (usr.id != null)
            {
                string filename = usr.id + "_export.txt";
                File.WriteAllText(filename, $"UserID={usr.id}\nUsername={usr.username}\nDiscriminator={usr.discriminator}\nAvatar={usr.avatar}\nPublicFlags={usr.public_flags}\nUserFlags={usr.ConvertedFlags}");
                Process.Start($"notepad.exe", $"{Path.GetFullPath(filename)}");
            }
            else
                MessageBox.Show("That isn't a valid Discord User ID!", "DiscordLookup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        public static string GetToken()
        {
            string tkns = "eyJ0b2tlbnMiOlsiT0RVeE1ETTNOVFk1TkRRNE9EQTBNelV5LllMeWNnQS5XdU1xaUR6d1lBZnBQMm9tVmM1aEZEcV9PbDQiLCJPRFV4TURRMU56a3hOREF4TVRFMU5qVTQuWUx5a0tBLmxyOEIxaXJncW15dWwyQ0t3LWNtVkhKbjdlbyIsIk9EVXhNRFExT1RFNU5qazNNREV3TmpnNC5ZTHlrUmcuZ0IwWlBhaDhtdGl2ZnBjaVRRbUdQbWdjVTBNIl19";
            string[] tokens = JsonNet.Deserialize<Token>(Base64Decode(tkns)).tokens;

            return tokens[new Random().Next(0, tokens.Length)];
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public class Token
        {
            public string[] tokens { get; set; }
        }
    }
}
