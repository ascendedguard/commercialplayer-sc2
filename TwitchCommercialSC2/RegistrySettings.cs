// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegistrySettings.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Contains all registry settings stored for the application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.IO;

    /// <summary>
    /// Contains all registry settings stored for the application.
    /// </summary>
    public class RegistrySettings
    {
        /// <summary>
        /// Base registry path where all settings are stored.
        /// </summary>
        private const string RegistryPath = @"HKEY_CURRENT_USER\SOFTWARE\Ascend\TwitchCommercialSC2\";

        /// <summary>
        /// Gets or sets the consumer key, an API component used in OAuth.
        /// </summary>
        public static string ConsumerKey
        {
            get
            {
                return (string)Microsoft.Win32.Registry.GetValue(RegistryPath, "ConsumerKey", string.Empty);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "ConsumerKey",
                    value,
                    Microsoft.Win32.RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets or sets the consumer secret, an API component used in OAuth.
        /// </summary>
        public static string ConsumerSecret
        {
            get
            {
                return (string)Microsoft.Win32.Registry.GetValue(RegistryPath, "ConsumerSecret", string.Empty);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "ConsumerSecret",
                    value,
                    Microsoft.Win32.RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets or sets the access token key, a unique value retrieved after the app has authorization.
        /// </summary>
        public static string AccessToken
        {
            get
            {
                return (string)Microsoft.Win32.Registry.GetValue(RegistryPath, "TwitchAccessToken", string.Empty);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "TwitchAccessToken",
                    value,
                    Microsoft.Win32.RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets or sets the access token secret, a unique value retrieved after the app has authorization.
        /// </summary>
        public static string AccessTokenSecret
        {
            get
            {
                return (string)Microsoft.Win32.Registry.GetValue(RegistryPath, "TwitchAccessSecret", string.Empty);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "TwitchAccessSecret",
                    value,
                    Microsoft.Win32.RegistryValueKind.String);
            }
        }
                
        /// <summary>
        /// Gets or sets the folder location to look for replays in.
        /// </summary>
        public static string ReplayLocation
        {
            get
            {
                return (string)Microsoft.Win32.Registry.GetValue(RegistryPath, "ReplayLocation", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Starcraft II", "Accounts"));
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "ReplayLocation",
                    value,
                    Microsoft.Win32.RegistryValueKind.String);
            }
        }
        
        /// <summary>
        /// Gets or sets the number of commercials always played.
        /// </summary>
        public static int InitialCommercials
        {
            get
            {
                return (int)Microsoft.Win32.Registry.GetValue(RegistryPath, "InitialCommercials", 1);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "InitialCommercials",
                    value,
                    Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the number of extra commercials played for longer replays.
        /// </summary>
        public static int ExtraCommercials
        {
            get
            {
                return (int)Microsoft.Win32.Registry.GetValue(RegistryPath, "ExtraCommercials", 1);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "ExtraCommercials",
                    value,
                    Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the delay, in seconds, before the first commercial begins playing.
        /// </summary>
        public static int SecondsDelay
        {
            get
            {
                return (int)Microsoft.Win32.Registry.GetValue(RegistryPath, "SecondsDelay", 15);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "SecondsDelay",
                    value,
                    Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the number of minutes a replay must be before it triggers extra commercials.
        /// </summary>
        public static int ReplayExtraMinutes
        {
            get
            {
                return (int)Microsoft.Win32.Registry.GetValue(RegistryPath, "ReplayExtraMinutes", 30);
            }

            set
            {
                Microsoft.Win32.Registry.SetValue(
                    RegistryPath,
                    "ReplayExtraMinutes",
                    value,
                    Microsoft.Win32.RegistryValueKind.DWord);
            }
        }
    }
}
