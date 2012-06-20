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
    }
}
