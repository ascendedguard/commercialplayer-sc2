// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TwitchApi.cs" company="AscendTV">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Collection of API Authorization and Commercial calls used against the TwitchTV service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TwitchCommercialSC2.TwitchTV
{
    using System;

    using DevDefined.OAuth.Consumer;
    using DevDefined.OAuth.Framework;

    /// <summary>
    /// Collection of API Authorization and Commercial calls used against the TwitchTV service.
    /// </summary>
    public class TwitchApi
    {
        /// <summary> Authorization URL for OAuth </summary>
        private readonly Uri authorizeUri = new Uri(@"http://api.justin.tv/oauth/authorize");

        /// <summary> Request Token URL for OAuth </summary>
        private readonly Uri requestTokenUri = new Uri(@"http://api.justin.tv/oauth/request_token");

        /// <summary> Access Token URI for OAuth </summary>
        private readonly Uri accessTokenUri = new Uri(@"http://api.justin.tv/oauth/access_token");

        /// <summary>
        /// Active API session being used.
        /// </summary>
        private readonly OAuthSession activeSession;

        /// <summary>
        /// Access token received from Twitch
        /// </summary>
        private IToken accessToken;

        /// <summary>
        /// Request token, stored to be converted to an access token after authorization is complete.
        /// </summary>
        private IToken requestToken;

        /// <summary> Initializes a new instance of the <see cref="TwitchApi"/> class. </summary>
        /// <param name="consumerKey"> The consumer key. </param>
        /// <param name="consumerSecret"> The consumer secret. </param>
        public TwitchApi(string consumerKey, string consumerSecret)
        {
            var context = new OAuthConsumerContext
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    SignatureMethod = SignatureMethod.HmacSha1,
                    UseHeaderForOAuthParameters = true
                };

            this.activeSession = new OAuthSession(context, this.requestTokenUri, this.authorizeUri, this.accessTokenUri);
        }

        /// <summary> Initializes a new instance of the <see cref="TwitchApi"/> class. </summary>
        /// <param name="consumerKey"> The consumer key. </param>
        /// <param name="consumerSecret"> The consumer secret. </param>
        /// <param name="accessToken"> The access token. </param>
        /// <param name="accessTokenSecret"> The access token secret. </param>
        public TwitchApi(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var context = new OAuthConsumerContext
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    SignatureMethod = SignatureMethod.HmacSha1,
                    UseHeaderForOAuthParameters = true
                };

            this.accessToken = new TokenBase
                {
                    ConsumerKey = consumerKey, Token = accessToken, TokenSecret = accessTokenSecret 
                };

            this.activeSession = new OAuthSession(context, this.requestTokenUri, this.authorizeUri, this.accessTokenUri) { AccessToken = this.accessToken };
        }

        /// <summary>
        /// Gets the session access token.
        /// </summary>
        public IToken AccessToken
        {
            get
            {
                return this.accessToken;
            }
        }

        /// <summary>
        /// Retrieves the authorization URL the user must go to for access to the API.
        /// </summary>
        /// <returns>Returns the URL of the website the user should proceed to.</returns>
        public string GetAuthorizationUrl()
        {
            this.requestToken = this.activeSession.GetRequestToken();
            return this.activeSession.GetUserAuthorizationUrlForToken(this.requestToken, "http://ascendtv.com/oauth/empty");
        }

        /// <summary>
        /// Finishes the authorization process and generates an access token. 
        /// Must be called after GetAuthorizationUrl()
        /// </summary>
        public void FinishAuthorization()
        {
            this.accessToken = this.activeSession.ExchangeRequestTokenForAccessToken(this.requestToken);
        }

        /// <summary> Plays a single commercial on stream. It will run for 30 seconds. </summary>
        public void PlayCommercial()
        {
            var requestUrl = new Uri(@"http://api.justin.tv/api/channel/commercial.json");
            var response = this.activeSession.Request().Post().ForUri(requestUrl).ToString();
        }
    }
}
