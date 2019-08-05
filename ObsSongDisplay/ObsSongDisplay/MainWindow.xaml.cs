using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Media;
using System.Diagnostics;
using System.IO;
using MahApps.Metro;

namespace ObsSongDisplay
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

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

        private void loadSettings()
        {
            textBox.Text = File.ReadAllLines("config.txt")[0];
            intervalBox.Text = File.ReadAllLines("config.txt")[1];
        }

        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            interval = intervalBox.Text;
            pattern = textBox.Text;

            File.WriteAllText("config.txt", String.Empty);
            using (StreamWriter outputFile = new StreamWriter("config.txt"))
            {
                outputFile.WriteLine(pattern);
                outputFile.WriteLine(interval);
                outputFile.WriteLine("");
                outputFile.WriteLine("");
                outputFile.WriteLine("#########################");
                outputFile.WriteLine("# Don't edit this file! #");
                outputFile.WriteLine("#########################");
            }
        }

        private void Link_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://0x0verflow.cf/");
        }

        private void MetroWindow_Initialized(object sender, EventArgs e)
        {
            loadSettings();
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

        String pattern = "/*author*/ - /*name*/";
        String interval = "5000";

        public void checkThread()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(int.Parse(interval));

                    bool written = false;

                    var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

                    if (proc == null)
                    {
                        File.WriteAllText("osd.txt", String.Empty);
                        using (StreamWriter outputFile = new StreamWriter("osd.txt"))
                        {
                            outputFile.Write("Nothing playing!");
                            written = true;
                        }
                    }

                    if (string.Equals(proc.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))
                    {
                        File.WriteAllText("osd.txt", String.Empty);
                        using (StreamWriter outputFile = new StreamWriter("osd.txt"))
                        {
                            outputFile.Write("Playback paused!");
                            written = true;
                        }
                    }

                    if (string.Equals(proc.MainWindowTitle, "Advertisement", StringComparison.InvariantCultureIgnoreCase))
                    {
                        File.WriteAllText("osd.txt", String.Empty);
                        using (StreamWriter outputFile = new StreamWriter("osd.txt"))
                        {
                            outputFile.Write("Waiting for playback...");
                            written = true;
                        }
                    }

                    if (string.Equals(proc.MainWindowTitle, "Spotify Free", StringComparison.InvariantCultureIgnoreCase))
                    {
                        File.WriteAllText("osd.txt", String.Empty);
                        using (StreamWriter outputFile = new StreamWriter("osd.txt"))
                        {
                            outputFile.Write("Playback paused!");
                            written = true;
                        }
                    }

                    if (!written)
                    {
                        File.WriteAllText("osd.txt", String.Empty);
                        using (StreamWriter outputFile = new StreamWriter("osd.txt"))
                        {
                            outputFile.WriteLine(parseSpotify(proc.MainWindowTitle));
                        }
                    }
                }

            });
            
            
        }

        private String parseSpotify(String s)
        {
            String[] a = s.Split('-');

            String name = a[1];
            String author = a[0];

            String r = pattern.Replace("/*author*/", author).Replace("/*name*/", name);

            return r;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            checkThread();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                ThemeManager.ChangeAppStyle(this,
                                    ThemeManager.GetAccent("Olive"),
                                    ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                ThemeManager.ChangeAppStyle(this,
                                    ThemeManager.GetAccent("Blue"),
                                    ThemeManager.GetAppTheme("BaseLight"));
            }
            
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeAppStyle(this,
                                    ThemeManager.GetAccent("Blue"),
                                    ThemeManager.GetAppTheme("BaseLight"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://steamcommunity.com/tradeoffer/new/?partner=255140219&token=DF94BUQs");
        }
    }
}
