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
        private readonly int[] delayChoices = new[] { 0, 2, 5, 8, 10, 15, 20, 30, 45, 60 };

        private readonly int[] replayMinuteChoices = new[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };

        private readonly int[] commercialChoices = new[] { 0, 1, 2, 3, 4, 5, 6 };

        /// <summary> Initializes a new instance of the <see cref="CommercialSetupWindow"/> class. </summary>
        public CommercialSetupWindow()
        {
            InitializeComponent();

            // Set up item sources
            this.cbxSecondsDelay.ItemsSource = this.delayChoices;
            this.cbxReplayMinutes.ItemsSource = this.replayMinuteChoices;
            this.cbxInitialCommercials.ItemsSource = this.commercialChoices;
            this.cbxExtraCommercials.ItemsSource = this.commercialChoices;

            // Choose previous options as default
            this.cbxSecondsDelay.SelectedItem = RegistrySettings.SecondsDelay;
            this.cbxReplayMinutes.SelectedItem = RegistrySettings.ReplayExtraMinutes;
            this.cbxInitialCommercials.SelectedItem = RegistrySettings.InitialCommercials;
            this.cbxExtraCommercials.SelectedItem = RegistrySettings.ExtraCommercials;

            this.txtReplayLocation.Text = RegistrySettings.ReplayLocation;
        }

        private void btnFindFolder_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description =
                        "Select the folder containing your SC2 accounts. You do not need to select your replay directory specifically."
                };

            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
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

        private void OKClicked(object sender, RoutedEventArgs e)
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
            this.Close();
        }
    }
}
