// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateControl.xaml.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for UpdateControl.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Windows;

    using TwitchCommercialSC2.Updates;

    /// <summary>
    /// Interaction logic for UpdateControl.xaml
    /// </summary>
    public partial class UpdateControl
    {
        /// <summary>
        /// Contains information about updates.
        /// </summary>
        private UpdateFile updates;

        /// <summary>
        /// Prevents a default instance of the <see cref="UpdateControl"/> class from being created. 
        /// </summary>
        private UpdateControl()
        {
            this.InitializeComponent();
        }

        public UpdateControl(UpdateFile update) : this()
        {
            this.updates = update;
        }

        private string updateDownloadLocation;

        private WebClient client;

        public void BeginUpdate()
        {
            this.updateDownloadLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Twitch Commercial Runner",
                "Updates",
                "TwitchCommercialSetup.exe");

            Directory.CreateDirectory(Path.GetDirectoryName(this.updateDownloadLocation));

            this.client = new WebClient();
            this.client.DownloadFileAsync(this.updates.DownloadLocation, this.updateDownloadLocation);

            this.client.DownloadProgressChanged += this.DownloadProgressChanged;
            this.client.DownloadFileCompleted += this.DownloadFileCompleted;
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.client.Dispose();

            try
            {
                Process.Start(this.updateDownloadLocation);
                this.Dispatcher.BeginInvoke((Action)(() => Application.Current.Shutdown()));
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(
                    "Updating the application failed. Please Restart." + Environment.NewLine + ex.Message,
                    "Failed to update",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
    }
}
