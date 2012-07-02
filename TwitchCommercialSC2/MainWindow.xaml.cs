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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;

    using Starcraft2.ReplayParser;

    using TwitchCommercialSC2.TwitchTV;
    using TwitchCommercialSC2.Updates;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary> References to the TwitchTV API </summary>
        private readonly TwitchApiV2 twitchApi = new TwitchApiV2();

        /// <summary> File system watcher that waits for replays to be created. </summary>
        private FileSystemWatcher watcher;

        /// <summary> Whether there is a user currently logged in. </summary>
        private bool isLoggedIn;

        private System.Timers.Timer updateTimer = new System.Timers.Timer(60000);

        private NotifyIcon notify = new NotifyIcon();

        /// <summary> Initializes a new instance of the <see cref="MainWindow"/> class. </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.twitchApi.CommercialPlayed += this.TwitchApiCommercialPlayed;

            this.updateTimer.AutoReset = true;
            this.updateTimer.Elapsed += this.updateTimer_Elapsed;
            this.updateTimer.Start();

            this.notify.Icon = TwitchCommercialSC2.Resources.IconBlue;
            this.notify.Text = "Twitch Commercial Runner";
            this.notify.DoubleClick += this.NotifyIconClicked;
            this.notify.Visible = true;

            var menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("&Show Window", this.NotifyIconClicked));
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem("Play a &Commercial", (s, e) => this.PlayACommercial()));
            menu.MenuItems.Add(new MenuItem("Play a &Delayed Commercial", (s, e) => this.PlayADelayedCommercial()));
            menu.MenuItems.Add(new MenuItem("&Toggle Pause", (s, e) => this.TogglePause()));
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem("&Exit", (s, e) => System.Windows.Application.Current.Shutdown()));

            this.notify.ContextMenu = menu;

            this.replayCountTimer.AutoReset = false;
            this.replayCountTimer.Elapsed += this.replayCountTimer_Elapsed;

            // Load the settings from the file.
            this.settings.Load();
            this.settings.Save(); // This save ensures the file exists if the user wants to do advanced setup.
            this.UpdateSettingsDescription();

            // Show version number
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.txtVersion.Text = string.Format(
                "Version {0}.{1}.{2}", version.Major, version.MajorRevision, version.Build);

            this.VerifyWhetherSetupIsComplete();

            Task.Factory.StartNew(this.CheckForUpdates);
        }

        private UpdateFile updates;

        private void CheckForUpdates()
        {
            this.updates = UpdateFile.FindUpdates();
            if (this.updates.IsNewerVersion)
            {
                this.Dispatcher.BeginInvoke((Action)delegate { btnUpdate.Visibility = Visibility.Visible; });
            }
        }

        private void UpdateSettingsDescription()
        {
            var text = new StringBuilder();

            text.AppendLine("Your Settings:");
            text.AppendLine();

            // Delay
            int minutes = this.settings.Delay / 60;
            int seconds = this.settings.Delay % 60;

            string time = string.Empty;

            if (minutes > 0)
            {
                time += minutes + "m ";
            }

            time += seconds + "s ";

            text.AppendLine(string.Format("{0} delay until first commercials start.", time));
            
            // Commercials
            var comText = this.settings.InitialCommercials.ToString() + " commercial";

            if (this.settings.InitialCommercials > 1)
            {
                comText += "s";
            }

            if (this.settings.GameMinutesPerExtra > 0)
            {
                comText += ", with 1 extra for every " + this.settings.GameMinutesPerExtra + " minutes in-game.";
            }

            text.AppendLine(comText);

            var minText = "A commercial won't play for games less than " + this.settings.MinimumGameMinutes + " minutes long.";

            text.AppendLine(minText);

            if (Directory.Exists(this.settings.ReplayDirectory) == false)
            {
                text.AppendLine("YOUR REPLAY DIRECTORY IS INVALID! Nothing will work until you fix this.");
            }

            this.txtSettingsDescription.Text = text.ToString();
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            this.notify.Dispose();
            this.notify = null;
        }

        private void NotifyIconClicked(object sender, EventArgs e)
        { 
            // Re-show the application when the taskbar icon is clicked.
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
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

        /// <summary> Verifies whether setup is complete and highlights the boxes the correct colors. </summary>
        private void VerifyWhetherSetupIsComplete()
        {
            bool twitchAccess = false;

            var result = this.twitchApi.HasUserAuthorized();

            if (result)
            {
                var user = this.twitchApi.GetUsersName();
                twitchAccess = true;
                this.isLoggedIn = true;
                this.txtUserName.Text = user.Name;
                this.imgProfile.Source = user.Image;
                this.txtLoginButton.Text = "LOG OUT";
            }
            else
            {
                this.isLoggedIn = false;
                this.txtUserName.Text = "Not Logged In";
                this.txtLoginButton.Text = "LOG IN";
            }

            bool commercialSetup = false;

            var folder = settings.ReplayDirectory;

            if (Directory.Exists(folder))
            {
                commercialSetup = true;
            }

            if (twitchAccess && commercialSetup)
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

        private SettingsFile settings = new SettingsFile();

        /// <summary> Opens the commercial setup dialog when clicked. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event arguments. </param>
        private void SetupCommercialClicked(object sender, RoutedEventArgs e)
        {
            if (this.settingsOverlaySection.Children.Count > 0)
            {
                // Already open, so close it.
                this.settingsOverlaySection.Children.Clear();
                this.settings.Load();
                this.UpdateSettingsDescription();
                this.VerifyWhetherSetupIsComplete();
                return;
            }

            var control = new SettingsControl(this.settings);
            this.settingsOverlaySection.Children.Add(control);
            control.SettingsClose += this.SettingsControlClosed;
        }

        void SettingsControlClosed(object sender, EventArgs e)
        {
            this.settingsOverlaySection.Children.Clear();
            this.settings.Load(); // Reload the settings.
            this.UpdateSettingsDescription();
            this.VerifyWhetherSetupIsComplete();
        }

        /// <summary> Adds a single line of text to the log. </summary>
        /// <param name="text"> The text to add. </param>
        private void AddToLog(string text)
        {
            // Just for debugging now.
            Debug.WriteLine(text);
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
                var requiredTime = new TimeSpan(0, this.settings.GameMinutesPerExtra, 0);
                while (time > requiredTime)
                {
                    commercials++;
                    time -= requiredTime;
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
                this.notify.ShowBalloonTip(
                    3000,
                    "Failed to read replay",
                    "The replay failed to read, which may be due to a" + Environment.NewLine + 
                    "newer version of Starcraft 2. You may need to update" + Environment.NewLine + 
                    "your software. Still playing a commercial for you.",
                    ToolTipIcon.Error);

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

            this.Dispatcher.BeginInvoke(
                (Action)delegate
                    {
                        var overlay = new CommercialTimerOverlay(delay, commercialTime);
                        overlay.Owner = this;
                        overlay.Show();
                    });

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

        private void LogInButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.isLoggedIn)
            {
                // We want to log out.
                RegistrySettings.AccessToken = string.Empty;
                this.isLoggedIn = false;
                this.txtUserName.Text = "Not Logged In";
                this.txtLoginButton.Text = "LOG IN";
            }
            else
            {
                var window = new AuthorizeTwitchWindow { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
                window.ShowDialog();
                this.VerifyWhetherSetupIsComplete();                
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void PlayCommercialClicked(object sender, RoutedEventArgs e)
        {
            this.PlayACommercial();
        }

        private void PlayACommercial()
        {
            this.twitchApi.PlayCommercial();
            this.ShowOverlay(0, 30);
        }

        private void ShowOverlay(int delay, int commercialSeconds)
        {
            var overlay = new CommercialTimerOverlay(delay, commercialSeconds) { Owner = this };
            overlay.Show();
        }

        private void PauseReplayMonitor(object sender, RoutedEventArgs e)
        {
            this.TogglePause();
        }

        private void TogglePause()
        {
            this.monitoringPaused = !this.monitoringPaused;

            this.txtPauseReplayMonitorButton.Text = this.monitoringPaused ? "Unpause" : "Pause Replay Monitoring";
            this.notify.Icon = this.monitoringPaused ? TwitchCommercialSC2.Resources.IconRed : TwitchCommercialSC2.Resources.IconBlue;            
        }

        private bool monitoringPaused;

        private void PlayCommercialDelayClicked(object sender, RoutedEventArgs e)
        {
            this.PlayADelayedCommercial();
        }

        private void PlayADelayedCommercial()
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

            this.ShowOverlay(delay, 30);
        }

        private void OpenWikiClicked(object sender, RoutedEventArgs e)
        {
            const string wikiPage = "https://github.com/ascendedguard/commercialplayer-sc2/wiki";

            Process.Start(wikiPage);
        }

        private bool updating = false;

        private void UpdateClicked(object sender, RoutedEventArgs e)
        {
            if (this.updating)
            {
                return;
            }


            this.updating = true;
            Task.Factory.StartNew(this.BeginUpdate);
        }

        private void BeginUpdate()
        {
            var updateDownloadLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Twitch Commercial Runner",
                "Updates",
                "TwitchCommercialSetup.exe");

            Directory.CreateDirectory(Path.GetDirectoryName(updateDownloadLocation));

            var client = new WebClient();
            client.DownloadFile(this.updates.DownloadLocation, updateDownloadLocation);
            client.Dispose();

            Process.Start(updateDownloadLocation);
            this.Dispatcher.BeginInvoke((Action)(() => System.Windows.Application.Current.Shutdown()));
        }
    }
}
