using System;
using System.Windows;

namespace TwitchCommercialSC2
{
    using System.Diagnostics;

    using TwitchCommercialSC2.TwitchTV;

    /// <summary>
    /// Interaction logic for AuthorizeTwitchWindow.xaml
    /// </summary>
    public partial class AuthorizeTwitchWindow
    {
        /// <summary> Initializes a new instance of the <see cref="AuthorizeTwitchWindow"/> class. </summary>
        public AuthorizeTwitchWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.txtConsumerKey.Text = RegistrySettings.ConsumerKey;
            this.txtConsumerSecret.Text = RegistrySettings.ConsumerSecret;

            this.webBrowser.Navigating += this.WebBrowserNavigating;
        }

        private void WebBrowserNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.Host.Equals(@"ascendtv.com", StringComparison.InvariantCultureIgnoreCase))
            {
                this.api.FinishAuthorization();

                RegistrySettings.ConsumerKey = this.consumerKey;
                RegistrySettings.ConsumerSecret = this.consumerSecret;
                RegistrySettings.AccessToken = this.api.AccessToken.Token;
                RegistrySettings.AccessTokenSecret = this.api.AccessToken.TokenSecret;

                this.DialogResult = true;
            }
        }

        private TwitchApi api;

        private string consumerKey;

        private string consumerSecret;

        private void btnAuthorize_Click(object sender, RoutedEventArgs e)
        {
            this.consumerKey = this.txtConsumerKey.Text;
            this.consumerSecret = this.txtConsumerSecret.Text;

            this.api = new TwitchApi(this.consumerKey, this.consumerSecret);
            var url = this.api.GetAuthorizationUrl();

            this.webBrowser.Navigate(url);
        }

        private void btnOpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://www.justin.tv/developer/activate"));
        }
    }
}
