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
        private TwitchApiV2 api;

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
                var fragment = e.Uri.Fragment;

                var splitterIndex = fragment.IndexOf('&');
                var accessToken = fragment.Substring(14, splitterIndex - 14);

                RegistrySettings.AccessToken = accessToken;

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
            this.webBrowser.Navigating += this.WebBrowserNavigating;

            this.api = new TwitchApiV2();
            var url = this.api.GetAuthorizationUrl();

            this.webBrowser.Navigate(url);
        }

        #endregion
    }
}