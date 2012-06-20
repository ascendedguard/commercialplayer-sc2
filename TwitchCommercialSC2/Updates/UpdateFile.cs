namespace TwitchCommercialSC2.Updates
{
    using System;
    using System.Net;
    using System.Reflection;
    using System.Xml;

    public class UpdateFile
    {
        public bool IsNewerVersion { get; private set; }

        public Version Version { get; private set; }

        public Uri DownloadLocation { get; private set; }
        
        private UpdateFile()
        {
        }

        public static UpdateFile FindUpdates()
        {
            var file = new UpdateFile();

            var client = new WebClient();

            using (var reader = XmlReader.Create("http://ascendtv.com/commercial-sc2/updates.xml"))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "Version":
                                reader.Read();
                                var content = reader.Value;
                                file.Version = Version.Parse(content);
                                break;
                            case "DownloadLocation":
                                reader.Read();
                                file.DownloadLocation = new Uri(reader.Value);
                                break;
                        }
                    }
                }
            }

            file.IsNewerVersion = Assembly.GetEntryAssembly().GetName().Version < file.Version;
            return file;
        }
    }
}
