// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for App.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            var error = exception.GetType().ToString() + " " + exception.Message;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                error += Environment.NewLine + exception.GetType().ToString() + " " + exception.Message;
            }

            MessageBox.Show(
                "An error was thrown: " + Environment.NewLine + error,
                "Error Thrown",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
