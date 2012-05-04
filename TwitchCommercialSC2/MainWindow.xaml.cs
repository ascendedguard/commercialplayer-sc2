// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
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

        /// <summary> Verifies whether setup is complete and highlights the boxes the correct colors. </summary>
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
                // If everything is set up correctly, we can begin watching for replays now.
                if (this.watcher != null)
                {
                    this.watcher.EnableRaisingEvents = false;
                    this.watcher.Dispose();
                }

                this.watcher = new FileSystemWatcher(RegistrySettings.ReplayLocation, "*.SC2Replay")
                    {
                        IncludeSubdirectories = true 
                    };

                this.watcher.Created += this.ReplayCreated;
                this.watcher.EnableRaisingEvents = true;

                this.AddToLog("Now watching for replays.");
            }
            else
            {
                this.AddToLog("Setup is incomplete. Finish all steps to begin watching for replays.");
            }
        }

        /// <summary> Opens the Authorize Twitch dialog when clicked. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event arguments. </param>
        private void AuthorizeTwitchClick(object sender, RoutedEventArgs e)
        {
            var window = new AuthorizeTwitchWindow
                { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
            window.ShowDialog();
            this.VerifyWhetherSetupIsComplete();
        }

        /// <summary> Opens the commercial setup dialog when clicked. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event arguments. </param>
        private void SetupCommercialClicked(object sender, RoutedEventArgs e)
        {
            var window = new CommercialSetupWindow
                { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
            window.ShowDialog();
            this.VerifyWhetherSetupIsComplete();
        }

        /// <summary> Adds a single line of text to the log. </summary>
        /// <param name="text"> The text to add. </param>
        private void AddToLog(string text)
        {
            txtLog.Dispatcher.BeginInvoke(
                (Action)delegate
                    {
                        txtLog.Text += text + Environment.NewLine;
                        txtLog.ScrollToEnd();
                    });
        }

        /// <summary> Begins the process of playing commercials (on a different thread) when a replay is created. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event arguments. </param>
        private void ReplayCreated(object sender, FileSystemEventArgs e)
        {
            Task.Factory.StartNew(() => this.PlayCommercials(e.FullPath), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        /// <summary> Plays commercials based on the settings set by the user. </summary>
        /// <param name="replay"> The replay to parse. </param>
        private void PlayCommercials(string replay)
        {
            // Sleep for 1 second to allow the replay to be freed from SC2 memory.
            Thread.Sleep(1000);

            int commercials = RegistrySettings.InitialCommercials;

            try
            {
                var rep = Replay.Parse(replay);
                var minMinutes = RegistrySettings.MinimumCommercialMinutes;

                if (rep.GameLength < new TimeSpan(0, minMinutes, 0))
                {
                    // Fluke, we shouldnt play any commercials.
                    this.AddToLog(string.Format("Game less than {0} minute detected. Not playing any commercials.", minMinutes));
                    return;
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
            catch (Exception)
            {
                // This way, even if something goes terribly wrong, like a new patch and we can't parse replays,
                // We will still play the regular commercial timings.
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
