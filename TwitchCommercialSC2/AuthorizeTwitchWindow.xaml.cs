// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizeTwitchWindow.xaml.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for AuthorizeTwitchWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Navigation;

    using TwitchCommercialSC2.TwitchTV;

    /// <summary>
    /// Interaction logic for AuthorizeTwitchWindow.xaml
    /// </summary>
    public partial class AuthorizeTwitchWindow
    {
        #region Constants and Fields

        /// <summary>
        /// Reference to the TwitchTV API.
        /// </summary>
        private TwitchApi api;

        /// <summary>
        /// The consumer key.
        /// </summary>
        private string consumerKey;

        /// <summary>
        /// The consumer secret.
        /// </summary>
        private string consumerSecret;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="AuthorizeTwitchWindow" /> class.
        /// </summary>
        public AuthorizeTwitchWindow()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Finishes the authorization process when JTV gives a redirect to AscendTV.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void WebBrowserNavigating(object sender, NavigatingCancelEventArgs e)
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

        /// <summary>
        /// Creates appropriate hooks and fills in text fields when the window finishes loading.
        /// </summary>
        /// <param name="sender">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The event arguments. 
        /// </param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.txtConsumerKey.Text = RegistrySettings.ConsumerKey;
            this.txtConsumerSecret.Text = RegistrySettings.ConsumerSecret;

            this.webBrowser.Navigating += this.WebBrowserNavigating;
        }

        /// <summary>
        /// Attempts to get user authorization for the API when the keys are given.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void AuthorizeClicked(object sender, RoutedEventArgs e)
        {
            this.consumerKey = this.txtConsumerKey.Text;
            this.consumerSecret = this.txtConsumerSecret.Text;

            this.api = new TwitchApi(this.consumerKey, this.consumerSecret);
            var url = this.api.GetAuthorizationUrl();

            this.webBrowser.Navigate(url);
        }

        /// <summary>
        /// Opens the developer portal when the open website button is pressed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void OpenWebsiteClicked(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://www.justin.tv/developer/activate"));
        }

        #endregion
    }
}