// -----------------------------------------------------------------------
// <copyright file="RegistrySettings.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class RegistrySettings
    {
        private const string RegistryPath = @"HKEY_CURRENT_USER\SOFTWARE\Ascend\TwitchCommercialSC2\";

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
