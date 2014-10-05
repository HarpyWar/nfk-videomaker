using Helper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Helper
{

    /// <summary>
    /// API format
    /// http://pastebin.com/NeKQQfqg
    /// </summary>
    public class APIClient
    {

        public static bool SetVideo(int demoid, string video, string appId)
        {
            var url = Config.Data.SetVideoUrl
                .Replace("{demoid}", demoid.ToString())
                .Replace("{apikey}", Config.Data.ApiKey)
                .Replace("{appid}", appId);
            Log.Info(string.Format("Updating video \"{0}\" on {1}", video, url));
            try
            {
                using (var wc = new WebClient())
                {
                    var data = new NameValueCollection();
                    data["video"] = video;
                    var response = wc.UploadValues(url, "POST", data);

                    if (Encoding.Default.GetString(response) == "success")
                        return true;
                }
            } 
            catch(Exception e)
            {
                Log.Error(e.Message);
            }
            return false;
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static  DateTime UnixTimeToDateTime(double seconds)
        {
            return Epoch.AddSeconds(seconds);
        }
    }

}
