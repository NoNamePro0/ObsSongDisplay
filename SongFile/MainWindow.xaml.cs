using System;
using System.Linq;
using System.Threading;
using System.Windows;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace ObsSongDisplay
{
    public partial class MainWindow : MetroWindow
    {
        String pattern = "%author - %name";
        String interval = "20";

        String config = "config.txt";
        String output = "output.txt";

        private System.Windows.Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(@"Resources/osdicon.ico");
            notifyIcon.MouseDoubleClick +=
                new System.Windows.Forms.MouseEventHandler
                    (notifyIcon_MouseDoubleClick);
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

        public void Refresh(object sender, ElapsedEventArgs e)
        {
            bool written = false;

            // Prionty
            written = Get(0, written); // Spotify
            written = Get(1, written); // VLC

            
            this.Dispatcher.Invoke(() =>
            {
                if (String.IsNullOrEmpty(song.Text))
                {
                    File.WriteAllText(output, String.Empty);
                    using (StreamWriter outputFile = new StreamWriter(output))
                    {
                        outputFile.WriteLine("Nothing playing");
                        song.Text = "Nothing playing";
                    }
                }
            });
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
                                File.WriteAllText(output, String.Empty);
                                using (StreamWriter outputFile = new StreamWriter(output))
                                {
                                    outputFile.Write("Playback paused!");
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        song.Text = "Playback paused!";
                                    });
                                    written = true;
                                }
                            }

                            // Waiting for playback...
                            if (string.Equals(procSpotify.MainWindowTitle, "Advertisement", StringComparison.InvariantCultureIgnoreCase))
                            {
                                File.WriteAllText(output, String.Empty);
                                using (StreamWriter outputFile = new StreamWriter(output))
                                {
                                    outputFile.Write("Waiting for playback...");
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        song.Text = "Waiting for playback...";
                                    });
                                    written = true;
                                }
                            }

                            // Playback paused!
                            if (string.Equals(procSpotify.MainWindowTitle, "Spotify Free", StringComparison.InvariantCultureIgnoreCase))
                            {
                                File.WriteAllText(output, String.Empty);
                                using (StreamWriter outputFile = new StreamWriter(output))
                                {
                                    outputFile.Write("Playback paused!");

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        song.Text = "Playback paused!";
                                    });

                                    written = true;
                                }
                            }

                            // Song playing!
                            if (!written)
                            {
                                File.WriteAllText(output, String.Empty);
                                using (StreamWriter outputFile = new StreamWriter(output))
                                {
                                    outputFile.WriteLine(Parse(0, procSpotify.MainWindowTitle));
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        song.Text = Parse(0, procSpotify.MainWindowTitle);
                                    });
                                }
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
                                File.WriteAllText(output, String.Empty);
                                using (StreamWriter outputFile = new StreamWriter(output))
                                {
                                    outputFile.Write("Playback paused!");
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        song.Text = "Playback paused!";
                                    });
                                    written = true;
                                }
                            }

                            // Song playing!
                            if (!written)
                            {
                                if (procVLC.MainWindowTitle.EndsWith(" - VLC media player"))
                                {
                                    File.WriteAllText(output, String.Empty);
                                    using (StreamWriter outputFile = new StreamWriter(output))
                                    {
                                        outputFile.WriteLine(Parse(1, procVLC.MainWindowTitle));
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            song.Text = Parse(1, procVLC.MainWindowTitle);
                                        });
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            return written;
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
                                             .Replace("%software", "VLC Media Player");
                    return rSpotify;
                case 1: // VLC media player
                    String[] aVLC = s.Split('-');

                    String nameVLC = aVLC[1].Replace("VLC media player", "");
                    String authorVLC = aVLC[0].Replace("VLC media player", "");

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
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh(null, null);    
        }
    }
}