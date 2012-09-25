// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginControl.xaml.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for LoginControl.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Windows;

    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginControl"/> class.
        /// </summary>
        public LoginControl()
        {
            this.InitializeComponent();
        }

        private void LoginClicked(object sender, RoutedEventArgs e)
        {
            AttemptToLogin();
        }

        private void AttemptToLogin()
        {
            var api = new TwitchTV.TwitchApiV2();
            string token = txtUsername.Text;

            try
            {
                if (api.HasUserAuthorized(token))
                {
                    var handler = this.LoginSuccessful;
                    if (handler != null)
                    {
                        handler(this, new LoginEventArgs(token));
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Failed to log in. Your username or password may be incorrect.",
                        "Log in failed.",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (WebException ex)
            {
                string error = string.Format(
                    "Failed to log in. The request returned the following error:{0}{1}", Environment.NewLine, ex.Message);

                MessageBox.Show(
                    error,
                    "Log in failed.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public event EventHandler<LoginEventArgs> LoginSuccessful;

        private void PasswordKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return || e.Key == System.Windows.Input.Key.Enter)
            {
                this.AttemptToLogin();
            }
        }

        private void AuthenticateButtonClicked(object sender, RoutedEventArgs e)
        {
            var url = TwitchTV.TwitchApiV2.GetAuthorizationUrl();
            Process.Start(url);
        }
    }
}
