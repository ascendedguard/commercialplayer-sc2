using System.Windows;

namespace TwitchCommercialSC2
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Starcraft2.ReplayParser;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary> File system watcher that waits for replays to be created. </summary>
        private FileSystemWatcher watcher;

        /// <summary> Initializes a new instance of the <see cref="MainWindow"/> class. </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.VerifyWhetherSetupIsComplete();
        }

        private void VerifyWhetherSetupIsComplete()
        {
            bool twitchAccess = false;

            if (string.IsNullOrEmpty(RegistrySettings.AccessToken) == false)
            {
                rectAuthorizeColor.Fill = Brushes.DarkGreen;
                txtAuthorizeStatus.Text = "Complete";
                twitchAccess = true;
            }
            else
            {
                rectAuthorizeColor.Fill = Brushes.DarkRed;
                txtAuthorizeStatus.Text = "Incomplete";
            }

            bool commercialSetup = false;

            var folder = RegistrySettings.ReplayLocation;

            if (Directory.Exists(folder))
            {
                rectSetupCommercials.Fill = Brushes.DarkGreen;
                txtSetupCommercials.Text = "Complete";
                commercialSetup = true;
            }
            else
            {
                rectSetupCommercials.Fill = Brushes.DarkRed;
                txtSetupCommercials.Text = "Incomplete";
            }

            if (twitchAccess && commercialSetup)
            {
                if (this.watcher != null)
                {
                    this.watcher.EnableRaisingEvents = false;
                    this.watcher.Dispose();
                }

                this.watcher = new FileSystemWatcher(RegistrySettings.ReplayLocation, "*.SC2Replay");
                this.watcher.IncludeSubdirectories = true;
                this.watcher.Created += this.ReplayCreated;
                this.watcher.EnableRaisingEvents = true;

                this.AddToLog("Now watching for replays.");
            }
            else
            {
                this.AddToLog("Setup is incomplete. Finish all steps to begin watching for replays.");
            }
        }

        private void AuthorizeTwitchClick(object sender, RoutedEventArgs e)
        {
            var window = new AuthorizeTwitchWindow
                { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
            window.ShowDialog();
            this.VerifyWhetherSetupIsComplete();
        }

        private void SetupCommercialClicked(object sender, RoutedEventArgs e)
        {
            var window = new CommercialSetupWindow
                { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
            window.ShowDialog();
            this.VerifyWhetherSetupIsComplete();
        }

        public void AddToLog(string text)
        {
            txtLog.Dispatcher.BeginInvoke(
                (Action)delegate
                    {
                        txtLog.Text += text + Environment.NewLine;
                        txtLog.ScrollToEnd();
                    });

        }

        private void ReplayCreated(object sender, FileSystemEventArgs e)
        {
            Task.Factory.StartNew(() => this.PlayCommercials(e.FullPath), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private void PlayCommercials(string replay)
        {
            // Sleep for 1 second to allow the replay to be freed from SC2 memory.
            Thread.Sleep(1000);

            int commercials = RegistrySettings.InitialCommercials;

            try
            {
                var rep = Replay.Parse(replay);

                if (rep.GameLength < new TimeSpan(0, 1, 0))
                {
                    // Fluke, we shouldnt play any commercials.
                    this.AddToLog("Game less than 1 minute detected. Not playing any commercials.");
                }

                if (rep.GameLength > new TimeSpan(0, RegistrySettings.ReplayExtraMinutes, 0))
                {
                    commercials += RegistrySettings.ExtraCommercials;
                }

                this.AddToLog(
                    string.Format(
                        "{0} vs {1} was {2} minutes long. Playing {3} commercial(s).",
                        rep.Players[0].Name,
                        rep.Players[1].Name,
                        rep.GameLength.Minutes,
                        commercials));
            }
            catch (Exception ex)
            {
                this.AddToLog(
                    "We failed to read the replay for some reason. We'll still play " + commercials + " commercial(s).");
            }

            // Sleep for the specified time.
            Thread.Sleep(RegistrySettings.SecondsDelay * 1000);

            var api = new TwitchTV.TwitchApi(
                RegistrySettings.ConsumerKey,
                RegistrySettings.ConsumerSecret,
                RegistrySettings.AccessToken,
                RegistrySettings.AccessTokenSecret);

            while (commercials > 0)
            {
                this.AddToLog(string.Format("Playing commercial. {0} more to go.", commercials - 1));

                api.PlayCommercial();
                Thread.Sleep(31000); // TwitchTV commercials are always 30 second blocks. Add a second just to be safe.
                commercials--;
            }

            this.AddToLog("Last commercial finished playing.");
        }
    }
}
