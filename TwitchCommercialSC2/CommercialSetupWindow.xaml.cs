// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommercialSetupWindow.xaml.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for CommercialSetupWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for CommercialSetupWindow.xaml
    /// </summary>
    public partial class CommercialSetupWindow
    {
        /// <summary> The list of choices for the delay seconds. </summary>
        private readonly int[] delayChoices = new[] { 0, 2, 5, 8, 10, 15, 20, 30, 45, 60, 90, 120, 150, 180, 240, 300 };

        /// <summary> The list of choices for the number of minutes in the replay to watch for. </summary>
        private readonly int[] replayMinuteChoices = new[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };

        /// <summary> The list of the number of commercials to play. </summary>
        private readonly int[] commercialChoices = new[] { 0, 1, 2, 3, 4, 5, 6 };

        /// <summary> The list of choices for the minimum game time before a commercial is played. </summary>
        private readonly int[] minimumTimeChoices = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        /// <summary> Initializes a new instance of the <see cref="CommercialSetupWindow"/> class. </summary>
        public CommercialSetupWindow()
        {
            InitializeComponent();

            // Set up item sources
            this.cbxSecondsDelay.ItemsSource = this.delayChoices;
            this.cbxReplayMinutes.ItemsSource = this.replayMinuteChoices;
            this.cbxInitialCommercials.ItemsSource = this.commercialChoices;
            this.cbxExtraCommercials.ItemsSource = this.commercialChoices;
            this.cbxMinimumTime.ItemsSource = this.minimumTimeChoices;

            // Choose previous options as default
            this.cbxSecondsDelay.SelectedItem = RegistrySettings.SecondsDelay;
            this.cbxReplayMinutes.SelectedItem = RegistrySettings.ReplayExtraMinutes;
            this.cbxInitialCommercials.SelectedItem = RegistrySettings.InitialCommercials;
            this.cbxExtraCommercials.SelectedItem = RegistrySettings.ExtraCommercials;
            this.cbxMinimumTime.SelectedItem = RegistrySettings.MinimumCommercialMinutes;

            this.txtReplayLocation.Text = RegistrySettings.ReplayLocation;
        }

        /// <summary> Opens the folder browser dialog when the ... button is clicked. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event arguments. </param>
        private void FindFolderClicked(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description =
                        "Select the folder containing your SC2 accounts. You do not need to select your replay directory specifically.",
                    RootFolder = Environment.SpecialFolder.MyComputer
                };

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var replayFolder = fbd.SelectedPath;

                var hasAnyReplays = Directory.EnumerateFiles(replayFolder, "*.SC2Replay", SearchOption.AllDirectories).Any();

                if (hasAnyReplays == false)
                {
                    var result = MessageBox.Show(
                        "The selected folder does not contain any replays. Are you sure this is correct?",
                        "No Replays Found",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.txtReplayLocation.Text = replayFolder;                        
                    }
                }
                else
                {
                    this.txtReplayLocation.Text = replayFolder;                    
                }
            }
        }

        /// <summary> Saves the settings to the registry and closes the dialog when OK is pressed. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event arguments. </param>
        private void OkClicked(object sender, RoutedEventArgs e)
        {
            var folder = txtReplayLocation.Text;

            if (Directory.Exists(folder) == false)
            {
                MessageBox.Show(
                    "The selected replay folder does not exist. Select another folder.",
                    "Folder does not exist.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Save the settings
            RegistrySettings.ReplayLocation = folder;
            RegistrySettings.ReplayExtraMinutes = (int)cbxReplayMinutes.SelectedItem;
            RegistrySettings.InitialCommercials = (int)cbxInitialCommercials.SelectedItem;
            RegistrySettings.ExtraCommercials = (int)cbxExtraCommercials.SelectedItem;
            RegistrySettings.SecondsDelay = (int)cbxSecondsDelay.SelectedItem;
            RegistrySettings.MinimumCommercialMinutes = (int)cbxMinimumTime.SelectedItem;

            this.Close();
        }
    }
}
