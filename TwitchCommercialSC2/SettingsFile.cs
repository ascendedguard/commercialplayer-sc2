// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsFile.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Settings file for the application.
//   This is now in a file so the user can manually edit this file if desired.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Settings file for the application.
    /// This is now in a file so the user can manually edit this file if desired.
    /// </summary>
    public class SettingsFile
    {
        public int Delay { get; set; }

        public int InitialCommercials { get; set; }

        public int GameMinutesPerExtra { get; set; }

        public int MinimumGameMinutes { get; set; }

        public string ReplayDirectory { get; set; }

        public bool ShowOverlay { get; set; }

        public static string SettingsPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Twitch Commercial Runner",
                "Data", 
                "Settings.xml");

        public static string DefaultReplayDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Starcraft II", "Accounts");

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineHandling = NewLineHandling.Entitize;

            XmlWriter writer = XmlWriter.Create(SettingsPath, settings);

            writer.WriteStartDocument();

            writer.WriteStartElement("Settings");

            writer.WriteElementString("Delay", this.Delay.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("InitialCommercials", this.InitialCommercials.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("GameMinutesPerExtra", this.GameMinutesPerExtra.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("MinimumGameMinutes", this.MinimumGameMinutes.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ReplayDirectory", this.ReplayDirectory);
            writer.WriteElementString("ShowOverlay", this.ShowOverlay.ToString(CultureInfo.InvariantCulture));

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();
        }

        public void Load()
        {
            // Default settings.
            this.Delay = 15;
            this.InitialCommercials = 1;
            this.GameMinutesPerExtra = 20;
            this.MinimumGameMinutes = 3;
            this.ReplayDirectory = DefaultReplayDirectory;
            this.ShowOverlay = true;

            if (File.Exists(SettingsPath) == false)
            {
                return;
            }

            using (XmlReader reader = XmlReader.Create(SettingsPath))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "Delay":
                                reader.Read();
                                this.Delay = int.Parse(reader.Value);
                                break;
                            case "InitialCommercials":
                                reader.Read();
                                this.InitialCommercials = int.Parse(reader.Value);
                                break;
                            case "GameMinutesPerExtra":
                                reader.Read();
                                this.GameMinutesPerExtra = int.Parse(reader.Value);
                                break;
                            case "MinimumGameMinutes":
                                reader.Read();
                                this.MinimumGameMinutes = int.Parse(reader.Value);
                                break;
                            case "ReplayDirectory":
                                reader.Read();
                                this.ReplayDirectory = reader.Value;
                                break;
                            case "ShowOverlay":
                                reader.Read();
                                this.ShowOverlay = bool.Parse(reader.Value);
                                break;
                        }
                    }
                }
            }
        }
    }
}
