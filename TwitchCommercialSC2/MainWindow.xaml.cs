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
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;

    using TwitchCommercialSC2.TwitchTV;
    using TwitchCommercialSC2.Updates;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary> References to the TwitchTV API </summary>
        private readonly TwitchApiV2 twitchApi = new TwitchApiV2();

        /// <summary> Whether there is a user currently logged in. </summary>
        private bool isLoggedIn;

        private NotifyIcon notify = new NotifyIcon();

        private MenuItem playCommercialMenuItem;

        private MenuItem playDelayedCommercialMenuItem;

        private MenuItem togglePauseMenuItem;

        /// <summary> Initializes a new instance of the <see cref="MainWindow"/> class. </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.notify.Icon = TwitchCommercialSC2.Resources.IconBlue;
            this.notify.Text = "SC2 Commercial Runner";
            this.notify.DoubleClick += this.NotifyIconClicked;
            this.notify.Visible = true;

            var menu = new ContextMenu();

            this.playCommercialMenuItem = new MenuItem("Play a &Commercial", (s, e) => this.PlayACommercial());
            this.playDelayedCommercialMenuItem = new MenuItem("Play a &Delayed Commercial", (s, e) => this.PlayADelayedCommercial());
            this.togglePauseMenuItem = new MenuItem("&Toggle Pause", (s, e) => this.TogglePause());

            menu.MenuItems.Add(new MenuItem("&Show Window", this.NotifyIconClicked));
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(this.playCommercialMenuItem);
            menu.MenuItems.Add(this.playDelayedCommercialMenuItem);
            menu.MenuItems.Add(this.togglePauseMenuItem);
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem("&Exit", (s, e) => System.Windows.Application.Current.Shutdown()));
            
            this.notify.ContextMenu = menu;

            // Load the settings from the file.
            this.settings.Load();
            this.settings.Save(); // This save ensures the file exists if the user wants to do advanced setup.
            
            //this.UpdateSettingsDescription();

            // Show version number
            
             /* var version = Assembly.GetExecutingAssembly().GetName().Version;
            
            this.txtVersion.Text = string.Format(
                "Version {0}.{1}.{2}", version.Major, version.MajorRevision, version.Build);
            */
            this.VerifyWhetherSetupIsComplete();

            Task.Factory.StartNew(this.CheckForUpdates);
        }

        private void TogglePause()
        {
            var c = this.activeControl as ReplayMonitorControl;
            if (c != null)
            {
                c.TogglePause();
            }
        }

        private void PlayADelayedCommercial()
        {
            var c = this.activeControl as ReplayMonitorControl;
            if (c != null)
            {
                c.PlayADelayedCommercial();
            }
        }

        private void PlayACommercial()
        {
            var c = this.activeControl as ReplayMonitorControl;
            if (c != null)
            {
                c.PlayACommercial();
            }
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

        /// <summary> Verifies whether setup is complete and highlights the boxes the correct colors. </summary>
        private void VerifyWhetherSetupIsComplete()
        {
            var result = this.twitchApi.HasUserAuthorized();

            if (result)
            {
                var user = this.twitchApi.GetUsersName();
                this.isLoggedIn = true;
                this.txtUserName.Text = string.Format("Logged in as {0}", user.Name);
                this.btnLogin.Visibility = Visibility.Visible;

                if (this.activeControl == null || this.activeControl.GetType() != typeof(ReplayMonitorControl))
                {
                    this.ContentGrid.Children.Clear();

                    this.playCommercialMenuItem.Enabled = true;
                    this.playDelayedCommercialMenuItem.Enabled = true;
                    this.togglePauseMenuItem.Enabled = true;

                    var control = new ReplayMonitorControl(this.settings, this.twitchApi);
                    control.OverlayRequested += this.ReplayMonitorOverlayRequested;
                    control.MonitoringStatusChanged += this.control_MonitoringStatusChanged;
                    control.ReplayFailedToParse += new EventHandler(control_ReplayFailedToParse);
                    this.ContentGrid.Children.Add(control);

                    this.activeControl = control;          
                }
            }
            else
            {
                this.isLoggedIn = false;
                this.txtUserName.Text = "Not Logged In";
                this.btnLogin.Visibility = Visibility.Collapsed;

                if (this.activeControl == null || this.activeControl.GetType() != typeof(LoginControl))
                {
                    this.playCommercialMenuItem.Enabled = false;
                    this.playDelayedCommercialMenuItem.Enabled = false;
                    this.togglePauseMenuItem.Enabled = false;
                    
                    var login = new LoginControl();
                    login.LoginSuccessful += this.login_LoginSuccessful;

                    this.ContentGrid.Children.Clear();
                    this.ContentGrid.Children.Add(login);

                    this.activeControl = login;
                }
            }
        }

        void control_ReplayFailedToParse(object sender, EventArgs e)
        {
            this.notify.ShowBalloonTip(
                3000,
                "Failed to read replay",
                "The replay failed to read, which may be due to a" + Environment.NewLine +
                "newer version of Starcraft 2. You may need to update" + Environment.NewLine +
                "your software. Still playing a commercial for you.",
                ToolTipIcon.Error);
        }

        void control_MonitoringStatusChanged(object sender, MonitorStatusEventArgs e)
        {
            this.notify.Icon = e.IsRunning ? TwitchCommercialSC2.Resources.IconBlue : TwitchCommercialSC2.Resources.IconRed;
        }

        private void login_LoginSuccessful(object sender, LoginEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Token) == false)
            {
                RegistrySettings.AccessToken = e.Token;

                this.VerifyWhetherSetupIsComplete();

                var c = this.activeControl as LoginControl;
                if (c != null)
                {
                    c.LoginSuccessful -= this.login_LoginSuccessful;
                }
            }
        }

        private void ReplayMonitorOverlayRequested(object sender, OverlayRequestEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                var overlay = new CommercialTimerOverlay(e.Delay, e.CommercialSeconds) { Owner = this };
                overlay.Show();
            }
            else
            {
                this.Dispatcher.BeginInvoke(
                    (Action)delegate
                        {
                            var overlay = new CommercialTimerOverlay(e.Delay, e.CommercialSeconds) { Owner = this };
                            overlay.Show();
                        });
            }

        }

        private FrameworkElement activeControl;

        private readonly SettingsFile settings = new SettingsFile();

        private void LogInButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.isLoggedIn)
            {
                // We want to log out, just wipe the access token. EZPZ
                RegistrySettings.AccessToken = string.Empty;
                this.isLoggedIn = false;
            }

            this.VerifyWhetherSetupIsComplete();                
            
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private bool updating = false;

        private void UpdateClicked(object sender, RoutedEventArgs e)
        {
            if (this.updating)
            {
                return;
            }

            this.updating = true;

            this.txtUserName.Text = "Updating...";
            this.btnLogin.Visibility = Visibility.Collapsed;
            this.btnUpdate.Visibility = Visibility.Collapsed;

            var c = new UpdateControl(this.updates);

            this.ContentGrid.Children.Clear();
            this.ContentGrid.Children.Add(c);

            c.BeginUpdate();
            
            //Task.Factory.StartNew(this.BeginUpdate);
        }
    }
}
