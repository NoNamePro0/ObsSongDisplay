using System;
using System.Linq;
using System.Threading;
using System.Windows;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using System.Timers;
using DiscordRPC;
using DiscordRPC.Logging;
using System.Windows.Forms;

namespace ObsSongDisplay
{
    public partial class MainWindow : MetroWindow
    {
        String pattern = "%author - %name";
        String interval = "20";

        readonly String config = "config.txt";
        readonly String output = "output.txt";

        const String messagePaused = "Playback paused!";
        const String messageAdvert = "Waiting for playback";
        const String messageNothing = "Nothing playing";

        readonly String VersionName = "SongFile V1.0";
        readonly String VersionTag = "1.0";

        private System.Windows.Forms.NotifyIcon notifyIcon;
        public DiscordRpcClient discord;

        public MainWindow()
        {
            InitializeComponent();

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(@"Resources/osdicon.ico");
            notifyIcon.MouseDoubleClick +=
                new System.Windows.Forms.MouseEventHandler
                    (notifyIcon_MouseDoubleClick);

            Header.Content = VersionName;

            DiscordLoad();
        }

        private void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(config, String.Empty);
            using (StreamWriter outputFile = new StreamWriter(config))
            {
                outputFile.WriteLine(textBox.Text);
                outputFile.WriteLine(intervalBox.Text);
            }

            Settings();
        }

        public void Settings()
        {
            if (!File.Exists(config))
            {
                File.WriteAllText(config, String.Empty);
                using (StreamWriter outputFile = new StreamWriter(config))
                {
                    outputFile.WriteLine(pattern);
                    outputFile.WriteLine(interval);
                }
            }

            pattern = File.ReadAllLines(config)[0];
            interval = File.ReadAllLines(config)[1];

            textBox.Text = pattern;
            intervalBox.Text = interval;
        }

        private void MetroWindow_Initialized(object sender, EventArgs e)
        {

        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !(System.Windows.MessageBox.Show("Are you sure? You'll completly close this App.", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes);
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon.BalloonTipTitle = "Minimized!";
                notifyIcon.BalloonTipText = "Double-click the Tray icon to get the UI back.";
                notifyIcon.ShowBalloonTip(1000);
                notifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                notifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        public void Timer()
        {
            System.Timers.Timer t = new System.Timers.Timer(TimeSpan.FromSeconds(int.Parse(interval)).TotalMilliseconds);
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler(Refresh);
            t.Start();
        }

        public void Refresh(object sender = null, ElapsedEventArgs e = null)
        {
            bool written = false;

            // Prionty
            written = Get(0, written); // Spotify
            written = Get(1, written); // VLC


            if (!written)
            {
                Set(messageNothing);
            }
        }

        private bool Get(int plattform, bool written)
        {
            if (!written)
            {
                switch (plattform)
                {
                    case 0: // Spotify
                        var procSpotify = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

                        if (!(procSpotify == null))
                        {
                            // Playback paused!
                            if (string.Equals(procSpotify.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Set(messagePaused);
                                written = true;
                            }

                            // Waiting for playback...
                            if (string.Equals(procSpotify.MainWindowTitle, "Advertisement", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Set(messageAdvert);
                                written = true;
                            }

                            // Playback paused!
                            if (string.Equals(procSpotify.MainWindowTitle, "Spotify Free", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Set(messagePaused);
                                written = true;
                            }

                            // Song playing!
                            if (!written)
                            {
                                Set(Parse(0, procSpotify.MainWindowTitle));
                                written = true;
                            }
                        }
                        break;
                    case 1: // VLC Media Player
                        var procVLC = Process.GetProcessesByName("VLC").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

                        if (!(procVLC == null))
                        {
                            // Playback paused!
                            if (procVLC.MainWindowTitle == "VLC media player")
                            {
                                Set(messagePaused);
                                written = true;
                            }

                            // Song playing!
                            if (!written)
                            {
                                if (procVLC.MainWindowTitle.EndsWith(" - VLC media player"))
                                {
                                    Set(Parse(1, procVLC.MainWindowTitle));
                                    written = true;
                                }
                            }
                        }
                        break;
                }
            }
            return written;
        }

        private void Set(String input)
        {
            // File
            File.WriteAllText(output, String.Empty);
            using (StreamWriter outputFile = new StreamWriter(output))
            {
                outputFile.WriteLine(input);
            }

            // UI
            this.Dispatcher.Invoke(() =>
            {
                song.Text = input;
            });

            // Discord Rich Presence
            DiscordRefresh(input);

        }

        private String Parse(int plattform, String s)
        {
            switch (plattform)
            {
                case 0: // Spotify
                    String[] aSpotify = s.Split('-');

                    String nameSpotify = aSpotify[1];
                    String authorSpotify = aSpotify[0];

                    authorSpotify = authorSpotify.TrimEnd(' ');
                    nameSpotify = nameSpotify.TrimStart(' ');

                    String rSpotify = pattern.Replace("%author", authorSpotify)
                                             .Replace("%artist", authorSpotify)
                                             .Replace("%name", nameSpotify)
                                             .Replace("%song", nameSpotify)
                                             .Replace("%software", "Spotify");
                    return rSpotify;
                case 1: // VLC media player
                    String[] aVLC = s.Split('-');

                    String nameVLC = aVLC[1].Replace(" VLC media player", "");
                    String authorVLC = aVLC[0].Replace(" VLC media player", "");

                    authorVLC = authorVLC.TrimEnd(' ');
                    nameVLC = nameVLC.TrimStart(' ');

                    String rVLC = pattern.Replace("%author", authorVLC)
                                         .Replace("%artist", authorVLC)
                                         .Replace("%name", nameVLC)
                                         .Replace("%song", nameVLC)
                                         .Replace("%software", "VLC Media Player");

                    return rVLC;
            }
            return null;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Settings();
            Timer();
            Refresh();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Link_MouseDown(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(String.Format("https://github.com/NoNamePro0/SongFile/blob/{0}/README.md", VersionTag));
        }

        public void DiscordLoad()
        {
            discord = new DiscordRpcClient("731868151402463242");

            discord.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            discord.OnReady += (sender, e) => { };
            discord.OnPresenceUpdate += (sender, e) => { };

            discord.Initialize();

            discord.SetPresence(new RichPresence()
            {
                Details = "Please wait.. Initialising..",
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    SmallImageKey = "image_small"
                }
            });
        }

        public void DiscordRefresh(string details)
        {
            switch (details)
            {
                case messageAdvert:  discord.ClearPresence(); return;
                case messagePaused:  discord.ClearPresence(); return;
                case messageNothing: discord.ClearPresence(); return;
            }

            discord.SetPresence(new RichPresence()
            {
                Details = details,
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    SmallImageKey = "image_small"
                }
            });
        }
    }
}