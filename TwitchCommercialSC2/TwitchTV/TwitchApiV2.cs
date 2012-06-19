// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TwitchApiV2.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Implements necessary calls to play commercials using version 2 of the Twitch API.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2.TwitchTV
{
    using System;
    using System.IO;
    using System.Net;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implements necessary calls to play commercials using version 2 of the Twitch API.
    /// </summary>
    public class TwitchApiV2
    {
        /// <summary> Time when the last commercial occured. </summary>
        private DateTime lastCommercialTime = DateTime.Now;

        /// <summary>
        /// Gets the time when the last commercial occured.
        /// </summary>
        public DateTime LastCommercialTime
        {
            get
            {
                return this.lastCommercialTime;
            }
        }

        private int totalCommercials;

        public int TotalCommercials
        {
            get
            {
                return this.totalCommercials;
            }

            set
            {
                this.totalCommercials = value;
            }
        }

        public readonly DateTime initializationTime = DateTime.Now;

        public DateTime InitializationTime
        {
            get
            {
                return this.initializationTime;
            }
        }

        public event EventHandler CommercialPlayed;

        /// <summary>
        /// Application Client ID used when authenticating the user.
        /// </summary>
        private const string ApplicationKey = "cjbqohk43qu345baa49ga16lz";

        /// <summary> Gets the authorization URL for OAuth2 authorization. </summary>
        /// <returns> Returns the string to direct the user to. </returns>
        public string GetAuthorizationUrl()
        {
            return "https://api.twitch.tv/kraken/oauth2/authorize?redirect_uri=http://ascendtv.com&client_id="
                         + ApplicationKey + "&response_type=token&scope=channel_commercial";
        }

        private string AttachOAuthToken(string s)
        {
            return s + "?oauth_token=" + RegistrySettings.AccessToken;
        }

        private const string RootApiUrl = "https://api.twitch.tv/kraken/";

        private string channelUrl = string.Empty;

        private string userUrl = string.Empty;

        public bool HasUserAuthorized()
        {
            var queryUrl = this.AttachOAuthToken(RootApiUrl);

            WebRequest request = WebRequest.Create(queryUrl);

            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";
            request.ContentType = "application/vnd.twitchtv+json";

            var response = request.GetResponse();

            string responseString = string.Empty;

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                responseString = reader.ReadToEnd();
            }

            response.Close();

            // Parse the response.
            JObject o = JObject.Parse(responseString);

            var isValid = (bool)o.SelectToken("token").SelectToken("valid");
            if (isValid == false)
            {
                return false;
            }
            
            var scopes = o.SelectToken("token").SelectToken("authorization").SelectToken("scopes");
            
            bool hasCommercialPermission = false;

            foreach (var s in scopes)
            {
                var permission = (string)s;

                if (permission.Equals("channel_commercial"))
                {
                    hasCommercialPermission = true;
                    break;
                }
            }

            var links = o.SelectToken("_links");
            
            this.channelUrl = (string)links.SelectToken("channels");
            this.userUrl = (string)links.SelectToken("users");

            return hasCommercialPermission;
        }

        public User GetUsersName()
        {
            if (this.userUrl == null)
            {
                throw new NotAuthorizedException("The user is not authorized, or HasUserAuthorized has not been called yet.");
            }

            WebRequest request = WebRequest.Create(this.userUrl);

            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";
            request.ContentType = "application/vnd.twitchtv+json";

            var response = request.GetResponse();

            string responseString = string.Empty;

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                responseString = reader.ReadToEnd();
            }

            response.Close();
            
            JObject o = JObject.Parse(responseString);
            var name = (string)o.SelectToken("display_name");
            var imgUrl = (string)o.SelectToken("logo");

            var user = new User { Name = name, Image = new BitmapImage(new Uri(imgUrl)) };
            return user;
        }

        /// <summary> Plays a commercial. Must be called after HasUserAuthorized to set up base requirements. </summary>
        /// <exception cref="NotAuthorizedException"> Throw if HasUserAuthorized was not called yet. </exception>
        public bool PlayCommercial()
        {
            if (this.channelUrl == null)
            {
                throw new NotAuthorizedException("The user is not authorized, or HasUserAuthorized has not been called yet.");
            }

            string commercialUrl = this.channelUrl + "/commercial";
            commercialUrl = this.AttachOAuthToken(commercialUrl);

            WebRequest request = WebRequest.Create(commercialUrl);

            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "POST";
            request.ContentType = "application/vnd.twitchtv+json";

            var response = request.GetResponse();
            
            var status = response.Headers["status"];
            bool success = status.Contains("204");

            response.Close();

            if (success)
            {
                this.TotalCommercials++;
                this.lastCommercialTime = DateTime.Now;

                var handler = this.CommercialPlayed;
                if (handler != null)
                {
                    this.CommercialPlayed(this, EventArgs.Empty);
                }
            }

            return success;
        }
    }
}
