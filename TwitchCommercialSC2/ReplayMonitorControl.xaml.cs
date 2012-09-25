namespace TwitchCommercialSC2
{
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Starcraft2.ReplayParser;

    using TwitchCommercialSC2.TwitchTV;

    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for ReplayMonitorControl.xaml
    /// </summary>
    public partial class ReplayMonitorControl
    {
        private SettingsControl settingsControl;

        /// <summary> Reference to the local API instance. </summary>
        private readonly TwitchApiV2 twitchApi;

        private readonly SettingsFile settings;

        private System.Timers.Timer updateTimer = new System.Timers.Timer(60000);

        /// <summary> File system watcher that waits for replays to be created. </summary>
        private FileSystemWatcher watcher;


        /// <summary> Initializes a new instance of the <see cref="ReplayMonitorControl"/> class. </summary>
        public ReplayMonitorControl()
        {
            InitializeComponent();
        }

        /// <summary> Initializes a new instance of the <see cref="ReplayMonitorControl"/> class. </summary>
        /// <param name="api"> The api instance. </param>
        public ReplayMonitorControl(SettingsFile settings, TwitchApiV2 api) : this()
        {
            this.twitchApi = api;
            this.twitchApi.CommercialPlayed += this.TwitchApiCommercialPlayed;

            this.settings = settings;

            this.replayCountTimer.AutoReset = false;
            this.replayCountTimer.Elapsed += this.replayCountTimer_Elapsed;

            this.updateTimer.AutoReset = true;
            this.updateTimer.Elapsed += this.updateTimer_Elapsed;
            this.updateTimer.Start();

            this.VerifyWhetherSetupIsComplete();
        }


        private int numReplaysCreated = 0;

        private string lastReplayPath = string.Empty;

        private System.Timers.Timer replayCountTimer = new System.Timers.Timer(5000);

        /// <summary> Begins the process of playing commercials (on a different thread) when a replay is created. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event arguments. </param>
        private void ReplayCreated(object sender, FileSystemEventArgs e)
        {
            if (this.monitoringPaused)
            {
                // Don't do anything while paused.
                return;
            }

            this.numReplaysCreated++;

            this.lastReplayPath = e.FullPath;

            this.replayCountTimer.Stop();
            this.replayCountTimer.Start();
        }

        /// <summary> Adds a single line of text to the log. </summary>
        /// <param name="text"> The text to add. </param>
        private void AddToLog(string text)
        {
            // Just for debugging now.
            Debug.WriteLine(text);
        }

        /// <summary> Plays commercials based on the settings set by the user. </summary>
        /// <param name="replay"> The replay to parse. </param>
        private void PlayCommercials(string replay)
        {
            int commercials = this.settings.InitialCommercials;

            try
            {
                var rep = Replay.Parse(replay);
                var minMinutes = this.settings.MinimumGameMinutes;

                if (rep.GameLength < new TimeSpan(0, minMinutes, 0))
                {
                    // Fluke, we shouldnt play any commercials.
                    this.AddToLog(string.Format("Game less than {0} minute detected. Not playing any commercials.", minMinutes));
                    return;
                }

                var time = rep.GameLength;

                if (this.settings.GameMinutesPerExtra != 0)
                {
                    var requiredTime = new TimeSpan(0, this.settings.GameMinutesPerExtra, 0);

                    while (time > requiredTime)
                    {
                        commercials++;
                        time -= requiredTime;
                    }
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
                var handler = this.ReplayFailedToParse;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }

                // This way, even if something goes terribly wrong, like a new patch and we can't parse replays,
                // We will still play the regular commercial timings.
                this.AddToLog("We failed to read the replay for some reason. We'll still play " + commercials + " commercial(s).");
            }

            // Subtract 5 due to the timer used to track multiple replays.
            var delay = this.settings.Delay - 5;

            if (delay < 0)
            {
                delay = 0;
            }

            var commercialTime = commercials * 30;

            if (this.settings.ShowOverlay)
            {
                this.ShowOverlay(delay, commercialTime);
            }

            // Sleep the thread for the delay period, then starts playing commercials.
            Thread.Sleep(delay * 1000);

            while (commercials > 0)
            {
                this.AddToLog(string.Format("Playing commercial. {0} more to go.", commercials - 1));

                this.twitchApi.PlayCommercial();
                Thread.Sleep(30000);
                commercials--;
            }

            this.AddToLog("Last commercial finished playing.");
        }

        private void updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                (Action)delegate
                {
                    var timeSinceLastInMinutes = (int)Math.Round((DateTime.Now - this.twitchApi.LastCommercialTime).TotalMinutes);
                    this.txtTimeSinceLastCommercial.Text =
                        timeSinceLastInMinutes.ToString(CultureInfo.InvariantCulture) + "m";

                    var timeSinceStart = (DateTime.Now - this.twitchApi.InitializationTime).TotalMinutes;
                    if (timeSinceStart < 1)
                    {
                        timeSinceStart = 1;
                    }

                    this.txtAverageCommercialsPerHour.Text = (this.twitchApi.TotalCommercials * 60 / timeSinceStart).ToString("F1", CultureInfo.InvariantCulture);
                });
        }

        private void TwitchApiCommercialPlayed(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                (Action)delegate
                {
                    this.txtTotalCommercialsPlayed.Text =
                        this.twitchApi.TotalCommercials.ToString(CultureInfo.InvariantCulture);
                    this.txtTimeSinceLastCommercial.Text = "0m";

                    var timeSinceStart = (DateTime.Now - this.twitchApi.InitializationTime).TotalMinutes;
                    if (timeSinceStart < 1)
                    {
                        timeSinceStart = 1;
                    }

                    this.txtAverageCommercialsPerHour.Text =
                        (this.twitchApi.TotalCommercials * 60 / timeSinceStart).ToString(
                            "F1", CultureInfo.InvariantCulture);
                });
        }

        private void PlayCommercialClicked(object sender, RoutedEventArgs e)
        {
            this.PlayACommercial();
        }

        public void PlayACommercial()
        {
            this.twitchApi.PlayCommercial();

            if (this.settings.ShowOverlay)
            {
                this.ShowOverlay(0, 30);
            }
        }

        private void ShowOverlay(int delay, int commercialSeconds)
        {
            var handler = this.OverlayRequested;
            if (handler != null)
            {
                handler(this, new OverlayRequestEventArgs(delay, commercialSeconds));
            }
        }

        public event EventHandler ReplayFailedToParse;

        public event EventHandler<OverlayRequestEventArgs> OverlayRequested;

        private void PauseReplayMonitor(object sender, RoutedEventArgs e)
        {
            this.TogglePause();
        }

        public void TogglePause()
        {
            this.monitoringPaused = !this.monitoringPaused;

            this.txtPauseReplayMonitorButton.Content = this.monitoringPaused ? "Unpause" : "Pause Replay Monitoring";

            var handler = this.MonitoringStatusChanged;
            if (handler != null)
            {
                handler(this, new MonitorStatusEventArgs(!this.monitoringPaused));
            }
        }

        public event EventHandler<MonitorStatusEventArgs> MonitoringStatusChanged;

        private bool monitoringPaused;

        private void PlayCommercialDelayClicked(object sender, RoutedEventArgs e)
        {
            this.PlayADelayedCommercial();
        }

        public void PlayADelayedCommercial()
        {
            var delay = this.settings.Delay;

            Task.Factory.StartNew(
                () =>
                {
                    Thread.Sleep(delay * 1000);
                    this.twitchApi.PlayCommercial();
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);

            if (this.settings.ShowOverlay)
            {
                this.ShowOverlay(delay, 30);
            }
        }

        private void OpenSettingsClicked(object sender, RoutedEventArgs e)
        {
            if (this.settingsControl == null)
            {
                this.settingsControl = new SettingsControl(this.settings);
                this.settingsControl.SettingsClose += this.SettingsPanelClosing;
            }

            gridMonitor.Visibility = Visibility.Collapsed;
            gridMain.Children.Add(this.settingsControl);
        }

        private void SettingsPanelClosing(object sender, EventArgs e)
        {
            this.settings.Load();

            gridMain.Children.Remove(this.settingsControl);
            gridMonitor.Visibility = Visibility.Visible;

            this.settingsControl = null;

            VerifyWhetherSetupIsComplete();
        }

        /// <summary> Verifies whether setup is complete and highlights the boxes the correct colors. </summary>
        private void VerifyWhetherSetupIsComplete()
        {
            bool commercialSetup = false;

            var folder = this.settings.ReplayDirectory;

            if (Directory.Exists(folder))
            {
                commercialSetup = true;
            }

            if (commercialSetup)
            {
                // If everything is set up correctly, we can begin watching for replays now.
                if (this.watcher != null)
                {
                    this.watcher.EnableRaisingEvents = false;
                    this.watcher.Dispose();
                }

                this.watcher = new FileSystemWatcher(folder, "*.SC2Replay")
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

        private void replayCountTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.numReplaysCreated > 1)
            {
                // We should assume that if this many replays were created in a 5 second period, it's likely they're
                // Just being copied, or something else is off. Either way, don't play commercials.
                this.numReplaysCreated = 0;
                return;
            }

            this.numReplaysCreated = 0;

            Task.Factory.StartNew(() => this.PlayCommercials(this.lastReplayPath), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
    }
}
