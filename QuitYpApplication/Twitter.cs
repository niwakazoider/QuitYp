using System;
using System.Text;
using System.Diagnostics;
using TweetSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace QuitYpApplication
{
    class Twitter
    {
        public const string consumerKey = "YourConsumerKey";
        public const string consumerSecret = "YourConsumerSecret";

        private QuitTokenState qtoken;
        private TwitterService service;
        private OAuthRequestToken requestToken;
        private long sinceId = 0;
        
        public delegate void OnIndexText(string txt, string raw);
        public delegate void OnRequest();
        public delegate void OnAuthComplete();
        public delegate void OnAuthError();
        public event OnIndexText OnIndexTextEvent = delegate (string txt, string raw) { };
        public event OnRequest OnRequestEvent = delegate () { };
        public event OnAuthComplete OnAuthCompleteEvent = delegate () { };
        public event OnAuthError OnAuthErrorEvent = delegate () { };

        public Twitter()
        {
        }

        public void Init()
        {
            service = new TwitterService(consumerKey, consumerSecret);

            qtoken = new QuitTokenState();
            qtoken.Load();
            //Console.WriteLine("load token {0} {1}", qtoken.token, qtoken.tokenSecret);

            if (qtoken.token == "")
            {
                Request();
            }
            else
            {
                Auth(qtoken.token, qtoken.tokenSecret);
            }
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    Search();
                    Thread.Sleep(20 * 60 * 1000);
                }
            }catch (ThreadInterruptedException)
            {

            }
            catch (Exception)
            {
                OnAuthErrorEvent();
            }
        }
        
        public void ClearToken()
        {
            qtoken.token = "";
            qtoken.tokenSecret = "";
            qtoken.Save();
        }

        public void Search()
        {
            var options = new SearchOptions { Q = "#peercast_yp -RT", IncludeEntities = true, SinceId = sinceId };
            var tweets = service.Search(options);
            if(service.Response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception();
            }

            HashSet<string> idset = new HashSet<string>();
            var sb = new StringBuilder();
            var rb = new StringBuilder();

            foreach (var tweet in tweets.Statuses)
            {
                if (sinceId < tweet.Id)
                {
                    sinceId = tweet.Id;
                }

                rb.AppendLine(tweet.Text);
                
                Channel channel = new Channel();
                if (channel.Parse(tweet))
                {
                    if (idset.Add(channel.name))
                    {
                        channel.UrlParseWithAPI(channel.expandedurl);
                        sb.Append(channel.ToString()).Append("\n");
                    }
                }

                //Console.WriteLine("{0} says '{1}'", tweet.User.ScreenName, tweet.Text);
            }

            OnIndexTextEvent(sb.ToString(), rb.ToString());
        }

        private void Auth(string token, string tokenSecret)
        {
            service.AuthenticateWith(token, tokenSecret);

            OnAuthCompleteEvent();
        }

        private void Request()
        {
            requestToken = service.GetRequestToken();
            Uri uri = service.GetAuthorizationUri(requestToken);
            Process.Start(uri.ToString());

            OnRequestEvent();
        }

        public void Pin(string code)
        {
            OAuthAccessToken access = service.GetAccessToken(requestToken, code);
            qtoken.token = access.Token;
            qtoken.tokenSecret = access.TokenSecret;
            qtoken.Save();
            //Console.WriteLine(access.Token);
            //Console.WriteLine(access.TokenSecret);

            Auth(access.Token, access.TokenSecret);
        }
    }
}
