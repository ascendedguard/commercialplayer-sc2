// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsControl.xaml.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for SettingsControl.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl
    {
        private readonly int[] delayChoices = new[]
            { 5, 10, 15, 20, 30, 45, 60, 90, 120, 150, 180, 240, 300, 360, 420, 480, 540, 600 };

        private readonly int[] initialCommercials = new[] { 1, 1, 1, 1, 1, 2, 2, 2, 2 };

        private readonly int[] gameMinutesPerExtra = new[] { 0, 30, 20, 15, 10, 25, 20, 15, 10 };

        private readonly int[] minimumGameMinutes = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        private int selectedDelay;

        private int initialCommercialCount;

        private int gameMinutesPerExtraCount;

        private int minimumGameMinuteCount;

        private string replayDirectory;

        private bool showOverlay;

        /// <summary> Initializes a new instance of the <see cref="SettingsControl"/> class. </summary>
        protected SettingsControl()
        {
            this.InitializeComponent();
        }

        public SettingsControl(SettingsFile settings) : this()
        {
            this.selectedDelay = settings.Delay;
            this.initialCommercialCount = settings.InitialCommercials;
            this.gameMinutesPerExtraCount = settings.GameMinutesPerExtra;
            this.minimumGameMinuteCount = settings.MinimumGameMinutes;
            this.replayDirectory = settings.ReplayDirectory;
            this.showOverlay = settings.ShowOverlay;

            this.cbxShowOverlay.IsChecked = this.showOverlay;

            this.UpdateCommercialValueText();
            this.UpdateDelayValueText();
            this.UpdateMinimumGameTimeValueText();
        }

        private void DelaySliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var index = (int)e.NewValue;

            this.selectedDelay = this.delayChoices[index];

            this.UpdateDelayValueText();
        }

        private void UpdateDelayValueText()
        {
            int minutes = this.selectedDelay / 60;
            int seconds = this.selectedDelay % 60;

            string time = string.Empty;

            if (minutes > 0)
            {
                time += string.Format("{0}m ", minutes);
            }

            this.txtDelayDescription.Text = string.Format("{0}{1}s delay until first commercial.", time, seconds);
        }

        private void CommercialValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var index = (int)e.NewValue;

            this.initialCommercialCount = this.initialCommercials[index];
            this.gameMinutesPerExtraCount = this.gameMinutesPerExtra[index];

            this.UpdateCommercialValueText();
        }

        private void UpdateCommercialValueText()
        {
            var text = string.Format(
                "{0} commercial{1}", this.initialCommercialCount, this.initialCommercialCount > 1 ? "s" : string.Empty);
            
            if (this.gameMinutesPerExtraCount > 0)
            {
                text += string.Format(", 1 extra for every {0} game minutes.", this.gameMinutesPerExtraCount);
            }

            this.txtCommercialDescription.Text = text;
        }

        public event EventHandler SettingsClose;

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            var handler = this.SettingsClose;
            if (handler != null)
            {
                this.SettingsClose(this, EventArgs.Empty);
            }
        }

        private void SaveSettingsClicked(object sender, RoutedEventArgs e)
        {
            this.showOverlay = cbxShowOverlay.IsChecked == true;

            // Save the settings to a file.
            var file = new SettingsFile
                {
                    MinimumGameMinutes = this.minimumGameMinuteCount,
                    InitialCommercials = this.initialCommercialCount,
                    GameMinutesPerExtra = this.gameMinutesPerExtraCount,
                    Delay = this.selectedDelay,
                    ReplayDirectory = this.replayDirectory,
                    ShowOverlay = this.showOverlay
                };

            file.Save();

            var handler = this.SettingsClose;
            if (handler != null)
            {
                this.SettingsClose(this, EventArgs.Empty);
            }
        }

        private void MinimumGameTimeValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var index = (int)e.NewValue;

            this.minimumGameMinuteCount = this.minimumGameMinutes[index];

            this.UpdateMinimumGameTimeValueText();
        }

        private void UpdateMinimumGameTimeValueText()
        {
            var text = string.Format(
                "Minimum game length: {0} minute{1}.",
                this.minimumGameMinuteCount,
                this.minimumGameMinuteCount > 1 ? "s" : string.Empty);

            this.txtMinimumGameTime.Text = text;
        }

        private void AdvancedSetupClicked(object sender, RoutedEventArgs e)
        {
            var dir = Path.GetDirectoryName(SettingsFile.SettingsPath);
            Process.Start(dir);
        }
    }
}
