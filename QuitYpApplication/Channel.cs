using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using TweetSharp;

namespace QuitYpApplication
{
    class Channel
    {
        public string name = "test";
        public string id = "00000000000000000000000000000000";
        public string tip = "127.0.0.1:7144";
        public string url = "";
        public string genre = "";
        public string desc = "";
        public string listeners = "-1";
        public string relays = "-1";
        public string bitrate = "300";
        public string type = "WMV";
        public string dummy11 = "";
        public string dummy12 = "";
        public string dummy13 = "";
        public string dummy14 = "";
        public string encodech = "test";
        public string time = "0:00";
        public string status = "click";
        public string comment = "";
        public string dummy19 = "0";
        public string expandedurl = "";

        private static Dictionary<string, string> urlCache = new Dictionary<string, string>();

        public override string ToString()
        {
            return String.Format(
                "{0}<>{1}<>{2}<>{3}<>{4}<>{5}<>{6}<>{7}<>{8}<>{9}<>" +
                "{10}<>{11}<>{12}<>{13}<>{14}<>{15}<>{16}<>{17}<>{18}",
                name, id, tip, url, genre, desc, listeners, relays, bitrate, type,
                dummy11, dummy12, dummy13, dummy14, encodech, time, status, comment, dummy19
            );
        }

        private string GetExpandUrl(TwitterStatus tweet)
        {
            foreach (var url in tweet.Entities.Urls)
            {
                return url.ExpandedValue;
            }
            return "";
        }

        public bool Parse(TwitterStatus tweet)
        {
            var text = tweet.Text;
            var date = tweet.CreatedDate;

            var option = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            Regex r = new Regex(@"PeerCastで配信中！(.*?)\s?\[([^\]]*)\s-\s([^\]]*)\]「(.*)」?\shttps?://t.co/", option);
            MatchCollection mc = r.Matches(text);
            if (mc.Count == 0)
            {
                return false;
            }
            foreach (Match m in mc)
            {
                name = m.Groups[1].ToString();
                genre = m.Groups[2].ToString();
                desc = m.Groups[3].ToString();
                comment = m.Groups[4].ToString();
                encodech = Uri.EscapeDataString(name);
                //Console.WriteLine("{0} {1} {2} {3}", ch, genre, desc, comment);
                break;
            }

            TimeSpan ts = DateTime.Now - date;
            time = ts.ToString(@"hh\:mm");
            expandedurl = GetExpandUrl(tweet);

            return true;
        }

        public void UrlParseWithAPI(string url)
        {
            var option = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            Regex u = new Regex(@"id=([A-F0-9]+)&tip=([0-9\.:]+)", option);
            if (url.StartsWith("http://is.gd/"))
            {
                url = decodeShortUrl(url);
            }
            MatchCollection mc = u.Matches(url);
            foreach (Match m in mc)
            {
                id = m.Groups[1].ToString();
                tip = m.Groups[2].ToString();
                //Console.WriteLine("{0} {1}",id,tip);
                break;
            }
        }
        
        private string decodeShortUrl(string url)
        {
            try {
                if (urlCache.ContainsKey(url))
                {
                    return urlCache[url];
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://untiny.me/api/1.0/extract/?url=" + url + "&format=text");
                req.Method = "GET";
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream s = res.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string durl = sr.ReadToEnd();
                urlCache.Add(url, durl);

                return durl;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
