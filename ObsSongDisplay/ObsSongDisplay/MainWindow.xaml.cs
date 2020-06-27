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
        String interval = "10";

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
        }

        private bool Get(int plattform, bool written)
        {
            if(!written)
            {
                switch(plattform)
                {
                    case 0: // Spotify
                        var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

                        // Not open
                        if (proc == null)
                        {
                            File.WriteAllText(output, String.Empty);
                            using (StreamWriter outputFile = new StreamWriter(output))
                            {
                                outputFile.Write("Nothing playing!");

                                this.Dispatcher.Invoke(() =>
                                {
                                    song.Text = "Nothing playing!";
                                });

                                written = true;
                            }
                        }

                        // Playback paused!
                        if (string.Equals(proc.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))
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
                        if (string.Equals(proc.MainWindowTitle, "Advertisement", StringComparison.InvariantCultureIgnoreCase))
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
                        if (string.Equals(proc.MainWindowTitle, "Spotify Free", StringComparison.InvariantCultureIgnoreCase))
                        {
                            File.WriteAllText(output, String.Empty);
                            using (StreamWriter outputFile = new StreamWriter(output))
                            {
                                outputFile.Write("Playback paused!");

                                this.Dispatcher.Invoke(() => {
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
                                outputFile.WriteLine(Parse(0, proc.MainWindowTitle));
                                this.Dispatcher.Invoke(() =>
                                {
                                    song.Text = Parse(0, proc.MainWindowTitle);
                                });
                            }
                        }
                        break;
                }
            }
            return written;
        }

        private String Parse(int plattform, String s)
        {
            switch (plattform) {
                case 0: // Spotify
                    String[] a = s.Split('-');
                        
                    String name = a[1];
                    String author = a[0];

                    String r = pattern.Replace("%author", author)
                              .Replace("%name", name);
                    return r;
            }
            return null;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Settings();
            Timer();
        }
    }
}