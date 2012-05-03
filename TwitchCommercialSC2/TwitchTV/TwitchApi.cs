namespace TwitchCommercialSC2.TwitchTV
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;

    using DevDefined.OAuth.Consumer;
    using DevDefined.OAuth.Framework;

    using Microsoft.Win32;

    public class TwitchApi
    {
        /// <summary> Authorization URL for OAuth </summary>
        private readonly Uri authorizeUri = new Uri(@"http://api.justin.tv/oauth/authorize");

        /// <summary> Request Token URL for OAuth </summary>
        private readonly Uri requestTokenUri = new Uri(@"http://api.justin.tv/oauth/request_token");

        /// <summary> Access Token URI for OAuth </summary>
        private readonly Uri accessTokenUri = new Uri(@"http://api.justin.tv/oauth/access_token");

        private readonly string consumerKey;

        private readonly string consumerSecret;

        private IToken accessToken;

        private OAuthSession activeSession;

        private IToken requestToken;

        public IToken AccessToken
        {
            get
            {
                return this.accessToken;
            }
        }

        /// <summary> Initializes a new instance of the <see cref="TwitchApi"/> class. </summary>
        /// <param name="consumerKey"> The consumer key. </param>
        /// <param name="consumerSecret"> The consumer secret. </param>
        public TwitchApi(string consumerKey, string consumerSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;

            var context = new OAuthConsumerContext
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                SignatureMethod = SignatureMethod.HmacSha1,
                UseHeaderForOAuthParameters = true
            };

            this.activeSession = new OAuthSession(context, this.requestTokenUri, this.authorizeUri, this.accessTokenUri);
        }

        public string GetAuthorizationUrl()
        {
            this.requestToken = this.activeSession.GetRequestToken();
            return this.activeSession.GetUserAuthorizationUrlForToken(this.requestToken, "http://ascendtv.com/oauth/empty");
        }

        public void FinishAuthorization()
        {
            this.accessToken = this.activeSession.ExchangeRequestTokenForAccessToken(this.requestToken);
        }

        /// <summary> Initializes a new instance of the <see cref="TwitchApi"/> class. </summary>
        /// <param name="consumerKey"> The consumer key. </param>
        /// <param name="consumerSecret"> The consumer secret. </param>
        /// <param name="accessToken"> The access token. </param>
        /// <param name="accessTokenSecret"> The access token secret. </param>
        public TwitchApi(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;

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

        public void PlayCommercial()
        {
            var requestUrl = new Uri(@"http://api.justin.tv/api/channel/commercial.json");
            var response = this.activeSession.Request().Post().ForUri(requestUrl).ToString();
        }
    }
}
